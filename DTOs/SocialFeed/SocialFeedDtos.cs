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
