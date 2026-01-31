using Microsoft.EntityFrameworkCore;
using Bridgo.DTOs.Category;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Repositories;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class CategoryRequestService : ICategoryRequestService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryRequestService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ========== Kullanici islemleri ==========

    public async Task<List<CategoryRequestDto>> GetVendorRequestsAsync(int vendorId, int? statusId = null)
    {
        var query = _unitOfWork.CategoryRequests.Query()
            .Include(r => r.SuggestedParentCategory)
            .Include(r => r.RequestedByUser)
            .Include(r => r.ReviewedByUser)
            .Include(r => r.Vendor)
            .Where(r => r.VendorId == vendorId && !r.IsDeleted);

        if (statusId.HasValue)
        {
            query = query.Where(r => r.StatusId == statusId.Value);
        }

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto).ToList();
    }

    public async Task<CategoryRequestDto?> GetByIdAsync(int id, int? vendorId = null)
    {
        var query = _unitOfWork.CategoryRequests.Query()
            .Include(r => r.SuggestedParentCategory)
            .Include(r => r.RequestedByUser)
            .Include(r => r.ReviewedByUser)
            .Include(r => r.Vendor)
            .Where(r => r.Id == id && !r.IsDeleted);

        if (vendorId.HasValue)
        {
            query = query.Where(r => r.VendorId == vendorId.Value);
        }

        var request = await query.FirstOrDefaultAsync();
        return request == null ? null : MapToDto(request);
    }

    public async Task<CategoryRequestDto> CreateAsync(CategoryRequestCreateDto dto, int vendorId, int userId)
    {
        var request = new CategoryRequest
        {
            RequestedName = dto.RequestedName.Trim(),
            Description = dto.Description?.Trim(),
            SuggestedParentCategoryId = dto.SuggestedParentCategoryId,
            StatusId = CategoryRequestStatuses.Pending.Id,
            VendorId = vendorId,
            RequestedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CategoryRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(request.Id) ?? throw new InvalidOperationException("Talep olusturulamadi");
    }

    public async Task<bool> CancelAsync(int id, int vendorId)
    {
        var request = await _unitOfWork.CategoryRequests.Query()
            .FirstOrDefaultAsync(r => r.Id == id && r.VendorId == vendorId && !r.IsDeleted);

        if (request == null)
            return false;

        if (request.StatusId != CategoryRequestStatuses.Pending.Id)
        {
            throw new InvalidOperationException("Sadece bekleyen talepler iptal edilebilir.");
        }

        request.IsDeleted = true;
        request.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.CategoryRequests.Update(request);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<CategorySearchResultDto>> SearchCategoriesAsync(string query, int? parentId = null)
    {
        var normalizedQuery = query.ToLower().Trim();

        var categories = await _unitOfWork.ProductCategories.Query()
            .Where(c => !c.IsDeleted && c.IsActive)
            .Where(c => c.Name.ToLower().Contains(normalizedQuery))
            .OrderBy(c => c.Level)
            .ThenBy(c => c.Name)
            .Take(20)
            .ToListAsync();

        var result = new List<CategorySearchResultDto>();
        foreach (var cat in categories)
        {
            result.Add(new CategorySearchResultDto
            {
                Id = cat.Id,
                Name = cat.Name,
                FullPath = await GetCategoryPathAsync(cat.Id),
                Level = cat.Level,
                HasChildren = await _unitOfWork.ProductCategories.Query()
                    .AnyAsync(c => c.ParentId == cat.Id && !c.IsDeleted)
            });
        }

        return result;
    }

    // ========== Admin islemleri ==========

    public async Task<List<CategoryRequestDto>> GetAllRequestsAsync(int? statusId = null)
    {
        var query = _unitOfWork.CategoryRequests.Query()
            .Include(r => r.SuggestedParentCategory)
            .Include(r => r.RequestedByUser)
            .Include(r => r.ReviewedByUser)
            .Include(r => r.Vendor)
            .Where(r => !r.IsDeleted);

        if (statusId.HasValue)
        {
            query = query.Where(r => r.StatusId == statusId.Value);
        }

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto).ToList();
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _unitOfWork.CategoryRequests.Query()
            .CountAsync(r => r.StatusId == CategoryRequestStatuses.Pending.Id && !r.IsDeleted);
    }

    public async Task<ServiceResult<CategoryRequestDto>> ReviewRequestAsync(CategoryRequestReviewDto dto, int reviewedByUserId)
    {
        var request = await _unitOfWork.CategoryRequests.Query()
            .Include(r => r.SuggestedParentCategory)
            .FirstOrDefaultAsync(r => r.Id == dto.Id && !r.IsDeleted);

        if (request == null)
            return ServiceResult<CategoryRequestDto>.Fail("Talep bulunamadi.");

        if (request.StatusId != CategoryRequestStatuses.Pending.Id)
            return ServiceResult<CategoryRequestDto>.Fail("Bu talep zaten incelenmis.");

        request.StatusId = dto.StatusId;
        request.ReviewNote = dto.ReviewNote?.Trim();
        request.ReviewedByUserId = reviewedByUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        // Onay durumunda yeni kategori olustur
        if (dto.StatusId == CategoryRequestStatuses.Approved.Id)
        {
            var categoryName = string.IsNullOrWhiteSpace(dto.ApprovedName)
                ? request.RequestedName
                : dto.ApprovedName.Trim();

            var parentId = dto.ParentCategoryId ?? request.SuggestedParentCategoryId;

            // Ayni isimde kategori var mi kontrol et
            var existingCategory = await _unitOfWork.ProductCategories.Query()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower()
                    && c.ParentId == parentId && !c.IsDeleted);

            if (existingCategory != null)
            {
                request.StatusId = CategoryRequestStatuses.Duplicate.Id;
                request.ReviewNote = $"Ayni isimde kategori zaten mevcut: {existingCategory.Name}";
                request.CreatedCategoryId = existingCategory.Id;
            }
            else
            {
                // Yeni kategori olustur
                var parentCategory = parentId.HasValue
                    ? await _unitOfWork.ProductCategories.Query()
                        .FirstOrDefaultAsync(c => c.Id == parentId.Value && !c.IsDeleted)
                    : null;

                var newCategory = new ProductCategory
                {
                    Name = categoryName,
                    Slug = GenerateSlug(categoryName),
                    ParentId = parentId,
                    Level = parentCategory != null ? parentCategory.Level + 1 : 0,
                    DisplayOrder = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ProductCategories.AddAsync(newCategory);
                await _unitOfWork.SaveChangesAsync();

                request.CreatedCategoryId = newCategory.Id;
            }
        }

        _unitOfWork.CategoryRequests.Update(request);
        await _unitOfWork.SaveChangesAsync();

        var result = await GetByIdAsync(request.Id);
        return ServiceResult<CategoryRequestDto>.Ok(result!);
    }

    public async Task<List<CategorySearchResultDto>> FindSimilarCategoriesAsync(string name)
    {
        var normalizedName = name.ToLower().Trim();

        // Benzer isimli kategorileri bul (contains veya soundex benzeri)
        var categories = await _unitOfWork.ProductCategories.Query()
            .Where(c => !c.IsDeleted && c.IsActive)
            .Where(c => c.Name.ToLower().Contains(normalizedName)
                     || normalizedName.Contains(c.Name.ToLower()))
            .OrderBy(c => c.Level)
            .ThenBy(c => c.Name)
            .Take(10)
            .ToListAsync();

        var result = new List<CategorySearchResultDto>();
        foreach (var cat in categories)
        {
            result.Add(new CategorySearchResultDto
            {
                Id = cat.Id,
                Name = cat.Name,
                FullPath = await GetCategoryPathAsync(cat.Id),
                Level = cat.Level,
                HasChildren = await _unitOfWork.ProductCategories.Query()
                    .AnyAsync(c => c.ParentId == cat.Id && !c.IsDeleted)
            });
        }

        return result;
    }

    // ========== Helper metodlar ==========

    private async Task<string> GetCategoryPathAsync(int categoryId)
    {
        var path = new List<string>();
        var currentId = categoryId as int?;

        while (currentId.HasValue)
        {
            var category = await _unitOfWork.ProductCategories.Query()
                .FirstOrDefaultAsync(c => c.Id == currentId.Value && !c.IsDeleted);

            if (category == null) break;

            path.Insert(0, category.Name);
            currentId = category.ParentId;
        }

        return string.Join(" > ", path);
    }

    private string GenerateSlug(string name)
    {
        var slug = name.ToLower()
            .Replace(" ", "-")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ö", "o")
            .Replace("ç", "c");

        // Sadece alfanumerik ve tire karakterlerini birak
        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        return slug;
    }

    private CategoryRequestDto MapToDto(CategoryRequest request)
    {
        var status = CategoryRequestStatuses.GetById(request.StatusId);
        return new CategoryRequestDto
        {
            Id = request.Id,
            RequestedName = request.RequestedName,
            Description = request.Description,
            SuggestedParentCategoryId = request.SuggestedParentCategoryId,
            SuggestedParentCategoryName = request.SuggestedParentCategory?.Name,
            StatusId = request.StatusId,
            StatusName = status?.NameResourceKey ?? "Unknown",
            StatusCssClass = status?.CssClass ?? "bg-secondary",
            VendorId = request.VendorId,
            VendorName = request.Vendor?.CompanyName ?? "",
            RequestedByUserId = request.RequestedByUserId,
            RequestedByUserName = request.RequestedByUser != null
                ? $"{request.RequestedByUser.FirstName} {request.RequestedByUser.LastName}"
                : "",
            ReviewedByUserId = request.ReviewedByUserId,
            ReviewedByUserName = request.ReviewedByUser != null
                ? $"{request.ReviewedByUser.FirstName} {request.ReviewedByUser.LastName}"
                : null,
            ReviewNote = request.ReviewNote,
            ReviewedAt = request.ReviewedAt,
            CreatedCategoryId = request.CreatedCategoryId,
            CreatedAt = request.CreatedAt
        };
    }
}
