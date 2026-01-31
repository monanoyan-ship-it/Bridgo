using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.Authorization;
using Bridgo.DTOs.Notification;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/notifications")]
[Authorize]
[RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
public class NotificationsApiController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationsApiController(
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager)
    {
        _notificationService = notificationService;
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
    // NOTIFICATION CRUD
    // ============================================

    /// <summary>
    /// Bildirimleri listele
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int limit = 20)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var notifications = await _notificationService.GetNotificationsAsync(
            vendorId.Value, GetUserId(), unreadOnly, limit);
        return Ok(notifications);
    }

    /// <summary>
    /// Okunmamis bildirim sayisini getir
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var count = await _notificationService.GetUnreadCountAsync(vendorId.Value, GetUserId());
        return Ok(new NotificationCountDto { UnreadCount = count });
    }

    /// <summary>
    /// Bildirimi okundu olarak isaretle
    /// </summary>
    [HttpPost("{id:int}/mark-read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _notificationService.MarkAsReadAsync(id, vendorId.Value);
        if (!success) return NotFound(new { message = "Bildirim bulunamadi." });

        return Ok(new { message = "Bildirim okundu olarak isaretlendi." });
    }

    /// <summary>
    /// Tum bildirimleri okundu olarak isaretle
    /// </summary>
    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var count = await _notificationService.MarkAllAsReadAsync(vendorId.Value, GetUserId());
        return Ok(new { message = $"{count} bildirim okundu olarak isaretlendi." });
    }

    /// <summary>
    /// Bildirimi sil
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _notificationService.DeleteAsync(id, vendorId.Value);
        if (!success) return NotFound(new { message = "Bildirim bulunamadi." });

        return Ok(new { message = "Bildirim silindi." });
    }
}
