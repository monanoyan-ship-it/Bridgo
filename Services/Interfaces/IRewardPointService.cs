using Bridgo.DTOs.Auction;
using Bridgo.DTOs.RewardPoints;

namespace Bridgo.Services.Interfaces;

public interface IRewardPointService
{
    Task<RewardPointSummaryDto> GetSummaryAsync(int vendorId);
    Task<AuctionPagedResult<RewardPointHistoryDto>> GetHistoryAsync(int vendorId, RewardPointFilterDto? filter = null);
    Task<ServiceResult> AddPointsAsync(int vendorId, int points, int actionTypeId, string? description = null, int? orderId = null);
    Task<ServiceResult> SpendPointsAsync(int vendorId, int points, string? description = null);
}
