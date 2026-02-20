namespace Bridgo.Models.Entities;

public class AuctionWatcher : BaseEntity
{
    public int AuctionId { get; set; }

    public int WatcherVendorId { get; set; }

    // Navigation properties
    public virtual Auction Auction { get; set; } = null!;
    public virtual Vendor WatcherVendor { get; set; } = null!;
}
