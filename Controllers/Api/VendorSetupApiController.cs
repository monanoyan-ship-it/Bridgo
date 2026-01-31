using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bridgo.DTOs.Team;
using Bridgo.DTOs.Vendor;
using Bridgo.Models.Entities;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// VendorSetup API Controller
/// Vendor olusturma, arama ve katilma islemleri
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendorSetupApiController : ControllerBase
{
    private readonly IVendorService _vendorService;
    private readonly ITeamService _teamService;

    public VendorSetupApiController(IVendorService vendorService, ITeamService teamService)
    {
        _vendorService = vendorService;
        _teamService = teamService;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string? GetUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name;
    }

    /// <summary>
    /// Kullanicinin durumunu kontrol eder
    /// Domain eslesmesi, bekleyen istek vb.
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<DomainMatchResultDto>> GetStatus()
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        // Kullanici zaten bir vendor'a bagli mi?
        if (await _teamService.UserHasVendorAsync(userId))
        {
            return Ok(new { hasVendor = true, redirectUrl = "/" });
        }

        var email = GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return BadRequest("E-posta bilgisi bulunamadi");

        var result = new DomainMatchResultDto();

        // Bekleyen istek var mi?
        var pendingRequest = await _teamService.GetUserPendingRequestAsync(userId);
        if (pendingRequest != null)
        {
            result.HasPendingRequest = true;
            result.PendingRequest = pendingRequest;
            return Ok(result);
        }

        // Domain eslesmesi kontrol et
        var domainMatch = await _teamService.CheckDomainMatchAsync(userId, email);
        if (domainMatch != null)
        {
            result.IsCorporateEmail = true;
            result.HasMatch = true;
            result.MatchedVendor = new VendorSearchResultDto
            {
                Id = domainMatch.VendorId,
                CompanyName = domainMatch.VendorName
            };
        }

        return Ok(result);
    }

    /// <summary>
    /// Vendor arama
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<VendorSearchResultDto>>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(Enumerable.Empty<VendorSearchResultDto>());

        var results = await _vendorService.SearchVendorsAsync(q);
        return Ok(results);
    }

    /// <summary>
    /// Yeni Vendor olustur
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<VendorSetupResultDto>> Create([FromBody] VendorSetupDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var email = GetUserEmail();
        var result = await _vendorService.SetupVendorAsync(dto, userId, email);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Mevcut Vendor'a katilma istegi gonder
    /// </summary>
    [HttpPost("join")]
    public async Task<ActionResult<TeamOperationResultDto>> Join([FromBody] CreateJoinRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var result = await _teamService.CreateJoinRequestAsync(userId, dto, TeamMemberSource.JoinRequest);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// E-posta domain eslesme ile otomatik katilma istegi
    /// </summary>
    [HttpPost("join-by-domain")]
    public async Task<ActionResult<TeamOperationResultDto>> JoinByDomain([FromBody] CreateJoinRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var result = await _teamService.CreateJoinRequestAsync(userId, dto, TeamMemberSource.DomainMatch);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Bekleyen katilma istegini iptal et
    /// </summary>
    [HttpDelete("join/{memberId}")]
    public async Task<ActionResult> CancelJoinRequest(int memberId)
    {
        var userId = GetUserId();
        if (userId == 0)
            return Unauthorized();

        var success = await _teamService.CancelJoinRequestAsync(userId, memberId);
        if (!success)
            return NotFound();

        return Ok(new { success = true, message = "Istek iptal edildi" });
    }
}
