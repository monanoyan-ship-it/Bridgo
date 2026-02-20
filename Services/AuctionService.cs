using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Auction;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class AuctionService : IAuctionService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public AuctionService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    // ============================================
    // AUCTIONS
    // ============================================

    public async Task<AuctionPagedResult<AuctionListDto>> GetAuctionsAsync(int vendorId, AuctionFilterDto? filter = null)
    {
        filter ??= new AuctionFilterDto();

        var query = _context.Auctions
            .Include(a => a.Vendor)
            .Include(a => a.Category)
            .Where(a => a.VendorId == vendorId)
            .AsQueryable();

        query = ApplyFilters(query, filter);
        query = ApplySorting(query, filter.SortBy);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new AuctionPagedResult<AuctionListDto>
        {
            Items = items.Select(a => MapToListDto(a, vendorId)).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<AuctionPagedResult<AuctionListDto>> GetPublicAuctionsAsync(AuctionFilterDto filter, int? viewerVendorId = null)
    {
        var query = _context.Auctions
            .Include(a => a.Vendor)
            .Include(a => a.Category)
            .Where(a => a.AuctionStatusId == AuctionStatuses.Ids.Active)
            .AsQueryable();

        query = ApplyFilters(query, filter);
        query = ApplySorting(query, filter.SortBy);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // Check watching status
        var auctionIds = items.Select(a => a.Id).ToList();
        var watchedIds = viewerVendorId.HasValue
            ? await _context.AuctionWatchers
                .Where(w => w.WatcherVendorId == viewerVendorId.Value && auctionIds.Contains(w.AuctionId))
                .Select(w => w.AuctionId)
                .ToListAsync()
            : new List<int>();

        return new AuctionPagedResult<AuctionListDto>
        {
            Items = items.Select(a =>
            {
                var dto = MapToListDto(a, viewerVendorId);
                dto.IsWatching = watchedIds.Contains(a.Id);
                return dto;
            }).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<AuctionDetailDto?> GetAuctionByIdAsync(int id, int? viewerVendorId = null)
    {
        var auction = await _context.Auctions
            .Include(a => a.Vendor)
            .Include(a => a.Category)
            .Include(a => a.Product)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null) return null;

        // Increment view count
        auction.ViewCount++;
        await _context.SaveChangesAsync();

        var recentBids = await _context.AuctionBids
            .Include(b => b.BuyerVendor)
            .Where(b => b.AuctionId == id)
            .OrderByDescending(b => b.BidAmount)
            .Take(10)
            .ToListAsync();

        var isWatching = viewerVendorId.HasValue
            ? await _context.AuctionWatchers.AnyAsync(w => w.AuctionId == id && w.WatcherVendorId == viewerVendorId.Value)
            : false;

        var statusType = AuctionStatuses.GetById(auction.AuctionStatusId);

        return new AuctionDetailDto
        {
            Id = auction.Id,
            VendorId = auction.VendorId,
            VendorName = auction.Vendor.CompanyName,
            Title = auction.Title,
            Slug = auction.Slug,
            Description = auction.Description,
            CategoryId = auction.CategoryId,
            CategoryName = auction.Category?.Name,
            ProductId = auction.ProductId,
            ProductName = auction.Product?.Name,
            StartingPrice = auction.StartingPrice,
            CurrentHighestBid = auction.CurrentHighestBid,
            ReservePrice = auction.VendorId == viewerVendorId ? auction.ReservePrice : null,
            BuyNowPrice = auction.BuyNowPrice,
            Currency = auction.Currency,
            StartAt = auction.StartAt,
            EndAt = auction.EndAt,
            AutoExtendMinutes = auction.AutoExtendMinutes,
            AuctionStatusId = auction.AuctionStatusId,
            AuctionStatusName = statusType?.SystemName ?? "",
            AuctionStatusCssClass = statusType?.CssClass ?? "",
            BidCount = auction.BidCount,
            ViewCount = auction.ViewCount,
            WatchlistCount = auction.WatchlistCount,
            IsWatching = isWatching,
            IsOwnAuction = viewerVendorId.HasValue && auction.VendorId == viewerVendorId.Value,
            CreatedAt = auction.CreatedAt,
            RecentBids = recentBids.Select(MapToBidDto).ToList()
        };
    }

    public async Task<ServiceResult<int>> CreateAuctionAsync(AuctionCreateUpdateDto dto, int vendorId)
    {
        if (dto.StartAt >= dto.EndAt)
            return ServiceResult<int>.Fail("Bitis tarihi baslangic tarihinden sonra olmalidir.");

        if (dto.StartingPrice <= 0)
            return ServiceResult<int>.Fail("Baslangic fiyati sifirdan buyuk olmalidir.");

        if (dto.BuyNowPrice.HasValue && dto.BuyNowPrice.Value <= dto.StartingPrice)
            return ServiceResult<int>.Fail("Hemen al fiyati baslangic fiyatindan buyuk olmalidir.");

        var slug = GenerateSlug(dto.Title);

        var auction = new Auction
        {
            VendorId = vendorId,
            Title = dto.Title,
            Description = dto.Description,
            ProductId = dto.ProductId,
            CategoryId = dto.CategoryId,
            StartingPrice = dto.StartingPrice,
            ReservePrice = dto.ReservePrice,
            CurrentHighestBid = 0,
            Currency = dto.Currency,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt,
            AutoExtendMinutes = dto.AutoExtendMinutes,
            AuctionStatusId = AuctionStatuses.Ids.Draft,
            BuyNowPrice = dto.BuyNowPrice,
            Slug = slug
        };

        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Ok(auction.Id, "Acik artirma olusturuldu.");
    }

    public async Task<ServiceResult> UpdateAuctionAsync(int id, AuctionCreateUpdateDto dto, int vendorId)
    {
        var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == id && a.VendorId == vendorId);
        if (auction == null) return ServiceResult.Fail("Acik artirma bulunamadi.");

        if (auction.AuctionStatusId != AuctionStatuses.Ids.Draft)
            return ServiceResult.Fail("Sadece taslak durumundaki acik artirmalar duzenlenebilir.");

        if (dto.StartAt >= dto.EndAt)
            return ServiceResult.Fail("Bitis tarihi baslangic tarihinden sonra olmalidir.");

        if (dto.BuyNowPrice.HasValue && dto.BuyNowPrice.Value <= dto.StartingPrice)
            return ServiceResult.Fail("Hemen al fiyati baslangic fiyatindan buyuk olmalidir.");

        auction.Title = dto.Title;
        auction.Description = dto.Description;
        auction.ProductId = dto.ProductId;
        auction.CategoryId = dto.CategoryId;
        auction.StartingPrice = dto.StartingPrice;
        auction.ReservePrice = dto.ReservePrice;
        auction.Currency = dto.Currency;
        auction.StartAt = dto.StartAt;
        auction.EndAt = dto.EndAt;
        auction.AutoExtendMinutes = dto.AutoExtendMinutes;
        auction.BuyNowPrice = dto.BuyNowPrice;
        auction.Slug = GenerateSlug(dto.Title);

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Acik artirma guncellendi.");
    }

    public async Task<ServiceResult> CancelAuctionAsync(int id, int vendorId)
    {
        var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == id && a.VendorId == vendorId);
        if (auction == null) return ServiceResult.Fail("Acik artirma bulunamadi.");

        if (auction.AuctionStatusId == AuctionStatuses.Ids.Ended || auction.AuctionStatusId == AuctionStatuses.Ids.Sold)
            return ServiceResult.Fail("Tamamlanmis acik artirmalar iptal edilemez.");

        auction.AuctionStatusId = AuctionStatuses.Ids.Cancelled;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Acik artirma iptal edildi.");
    }

    public async Task<ServiceResult> ActivateAuctionAsync(int id, int vendorId)
    {
        var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == id && a.VendorId == vendorId);
        if (auction == null) return ServiceResult.Fail("Acik artirma bulunamadi.");

        if (auction.AuctionStatusId != AuctionStatuses.Ids.Draft)
            return ServiceResult.Fail("Sadece taslak durumundaki acik artirmalar aktiflestirebilir.");

        // If start time is in the future, set as Scheduled; otherwise Active
        auction.AuctionStatusId = auction.StartAt > DateTime.UtcNow
            ? AuctionStatuses.Ids.Scheduled
            : AuctionStatuses.Ids.Active;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Acik artirma aktiflestirildi.");
    }

    // ============================================
    // BIDS
    // ============================================

    public async Task<ServiceResult> PlaceBidAsync(int auctionId, PlaceBidDto dto, int buyerVendorId)
    {
        var auction = await _context.Auctions
            .Include(a => a.Vendor)
            .FirstOrDefaultAsync(a => a.Id == auctionId);

        if (auction == null) return ServiceResult.Fail("Acik artirma bulunamadi.");

        if (auction.AuctionStatusId != AuctionStatuses.Ids.Active)
            return ServiceResult.Fail("Acik artirma aktif degil.");

        if (auction.EndAt <= DateTime.UtcNow)
            return ServiceResult.Fail("Acik artirma suresi dolmus.");

        if (auction.VendorId == buyerVendorId)
            return ServiceResult.Fail("Kendi acik artirmaniza teklif veremezsiniz.");

        var minBid = auction.CurrentHighestBid > 0 ? auction.CurrentHighestBid : auction.StartingPrice;
        if (dto.BidAmount <= minBid)
            return ServiceResult.Fail($"Teklifiniz mevcut en yuksek tekliften ({minBid:N2} {auction.Currency}) buyuk olmalidir.");

        // Mark previous highest bid as Outbid
        var previousHighestBid = await _context.AuctionBids
            .Where(b => b.AuctionId == auctionId && b.BidStatusId == BidStatuses.Ids.Valid)
            .OrderByDescending(b => b.BidAmount)
            .FirstOrDefaultAsync();

        if (previousHighestBid != null)
        {
            previousHighestBid.BidStatusId = BidStatuses.Ids.Outbid;

            // Notify previous bidder they've been outbid
            await _notificationService.CreateAsync(new DTOs.Notification.NotificationCreateDto
            {
                VendorId = previousHighestBid.BuyerVendorId,
                Type = NotificationType.AuctionOutbid,
                Title = "Teklifiniz gecildi",
                Message = $"\"{auction.Title}\" icin teklifiniz gecildi. Yeni teklif: {dto.BidAmount:N2} {auction.Currency}",
                EntityType = "Auction",
                EntityId = auctionId,
                ActionUrl = $"/Auctions?auctionId={auctionId}",
                Icon = "bi-arrow-down-circle"
            });
        }

        var bidSequence = await _context.AuctionBids.CountAsync(b => b.AuctionId == auctionId) + 1;

        var bid = new AuctionBid
        {
            AuctionId = auctionId,
            BuyerVendorId = buyerVendorId,
            BidAmount = dto.BidAmount,
            BidSequence = bidSequence,
            BidStatusId = BidStatuses.Ids.Valid,
            BidderMessage = dto.BidderMessage
        };

        _context.AuctionBids.Add(bid);

        // Update auction
        auction.CurrentHighestBid = dto.BidAmount;
        auction.BidCount++;

        // Auto-extend: if bid comes in last 5 minutes, extend by AutoExtendMinutes
        if (auction.AutoExtendMinutes.HasValue && auction.AutoExtendMinutes.Value > 0)
        {
            var timeRemaining = auction.EndAt - DateTime.UtcNow;
            if (timeRemaining.TotalMinutes <= 5)
            {
                auction.EndAt = auction.EndAt.AddMinutes(auction.AutoExtendMinutes.Value);
            }
        }

        // Buy Now: if bid >= BuyNowPrice, end auction immediately
        if (auction.BuyNowPrice.HasValue && dto.BidAmount >= auction.BuyNowPrice.Value)
        {
            auction.AuctionStatusId = AuctionStatuses.Ids.Sold;
            auction.EndAt = DateTime.UtcNow;

            // Notify winner
            await _notificationService.CreateAsync(new DTOs.Notification.NotificationCreateDto
            {
                VendorId = buyerVendorId,
                Type = NotificationType.AuctionWon,
                Title = "Acik artirmayi kazandiniz!",
                Message = $"\"{auction.Title}\" icin hemen al fiyatindan satin aldiniz.",
                EntityType = "Auction",
                EntityId = auctionId,
                ActionUrl = $"/Auctions?auctionId={auctionId}",
                Icon = "bi-trophy"
            });
        }

        await _context.SaveChangesAsync();

        // Notify auction owner about new bid
        await _notificationService.CreateAsync(new DTOs.Notification.NotificationCreateDto
        {
            VendorId = auction.VendorId,
            Type = NotificationType.NewAuctionBid,
            Title = "Yeni teklif geldi",
            Message = $"\"{auction.Title}\" icin {dto.BidAmount:N2} {auction.Currency} teklif verildi.",
            EntityType = "Auction",
            EntityId = auctionId,
            ActionUrl = $"/Auctions?auctionId={auctionId}",
            Icon = "bi-hammer"
        });

        return ServiceResult.Ok("Teklifiniz basariyla verildi.");
    }

    public async Task<List<AuctionBidDto>> GetBidsAsync(int auctionId)
    {
        var bids = await _context.AuctionBids
            .Include(b => b.BuyerVendor)
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.BidAmount)
            .ToListAsync();

        return bids.Select(MapToBidDto).ToList();
    }

    // ============================================
    // WATCHLIST
    // ============================================

    public async Task<ServiceResult> ToggleWatchAsync(int auctionId, int watcherVendorId)
    {
        var exists = await _context.AuctionWatchers
            .FirstOrDefaultAsync(w => w.AuctionId == auctionId && w.WatcherVendorId == watcherVendorId);

        var auction = await _context.Auctions.FindAsync(auctionId);
        if (auction == null) return ServiceResult.Fail("Acik artirma bulunamadi.");

        if (exists != null)
        {
            _context.AuctionWatchers.Remove(exists);
            auction.WatchlistCount = Math.Max(0, auction.WatchlistCount - 1);
            await _context.SaveChangesAsync();
            return ServiceResult.Ok("Izleme listesinden cikarildi.");
        }

        _context.AuctionWatchers.Add(new AuctionWatcher
        {
            AuctionId = auctionId,
            WatcherVendorId = watcherVendorId
        });
        auction.WatchlistCount++;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Izleme listesine eklendi.");
    }

    public async Task<AuctionPagedResult<AuctionListDto>> GetWatchedAuctionsAsync(int watcherVendorId, AuctionFilterDto? filter = null)
    {
        filter ??= new AuctionFilterDto();

        var watchedAuctionIds = await _context.AuctionWatchers
            .Where(w => w.WatcherVendorId == watcherVendorId)
            .Select(w => w.AuctionId)
            .ToListAsync();

        var query = _context.Auctions
            .Include(a => a.Vendor)
            .Include(a => a.Category)
            .Where(a => watchedAuctionIds.Contains(a.Id))
            .AsQueryable();

        query = ApplySorting(query, filter.SortBy);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new AuctionPagedResult<AuctionListDto>
        {
            Items = items.Select(a =>
            {
                var dto = MapToListDto(a, watcherVendorId);
                dto.IsWatching = true;
                return dto;
            }).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    // ============================================
    // HELPERS
    // ============================================

    private static IQueryable<Auction> ApplyFilters(IQueryable<Auction> query, AuctionFilterDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(a => a.Title.ToLower().Contains(search));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(a => a.CategoryId == filter.CategoryId.Value);

        if (filter.StatusId.HasValue)
            query = query.Where(a => a.AuctionStatusId == filter.StatusId.Value);

        return query;
    }

    private static IQueryable<Auction> ApplySorting(IQueryable<Auction> query, string? sortBy)
    {
        return sortBy switch
        {
            "endingSoon" => query.OrderBy(a => a.EndAt),
            "newest" => query.OrderByDescending(a => a.CreatedAt),
            "priceLow" => query.OrderBy(a => a.CurrentHighestBid > 0 ? a.CurrentHighestBid : a.StartingPrice),
            "priceHigh" => query.OrderByDescending(a => a.CurrentHighestBid > 0 ? a.CurrentHighestBid : a.StartingPrice),
            "mostBids" => query.OrderByDescending(a => a.BidCount),
            _ => query.OrderByDescending(a => a.CreatedAt)
        };
    }

    private static AuctionListDto MapToListDto(Auction a, int? viewerVendorId = null)
    {
        var statusType = AuctionStatuses.GetById(a.AuctionStatusId);
        return new AuctionListDto
        {
            Id = a.Id,
            VendorId = a.VendorId,
            VendorName = a.Vendor.CompanyName,
            Title = a.Title,
            Slug = a.Slug,
            CategoryId = a.CategoryId,
            CategoryName = a.Category?.Name,
            StartingPrice = a.StartingPrice,
            CurrentHighestBid = a.CurrentHighestBid,
            BuyNowPrice = a.BuyNowPrice,
            Currency = a.Currency,
            StartAt = a.StartAt,
            EndAt = a.EndAt,
            AuctionStatusId = a.AuctionStatusId,
            AuctionStatusName = statusType?.SystemName ?? "",
            AuctionStatusCssClass = statusType?.CssClass ?? "",
            BidCount = a.BidCount,
            ViewCount = a.ViewCount,
            WatchlistCount = a.WatchlistCount,
            IsOwnAuction = viewerVendorId.HasValue && a.VendorId == viewerVendorId.Value,
            CreatedAt = a.CreatedAt
        };
    }

    private static AuctionBidDto MapToBidDto(AuctionBid b)
    {
        var statusType = BidStatuses.GetById(b.BidStatusId);
        return new AuctionBidDto
        {
            Id = b.Id,
            AuctionId = b.AuctionId,
            BuyerVendorId = b.BuyerVendorId,
            BuyerVendorName = b.BuyerVendor.CompanyName,
            BidAmount = b.BidAmount,
            BidSequence = b.BidSequence,
            BidStatusId = b.BidStatusId,
            BidStatusName = statusType?.SystemName ?? "",
            BidStatusCssClass = statusType?.CssClass ?? "",
            BidderMessage = b.BidderMessage,
            CreatedAt = b.CreatedAt
        };
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLower()
            .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u")
            .Replace("ş", "s").Replace("ö", "o").Replace("ç", "c")
            .Replace(" ", "-");

        // Remove non-alphanumeric except hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');

        return slug + "-" + DateTime.UtcNow.Ticks.ToString()[^6..];
    }
}
