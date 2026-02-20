namespace Bridgo.DTOs.CapabilityProfile;

/// <summary>
/// Profil detay DTO - tum capability tipleri icin ortak
/// </summary>
public class CapabilityProfileDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public int CapabilityId { get; set; }
    public string CapabilityName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    // Genel bilgiler
    public string? DisplayName { get; set; }
    public string? CompanyName { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? Tagline { get; set; }

    // Hizmetler
    public string? Services { get; set; }
    public string? Capabilities { get; set; }
    public string? Certifications { get; set; }
    public string? ServiceRegions { get; set; }

    // Lokasyon
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }

    // Iletisim
    public string? PublicEmail { get; set; }
    public string? PublicPhone { get; set; }
    public string? PublicWebsite { get; set; }

    // Gorseller
    public string? CoverImageUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? GalleryImages { get; set; }
    public List<GalleryImageItemDto>? GalleryImageList { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    // Durum ve Onay
    public bool IsPubliclyVisible { get; set; }
    public bool IsVerified { get; set; }
    public bool AcceptingNewRequests { get; set; }
    public bool ShowContactInfo { get; set; }
    public DateTime? PublicationRequestedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Computed: Profil herkes tarafindan gorulebilir mi?
    public bool IsPublic => IsPubliclyVisible && IsVerified;
    // Onay bekliyor mu?
    public bool IsPendingApproval => IsPubliclyVisible && !IsVerified && RejectionReason == null;

    // Mini Website
    public string? SocialLinks { get; set; }
    public List<SocialLinkItemDto>? SocialLinkList { get; set; }
    public string? WorkingHours { get; set; }
    public List<WorkingHourItemDto>? WorkingHourList { get; set; }
    public string? Highlights { get; set; }
    public List<HighlightItemDto>? HighlightList { get; set; }
    public string? FeaturedProductIds { get; set; }
    public int ProductCount { get; set; }  // Vendor'in toplam aktif urun sayisi

    // Istatistikler
    public int ViewCount { get; set; }
    public int ContactRequestCount { get; set; }
    public int ResponseCount { get; set; }
    public decimal? AverageRating { get; set; }
    public int RatingCount { get; set; }

    // === SATICI (2) OZEL ALANLARI ===
    public string? CategoryIds { get; set; }
    public List<CategoryInfoItemDto>? Categories { get; set; }
    public string? ProductionCapacity { get; set; }
    public string? MinimumOrderValue { get; set; }
    public string? LeadTime { get; set; }

    // === ALICI (3) OZEL ALANLARI ===
    public string? Industry { get; set; }
    public string? BusinessType { get; set; }
    public string? PreferredCategories { get; set; }
    public decimal? AnnualPurchaseVolume { get; set; }
    public string? PurchaseVolumeCurrency { get; set; }

    // === TASIMACI (4) OZEL ALANLARI ===
    public string? FleetInfo { get; set; }
    public string? TransportModes { get; set; }
    public string? Routes { get; set; }

    // === SIGORTA (5) OZEL ALANLARI ===
    public string? InsuranceTypes { get; set; }
    public string? CoverageTypes { get; set; }
    public decimal? MaxCoverageAmount { get; set; }
    public string? MaxCoverageCurrency { get; set; }

    // === GUMRUK (6) OZEL ALANLARI ===
    public string? CustomsServices { get; set; }
    public string? LicenseNumbers { get; set; }
    public string? CustomsOffices { get; set; }

    // === GOZETIM (7) OZEL ALANLARI ===
    public string? InspectionTypes { get; set; }
    public string? InspectionStandards { get; set; }

    // === YATIRIMCI (8) OZEL ALANLARI ===
    public string? InvestmentTypes { get; set; }
    public string? InvestmentFocus { get; set; }
    public decimal? MinInvestmentAmount { get; set; }
    public decimal? MaxInvestmentAmount { get; set; }
    public string? InvestmentCurrency { get; set; }
    public string? InterestRateRange { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Profil olusturma/guncelleme DTO
/// </summary>
public class CapabilityProfileCreateUpdateDto
{
    // Genel
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? Tagline { get; set; }

    // Hizmetler
    public string? Services { get; set; }
    public string? Capabilities { get; set; }
    public string? Certifications { get; set; }
    public string? ServiceRegions { get; set; }

    // Lokasyon
    public int? CountryId { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }

    // Iletisim
    public string? PublicEmail { get; set; }
    public string? Email { get; set; }  // Alias
    public string? PublicPhone { get; set; }
    public string? Phone { get; set; }  // Alias
    public string? PublicWebsite { get; set; }
    public string? Website { get; set; }  // Alias

    // Gorseller
    public string? CoverImageUrl { get; set; }
    public string? GalleryImages { get; set; }
    public List<GalleryImageItemDto>? GalleryImageList { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Slug { get; set; }

    // Mini Website
    public string? SocialLinks { get; set; }
    public List<SocialLinkItemDto>? SocialLinkList { get; set; }
    public string? WorkingHours { get; set; }
    public List<WorkingHourItemDto>? WorkingHourList { get; set; }
    public string? Highlights { get; set; }
    public List<HighlightItemDto>? HighlightList { get; set; }
    public string? FeaturedProductIds { get; set; }

    // Durum
    public bool IsPubliclyVisible { get; set; }
    public bool IsPublic { get; set; }  // Alias
    public bool AcceptingNewRequests { get; set; } = true;
    public bool ShowContactInfo { get; set; } = true;

    // === SATICI (2) OZEL ALANLARI ===
    public string? CategoryIds { get; set; }
    public List<int>? CategoryIdList { get; set; }
    public string? ProductionCapacity { get; set; }
    public string? MinimumOrderValue { get; set; }
    public string? LeadTime { get; set; }

    // === ALICI (3) OZEL ALANLARI ===
    public string? Industry { get; set; }
    public string? BusinessType { get; set; }
    public string? PreferredCategories { get; set; }
    public decimal? AnnualPurchaseVolume { get; set; }
    public string? PurchaseVolumeCurrency { get; set; }

    // === TASIMACI (4) OZEL ALANLARI ===
    public string? FleetInfo { get; set; }
    public string? TransportModes { get; set; }
    public string? Routes { get; set; }

    // === SIGORTA (5) OZEL ALANLARI ===
    public string? InsuranceTypes { get; set; }
    public string? CoverageTypes { get; set; }
    public decimal? MaxCoverageAmount { get; set; }
    public string? MaxCoverageCurrency { get; set; }

    // === GUMRUK (6) OZEL ALANLARI ===
    public string? CustomsServices { get; set; }
    public string? LicenseNumbers { get; set; }
    public string? CustomsOffices { get; set; }

    // === GOZETIM (7) OZEL ALANLARI ===
    public string? InspectionTypes { get; set; }
    public string? InspectionStandards { get; set; }

    // === YATIRIMCI (8) OZEL ALANLARI ===
    public string? InvestmentTypes { get; set; }
    public string? InvestmentFocus { get; set; }
    public decimal? MinInvestmentAmount { get; set; }
    public decimal? MaxInvestmentAmount { get; set; }
    public string? InvestmentCurrency { get; set; }
    public string? InterestRateRange { get; set; }
}

/// <summary>
/// Galeri gorseli DTO
/// </summary>
public class GalleryImageItemDto
{
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
}

/// <summary>
/// Kategori bilgisi DTO
/// </summary>
public class CategoryInfoItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

/// <summary>
/// Profil istatistikleri
/// </summary>
public class CapabilityProfileStatsDto
{
    public int TotalViews { get; set; }
    public int TotalContactRequests { get; set; }
    public int TotalResponses { get; set; }
    public decimal? AverageRating { get; set; }
    public int RatingCount { get; set; }
}

/// <summary>
/// Admin onay listesi icin DTO
/// </summary>
public class ProfileApprovalListItemDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public int CapabilityId { get; set; }
    public string CapabilityName { get; set; } = string.Empty;
    public string? CapabilityIcon { get; set; }
    public string? DisplayName { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public DateTime? PublicationRequestedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Admin onay/red DTO
/// </summary>
public class ProfileApprovalActionDto
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}

/// <summary>
/// Sosyal medya linki
/// </summary>
public class SocialLinkItemDto
{
    public string Platform { get; set; } = string.Empty;  // linkedin, twitter, facebook, instagram, youtube
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Calisma saati
/// </summary>
public class WorkingHourItemDto
{
    public string Day { get; set; } = string.Empty;  // mon, tue, wed, thu, fri, sat, sun
    public string? Hours { get; set; }  // "09:00-18:00" veya null
    public bool IsClosed { get; set; }
}

/// <summary>
/// One cikan bilgi (rakamsal highlight)
/// </summary>
public class HighlightItemDto
{
    public string? Icon { get; set; }  // Bootstrap icon class: bi-truck, bi-globe, etc.
    public string Value { get; set; } = string.Empty;  // "50+", "1000+", "24/7"
    public string Label { get; set; } = string.Empty;  // "Ulkeye Ihracat", "Urun Cesidi"
}

/// <summary>
/// Profil iletisim formu DTO
/// </summary>
public class ProfileContactRequestDto
{
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string? SenderPhone { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
