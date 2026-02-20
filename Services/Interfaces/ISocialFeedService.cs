using Bridgo.DTOs.SocialFeed;

namespace Bridgo.Services.Interfaces;

public interface ISocialFeedService
{
    // Posts
    Task<FeedPageDto> GetFeedAsync(int vendorId, int currentUserId, int? lastPostId = null, int pageSize = 20, string? sortMode = null);
    Task<FeedPageDto> GetDiscoverFeedAsync(int currentUserId, int? lastPostId = null, int pageSize = 20, string? sortMode = null);
    Task<FeedPageDto> GetVendorPostsAsync(int vendorId, int currentUserId, int? lastPostId = null, int pageSize = 20);
    Task<SocialPostDto?> GetPostByIdAsync(int postId, int? currentUserId = null);
    Task<ServiceResult<int>> CreatePostAsync(SocialPostCreateDto dto, int vendorId, int userId);
    Task<ServiceResult> UpdatePostAsync(int postId, SocialPostUpdateDto dto, int vendorId);
    Task<ServiceResult> DeletePostAsync(int postId, int vendorId);
    Task<ServiceResult> PublishPostAsync(int postId, int vendorId);

    // Likes
    Task<ServiceResult> ToggleLikeAsync(int postId, int userId, int vendorId);

    // Comments
    Task<List<SocialPostCommentDto>> GetCommentsAsync(int postId, int currentUserId, int? lastCommentId = null, int pageSize = 20);
    Task<ServiceResult<int>> AddCommentAsync(int postId, SocialPostCommentCreateDto dto, int userId, int vendorId);
    Task<ServiceResult> DeleteCommentAsync(int commentId, int userId, int vendorId);

    // Follows
    Task<ServiceResult> ToggleFollowAsync(int targetVendorId, int followerVendorId, int userId);
    Task<bool> IsFollowingAsync(int followerVendorId, int followedVendorId);
    Task<FollowStatsDto> GetFollowStatsAsync(int vendorId);
    Task<List<VendorSummaryDto>> GetFollowersAsync(int vendorId, int currentVendorId, int pageSize = 20, int? lastId = null);
    Task<List<VendorSummaryDto>> GetFollowingAsync(int vendorId, int currentVendorId, int pageSize = 20, int? lastId = null);

    // Hashtags
    Task<List<TrendingHashtagDto>> GetTrendingHashtagsAsync(int count = 10, int hoursWindow = 24);
    Task<FeedPageDto> GetPostsByHashtagAsync(string tag, int currentUserId, int? lastPostId = null, int pageSize = 20);

    // Search
    Task<FeedPageDto> SearchPostsAsync(FeedSearchDto dto, int currentUserId);

    // Reports
    Task<ServiceResult> ReportPostAsync(int postId, SocialPostReportCreateDto dto, int userId, int vendorId);
    Task<List<SocialPostReportDto>> GetPendingReportsAsync(int pageSize = 20, int? lastId = null);
    Task<ServiceResult> ReviewReportAsync(int reportId, SocialPostReportReviewDto dto, int adminUserId);
}
