using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/follow")]
[Authorize]
public class FollowApiController : ControllerBase
{
    private readonly ISocialFeedService _feedService;
    private readonly UserManager<ApplicationUser> _userManager;

    public FollowApiController(
        ISocialFeedService feedService,
        UserManager<ApplicationUser> userManager)
    {
        _feedService = feedService;
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

    [HttpPost("{vendorId:int}")]
    public async Task<IActionResult> ToggleFollow(int vendorId)
    {
        var myVendorId = await GetUserVendorIdAsync();
        if (myVendorId == null) return Unauthorized();

        var result = await _feedService.ToggleFollowAsync(vendorId, myVendorId.Value, GetUserId());
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpGet("{vendorId:int}/status")]
    public async Task<IActionResult> IsFollowing(int vendorId)
    {
        var myVendorId = await GetUserVendorIdAsync();
        if (myVendorId == null) return Unauthorized();

        var isFollowing = await _feedService.IsFollowingAsync(myVendorId.Value, vendorId);
        return Ok(new { isFollowing });
    }

    [HttpGet("{vendorId:int}/stats")]
    public async Task<IActionResult> GetFollowStats(int vendorId)
    {
        var stats = await _feedService.GetFollowStatsAsync(vendorId);
        return Ok(stats);
    }

    [HttpGet("{vendorId:int}/followers")]
    public async Task<IActionResult> GetFollowers(int vendorId, [FromQuery] int pageSize = 20, [FromQuery] int? lastId = null)
    {
        var myVendorId = await GetUserVendorIdAsync();
        if (myVendorId == null) return Unauthorized();

        var followers = await _feedService.GetFollowersAsync(vendorId, myVendorId.Value, pageSize, lastId);
        return Ok(followers);
    }

    [HttpGet("{vendorId:int}/following")]
    public async Task<IActionResult> GetFollowing(int vendorId, [FromQuery] int pageSize = 20, [FromQuery] int? lastId = null)
    {
        var myVendorId = await GetUserVendorIdAsync();
        if (myVendorId == null) return Unauthorized();

        var following = await _feedService.GetFollowingAsync(vendorId, myVendorId.Value, pageSize, lastId);
        return Ok(following);
    }
}
