using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Checkout;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Checkout islemleri API
/// Cart -> Order donusumu ve hizmet secimi
/// </summary>
[Authorize]
[ApiController]
[Route("api/checkout")]
public class CheckoutApiController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly ApplicationDbContext _context;

    public CheckoutApiController(
        ICheckoutService checkoutService,
        ApplicationDbContext context)
    {
        _checkoutService = checkoutService;
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var userId = GetUserId();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user?.VendorId;
    }

    /// <summary>
    /// Checkout ozeti getir
    /// Sepet, adresler ve hizmet secenekleri
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var vendorId = await GetUserVendorIdAsync();
        var userId = GetUserId();

        if (vendorId == null || userId == 0)
            return Unauthorized();

        var summary = await _checkoutService.GetCheckoutSummaryAsync(vendorId.Value, userId);
        if (summary == null)
            return Ok(new { success = false, message = "Sepetiniz bos veya bulunamadi" });

        return Ok(new { success = true, data = summary });
    }

    /// <summary>
    /// Checkout'u baslat
    /// Order ve ServiceRequest'ler olustur
    /// </summary>
    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiateCheckoutDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        var userId = GetUserId();

        if (vendorId == null || userId == 0)
            return Unauthorized();

        var result = await _checkoutService.InitiateCheckoutAsync(vendorId.Value, userId, dto);
        return Ok(result);
    }

    /// <summary>
    /// Siparis icin gelen teklifleri getir
    /// </summary>
    [HttpGet("orders/{orderId}/quotes")]
    public async Task<IActionResult> GetOrderQuotes(int orderId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized();

        var quotes = await _checkoutService.GetOrderQuotesAsync(orderId, vendorId.Value);
        if (quotes == null)
            return NotFound(new { success = false, message = "Siparis bulunamadi" });

        return Ok(new { success = true, data = quotes });
    }

    /// <summary>
    /// Teklif sec
    /// </summary>
    [HttpPost("orders/{orderId}/quotes/{quoteId}/select")]
    public async Task<IActionResult> SelectQuote(int orderId, int quoteId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized();

        var result = await _checkoutService.SelectQuoteAsync(orderId, quoteId, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, message = "Teklif secildi" });
    }

    /// <summary>
    /// Checkout'u tamamla - Odeme adimina gec
    /// </summary>
    [HttpPost("orders/{orderId}/complete")]
    public async Task<IActionResult> Complete(int orderId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized();

        var result = await _checkoutService.CompleteCheckoutAsync(orderId, vendorId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Tum hizmetler secildi mi kontrol et
    /// </summary>
    [HttpGet("orders/{orderId}/services-status")]
    public async Task<IActionResult> GetServicesStatus(int orderId)
    {
        var allSelected = await _checkoutService.AreAllServicesSelectedAsync(orderId);
        return Ok(new { success = true, allSelected });
    }
}
