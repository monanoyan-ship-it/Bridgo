namespace Bridgo.DTOs.Demand;

// ============================================
// PUBLIC DEMAND DTO'lari
// ============================================

/// <summary>
/// Liste görünümü için talep DTO
/// </summary>
public class DemandListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Tags { get; set; }
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public string? City { get; set; }
    public int? DesiredLeadTimeDays { get; set; }
    public DateTime? DesiredDeliveryDate { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public string? BudgetCurrency { get; set; }
    public int Visibility { get; set; }
    public int Status { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ViewCount { get; set; }
    public int ResponseCount { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public bool HasReferenceProduct { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Detay görünümü için talep DTO
/// </summary>
public class DemandDetailDto : DemandListDto
{
    public string? ModificationNotes { get; set; }
    public int? ReferenceProductId { get; set; }
    public string? ReferenceProductName { get; set; }
    public string? ReferenceProductImage { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool IsIndexable { get; set; }
    public List<DemandModificationDto> Modifications { get; set; } = new();
    public List<DemandAttachmentDto> Attachments { get; set; } = new();
    public List<DemandResponseListDto> Responses { get; set; } = new();
}

/// <summary>
/// Talep oluşturma DTO
/// </summary>
public class DemandCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public int? CategoryId { get; set; }
    public string? Tags { get; set; }
    public int? ReferenceProductId { get; set; }
    public string? ModificationNotes { get; set; }
    public int? CountryId { get; set; }
    public string? City { get; set; }
    public int? DesiredLeadTimeDays { get; set; }
    public DateTime? DesiredDeliveryDate { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public string? BudgetCurrency { get; set; }
    public int Visibility { get; set; } = 1;  // Default: Public
    public DateTime? ExpiresAt { get; set; }
    public bool IsIndexable { get; set; } = true;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public List<DemandModificationCreateDto>? Modifications { get; set; }
}

/// <summary>
/// Talep güncelleme DTO
/// </summary>
public class DemandUpdateDto : DemandCreateDto
{
    public int Status { get; set; }
}

// ============================================
// DEMAND MODIFICATION DTO'lari
// ============================================

public class DemandModificationDto
{
    public int Id { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string? OriginalValue { get; set; }
    public string DesiredValue { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int DisplayOrder { get; set; }
}

public class DemandModificationCreateDto
{
    public string PropertyName { get; set; } = string.Empty;
    public string? OriginalValue { get; set; }
    public string DesiredValue { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int DisplayOrder { get; set; }
}

// ============================================
// DEMAND ATTACHMENT DTO'lari
// ============================================

public class DemandAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public long? FileSize { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

// ============================================
// DEMAND RESPONSE DTO'lari
// ============================================

/// <summary>
/// Liste görünümü için teklif yanıtı DTO
/// </summary>
public class DemandResponseListDto
{
    public int Id { get; set; }
    public int DemandId { get; set; }
    public string? DemandTitle { get; set; }
    public string? DemandSlug { get; set; }
    public string? DemandCategoryName { get; set; }
    public int? BuyerVendorId { get; set; }
    public string? BuyerCompanyName { get; set; }
    public int? SupplierVendorId { get; set; }
    public string? SupplierName { get; set; }
    public bool IsExternalSupplier { get; set; }
    public string? ExternalCompanyName { get; set; }
    public string? ExternalEmail { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Currency { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public int? LeadTimeDays { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public int Status { get; set; }
    public string? StatusName { get; set; }
    public DateTime CreatedAt { get; set; }

    // Pazarlik alanlari
    public bool IsNegotiationActive { get; set; }
    public int? CurrentTurnVendorId { get; set; }
    public int CurrentRoundNumber { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsMyTurn { get; set; }
}

/// <summary>
/// Detay görünümü için teklif yanıtı DTO
/// </summary>
public class DemandResponseDetailDto : DemandResponseListDto
{
    public string? ExternalContactName { get; set; }
    public string? ExternalPhone { get; set; }
    public string? ExternalWebsite { get; set; }
    public string? TermsAndConditions { get; set; }
    public DateTime? ViewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public List<DemandResponseAttachmentDto> Attachments { get; set; } = new();
}

/// <summary>
/// Kayıtlı üretici için teklif oluşturma DTO
/// </summary>
public class DemandResponseCreateDto
{
    public int DemandId { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Currency { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public int? LeadTimeDays { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
}

/// <summary>
/// Dış üretici (kayıtsız) için teklif oluşturma DTO
/// </summary>
public class ExternalDemandResponseCreateDto : DemandResponseCreateDto
{
    public string ExternalCompanyName { get; set; } = string.Empty;
    public string? ExternalContactName { get; set; }
    public string ExternalEmail { get; set; } = string.Empty;
    public string? ExternalPhone { get; set; }
    public string? ExternalWebsite { get; set; }
}

/// <summary>
/// Teklif durum güncelleme DTO
/// </summary>
public class DemandResponseStatusUpdateDto
{
    public int Status { get; set; }
    public string? RejectionReason { get; set; }
}

// ============================================
// DEMAND RESPONSE ATTACHMENT DTO'lari
// ============================================

public class DemandResponseAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public long? FileSize { get; set; }
    public string? Title { get; set; }
    public int DisplayOrder { get; set; }
}

// ============================================
// FILTRE VE ARAMA DTO'lari
// ============================================

public class DemandFilterDto
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? CountryId { get; set; }
    public string? City { get; set; }
    public int? Status { get; set; }
    public int? Visibility { get; set; }
    public bool? HasReferenceProduct { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

public class DemandSearchResultDto
{
    public List<DemandListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

// ============================================
// DURUM ENUM'lari (Frontend icin)
// ============================================

public static class DemandVisibility
{
    public const int Public = 1;      // Tum dunya gorebilir
    public const int Platform = 2;    // Sadece kayitli ureticiler
    public const int Invited = 3;     // Sadece davet edilenler
}

public static class DemandStatus
{
    public const int Draft = 1;       // Taslak
    public const int Active = 2;      // Aktif - teklif alinabilir
    public const int Closed = 3;      // Kapandi
    public const int Awarded = 4;     // Birisi kazandi
    public const int Expired = 5;     // Suresi doldu
    public const int Cancelled = 6;   // Iptal edildi
}

public static class DemandResponseStatus
{
    public const int Pending = 1;     // Beklemede
    public const int Viewed = 2;      // Goruldu
    public const int Shortlisted = 3; // Kisa listeye alindi
    public const int Accepted = 4;    // Kabul edildi
    public const int Rejected = 5;    // Reddedildi
}
