using Bridgo.DTOs.ProductInquiry;

namespace Bridgo.Services.Interfaces;

public interface IProductInquiryService
{
    // ============================================
    // BUYER (ALICI) OPERATIONS
    // ============================================

    /// <summary>
    /// Alicinin isteklerini listele
    /// </summary>
    Task<ProductInquirySearchResultDto> GetBuyerInquiriesAsync(int buyerVendorId, ProductInquiryFilterDto filter);

    /// <summary>
    /// Istek detayi getir (alici icin)
    /// </summary>
    Task<ProductInquiryDetailDto?> GetInquiryForBuyerAsync(int inquiryId, int buyerVendorId);

    /// <summary>
    /// Yeni istek olustur
    /// </summary>
    Task<ProductInquiryDetailDto> CreateInquiryAsync(int buyerVendorId, ProductInquiryCreateDto dto, string? createdBy);

    /// <summary>
    /// Istegi iptal et
    /// </summary>
    Task<bool> CancelInquiryAsync(int inquiryId, int buyerVendorId, string? updatedBy);

    /// <summary>
    /// Teklifi kabul et
    /// </summary>
    Task<bool> AcceptResponseAsync(int responseId, int buyerVendorId, string? updatedBy);

    /// <summary>
    /// Teklifi reddet
    /// </summary>
    Task<bool> RejectResponseAsync(int responseId, int buyerVendorId, string? notes, string? updatedBy);

    /// <summary>
    /// Alici istatistikleri
    /// </summary>
    Task<BuyerInquiryStatsDto> GetBuyerStatsAsync(int buyerVendorId);

    // ============================================
    // SELLER (SATICI) OPERATIONS
    // ============================================

    /// <summary>
    /// Saticiya gelen istekleri listele
    /// </summary>
    Task<ProductInquirySearchResultDto> GetSellerInquiriesAsync(int sellerVendorId, ProductInquiryFilterDto filter);

    /// <summary>
    /// Istek detayi getir (satici icin)
    /// </summary>
    Task<ProductInquiryDetailDto?> GetInquiryForSellerAsync(int inquiryId, int sellerVendorId);

    /// <summary>
    /// Istegi okundu olarak isaretle
    /// </summary>
    Task<bool> MarkAsReadAsync(int inquiryId, int sellerVendorId);

    /// <summary>
    /// Teklif/yanit gonder
    /// </summary>
    Task<ProductInquiryResponseDto> CreateResponseAsync(int sellerVendorId, ProductInquiryResponseCreateDto dto, string? createdBy);

    /// <summary>
    /// Satici istatistikleri
    /// </summary>
    Task<SellerInquiryStatsDto> GetSellerStatsAsync(int sellerVendorId);

    // ============================================
    // COMMON OPERATIONS
    // ============================================

    /// <summary>
    /// Okunmamis istek sayisi (satici icin)
    /// </summary>
    Task<int> GetUnreadCountAsync(int sellerVendorId);
}
