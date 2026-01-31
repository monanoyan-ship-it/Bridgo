using Bridgo.Models.Entities;

namespace Bridgo.DTOs.Notification;

/// <summary>
/// Bildirim DTO
/// </summary>
public class NotificationDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public int? UserId { get; set; }
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? ActionUrl { get; set; }
    public string? Icon { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

/// <summary>
/// Bildirim olusturma DTO
/// </summary>
public class NotificationCreateDto
{
    public int VendorId { get; set; }
    public int? UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? ActionUrl { get; set; }
    public string? Icon { get; set; }
    public bool SendEmail { get; set; }
}

/// <summary>
/// Okunmamis bildirim sayisi
/// </summary>
public class NotificationCountDto
{
    public int UnreadCount { get; set; }
}
