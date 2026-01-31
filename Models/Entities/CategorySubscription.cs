using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Uretici/Tedarikci kategori takibi
/// Ilgilendigi kategorilerde yeni talep gelince bildirim alir
/// </summary>
public class CategorySubscription : BaseEntity
{
    public int VendorId { get; set; }

    public int CategoryId { get; set; }

    // === BILDIRIM AYARLARI ===
    public bool NotifyByEmail { get; set; } = true;

    public bool NotifyInApp { get; set; } = true;

    // === FILTRELER ===
    /// <summary>
    /// Minimum miktar filtresi
    /// Sadece bu miktarin uzerindeki talepler icin bildirim
    /// </summary>
    public int? MinQuantity { get; set; }

    /// <summary>
    /// Ulke filtresi (virgul ile ayrilmis ulke ID'leri)
    /// Bos ise tum ulkelerden bildirim alir
    /// </summary>
    [MaxLength(500)]
    public string? CountryFilter { get; set; }

    /// <summary>
    /// Anahtar kelime filtresi (virgul ile ayrilmis)
    /// Talep baslik/aciklamasinda bu kelimeler gecerse bildirim
    /// </summary>
    [MaxLength(500)]
    public string? KeywordFilter { get; set; }

    // === ISTATISTIKLER ===
    public int NotificationCount { get; set; } = 0;

    public DateTime? LastNotifiedAt { get; set; }

    // === NAVIGATION ===
    public Vendor Vendor { get; set; } = null!;
    public ProductCategory Category { get; set; } = null!;
}
