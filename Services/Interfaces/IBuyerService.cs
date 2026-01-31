using Bridgo.DTOs.Buyer;

namespace Bridgo.Services.Interfaces;

public interface IBuyerService
{
    // ============================================
    // TEDARİKÇİ KEŞFİ
    // ============================================
    Task<SupplierSearchResultDto> SearchSuppliersAsync(int buyerVendorId, SupplierFilterDto filter);
    Task<SupplierDetailDto?> GetSupplierDetailAsync(int buyerVendorId, int vendorId);
    Task<List<SupplierListDto>> GetRecommendedSuppliersAsync(int buyerVendorId, int count = 10);

    // ============================================
    // SİPARİŞ GEÇMİŞİNDEN TEDARİKÇİLER
    // ============================================
    Task<List<OrderSupplierDto>> GetOrderSuppliersAsync(int buyerVendorId);

    // ============================================
    // FAVORİ TEDARİKÇİLER
    // ============================================
    Task<List<FavoriteVendorDto>> GetFavoriteVendorsAsync(int buyerVendorId);
    Task<FavoriteVendorDto?> GetFavoriteVendorAsync(int buyerVendorId, int favoriteId);
    Task<ServiceResult<FavoriteVendorDto>> AddFavoriteVendorAsync(int buyerVendorId, int userId, FavoriteVendorCreateDto dto);
    Task<ServiceResult> UpdateFavoriteVendorAsync(int buyerVendorId, int favoriteId, FavoriteVendorUpdateDto dto);
    Task<ServiceResult> RemoveFavoriteVendorAsync(int buyerVendorId, int favoriteId);
    Task<bool> IsFavoriteAsync(int buyerVendorId, int sellerVendorId);

    // ============================================
    // TEDARİKÇİ DEĞERLENDİRMELERİ
    // ============================================
    Task<List<VendorReviewDto>> GetVendorReviewsAsync(int vendorId, int page = 1, int pageSize = 10);
    Task<SupplierRatingStatsDto> GetVendorRatingStatsAsync(int vendorId);
    Task<VendorReviewDto?> GetReviewByOrderAsync(int orderId);
    Task<ServiceResult<VendorReviewDto>> CreateReviewAsync(int reviewerVendorId, int userId, VendorReviewCreateDto dto);
    Task<ServiceResult> RespondToReviewAsync(int reviewedVendorId, int reviewId, VendorReviewResponseDto dto);
    Task<List<VendorReviewDto>> GetMyReviewsAsync(int buyerVendorId);

    // ============================================
    // ALICI İSTATİSTİKLERİ
    // ============================================
    Task<BuyerStatsDto> GetBuyerStatsAsync(int buyerVendorId);

    // ============================================
    // TEKLİF KARŞILAŞTIRMA
    // ============================================
    Task<List<DemandWithProposalsDto>> GetDemandsWithProposalsAsync(int buyerVendorId);
    Task<List<InquiryWithProposalsDto>> GetInquiriesWithProposalsAsync(int buyerVendorId);
    Task<ProposalCompareStatsDto> GetProposalCompareStatsAsync(int buyerVendorId);
    Task<ServiceResult> UpdateProposalStatusAsync(int buyerVendorId, int proposalId, ProposalActionDto action);
    Task<ServiceResult> UpdateInquiryProposalStatusAsync(int buyerVendorId, int proposalId, ProposalActionDto action);
}
