using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Demand;
using Bridgo.Extensions;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class DemandService : IDemandService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public DemandService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    // ============================================
    // PUBLIC DEMAND - Talep CRUD
    // ============================================

    public async Task<DemandSearchResultDto> GetPublicDemandsAsync(DemandFilterDto filter)
    {
        var query = _context.PublicDemands
            .Include(d => d.Vendor)
            .Include(d => d.Category)
            .Include(d => d.Country)
            .Where(d => d.Status == DemandStatus.Active && d.Visibility == DemandVisibility.Public)
            .AsQueryable();

        // Filtreler
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(d => d.Title.ToLower().Contains(search) ||
                                     (d.Description != null && d.Description.ToLower().Contains(search)) ||
                                     (d.Tags != null && d.Tags.ToLower().Contains(search)));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(d => d.CategoryId == filter.CategoryId);

        if (filter.CountryId.HasValue)
            query = query.Where(d => d.CountryId == filter.CountryId);

        if (!string.IsNullOrEmpty(filter.City))
            query = query.Where(d => d.City != null && d.City.ToLower().Contains(filter.City.ToLower()));

        if (filter.HasReferenceProduct.HasValue)
            query = query.Where(d => filter.HasReferenceProduct.Value ? d.ReferenceProductId != null : d.ReferenceProductId == null);

        if (filter.BudgetMin.HasValue)
            query = query.Where(d => d.BudgetMax >= filter.BudgetMin);

        if (filter.BudgetMax.HasValue)
            query = query.Where(d => d.BudgetMin <= filter.BudgetMax);

        // Toplam sayı
        var totalCount = await query.CountAsync();

        // Sıralama
        query = filter.SortBy?.ToLower() switch
        {
            "title" => filter.SortDescending ? query.OrderByDescending(d => d.Title) : query.OrderBy(d => d.Title),
            "responses" => filter.SortDescending ? query.OrderByDescending(d => d.ResponseCount) : query.OrderBy(d => d.ResponseCount),
            "views" => filter.SortDescending ? query.OrderByDescending(d => d.ViewCount) : query.OrderBy(d => d.ViewCount),
            "expires" => filter.SortDescending ? query.OrderByDescending(d => d.ExpiresAt) : query.OrderBy(d => d.ExpiresAt),
            _ => filter.SortDescending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt)
        };

        // Sayfalama
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(d => MapToListDto(d))
            .ToListAsync();

        return new DemandSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<DemandDetailDto?> GetDemandBySlugAsync(string slug, bool incrementViewCount = false)
    {
        var demand = await _context.PublicDemands
            .Include(d => d.Vendor)
            .Include(d => d.Category)
            .Include(d => d.Country)
            .Include(d => d.ReferenceProduct)
                .ThenInclude(p => p!.Images)
            .Include(d => d.Modifications.OrderBy(m => m.DisplayOrder))
            .Include(d => d.Attachments.OrderBy(a => a.DisplayOrder))
            .FirstOrDefaultAsync(d => d.Slug == slug);

        if (demand == null) return null;

        if (incrementViewCount)
        {
            demand.ViewCount++;
            await _context.SaveChangesAsync();
        }

        return MapToDetailDto(demand);
    }

    public async Task<DemandDetailDto?> GetDemandByIdAsync(int id)
    {
        var demand = await _context.PublicDemands
            .Include(d => d.Vendor)
            .Include(d => d.Category)
            .Include(d => d.Country)
            .Include(d => d.ReferenceProduct)
                .ThenInclude(p => p!.Images)
            .Include(d => d.Modifications.OrderBy(m => m.DisplayOrder))
            .Include(d => d.Attachments.OrderBy(a => a.DisplayOrder))
            .Include(d => d.Responses.OrderByDescending(r => r.CreatedAt))
                .ThenInclude(r => r.SupplierVendor)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (demand == null) return null;

        return MapToDetailDto(demand, includeResponses: true);
    }

    public async Task<DemandSearchResultDto> GetVendorDemandsAsync(int vendorId, DemandFilterDto filter)
    {
        var query = _context.PublicDemands
            .Include(d => d.Category)
            .Include(d => d.Country)
            .Where(d => d.VendorId == vendorId)
            .AsQueryable();

        // Status filtresi
        if (filter.Status.HasValue)
            query = query.Where(d => d.Status == filter.Status);

        // Arama
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(d => d.Title.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        // Sıralama
        query = filter.SortDescending
            ? query.OrderByDescending(d => d.CreatedAt)
            : query.OrderBy(d => d.CreatedAt);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(d => new DemandListDto
            {
                Id = d.Id,
                Title = d.Title,
                Slug = d.Slug,
                Description = d.Description,
                Quantity = d.Quantity,
                Unit = d.Unit,
                CategoryId = d.CategoryId,
                CategoryName = d.Category != null ? d.Category.Name : null,
                CountryId = d.CountryId,
                CountryName = d.Country != null ? d.Country.Name : null,
                City = d.City,
                DesiredLeadTimeDays = d.DesiredLeadTimeDays,
                DesiredDeliveryDate = d.DesiredDeliveryDate,
                BudgetMin = d.BudgetMin,
                BudgetMax = d.BudgetMax,
                BudgetCurrency = d.BudgetCurrency,
                Visibility = d.Visibility,
                Status = d.Status,
                ExpiresAt = d.ExpiresAt,
                ViewCount = d.ViewCount,
                ResponseCount = d.ResponseCount,
                VendorId = d.VendorId,
                HasReferenceProduct = d.ReferenceProductId != null,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();

        return new DemandSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<DemandSearchResultDto> GetSubscribedDemandsAsync(int vendorId, DemandFilterDto filter)
    {
        // Vendor'in takip ettigi kategorileri bul
        var subscribedCategoryIds = await _context.CategorySubscriptions
            .Where(s => s.VendorId == vendorId)
            .Select(s => s.CategoryId)
            .ToListAsync();

        if (!subscribedCategoryIds.Any())
        {
            return new DemandSearchResultDto
            {
                Items = new List<DemandListDto>(),
                TotalCount = 0,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        // Takip edilen kategorilerdeki aktif talepleri getir (kendi taleplerimiz haric)
        var query = _context.PublicDemands
            .Include(d => d.Category)
            .Include(d => d.Country)
            .Where(d => d.CategoryId.HasValue && subscribedCategoryIds.Contains(d.CategoryId.Value))
            .Where(d => d.Status == 2) // Sadece aktif talepler
            .Where(d => d.VendorId != vendorId) // Kendi taleplerimizi gosterme
            .AsQueryable();

        // Arama
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(d => d.Title.ToLower().Contains(search));
        }

        // Kategori filtresi
        if (filter.CategoryId.HasValue)
            query = query.Where(d => d.CategoryId == filter.CategoryId);

        var totalCount = await query.CountAsync();

        // Sıralama - en yeniler uste
        query = query.OrderByDescending(d => d.PublishedAt ?? d.CreatedAt);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(d => new DemandListDto
            {
                Id = d.Id,
                Title = d.Title,
                Slug = d.Slug,
                Description = d.Description,
                Quantity = d.Quantity,
                Unit = d.Unit,
                CategoryId = d.CategoryId,
                CategoryName = d.Category != null ? d.Category.Name : null,
                CountryId = d.CountryId,
                CountryName = d.Country != null ? d.Country.Name : null,
                City = d.City,
                DesiredLeadTimeDays = d.DesiredLeadTimeDays,
                DesiredDeliveryDate = d.DesiredDeliveryDate,
                BudgetMin = d.BudgetMin,
                BudgetMax = d.BudgetMax,
                BudgetCurrency = d.BudgetCurrency,
                Visibility = d.Visibility,
                Status = d.Status,
                ExpiresAt = d.ExpiresAt,
                ViewCount = d.ViewCount,
                ResponseCount = d.ResponseCount,
                VendorId = d.VendorId,
                HasReferenceProduct = d.ReferenceProductId != null,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();

        return new DemandSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<DemandDetailDto> CreateDemandAsync(int vendorId, DemandCreateDto dto, string? createdBy)
    {
        var slug = await GenerateUniqueSlugAsync(dto.Title);

        var demand = new PublicDemand
        {
            Title = dto.Title,
            Slug = slug,
            Description = dto.Description,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            CategoryId = dto.CategoryId,
            Tags = dto.Tags,
            ReferenceProductId = dto.ReferenceProductId,
            ModificationNotes = dto.ModificationNotes,
            CountryId = dto.CountryId,
            City = dto.City,
            DesiredLeadTimeDays = dto.DesiredLeadTimeDays,
            DesiredDeliveryDate = dto.DesiredDeliveryDate.ToUtcSafe(),
            BudgetMin = dto.BudgetMin,
            BudgetMax = dto.BudgetMax,
            BudgetCurrency = dto.BudgetCurrency ?? "TRY",
            Visibility = dto.Visibility,
            Status = DemandStatus.Draft,
            ExpiresAt = dto.ExpiresAt.ToUtcSafe(),
            IsIndexable = dto.IsIndexable,
            MetaTitle = dto.MetaTitle ?? dto.Title,
            MetaDescription = dto.MetaDescription ?? dto.Description?.Substring(0, Math.Min(dto.Description.Length, 160)),
            VendorId = vendorId,
            CreatedBy = createdBy
        };

        _context.PublicDemands.Add(demand);
        await _context.SaveChangesAsync();

        // Modifikasyonlar
        if (dto.Modifications != null && dto.Modifications.Any())
        {
            foreach (var mod in dto.Modifications)
            {
                _context.DemandModifications.Add(new DemandModification
                {
                    DemandId = demand.Id,
                    PropertyName = mod.PropertyName,
                    OriginalValue = mod.OriginalValue,
                    DesiredValue = mod.DesiredValue,
                    Notes = mod.Notes,
                    DisplayOrder = mod.DisplayOrder,
                    CreatedBy = createdBy
                });
            }
            await _context.SaveChangesAsync();
        }

        return (await GetDemandByIdAsync(demand.Id))!;
    }

    public async Task<bool> UpdateDemandAsync(int demandId, int vendorId, DemandUpdateDto dto, string? updatedBy)
    {
        var demand = await _context.PublicDemands
            .FirstOrDefaultAsync(d => d.Id == demandId && d.VendorId == vendorId);

        if (demand == null) return false;

        demand.Title = dto.Title;
        demand.Description = dto.Description;
        demand.Quantity = dto.Quantity;
        demand.Unit = dto.Unit;
        demand.CategoryId = dto.CategoryId;
        demand.Tags = dto.Tags;
        demand.ReferenceProductId = dto.ReferenceProductId;
        demand.ModificationNotes = dto.ModificationNotes;
        demand.CountryId = dto.CountryId;
        demand.City = dto.City;
        demand.DesiredLeadTimeDays = dto.DesiredLeadTimeDays;
        demand.DesiredDeliveryDate = dto.DesiredDeliveryDate.ToUtcSafe();
        demand.BudgetMin = dto.BudgetMin;
        demand.BudgetMax = dto.BudgetMax;
        demand.BudgetCurrency = dto.BudgetCurrency;
        demand.Visibility = dto.Visibility;
        demand.Status = dto.Status;
        demand.ExpiresAt = dto.ExpiresAt.ToUtcSafe();
        demand.IsIndexable = dto.IsIndexable;
        demand.MetaTitle = dto.MetaTitle;
        demand.MetaDescription = dto.MetaDescription;
        demand.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDemandAsync(int demandId, int vendorId, string? deletedBy)
    {
        var demand = await _context.PublicDemands
            .FirstOrDefaultAsync(d => d.Id == demandId && d.VendorId == vendorId);

        if (demand == null) return false;

        demand.IsDeleted = true;
        demand.UpdatedBy = deletedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateDemandStatusAsync(int demandId, int vendorId, int status, string? updatedBy)
    {
        var demand = await _context.PublicDemands
            .FirstOrDefaultAsync(d => d.Id == demandId && d.VendorId == vendorId);

        if (demand == null) return false;

        demand.Status = status;
        demand.UpdatedBy = updatedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PublishDemandAsync(int demandId, int vendorId, string? updatedBy)
    {
        var demand = await _context.PublicDemands
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d => d.Id == demandId && d.VendorId == vendorId);

        if (demand == null) return false;

        demand.Status = DemandStatus.Active;
        demand.UpdatedBy = updatedBy;
        demand.PublishedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Kategori abonelerine bildirim gonder
        await _notificationService.NotifySubscribersAsync(demand);

        return true;
    }

    public async Task<bool> CloseDemandAsync(int demandId, int vendorId, string? updatedBy)
    {
        return await UpdateDemandStatusAsync(demandId, vendorId, DemandStatus.Closed, updatedBy);
    }

    // ============================================
    // DEMAND MODIFICATIONS
    // ============================================

    public async Task<DemandModificationDto> AddModificationAsync(int demandId, int vendorId, DemandModificationCreateDto dto, string? createdBy)
    {
        var demand = await _context.PublicDemands.FirstOrDefaultAsync(d => d.Id == demandId && d.VendorId == vendorId);
        if (demand == null) throw new InvalidOperationException("Talep bulunamadi.");

        var modification = new DemandModification
        {
            DemandId = demandId,
            PropertyName = dto.PropertyName,
            OriginalValue = dto.OriginalValue,
            DesiredValue = dto.DesiredValue,
            Notes = dto.Notes,
            DisplayOrder = dto.DisplayOrder,
            CreatedBy = createdBy
        };

        _context.DemandModifications.Add(modification);
        await _context.SaveChangesAsync();

        return new DemandModificationDto
        {
            Id = modification.Id,
            PropertyName = modification.PropertyName,
            OriginalValue = modification.OriginalValue,
            DesiredValue = modification.DesiredValue,
            Notes = modification.Notes,
            DisplayOrder = modification.DisplayOrder
        };
    }

    public async Task<bool> UpdateModificationAsync(int modificationId, int vendorId, DemandModificationCreateDto dto, string? updatedBy)
    {
        var modification = await _context.DemandModifications
            .Include(m => m.Demand)
            .FirstOrDefaultAsync(m => m.Id == modificationId && m.Demand.VendorId == vendorId);

        if (modification == null) return false;

        modification.PropertyName = dto.PropertyName;
        modification.OriginalValue = dto.OriginalValue;
        modification.DesiredValue = dto.DesiredValue;
        modification.Notes = dto.Notes;
        modification.DisplayOrder = dto.DisplayOrder;
        modification.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteModificationAsync(int modificationId, int vendorId, string? deletedBy)
    {
        var modification = await _context.DemandModifications
            .Include(m => m.Demand)
            .FirstOrDefaultAsync(m => m.Id == modificationId && m.Demand.VendorId == vendorId);

        if (modification == null) return false;

        modification.IsDeleted = true;
        modification.UpdatedBy = deletedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    // ============================================
    // DEMAND ATTACHMENTS
    // ============================================

    public async Task<DemandAttachmentDto> AddAttachmentAsync(int demandId, int vendorId, string fileName, string filePath, string? mimeType, long? fileSize, string? title, string? description, string? createdBy)
    {
        var demand = await _context.PublicDemands.FirstOrDefaultAsync(d => d.Id == demandId && d.VendorId == vendorId);
        if (demand == null) throw new InvalidOperationException("Talep bulunamadi.");

        var maxOrder = await _context.DemandAttachments
            .Where(a => a.DemandId == demandId)
            .MaxAsync(a => (int?)a.DisplayOrder) ?? 0;

        var attachment = new DemandAttachment
        {
            DemandId = demandId,
            FileName = fileName,
            FilePath = filePath,
            MimeType = mimeType,
            FileSize = fileSize,
            Title = title,
            Description = description,
            DisplayOrder = maxOrder + 1,
            CreatedBy = createdBy
        };

        _context.DemandAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        return new DemandAttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            FilePath = attachment.FilePath,
            MimeType = attachment.MimeType,
            FileSize = attachment.FileSize,
            Title = attachment.Title,
            Description = attachment.Description,
            DisplayOrder = attachment.DisplayOrder
        };
    }

    public async Task<bool> DeleteAttachmentAsync(int attachmentId, int vendorId, string? deletedBy)
    {
        var attachment = await _context.DemandAttachments
            .Include(a => a.Demand)
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.Demand.VendorId == vendorId);

        if (attachment == null) return false;

        attachment.IsDeleted = true;
        attachment.UpdatedBy = deletedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    // ============================================
    // DEMAND RESPONSES
    // ============================================

    public async Task<List<DemandResponseListDto>> GetDemandResponsesAsync(int demandId, int vendorId)
    {
        var demand = await _context.PublicDemands.FirstOrDefaultAsync(d => d.Id == demandId && d.VendorId == vendorId);
        if (demand == null) return new List<DemandResponseListDto>();

        return await _context.DemandResponses
            .Include(r => r.SupplierVendor)
            .Where(r => r.DemandId == demandId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new DemandResponseListDto
            {
                Id = r.Id,
                DemandId = r.DemandId,
                SupplierVendorId = r.SupplierVendorId,
                SupplierName = r.SupplierVendor != null ? r.SupplierVendor.CompanyName : null,
                IsExternalSupplier = r.SupplierVendorId == null,
                ExternalCompanyName = r.ExternalCompanyName,
                UnitPrice = r.UnitPrice,
                TotalPrice = r.TotalPrice,
                Currency = r.Currency,
                Quantity = r.Quantity,
                Unit = r.Unit,
                LeadTimeDays = r.LeadTimeDays,
                ValidUntil = r.ValidUntil,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<DemandResponseListDto>> GetAllResponsesForVendorAsync(int vendorId)
    {
        // Vendor'in tum taleplerine gelen yanitlari getir
        return await _context.DemandResponses
            .Include(r => r.Demand)
            .Include(r => r.SupplierVendor)
            .Where(r => r.Demand.VendorId == vendorId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new DemandResponseListDto
            {
                Id = r.Id,
                DemandId = r.DemandId,
                DemandTitle = r.Demand.Title,
                DemandCategoryName = r.Demand.Category != null ? r.Demand.Category.Name : null,
                SupplierVendorId = r.SupplierVendorId,
                SupplierName = r.SupplierVendor != null ? r.SupplierVendor.CompanyName : null,
                IsExternalSupplier = r.SupplierVendorId == null,
                ExternalCompanyName = r.ExternalCompanyName,
                ExternalEmail = r.ExternalEmail,
                UnitPrice = r.UnitPrice,
                TotalPrice = r.TotalPrice,
                Currency = r.Currency,
                Quantity = r.Quantity,
                Unit = r.Unit,
                LeadTimeDays = r.LeadTimeDays,
                ValidUntil = r.ValidUntil,
                Notes = r.Notes,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<DemandResponseDetailDto?> GetResponseByIdAsync(int responseId, int vendorId)
    {
        var response = await _context.DemandResponses
            .Include(r => r.Demand)
            .Include(r => r.SupplierVendor)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == responseId &&
                (r.Demand.VendorId == vendorId || r.SupplierVendorId == vendorId));

        if (response == null) return null;

        return new DemandResponseDetailDto
        {
            Id = response.Id,
            DemandId = response.DemandId,
            SupplierVendorId = response.SupplierVendorId,
            SupplierName = response.SupplierVendor?.CompanyName,
            IsExternalSupplier = response.SupplierVendorId == null,
            ExternalCompanyName = response.ExternalCompanyName,
            ExternalContactName = response.ExternalContactName,
            ExternalEmail = response.ExternalEmail,
            ExternalPhone = response.ExternalPhone,
            ExternalWebsite = response.ExternalWebsite,
            UnitPrice = response.UnitPrice,
            TotalPrice = response.TotalPrice,
            Currency = response.Currency,
            Quantity = response.Quantity,
            Unit = response.Unit,
            LeadTimeDays = response.LeadTimeDays,
            ValidUntil = response.ValidUntil,
            Notes = response.Notes,
            TermsAndConditions = response.TermsAndConditions,
            Status = response.Status,
            ViewedAt = response.ViewedAt,
            RejectionReason = response.RejectionReason,
            CreatedAt = response.CreatedAt,
            Attachments = response.Attachments.Select(a => new DemandResponseAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FilePath = a.FilePath,
                MimeType = a.MimeType,
                FileSize = a.FileSize,
                Title = a.Title,
                DisplayOrder = a.DisplayOrder
            }).ToList()
        };
    }

    public async Task<DemandResponseDetailDto> CreateResponseAsync(int supplierVendorId, DemandResponseCreateDto dto, string? createdBy)
    {
        var demand = await _context.PublicDemands.FirstOrDefaultAsync(d => d.Id == dto.DemandId && d.Status == DemandStatus.Active);
        if (demand == null) throw new InvalidOperationException("Talep bulunamadi veya aktif degil.");

        // Ayni vendor'dan daha once yanit var mi kontrol et
        var existingResponse = await _context.DemandResponses
            .FirstOrDefaultAsync(r => r.DemandId == dto.DemandId && r.SupplierVendorId == supplierVendorId);
        if (existingResponse != null) throw new InvalidOperationException("Bu talebe zaten yanit verdiniz.");

        var response = new DemandResponse
        {
            DemandId = dto.DemandId,
            SupplierVendorId = supplierVendorId,
            UnitPrice = dto.UnitPrice,
            TotalPrice = dto.TotalPrice,
            Currency = dto.Currency ?? "TRY",
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            LeadTimeDays = dto.LeadTimeDays,
            ValidUntil = dto.ValidUntil.ToUtcSafe(),
            Notes = dto.Notes,
            TermsAndConditions = dto.TermsAndConditions,
            Status = DemandResponseStatus.Pending,
            CreatedBy = createdBy
        };

        _context.DemandResponses.Add(response);
        demand.ResponseCount++;
        await _context.SaveChangesAsync();

        // Talep sahibine bildirim gonder
        await _notificationService.NotifyNewResponseAsync(response);

        return (await GetResponseByIdAsync(response.Id, supplierVendorId))!;
    }

    public async Task<DemandResponseDetailDto> CreateExternalResponseAsync(ExternalDemandResponseCreateDto dto)
    {
        var demand = await _context.PublicDemands.FirstOrDefaultAsync(d => d.Id == dto.DemandId && d.Status == DemandStatus.Active);
        if (demand == null) throw new InvalidOperationException("Talep bulunamadi veya aktif degil.");

        var response = new DemandResponse
        {
            DemandId = dto.DemandId,
            SupplierVendorId = null,  // Dis uretici
            ExternalCompanyName = dto.ExternalCompanyName,
            ExternalContactName = dto.ExternalContactName,
            ExternalEmail = dto.ExternalEmail,
            ExternalPhone = dto.ExternalPhone,
            ExternalWebsite = dto.ExternalWebsite,
            UnitPrice = dto.UnitPrice,
            TotalPrice = dto.TotalPrice,
            Currency = dto.Currency ?? "TRY",
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            LeadTimeDays = dto.LeadTimeDays,
            ValidUntil = dto.ValidUntil.ToUtcSafe(),
            Notes = dto.Notes,
            TermsAndConditions = dto.TermsAndConditions,
            Status = DemandResponseStatus.Pending
        };

        _context.DemandResponses.Add(response);
        demand.ResponseCount++;
        await _context.SaveChangesAsync();

        // Talep sahibine bildirim gonder
        await _notificationService.NotifyNewResponseAsync(response);

        // External response icin ozel getir (vendorId yok)
        var createdResponse = await _context.DemandResponses
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == response.Id);

        return new DemandResponseDetailDto
        {
            Id = createdResponse!.Id,
            DemandId = createdResponse.DemandId,
            IsExternalSupplier = true,
            ExternalCompanyName = createdResponse.ExternalCompanyName,
            ExternalContactName = createdResponse.ExternalContactName,
            ExternalEmail = createdResponse.ExternalEmail,
            ExternalPhone = createdResponse.ExternalPhone,
            ExternalWebsite = createdResponse.ExternalWebsite,
            UnitPrice = createdResponse.UnitPrice,
            TotalPrice = createdResponse.TotalPrice,
            Currency = createdResponse.Currency,
            Quantity = createdResponse.Quantity,
            Unit = createdResponse.Unit,
            LeadTimeDays = createdResponse.LeadTimeDays,
            ValidUntil = createdResponse.ValidUntil,
            Notes = createdResponse.Notes,
            TermsAndConditions = createdResponse.TermsAndConditions,
            Status = createdResponse.Status,
            CreatedAt = createdResponse.CreatedAt,
            Attachments = new List<DemandResponseAttachmentDto>()
        };
    }

    public async Task<bool> UpdateResponseStatusAsync(int responseId, int demandOwnerVendorId, DemandResponseStatusUpdateDto dto, string? updatedBy)
    {
        var response = await _context.DemandResponses
            .Include(r => r.Demand)
            .FirstOrDefaultAsync(r => r.Id == responseId && r.Demand.VendorId == demandOwnerVendorId);

        if (response == null) return false;

        var oldStatus = response.Status;
        response.Status = dto.Status;
        response.RejectionReason = dto.RejectionReason;
        response.UpdatedBy = updatedBy;
        await _context.SaveChangesAsync();

        // Tedarikçiye durum degisikligi bildirimi gonder
        await _notificationService.NotifyResponseStatusChangedAsync(response, oldStatus);

        return true;
    }

    public async Task<bool> MarkResponseAsViewedAsync(int responseId, int demandOwnerVendorId)
    {
        var response = await _context.DemandResponses
            .Include(r => r.Demand)
            .FirstOrDefaultAsync(r => r.Id == responseId && r.Demand.VendorId == demandOwnerVendorId);

        if (response == null) return false;

        if (response.Status == DemandResponseStatus.Pending)
        {
            response.Status = DemandResponseStatus.Viewed;
        }
        response.ViewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<DemandResponseListDto>> GetMyResponsesAsync(int supplierVendorId)
    {
        var responses = await _context.DemandResponses
            .Include(r => r.Demand)
                .ThenInclude(d => d.Vendor)
            .Where(r => r.SupplierVendorId == supplierVendorId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return responses.Select(r =>
        {
            var status = DemandResponseStatuses.GetById(r.Status);
            return new DemandResponseListDto
            {
                Id = r.Id,
                DemandId = r.DemandId,
                DemandTitle = r.Demand?.Title,
                DemandSlug = r.Demand?.Slug,
                BuyerVendorId = r.Demand?.VendorId,
                BuyerCompanyName = r.Demand?.Vendor?.CompanyName,
                SupplierVendorId = r.SupplierVendorId,
                UnitPrice = r.UnitPrice,
                TotalPrice = r.TotalPrice,
                Currency = r.Currency,
                Quantity = r.Quantity,
                Unit = r.Unit,
                LeadTimeDays = r.LeadTimeDays,
                ValidUntil = r.ValidUntil,
                Status = r.Status,
                StatusName = status?.Description ?? "",
                CreatedAt = r.CreatedAt,
                // Pazarlik alanlari
                IsNegotiationActive = r.IsNegotiationActive,
                CurrentTurnVendorId = r.CurrentTurnVendorId,
                CurrentRoundNumber = r.CurrentRoundNumber,
                ExpiresAt = r.NegotiationExpiresAt,
                IsMyTurn = r.IsNegotiationActive && r.CurrentTurnVendorId == supplierVendorId
            };
        }).ToList();
    }

    // ============================================
    // RESPONSE ATTACHMENTS
    // ============================================

    public async Task<DemandResponseAttachmentDto> AddResponseAttachmentAsync(int responseId, int supplierVendorId, string fileName, string filePath, string? mimeType, long? fileSize, string? title, string? createdBy)
    {
        var response = await _context.DemandResponses.FirstOrDefaultAsync(r => r.Id == responseId && r.SupplierVendorId == supplierVendorId);
        if (response == null) throw new InvalidOperationException("Yanit bulunamadi.");

        var maxOrder = await _context.DemandResponseAttachments
            .Where(a => a.ResponseId == responseId)
            .MaxAsync(a => (int?)a.DisplayOrder) ?? 0;

        var attachment = new DemandResponseAttachment
        {
            ResponseId = responseId,
            FileName = fileName,
            FilePath = filePath,
            MimeType = mimeType,
            FileSize = fileSize,
            Title = title,
            DisplayOrder = maxOrder + 1,
            CreatedBy = createdBy
        };

        _context.DemandResponseAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        return new DemandResponseAttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            FilePath = attachment.FilePath,
            MimeType = attachment.MimeType,
            FileSize = attachment.FileSize,
            Title = attachment.Title,
            DisplayOrder = attachment.DisplayOrder
        };
    }

    public async Task<bool> DeleteResponseAttachmentAsync(int attachmentId, int supplierVendorId, string? deletedBy)
    {
        var attachment = await _context.DemandResponseAttachments
            .Include(a => a.Response)
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.Response.SupplierVendorId == supplierVendorId);

        if (attachment == null) return false;

        attachment.IsDeleted = true;
        attachment.UpdatedBy = deletedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    // ============================================
    // ISTATISTIKLER
    // ============================================

    public async Task<DemandStatsDto> GetVendorDemandStatsAsync(int vendorId)
    {
        var demands = await _context.PublicDemands
            .Where(d => d.VendorId == vendorId)
            .ToListAsync();

        var demandIds = demands.Select(d => d.Id).ToList();
        var responses = await _context.DemandResponses
            .Where(r => demandIds.Contains(r.DemandId))
            .ToListAsync();

        return new DemandStatsDto
        {
            TotalDemands = demands.Count,
            ActiveDemands = demands.Count(d => d.Status == DemandStatus.Active),
            ClosedDemands = demands.Count(d => d.Status == DemandStatus.Closed || d.Status == DemandStatus.Awarded),
            TotalResponses = responses.Count,
            PendingResponses = responses.Count(r => r.Status == DemandResponseStatus.Pending),
            AcceptedResponses = responses.Count(r => r.Status == DemandResponseStatus.Accepted)
        };
    }

    public async Task<ResponseStatsDto> GetSupplierResponseStatsAsync(int vendorId)
    {
        var responses = await _context.DemandResponses
            .Where(r => r.SupplierVendorId == vendorId)
            .ToListAsync();

        return new ResponseStatsDto
        {
            TotalResponses = responses.Count,
            PendingResponses = responses.Count(r => r.Status == DemandResponseStatus.Pending),
            ViewedResponses = responses.Count(r => r.Status == DemandResponseStatus.Viewed),
            AcceptedResponses = responses.Count(r => r.Status == DemandResponseStatus.Accepted),
            RejectedResponses = responses.Count(r => r.Status == DemandResponseStatus.Rejected)
        };
    }

    // ============================================
    // SLUG URETIMI
    // ============================================

    public async Task<string> GenerateUniqueSlugAsync(string title)
    {
        var slug = GenerateSlug(title);
        var baseSlug = slug;
        var counter = 1;

        while (await _context.PublicDemands.AnyAsync(d => d.Slug == slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private static string GenerateSlug(string title)
    {
        // Turkce karakterleri donustur
        var slug = title.ToLowerInvariant()
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("İ", "i")
            .Replace("Ğ", "g")
            .Replace("Ü", "u")
            .Replace("Ş", "s")
            .Replace("Ö", "o")
            .Replace("Ç", "c");

        // Alfanumerik olmayan karakterleri tire ile degistir
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        return slug;
    }

    // ============================================
    // MAPPING HELPERS
    // ============================================

    private static DemandListDto MapToListDto(PublicDemand d)
    {
        return new DemandListDto
        {
            Id = d.Id,
            Title = d.Title,
            Slug = d.Slug,
            Description = d.Description,
            Quantity = d.Quantity,
            Unit = d.Unit,
            CategoryId = d.CategoryId,
            CategoryName = d.Category?.Name,
            Tags = d.Tags,
            CountryId = d.CountryId,
            CountryName = d.Country?.Name,
            City = d.City,
            DesiredLeadTimeDays = d.DesiredLeadTimeDays,
            DesiredDeliveryDate = d.DesiredDeliveryDate,
            BudgetMin = d.BudgetMin,
            BudgetMax = d.BudgetMax,
            BudgetCurrency = d.BudgetCurrency,
            Visibility = d.Visibility,
            Status = d.Status,
            ExpiresAt = d.ExpiresAt,
            ViewCount = d.ViewCount,
            ResponseCount = d.ResponseCount,
            VendorId = d.VendorId,
            VendorName = d.Vendor?.CompanyName,
            HasReferenceProduct = d.ReferenceProductId != null,
            CreatedAt = d.CreatedAt
        };
    }

    private static DemandDetailDto MapToDetailDto(PublicDemand d, bool includeResponses = false)
    {
        var dto = new DemandDetailDto
        {
            Id = d.Id,
            Title = d.Title,
            Slug = d.Slug,
            Description = d.Description,
            Quantity = d.Quantity,
            Unit = d.Unit,
            CategoryId = d.CategoryId,
            CategoryName = d.Category?.Name,
            Tags = d.Tags,
            CountryId = d.CountryId,
            CountryName = d.Country?.Name,
            City = d.City,
            DesiredLeadTimeDays = d.DesiredLeadTimeDays,
            DesiredDeliveryDate = d.DesiredDeliveryDate,
            BudgetMin = d.BudgetMin,
            BudgetMax = d.BudgetMax,
            BudgetCurrency = d.BudgetCurrency,
            Visibility = d.Visibility,
            Status = d.Status,
            ExpiresAt = d.ExpiresAt,
            ViewCount = d.ViewCount,
            ResponseCount = d.ResponseCount,
            VendorId = d.VendorId,
            VendorName = d.Vendor?.CompanyName,
            HasReferenceProduct = d.ReferenceProductId != null,
            CreatedAt = d.CreatedAt,
            ModificationNotes = d.ModificationNotes,
            ReferenceProductId = d.ReferenceProductId,
            ReferenceProductName = d.ReferenceProduct?.Name,
            ReferenceProductImage = d.ReferenceProduct?.Images.FirstOrDefault(i => i.IsMain)?.Url ?? d.ReferenceProduct?.Images.FirstOrDefault()?.Url,
            MetaTitle = d.MetaTitle,
            MetaDescription = d.MetaDescription,
            IsIndexable = d.IsIndexable,
            Modifications = d.Modifications.Select(m => new DemandModificationDto
            {
                Id = m.Id,
                PropertyName = m.PropertyName,
                OriginalValue = m.OriginalValue,
                DesiredValue = m.DesiredValue,
                Notes = m.Notes,
                DisplayOrder = m.DisplayOrder
            }).ToList(),
            Attachments = d.Attachments.Select(a => new DemandAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FilePath = a.FilePath,
                MimeType = a.MimeType,
                FileSize = a.FileSize,
                Title = a.Title,
                Description = a.Description,
                DisplayOrder = a.DisplayOrder
            }).ToList()
        };

        if (includeResponses && d.Responses != null)
        {
            dto.Responses = d.Responses.Select(r => new DemandResponseListDto
            {
                Id = r.Id,
                DemandId = r.DemandId,
                SupplierVendorId = r.SupplierVendorId,
                SupplierName = r.SupplierVendor?.CompanyName,
                IsExternalSupplier = r.SupplierVendorId == null,
                ExternalCompanyName = r.ExternalCompanyName,
                UnitPrice = r.UnitPrice,
                TotalPrice = r.TotalPrice,
                Currency = r.Currency,
                Quantity = r.Quantity,
                Unit = r.Unit,
                LeadTimeDays = r.LeadTimeDays,
                ValidUntil = r.ValidUntil,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        return dto;
    }
}
