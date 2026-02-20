using System.ComponentModel.DataAnnotations;
using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

public class SocialPost : BaseEntity
{
    public int VendorId { get; set; }

    public int AuthorUserId { get; set; }

    /// <summary>
    /// SocialPostTypes static class (TypeDefinitions.cs)
    /// </summary>
    public int PostTypeId { get; set; }

    /// <summary>
    /// SocialPostStatuses static class (TypeDefinitions.cs)
    /// </summary>
    public int StatusId { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// ProductShowcase tipi icin iliskili urun
    /// </summary>
    public int? ProductId { get; set; }

    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ViewCount { get; set; }

    public DateTime? PublishedAt { get; set; }

    // Navigation properties
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual ApplicationUser Author { get; set; } = null!;
    public virtual Product? Product { get; set; }
    public virtual ICollection<SocialPostImage> Images { get; set; } = new List<SocialPostImage>();
    public virtual ICollection<SocialPostLike> Likes { get; set; } = new List<SocialPostLike>();
    public virtual ICollection<SocialPostComment> Comments { get; set; } = new List<SocialPostComment>();
    public virtual ICollection<SocialPostHashtag> Hashtags { get; set; } = new List<SocialPostHashtag>();
}
