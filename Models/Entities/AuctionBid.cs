using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

public class AuctionBid : BaseEntity
{
    public int AuctionId { get; set; }

    public int BuyerVendorId { get; set; }

    public decimal BidAmount { get; set; }

    public int BidSequence { get; set; }

    /// <summary>
    /// BidStatuses (TypeDefinitions.cs)
    /// </summary>
    public int BidStatusId { get; set; }

    [MaxLength(500)]
    public string? BidderMessage { get; set; }

    // Navigation properties
    public virtual Auction Auction { get; set; } = null!;
    public virtual Vendor BuyerVendor { get; set; } = null!;
}
