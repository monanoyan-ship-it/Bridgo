using System.ComponentModel.DataAnnotations;

namespace Bridgo.DTOs.SocialFeed;

// ============================================
// POST DTOs
// ============================================

public class SocialPostDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? VendorLogoUrl { get; set; }
    public int AuthorUserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int PostTypeId { get; set; }
    public string PostTypeName { get; set; } = string.Empty;
    public string PostTypeCssClass { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImageUrl { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public bool IsOwnPost { get; set; }
    public bool IsSponsored { get; set; }
    public int? SponsoredPostId { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SocialPostImageDto> Images { get; set; } = new();
}

public class SocialPostCreateDto
{
    [Required]
    public int PostTypeId { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    public int? ProductId { get; set; }

    /// <summary>
    /// true ise hemen yayinla, false ise taslak olarak kaydet
    /// </summary>
    public bool PublishImmediately { get; set; } = true;
}

public class SocialPostUpdateDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    public int? ProductId { get; set; }
}

public class SocialPostImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
}

// ============================================
// FEED DTOs
// ============================================

public class FeedPageDto
{
    public List<SocialPostDto> Posts { get; set; } = new();
    public bool HasMore { get; set; }
    public int? NextCursor { get; set; }
}

// ============================================
// COMMENT DTOs
// ============================================

public class SocialPostCommentDto
{
    public int Id { get; set; }
    public int SocialPostId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
    public int LikeCount { get; set; }
    public bool IsOwnComment { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SocialPostCommentDto> Replies { get; set; } = new();
}

public class SocialPostCommentCreateDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public int? ParentCommentId { get; set; }
}

// ============================================
// FOLLOW DTOs
// ============================================

public class FollowStatsDto
{
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
}

public class VendorSummaryDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsFollowing { get; set; }
}

// ============================================
// HASHTAG DTOs
// ============================================

public class TrendingHashtagDto
{
    public string Tag { get; set; } = string.Empty;
    public int PostCount { get; set; }
}

// ============================================
// SEARCH DTOs
// ============================================

public class FeedSearchDto
{
    public string? Query { get; set; }
    public int? PostTypeId { get; set; }
    public int? VendorId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

// ============================================
// REPORT DTOs
// ============================================

public class SocialPostReportCreateDto
{
    [Required]
    public int ReasonId { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class SocialPostReportDto
{
    public int Id { get; set; }
    public int SocialPostId { get; set; }
    public string? PostTitle { get; set; }
    public string PostContentPreview { get; set; } = string.Empty;
    public int ReporterUserId { get; set; }
    public string ReporterUserName { get; set; } = string.Empty;
    public int ReporterVendorId { get; set; }
    public string ReporterVendorName { get; set; } = string.Empty;
    public int ReasonId { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SocialPostReportReviewDto
{
    [Required]
    public int StatusId { get; set; }

    [MaxLength(2000)]
    public string? AdminNote { get; set; }
}

// ============================================
// SPONSORED POST DTOs
// ============================================

public class SponsoredPostDto
{
    public int Id { get; set; }
    public int SocialPostId { get; set; }
    public string? PostTitle { get; set; }
    public string PostContentPreview { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCssClass { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal SpentAmount { get; set; }
    public int TargetImpressions { get; set; }
    public int CurrentImpressions { get; set; }
    public int CurrentClicks { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SponsoredPostCreateDto
{
    [Required]
    public int SocialPostId { get; set; }

    [Required]
    public decimal BudgetAmount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    [Required]
    public int TargetImpressions { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class SponsoredPostUpdateDto
{
    public decimal BudgetAmount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public int TargetImpressions { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
