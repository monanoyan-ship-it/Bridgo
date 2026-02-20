using System.ComponentModel.DataAnnotations;
using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

public class SocialPostComment : BaseEntity
{
    public int SocialPostId { get; set; }
    public int UserId { get; set; }
    public int VendorId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Yanit verilen yorum (null ise root comment)
    /// </summary>
    public int? ParentCommentId { get; set; }

    public int LikeCount { get; set; }

    // Navigation properties
    public virtual SocialPost SocialPost { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual SocialPostComment? ParentComment { get; set; }
    public virtual ICollection<SocialPostComment> Replies { get; set; } = new List<SocialPostComment>();
}
