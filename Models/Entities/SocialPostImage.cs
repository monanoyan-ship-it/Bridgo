using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

public class SocialPostImage : BaseEntity
{
    public int SocialPostId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AltText { get; set; }

    public int DisplayOrder { get; set; }

    public long? FileSize { get; set; }

    [MaxLength(50)]
    public string? MimeType { get; set; }

    // Navigation properties
    public virtual SocialPost SocialPost { get; set; } = null!;
}
