using Bridgo.DTOs.Auction;

namespace Bridgo.Services.Interfaces;

public interface IAuctionService
{
    // Auctions
    Task<AuctionPagedResult<AuctionListDto>> GetAuctionsAsync(int vendorId, AuctionFilterDto? filter = null);
    Task<AuctionPagedResult<AuctionListDto>> GetPublicAuctionsAsync(AuctionFilterDto filter, int? viewerVendorId = null);
    Task<AuctionDetailDto?> GetAuctionByIdAsync(int id, int? viewerVendorId = null);
    Task<ServiceResult<int>> CreateAuctionAsync(AuctionCreateUpdateDto dto, int vendorId);
    Task<ServiceResult> UpdateAuctionAsync(int id, AuctionCreateUpdateDto dto, int vendorId);
    Task<ServiceResult> CancelAuctionAsync(int id, int vendorId);
    Task<ServiceResult> ActivateAuctionAsync(int id, int vendorId);

    // Bids
    Task<ServiceResult> PlaceBidAsync(int auctionId, PlaceBidDto dto, int buyerVendorId);
    Task<List<AuctionBidDto>> GetBidsAsync(int auctionId);

    // Watchlist
    Task<ServiceResult> ToggleWatchAsync(int auctionId, int watcherVendorId);
    Task<AuctionPagedResult<AuctionListDto>> GetWatchedAuctionsAsync(int watcherVendorId, AuctionFilterDto? filter = null);
}
