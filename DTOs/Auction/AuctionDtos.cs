using System.ComponentModel.DataAnnotations;

namespace Bridgo.DTOs.Auction;

// ============================================
// LIST DTO
// ============================================

public class AuctionListDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal CurrentHighestBid { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int AuctionStatusId { get; set; }
    public string AuctionStatusName { get; set; } = string.Empty;
    public string AuctionStatusCssClass { get; set; } = string.Empty;
    public int BidCount { get; set; }
    public int ViewCount { get; set; }
    public int WatchlistCount { get; set; }
    public bool IsWatching { get; set; }
    public bool IsOwnAuction { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ============================================
// DETAIL DTO
// ============================================

public class AuctionDetailDto : AuctionListDto
{
    public string? Description { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal? ReservePrice { get; set; }
    public int? AutoExtendMinutes { get; set; }
    public List<AuctionBidDto> RecentBids { get; set; } = new();
}

// ============================================
// CREATE/UPDATE DTO
// ============================================

public class AuctionCreateUpdateDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    public int? ProductId { get; set; }

    public int? CategoryId { get; set; }

    [Required]
    public decimal StartingPrice { get; set; }

    public decimal? ReservePrice { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    public int? AutoExtendMinutes { get; set; }

    public decimal? BuyNowPrice { get; set; }
}

// ============================================
// BID DTOs
// ============================================

public class AuctionBidDto
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public int BuyerVendorId { get; set; }
    public string BuyerVendorName { get; set; } = string.Empty;
    public decimal BidAmount { get; set; }
    public int BidSequence { get; set; }
    public int BidStatusId { get; set; }
    public string BidStatusName { get; set; } = string.Empty;
    public string BidStatusCssClass { get; set; } = string.Empty;
    public string? BidderMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PlaceBidDto
{
    [Required]
    public decimal BidAmount { get; set; }

    [MaxLength(500)]
    public string? BidderMessage { get; set; }
}

// ============================================
// FILTER DTO
// ============================================

public class AuctionFilterDto
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? StatusId { get; set; }
    public string? SortBy { get; set; } // endingSoon, newest, priceLow, priceHigh, mostBids
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

// ============================================
// PAGED RESULT
// ============================================

public class AuctionPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasMore => Page < TotalPages;
}
