using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Auction;
using Bridgo.DTOs.RewardPoints;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class RewardPointService : IRewardPointService
{
    private readonly ApplicationDbContext _context;

    public RewardPointService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RewardPointSummaryDto> GetSummaryAsync(int vendorId)
    {
        var lastEntry = await _context.RewardPointHistories
            .Where(r => r.VendorId == vendorId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var monthEntries = await _context.RewardPointHistories
            .Where(r => r.VendorId == vendorId && r.CreatedAt >= startOfMonth)
            .ToListAsync();

        return new RewardPointSummaryDto
        {
            TotalPoints = lastEntry?.BalanceAfter ?? 0,
            EarnedThisMonth = monthEntries.Where(r => r.Points > 0).Sum(r => r.Points),
            SpentThisMonth = monthEntries.Where(r => r.Points < 0).Sum(r => Math.Abs(r.Points))
        };
    }

    public async Task<AuctionPagedResult<RewardPointHistoryDto>> GetHistoryAsync(int vendorId, RewardPointFilterDto? filter = null)
    {
        filter ??= new RewardPointFilterDto();

        var query = _context.RewardPointHistories
            .Where(r => r.VendorId == vendorId)
            .AsQueryable();

        if (filter.ActionTypeId.HasValue)
            query = query.Where(r => r.ActionTypeId == filter.ActionTypeId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new AuctionPagedResult<RewardPointHistoryDto>
        {
            Items = items.Select(r =>
            {
                var actionType = RewardPointActionTypes.GetById(r.ActionTypeId);
                return new RewardPointHistoryDto
                {
                    Id = r.Id,
                    Points = r.Points,
                    ActionTypeId = r.ActionTypeId,
                    ActionTypeName = actionType?.SystemName ?? "",
                    ActionTypeCssClass = actionType?.CssClass ?? "",
                    Description = r.Description,
                    OrderId = r.OrderId,
                    BalanceAfter = r.BalanceAfter,
                    CreatedAt = r.CreatedAt
                };
            }).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<ServiceResult> AddPointsAsync(int vendorId, int points, int actionTypeId, string? description = null, int? orderId = null)
    {
        if (points <= 0) return ServiceResult.Fail("Puan sifirdan buyuk olmalidir.");

        var currentBalance = await GetCurrentBalanceAsync(vendorId);

        var entry = new RewardPointHistory
        {
            VendorId = vendorId,
            Points = points,
            ActionTypeId = actionTypeId,
            Description = description,
            OrderId = orderId,
            BalanceAfter = currentBalance + points
        };

        _context.RewardPointHistories.Add(entry);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok($"{points} puan eklendi.");
    }

    public async Task<ServiceResult> SpendPointsAsync(int vendorId, int points, string? description = null)
    {
        if (points <= 0) return ServiceResult.Fail("Puan sifirdan buyuk olmalidir.");

        var currentBalance = await GetCurrentBalanceAsync(vendorId);
        if (currentBalance < points)
            return ServiceResult.Fail($"Yetersiz puan. Mevcut: {currentBalance}");

        var entry = new RewardPointHistory
        {
            VendorId = vendorId,
            Points = -points,
            ActionTypeId = RewardPointActionTypes.Ids.Spent,
            Description = description ?? "Puan harcama",
            BalanceAfter = currentBalance - points
        };

        _context.RewardPointHistories.Add(entry);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok($"{points} puan harcandi.");
    }

    private async Task<int> GetCurrentBalanceAsync(int vendorId)
    {
        var lastEntry = await _context.RewardPointHistories
            .Where(r => r.VendorId == vendorId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        return lastEntry?.BalanceAfter ?? 0;
    }
}
