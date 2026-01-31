namespace Bridgo.Models.Entities;

/// <summary>
/// Vendor'in varsayilan servis tercihleri
/// Checkout'ta bu tercihler varsayilan olarak kullanilir
/// </summary>
public class VendorServicePreference : BaseEntity
{
    /// <summary>
    /// Vendor ID (FK)
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Servis tipi (ServiceTypes'dan: Logistics, Insurance, Customs, Financing)
    /// </summary>
    public int ServiceType { get; set; }

    /// <summary>
    /// Bu servis tipi aktif mi? (Kullanici bu servisi kullanmak istiyor mu?)
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Checkout'ta varsayilan secili mi?
    /// </summary>
    public bool IsDefaultSelected { get; set; }

    /// <summary>
    /// Tercih edilen saglayici vendor ID (opsiyonel)
    /// </summary>
    public int? PreferredProviderVendorId { get; set; }

    /// <summary>
    /// Aciklama/notlar
    /// </summary>
    public string? Notes { get; set; }

    // Navigation
    public Vendor? Vendor { get; set; }
    public Vendor? PreferredProvider { get; set; }
}
