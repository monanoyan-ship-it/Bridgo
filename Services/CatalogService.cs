using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Catalog;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Public urun katalogu servisi (login gerektirmez)
/// </summary>
public class CatalogService : ICatalogService
{
    private readonly ApplicationDbContext _context;

    public CatalogService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================
    // PRODUCTS
    // ============================================

    public async Task<CatalogSearchResultDto> GetProductsAsync(CatalogFilterDto filter)
    {
        // Kategori secilmisse alt kategorileri de dahil et
        if (filter.CategoryId.HasValue && filter.CategoryIds == null)
        {
            filter.CategoryIds = await GetCategoryWithChildrenIdsAsync(filter.CategoryId.Value);
        }

        var query = GetBaseProductQuery();

        // Filtreleme (attribute filtresi HARIC - facet hesaplamasi icin)
        var filterWithoutAttributes = new CatalogFilterDto
        {
            Search = filter.Search,
            CategoryId = filter.CategoryId,
            CategoryIds = filter.CategoryIds,
            VendorId = filter.VendorId,
            MinPrice = filter.MinPrice,
            MaxPrice = filter.MaxPrice,
            Currency = filter.Currency,
            InStock = filter.InStock,
            IsFeatured = filter.IsFeatured,
            CountryOfOrigin = filter.CountryOfOrigin,
            SortBy = filter.SortBy,
            Page = filter.Page,
            PageSize = filter.PageSize
            // AttributeValueIds dahil DEGIL
        };

        var queryForFacets = ApplyFilters(query, filterWithoutAttributes);

        // Facet hesapla (attribute filtresi olmadan)
        var facets = await GetFacetsAsync(queryForFacets, filter.CategoryIds);

        // Tam filtreleme (attribute filtresi dahil)
        query = ApplyFilters(query, filter);

        // Toplam sayi
        var totalCount = await query.CountAsync();

        // Fiyat araligi
        decimal? minPrice = null;
        decimal? maxPrice = null;
        if (totalCount > 0)
        {
            minPrice = await query.MinAsync(p => p.Price);
            maxPrice = await query.MaxAsync(p => p.Price);
        }

        // Siralama
        query = ApplySorting(query, filter.SortBy);

        // Sayfalama
        var products = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => MapToListDto(p))
            .ToListAsync();

        return new CatalogSearchResultDto
        {
            Products = products,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize),
            AppliedFilters = filter,
            MinPriceInResults = minPrice,
            MaxPriceInResults = maxPrice,
            Facets = facets
        };
    }

    public async Task<CatalogProductDetailDto?> GetProductBySlugAsync(string slug, bool incrementViewCount = false)
    {
        // GetBaseProductQuery zaten Images ve PriceTiers include ediyor
        var product = await GetBaseProductQuery()
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (product == null) return null;

        var detail = await MapToDetailDto(product);

        // Benzer urunler
        detail.RelatedProducts = await GetRelatedProductsAsync(product.Id, 8);

        return detail;
    }

    public async Task<CatalogProductDetailDto?> GetProductByIdAsync(int id)
    {
        // GetBaseProductQuery zaten Images ve PriceTiers include ediyor
        var product = await GetBaseProductQuery()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return null;

        var detail = await MapToDetailDto(product);
        detail.RelatedProducts = await GetRelatedProductsAsync(product.Id, 8);

        return detail;
    }

    public async Task<CatalogSearchResultDto> GetProductsByCategoryAsync(string categorySlug, CatalogFilterDto filter)
    {
        // Kategoriyi bul
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Slug == categorySlug && c.IsActive && !c.IsDeleted);

        if (category == null)
        {
            return new CatalogSearchResultDto { Products = new(), TotalCount = 0, Page = 1, PageSize = filter.PageSize };
        }

        // Alt kategorileri de dahil et
        var categoryIds = await GetCategoryWithChildrenIdsAsync(category.Id);
        filter.CategoryId = category.Id;
        filter.CategoryIds = categoryIds;

        var baseQuery = GetBaseProductQuery()
            .Where(p => p.CategoryId != null && categoryIds.Contains(p.CategoryId.Value));

        // Facet hesaplamasi icin attribute filtresi haric
        var filterWithoutAttributes = new CatalogFilterDto
        {
            Search = filter.Search,
            VendorId = filter.VendorId,
            MinPrice = filter.MinPrice,
            MaxPrice = filter.MaxPrice,
            Currency = filter.Currency,
            InStock = filter.InStock,
            IsFeatured = filter.IsFeatured,
            CountryOfOrigin = filter.CountryOfOrigin
        };

        var queryForFacets = ApplyFilters(baseQuery, filterWithoutAttributes, skipCategory: true);
        var facets = await GetFacetsAsync(queryForFacets, categoryIds);

        // Tam filtreleme
        var query = ApplyFilters(baseQuery, filter, skipCategory: true);

        var totalCount = await query.CountAsync();

        // Fiyat araligi
        decimal? minPrice = null;
        decimal? maxPrice = null;
        if (totalCount > 0)
        {
            minPrice = await query.MinAsync(p => p.Price);
            maxPrice = await query.MaxAsync(p => p.Price);
        }

        query = ApplySorting(query, filter.SortBy);

        var products = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => MapToListDto(p))
            .ToListAsync();

        return new CatalogSearchResultDto
        {
            Products = products,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize),
            AppliedFilters = filter,
            MinPriceInResults = minPrice,
            MaxPriceInResults = maxPrice,
            Facets = facets
        };
    }

    public async Task<CatalogSearchResultDto> GetProductsByVendorAsync(string vendorSlug, CatalogFilterDto filter)
    {
        // Satici bul
        var supplierProfile = await _context.Set<SupplierProfile>()
            .FirstOrDefaultAsync(s => s.Slug == vendorSlug && s.IsPubliclyVisible && !s.IsDeleted);

        if (supplierProfile == null)
        {
            return new CatalogSearchResultDto { Products = new(), TotalCount = 0, Page = 1, PageSize = filter.PageSize };
        }

        filter.VendorId = supplierProfile.VendorId;

        return await GetProductsAsync(filter);
    }

    public async Task<List<CatalogProductListDto>> GetRelatedProductsAsync(int productId, int count = 8)
    {
        var product = await _context.Products
            .Where(p => p.Id == productId && !p.IsDeleted)
            .Select(p => new { p.CategoryId, p.VendorId, p.Tags })
            .FirstOrDefaultAsync();

        if (product == null) return new();

        var query = GetBaseProductQuery()
            .Where(p => p.Id != productId);

        // Oncelik: Ayni kategori
        if (product.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == product.CategoryId);
        }

        return await query
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.PublishedAt)
            .Take(count)
            .Select(p => MapToListDto(p))
            .ToListAsync();
    }

    public async Task<CatalogFeaturedDto> GetFeaturedAsync()
    {
        var baseQuery = GetBaseProductQuery();

        // One cikan urunler
        var featured = await baseQuery
            .Where(p => p.IsFeatured)
            .OrderByDescending(p => p.PublishedAt)
            .Take(8)
            .Select(p => MapToListDto(p))
            .ToListAsync();

        // Yeni urunler
        var newProducts = await baseQuery
            .OrderByDescending(p => p.PublishedAt)
            .Take(12)
            .Select(p => MapToListDto(p))
            .ToListAsync();

        // Populer kategoriler (en cok urun olan)
        var popularCategories = await _context.ProductCategories
            .Where(c => c.IsActive && !c.IsDeleted && c.ParentId == null)
            .Select(c => new CatalogCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug ?? "",
                Icon = c.Icon,
                ImageUrl = c.ImageUrl,
                ProductCount = _context.Products.Count(p =>
                    p.CategoryId == c.Id &&
                    p.ProductStatusId == ProductStatuses.Active.Id &&
                    !p.IsDeleted),
                Level = c.Level
            })
            .OrderByDescending(c => c.ProductCount)
            .Take(8)
            .ToListAsync();

        return new CatalogFeaturedDto
        {
            FeaturedProducts = featured,
            NewProducts = newProducts,
            PopularCategories = popularCategories
        };
    }

    // ============================================
    // CATEGORIES
    // ============================================

    public async Task<List<CatalogCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _context.ProductCategories
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.Level)
            .ThenBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        // Urun sayilarini hesapla
        var productCounts = await _context.Products
            .Where(p => p.ProductStatusId == ProductStatuses.Active.Id && !p.IsDeleted && p.CategoryId != null)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId!.Value, x => x.Count);

        // Tree yapisi olustur
        var rootCategories = categories.Where(c => c.ParentId == null).ToList();
        return rootCategories.Select(c => MapCategoryToDto(c, categories, productCounts)).ToList();
    }

    public async Task<CatalogCategoryDto?> GetCategoryBySlugAsync(string slug)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive && !c.IsDeleted);

        if (category == null) return null;

        var productCount = await _context.Products
            .CountAsync(p => p.CategoryId == category.Id && p.ProductStatusId == ProductStatuses.Active.Id && !p.IsDeleted);

        return new CatalogCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug ?? "",
            Icon = category.Icon,
            ImageUrl = category.ImageUrl,
            ProductCount = productCount,
            ParentId = category.ParentId,
            Level = category.Level
        };
    }

    public async Task<List<CategoryBreadcrumbDto>> GetCategoryBreadcrumbAsync(int categoryId)
    {
        var breadcrumb = new List<CategoryBreadcrumbDto>();
        var current = await _context.ProductCategories.FindAsync(categoryId);

        while (current != null)
        {
            breadcrumb.Insert(0, new CategoryBreadcrumbDto
            {
                Id = current.Id,
                Name = current.Name,
                Slug = current.Slug ?? ""
            });

            if (current.ParentId.HasValue)
            {
                current = await _context.ProductCategories.FindAsync(current.ParentId.Value);
            }
            else
            {
                current = null;
            }
        }

        return breadcrumb;
    }

    // ============================================
    // STATS
    // ============================================

    public async Task<CatalogStatsDto> GetStatsAsync()
    {
        var activeProductsQuery = _context.Products
            .Where(p => p.ProductStatusId == ProductStatuses.Active.Id && !p.IsDeleted);

        return new CatalogStatsDto
        {
            TotalProducts = await activeProductsQuery.CountAsync(),
            TotalCategories = await _context.ProductCategories.CountAsync(c => c.IsActive && !c.IsDeleted),
            TotalVendors = await activeProductsQuery.Select(p => p.VendorId).Distinct().CountAsync(),
            FeaturedProducts = await activeProductsQuery.CountAsync(p => p.IsFeatured)
        };
    }

    // ============================================
    // SEARCH SUGGESTIONS
    // ============================================

    public async Task<List<string>> GetSearchSuggestionsAsync(string query, int count = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new();

        var searchTerm = query.ToLower();

        // Urun isimlerinden oneriler
        var productNames = await _context.Products
            .Where(p => p.ProductStatusId == ProductStatuses.Active.Id && !p.IsDeleted)
            .Where(p => p.Name.ToLower().Contains(searchTerm))
            .Select(p => p.Name)
            .Distinct()
            .Take(count)
            .ToListAsync();

        // Kategori isimlerinden oneriler
        var categoryNames = await _context.ProductCategories
            .Where(c => c.IsActive && !c.IsDeleted)
            .Where(c => c.Name.ToLower().Contains(searchTerm))
            .Select(c => c.Name)
            .Distinct()
            .Take(count / 2)
            .ToListAsync();

        return productNames.Concat(categoryNames).Distinct().Take(count).ToList();
    }

    // ============================================
    // PRIVATE HELPERS
    // ============================================

    private IQueryable<Product> GetBaseProductQuery()
    {
        return _context.Products
            .Include(p => p.Vendor)
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder))
            .Include(p => p.PriceTiers.OrderBy(pt => pt.MinQuantity))
            .Include(p => p.Packagings.Where(pkg => !pkg.IsDeleted && pkg.IsActive).OrderBy(pkg => pkg.DisplayOrder).ThenBy(pkg => pkg.UnitId))
            .Include(p => p.AttributeMappings.Where(am => !am.IsDeleted))
                .ThenInclude(am => am.AttributeValue)
            .Where(p => p.ProductStatusId == ProductStatuses.Active.Id && !p.IsDeleted)
            .Where(p => !p.Vendor.IsDeleted); // Aktif saticilar
    }

    private IQueryable<Product> ApplyFilters(IQueryable<Product> query, CatalogFilterDto filter, bool skipCategory = false)
    {
        // Arama
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(searchTerm)) ||
                (p.SKU != null && p.SKU.ToLower().Contains(searchTerm)) ||
                (p.Tags != null && p.Tags.ToLower().Contains(searchTerm)));
        }

        // Kategori (alt kategoriler dahil)
        if (!skipCategory && filter.CategoryIds != null && filter.CategoryIds.Count > 0)
        {
            query = query.Where(p => p.CategoryId != null && filter.CategoryIds.Contains(p.CategoryId.Value));
        }
        else if (!skipCategory && filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId);
        }

        // Satici
        if (filter.VendorId.HasValue)
        {
            query = query.Where(p => p.VendorId == filter.VendorId);
        }

        // Fiyat araligi
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice);
        }
        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice);
        }

        // Stok durumu
        if (filter.InStock == true)
        {
            query = query.Where(p => p.StockQuantity > 0 || p.AllowBackorder);
        }

        // One cikan
        if (filter.IsFeatured == true)
        {
            query = query.Where(p => p.IsFeatured);
        }

        // Mensei ulke
        if (!string.IsNullOrWhiteSpace(filter.CountryOfOrigin))
        {
            query = query.Where(p => p.CountryOfOrigin == filter.CountryOfOrigin);
        }

        // Ozellik filtreleri (faceted filtering)
        if (filter.AttributeValueIds != null && filter.AttributeValueIds.Count > 0)
        {
            query = query.Where(p =>
                p.AttributeMappings.Any(am =>
                    am.AttributeValueId.HasValue &&
                    filter.AttributeValueIds.Contains(am.AttributeValueId.Value)));
        }

        return query;
    }

    private IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortBy)
    {
        return sortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name" => query.OrderBy(p => p.Name),
            "popular" => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.PublishedAt),
            "newest" or _ => query.OrderByDescending(p => p.PublishedAt).ThenByDescending(p => p.CreatedAt)
        };
    }

    private static CatalogProductListDto MapToListDto(Product p)
    {
        var mainImage = p.Images.FirstOrDefault(i => i.IsMain) ?? p.Images.FirstOrDefault();
        var minTier = p.PriceTiers.FirstOrDefault();

        return new CatalogProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug ?? "",
            ShortDescription = p.ShortDescription,
            Price = p.Price,
            CompareAtPrice = p.CompareAtPrice,
            Currency = p.Currency,
            InStock = p.StockQuantity > 0 || p.AllowBackorder,
            StockQuantity = p.StockQuantity,
            IsFeatured = p.IsFeatured,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
            CategorySlug = p.Category?.Slug,
            VendorId = p.VendorId,
            VendorName = p.Vendor.CompanyName,
            MainImageUrl = mainImage?.Url,
            MinBulkPrice = minTier?.Price,
            MinBulkQuantity = minTier?.MinQuantity
        };
    }

    private async Task<CatalogProductDetailDto> MapToDetailDto(Product p)
    {
        var categoryBreadcrumb = p.CategoryId.HasValue
            ? await GetCategoryBreadcrumbAsync(p.CategoryId.Value)
            : new List<CategoryBreadcrumbDto>();

        // Satici bilgisi
        var supplierProfile = await _context.Set<SupplierProfile>()
            .Include(s => s.Country)
            .FirstOrDefaultAsync(s => s.VendorId == p.VendorId && !s.IsDeleted);

        var tags = !string.IsNullOrWhiteSpace(p.Tags)
            ? p.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList()
            : new List<string>();

        return new CatalogProductDetailDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug ?? "",
            SKU = p.SKU,
            Description = p.Description,
            ShortDescription = p.ShortDescription,
            Price = p.Price,
            CompareAtPrice = p.CompareAtPrice,
            Currency = p.Currency,
            IsFeatured = p.IsFeatured,
            InStock = p.StockQuantity > 0 || p.AllowBackorder,
            StockQuantity = p.StockQuantity,
            AllowBackorder = p.AllowBackorder,
            Weight = p.Weight,
            Length = p.Length,
            Width = p.Width,
            Height = p.Height,
            HSCode = p.HSCode,
            CountryOfOrigin = p.CountryOfOrigin,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
            CategorySlug = p.Category?.Slug,
            CategoryBreadcrumb = categoryBreadcrumb,
            VendorId = p.VendorId,
            VendorName = supplierProfile?.DisplayName ?? p.Vendor.CompanyName,
            VendorSlug = supplierProfile?.Slug,
            VendorCountry = supplierProfile?.Country?.Name,
            VendorCity = supplierProfile?.City,
            VendorIsVerified = supplierProfile?.IsVerified ?? false,
            Images = p.Images.Select(i => new CatalogProductImageDto
            {
                Id = i.Id,
                Url = i.Url,
                AltText = i.AltText,
                IsMain = i.IsMain,
                DisplayOrder = i.DisplayOrder
            }).ToList(),
            PriceTiers = p.PriceTiers.Select(pt => new CatalogPriceTierDto
            {
                MinQuantity = pt.MinQuantity,
                MaxQuantity = pt.MaxQuantity,
                Price = pt.Price,
                Description = pt.Description,
                DiscountPercent = p.Price > 0 ? Math.Round((1 - (pt.Price / p.Price)) * 100, 1) : 0
            }).ToList(),
            Packagings = p.Packagings.Select(pkg =>
            {
                var unit = UnitTypes.GetById(pkg.UnitId);
                return new CatalogPackagingDto
                {
                    Id = pkg.Id,
                    UnitId = pkg.UnitId,
                    UnitName = unit?.Description ?? unit?.SystemName ?? "Bilinmiyor",
                    UnitResourceKey = unit?.NameResourceKey ?? "UnitType.Piece",
                    UnitIcon = unit?.Icon,
                    Quantity = pkg.Quantity,
                    IsDefault = pkg.IsDefault,
                    MinOrderQuantity = pkg.MinOrderQuantity
                };
            }).ToList(),
            SalesUnit = p.Packagings.Where(pkg => pkg.IsDefault).Select(pkg =>
            {
                var unit = UnitTypes.GetById(pkg.UnitId);
                return new CatalogPackagingDto
                {
                    Id = pkg.Id,
                    UnitId = pkg.UnitId,
                    UnitName = unit?.Description ?? unit?.SystemName ?? "Bilinmiyor",
                    UnitResourceKey = unit?.NameResourceKey ?? "UnitType.Piece",
                    UnitIcon = unit?.Icon,
                    Quantity = pkg.Quantity,
                    IsDefault = pkg.IsDefault,
                    MinOrderQuantity = pkg.MinOrderQuantity
                };
            }).FirstOrDefault(),
            MetaTitle = p.MetaTitle,
            MetaDescription = p.MetaDescription,
            Tags = tags,
            PublishedAt = p.PublishedAt
        };
    }

    private CatalogCategoryDto MapCategoryToDto(ProductCategory category, List<ProductCategory> allCategories, Dictionary<int, int> productCounts)
    {
        var children = allCategories.Where(c => c.ParentId == category.Id).ToList();

        // Alt kategorilerin urun sayilarini da dahil et
        int totalCount = productCounts.GetValueOrDefault(category.Id, 0);
        foreach (var child in children)
        {
            totalCount += GetCategoryProductCountRecursive(child.Id, allCategories, productCounts);
        }

        return new CatalogCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug ?? "",
            Icon = category.Icon,
            ImageUrl = category.ImageUrl,
            ProductCount = totalCount,
            ParentId = category.ParentId,
            Level = category.Level,
            Children = children.Select(c => MapCategoryToDto(c, allCategories, productCounts)).ToList()
        };
    }

    private int GetCategoryProductCountRecursive(int categoryId, List<ProductCategory> allCategories, Dictionary<int, int> productCounts)
    {
        int count = productCounts.GetValueOrDefault(categoryId, 0);
        var children = allCategories.Where(c => c.ParentId == categoryId).ToList();
        foreach (var child in children)
        {
            count += GetCategoryProductCountRecursive(child.Id, allCategories, productCounts);
        }
        return count;
    }

    private async Task<List<int>> GetCategoryWithChildrenIdsAsync(int categoryId)
    {
        var ids = new List<int> { categoryId };
        var childIds = await _context.ProductCategories
            .Where(c => c.ParentId == categoryId && c.IsActive && !c.IsDeleted)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var childId in childIds)
        {
            ids.AddRange(await GetCategoryWithChildrenIdsAsync(childId));
        }

        return ids;
    }

    /// <summary>
    /// Mevcut filtreler icin facet degerlerini hesapla
    /// </summary>
    private async Task<List<CatalogFacetDto>> GetFacetsAsync(IQueryable<Product> filteredQuery, List<int>? categoryIds)
    {
        var facets = new List<CatalogFacetDto>();

        // Filtrelenebilir ozellikleri getir
        var attributesQuery = _context.ProductAttributes
            .Where(a => a.IsFilterable && a.IsActive && !a.IsDeleted);

        // Kategori bazli veya global ozellikler
        if (categoryIds != null && categoryIds.Count > 0)
        {
            attributesQuery = attributesQuery.Where(a =>
                a.CategoryId == null || categoryIds.Contains(a.CategoryId.Value));
        }
        else
        {
            // Kategori secilmemisse sadece global ozellikler
            attributesQuery = attributesQuery.Where(a => a.CategoryId == null);
        }

        var attributes = await attributesQuery
            .Include(a => a.Values.Where(v => v.IsActive && !v.IsDeleted))
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.Name)
            .ToListAsync();

        // Filtrelenmis urunlerin ID'lerini al
        var productIds = await filteredQuery.Select(p => p.Id).ToListAsync();

        if (productIds.Count == 0)
            return facets;

        // Her ozellik icin deger sayilarini hesapla
        foreach (var attr in attributes)
        {
            if (attr.Values.Count == 0)
                continue;

            // Bu ozelligin degerlerinin urun sayilarini hesapla
            var valueCounts = await _context.ProductAttributeMappings
                .Where(am => !am.IsDeleted &&
                             am.AttributeId == attr.Id &&
                             am.AttributeValueId.HasValue &&
                             productIds.Contains(am.ProductId))
                .GroupBy(am => am.AttributeValueId)
                .Select(g => new { ValueId = g.Key, Count = g.Select(x => x.ProductId).Distinct().Count() })
                .ToListAsync();

            if (valueCounts.Count == 0)
                continue;

            var facetValues = attr.Values
                .Select(v =>
                {
                    var countInfo = valueCounts.FirstOrDefault(vc => vc.ValueId == v.Id);
                    return new CatalogFacetValueDto
                    {
                        ValueId = v.Id,
                        Value = v.Value,
                        ValueResourceKey = v.ValueResourceKey,
                        AdditionalData = v.AdditionalData,
                        ProductCount = countInfo?.Count ?? 0
                    };
                })
                .Where(fv => fv.ProductCount > 0)
                .OrderByDescending(fv => fv.ProductCount)
                .ThenBy(fv => fv.Value)
                .ToList();

            if (facetValues.Count > 0)
            {
                facets.Add(new CatalogFacetDto
                {
                    AttributeId = attr.Id,
                    AttributeName = attr.Name,
                    AttributeNameResourceKey = attr.NameResourceKey,
                    Icon = attr.Icon,
                    DisplayOrder = attr.DisplayOrder,
                    Values = facetValues
                });
            }
        }

        return facets;
    }
}
