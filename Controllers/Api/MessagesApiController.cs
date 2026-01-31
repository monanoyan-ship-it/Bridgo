using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.Authorization;
using Bridgo.DTOs.Message;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/messages")]
[Authorize]
[RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
public class MessagesApiController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly UserManager<ApplicationUser> _userManager;

    public MessagesApiController(
        IMessageService messageService,
        UserManager<ApplicationUser> userManager)
    {
        _messageService = messageService;
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
    // THREAD OPERATIONS
    // ============================================

    /// <summary>
    /// Mesaj konularini listele
    /// </summary>
    [HttpGet("threads")]
    public async Task<IActionResult> GetThreads([FromQuery] MessageThreadFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _messageService.GetThreadsAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Konu detayini getir
    /// </summary>
    [HttpGet("threads/{threadId}")]
    public async Task<IActionResult> GetThreadDetail(int threadId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _messageService.GetThreadDetailAsync(vendorId.Value, threadId);
        if (result == null) return NotFound(new { message = "Konu bulunamadi." });

        return Ok(result);
    }

    /// <summary>
    /// Yeni mesaj konusu olustur
    /// </summary>
    [HttpPost("threads")]
    public async Task<IActionResult> CreateThread([FromBody] CreateThreadDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var userId = GetUserId();
        var result = await _messageService.CreateThreadAsync(vendorId.Value, userId, dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    /// <summary>
    /// Konuyu kapat
    /// </summary>
    [HttpPost("threads/{threadId}/close")]
    public async Task<IActionResult> CloseThread(int threadId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _messageService.CloseThreadAsync(vendorId.Value, threadId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Konuyu yeniden ac
    /// </summary>
    [HttpPost("threads/{threadId}/reopen")]
    public async Task<IActionResult> ReopenThread(int threadId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _messageService.ReopenThreadAsync(vendorId.Value, threadId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    // ============================================
    // MESSAGE OPERATIONS
    // ============================================

    /// <summary>
    /// Mesaj gonder
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var userId = GetUserId();
        var result = await _messageService.SendMessageAsync(vendorId.Value, userId, dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    /// <summary>
    /// Mesajlari okundu olarak isaretle
    /// </summary>
    [HttpPost("read")]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _messageService.MarkAsReadAsync(vendorId.Value, dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Mesaj sil
    /// </summary>
    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _messageService.DeleteMessageAsync(vendorId.Value, messageId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    // ============================================
    // STATS
    // ============================================

    /// <summary>
    /// Mesaj istatistiklerini getir
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _messageService.GetStatsAsync(vendorId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Okunmamis mesaj sayisini getir
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var count = await _messageService.GetUnreadCountAsync(vendorId.Value);
        return Ok(new { count });
    }

    /// <summary>
    /// Referansa gore konuyu getir
    /// </summary>
    [HttpGet("by-reference")]
    public async Task<IActionResult> GetByReference([FromQuery] int referenceType, [FromQuery] int referenceId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _messageService.GetThreadByReferenceAsync(vendorId.Value, referenceType, referenceId);
        if (result == null) return NotFound(new { message = "Konu bulunamadi." });

        return Ok(result);
    }
}
