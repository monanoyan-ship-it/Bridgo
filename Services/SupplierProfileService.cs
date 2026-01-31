using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Supplier;
using Bridgo.DTOs.Demand;
using Bridgo.Models.Entities;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class SupplierProfileService : ISupplierProfileService
{
    private readonly ApplicationDbContext _context;

    public SupplierProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================
    // SUPPLIER PROFILE - Public
    // ============================================

    public async Task<SupplierSearchResultDto> GetPublicSuppliersAsync(SupplierFilterDto filter)
    {
        var query = _context.SupplierProfiles
            .Include(s => s.Vendor)
            .Include(s => s.Country)
            .Where(s => s.IsPubliclyVisible)
            .AsQueryable();

        // Filtreler
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(s =>
                (s.DisplayName != null && s.DisplayName.ToLower().Contains(search)) ||
                s.Vendor.CompanyName.ToLower().Contains(search) ||
                (s.Description != null && s.Description.ToLower().Contains(search)) ||
                (s.Capabilities != null && s.Capabilities.ToLower().Contains(search)));
        }

        if (filter.CategoryId.HasValue)
        {
            var categoryIdStr = filter.CategoryId.Value.ToString();
            query = query.Where(s => s.CategoryIds != null && s.CategoryIds.Contains(categoryIdStr));
        }

        if (filter.CountryId.HasValue)
            query = query.Where(s => s.CountryId == filter.CountryId);

        if (!string.IsNullOrEmpty(filter.City))
            query = query.Where(s => s.City != null && s.City.ToLower().Contains(filter.City.ToLower()));

        if (filter.IsVerified.HasValue)
            query = query.Where(s => s.IsVerified == filter.IsVerified.Value);

        if (filter.AcceptingNewRequests.HasValue)
            query = query.Where(s => s.AcceptingNewRequests == filter.AcceptingNewRequests.Value);

        if (filter.MinRating.HasValue)
            query = query.Where(s => s.AverageRating >= filter.MinRating.Value);

        if (!string.IsNullOrEmpty(filter.Certifications))
        {
            var cert = filter.Certifications.ToLower();
            query = query.Where(s => s.Certifications != null && s.Certifications.ToLower().Contains(cert));
        }

        // Toplam sayı
        var totalCount = await query.CountAsync();

        // Sıralama
        query = filter.SortBy?.ToLower() switch
        {
            "name" => filter.SortDescending
                ? query.OrderByDescending(s => s.DisplayName ?? s.Vendor.CompanyName)
                : query.OrderBy(s => s.DisplayName ?? s.Vendor.CompanyName),
            "rating" => filter.SortDescending
                ? query.OrderByDescending(s => s.AverageRating)
                : query.OrderBy(s => s.AverageRating),
            "responses" => filter.SortDescending
                ? query.OrderByDescending(s => s.ResponseCount)
                : query.OrderBy(s => s.ResponseCount),
            "views" => filter.SortDescending
                ? query.OrderByDescending(s => s.ViewCount)
                : query.OrderBy(s => s.ViewCount),
            _ => filter.SortDescending
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt)
        };

        // Sayfalama
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(s => MapToListDto(s))
            .ToListAsync();

        // Kategori bilgilerini ekle
        await EnrichWithCategoryInfoAsync(items);

        return new SupplierSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<SupplierProfileDetailDto?> GetProfileBySlugAsync(string slug, bool incrementViewCount = false)
    {
        var profile = await _context.SupplierProfiles
            .Include(s => s.Vendor)
            .Include(s => s.Country)
            .FirstOrDefaultAsync(s => s.Slug == slug && s.IsPubliclyVisible);

        if (profile == null) return null;

        if (incrementViewCount)
        {
            profile.ViewCount++;
            await _context.SaveChangesAsync();
        }

        var dto = MapToDetailDto(profile);
        await EnrichWithCategoryInfoAsync(new List<SupplierProfileListDto> { dto });
        return dto;
    }

    public async Task<SupplierProfileDetailDto?> GetProfileByVendorIdAsync(int vendorId)
    {
        var profile = await _context.SupplierProfiles
            .Include(s => s.Vendor)
            .Include(s => s.Country)
            .FirstOrDefaultAsync(s => s.VendorId == vendorId);

        if (profile == null) return null;

        var dto = MapToDetailDto(profile);
        await EnrichWithCategoryInfoAsync(new List<SupplierProfileListDto> { dto });
        return dto;
    }

    public async Task<bool> SendContactMessageAsync(int supplierId, SupplierContactDto dto)
    {
        var profile = await _context.SupplierProfiles
            .Include(s => s.Vendor)
            .FirstOrDefaultAsync(s => s.Id == supplierId);

        if (profile == null) return false;

        // Contact request sayısını artır
        profile.ContactRequestCount++;
        await _context.SaveChangesAsync();

        // TODO: E-posta gönderme işlemi (IEmailService entegrasyonu)
        // E-posta gönderimi şu an için loglanabilir veya kuyruk sistemiyle işlenebilir

        return true;
    }

    // ============================================
    // SUPPLIER PROFILE - Yonetim
    // ============================================

    public async Task<SupplierProfileDetailDto> CreateOrUpdateProfileAsync(int vendorId, SupplierProfileCreateUpdateDto dto, string? updatedBy)
    {
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(s => s.VendorId == vendorId);

        if (profile == null)
        {
            // Yeni profil oluştur
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == vendorId);
            if (vendor == null) throw new InvalidOperationException("Vendor bulunamadi.");

            var slug = await GenerateUniqueSlugAsync(dto.DisplayName ?? vendor.CompanyName);

            profile = new SupplierProfile
            {
                VendorId = vendorId,
                Slug = slug,
                CreatedBy = updatedBy
            };
            _context.SupplierProfiles.Add(profile);
        }

        // Güncelle
        profile.DisplayName = dto.DisplayName;
        profile.Description = dto.Description;
        profile.ShortDescription = dto.ShortDescription;
        profile.Tagline = dto.Tagline;
        profile.Capabilities = dto.Capabilities;
        profile.ProductionCapacity = dto.ProductionCapacity;
        profile.MinimumOrderValue = dto.MinimumOrderValue?.ToString();
        profile.LeadTime = dto.LeadTime;
        profile.CategoryIds = dto.CategoryIds;
        profile.Certifications = dto.Certifications;
        profile.CountryId = dto.CountryId;
        profile.City = dto.City;
        profile.ServiceRegions = dto.ServiceRegions;
        profile.PublicEmail = dto.PublicEmail;
        profile.PublicPhone = dto.PublicPhone;
        profile.PublicWebsite = dto.PublicWebsite;
        profile.CoverImageUrl = dto.CoverImageUrl;
        profile.GalleryImages = dto.GalleryImages;
        profile.MetaTitle = dto.MetaTitle;
        profile.MetaDescription = dto.MetaDescription;
        profile.MetaKeywords = dto.MetaKeywords;
        profile.IsPubliclyVisible = dto.IsPubliclyVisible;
        profile.AcceptingNewRequests = dto.AcceptingNewRequests;
        profile.ShowContactInfo = dto.ShowContactInfo;
        profile.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();

        return (await GetProfileByVendorIdAsync(vendorId))!;
    }

    public async Task<bool> SetProfileVisibilityAsync(int vendorId, bool isPubliclyVisible, string? updatedBy)
    {
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(s => s.VendorId == vendorId);

        if (profile == null) return false;

        profile.IsPubliclyVisible = isPubliclyVisible;
        profile.UpdatedBy = updatedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GetOrCreateSlugAsync(int vendorId)
    {
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(s => s.VendorId == vendorId);

        if (profile != null) return profile.Slug;

        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == vendorId);
        if (vendor == null) throw new InvalidOperationException("Vendor bulunamadi.");

        return await GenerateUniqueSlugAsync(vendor.CompanyName);
    }

    public async Task<SupplierStatsDto> GetProfileStatsAsync(int vendorId)
    {
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(s => s.VendorId == vendorId);

        var responses = await _context.DemandResponses
            .Where(r => r.SupplierVendorId == vendorId)
            .ToListAsync();

        var subscriptionCount = await _context.CategorySubscriptions
            .CountAsync(s => s.VendorId == vendorId);

        return new SupplierStatsDto
        {
            TotalViews = profile?.ViewCount ?? 0,
            TotalContactRequests = profile?.ContactRequestCount ?? 0,
            TotalResponses = responses.Count,
            AcceptedResponses = responses.Count(r => r.Status == DemandResponseStatus.Accepted),
            AverageRating = profile?.AverageRating,
            RatingCount = profile?.RatingCount ?? 0,
            SubscribedCategories = subscriptionCount
        };
    }

    // ============================================
    // CATEGORY SUBSCRIPTION
    // ============================================

    public async Task<List<CategorySubscriptionDto>> GetSubscriptionsAsync(int vendorId)
    {
        // Oncelikle abonelikleri al
        var subscriptions = await _context.CategorySubscriptions
            .Include(s => s.Category)
            .Where(s => s.VendorId == vendorId)
            .OrderBy(s => s.Category.Name)
            .ToListAsync();

        if (!subscriptions.Any())
            return new List<CategorySubscriptionDto>();

        // Takip edilen kategori ID'lerini al
        var categoryIds = subscriptions.Select(s => s.CategoryId).ToList();

        // Her kategorideki aktif talep sayisini hesapla (kendi taleplerini haric tut)
        var demandCounts = await _context.PublicDemands
            .Where(d => d.CategoryId.HasValue && categoryIds.Contains(d.CategoryId.Value))
            .Where(d => d.Status == 2) // Sadece aktif talepler
            .Where(d => d.VendorId != vendorId) // Kendi taleplerini sayma
            .GroupBy(d => d.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId!.Value, x => x.Count);

        // DTO'lara donustur
        return subscriptions.Select(s => new CategorySubscriptionDto
        {
            Id = s.Id,
            VendorId = s.VendorId,
            CategoryId = s.CategoryId,
            CategoryName = s.Category?.Name,
            CategoryPath = s.Category != null ? GetCategoryPath(s.Category) : null,
            NotifyByEmail = s.NotifyByEmail,
            NotifyInApp = s.NotifyInApp,
            MinQuantity = s.MinQuantity,
            CountryFilter = s.CountryFilter,
            KeywordFilter = s.KeywordFilter,
            NotificationCount = s.NotificationCount,
            LastNotifiedAt = s.LastNotifiedAt,
            DemandCount = demandCounts.TryGetValue(s.CategoryId, out var count) ? count : 0
        }).ToList();
    }

    public async Task<CategorySubscriptionDto> AddSubscriptionAsync(int vendorId, CategorySubscriptionCreateDto dto, string? createdBy)
    {
        // Zaten abone mi kontrol et
        var existing = await _context.CategorySubscriptions
            .FirstOrDefaultAsync(s => s.VendorId == vendorId && s.CategoryId == dto.CategoryId);

        if (existing != null)
            throw new InvalidOperationException("Bu kategoriye zaten abonesiniz.");

        var category = await _context.ProductCategories.FirstOrDefaultAsync(c => c.Id == dto.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Kategori bulunamadi.");

        var subscription = new CategorySubscription
        {
            VendorId = vendorId,
            CategoryId = dto.CategoryId,
            NotifyByEmail = dto.NotifyByEmail,
            NotifyInApp = dto.NotifyInApp,
            MinQuantity = dto.MinQuantity,
            CountryFilter = dto.CountryFilter,
            KeywordFilter = dto.KeywordFilter,
            CreatedBy = createdBy
        };

        _context.CategorySubscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return new CategorySubscriptionDto
        {
            Id = subscription.Id,
            VendorId = subscription.VendorId,
            CategoryId = subscription.CategoryId,
            CategoryName = category.Name,
            NotifyByEmail = subscription.NotifyByEmail,
            NotifyInApp = subscription.NotifyInApp,
            MinQuantity = subscription.MinQuantity,
            CountryFilter = subscription.CountryFilter,
            KeywordFilter = subscription.KeywordFilter,
            NotificationCount = 0
        };
    }

    public async Task<bool> UpdateSubscriptionAsync(int subscriptionId, int vendorId, CategorySubscriptionUpdateDto dto, string? updatedBy)
    {
        var subscription = await _context.CategorySubscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.VendorId == vendorId);

        if (subscription == null) return false;

        subscription.NotifyByEmail = dto.NotifyByEmail;
        subscription.NotifyInApp = dto.NotifyInApp;
        subscription.MinQuantity = dto.MinQuantity;
        subscription.CountryFilter = dto.CountryFilter;
        subscription.KeywordFilter = dto.KeywordFilter;
        subscription.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveSubscriptionAsync(int subscriptionId, int vendorId, string? deletedBy)
    {
        var subscription = await _context.CategorySubscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.VendorId == vendorId);

        if (subscription == null) return false;

        subscription.IsDeleted = true;
        subscription.UpdatedBy = deletedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CategorySubscriptionDto>> AddBulkSubscriptionsAsync(int vendorId, List<int> categoryIds, string? createdBy)
    {
        var result = new List<CategorySubscriptionDto>();

        foreach (var categoryId in categoryIds)
        {
            try
            {
                var sub = await AddSubscriptionAsync(vendorId, new CategorySubscriptionCreateDto
                {
                    CategoryId = categoryId,
                    NotifyByEmail = true,
                    NotifyInApp = true
                }, createdBy);
                result.Add(sub);
            }
            catch
            {
                // Zaten varsa atla
            }
        }

        return result;
    }

    public async Task<bool> IsSubscribedToCategoryAsync(int vendorId, int categoryId)
    {
        return await _context.CategorySubscriptions
            .AnyAsync(s => s.VendorId == vendorId && s.CategoryId == categoryId);
    }

    // ============================================
    // NOTIFICATION MATCHING
    // ============================================

    public async Task<List<int>> FindMatchingSuppliersForDemandAsync(int demandId)
    {
        var demand = await _context.PublicDemands
            .FirstOrDefaultAsync(d => d.Id == demandId);

        if (demand == null || demand.CategoryId == null)
            return new List<int>();

        var matchingVendorIds = new List<int>();

        // Kategori abonelikleri
        var subscriptions = await _context.CategorySubscriptions
            .Include(s => s.Vendor)
            .Where(s => s.CategoryId == demand.CategoryId)
            .ToListAsync();

        foreach (var sub in subscriptions)
        {
            // Minimum miktar kontrolü
            if (sub.MinQuantity.HasValue && demand.Quantity.HasValue && demand.Quantity < sub.MinQuantity)
                continue;

            // Ülke filtresi kontrolü
            if (!string.IsNullOrEmpty(sub.CountryFilter) && demand.CountryId.HasValue)
            {
                var countryIds = sub.CountryFilter.Split(',').Select(x => int.TryParse(x.Trim(), out var id) ? id : 0).ToList();
                if (!countryIds.Contains(demand.CountryId.Value))
                    continue;
            }

            // Keyword filtresi kontrolü
            if (!string.IsNullOrEmpty(sub.KeywordFilter))
            {
                var keywords = sub.KeywordFilter.Split(',').Select(k => k.Trim().ToLower()).ToList();
                var demandText = $"{demand.Title} {demand.Description}".ToLower();
                if (!keywords.Any(k => demandText.Contains(k)))
                    continue;
            }

            matchingVendorIds.Add(sub.VendorId);
        }

        return matchingVendorIds.Distinct().ToList();
    }

    public async Task IncrementNotificationCountAsync(int subscriptionId)
    {
        var subscription = await _context.CategorySubscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);

        if (subscription != null)
        {
            subscription.NotificationCount++;
            subscription.LastNotifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private async Task<string> GenerateUniqueSlugAsync(string name)
    {
        var slug = GenerateSlug(name);
        var baseSlug = slug;
        var counter = 1;

        while (await _context.SupplierProfiles.AnyAsync(s => s.Slug == slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
            .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u")
            .Replace("ş", "s").Replace("ö", "o").Replace("ç", "c")
            .Replace("İ", "i").Replace("Ğ", "g").Replace("Ü", "u")
            .Replace("Ş", "s").Replace("Ö", "o").Replace("Ç", "c");

        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        return slug;
    }

    private static string? GetCategoryPath(ProductCategory category)
    {
        // Basit path - ileride parent chain ile genişletilebilir
        return category.Name;
    }

    private async Task EnrichWithCategoryInfoAsync(List<SupplierProfileListDto> items)
    {
        var allCategoryIds = items
            .Where(i => !string.IsNullOrEmpty(i.CategoryIds))
            .SelectMany(i => i.CategoryIds!.Split(',').Select(x => int.TryParse(x.Trim(), out var id) ? id : 0))
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (!allCategoryIds.Any()) return;

        var categories = await _context.ProductCategories
            .Where(c => allCategoryIds.Contains(c.Id))
            .Select(c => new CategoryInfoDto
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon
            })
            .ToListAsync();

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.CategoryIds)) continue;

            var ids = item.CategoryIds.Split(',')
                .Select(x => int.TryParse(x.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();

            item.Categories = categories.Where(c => ids.Contains(c.Id)).ToList();
        }
    }

    private static SupplierProfileListDto MapToListDto(SupplierProfile s)
    {
        return new SupplierProfileListDto
        {
            Id = s.Id,
            VendorId = s.VendorId,
            Slug = s.Slug,
            DisplayName = s.DisplayName,
            CompanyName = s.Vendor?.CompanyName,
            ShortDescription = s.ShortDescription,
            Tagline = s.Tagline,
            CoverImageUrl = s.CoverImageUrl,
            LogoUrl = s.Vendor?.LogoUrl,
            CountryId = s.CountryId,
            CountryName = s.Country?.Name,
            City = s.City,
            CategoryIds = s.CategoryIds,
            Certifications = s.Certifications,
            IsVerified = s.IsVerified,
            AcceptingNewRequests = s.AcceptingNewRequests,
            AverageRating = s.AverageRating,
            RatingCount = s.RatingCount,
            ViewCount = s.ViewCount,
            ResponseCount = s.ResponseCount
        };
    }

    private static SupplierProfileDetailDto MapToDetailDto(SupplierProfile s)
    {
        return new SupplierProfileDetailDto
        {
            Id = s.Id,
            VendorId = s.VendorId,
            Slug = s.Slug,
            DisplayName = s.DisplayName,
            CompanyName = s.Vendor?.CompanyName,
            ShortDescription = s.ShortDescription,
            Tagline = s.Tagline,
            CoverImageUrl = s.CoverImageUrl,
            LogoUrl = s.Vendor?.LogoUrl,
            CountryId = s.CountryId,
            CountryName = s.Country?.Name,
            City = s.City,
            CategoryIds = s.CategoryIds,
            Certifications = s.Certifications,
            IsVerified = s.IsVerified,
            AcceptingNewRequests = s.AcceptingNewRequests,
            AverageRating = s.AverageRating,
            RatingCount = s.RatingCount,
            ViewCount = s.ViewCount,
            ResponseCount = s.ResponseCount,
            Description = s.Description,
            Capabilities = s.Capabilities,
            ProductionCapacity = s.ProductionCapacity,
            MinimumOrderValue = s.MinimumOrderValue,
            LeadTime = s.LeadTime,
            ServiceRegions = s.ServiceRegions,
            PublicEmail = s.ShowContactInfo ? s.PublicEmail : null,
            PublicPhone = s.ShowContactInfo ? s.PublicPhone : null,
            PublicWebsite = s.ShowContactInfo ? s.PublicWebsite : null,
            GalleryImages = s.GalleryImages,
            GalleryImageList = s.GalleryImages?.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList(),
            MetaTitle = s.MetaTitle,
            MetaDescription = s.MetaDescription,
            MetaKeywords = s.MetaKeywords,
            IsPubliclyVisible = s.IsPubliclyVisible,
            ShowContactInfo = s.ShowContactInfo,
            ContactRequestCount = s.ContactRequestCount,
            CreatedAt = s.CreatedAt
        };
    }
}
