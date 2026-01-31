using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Proposal;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Satıcının verdiği tüm teklifleri yöneten birleşik servis
/// ProductInquiryResponse + DemandResponse
/// </summary>
public class ProposalService : IProposalService
{
    private readonly ApplicationDbContext _context;

    public ProposalService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UnifiedProposalSearchResultDto> GetSellerProposalsAsync(int sellerVendorId, UnifiedProposalFilterDto filter)
    {
        var inquiryProposals = new List<UnifiedProposalListDto>();
        var demandProposals = new List<UnifiedProposalListDto>();

        // Sadece inquiry veya sadece demand istenmiyorsa her ikisini de al
        var includeInquiry = string.IsNullOrEmpty(filter.SourceType) || filter.SourceType == "inquiry";
        var includeDemand = string.IsNullOrEmpty(filter.SourceType) || filter.SourceType == "demand";

        if (includeInquiry)
        {
            // ProductInquiryResponse - Satıcının ürün isteklerine verdiği teklifler
            var inquiryQuery = _context.ProductInquiryResponses
                .Include(r => r.Inquiry)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p!.Images)
                .Include(r => r.Inquiry)
                    .ThenInclude(i => i.BuyerVendor)
                .Where(r => r.Inquiry.SellerVendorId == sellerVendorId)
                .AsQueryable();

            // Durum filtresi
            if (filter.Status.HasValue)
            {
                inquiryQuery = inquiryQuery.Where(r => r.Status == filter.Status.Value);
            }

            // Arama
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.ToLower();
                inquiryQuery = inquiryQuery.Where(r =>
                    (r.Inquiry.Product != null && r.Inquiry.Product.Name.ToLower().Contains(search)) ||
                    (r.Inquiry.BuyerVendor != null && r.Inquiry.BuyerVendor.CompanyName != null && r.Inquiry.BuyerVendor.CompanyName.ToLower().Contains(search)));
            }

            inquiryProposals = await inquiryQuery
                .Select(r => new UnifiedProposalListDto
                {
                    Id = r.Id,
                    SourceType = "inquiry",
                    SourceId = r.InquiryId,
                    SourceTitle = r.Inquiry.Product != null ? r.Inquiry.Product.Name : "Ürün",
                    SourceImageUrl = r.Inquiry.Product != null && r.Inquiry.Product.Images.Any(img => img.IsMain)
                        ? r.Inquiry.Product.Images.First(img => img.IsMain).Url
                        : null,
                    BuyerVendorId = r.Inquiry.BuyerVendorId,
                    BuyerCompanyName = r.Inquiry.BuyerVendor != null ? r.Inquiry.BuyerVendor.CompanyName : null,
                    UnitPrice = r.UnitPrice,
                    TotalPrice = r.TotalPrice,
                    Currency = r.Currency,
                    OfferedQuantity = r.OfferedQuantity,
                    OfferedUnit = r.OfferedUnit,
                    LeadTimeDays = r.LeadTimeDays,
                    EstimatedDeliveryDate = r.EstimatedDeliveryDate,
                    ValidUntil = r.ValidUntil,
                    Status = r.Status,
                    StatusText = GetInquiryResponseStatusText(r.Status),
                    IsReadByBuyer = r.IsReadByBuyer,
                    ReadByBuyerAt = r.ReadByBuyerAt,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        if (includeDemand)
        {
            // DemandResponse - Satıcının taleplere verdiği teklifler
            var demandQuery = _context.DemandResponses
                .Include(r => r.Demand)
                    .ThenInclude(d => d.Vendor)
                .Where(r => r.SupplierVendorId == sellerVendorId)
                .AsQueryable();

            // Durum filtresi
            if (filter.Status.HasValue)
            {
                demandQuery = demandQuery.Where(r => r.Status == filter.Status.Value);
            }

            // Arama
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.ToLower();
                demandQuery = demandQuery.Where(r =>
                    r.Demand.Title.ToLower().Contains(search) ||
                    (r.Demand.Vendor != null && r.Demand.Vendor.CompanyName != null && r.Demand.Vendor.CompanyName.ToLower().Contains(search)));
            }

            demandProposals = await demandQuery
                .Select(r => new UnifiedProposalListDto
                {
                    Id = r.Id,
                    SourceType = "demand",
                    SourceId = r.DemandId,
                    SourceTitle = r.Demand.Title,
                    SourceImageUrl = null, // Taleplerde genellikle resim yok
                    BuyerVendorId = r.Demand.VendorId,
                    BuyerCompanyName = r.Demand.Vendor != null ? r.Demand.Vendor.CompanyName : null,
                    UnitPrice = r.UnitPrice,
                    TotalPrice = r.TotalPrice,
                    Currency = r.Currency ?? "TRY",
                    OfferedQuantity = r.Quantity,
                    OfferedUnit = r.Unit,
                    LeadTimeDays = r.LeadTimeDays,
                    EstimatedDeliveryDate = null,
                    ValidUntil = r.ValidUntil,
                    Status = r.Status,
                    StatusText = GetDemandResponseStatusText(r.Status),
                    IsReadByBuyer = r.ViewedAt.HasValue,
                    ReadByBuyerAt = r.ViewedAt,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        // Birleştir ve sırala
        var allProposals = inquiryProposals.Concat(demandProposals);

        if (filter.SortDescending)
            allProposals = allProposals.OrderByDescending(p => p.CreatedAt);
        else
            allProposals = allProposals.OrderBy(p => p.CreatedAt);

        var totalCount = allProposals.Count();

        // Sayfalama
        var items = allProposals
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return new UnifiedProposalSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<UnifiedProposalStatsDto> GetSellerProposalStatsAsync(int sellerVendorId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // ProductInquiryResponse istatistikleri
        var inquiryStats = await _context.ProductInquiryResponses
            .Include(r => r.Inquiry)
            .Where(r => r.Inquiry.SellerVendorId == sellerVendorId)
            .GroupBy(r => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Pending = g.Count(r => r.Status == ProductInquiryResponseStatuses.Pending.Id),
                Accepted = g.Count(r => r.Status == ProductInquiryResponseStatuses.Accepted.Id),
                Rejected = g.Count(r => r.Status == ProductInquiryResponseStatuses.Rejected.Id),
                ThisMonth = g.Count(r => r.CreatedAt >= startOfMonth),
                ThisMonthValue = g.Where(r => r.CreatedAt >= startOfMonth && r.TotalPrice.HasValue)
                    .Sum(r => r.TotalPrice ?? 0)
            })
            .FirstOrDefaultAsync();

        // DemandResponse istatistikleri
        var demandStats = await _context.DemandResponses
            .Where(r => r.SupplierVendorId == sellerVendorId)
            .GroupBy(r => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Pending = g.Count(r => r.Status == 1), // Pending
                Accepted = g.Count(r => r.Status == 4), // Accepted
                Rejected = g.Count(r => r.Status == 5), // Rejected
                ThisMonth = g.Count(r => r.CreatedAt >= startOfMonth),
                ThisMonthValue = g.Where(r => r.CreatedAt >= startOfMonth && r.TotalPrice.HasValue)
                    .Sum(r => r.TotalPrice ?? 0)
            })
            .FirstOrDefaultAsync();

        return new UnifiedProposalStatsDto
        {
            TotalProposals = (inquiryStats?.Total ?? 0) + (demandStats?.Total ?? 0),
            PendingProposals = (inquiryStats?.Pending ?? 0) + (demandStats?.Pending ?? 0),
            AcceptedProposals = (inquiryStats?.Accepted ?? 0) + (demandStats?.Accepted ?? 0),
            RejectedProposals = (inquiryStats?.Rejected ?? 0) + (demandStats?.Rejected ?? 0),
            InquiryProposals = inquiryStats?.Total ?? 0,
            DemandProposals = demandStats?.Total ?? 0,
            ThisMonthProposals = (inquiryStats?.ThisMonth ?? 0) + (demandStats?.ThisMonth ?? 0),
            ThisMonthValue = (inquiryStats?.ThisMonthValue ?? 0) + (demandStats?.ThisMonthValue ?? 0)
        };
    }

    public async Task<UnifiedProposalSearchResultDto> GetInquiryProposalsAsync(int sellerVendorId, UnifiedProposalFilterDto filter)
    {
        filter.SourceType = "inquiry";
        return await GetSellerProposalsAsync(sellerVendorId, filter);
    }

    public async Task<UnifiedProposalSearchResultDto> GetDemandProposalsAsync(int sellerVendorId, UnifiedProposalFilterDto filter)
    {
        filter.SourceType = "demand";
        return await GetSellerProposalsAsync(sellerVendorId, filter);
    }

    // ============================================
    // HELPERS
    // ============================================

    private static string GetInquiryResponseStatusText(int status)
    {
        return status switch
        {
            1 => "Beklemede",      // Pending
            2 => "Görüldü",        // Viewed
            3 => "Kabul Edildi",   // Accepted
            4 => "Reddedildi",     // Rejected
            5 => "Karşı Teklif",   // CounterOffer
            6 => "Süresi Doldu",   // Expired
            _ => "Bilinmiyor"
        };
    }

    private static string GetDemandResponseStatusText(int status)
    {
        return status switch
        {
            1 => "Beklemede",      // Pending
            2 => "Görüldü",        // Viewed
            3 => "Kısa Listede",   // Shortlisted
            4 => "Kabul Edildi",   // Accepted
            5 => "Reddedildi",     // Rejected
            _ => "Bilinmiyor"
        };
    }
}
