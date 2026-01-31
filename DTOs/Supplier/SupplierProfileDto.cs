namespace Bridgo.DTOs.Supplier;

// ============================================
// SUPPLIER PROFILE DTO'lari
// ============================================

/// <summary>
/// Liste görünümü için tedarikçi profil DTO
/// </summary>
public class SupplierProfileListDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? CompanyName { get; set; }
    public string? ShortDescription { get; set; }
    public string? Tagline { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? LogoUrl { get; set; }
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public string? City { get; set; }
    public string? CategoryIds { get; set; }
    public List<CategoryInfoDto>? Categories { get; set; }
    public string? Certifications { get; set; }
    public bool IsVerified { get; set; }
    public bool AcceptingNewRequests { get; set; }
    public decimal? AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int ViewCount { get; set; }
    public int ResponseCount { get; set; }
}

/// <summary>
/// Detay görünümü için tedarikçi profil DTO
/// </summary>
public class SupplierProfileDetailDto : SupplierProfileListDto
{
    public string? Description { get; set; }
    public string? Capabilities { get; set; }
    public string? ProductionCapacity { get; set; }
    public string? MinimumOrderValue { get; set; }
    public string? LeadTime { get; set; }
    public string? ServiceRegions { get; set; }
    public string? PublicEmail { get; set; }
    public string? PublicPhone { get; set; }
    public string? PublicWebsite { get; set; }
    public string? GalleryImages { get; set; }
    public List<string>? GalleryImageList { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsPubliclyVisible { get; set; }
    public bool ShowContactInfo { get; set; }
    public int ContactRequestCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Profil oluşturma/güncelleme DTO
/// </summary>
public class SupplierProfileCreateUpdateDto
{
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? Tagline { get; set; }
    public string? Capabilities { get; set; }
    public string? ProductionCapacity { get; set; }
    public string? LeadTime { get; set; }
    public string? CategoryIds { get; set; }
    public List<int>? CategoryIdList { get; set; }
    public string? Certifications { get; set; }
    public int? CountryId { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ServiceRegions { get; set; }
    public string? PublicEmail { get; set; }
    public string? Email { get; set; }
    public string? PublicPhone { get; set; }
    public string? Phone { get; set; }
    public string? PublicWebsite { get; set; }
    public string? Website { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? GalleryImages { get; set; }
    public List<GalleryImageDto>? GalleryImageList { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? Slug { get; set; }
    public bool IsPubliclyVisible { get; set; }
    public bool IsPublic { get; set; }
    public bool IsIndexable { get; set; } = true;
    public bool AcceptingNewRequests { get; set; } = true;
    public bool ShowContactInfo { get; set; } = true;
    public bool AcceptsCustomOrders { get; set; }
    public int? FoundedYear { get; set; }
    public string? EmployeeCountRange { get; set; }
    public decimal? MinimumOrderValue { get; set; }
    public string? MinimumOrderCurrency { get; set; }
    public int? LeadTimeDays { get; set; }
    public string? ExportCountries { get; set; }
    public string? PaymentTerms { get; set; }
}

/// <summary>
/// Galeri gorseli DTO
/// </summary>
public class GalleryImageDto
{
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
}

// ============================================
// CATEGORY SUBSCRIPTION DTO'lari
// ============================================

public class CategorySubscriptionDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryPath { get; set; }
    public bool NotifyByEmail { get; set; }
    public bool NotifyInApp { get; set; }
    public int? MinQuantity { get; set; }
    public string? CountryFilter { get; set; }
    public string? KeywordFilter { get; set; }
    public int NotificationCount { get; set; }
    public DateTime? LastNotifiedAt { get; set; }

    // UI icin - bu kategorideki aktif talep sayisi
    public int DemandCount { get; set; }
}

public class CategorySubscriptionCreateDto
{
    public int CategoryId { get; set; }
    public bool NotifyByEmail { get; set; } = true;
    public bool NotifyInApp { get; set; } = true;
    public int? MinQuantity { get; set; }
    public string? CountryFilter { get; set; }
    public string? KeywordFilter { get; set; }
}

public class CategorySubscriptionUpdateDto
{
    public bool NotifyByEmail { get; set; }
    public bool NotifyInApp { get; set; }
    public int? MinQuantity { get; set; }
    public string? CountryFilter { get; set; }
    public string? KeywordFilter { get; set; }
}

// ============================================
// FILTRE VE ARAMA DTO'lari
// ============================================

public class SupplierFilterDto
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? CountryId { get; set; }
    public string? City { get; set; }
    public bool? IsVerified { get; set; }
    public bool? AcceptingNewRequests { get; set; }
    public decimal? MinRating { get; set; }
    public string? Certifications { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

public class SupplierSearchResultDto
{
    public List<SupplierProfileListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

// ============================================
// YARDIMCI DTO'lar
// ============================================

public class CategoryInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

public class SupplierStatsDto
{
    public int TotalViews { get; set; }
    public int TotalContactRequests { get; set; }
    public int TotalResponses { get; set; }
    public int AcceptedResponses { get; set; }
    public decimal? AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int SubscribedCategories { get; set; }
}

/// <summary>
/// Tedarikçiye iletişim mesajı DTO
/// </summary>
public class SupplierContactDto
{
    public int SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
}
