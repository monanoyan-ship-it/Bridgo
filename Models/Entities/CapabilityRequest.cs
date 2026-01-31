using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

/// <summary>
/// Firma capability ekleme talebi
/// Kullanici sirketi icin yeni capability talep eder, admin onaylar/reddeder
/// </summary>
public class CapabilityRequest : BaseEntity
{
    /// <summary>
    /// Talep eden firma
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Talep edilen capability
    /// Capabilities.Ids'den bir deger (Seller=2, Buyer=3, vb.)
    /// </summary>
    public int CapabilityId { get; set; }

    /// <summary>
    /// Talep eden kullanici
    /// </summary>
    public int RequestedByUserId { get; set; }

    /// <summary>
    /// Talep durumu (CapabilityRequestStatuses)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Kullanici notu - neden bu capability isteniyor
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Admin notu - onay/red nedeni
    /// </summary>
    public string? AdminNotes { get; set; }

    /// <summary>
    /// Isleme alan admin kullanici
    /// </summary>
    public int? ProcessedByUserId { get; set; }

    /// <summary>
    /// Islem tarihi (onay/red)
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    // Navigation properties (Capability artik static class)
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual ApplicationUser RequestedByUser { get; set; } = null!;
    public virtual ApplicationUser? ProcessedByUser { get; set; }
}
