using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.DTOs.RewardPoints;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/reward-points")]
[Authorize]
public class RewardPointsApiController : ControllerBase
{
    private readonly IRewardPointService _rewardPointService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RewardPointsApiController(
        IRewardPointService rewardPointService,
        UserManager<ApplicationUser> userManager)
    {
        _rewardPointService = rewardPointService;
        _userManager = userManager;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var userId = GetUserId();
        if (userId == 0) return null;
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.VendorId;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var summary = await _rewardPointService.GetSummaryAsync(vendorId.Value);
        return Ok(summary);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] RewardPointFilterDto? filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _rewardPointService.GetHistoryAsync(vendorId.Value, filter);
        return Ok(result);
    }

    [HttpGet("action-types")]
    public IActionResult GetActionTypes()
    {
        var types = Bridgo.Models.Enums.RewardPointActionTypes.All.Select(t => new
        {
            id = t.Id,
            systemName = t.SystemName,
            nameResourceKey = t.NameResourceKey,
            icon = t.Icon,
            cssClass = t.CssClass
        });
        return Ok(types);
    }
}
