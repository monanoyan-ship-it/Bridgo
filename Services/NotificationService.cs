using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Notification;
using Bridgo.Hubs;
using Bridgo.Models.Entities;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Bildirim servisi implementasyonu
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        ApplicationDbContext context,
        ILogger<NotificationService> logger,
        IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
    }

    #region NOTIFICATION CRUD

    public async Task<List<NotificationDto>> GetNotificationsAsync(int vendorId, int? userId = null, bool unreadOnly = false, int limit = 20)
    {
        var query = _context.Notifications
            .Where(n => n.VendorId == vendorId)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(n => n.UserId == null || n.UserId == userId.Value);
        }

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .Select(n => MapToDto(n))
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int vendorId, int? userId = null)
    {
        var query = _context.Notifications
            .Where(n => n.VendorId == vendorId && !n.IsRead);

        if (userId.HasValue)
        {
            query = query.Where(n => n.UserId == null || n.UserId == userId.Value);
        }

        return await query.CountAsync();
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int vendorId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.VendorId == vendorId);

        if (notification == null) return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(int vendorId, int? userId = null)
    {
        var query = _context.Notifications
            .Where(n => n.VendorId == vendorId && !n.IsRead);

        if (userId.HasValue)
        {
            query = query.Where(n => n.UserId == null || n.UserId == userId.Value);
        }

        var notifications = await query.ToListAsync();
        var now = DateTime.UtcNow;

        foreach (var n in notifications)
        {
            n.IsRead = true;
            n.ReadAt = now;
        }

        await _context.SaveChangesAsync();
        return notifications.Count;
    }

    public async Task<bool> DeleteAsync(int notificationId, int vendorId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.VendorId == vendorId);

        if (notification == null) return false;

        notification.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region NOTIFICATION CREATION

    public async Task<NotificationDto> CreateAsync(NotificationCreateDto dto)
    {
        var notification = new Notification
        {
            VendorId = dto.VendorId,
            UserId = dto.UserId,
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            ActionUrl = dto.ActionUrl,
            Icon = dto.Icon ?? GetDefaultIcon(dto.Type),
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // E-posta gondermek isteniyorsa (TODO: Email servisi entegrasyonu)
        if (dto.SendEmail)
        {
            _logger.LogInformation("Email notification would be sent for notification {NotificationId}", notification.Id);
            notification.EmailSent = true;
            notification.EmailSentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        var notificationDto = MapToDto(notification);

        // SignalR ile gercek zamanli bildirim gonder
        await SendRealTimeNotificationAsync(notificationDto, dto.VendorId, dto.UserId);

        return notificationDto;
    }

    public async Task<int> CreateBulkAsync(List<NotificationCreateDto> notifications)
    {
        var entities = notifications.Select(dto => new Notification
        {
            VendorId = dto.VendorId,
            UserId = dto.UserId,
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            ActionUrl = dto.ActionUrl,
            Icon = dto.Icon ?? GetDefaultIcon(dto.Type),
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.Notifications.AddRange(entities);
        await _context.SaveChangesAsync();

        // SignalR ile gercek zamanli bildirimler gonder
        for (int i = 0; i < entities.Count; i++)
        {
            var dto = MapToDto(entities[i]);
            await SendRealTimeNotificationAsync(dto, notifications[i].VendorId, notifications[i].UserId);
        }

        return entities.Count;
    }

    #endregion

    #region DEMAND NOTIFICATIONS

    public async Task NotifySubscribersAsync(PublicDemand demand)
    {
        if (demand.CategoryId == null) return;

        // Bu kategoriye abone olan vendor'lari bul
        var subscriptions = await _context.CategorySubscriptions
            .Where(s => s.CategoryId == demand.CategoryId && s.NotifyInApp)
            .Include(s => s.Category)
            .ToListAsync();

        if (!subscriptions.Any()) return;

        var notifications = new List<NotificationCreateDto>();

        foreach (var sub in subscriptions)
        {
            // Kendi talebine bildirim gonderme
            if (sub.VendorId == demand.VendorId) continue;

            // Miktar filtresi
            if (sub.MinQuantity.HasValue && demand.Quantity.HasValue && demand.Quantity < sub.MinQuantity)
                continue;

            // Ulke filtresi
            if (!string.IsNullOrEmpty(sub.CountryFilter) && demand.CountryId.HasValue)
            {
                // TODO: Ulke filtresini uygula
            }

            // Keyword filtresi
            if (!string.IsNullOrEmpty(sub.KeywordFilter))
            {
                var keywords = sub.KeywordFilter.ToLower().Split(',').Select(k => k.Trim()).Where(k => !string.IsNullOrEmpty(k));
                var demandText = $"{demand.Title} {demand.Description}".ToLower();
                if (!keywords.Any(k => demandText.Contains(k)))
                    continue;
            }

            notifications.Add(new NotificationCreateDto
            {
                VendorId = sub.VendorId,
                Type = NotificationType.NewDemand,
                Title = "Yeni Talep: " + demand.Title,
                Message = $"{sub.Category?.Name ?? "Kategori"} kategorisinde yeni bir talep yayinlandi.",
                EntityType = "Demand",
                EntityId = demand.Id,
                ActionUrl = $"/Demands/{demand.Slug}",
                SendEmail = sub.NotifyByEmail
            });

            // Subscription istatistiklerini guncelle
            sub.NotificationCount++;
            sub.LastNotifiedAt = DateTime.UtcNow;
        }

        if (notifications.Any())
        {
            await CreateBulkAsync(notifications);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created {Count} notifications for demand {DemandId}", notifications.Count, demand.Id);
        }
    }

    public async Task NotifyNewResponseAsync(DemandResponse response)
    {
        var demand = await _context.PublicDemands
            .FirstOrDefaultAsync(d => d.Id == response.DemandId);

        if (demand == null) return;

        var supplierName = response.SupplierVendorId == null
            ? response.ExternalCompanyName
            : "Bir tedarikci";

        await CreateAsync(new NotificationCreateDto
        {
            VendorId = demand.VendorId,
            Type = NotificationType.NewResponse,
            Title = "Yeni Teklif: " + demand.Title,
            Message = $"{supplierName} talebinize teklif verdi.",
            EntityType = "DemandResponse",
            EntityId = response.Id,
            ActionUrl = "/Dashboard/MyDemands",
            SendEmail = true
        });
    }

    public async Task NotifyResponseStatusChangedAsync(DemandResponse response, int oldStatus)
    {
        if (response.SupplierVendorId == null) return;

        var demand = await _context.PublicDemands
            .FirstOrDefaultAsync(d => d.Id == response.DemandId);

        if (demand == null) return;

        var type = response.Status switch
        {
            4 => NotificationType.ResponseAccepted, // Accepted
            5 => NotificationType.ResponseRejected, // Rejected
            _ => NotificationType.Info
        };

        var statusText = response.Status switch
        {
            4 => "kabul edildi",
            5 => "reddedildi",
            3 => "degerlendiriliyor",
            _ => "guncellendi"
        };

        await CreateAsync(new NotificationCreateDto
        {
            VendorId = response.SupplierVendorId.Value,
            Type = type,
            Title = $"Teklifiniz {statusText}",
            Message = $"\"{demand.Title}\" talebine verdiginiz teklif {statusText}.",
            EntityType = "DemandResponse",
            EntityId = response.Id,
            ActionUrl = "/Dashboard/MyDemands",
            SendEmail = true
        });
    }

    #endregion

    #region Private Helpers

    private static NotificationDto MapToDto(Notification n)
    {
        return new NotificationDto
        {
            Id = n.Id,
            VendorId = n.VendorId,
            UserId = n.UserId,
            Type = (int)n.Type,
            TypeName = GetTypeName(n.Type),
            Title = n.Title,
            Message = n.Message,
            EntityType = n.EntityType,
            EntityId = n.EntityId,
            ActionUrl = n.ActionUrl,
            Icon = n.Icon,
            IsRead = n.IsRead,
            ReadAt = n.ReadAt,
            CreatedAt = n.CreatedAt,
            TimeAgo = GetTimeAgo(n.CreatedAt)
        };
    }

    private static string GetTypeName(NotificationType type)
    {
        return type switch
        {
            NotificationType.Info => "Bilgi",
            NotificationType.NewDemand => "Yeni Talep",
            NotificationType.NewResponse => "Yeni Teklif",
            NotificationType.ResponseAccepted => "Teklif Kabul",
            NotificationType.ResponseRejected => "Teklif Red",
            NotificationType.DemandStatusChanged => "Talep Guncellendi",
            NotificationType.ContactMessage => "Mesaj",
            NotificationType.System => "Sistem",
            _ => "Bildirim"
        };
    }

    private static string GetDefaultIcon(NotificationType type)
    {
        return type switch
        {
            NotificationType.Info => "bi-info-circle",
            NotificationType.NewDemand => "bi-megaphone",
            NotificationType.NewResponse => "bi-chat-left-text",
            NotificationType.ResponseAccepted => "bi-check-circle",
            NotificationType.ResponseRejected => "bi-x-circle",
            NotificationType.DemandStatusChanged => "bi-arrow-repeat",
            NotificationType.ContactMessage => "bi-envelope",
            NotificationType.System => "bi-gear",
            _ => "bi-bell"
        };
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1) return "simdi";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} dakika once";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} saat once";
        if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} gun once";
        if (timeSpan.TotalDays < 30) return $"{(int)(timeSpan.TotalDays / 7)} hafta once";
        if (timeSpan.TotalDays < 365) return $"{(int)(timeSpan.TotalDays / 30)} ay once";
        return $"{(int)(timeSpan.TotalDays / 365)} yil once";
    }

    /// <summary>
    /// SignalR ile gercek zamanli bildirim gonder
    /// </summary>
    private async Task SendRealTimeNotificationAsync(NotificationDto notification, int vendorId, int? userId)
    {
        try
        {
            var message = new NotificationMessage
            {
                Id = notification.Id,
                Type = notification.TypeName,
                Title = notification.Title,
                Message = notification.Message,
                ActionUrl = notification.ActionUrl,
                Icon = notification.Icon,
                CreatedAt = notification.CreatedAt
            };

            // Kullaniciya ozel bildirim
            if (userId.HasValue)
            {
                await _hubContext.Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", message);
            }
            else
            {
                // Tum vendor kullanicilarina bildirim
                await _hubContext.Clients.Group($"vendor-{vendorId}").SendAsync("ReceiveNotification", message);
            }

            _logger.LogDebug("Real-time notification sent: {NotificationId} to vendor {VendorId}", notification.Id, vendorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send real-time notification: {NotificationId}", notification.Id);
        }
    }

    #endregion
}
