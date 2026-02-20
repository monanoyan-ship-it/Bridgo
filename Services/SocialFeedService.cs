using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Notification;
using Bridgo.DTOs.SocialFeed;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public partial class SocialFeedService : ISocialFeedService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public SocialFeedService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    [GeneratedRegex(@"#(\w{2,50})", RegexOptions.Compiled)]
    private static partial Regex HashtagRegex();

    // ============================================
    // POSTS
    // ============================================

    public async Task<FeedPageDto> GetFeedAsync(int vendorId, int currentUserId, int? lastPostId = null, int pageSize = 20, string? sortMode = null)
    {
        var followedVendorIds = await _context.VendorFollows
            .Where(f => f.FollowerVendorId == vendorId)
            .Select(f => f.FollowedVendorId)
            .ToListAsync();

        followedVendorIds.Add(vendorId);

        var query = _context.SocialPosts
            .Include(p => p.Vendor)
            .Include(p => p.Author)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .Where(p => followedVendorIds.Contains(p.VendorId))
            .Where(p => p.StatusId == SocialPostStatuses.Ids.Published || p.VendorId == vendorId)
            .AsQueryable();

        // Algoritma modu: "recommended" ise skor bazli siralama
        if (sortMode == "recommended")
        {
            return await BuildScoredFeedPageAsync(query, currentUserId, vendorId, followedVendorIds, lastPostId, pageSize);
        }

        return await BuildFeedPageAsync(query, currentUserId, vendorId, lastPostId, pageSize);
    }

    public async Task<FeedPageDto> GetDiscoverFeedAsync(int currentUserId, int? lastPostId = null, int pageSize = 20, string? sortMode = null)
    {
        var query = _context.SocialPosts
            .Include(p => p.Vendor)
            .Include(p => p.Author)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .Where(p => p.StatusId == SocialPostStatuses.Ids.Published)
            .AsQueryable();

        var user = await _context.Users.FindAsync(currentUserId);
        var vendorId = user?.VendorId ?? 0;

        if (sortMode == "recommended")
        {
            return await BuildScoredFeedPageAsync(query, currentUserId, vendorId, new List<int>(), lastPostId, pageSize);
        }

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

        // Hashtag'leri kaydet
        await SaveHashtagsAsync(post.Id, post.Content);

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

        // Hashtag'leri guncelle
        await SaveHashtagsAsync(post.Id, post.Content);

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
            existingLike.IsDeleted = true;
            existingLike.DeletedAt = DateTime.UtcNow;
            post.LikeCount = Math.Max(0, post.LikeCount - 1);
        }
        else
        {
            _context.SocialPostLikes.Add(new SocialPostLike
            {
                SocialPostId = postId,
                UserId = userId,
                VendorId = vendorId
            });
            post.LikeCount++;

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
            existingFollow.IsDeleted = true;
            existingFollow.DeletedAt = DateTime.UtcNow;
        }
        else
        {
            _context.VendorFollows.Add(new VendorFollow
            {
                FollowerVendorId = followerVendorId,
                FollowedVendorId = targetVendorId,
                FollowedByUserId = userId
            });

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
    // HASHTAGS
    // ============================================

    public async Task<List<TrendingHashtagDto>> GetTrendingHashtagsAsync(int count = 10, int hoursWindow = 24)
    {
        var since = DateTime.UtcNow.AddHours(-hoursWindow);

        return await _context.SocialPostHashtags
            .Where(h => h.CreatedAt >= since)
            .GroupBy(h => h.Tag)
            .Select(g => new TrendingHashtagDto
            {
                Tag = g.Key,
                PostCount = g.Count()
            })
            .OrderByDescending(t => t.PostCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<FeedPageDto> GetPostsByHashtagAsync(string tag, int currentUserId, int? lastPostId = null, int pageSize = 20)
    {
        tag = tag.TrimStart('#').ToLowerInvariant();

        var user = await _context.Users.FindAsync(currentUserId);
        var vendorId = user?.VendorId ?? 0;

        var postIds = _context.SocialPostHashtags
            .Where(h => h.Tag == tag)
            .Select(h => h.SocialPostId);

        var query = _context.SocialPosts
            .Include(p => p.Vendor)
            .Include(p => p.Author)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .Where(p => postIds.Contains(p.Id))
            .Where(p => p.StatusId == SocialPostStatuses.Ids.Published)
            .AsQueryable();

        return await BuildFeedPageAsync(query, currentUserId, vendorId, lastPostId, pageSize);
    }

    // ============================================
    // SEARCH
    // ============================================

    public async Task<FeedPageDto> SearchPostsAsync(FeedSearchDto dto, int currentUserId)
    {
        var user = await _context.Users.FindAsync(currentUserId);
        var vendorId = user?.VendorId ?? 0;

        var query = _context.SocialPosts
            .Include(p => p.Vendor)
            .Include(p => p.Author)
            .Include(p => p.Images)
            .Include(p => p.Product)
            .Where(p => p.StatusId == SocialPostStatuses.Ids.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Query))
        {
            var q = dto.Query.Trim().ToLower();
            query = query.Where(p => p.Content.ToLower().Contains(q) ||
                                     (p.Title != null && p.Title.ToLower().Contains(q)));
        }

        if (dto.PostTypeId.HasValue)
            query = query.Where(p => p.PostTypeId == dto.PostTypeId.Value);

        if (dto.VendorId.HasValue)
            query = query.Where(p => p.VendorId == dto.VendorId.Value);

        if (dto.DateFrom.HasValue)
            query = query.Where(p => p.PublishedAt >= dto.DateFrom.Value);

        if (dto.DateTo.HasValue)
            query = query.Where(p => p.PublishedAt <= dto.DateTo.Value);

        var posts = await query
            .OrderByDescending(p => p.Id)
            .Take(21)
            .ToListAsync();

        var hasMore = posts.Count > 20;
        if (hasMore)
            posts = posts.Take(20).ToList();

        var postDtos = new List<SocialPostDto>();
        foreach (var post in posts)
        {
            postDtos.Add(await MapToPostDtoAsync(post, currentUserId, vendorId));
        }

        return new FeedPageDto
        {
            Posts = postDtos,
            HasMore = hasMore,
            NextCursor = posts.Any() ? posts.Last().Id : null
        };
    }

    // ============================================
    // REPORTS
    // ============================================

    public async Task<ServiceResult> ReportPostAsync(int postId, SocialPostReportCreateDto dto, int userId, int vendorId)
    {
        var post = await _context.SocialPosts.FindAsync(postId);
        if (post == null)
            return ServiceResult.Fail("Post bulunamadi");

        // Ayni kullanici ayni postu tekrar sikayet etmesin
        var existingReport = await _context.SocialPostReports
            .AnyAsync(r => r.SocialPostId == postId && r.ReporterUserId == userId);
        if (existingReport)
            return ServiceResult.Fail("Bu paylasimi zaten sikayet ettiniz");

        var report = new SocialPostReport
        {
            SocialPostId = postId,
            ReporterUserId = userId,
            ReporterVendorId = vendorId,
            ReasonId = dto.ReasonId,
            Description = dto.Description,
            StatusId = SocialPostReportStatuses.Ids.Pending
        };

        _context.SocialPostReports.Add(report);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<List<SocialPostReportDto>> GetPendingReportsAsync(int pageSize = 20, int? lastId = null)
    {
        var query = _context.SocialPostReports
            .Include(r => r.SocialPost)
            .Include(r => r.ReporterUser)
            .Include(r => r.ReporterVendor)
            .AsQueryable();

        if (lastId.HasValue)
            query = query.Where(r => r.Id < lastId.Value);

        var reports = await query
            .OrderByDescending(r => r.Id)
            .Take(pageSize)
            .ToListAsync();

        return reports.Select(r =>
        {
            var reason = SocialPostReportReasons.GetById(r.ReasonId);
            var status = SocialPostReportStatuses.GetById(r.StatusId);

            return new SocialPostReportDto
            {
                Id = r.Id,
                SocialPostId = r.SocialPostId,
                PostTitle = r.SocialPost?.Title,
                PostContentPreview = r.SocialPost?.Content?.Length > 100
                    ? r.SocialPost.Content[..100] + "..."
                    : r.SocialPost?.Content ?? "",
                ReporterUserId = r.ReporterUserId,
                ReporterUserName = r.ReporterUser != null ? $"{r.ReporterUser.FirstName} {r.ReporterUser.LastName}".Trim() : "",
                ReporterVendorId = r.ReporterVendorId,
                ReporterVendorName = r.ReporterVendor?.CompanyName ?? "",
                ReasonId = r.ReasonId,
                ReasonName = reason?.SystemName ?? "",
                Description = r.Description,
                StatusId = r.StatusId,
                StatusName = status?.SystemName ?? "",
                AdminNote = r.AdminNote,
                ReviewedByUserId = r.ReviewedByUserId,
                ReviewedAt = r.ReviewedAt,
                CreatedAt = r.CreatedAt
            };
        }).ToList();
    }

    public async Task<ServiceResult> ReviewReportAsync(int reportId, SocialPostReportReviewDto dto, int adminUserId)
    {
        var report = await _context.SocialPostReports.FindAsync(reportId);
        if (report == null)
            return ServiceResult.Fail("Sikayet bulunamadi");

        report.StatusId = dto.StatusId;
        report.AdminNote = dto.AdminNote;
        report.ReviewedByUserId = adminUserId;
        report.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ============================================
    // PRIVATE HELPERS
    // ============================================

    private static List<string> ExtractHashtags(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return new();

        return HashtagRegex()
            .Matches(content)
            .Select(m => m.Groups[1].Value.ToLowerInvariant())
            .Distinct()
            .ToList();
    }

    private async Task SaveHashtagsAsync(int postId, string content)
    {
        // Mevcut hashtag'leri sil
        var existing = await _context.SocialPostHashtags
            .Where(h => h.SocialPostId == postId)
            .ToListAsync();

        if (existing.Any())
            _context.SocialPostHashtags.RemoveRange(existing);

        // Yeni hashtag'leri ekle
        var tags = ExtractHashtags(content);
        foreach (var tag in tags)
        {
            _context.SocialPostHashtags.Add(new SocialPostHashtag
            {
                SocialPostId = postId,
                Tag = tag
            });
        }

        if (tags.Any())
            await _context.SaveChangesAsync();
    }

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

    /// <summary>
    /// Feed algoritmasi ile skorlanmis siralama
    /// </summary>
    private async Task<FeedPageDto> BuildScoredFeedPageAsync(
        IQueryable<SocialPost> query,
        int currentUserId,
        int currentVendorId,
        List<int> followedVendorIds,
        int? lastPostId,
        int pageSize)
    {
        // Son 14 gunluk postlari cek
        var since = DateTime.UtcNow.AddDays(-14);
        query = query.Where(p => p.CreatedAt >= since);

        if (lastPostId.HasValue)
            query = query.Where(p => p.Id < lastPostId.Value);

        // Yeterli post cek (skorlama icin fazladan)
        var posts = await query
            .OrderByDescending(p => p.Id)
            .Take(pageSize * 3)
            .ToListAsync();

        // Mutual follow ID'lerini bul
        var mutualFollowIds = new HashSet<int>();
        if (followedVendorIds.Any())
        {
            var mutuals = await _context.VendorFollows
                .Where(f => f.FollowedVendorId == currentVendorId && followedVendorIds.Contains(f.FollowerVendorId))
                .Select(f => f.FollowerVendorId)
                .ToListAsync();
            mutualFollowIds = new HashSet<int>(mutuals);
        }

        var followedSet = new HashSet<int>(followedVendorIds);

        // Skorla ve sirala
        var scoredPosts = posts.Select(p =>
        {
            var hoursAge = (DateTime.UtcNow - p.CreatedAt).TotalHours;
            var freshness = 100.0 * Math.Exp(-0.05 * hoursAge);
            var engagement = Math.Min(50, p.LikeCount + p.CommentCount * 2);

            double relationship = 0;
            if (p.VendorId == currentVendorId)
                relationship = 20;
            else if (mutualFollowIds.Contains(p.VendorId))
                relationship = 30;
            else if (followedSet.Contains(p.VendorId))
                relationship = 10;

            var viewPenalty = Math.Min(10, p.ViewCount * 0.1);
            var score = freshness + engagement + relationship - viewPenalty;

            return new { Post = p, Score = score };
        })
        .OrderByDescending(x => x.Score)
        .Take(pageSize + 1)
        .ToList();

        var hasMore = scoredPosts.Count > pageSize;
        if (hasMore)
            scoredPosts = scoredPosts.Take(pageSize).ToList();

        var postDtos = new List<SocialPostDto>();
        foreach (var item in scoredPosts)
        {
            postDtos.Add(await MapToPostDtoAsync(item.Post, currentUserId, currentVendorId));
        }

        return new FeedPageDto
        {
            Posts = postDtos,
            HasMore = hasMore,
            NextCursor = scoredPosts.Any() ? scoredPosts.Last().Post.Id : null
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
