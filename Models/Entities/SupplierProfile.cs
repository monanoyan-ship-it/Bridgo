using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Uretici/Tedarikci public profili
/// Google'da indexlenir, ureticiler kesfedilebilir olur
/// </summary>
public class SupplierProfile : BaseEntity
{
    // === ILISKI ===
    public int VendorId { get; set; }

    // === GENEL BILGILER ===
    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;  // SEO-friendly URL

    [MaxLength(200)]
    public string? DisplayName { get; set; }  // Gosterilecek isim (Vendor.CompanyName override)

    [MaxLength(2000)]
    public string? Description { get; set; }  // Firma tanitimi

    [MaxLength(500)]
    public string? ShortDescription { get; set; }  // Kisa tanitim

    [MaxLength(500)]
    public string? Tagline { get; set; }  // Slogan

    // === YETENEKLER VE URETIM ===
    [MaxLength(2000)]
    public string? Capabilities { get; set; }  // Uretim yetenekleri (JSON veya metin)

    [MaxLength(1000)]
    public string? ProductionCapacity { get; set; }  // Uretim kapasitesi

    [MaxLength(500)]
    public string? MinimumOrderValue { get; set; }  // Minimum siparis tutari

    [MaxLength(500)]
    public string? LeadTime { get; set; }  // Tipik teslim suresi

    // === KATEGORILER ===
    [MaxLength(500)]
    public string? CategoryIds { get; set; }  // Virgul ile ayrilmis kategori ID'leri

    // === SERTIFIKALAR ===
    [MaxLength(1000)]
    public string? Certifications { get; set; }  // ISO 9001, CE, vb. (JSON veya metin)

    // === LOKASYON ===
    public int? CountryId { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(500)]
    public string? ServiceRegions { get; set; }  // Hizmet verdigi bolgeler

    // === ILETISIM (Public gosterim) ===
    [MaxLength(200)]
    public string? PublicEmail { get; set; }

    [MaxLength(50)]
    public string? PublicPhone { get; set; }

    [MaxLength(500)]
    public string? PublicWebsite { get; set; }

    // === GORSELLER ===
    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }  // Kapak gorseli

    [MaxLength(500)]
    public string? GalleryImages { get; set; }  // Virgul ile ayrilmis gorsel URL'leri

    // === SEO ===
    [MaxLength(200)]
    public string? MetaTitle { get; set; }

    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    [MaxLength(500)]
    public string? MetaKeywords { get; set; }

    // === DURUM VE AYARLAR ===
    public bool IsPubliclyVisible { get; set; } = false;  // Public sayfada gorunsun mu

    public bool IsVerified { get; set; } = false;  // Platform tarafindan dogrulandi mi

    public bool AcceptingNewRequests { get; set; } = true;  // Yeni teklif talepleri kabul ediyor mu

    public bool ShowContactInfo { get; set; } = true;  // Iletisim bilgileri gorunsun mu

    // === ISTATISTIKLER ===
    public int ViewCount { get; set; } = 0;

    public int ContactRequestCount { get; set; } = 0;

    public int ResponseCount { get; set; } = 0;  // Verdigi teklif sayisi

    public decimal? AverageRating { get; set; }  // Ortalama puan

    public int RatingCount { get; set; } = 0;  // Puan veren sayisi

    // === NAVIGATION ===
    public Vendor Vendor { get; set; } = null!;
    public Country? Country { get; set; }
}
