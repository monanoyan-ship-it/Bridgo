using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.Authorization;
using Bridgo.DTOs.Demand;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Talepler - Buyer olusturur, Seller gorur
/// </summary>
[ApiController]
[Route("api/demands")]
[Authorize]
[RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
public class DemandsApiController : ControllerBase
{
    private readonly IDemandService _demandService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DemandsApiController(
        IDemandService demandService,
        UserManager<ApplicationUser> userManager)
    {
        _demandService = demandService;
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
    // PUBLIC ENDPOINTS (Login gerektirmez)
    // ============================================

    /// <summary>
    /// Public talepleri listele (SEO sayfasi icin)
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicDemands([FromQuery] DemandFilterDto filter)
    {
        var result = await _demandService.GetPublicDemandsAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Talep detayi getir (slug ile - SEO)
    /// </summary>
    [HttpGet("public/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDemandBySlug(string slug)
    {
        var demand = await _demandService.GetDemandBySlugAsync(slug, incrementViewCount: true);
        if (demand == null) return NotFound(new { message = "Talep bulunamadi." });
        return Ok(demand);
    }

    /// <summary>
    /// Dis uretici (kayitsiz) olarak teklif ver
    /// </summary>
    [HttpPost("public/{demandId}/respond")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateExternalResponse(int demandId, [FromBody] ExternalDemandResponseCreateDto dto)
    {
        if (string.IsNullOrEmpty(dto.ExternalCompanyName))
            return BadRequest(new { message = "Firma adi zorunludur." });
        if (string.IsNullOrEmpty(dto.ExternalEmail))
            return BadRequest(new { message = "E-posta zorunludur." });

        dto.DemandId = demandId;

        try
        {
            var response = await _demandService.CreateExternalResponseAsync(dto);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ============================================
    // AUTHENTICATED ENDPOINTS - Talep Yonetimi
    // ============================================

    /// <summary>
    /// Kendi taleplerimi listele
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyDemands([FromQuery] DemandFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _demandService.GetVendorDemandsAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Takip edilen kategorilerdeki talepleri listele
    /// </summary>
    [HttpGet("subscribed")]
    [Authorize]
    public async Task<IActionResult> GetSubscribedDemands([FromQuery] DemandFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _demandService.GetSubscribedDemandsAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Talep detayi getir (ID ile - sahibi icin)
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetDemandById(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var demand = await _demandService.GetDemandByIdAsync(id);
        if (demand == null) return NotFound(new { message = "Talep bulunamadi." });

        // Sadece sahibi gorebilir (detayli - yanitlarla birlikte)
        if (demand.VendorId != vendorId.Value)
            return Forbid();

        return Ok(demand);
    }

    /// <summary>
    /// Yeni talep olustur
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateDemand([FromBody] DemandCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        if (string.IsNullOrEmpty(dto.Title))
            return BadRequest(new { message = "Baslik zorunludur." });

        var demand = await _demandService.CreateDemandAsync(vendorId.Value, dto, GetUserId().ToString());
        return CreatedAtAction(nameof(GetDemandById), new { id = demand.Id }, demand);
    }

    /// <summary>
    /// Talep guncelle
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateDemand(int id, [FromBody] DemandUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _demandService.UpdateDemandAsync(id, vendorId.Value, dto, GetUserId().ToString());
        if (!success) return NotFound(new { message = "Talep bulunamadi." });

        return Ok(new { message = "Talep guncellendi." });
    }

    /// <summary>
    /// Talep sil
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteDemand(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _demandService.DeleteDemandAsync(id, vendorId.Value, GetUserId().ToString());
        if (!success) return NotFound(new { message = "Talep bulunamadi." });

        return Ok(new { message = "Talep silindi." });
    }

    /// <summary>
    /// Talep yayinla (Draft -> Active)
    /// </summary>
    [HttpPost("{id:int}/publish")]
    [Authorize]
    public async Task<IActionResult> PublishDemand(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _demandService.PublishDemandAsync(id, vendorId.Value, GetUserId().ToString());
        if (!success) return NotFound(new { message = "Talep bulunamadi." });

        return Ok(new { message = "Talep yayinlandi." });
    }

    /// <summary>
    /// Talep kapat
    /// </summary>
    [HttpPost("{id:int}/close")]
    [Authorize]
    public async Task<IActionResult> CloseDemand(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _demandService.CloseDemandAsync(id, vendorId.Value, GetUserId().ToString());
        if (!success) return NotFound(new { message = "Talep bulunamadi." });

        return Ok(new { message = "Talep kapatildi." });
    }

    // ============================================
    // MODIFIKASYONLAR
    // ============================================

    /// <summary>
    /// Talebe modifikasyon ekle
    /// </summary>
    [HttpPost("{demandId:int}/modifications")]
    [Authorize]
    public async Task<IActionResult> AddModification(int demandId, [FromBody] DemandModificationCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        try
        {
            var modification = await _demandService.AddModificationAsync(demandId, vendorId.Value, dto, GetUserId().ToString());
            return Ok(modification);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Modifikasyon guncelle
    /// </summary>
    [HttpPut("modifications/{modificationId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateModification(int modificationId, [FromBody] DemandModificationCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _demandService.UpdateModificationAsync(modificationId, vendorId.Value, dto, GetUserId().ToString());
        if (!success) return NotFound(new { message = "Modifikasyon bulunamadi." });

        return Ok(new { message = "Modifikasyon guncellendi." });
    }

    /// <summary>
    /// Modifikasyon sil
    /// </summary>
    [HttpDelete("modifications/{modificationId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteModification(int modificationId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _demandService.DeleteModificationAsync(modificationId, vendorId.Value, GetUserId().ToString());
        if (!success) return NotFound(new { message = "Modifikasyon bulunamadi." });

        return Ok(new { message = "Modifikasyon silindi." });
    }

    // ============================================
    // YANITLAR (Talep sahibi icin)
    // ============================================

    /// <summary>
    /// Tum taleplerime gelen yanitlari listele
    /// </summary>
    [HttpGet("responses")]
    [Authorize]
    public async Task<IActionResult> GetAllMyDemandResponses()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var responses = await _demandService.GetAllResponsesForVendorAsync(vendorId.Value);
        return Ok(responses);
    }

    /// <summary>
    /// Talebe gelen yanitlari listele
    /// </summary>
    [HttpGet("{demandId:int}/responses")]
    [Authorize]
    public async Task<IActionResult> GetDemandResponses(int demandId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var responses = await _demandService.GetDemandResponsesAsync(demandId, vendorId.Value);
        return Ok(responses);
    }

    /// <summary>
    /// Yanit detayi getir
    /// </summary>
    [HttpGet("responses/{responseId:int}")]
    [Authorize]
    public async Task<IActionResult> GetResponseById(int responseId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var response = await _demandService.GetResponseByIdAsync(responseId, vendorId.Value);
        if (response == null) return NotFound(new { message = "Yanit bulunamadi." });

        return Ok(response);
    }

    /// <summary>
    /// Yanit durumunu guncelle
    /// </summary>
    [HttpPut("responses/{responseId:int}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateResponseStatus(int responseId, [FromBody] DemandResponseStatusUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _demandService.UpdateResponseStatusAsync(responseId, vendorId.Value, dto, GetUserId().ToString());
        if (!success) return NotFound(new { message = "Yanit bulunamadi." });

        return Ok(new { message = "Yanit durumu guncellendi." });
    }

    /// <summary>
    /// Yanit goruldu olarak isaretle
    /// </summary>
    [HttpPost("responses/{responseId:int}/mark-viewed")]
    [Authorize]
    public async Task<IActionResult> MarkResponseAsViewed(int responseId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        await _demandService.MarkResponseAsViewedAsync(responseId, vendorId.Value);
        return Ok(new { message = "Yanit goruldu olarak isaretlendi." });
    }

    // ============================================
    // YANITLAR (Tedarikci icin)
    // ============================================

    /// <summary>
    /// Kayitli uretici olarak teklif ver
    /// </summary>
    [HttpPost("{demandId:int}/respond")]
    [Authorize]
    public async Task<IActionResult> CreateResponse(int demandId, [FromBody] DemandResponseCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        dto.DemandId = demandId;

        try
        {
            var response = await _demandService.CreateResponseAsync(vendorId.Value, dto, GetUserId().ToString());
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kendi verdigi yanitlari listele
    /// </summary>
    [HttpGet("my-responses")]
    [Authorize]
    public async Task<IActionResult> GetMyResponses()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var responses = await _demandService.GetMyResponsesAsync(vendorId.Value);
        return Ok(responses);
    }

    // ============================================
    // ISTATISTIKLER
    // ============================================

    /// <summary>
    /// Talep istatistikleri
    /// </summary>
    [HttpGet("stats")]
    [Authorize]
    public async Task<IActionResult> GetDemandStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var stats = await _demandService.GetVendorDemandStatsAsync(vendorId.Value);
        return Ok(stats);
    }

    /// <summary>
    /// Yanit istatistikleri (tedarikci icin)
    /// </summary>
    [HttpGet("response-stats")]
    [Authorize]
    public async Task<IActionResult> GetResponseStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var stats = await _demandService.GetSupplierResponseStatsAsync(vendorId.Value);
        return Ok(stats);
    }
}
