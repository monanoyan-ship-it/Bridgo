using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

public class SocialPostLike : BaseEntity
{
    public int SocialPostId { get; set; }
    public int UserId { get; set; }
    public int VendorId { get; set; }

    // Navigation properties
    public virtual SocialPost SocialPost { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Vendor Vendor { get; set; } = null!;
}
