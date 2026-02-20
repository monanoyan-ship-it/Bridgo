using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

public class SocialPostHashtag : BaseEntity
{
    public int SocialPostId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Tag { get; set; } = string.Empty;

    // Navigation properties
    public virtual SocialPost SocialPost { get; set; } = null!;
}
