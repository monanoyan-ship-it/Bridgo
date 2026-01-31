using Bridgo.Models.Enums;

namespace Bridgo.DTOs.ProductInquiry;

// ============================================
// CREATE/UPDATE DTOs
// ============================================

/// <summary>
/// Yeni urun istegi olusturma
/// </summary>
public class ProductInquiryCreateDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Message { get; set; }
    public string? SpecialRequirements { get; set; }

    // Teslimat
    public int? DeliveryCountryId { get; set; }
    public string? DeliveryCity { get; set; }
    public string? DeliveryAddress { get; set; }
    public DateTime? DesiredDeliveryDate { get; set; }

    // Iletisim (opsiyonel)
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}

/// <summary>
/// Satici teklif/yanit olusturma
/// </summary>
public class ProductInquiryResponseCreateDto
{
    public int InquiryId { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public int? OfferedQuantity { get; set; }
    public string? OfferedUnit { get; set; }
    public int? LeadTimeDays { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
}

/// <summary>
/// Yanit durumu guncelleme (alici kabul/red)
/// </summary>
public class ProductInquiryResponseStatusDto
{
    public int Status { get; set; }
    public string? Notes { get; set; }
}

// ============================================
// LIST DTOs
// ============================================

/// <summary>
/// Istek listesi icin
/// </summary>
public class ProductInquiryListDto
{
    public int Id { get; set; }

    // Urun bilgisi
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductImageUrl { get; set; }

    // Alici/Satici
    public int BuyerVendorId { get; set; }
    public string? BuyerCompanyName { get; set; }
    public int SellerVendorId { get; set; }
    public string? SellerCompanyName { get; set; }

    // Istek detaylari
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Message { get; set; }

    // Durum
    public int Status { get; set; }
    public string? StatusText { get; set; }
    public bool IsReadBySeller { get; set; }

    // Yanit bilgisi
    public int ResponseCount { get; set; }
    public bool HasResponse { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Istek detayi
/// </summary>
public class ProductInquiryDetailDto
{
    public int Id { get; set; }

    // Urun bilgisi
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductImageUrl { get; set; }
    public string? ProductDescription { get; set; }
    public decimal? ProductPrice { get; set; }
    public string? ProductCurrency { get; set; }

    // Alici
    public int BuyerVendorId { get; set; }
    public string? BuyerCompanyName { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerPhone { get; set; }

    // Satici
    public int SellerVendorId { get; set; }
    public string? SellerCompanyName { get; set; }

    // Istek detaylari
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Message { get; set; }
    public string? SpecialRequirements { get; set; }

    // Teslimat
    public int? DeliveryCountryId { get; set; }
    public string? DeliveryCountryName { get; set; }
    public string? DeliveryCity { get; set; }
    public string? DeliveryAddress { get; set; }
    public DateTime? DesiredDeliveryDate { get; set; }

    // Iletisim
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }

    // Durum
    public int Status { get; set; }
    public string? StatusText { get; set; }
    public bool IsReadBySeller { get; set; }
    public DateTime? ReadAt { get; set; }

    // Yanitlar
    public List<ProductInquiryResponseDto> Responses { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Yanit listesi
/// </summary>
public class ProductInquiryResponseDto
{
    public int Id { get; set; }
    public int InquiryId { get; set; }

    // Fiyat
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string Currency { get; set; } = "TRY";

    // Miktar
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
    public string? StatusText { get; set; }
    public bool IsReadByBuyer { get; set; }
    public DateTime? ReadByBuyerAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

// ============================================
// SEARCH/FILTER DTOs
// ============================================

/// <summary>
/// Istek filtresi
/// </summary>
public class ProductInquiryFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public int? Status { get; set; }
    public int? ProductId { get; set; }
    public bool? HasResponse { get; set; }
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Arama sonucu
/// </summary>
public class ProductInquirySearchResultDto
{
    public List<ProductInquiryListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

// ============================================
// STATISTICS DTOs
// ============================================

/// <summary>
/// Alici istatistikleri
/// </summary>
public class BuyerInquiryStatsDto
{
    public int TotalInquiries { get; set; }
    public int PendingInquiries { get; set; }
    public int RespondedInquiries { get; set; }
    public int AcceptedOffers { get; set; }
}

/// <summary>
/// Satici istatistikleri
/// </summary>
public class SellerInquiryStatsDto
{
    public int TotalInquiries { get; set; }
    public int UnreadInquiries { get; set; }
    public int PendingResponse { get; set; }
    public int AcceptedOffers { get; set; }
}
