using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Bridgo.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var vendorId = GetVendorId();
        var userId = GetUserId();

        if (vendorId.HasValue)
        {
            // Join vendor group - all vendor users will receive vendor-wide notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"vendor-{vendorId}");
            _logger.LogInformation("User {UserId} connected to notification hub for vendor {VendorId}", userId, vendorId);
        }

        if (userId.HasValue)
        {
            // Join user-specific group for personal notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var vendorId = GetVendorId();
        var userId = GetUserId();

        if (vendorId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"vendor-{vendorId}");
        }

        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        _logger.LogInformation("User {UserId} disconnected from notification hub", userId);
        await base.OnDisconnectedAsync(exception);
    }

    private int? GetVendorId()
    {
        var claim = Context.User?.FindFirst("VendorId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
    }

    private int? GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
    }
}

/// <summary>
/// Notification DTO for SignalR
/// </summary>
public class NotificationMessage
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? Icon { get; set; }
    public DateTime CreatedAt { get; set; }
}
