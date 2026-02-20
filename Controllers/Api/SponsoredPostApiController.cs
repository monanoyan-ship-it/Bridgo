using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.DTOs.SocialFeed;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/sponsored-posts")]
[Authorize]
public class SponsoredPostApiController : ControllerBase
{
    private readonly ISponsoredPostService _sponsoredPostService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SponsoredPostApiController(
        ISponsoredPostService sponsoredPostService,
        UserManager<ApplicationUser> userManager)
    {
        _sponsoredPostService = sponsoredPostService;
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

    [HttpGet]
    public async Task<IActionResult> GetVendorSponsoredPosts()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _sponsoredPostService.GetVendorSponsoredPostsAsync(vendorId.Value);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSponsoredPost(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _sponsoredPostService.GetSponsoredPostAsync(id, vendorId.Value);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSponsoredPost([FromBody] SponsoredPostCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _sponsoredPostService.CreateSponsoredPostAsync(dto, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { id = result.Data, message = result.Message });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSponsoredPost(int id, [FromBody] SponsoredPostUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _sponsoredPostService.UpdateSponsoredPostAsync(id, dto, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> ActivateSponsoredPost(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _sponsoredPostService.ActivateSponsoredPostAsync(id, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpPost("{id:int}/pause")]
    public async Task<IActionResult> PauseSponsoredPost(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _sponsoredPostService.PauseSponsoredPostAsync(id, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpPost("{id:int}/click")]
    public async Task<IActionResult> RecordClick(int id)
    {
        await _sponsoredPostService.RecordClickAsync(id);
        return Ok();
    }
}
