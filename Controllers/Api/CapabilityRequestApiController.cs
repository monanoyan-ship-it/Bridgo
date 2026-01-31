using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Services.Interfaces;
using Bridgo.Models.Enums;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Capability (Hizmet) talep API'si
/// </summary>
[Authorize]
[ApiController]
[Route("api/capability-requests")]
public class CapabilityRequestApiController : ControllerBase
{
    private readonly ICapabilityRequestService _service;
    private readonly ILogger<CapabilityRequestApiController> _logger;

    public CapabilityRequestApiController(
        ICapabilityRequestService service,
        ILogger<CapabilityRequestApiController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private int GetUserId() =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    private int GetVendorId() =>
        int.TryParse(User.FindFirst("VendorId")?.Value, out var id) ? id : 0;

    private bool IsSystemAdmin() =>
        User.FindFirst("IsSystemAdmin")?.Value == "true";

    /// <summary>
    /// Talep edilebilir capability'leri getir
    /// </summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        var vendorId = GetVendorId();
        if (vendorId == 0)
            return BadRequest(new { message = "Vendor bulunamadi" });

        var capabilities = await _service.GetAvailableCapabilitiesAsync(vendorId);
        return Ok(capabilities);
    }

    /// <summary>
    /// Kullanicinin taleplerini getir
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests()
    {
        var vendorId = GetVendorId();
        if (vendorId == 0)
            return BadRequest(new { message = "Vendor bulunamadi" });

        var requests = await _service.GetMyRequestsAsync(vendorId);
        return Ok(requests);
    }

    /// <summary>
    /// Yeni talep olustur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCapabilityRequestDto dto)
    {
        var vendorId = GetVendorId();
        var userId = GetUserId();

        if (vendorId == 0 || userId == 0)
            return BadRequest(new { message = "Gecersiz kullanici" });

        var result = await _service.CreateRequestAsync(vendorId, userId, dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Durum tiplerini getir
    /// </summary>
    [HttpGet("statuses")]
    public IActionResult GetStatuses()
    {
        var statuses = CapabilityRequestStatuses.All.Select(s => new
        {
            id = s.Id,
            name = s.SystemName,
            nameResourceKey = s.NameResourceKey,
            cssClass = s.CssClass
        });
        return Ok(statuses);
    }

    // ============================================
    // ADMIN ENDPOINTS
    // ============================================

    /// <summary>
    /// Bekleyen talepleri getir (admin)
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        if (!IsSystemAdmin())
            return Forbid();

        var requests = await _service.GetPendingRequestsAsync();
        return Ok(requests);
    }

    /// <summary>
    /// Tum talepleri getir (admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? status = null)
    {
        if (!IsSystemAdmin())
            return Forbid();

        var requests = await _service.GetAllRequestsAsync(status);
        return Ok(requests);
    }

    /// <summary>
    /// Talebi onayla (admin)
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] AdminActionDto? dto = null)
    {
        if (!IsSystemAdmin())
            return Forbid();

        var adminUserId = GetUserId();
        var result = await _service.ApproveRequestAsync(id, adminUserId, dto?.Notes);

        if (!result)
            return BadRequest(new { message = "Talep onaylanamadi" });

        return Ok(new { message = "Talep onaylandi" });
    }

    /// <summary>
    /// Talebi reddet (admin)
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] AdminActionDto? dto = null)
    {
        if (!IsSystemAdmin())
            return Forbid();

        var adminUserId = GetUserId();
        var result = await _service.RejectRequestAsync(id, adminUserId, dto?.Notes);

        if (!result)
            return BadRequest(new { message = "Talep reddedilemedi" });

        return Ok(new { message = "Talep reddedildi" });
    }
}

public class AdminActionDto
{
    public string? Notes { get; set; }
}
