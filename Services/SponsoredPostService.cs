using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.SocialFeed;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class SponsoredPostService : ISponsoredPostService
{
    private readonly ApplicationDbContext _context;
    private readonly ISocialFeedService _feedService;

    public SponsoredPostService(ApplicationDbContext context, ISocialFeedService feedService)
    {
        _context = context;
        _feedService = feedService;
    }

    public async Task<ServiceResult<int>> CreateSponsoredPostAsync(SponsoredPostCreateDto dto, int vendorId)
    {
        // Post'un vendor'a ait oldugundan emin ol
        var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == dto.SocialPostId && p.VendorId == vendorId);
        if (post == null)
            return ServiceResult<int>.Fail("Post bulunamadi veya size ait degil");

        if (post.StatusId != SocialPostStatuses.Ids.Published)
            return ServiceResult<int>.Fail("Sadece yayindaki postlar sponsorlanabilir");

        // Ayni post icin aktif sponsorluk var mi
        var existing = await _context.SponsoredPosts
            .AnyAsync(s => s.SocialPostId == dto.SocialPostId &&
                           (s.StatusId == SponsoredPostStatuses.Ids.Active || s.StatusId == SponsoredPostStatuses.Ids.Draft));
        if (existing)
            return ServiceResult<int>.Fail("Bu post icin zaten aktif/taslak sponsorluk var");

        var sponsored = new SponsoredPost
        {
            SocialPostId = dto.SocialPostId,
            VendorId = vendorId,
            StatusId = SponsoredPostStatuses.Ids.Draft,
            BudgetAmount = dto.BudgetAmount,
            Currency = dto.Currency,
            TargetImpressions = dto.TargetImpressions,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        _context.SponsoredPosts.Add(sponsored);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Ok(sponsored.Id);
    }

    public async Task<ServiceResult> UpdateSponsoredPostAsync(int id, SponsoredPostUpdateDto dto, int vendorId)
    {
        var sponsored = await _context.SponsoredPosts
            .FirstOrDefaultAsync(s => s.Id == id && s.VendorId == vendorId);

        if (sponsored == null)
            return ServiceResult.Fail("Sponsorlu icerik bulunamadi");

        if (sponsored.StatusId != SponsoredPostStatuses.Ids.Draft && sponsored.StatusId != SponsoredPostStatuses.Ids.Paused)
            return ServiceResult.Fail("Sadece taslak veya duraklatilmis sponsorluklar guncellenebilir");

        sponsored.BudgetAmount = dto.BudgetAmount;
        sponsored.Currency = dto.Currency;
        sponsored.TargetImpressions = dto.TargetImpressions;
        sponsored.StartDate = dto.StartDate;
        sponsored.EndDate = dto.EndDate;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ActivateSponsoredPostAsync(int id, int vendorId)
    {
        var sponsored = await _context.SponsoredPosts
            .FirstOrDefaultAsync(s => s.Id == id && s.VendorId == vendorId);

        if (sponsored == null)
            return ServiceResult.Fail("Sponsorlu icerik bulunamadi");

        if (sponsored.StatusId != SponsoredPostStatuses.Ids.Draft && sponsored.StatusId != SponsoredPostStatuses.Ids.Paused)
            return ServiceResult.Fail("Bu sponsorluk aktiflestirilemiyor");

        sponsored.StatusId = SponsoredPostStatuses.Ids.Active;
        if (sponsored.StartDate == null)
            sponsored.StartDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> PauseSponsoredPostAsync(int id, int vendorId)
    {
        var sponsored = await _context.SponsoredPosts
            .FirstOrDefaultAsync(s => s.Id == id && s.VendorId == vendorId);

        if (sponsored == null)
            return ServiceResult.Fail("Sponsorlu icerik bulunamadi");

        if (sponsored.StatusId != SponsoredPostStatuses.Ids.Active)
            return ServiceResult.Fail("Sadece aktif sponsorluklar duraklatilabilir");

        sponsored.StatusId = SponsoredPostStatuses.Ids.Paused;
        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<List<SponsoredPostDto>> GetVendorSponsoredPostsAsync(int vendorId)
    {
        var items = await _context.SponsoredPosts
            .Include(s => s.SocialPost)
            .Include(s => s.Vendor)
            .Where(s => s.VendorId == vendorId)
            .OrderByDescending(s => s.Id)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<SponsoredPostDto?> GetSponsoredPostAsync(int id, int vendorId)
    {
        var item = await _context.SponsoredPosts
            .Include(s => s.SocialPost)
            .Include(s => s.Vendor)
            .FirstOrDefaultAsync(s => s.Id == id && s.VendorId == vendorId);

        return item == null ? null : MapToDto(item);
    }

    public async Task<SocialPostDto?> GetNextSponsoredPostForFeedAsync(int viewerVendorId, int currentUserId)
    {
        var now = DateTime.UtcNow;

        var sponsored = await _context.SponsoredPosts
            .Include(s => s.SocialPost)
                .ThenInclude(p => p.Vendor)
            .Include(s => s.SocialPost)
                .ThenInclude(p => p.Author)
            .Include(s => s.SocialPost)
                .ThenInclude(p => p.Images)
            .Include(s => s.SocialPost)
                .ThenInclude(p => p.Product)
            .Where(s => s.StatusId == SponsoredPostStatuses.Ids.Active)
            .Where(s => s.CurrentImpressions < s.TargetImpressions)
            .Where(s => s.StartDate == null || s.StartDate <= now)
            .Where(s => s.EndDate == null || s.EndDate >= now)
            .Where(s => s.VendorId != viewerVendorId)
            .OrderBy(s => s.CurrentImpressions) // En az gosterilen one gelsin
            .FirstOrDefaultAsync();

        if (sponsored == null) return null;

        var post = sponsored.SocialPost;
        if (post == null || post.StatusId != SocialPostStatuses.Ids.Published) return null;

        var dto = await _feedService.GetPostByIdAsync(post.Id, currentUserId);
        if (dto == null) return null;

        dto.IsSponsored = true;
        dto.SponsoredPostId = sponsored.Id;

        return dto;
    }

    public async Task RecordImpressionAsync(int sponsoredPostId)
    {
        var sponsored = await _context.SponsoredPosts.FindAsync(sponsoredPostId);
        if (sponsored == null) return;

        sponsored.CurrentImpressions++;

        // Hedef gosterime ulasildiysa tamamla
        if (sponsored.CurrentImpressions >= sponsored.TargetImpressions)
            sponsored.StatusId = SponsoredPostStatuses.Ids.Completed;

        await _context.SaveChangesAsync();
    }

    public async Task RecordClickAsync(int sponsoredPostId)
    {
        var sponsored = await _context.SponsoredPosts.FindAsync(sponsoredPostId);
        if (sponsored == null) return;

        sponsored.CurrentClicks++;
        await _context.SaveChangesAsync();
    }

    private static SponsoredPostDto MapToDto(SponsoredPost s)
    {
        var status = SponsoredPostStatuses.GetById(s.StatusId);

        return new SponsoredPostDto
        {
            Id = s.Id,
            SocialPostId = s.SocialPostId,
            PostTitle = s.SocialPost?.Title,
            PostContentPreview = s.SocialPost?.Content?.Length > 100
                ? s.SocialPost.Content[..100] + "..."
                : s.SocialPost?.Content ?? "",
            VendorId = s.VendorId,
            VendorName = s.Vendor?.CompanyName ?? "",
            StatusId = s.StatusId,
            StatusName = status?.SystemName ?? "",
            StatusCssClass = status?.CssClass ?? "",
            BudgetAmount = s.BudgetAmount,
            Currency = s.Currency,
            SpentAmount = s.SpentAmount,
            TargetImpressions = s.TargetImpressions,
            CurrentImpressions = s.CurrentImpressions,
            CurrentClicks = s.CurrentClicks,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            CreatedAt = s.CreatedAt
        };
    }
}
