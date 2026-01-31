namespace Bridgo.Models.Entities;

/// <summary>
/// Siparis durum gecmisi
/// </summary>
public class OrderStatusHistory : BaseEntity
{
    /// <summary>
    /// Siparis ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Onceki durum (OrderStatuses)
    /// </summary>
    public int OldStatus { get; set; }

    /// <summary>
    /// Yeni durum (OrderStatuses)
    /// </summary>
    public int NewStatus { get; set; }

    /// <summary>
    /// Degisiklik notu
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Degistiren kullanici ID
    /// </summary>
    public int? ChangedByUserId { get; set; }

    // Navigation properties
    public virtual Order? Order { get; set; }
}
