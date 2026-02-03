using Bridgo.DTOs.ProductReview;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Urun degerlendirme servisi
/// </summary>
public interface IProductReviewService
{
    // Buyer
    Task<List<ProductReviewListDto>> GetMyReviewsAsync(int buyerVendorId);
    Task<ProductReviewListDto> CreateAsync(ProductReviewCreateDto dto, int buyerVendorId, int reviewerUserId);
    Task<bool> UpdateAsync(int id, ProductReviewUpdateDto dto, int buyerVendorId);
    Task<bool> DeleteAsync(int id, int buyerVendorId);
    Task<bool> CanReviewProductAsync(int productId, int buyerVendorId);

    // Seller
    Task<List<ProductReviewListDto>> GetIncomingReviewsAsync(int sellerVendorId);
    Task<ProductReviewStatsDto> GetStatsAsync(int sellerVendorId);
    Task<bool> RespondToReviewAsync(int id, SellerReviewResponseDto dto, int sellerVendorId);
}
