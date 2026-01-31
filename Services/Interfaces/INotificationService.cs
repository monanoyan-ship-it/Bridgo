using Bridgo.DTOs.Notification;
using Bridgo.Models.Entities;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Bildirim servisi interface
/// </summary>
public interface INotificationService
{
    // ============================================
    // NOTIFICATION CRUD
    // ============================================

    /// <summary>
    /// Vendor'un bildirimlerini getir
    /// </summary>
    Task<List<NotificationDto>> GetNotificationsAsync(int vendorId, int? userId = null, bool unreadOnly = false, int limit = 20);

    /// <summary>
    /// Okunmamis bildirim sayisini getir
    /// </summary>
    Task<int> GetUnreadCountAsync(int vendorId, int? userId = null);

    /// <summary>
    /// Bildirimi okundu olarak isaretle
    /// </summary>
    Task<bool> MarkAsReadAsync(int notificationId, int vendorId);

    /// <summary>
    /// Tum bildirimleri okundu olarak isaretle
    /// </summary>
    Task<int> MarkAllAsReadAsync(int vendorId, int? userId = null);

    /// <summary>
    /// Bildirimi sil
    /// </summary>
    Task<bool> DeleteAsync(int notificationId, int vendorId);

    // ============================================
    // NOTIFICATION CREATION
    // ============================================

    /// <summary>
    /// Bildirim olustur
    /// </summary>
    Task<NotificationDto> CreateAsync(NotificationCreateDto dto);

    /// <summary>
    /// Toplu bildirim olustur (ornegin yeni talep geldiginde tum abonelere)
    /// </summary>
    Task<int> CreateBulkAsync(List<NotificationCreateDto> notifications);

    // ============================================
    // DEMAND NOTIFICATIONS
    // ============================================

    /// <summary>
    /// Yeni talep icin kategori abonelerine bildirim gonder
    /// </summary>
    Task NotifySubscribersAsync(PublicDemand demand);

    /// <summary>
    /// Talebinize teklif geldi bildirimi
    /// </summary>
    Task NotifyNewResponseAsync(DemandResponse response);

    /// <summary>
    /// Teklif durumu degisti bildirimi
    /// </summary>
    Task NotifyResponseStatusChangedAsync(DemandResponse response, int oldStatus);
}
