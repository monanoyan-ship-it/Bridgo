using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.DTOs.SocialFeed;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/feed")]
[Authorize]
public class FeedApiController : ControllerBase
{
    private readonly ISocialFeedService _feedService;
    private readonly UserManager<ApplicationUser> _userManager;

    public FeedApiController(
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

    // ============================================
    // FEED
    // ============================================

    [HttpGet]
    public async Task<IActionResult> GetFeed([FromQuery] int? lastPostId, [FromQuery] int pageSize = 20)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _feedService.GetFeedAsync(vendorId.Value, GetUserId(), lastPostId, pageSize);
        return Ok(result);
    }

    [HttpGet("discover")]
    public async Task<IActionResult> GetDiscoverFeed([FromQuery] int? lastPostId, [FromQuery] int pageSize = 20)
    {
        var result = await _feedService.GetDiscoverFeedAsync(GetUserId(), lastPostId, pageSize);
        return Ok(result);
    }

    [HttpGet("vendor/{vendorId:int}")]
    public async Task<IActionResult> GetVendorPosts(int vendorId, [FromQuery] int? lastPostId, [FromQuery] int pageSize = 20)
    {
        var result = await _feedService.GetVendorPostsAsync(vendorId, GetUserId(), lastPostId, pageSize);
        return Ok(result);
    }

    // ============================================
    // POSTS CRUD
    // ============================================

    [HttpPost("posts")]
    public async Task<IActionResult> CreatePost([FromBody] SocialPostCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _feedService.CreatePostAsync(dto, vendorId.Value, GetUserId());
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { id = result.Data, message = result.Message });
    }

    [HttpGet("posts/{id:int}")]
    public async Task<IActionResult> GetPost(int id)
    {
        var post = await _feedService.GetPostByIdAsync(id, GetUserId());
        if (post == null) return NotFound();
        return Ok(post);
    }

    [HttpPut("posts/{id:int}")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] SocialPostUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _feedService.UpdatePostAsync(id, dto, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpDelete("posts/{id:int}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _feedService.DeletePostAsync(id, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpPost("posts/{id:int}/publish")]
    public async Task<IActionResult> PublishPost(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _feedService.PublishPostAsync(id, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // ============================================
    // LIKES
    // ============================================

    [HttpPost("posts/{id:int}/like")]
    public async Task<IActionResult> ToggleLike(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _feedService.ToggleLikeAsync(id, GetUserId(), vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // ============================================
    // COMMENTS
    // ============================================

    [HttpGet("posts/{id:int}/comments")]
    public async Task<IActionResult> GetComments(int id, [FromQuery] int? lastCommentId, [FromQuery] int pageSize = 20)
    {
        var comments = await _feedService.GetCommentsAsync(id, GetUserId(), lastCommentId, pageSize);
        return Ok(comments);
    }

    [HttpPost("posts/{id:int}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] SocialPostCommentCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _feedService.AddCommentAsync(id, dto, GetUserId(), vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { id = result.Data, message = result.Message });
    }

    [HttpDelete("comments/{id:int}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _feedService.DeleteCommentAsync(id, GetUserId(), vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }
}
