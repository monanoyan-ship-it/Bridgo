using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.Authorization;
using Bridgo.DTOs.ProductInquiry;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Urun talepleri - Seller ve Buyer erisebilir
/// </summary>
[ApiController]
[Route("api/product-inquiries")]
[Authorize]
[RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
public class ProductInquiriesApiController : ControllerBase
{
    private readonly IProductInquiryService _inquiryService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductInquiriesApiController(
        IProductInquiryService inquiryService,
        UserManager<ApplicationUser> userManager)
    {
        _inquiryService = inquiryService;
        _userManager = userManager;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = GetUserId();
        return userId == 0 ? null : await _userManager.FindByIdAsync(userId.ToString());
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.VendorId;
    }

    // ============================================
    // BUYER (ALICI) ENDPOINTS
    // ============================================

    /// <summary>
    /// Alicinin isteklerini listele
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyInquiries([FromQuery] ProductInquiryFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _inquiryService.GetBuyerInquiriesAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Istek detayi getir (alici icin)
    /// </summary>
    [HttpGet("my/{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetMyInquiry(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var inquiry = await _inquiryService.GetInquiryForBuyerAsync(id, vendorId.Value);
        if (inquiry == null) return NotFound(new { message = "Istek bulunamadi." });

        return Ok(inquiry);
    }

    /// <summary>
    /// Yeni istek olustur
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateInquiry([FromBody] ProductInquiryCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        try
        {
            var result = await _inquiryService.CreateInquiryAsync(vendorId.Value, dto, GetUserId().ToString());
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Istegi iptal et
    /// </summary>
    [HttpPost("my/{id:int}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelInquiry(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        try
        {
            var success = await _inquiryService.CancelInquiryAsync(id, vendorId.Value, GetUserId().ToString());
            if (!success) return NotFound(new { message = "Istek bulunamadi." });
            return Ok(new { message = "Istek iptal edildi." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Teklifi kabul et
    /// </summary>
    [HttpPost("responses/{responseId:int}/accept")]
    [Authorize]
    public async Task<IActionResult> AcceptResponse(int responseId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _inquiryService.AcceptResponseAsync(responseId, vendorId.Value, GetUserId().ToString());
        if (!success) return NotFound(new { message = "Teklif bulunamadi." });

        return Ok(new { message = "Teklif kabul edildi." });
    }

    /// <summary>
    /// Teklifi reddet
    /// </summary>
    [HttpPost("responses/{responseId:int}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectResponse(int responseId, [FromBody] RejectResponseDto? dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _inquiryService.RejectResponseAsync(responseId, vendorId.Value, dto?.Notes, GetUserId().ToString());
        if (!success) return NotFound(new { message = "Teklif bulunamadi." });

        return Ok(new { message = "Teklif reddedildi." });
    }

    /// <summary>
    /// Alici istatistikleri
    /// </summary>
    [HttpGet("my/stats")]
    [Authorize]
    public async Task<IActionResult> GetBuyerStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var stats = await _inquiryService.GetBuyerStatsAsync(vendorId.Value);
        return Ok(stats);
    }

    // ============================================
    // SELLER (SATICI) ENDPOINTS
    // ============================================

    /// <summary>
    /// Saticiya gelen istekleri listele
    /// </summary>
    [HttpGet("incoming")]
    [Authorize]
    public async Task<IActionResult> GetIncomingInquiries([FromQuery] ProductInquiryFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _inquiryService.GetSellerInquiriesAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Istek detayi getir (satici icin)
    /// </summary>
    [HttpGet("incoming/{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetIncomingInquiry(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        // Otomatik olarak okundu isaretle
        await _inquiryService.MarkAsReadAsync(id, vendorId.Value);

        var inquiry = await _inquiryService.GetInquiryForSellerAsync(id, vendorId.Value);
        if (inquiry == null) return NotFound(new { message = "Istek bulunamadi." });

        return Ok(inquiry);
    }

    /// <summary>
    /// Teklif/yanit gonder
    /// </summary>
    [HttpPost("incoming/{inquiryId:int}/respond")]
    [Authorize]
    public async Task<IActionResult> CreateResponse(int inquiryId, [FromBody] ProductInquiryResponseCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        dto.InquiryId = inquiryId;

        try
        {
            var result = await _inquiryService.CreateResponseAsync(vendorId.Value, dto, GetUserId().ToString());
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Satici istatistikleri
    /// </summary>
    [HttpGet("incoming/stats")]
    [Authorize]
    public async Task<IActionResult> GetSellerStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var stats = await _inquiryService.GetSellerStatsAsync(vendorId.Value);
        return Ok(stats);
    }

    /// <summary>
    /// Okunmamis istek sayisi
    /// </summary>
    [HttpGet("incoming/unread-count")]
    [Authorize]
    public async Task<IActionResult> GetUnreadCount()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var count = await _inquiryService.GetUnreadCountAsync(vendorId.Value);
        return Ok(new { count });
    }
}

// Helper DTO
public class RejectResponseDto
{
    public string? Notes { get; set; }
}
