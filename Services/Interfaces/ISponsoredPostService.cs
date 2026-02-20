using Bridgo.DTOs.SocialFeed;

namespace Bridgo.Services.Interfaces;

public interface ISponsoredPostService
{
    Task<ServiceResult<int>> CreateSponsoredPostAsync(SponsoredPostCreateDto dto, int vendorId);
    Task<ServiceResult> UpdateSponsoredPostAsync(int id, SponsoredPostUpdateDto dto, int vendorId);
    Task<ServiceResult> ActivateSponsoredPostAsync(int id, int vendorId);
    Task<ServiceResult> PauseSponsoredPostAsync(int id, int vendorId);
    Task<List<SponsoredPostDto>> GetVendorSponsoredPostsAsync(int vendorId);
    Task<SponsoredPostDto?> GetSponsoredPostAsync(int id, int vendorId);
    Task<SocialPostDto?> GetNextSponsoredPostForFeedAsync(int viewerVendorId, int currentUserId);
    Task RecordImpressionAsync(int sponsoredPostId);
    Task RecordClickAsync(int sponsoredPostId);
}
