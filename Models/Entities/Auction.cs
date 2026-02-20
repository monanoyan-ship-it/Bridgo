using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

public class Auction : BaseEntity
{
    public int VendorId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    public int? ProductId { get; set; }

    public int? CategoryId { get; set; }

    public decimal StartingPrice { get; set; }

    public decimal? ReservePrice { get; set; }

    public decimal CurrentHighestBid { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public int? AutoExtendMinutes { get; set; }

    /// <summary>
    /// AuctionStatuses (TypeDefinitions.cs)
    /// </summary>
    public int AuctionStatusId { get; set; }

    public decimal? BuyNowPrice { get; set; }

    public int ViewCount { get; set; }

    public int BidCount { get; set; }

    public int WatchlistCount { get; set; }

    [MaxLength(200)]
    public string? Slug { get; set; }

    // Navigation properties
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual Product? Product { get; set; }
    public virtual ProductCategory? Category { get; set; }
    public virtual ICollection<AuctionBid> Bids { get; set; } = new List<AuctionBid>();
    public virtual ICollection<AuctionWatcher> Watchers { get; set; } = new List<AuctionWatcher>();
}
