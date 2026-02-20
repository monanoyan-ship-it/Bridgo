using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Her capability icin profil - Tek tablo
/// VendorId + CapabilityId unique
/// Capabilities: Satici(2), Alici(3), Tasimaci(4), Sigorta(5), Gumruk(6), Gozetim(7), Yatirimci(8)
/// </summary>
public class CapabilityProfile : BaseEntity
{
    // === ILISKI ===
    public int VendorId { get; set; }
    public int CapabilityId { get; set; }  // VendorCapabilities.Id

    // === GENEL BILGILER ===
    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    [MaxLength(500)]
    public string? Tagline { get; set; }

    // === HIZMETLER VE YETENEKLER ===
    [MaxLength(2000)]
    public string? Services { get; set; }  // JSON - Sunulan hizmetler

    [MaxLength(1000)]
    public string? Capabilities { get; set; }  // JSON - Yetenekler

    [MaxLength(1000)]
    public string? Certifications { get; set; }  // JSON - Sertifikalar

    [MaxLength(1000)]
    public string? ServiceRegions { get; set; }  // JSON - Hizmet bölgeleri

    // === SATICI (2) OZEL ALANLARI ===
    [MaxLength(500)]
    public string? CategoryIds { get; set; }  // Virgul ile ayrilmis kategori ID'leri

    [MaxLength(1000)]
    public string? ProductionCapacity { get; set; }

    [MaxLength(500)]
    public string? MinimumOrderValue { get; set; }

    [MaxLength(500)]
    public string? LeadTime { get; set; }

    // === ALICI (3) OZEL ALANLARI ===
    [MaxLength(500)]
    public string? Industry { get; set; }

    [MaxLength(500)]
    public string? BusinessType { get; set; }

    [MaxLength(500)]
    public string? PreferredCategories { get; set; }  // JSON

    public decimal? AnnualPurchaseVolume { get; set; }

    [MaxLength(10)]
    public string? PurchaseVolumeCurrency { get; set; }

    // === TASIMACI (4) OZEL ALANLARI ===
    [MaxLength(500)]
    public string? FleetInfo { get; set; }  // JSON

    [MaxLength(500)]
    public string? TransportModes { get; set; }  // JSON

    [MaxLength(1000)]
    public string? Routes { get; set; }  // JSON

    // === SIGORTA (5) OZEL ALANLARI ===
    [MaxLength(500)]
    public string? InsuranceTypes { get; set; }  // JSON

    [MaxLength(500)]
    public string? CoverageTypes { get; set; }  // JSON

    public decimal? MaxCoverageAmount { get; set; }

    [MaxLength(10)]
    public string? MaxCoverageCurrency { get; set; }

    // === GUMRUK (6) OZEL ALANLARI ===
    [MaxLength(500)]
    public string? CustomsServices { get; set; }  // JSON

    [MaxLength(500)]
    public string? LicenseNumbers { get; set; }

    [MaxLength(500)]
    public string? CustomsOffices { get; set; }  // JSON

    // === GOZETIM (7) OZEL ALANLARI ===
    [MaxLength(500)]
    public string? InspectionTypes { get; set; }  // JSON

    [MaxLength(500)]
    public string? InspectionStandards { get; set; }  // JSON

    // === YATIRIMCI (8) OZEL ALANLARI ===
    [MaxLength(500)]
    public string? InvestmentTypes { get; set; }  // JSON

    [MaxLength(500)]
    public string? InvestmentFocus { get; set; }  // JSON

    public decimal? MinInvestmentAmount { get; set; }

    public decimal? MaxInvestmentAmount { get; set; }

    [MaxLength(10)]
    public string? InvestmentCurrency { get; set; }

    [MaxLength(500)]
    public string? InterestRateRange { get; set; }

    // === LOKASYON ===
    public int? CountryId { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    // === ILETISIM ===
    [MaxLength(200)]
    public string? PublicEmail { get; set; }

    [MaxLength(50)]
    public string? PublicPhone { get; set; }

    [MaxLength(500)]
    public string? PublicWebsite { get; set; }

    // === GORSELLER ===
    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    [MaxLength(2000)]
    public string? GalleryImages { get; set; }  // JSON

    // === SEO ===
    [MaxLength(200)]
    public string? MetaTitle { get; set; }

    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    // === DURUM VE ONAY ===
    public bool IsPubliclyVisible { get; set; } = false;  // Kullanici yayinlamak istiyor
    public bool IsVerified { get; set; } = false;  // Admin onayli
    public bool AcceptingNewRequests { get; set; } = true;
    public bool ShowContactInfo { get; set; } = true;

    // Onay sureci
    public DateTime? PublicationRequestedAt { get; set; }  // Yayinlama talep tarihi
    public DateTime? VerifiedAt { get; set; }  // Onay tarihi
    public int? VerifiedByUserId { get; set; }  // Onaylayan admin

    [MaxLength(500)]
    public string? RejectionReason { get; set; }  // Red sebebi

    // === MINI WEBSITE ===
    [MaxLength(2000)]
    public string? SocialLinks { get; set; }  // JSON - {"linkedin":"url","twitter":"url","facebook":"url","instagram":"url","youtube":"url"}

    [MaxLength(1000)]
    public string? WorkingHours { get; set; }  // JSON - {"mon":"09:00-18:00","tue":"09:00-18:00",...,"sat":"closed","sun":"closed"}

    [MaxLength(2000)]
    public string? Highlights { get; set; }  // JSON - [{"icon":"bi-truck","value":"50+","label":"Ulkeye Ihracat"}]

    [MaxLength(200)]
    public string? FeaturedProductIds { get; set; }  // Virgul ile ayrilmis urun ID'leri: "12,45,78"

    // === ISTATISTIKLER ===
    public int ViewCount { get; set; } = 0;
    public int ContactRequestCount { get; set; } = 0;
    public int ResponseCount { get; set; } = 0;
    public decimal? AverageRating { get; set; }
    public int RatingCount { get; set; } = 0;

    // === NAVIGATION ===
    public Vendor Vendor { get; set; } = null!;
    public Country? Country { get; set; }
}
