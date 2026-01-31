using System.Security.Claims;
using Bridgo.Authorization;
using Bridgo.DTOs.Demand;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Pazarlik (Negotiation) API Controller
/// DemandResponse uzerinde cift yonlu pazarlik islemleri
/// </summary>
[ApiController]
[Route("api/negotiations")]
[Authorize]
[RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
public class NegotiationApiController : ControllerBase
{
    private readonly INegotiationService _negotiationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<NegotiationApiController> _logger;

    public NegotiationApiController(
        INegotiationService negotiationService,
        UserManager<ApplicationUser> userManager,
        ILogger<NegotiationApiController> logger)
    {
        _negotiationService = negotiationService;
        _userManager = userManager;
        _logger = logger;
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

    private async Task<string?> GetUserFullNameAsync()
    {
        var user = await GetCurrentUserAsync();
        return user != null ? $"{user.FirstName} {user.LastName}".Trim() : null;
    }

    // ============================================
    // PAZARLIK BASLATMA
    // ============================================

    /// <summary>
    /// Buyer pazarlik baslatir
    /// POST /api/negotiations/start
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> StartNegotiation([FromBody] CounterOfferCreateDto dto)
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Vendor bulunamadi." });

            var userName = await GetUserFullNameAsync();
            var result = await _negotiationService.StartNegotiationAsync(dto.DemandResponseId, vendorId.Value, dto, userName);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StartNegotiation hatasi");
            return StatusCode(500, new { message = "Bir hata olustu." });
        }
    }

    // ============================================
    // KARSI TEKLIF
    // ============================================

    /// <summary>
    /// Karsi teklif ver
    /// POST /api/negotiations/counter
    /// </summary>
    [HttpPost("counter")]
    public async Task<IActionResult> CreateCounterOffer([FromBody] CounterOfferCreateDto dto)
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Vendor bulunamadi." });

            var userName = await GetUserFullNameAsync();
            var result = await _negotiationService.CreateCounterOfferAsync(vendorId.Value, dto, userName);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateCounterOffer hatasi");
            return StatusCode(500, new { message = "Bir hata olustu." });
        }
    }

    // ============================================
    // YANIT ISLEMLERI
    // ============================================

    /// <summary>
    /// Karsi teklifi kabul et
    /// POST /api/negotiations/rounds/{roundId}/accept
    /// </summary>
    [HttpPost("rounds/{roundId}/accept")]
    public async Task<IActionResult> AcceptCounterOffer(int roundId)
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Vendor bulunamadi." });

            var userName = await GetUserFullNameAsync();
            var result = await _negotiationService.AcceptCounterOfferAsync(roundId, vendorId.Value, userName);
            return Ok(new { success = result, message = "Teklif kabul edildi." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AcceptCounterOffer hatasi");
            return StatusCode(500, new { message = "Bir hata olustu." });
        }
    }

    /// <summary>
    /// Karsi teklifi reddet
    /// POST /api/negotiations/rounds/{roundId}/reject
    /// </summary>
    [HttpPost("rounds/{roundId}/reject")]
    public async Task<IActionResult> RejectCounterOffer(int roundId, [FromBody] NegotiationRejectDto? dto)
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Vendor bulunamadi." });

            var userName = await GetUserFullNameAsync();
            var result = await _negotiationService.RejectCounterOfferAsync(roundId, vendorId.Value, dto?.Reason, userName);
            return Ok(new { success = result, message = "Teklif reddedildi." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RejectCounterOffer hatasi");
            return StatusCode(500, new { message = "Bir hata olustu." });
        }
    }

    /// <summary>
    /// Pazarliktan geri cekil
    /// POST /api/negotiations/{demandResponseId}/withdraw
    /// </summary>
    [HttpPost("{demandResponseId}/withdraw")]
    public async Task<IActionResult> Withdraw(int demandResponseId)
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Vendor bulunamadi." });

            var userName = await GetUserFullNameAsync();
            var result = await _negotiationService.WithdrawFromNegotiationAsync(demandResponseId, vendorId.Value, userName);
            return Ok(new { success = result, message = "Pazarliktan cekildiniz." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Withdraw hatasi");
            return StatusCode(500, new { message = "Bir hata olustu." });
        }
    }

    // ============================================
    // SORGULAMA
    // ============================================

    /// <summary>
    /// Pazarlik gecmisi
    /// GET /api/negotiations/{demandResponseId}/history
    /// </summary>
    [HttpGet("{demandResponseId}/history")]
    public async Task<IActionResult> GetHistory(int demandResponseId)
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Vendor bulunamadi." });

            // Timeout kontrolu
            await _negotiationService.CheckAndExpireNegotiationAsync(demandResponseId);

            var result = await _negotiationService.GetNegotiationHistoryAsync(demandResponseId, vendorId.Value);
            if (result == null)
                return NotFound(new { message = "Pazarlik bulunamadi veya erisim yetkiniz yok." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetHistory hatasi");
            return StatusCode(500, new { message = "Bir hata olustu." });
        }
    }

    /// <summary>
    /// Pazarlik ozeti
    /// GET /api/negotiations/{demandResponseId}/summary
    /// </summary>
    [HttpGet("{demandResponseId}/summary")]
    public async Task<IActionResult> GetSummary(int demandResponseId)
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Vendor bulunamadi." });

            // Timeout kontrolu
            await _negotiationService.CheckAndExpireNegotiationAsync(demandResponseId);

            var result = await _negotiationService.GetNegotiationSummaryAsync(demandResponseId, vendorId.Value);
            if (result == null)
                return NotFound(new { message = "Pazarlik bulunamadi veya erisim yetkiniz yok." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSummary hatasi");
            return StatusCode(500, new { message = "Bir hata olustu." });
        }
    }

    /// <summary>
    /// Aktif pazarliklarim (sira bende)
    /// GET /api/negotiations/my-active
    /// </summary>
    [HttpGet("my-active")]
    public async Task<IActionResult> GetMyActive()
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Vendor bulunamadi." });

            var result = await _negotiationService.GetMyActiveNegotiationsAsync(vendorId.Value);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMyActive hatasi");
            return StatusCode(500, new { message = "Bir hata olustu." });
        }
    }

    /// <summary>
    /// Bekleyen pazarliklarim (sira karsi tarafta)
    /// GET /api/negotiations/pending
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Vendor bulunamadi." });

            var result = await _negotiationService.GetPendingNegotiationsAsync(vendorId.Value);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPending hatasi");
            return StatusCode(500, new { message = "Bir hata olustu." });
        }
    }
}
