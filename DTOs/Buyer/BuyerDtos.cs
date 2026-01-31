namespace Bridgo.DTOs.Buyer;

// ============================================
// TEDARIKÇI KEŞFI DTO'LARI
// ============================================

/// <summary>
/// Tedarikçi liste görünümü
/// </summary>
public class SupplierListDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }

    // Konum
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public string? City { get; set; }

    // İstatistikler
    public int ProductCount { get; set; }
    public int CompletedOrderCount { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }

    // Kategoriler
    public List<string> Categories { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();

    // Favori mi?
    public bool IsFavorite { get; set; }
    public int? FavoriteId { get; set; }

    // Profil bilgileri
    public string? ShortDescription { get; set; }
    public int YearsInBusiness { get; set; }
    public bool IsVerified { get; set; }

    // Capability'ler
    public List<string> Capabilities { get; set; } = new();
}

/// <summary>
/// Tedarikçi detay görünümü
/// </summary>
public class SupplierDetailDto : SupplierListDto
{
    public string? Description { get; set; }
    public string? AuthorizedPersonName { get; set; }
    public string? AuthorizedPersonTitle { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // Adresler
    public List<SupplierAddressDto> Addresses { get; set; } = new();

    // Ürünler
    public List<SupplierProductDto> FeaturedProducts { get; set; } = new();

    // Son değerlendirmeler
    public List<VendorReviewDto> RecentReviews { get; set; } = new();

    // İstatistikler detay
    public int TotalProductCount { get; set; }
    public int ActiveDemandResponseCount { get; set; }
}

public class SupplierAddressDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? CountryName { get; set; }
    public string? StateName { get; set; }
    public string? City { get; set; }
}

public class SupplierProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public string? MainImageUrl { get; set; }
    public string? CategoryName { get; set; }
}

/// <summary>
/// Tedarikçi arama filtresi
/// </summary>
public class SupplierFilterDto
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? CountryId { get; set; }
    public bool? IsVerified { get; set; }
    public bool? HasProducts { get; set; }
    public decimal? MinRating { get; set; }
    public string? SortBy { get; set; } // "rating", "products", "orders", "name"
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SupplierSearchResultDto
{
    public List<SupplierListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// ============================================
// FAVORİ TEDARİKÇİLER DTO'LARI
// ============================================

public class FavoriteVendorDto
{
    public int Id { get; set; }
    public int SellerVendorId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Notes { get; set; }
    public string? Tags { get; set; }
    public DateTime AddedAt { get; set; }

    // Tedarikçi bilgileri
    public int ProductCount { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsVerified { get; set; }
    public string? CountryName { get; set; }
}

public class FavoriteVendorCreateDto
{
    public int SellerVendorId { get; set; }
    public string? Notes { get; set; }
    public string? Tags { get; set; }
}

public class FavoriteVendorUpdateDto
{
    public string? Notes { get; set; }
    public string? Tags { get; set; }
}

// ============================================
// SİPARİŞ GEÇMİŞİ TEDARİKÇİ DTO
// ============================================

/// <summary>
/// Sipariş geçmişinden tedarikçi bilgisi
/// </summary>
public class OrderSupplierDto
{
    public int VendorId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? CountryName { get; set; }
    public bool IsVerified { get; set; }

    // Sipariş istatistikleri
    public int OrderCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime FirstOrderDate { get; set; }
    public DateTime LastOrderDate { get; set; }

    // Favori durumu
    public bool IsFavorite { get; set; }
    public int? FavoriteId { get; set; }
}

// ============================================
// TEDARİKÇİ DEĞERLENDİRME DTO'LARI
// ============================================

public class VendorReviewDto
{
    public int Id { get; set; }
    public int ReviewerVendorId { get; set; }
    public string ReviewerCompanyName { get; set; } = string.Empty;
    public string? ReviewerLogoUrl { get; set; }
    public int ReviewedVendorId { get; set; }
    public int? OrderId { get; set; }
    public string? OrderNumber { get; set; }

    // Puanlar
    public int OverallRating { get; set; }
    public int? QualityRating { get; set; }
    public int? DeliveryRating { get; set; }
    public int? CommunicationRating { get; set; }
    public int? ValueRating { get; set; }

    // Yorum
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public bool WouldRecommend { get; set; }

    public DateTime ReviewedAt { get; set; }

    // Satıcı yanıtı
    public string? SellerResponse { get; set; }
    public DateTime? SellerRespondedAt { get; set; }
}

public class VendorReviewCreateDto
{
    public int ReviewedVendorId { get; set; }
    public int? OrderId { get; set; }

    public int OverallRating { get; set; }
    public int? QualityRating { get; set; }
    public int? DeliveryRating { get; set; }
    public int? CommunicationRating { get; set; }
    public int? ValueRating { get; set; }

    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public bool WouldRecommend { get; set; } = true;
}

public class VendorReviewResponseDto
{
    public string Response { get; set; } = string.Empty;
}

// ============================================
// İSTATİSTİK DTO'LARI
// ============================================

public class BuyerStatsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int FavoriteSupplierCount { get; set; }
    public int ActiveDemandsCount { get; set; }
    public int ActiveInquiriesCount { get; set; }
    public int PendingResponsesCount { get; set; }
}

public class SupplierRatingStatsDto
{
    public int VendorId { get; set; }
    public decimal AverageOverall { get; set; }
    public decimal AverageQuality { get; set; }
    public decimal AverageDelivery { get; set; }
    public decimal AverageCommunication { get; set; }
    public decimal AverageValue { get; set; }
    public int TotalReviews { get; set; }
    public int RecommendCount { get; set; }
    public decimal RecommendPercentage { get; set; }

    // Rating dağılımı
    public int Rating5Count { get; set; }
    public int Rating4Count { get; set; }
    public int Rating3Count { get; set; }
    public int Rating2Count { get; set; }
    public int Rating1Count { get; set; }
}

// ============================================
// TEKLİF KARŞILAŞTIRMA DTO'LARI
// ============================================

/// <summary>
/// Alıcının talebi/isteği ile gelen teklifler
/// </summary>
public class DemandWithProposalsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? CategoryName { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public List<ProposalDto> Proposals { get; set; } = new();
}

/// <summary>
/// Ürün isteği ile gelen teklifler
/// </summary>
public class InquiryWithProposalsDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int SellerVendorId { get; set; }
    public string SellerCompanyName { get; set; } = string.Empty;
    public int? Quantity { get; set; }
    public string? Message { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<InquiryProposalDto> Proposals { get; set; } = new();
}

/// <summary>
/// Talebe gelen teklif (DemandResponse)
/// </summary>
public class ProposalDto
{
    public int Id { get; set; }
    public int DemandId { get; set; }

    // Tedarikçi bilgileri
    public int? SupplierVendorId { get; set; }
    public string? CompanyName { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsVerified { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }

    // Dış tedarikçi bilgileri
    public string? ExternalCompanyName { get; set; }
    public string? ExternalContactName { get; set; }
    public string? ExternalEmail { get; set; }
    public string? ExternalPhone { get; set; }

    // Teklif detayları
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Currency { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public int? LeadTimeDays { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }

    // Durum
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ViewedAt { get; set; }

    // Favori mi?
    public bool IsFavorite { get; set; }

    // Hesaplanmış değerler (karşılaştırma için)
    public bool IsLowestPrice { get; set; }
    public bool IsFastestDelivery { get; set; }
    public bool IsHighestRated { get; set; }
}

/// <summary>
/// Ürün isteğine gelen teklif (ProductInquiryResponse)
/// </summary>
public class InquiryProposalDto
{
    public int Id { get; set; }
    public int InquiryId { get; set; }

    // Fiyat teklifi
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Currency { get; set; }
    public int? OfferedQuantity { get; set; }
    public string? OfferedUnit { get; set; }

    // Teslim
    public int? LeadTimeDays { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ValidUntil { get; set; }

    // Notlar
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }

    // Durum
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsReadByBuyer { get; set; }
}

/// <summary>
/// Teklif karşılaştırma özet istatistikleri
/// </summary>
public class ProposalCompareStatsDto
{
    public int TotalDemandsWithProposals { get; set; }
    public int TotalInquiriesWithProposals { get; set; }
    public int TotalPendingProposals { get; set; }
    public int TotalShortlistedProposals { get; set; }
    public int TotalAcceptedProposals { get; set; }
}

/// <summary>
/// Teklif kabul/red işlemi
/// </summary>
public class ProposalActionDto
{
    public int ProposalId { get; set; }
    public string Action { get; set; } = string.Empty; // "accept", "reject", "shortlist"
    public string? RejectionReason { get; set; }
}
