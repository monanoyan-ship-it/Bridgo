using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.CapabilityProfile;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class CapabilityProfileService : ICapabilityProfileService
{
    private readonly ApplicationDbContext _context;

    public CapabilityProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CapabilityProfileDto?> GetProfileAsync(int vendorId, int capabilityId)
    {
        var profile = await _context.CapabilityProfiles
            .Include(p => p.Vendor)
            .Include(p => p.Country)
            .FirstOrDefaultAsync(p => p.VendorId == vendorId && p.CapabilityId == capabilityId);

        if (profile == null) return null;

        return MapToDto(profile);
    }

    public async Task<CapabilityProfileDto> GetOrCreateProfileAsync(int vendorId, int capabilityId, string? createdBy)
    {
        var profile = await _context.CapabilityProfiles
            .Include(p => p.Vendor)
            .Include(p => p.Country)
            .FirstOrDefaultAsync(p => p.VendorId == vendorId && p.CapabilityId == capabilityId);

        if (profile != null)
        {
            return MapToDto(profile);
        }

        // Yoksa olustur
        var vendor = await _context.Vendors.FindAsync(vendorId);
        var capability = Capabilities.GetById(capabilityId);

        if (vendor == null || capability == null)
        {
            throw new InvalidOperationException("Vendor veya Capability bulunamadi.");
        }

        // Slug olustur
        var slug = await GenerateSlugAsync(vendor.CompanyName, capabilityId);

        profile = new Models.Entities.CapabilityProfile
        {
            VendorId = vendorId,
            CapabilityId = capabilityId,
            Slug = slug,
            DisplayName = vendor.CompanyName,
            CreatedBy = createdBy
        };

        _context.CapabilityProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // Include'lari yukle
        await _context.Entry(profile).Reference(p => p.Vendor).LoadAsync();

        return MapToDto(profile);
    }

    public async Task<CapabilityProfileDto> CreateOrUpdateProfileAsync(int vendorId, int capabilityId, CapabilityProfileCreateUpdateDto dto, string? updatedBy)
    {
        var profile = await _context.CapabilityProfiles
            .Include(p => p.Vendor)
            .Include(p => p.Country)
            .FirstOrDefaultAsync(p => p.VendorId == vendorId && p.CapabilityId == capabilityId);

        var vendor = await _context.Vendors.FindAsync(vendorId);
        if (vendor == null)
        {
            throw new InvalidOperationException("Vendor bulunamadi.");
        }

        if (profile == null)
        {
            // Yeni profil olustur
            var slug = !string.IsNullOrEmpty(dto.Slug)
                ? await GenerateSlugAsync(dto.Slug, capabilityId)
                : await GenerateSlugAsync(vendor.CompanyName, capabilityId);

            profile = new Models.Entities.CapabilityProfile
            {
                VendorId = vendorId,
                CapabilityId = capabilityId,
                Slug = slug,
                CreatedBy = updatedBy
            };
            _context.CapabilityProfiles.Add(profile);
        }

        // Genel alanlar
        profile.DisplayName = dto.DisplayName ?? vendor.CompanyName;
        profile.Description = dto.Description;
        profile.ShortDescription = dto.ShortDescription;
        profile.Tagline = dto.Tagline;

        // Hizmetler
        profile.Services = dto.Services;
        profile.Capabilities = dto.Capabilities;
        profile.Certifications = dto.Certifications;
        profile.ServiceRegions = dto.ServiceRegions;

        // Lokasyon
        profile.CountryId = dto.CountryId;
        profile.City = dto.City;
        profile.Address = dto.Address;

        // Iletisim
        profile.PublicEmail = dto.PublicEmail ?? dto.Email;
        profile.PublicPhone = dto.PublicPhone ?? dto.Phone;
        profile.PublicWebsite = dto.PublicWebsite ?? dto.Website;

        // Gorseller
        profile.CoverImageUrl = dto.CoverImageUrl;
        if (dto.GalleryImageList != null && dto.GalleryImageList.Any())
        {
            profile.GalleryImages = JsonSerializer.Serialize(dto.GalleryImageList);
        }
        else if (!string.IsNullOrEmpty(dto.GalleryImages))
        {
            profile.GalleryImages = dto.GalleryImages;
        }

        // SEO
        profile.MetaTitle = dto.MetaTitle;
        profile.MetaDescription = dto.MetaDescription;

        // Durum
        profile.IsPubliclyVisible = dto.IsPubliclyVisible || dto.IsPublic;
        profile.AcceptingNewRequests = dto.AcceptingNewRequests;
        profile.ShowContactInfo = dto.ShowContactInfo;

        // === SATICI (2) OZEL ALANLARI ===
        if (dto.CategoryIdList != null && dto.CategoryIdList.Any())
        {
            profile.CategoryIds = string.Join(",", dto.CategoryIdList);
        }
        else if (!string.IsNullOrEmpty(dto.CategoryIds))
        {
            profile.CategoryIds = dto.CategoryIds;
        }
        profile.ProductionCapacity = dto.ProductionCapacity;
        profile.MinimumOrderValue = dto.MinimumOrderValue;
        profile.LeadTime = dto.LeadTime;

        // === ALICI (3) OZEL ALANLARI ===
        profile.Industry = dto.Industry;
        profile.BusinessType = dto.BusinessType;
        profile.PreferredCategories = dto.PreferredCategories;
        profile.AnnualPurchaseVolume = dto.AnnualPurchaseVolume;
        profile.PurchaseVolumeCurrency = dto.PurchaseVolumeCurrency;

        // === TASIMACI (4) OZEL ALANLARI ===
        profile.FleetInfo = dto.FleetInfo;
        profile.TransportModes = dto.TransportModes;
        profile.Routes = dto.Routes;

        // === SIGORTA (5) OZEL ALANLARI ===
        profile.InsuranceTypes = dto.InsuranceTypes;
        profile.CoverageTypes = dto.CoverageTypes;
        profile.MaxCoverageAmount = dto.MaxCoverageAmount;
        profile.MaxCoverageCurrency = dto.MaxCoverageCurrency;

        // === GUMRUK (6) OZEL ALANLARI ===
        profile.CustomsServices = dto.CustomsServices;
        profile.LicenseNumbers = dto.LicenseNumbers;
        profile.CustomsOffices = dto.CustomsOffices;

        // === GOZETIM (7) OZEL ALANLARI ===
        profile.InspectionTypes = dto.InspectionTypes;
        profile.InspectionStandards = dto.InspectionStandards;

        // === YATIRIMCI (8) OZEL ALANLARI ===
        profile.InvestmentTypes = dto.InvestmentTypes;
        profile.InvestmentFocus = dto.InvestmentFocus;
        profile.MinInvestmentAmount = dto.MinInvestmentAmount;
        profile.MaxInvestmentAmount = dto.MaxInvestmentAmount;
        profile.InvestmentCurrency = dto.InvestmentCurrency;
        profile.InterestRateRange = dto.InterestRateRange;

        profile.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();

        // Include'lari yukle
        await _context.Entry(profile).Reference(p => p.Vendor).LoadAsync();
        if (profile.CountryId.HasValue)
        {
            await _context.Entry(profile).Reference(p => p.Country).LoadAsync();
        }

        return MapToDto(profile);
    }

    public async Task<bool> SetVisibilityAsync(int vendorId, int capabilityId, bool isPubliclyVisible, string? updatedBy)
    {
        var profile = await _context.CapabilityProfiles
            .FirstOrDefaultAsync(p => p.VendorId == vendorId && p.CapabilityId == capabilityId);

        if (profile == null) return false;

        // Yayinlama talep ediliyor
        if (isPubliclyVisible && !profile.IsPubliclyVisible)
        {
            profile.PublicationRequestedAt = DateTime.UtcNow;
            profile.RejectionReason = null; // Onceki red sebebini temizle
            // IsVerified degismiyor - admin onaylayacak
        }
        // Yayindan kaldiriliyor
        else if (!isPubliclyVisible)
        {
            profile.PublicationRequestedAt = null;
        }

        profile.IsPubliclyVisible = isPubliclyVisible;
        profile.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CapabilityProfileStatsDto> GetStatsAsync(int vendorId, int capabilityId)
    {
        var profile = await _context.CapabilityProfiles
            .FirstOrDefaultAsync(p => p.VendorId == vendorId && p.CapabilityId == capabilityId);

        if (profile == null)
        {
            return new CapabilityProfileStatsDto();
        }

        return new CapabilityProfileStatsDto
        {
            TotalViews = profile.ViewCount,
            TotalContactRequests = profile.ContactRequestCount,
            TotalResponses = profile.ResponseCount,
            AverageRating = profile.AverageRating,
            RatingCount = profile.RatingCount
        };
    }

    public async Task<CapabilityProfileDto?> GetBySlugAsync(string slug, bool incrementViewCount = false)
    {
        // Sadece hem IsPubliclyVisible hem de IsVerified olan profiller public
        var profile = await _context.CapabilityProfiles
            .Include(p => p.Vendor)
            .Include(p => p.Country)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPubliclyVisible && p.IsVerified);

        if (profile == null) return null;

        if (incrementViewCount)
        {
            profile.ViewCount++;
            await _context.SaveChangesAsync();
        }

        return MapToDto(profile);
    }

    public async Task<List<CapabilityProfileDto>> GetVendorProfilesAsync(int vendorId)
    {
        var profiles = await _context.CapabilityProfiles
            .Include(p => p.Vendor)
            .Include(p => p.Country)
            .Where(p => p.VendorId == vendorId)
            .ToListAsync();

        return profiles.Select(MapToDto).ToList();
    }

    // ============================================
    // ADMIN METODLARI
    // ============================================

    public async Task<List<ProfileApprovalListItemDto>> GetPendingApprovalsAsync()
    {
        // Onay bekleyenler: IsPubliclyVisible = true, IsVerified = false, RejectionReason = null
        var profiles = await _context.CapabilityProfiles
            .Include(p => p.Vendor)
            .Where(p => p.IsPubliclyVisible && !p.IsVerified && p.RejectionReason == null)
            .OrderBy(p => p.PublicationRequestedAt)
            .ToListAsync();

        return profiles.Select(p => new ProfileApprovalListItemDto
        {
            Id = p.Id,
            VendorId = p.VendorId,
            CompanyName = p.Vendor?.CompanyName ?? "",
            LogoUrl = p.Vendor?.LogoUrl,
            CapabilityId = p.CapabilityId,
            CapabilityName = Capabilities.GetById(p.CapabilityId)?.Description ?? "",
            CapabilityIcon = Capabilities.GetById(p.CapabilityId)?.Icon,
            DisplayName = p.DisplayName,
            Slug = p.Slug,
            ShortDescription = p.ShortDescription,
            PublicationRequestedAt = p.PublicationRequestedAt,
            CreatedAt = p.CreatedAt
        }).ToList();
    }

    public async Task<bool> ApproveProfileAsync(int profileId, int approvedByUserId)
    {
        var profile = await _context.CapabilityProfiles.FindAsync(profileId);
        if (profile == null) return false;

        profile.IsVerified = true;
        profile.VerifiedAt = DateTime.UtcNow;
        profile.VerifiedByUserId = approvedByUserId;
        profile.RejectionReason = null;
        profile.UpdatedBy = approvedByUserId.ToString();

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectProfileAsync(int profileId, string reason, int rejectedByUserId)
    {
        var profile = await _context.CapabilityProfiles.FindAsync(profileId);
        if (profile == null) return false;

        profile.IsVerified = false;
        profile.IsPubliclyVisible = false; // Yayindan da kaldir
        profile.RejectionReason = reason;
        profile.UpdatedBy = rejectedByUserId.ToString();

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CapabilityProfileDto?> GetProfileByIdAsync(int profileId)
    {
        var profile = await _context.CapabilityProfiles
            .Include(p => p.Vendor)
            .Include(p => p.Country)
            .FirstOrDefaultAsync(p => p.Id == profileId);

        if (profile == null) return null;
        return MapToDto(profile);
    }

    // ============================================
    // PRIVATE HELPERS
    // ============================================

    private async Task<string> GenerateSlugAsync(string baseName, int capabilityId)
    {
        var capability = Capabilities.GetById(capabilityId);
        var capabilitySlug = capability?.SystemName?.ToLower() ?? "profile";

        var slug = GenerateSlugFromName(baseName) + "-" + capabilitySlug;

        // Unique kontrolu
        var exists = await _context.CapabilityProfiles.AnyAsync(p => p.Slug == slug);
        if (exists)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks % 10000}";
        }

        return slug;
    }

    private static string GenerateSlugFromName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "profile";

        // Turkce karakterleri degistir
        var slug = name.ToLower()
            .Replace("ü", "u")
            .Replace("ğ", "g")
            .Replace("ş", "s")
            .Replace("ç", "c")
            .Replace("ö", "o")
            .Replace("ı", "i");

        // Alfanumerik olmayan karakterleri tire ile degistir
        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");

        // Bas ve sondaki tireleri kaldir
        slug = slug.Trim('-');

        return string.IsNullOrEmpty(slug) ? "profile" : slug;
    }

    private CapabilityProfileDto MapToDto(Models.Entities.CapabilityProfile profile)
    {
        var dto = new CapabilityProfileDto
        {
            Id = profile.Id,
            VendorId = profile.VendorId,
            CapabilityId = profile.CapabilityId,
            CapabilityName = Capabilities.GetById(profile.CapabilityId)?.SystemName ?? "",
            Slug = profile.Slug,

            DisplayName = profile.DisplayName,
            CompanyName = profile.Vendor?.CompanyName,
            Description = profile.Description,
            ShortDescription = profile.ShortDescription,
            Tagline = profile.Tagline,

            Services = profile.Services,
            Capabilities = profile.Capabilities,
            Certifications = profile.Certifications,
            ServiceRegions = profile.ServiceRegions,

            CountryId = profile.CountryId,
            CountryName = profile.Country?.Name,
            City = profile.City,
            Address = profile.Address,

            PublicEmail = profile.PublicEmail,
            PublicPhone = profile.PublicPhone,
            PublicWebsite = profile.PublicWebsite,

            CoverImageUrl = profile.CoverImageUrl,
            LogoUrl = profile.Vendor?.LogoUrl,
            GalleryImages = profile.GalleryImages,

            MetaTitle = profile.MetaTitle,
            MetaDescription = profile.MetaDescription,

            IsPubliclyVisible = profile.IsPubliclyVisible,
            IsVerified = profile.IsVerified,
            AcceptingNewRequests = profile.AcceptingNewRequests,
            ShowContactInfo = profile.ShowContactInfo,
            PublicationRequestedAt = profile.PublicationRequestedAt,
            VerifiedAt = profile.VerifiedAt,
            RejectionReason = profile.RejectionReason,

            ViewCount = profile.ViewCount,
            ContactRequestCount = profile.ContactRequestCount,
            ResponseCount = profile.ResponseCount,
            AverageRating = profile.AverageRating,
            RatingCount = profile.RatingCount,

            // Satici
            CategoryIds = profile.CategoryIds,
            ProductionCapacity = profile.ProductionCapacity,
            MinimumOrderValue = profile.MinimumOrderValue,
            LeadTime = profile.LeadTime,

            // Alici
            Industry = profile.Industry,
            BusinessType = profile.BusinessType,
            PreferredCategories = profile.PreferredCategories,
            AnnualPurchaseVolume = profile.AnnualPurchaseVolume,
            PurchaseVolumeCurrency = profile.PurchaseVolumeCurrency,

            // Tasimaci
            FleetInfo = profile.FleetInfo,
            TransportModes = profile.TransportModes,
            Routes = profile.Routes,

            // Sigorta
            InsuranceTypes = profile.InsuranceTypes,
            CoverageTypes = profile.CoverageTypes,
            MaxCoverageAmount = profile.MaxCoverageAmount,
            MaxCoverageCurrency = profile.MaxCoverageCurrency,

            // Gumruk
            CustomsServices = profile.CustomsServices,
            LicenseNumbers = profile.LicenseNumbers,
            CustomsOffices = profile.CustomsOffices,

            // Gozetim
            InspectionTypes = profile.InspectionTypes,
            InspectionStandards = profile.InspectionStandards,

            // Yatirimci
            InvestmentTypes = profile.InvestmentTypes,
            InvestmentFocus = profile.InvestmentFocus,
            MinInvestmentAmount = profile.MinInvestmentAmount,
            MaxInvestmentAmount = profile.MaxInvestmentAmount,
            InvestmentCurrency = profile.InvestmentCurrency,
            InterestRateRange = profile.InterestRateRange,

            CreatedAt = profile.CreatedAt
        };

        // Gallery images parse
        if (!string.IsNullOrEmpty(profile.GalleryImages))
        {
            try
            {
                dto.GalleryImageList = JsonSerializer.Deserialize<List<GalleryImageItemDto>>(profile.GalleryImages);
            }
            catch
            {
                dto.GalleryImageList = new List<GalleryImageItemDto>();
            }
        }

        // Categories parse (for Satici)
        if (!string.IsNullOrEmpty(profile.CategoryIds))
        {
            var categoryIds = profile.CategoryIds.Split(',')
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();

            if (categoryIds.Any())
            {
                dto.Categories = _context.ProductCategories
                    .Where(c => categoryIds.Contains(c.Id))
                    .Select(c => new CategoryInfoItemDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Icon = c.Icon
                    })
                    .ToList();
            }
        }

        return dto;
    }
}
