using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Notification;
using Bridgo.DTOs.SocialFeed;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class SocialFeedService : ISocialFeedService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public SocialFeedService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    // ============================================
    // POSTS
    // ============================================

    public async Task<FeedPageDto> GetFeedAsync(int vendorId, int currentUserId, int? lastPostId = null, int pageSize = 20)
    {
        // My Feed: own posts + followed vendors' published posts
        var followedVendorIds = await _context.VendorFollows
            .Where(f => f.FollowerVendorId == vendorId)
            .Select(f => f.FollowedVendorId)
            .ToListAsync();

        followedVendorIds.Add(vendorId); // Include own posts

        var query = _context.SocialPosts
            .Include(p => p.Vendor)
            .Include(p => p.Author)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .Where(p => followedVendorIds.Contains(p.VendorId))
            .Where(p => p.StatusId == SocialPostStatuses.Ids.Published || (p.VendorId == vendorId))
            .AsQueryable();

        return await BuildFeedPageAsync(query, currentUserId, vendorId, lastPostId, pageSize);
    }

    public async Task<FeedPageDto> GetDiscoverFeedAsync(int currentUserId, int? lastPostId = null, int pageSize = 20)
    {
        var query = _context.SocialPosts
            .Include(p => p.Vendor)
            .Include(p => p.Author)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .Where(p => p.StatusId == SocialPostStatuses.Ids.Published)
            .AsQueryable();

        // currentUserId'nin vendorId'sini bul
        var user = await _context.Users.FindAsync(currentUserId);
        var vendorId = user?.VendorId ?? 0;

        return await BuildFeedPageAsync(query, currentUserId, vendorId, lastPostId, pageSize);
    }

    public async Task<FeedPageDto> GetVendorPostsAsync(int vendorId, int currentUserId, int? lastPostId = null, int pageSize = 20)
    {
        var user = await _context.Users.FindAsync(currentUserId);
        var currentVendorId = user?.VendorId ?? 0;

        var query = _context.SocialPosts
            .Include(p => p.Vendor)
            .Include(p => p.Author)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .Where(p => p.VendorId == vendorId)
            .AsQueryable();

        // Kendi vendor'umuz degilse sadece published postlari goster
        if (vendorId != currentVendorId)
            query = query.Where(p => p.StatusId == SocialPostStatuses.Ids.Published);

        return await BuildFeedPageAsync(query, currentUserId, currentVendorId, lastPostId, pageSize);
    }

    public async Task<SocialPostDto?> GetPostByIdAsync(int postId, int? currentUserId = null)
    {
        var post = await _context.SocialPosts
            .Include(p => p.Vendor)
            .Include(p => p.Author)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null) return null;

        var currentVendorId = 0;
        if (currentUserId.HasValue)
        {
            var user = await _context.Users.FindAsync(currentUserId.Value);
            currentVendorId = user?.VendorId ?? 0;
        }

        return await MapToPostDtoAsync(post, currentUserId ?? 0, currentVendorId);
    }

    public async Task<ServiceResult<int>> CreatePostAsync(SocialPostCreateDto dto, int vendorId, int userId)
    {
        var post = new SocialPost
        {
            VendorId = vendorId,
            AuthorUserId = userId,
            PostTypeId = dto.PostTypeId,
            StatusId = dto.PublishImmediately ? SocialPostStatuses.Ids.Published : SocialPostStatuses.Ids.Draft,
            Title = dto.Title,
            Content = dto.Content,
            ProductId = dto.ProductId,
            PublishedAt = dto.PublishImmediately ? DateTime.UtcNow : null
        };

        _context.SocialPosts.Add(post);
        await _context.SaveChangesAsync();

        // Yayinlandiysa takipcilere bildirim gonder
        if (dto.PublishImmediately)
            await NotifyFollowersAsync(post);

        return ServiceResult<int>.Ok(post.Id);
    }

    public async Task<ServiceResult> UpdatePostAsync(int postId, SocialPostUpdateDto dto, int vendorId)
    {
        var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == postId && p.VendorId == vendorId);
        if (post == null)
            return ServiceResult.Fail("Post bulunamadi");

        post.Title = dto.Title;
        post.Content = dto.Content;
        post.ProductId = dto.ProductId;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeletePostAsync(int postId, int vendorId)
    {
        var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == postId && p.VendorId == vendorId);
        if (post == null)
            return ServiceResult.Fail("Post bulunamadi");

        post.IsDeleted = true;
        post.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> PublishPostAsync(int postId, int vendorId)
    {
        var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == postId && p.VendorId == vendorId);
        if (post == null)
            return ServiceResult.Fail("Post bulunamadi");

        if (post.StatusId == SocialPostStatuses.Ids.Published)
            return ServiceResult.Fail("Post zaten yayinda");

        post.StatusId = SocialPostStatuses.Ids.Published;
        post.PublishedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await NotifyFollowersAsync(post);

        return ServiceResult.Ok();
    }

    // ============================================
    // LIKES
    // ============================================

    public async Task<ServiceResult> ToggleLikeAsync(int postId, int userId, int vendorId)
    {
        var post = await _context.SocialPosts.FindAsync(postId);
        if (post == null)
            return ServiceResult.Fail("Post bulunamadi");

        var existingLike = await _context.SocialPostLikes
            .FirstOrDefaultAsync(l => l.SocialPostId == postId && l.UserId == userId);

        if (existingLike != null)
        {
            // Unlike - soft delete
            existingLike.IsDeleted = true;
            existingLike.DeletedAt = DateTime.UtcNow;
            post.LikeCount = Math.Max(0, post.LikeCount - 1);
        }
        else
        {
            // Like
            _context.SocialPostLikes.Add(new SocialPostLike
            {
                SocialPostId = postId,
                UserId = userId,
                VendorId = vendorId
            });
            post.LikeCount++;

            // Post sahibine bildirim (kendi postunu begenmediyse)
            if (post.VendorId != vendorId)
            {
                var vendor = await _context.Vendors.FindAsync(vendorId);
                await _notificationService.CreateAsync(new NotificationCreateDto
                {
                    VendorId = post.VendorId,
                    UserId = post.AuthorUserId,
                    Type = NotificationType.SocialPostLiked,
                    Title = "Paylasim begeni aldi",
                    Message = $"{vendor?.CompanyName ?? "Bir firma"} paylasiminizi begendi",
                    EntityType = "SocialPost",
                    EntityId = postId,
                    ActionUrl = "/Feed",
                    Icon = "bi-heart-fill"
                });
            }
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ============================================
    // COMMENTS
    // ============================================

    public async Task<List<SocialPostCommentDto>> GetCommentsAsync(int postId, int currentUserId, int? lastCommentId = null, int pageSize = 20)
    {
        var query = _context.SocialPostComments
            .Include(c => c.User)
            .Include(c => c.Vendor)
            .Where(c => c.SocialPostId == postId && c.ParentCommentId == null)
            .AsQueryable();

        if (lastCommentId.HasValue)
            query = query.Where(c => c.Id > lastCommentId.Value);

        var comments = await query
            .OrderBy(c => c.Id)
            .Take(pageSize)
            .ToListAsync();

        // Load replies
        var commentIds = comments.Select(c => c.Id).ToList();
        var replies = await _context.SocialPostComments
            .Include(c => c.User)
            .Include(c => c.Vendor)
            .Where(c => c.ParentCommentId != null && commentIds.Contains(c.ParentCommentId.Value))
            .OrderBy(c => c.Id)
            .ToListAsync();

        return comments.Select(c => MapToCommentDto(c, replies.Where(r => r.ParentCommentId == c.Id).ToList(), currentUserId)).ToList();
    }

    public async Task<ServiceResult<int>> AddCommentAsync(int postId, SocialPostCommentCreateDto dto, int userId, int vendorId)
    {
        var post = await _context.SocialPosts.FindAsync(postId);
        if (post == null)
            return ServiceResult<int>.Fail("Post bulunamadi");

        var comment = new SocialPostComment
        {
            SocialPostId = postId,
            UserId = userId,
            VendorId = vendorId,
            Content = dto.Content,
            ParentCommentId = dto.ParentCommentId
        };

        _context.SocialPostComments.Add(comment);
        post.CommentCount++;
        await _context.SaveChangesAsync();

        // Post sahibine bildirim (kendi postuna yorum yapmadiysa)
        if (post.VendorId != vendorId)
        {
            var vendor = await _context.Vendors.FindAsync(vendorId);
            await _notificationService.CreateAsync(new NotificationCreateDto
            {
                VendorId = post.VendorId,
                UserId = post.AuthorUserId,
                Type = NotificationType.SocialPostCommented,
                Title = "Paylasima yorum yapildi",
                Message = $"{vendor?.CompanyName ?? "Bir firma"} paylasiminiza yorum yapti",
                EntityType = "SocialPost",
                EntityId = postId,
                ActionUrl = "/Feed",
                Icon = "bi-chat-dots"
            });
        }

        return ServiceResult<int>.Ok(comment.Id);
    }

    public async Task<ServiceResult> DeleteCommentAsync(int commentId, int userId, int vendorId)
    {
        var comment = await _context.SocialPostComments
            .Include(c => c.SocialPost)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            return ServiceResult.Fail("Yorum bulunamadi");

        // Yorum sahibi veya post sahibi silebilir
        if (comment.UserId != userId && comment.SocialPost.VendorId != vendorId)
            return ServiceResult.Fail("Bu yorumu silme yetkiniz yok");

        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;
        comment.SocialPost.CommentCount = Math.Max(0, comment.SocialPost.CommentCount - 1);
        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ============================================
    // FOLLOWS
    // ============================================

    public async Task<ServiceResult> ToggleFollowAsync(int targetVendorId, int followerVendorId, int userId)
    {
        if (targetVendorId == followerVendorId)
            return ServiceResult.Fail("Kendi firmanizi takip edemezsiniz");

        var existingFollow = await _context.VendorFollows
            .FirstOrDefaultAsync(f => f.FollowerVendorId == followerVendorId && f.FollowedVendorId == targetVendorId);

        if (existingFollow != null)
        {
            // Unfollow - soft delete
            existingFollow.IsDeleted = true;
            existingFollow.DeletedAt = DateTime.UtcNow;
        }
        else
        {
            // Follow
            _context.VendorFollows.Add(new VendorFollow
            {
                FollowerVendorId = followerVendorId,
                FollowedVendorId = targetVendorId,
                FollowedByUserId = userId
            });

            // Takip edilen firmaya bildirim
            var followerVendor = await _context.Vendors.FindAsync(followerVendorId);
            await _notificationService.CreateAsync(new NotificationCreateDto
            {
                VendorId = targetVendorId,
                Type = NotificationType.NewFollower,
                Title = "Yeni takipci",
                Message = $"{followerVendor?.CompanyName ?? "Bir firma"} sizi takip etmeye basladi",
                EntityType = "Vendor",
                EntityId = followerVendorId,
                ActionUrl = "/Feed",
                Icon = "bi-person-plus"
            });
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<bool> IsFollowingAsync(int followerVendorId, int followedVendorId)
    {
        return await _context.VendorFollows
            .AnyAsync(f => f.FollowerVendorId == followerVendorId && f.FollowedVendorId == followedVendorId);
    }

    public async Task<FollowStatsDto> GetFollowStatsAsync(int vendorId)
    {
        return new FollowStatsDto
        {
            FollowerCount = await _context.VendorFollows.CountAsync(f => f.FollowedVendorId == vendorId),
            FollowingCount = await _context.VendorFollows.CountAsync(f => f.FollowerVendorId == vendorId)
        };
    }

    public async Task<List<VendorSummaryDto>> GetFollowersAsync(int vendorId, int currentVendorId, int pageSize = 20, int? lastId = null)
    {
        var query = _context.VendorFollows
            .Include(f => f.FollowerVendor)
            .Where(f => f.FollowedVendorId == vendorId)
            .AsQueryable();

        if (lastId.HasValue)
            query = query.Where(f => f.Id < lastId.Value);

        var follows = await query
            .OrderByDescending(f => f.Id)
            .Take(pageSize)
            .ToListAsync();

        var followerVendorIds = follows.Select(f => f.FollowerVendorId).ToList();
        var myFollows = await _context.VendorFollows
            .Where(f => f.FollowerVendorId == currentVendorId && followerVendorIds.Contains(f.FollowedVendorId))
            .Select(f => f.FollowedVendorId)
            .ToListAsync();

        return follows.Select(f => new VendorSummaryDto
        {
            Id = f.FollowerVendorId,
            CompanyName = f.FollowerVendor.CompanyName,
            LogoUrl = f.FollowerVendor.LogoUrl,
            IsFollowing = myFollows.Contains(f.FollowerVendorId)
        }).ToList();
    }

    public async Task<List<VendorSummaryDto>> GetFollowingAsync(int vendorId, int currentVendorId, int pageSize = 20, int? lastId = null)
    {
        var query = _context.VendorFollows
            .Include(f => f.FollowedVendor)
            .Where(f => f.FollowerVendorId == vendorId)
            .AsQueryable();

        if (lastId.HasValue)
            query = query.Where(f => f.Id < lastId.Value);

        var follows = await query
            .OrderByDescending(f => f.Id)
            .Take(pageSize)
            .ToListAsync();

        var followedVendorIds = follows.Select(f => f.FollowedVendorId).ToList();
        var myFollows = await _context.VendorFollows
            .Where(f => f.FollowerVendorId == currentVendorId && followedVendorIds.Contains(f.FollowedVendorId))
            .Select(f => f.FollowedVendorId)
            .ToListAsync();

        return follows.Select(f => new VendorSummaryDto
        {
            Id = f.FollowedVendorId,
            CompanyName = f.FollowedVendor.CompanyName,
            LogoUrl = f.FollowedVendor.LogoUrl,
            IsFollowing = myFollows.Contains(f.FollowedVendorId)
        }).ToList();
    }

    // ============================================
    // PRIVATE HELPERS
    // ============================================

    private async Task<FeedPageDto> BuildFeedPageAsync(IQueryable<SocialPost> query, int currentUserId, int currentVendorId, int? lastPostId, int pageSize)
    {
        if (lastPostId.HasValue)
            query = query.Where(p => p.Id < lastPostId.Value);

        var posts = await query
            .OrderByDescending(p => p.Id)
            .Take(pageSize + 1)
            .ToListAsync();

        var hasMore = posts.Count > pageSize;
        if (hasMore)
            posts = posts.Take(pageSize).ToList();

        var postDtos = new List<SocialPostDto>();
        foreach (var post in posts)
        {
            postDtos.Add(await MapToPostDtoAsync(post, currentUserId, currentVendorId));
        }

        return new FeedPageDto
        {
            Posts = postDtos,
            HasMore = hasMore,
            NextCursor = posts.Any() ? posts.Last().Id : null
        };
    }

    private async Task<SocialPostDto> MapToPostDtoAsync(SocialPost post, int currentUserId, int currentVendorId)
    {
        var isLiked = currentUserId > 0 && await _context.SocialPostLikes
            .AnyAsync(l => l.SocialPostId == post.Id && l.UserId == currentUserId);

        var postType = SocialPostTypes.GetById(post.PostTypeId);
        var status = SocialPostStatuses.GetById(post.StatusId);

        return new SocialPostDto
        {
            Id = post.Id,
            VendorId = post.VendorId,
            VendorName = post.Vendor?.CompanyName ?? "",
            VendorLogoUrl = post.Vendor?.LogoUrl,
            AuthorUserId = post.AuthorUserId,
            AuthorName = post.Author != null ? $"{post.Author.FirstName} {post.Author.LastName}".Trim() : "",
            PostTypeId = post.PostTypeId,
            PostTypeName = postType?.SystemName ?? "",
            PostTypeCssClass = postType?.CssClass ?? "",
            StatusId = post.StatusId,
            StatusName = status?.SystemName ?? "",
            Title = post.Title,
            Content = post.Content,
            ProductId = post.ProductId,
            ProductName = post.Product?.Name,
            ProductImageUrl = post.Product?.Images?.FirstOrDefault(i => i.IsMain)?.Url,
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            ViewCount = post.ViewCount,
            IsLikedByCurrentUser = isLiked,
            IsOwnPost = post.VendorId == currentVendorId,
            PublishedAt = post.PublishedAt,
            CreatedAt = post.CreatedAt,
            Images = post.Images?.OrderBy(i => i.DisplayOrder).Select(i => new SocialPostImageDto
            {
                Id = i.Id,
                Url = i.Url,
                AltText = i.AltText,
                DisplayOrder = i.DisplayOrder
            }).ToList() ?? new()
        };
    }

    private SocialPostCommentDto MapToCommentDto(SocialPostComment comment, List<SocialPostComment> replies, int currentUserId)
    {
        return new SocialPostCommentDto
        {
            Id = comment.Id,
            SocialPostId = comment.SocialPostId,
            UserId = comment.UserId,
            UserName = comment.User != null ? $"{comment.User.FirstName} {comment.User.LastName}".Trim() : "",
            VendorId = comment.VendorId,
            VendorName = comment.Vendor?.CompanyName ?? "",
            Content = comment.Content,
            ParentCommentId = comment.ParentCommentId,
            LikeCount = comment.LikeCount,
            IsOwnComment = comment.UserId == currentUserId,
            CreatedAt = comment.CreatedAt,
            Replies = replies.Select(r => MapToCommentDto(r, new(), currentUserId)).ToList()
        };
    }

    private async Task NotifyFollowersAsync(SocialPost post)
    {
        var followers = await _context.VendorFollows
            .Where(f => f.FollowedVendorId == post.VendorId)
            .Select(f => new { f.FollowerVendorId, f.FollowedByUserId })
            .ToListAsync();

        var vendor = await _context.Vendors.FindAsync(post.VendorId);
        var vendorName = vendor?.CompanyName ?? "Bir firma";

        foreach (var follower in followers)
        {
            await _notificationService.CreateAsync(new NotificationCreateDto
            {
                VendorId = follower.FollowerVendorId,
                Type = NotificationType.NewSocialPost,
                Title = "Yeni paylasim",
                Message = $"{vendorName} yeni bir paylasim yapti",
                EntityType = "SocialPost",
                EntityId = post.Id,
                ActionUrl = "/Feed",
                Icon = "bi-rss"
            });
        }
    }
}
