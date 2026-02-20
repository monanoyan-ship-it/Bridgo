using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

public class VendorFollow : BaseEntity
{
    public int FollowerVendorId { get; set; }
    public int FollowedVendorId { get; set; }
    public int FollowedByUserId { get; set; }

    // Navigation properties
    public virtual Vendor FollowerVendor { get; set; } = null!;
    public virtual Vendor FollowedVendor { get; set; } = null!;
    public virtual ApplicationUser FollowedByUser { get; set; } = null!;
}
