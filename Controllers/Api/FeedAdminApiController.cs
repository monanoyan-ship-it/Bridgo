using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.DTOs.SocialFeed;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/admin/feed")]
[Authorize]
public class FeedAdminApiController : ControllerBase
{
    private readonly ISocialFeedService _feedService;
    private readonly UserManager<ApplicationUser> _userManager;

    public FeedAdminApiController(
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

    [HttpGet("reports")]
    public async Task<IActionResult> GetPendingReports([FromQuery] int pageSize = 20, [FromQuery] int? lastId = null)
    {
        var reports = await _feedService.GetPendingReportsAsync(pageSize, lastId);
        return Ok(reports);
    }

    [HttpPut("reports/{id:int}/review")]
    public async Task<IActionResult> ReviewReport(int id, [FromBody] SocialPostReportReviewDto dto)
    {
        var result = await _feedService.ReviewReportAsync(id, dto, GetUserId());
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }
}
