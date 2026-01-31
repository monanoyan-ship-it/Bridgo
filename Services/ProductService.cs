using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Product;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Repositories;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Urun yonetimi servisi implementation
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly ILocalizationService _localizationService;

    public ProductService(IUnitOfWork unitOfWork, ApplicationDbContext context, ILocalizationService localizationService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _localizationService = localizationService;
    }

    #region Products

    public async Task<List<ProductListDto>> GetProductsAsync(int vendorId, ProductFilterDto? filter = null)
    {
        var query = _unitOfWork.Products.Query()
            .Where(p => p.VendorId == vendorId && !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsMain))
            .AsQueryable();

        // Filtreleme
        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    (p.SKU != null && p.SKU.ToLower().Contains(search)) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(search)));
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            if (filter.ProductStatusId.HasValue)
                query = query.Where(p => p.ProductStatusId == filter.ProductStatusId.Value);

            if (filter.IsFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            if (filter.InStock.HasValue)
            {
                if (filter.InStock.Value)
                    query = query.Where(p => p.StockQuantity > 0);
                else
                    query = query.Where(p => p.StockQuantity <= 0);
            }

            // Siralama
            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => filter.SortDesc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "stock" => filter.SortDesc ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
                "created" => filter.SortDesc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // Pagination
            query = query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);
        }
        else
        {
            query = query.OrderByDescending(p => p.CreatedAt);
        }

        return await query.Select(p => new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            Barcode = p.Barcode,
            Price = p.Price,
            Currency = p.Currency,
            StockQuantity = p.StockQuantity,
            ProductStatusId = p.ProductStatusId,
            IsFeatured = p.IsFeatured,
            CategoryId = p.CategoryId,
            CategoryName = p.Category != null ? p.Category.Name : null,
            MainImageUrl = p.Images.FirstOrDefault(i => i.IsMain) != null
                ? p.Images.First(i => i.IsMain).Url
                : p.Images.FirstOrDefault() != null ? p.Images.First().Url : null,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToListAsync();
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int id, int vendorId)
    {
        var product = await _unitOfWork.Products.Query()
            .Where(p => p.Id == id && p.VendorId == vendorId && !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.PriceTiers.OrderBy(t => t.MinQuantity))
            .FirstOrDefaultAsync();

        if (product == null)
            return null;

        return new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            Barcode = product.Barcode,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            Price = product.Price,
            CompareAtPrice = product.CompareAtPrice,
            CostPrice = product.CostPrice,
            Currency = product.Currency,
            StockQuantity = product.StockQuantity,
            MinStockLevel = product.MinStockLevel,
            TrackStock = product.TrackStock,
            AllowBackorder = product.AllowBackorder,
            SalesUnitId = product.SalesUnitId,
            SalesUnitName = product.SalesUnitId.HasValue
                ? _localizationService.GetResource(UnitTypes.GetById(product.SalesUnitId.Value)?.NameResourceKey ?? "", UnitTypes.GetById(product.SalesUnitId.Value)?.Description ?? "Adet")
                : null,
            Weight = product.Weight,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            Tags = product.Tags,
            DisplayOrder = product.DisplayOrder,
            ProductStatusId = product.ProductStatusId,
            IsFeatured = product.IsFeatured,
            PublishedAt = product.PublishedAt,
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            Slug = product.Slug,
            Images = product.Images.Select(i => new ProductImageDto
            {
                Id = i.Id,
                Url = i.Url,
                AltText = i.AltText,
                Title = i.Title,
                DisplayOrder = i.DisplayOrder,
                IsMain = i.IsMain
            }).ToList(),
            PriceTiers = product.PriceTiers.Select(t => new ProductPriceTierDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                MinQuantity = t.MinQuantity,
                MaxQuantity = t.MaxQuantity,
                Price = t.Price,
                Description = t.Description
            }).ToList(),
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    public async Task<ServiceResult<int>> CreateProductAsync(ProductCreateUpdateDto dto, int vendorId)
    {
        // Validasyon
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ServiceResult<int>.Fail("Urun adi zorunludur");

        // SKU uniq kontrol
        if (!string.IsNullOrWhiteSpace(dto.SKU))
        {
            var skuExists = await _unitOfWork.Products.AnyAsync(p =>
                p.VendorId == vendorId && p.SKU == dto.SKU && !p.IsDeleted);
            if (skuExists)
                return ServiceResult<int>.Fail("Bu SKU zaten kullaniliyor");
        }

        // Barcode uniq kontrol
        if (!string.IsNullOrWhiteSpace(dto.Barcode))
        {
            var barcodeExists = await _unitOfWork.Products.AnyAsync(p =>
                p.VendorId == vendorId && p.Barcode == dto.Barcode && !p.IsDeleted);
            if (barcodeExists)
                return ServiceResult<int>.Fail("Bu barkod zaten kullaniliyor");
        }

        // Kategori kontrol (global kategoriler)
        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await _unitOfWork.ProductCategories.AnyAsync(c =>
                c.Id == dto.CategoryId.Value && !c.IsDeleted && c.IsActive);
            if (!categoryExists)
                return ServiceResult<int>.Fail("Gecersiz kategori");
        }

        var product = new Product
        {
            Name = dto.Name,
            SKU = dto.SKU,
            Barcode = dto.Barcode,
            Description = dto.Description,
            ShortDescription = dto.ShortDescription,
            Price = dto.Price,
            CompareAtPrice = dto.CompareAtPrice,
            CostPrice = dto.CostPrice,
            Currency = dto.Currency,
            StockQuantity = dto.StockQuantity,
            MinStockLevel = dto.MinStockLevel,
            TrackStock = dto.TrackStock,
            AllowBackorder = dto.AllowBackorder,
            SalesUnitId = dto.SalesUnitId,
            Weight = dto.Weight,
            Length = dto.Length,
            Width = dto.Width,
            Height = dto.Height,
            CategoryId = dto.CategoryId,
            Tags = dto.Tags,
            DisplayOrder = dto.DisplayOrder,
            ProductStatusId = dto.ProductStatusId,
            IsFeatured = dto.IsFeatured,
            MetaTitle = dto.MetaTitle,
            MetaDescription = dto.MetaDescription,
            Slug = GenerateSlug(dto.Name),
            VendorId = vendorId,
            PublishedAt = dto.ProductStatusId == 2 ? DateTime.UtcNow : null  // 2 = Active
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<int>.Ok(product.Id, "Urun olusturuldu");
    }

    public async Task<ServiceResult> UpdateProductAsync(int id, ProductCreateUpdateDto dto, int vendorId)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p =>
            p.Id == id && p.VendorId == vendorId && !p.IsDeleted);

        if (product == null)
            return ServiceResult.Fail("Urun bulunamadi");

        // Validasyon
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ServiceResult.Fail("Urun adi zorunludur");

        // SKU uniq kontrol
        if (!string.IsNullOrWhiteSpace(dto.SKU))
        {
            var skuExists = await _unitOfWork.Products.AnyAsync(p =>
                p.VendorId == vendorId && p.SKU == dto.SKU && p.Id != id && !p.IsDeleted);
            if (skuExists)
                return ServiceResult.Fail("Bu SKU zaten kullaniliyor");
        }

        // Barcode uniq kontrol
        if (!string.IsNullOrWhiteSpace(dto.Barcode))
        {
            var barcodeExists = await _unitOfWork.Products.AnyAsync(p =>
                p.VendorId == vendorId && p.Barcode == dto.Barcode && p.Id != id && !p.IsDeleted);
            if (barcodeExists)
                return ServiceResult.Fail("Bu barkod zaten kullaniliyor");
        }

        // Kategori kontrol (global kategoriler)
        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await _unitOfWork.ProductCategories.AnyAsync(c =>
                c.Id == dto.CategoryId.Value && !c.IsDeleted && c.IsActive);
            if (!categoryExists)
                return ServiceResult.Fail("Gecersiz kategori");
        }

        // Ilk kez aktif oluyorsa PublishedAt set et (2 = Active)
        if (dto.ProductStatusId == 2 && product.ProductStatusId != 2 && !product.PublishedAt.HasValue)
        {
            product.PublishedAt = DateTime.UtcNow;
        }

        product.Name = dto.Name;
        product.SKU = dto.SKU;
        product.Barcode = dto.Barcode;
        product.Description = dto.Description;
        product.ShortDescription = dto.ShortDescription;
        product.Price = dto.Price;
        product.CompareAtPrice = dto.CompareAtPrice;
        product.CostPrice = dto.CostPrice;
        product.Currency = dto.Currency;
        product.StockQuantity = dto.StockQuantity;
        product.MinStockLevel = dto.MinStockLevel;
        product.TrackStock = dto.TrackStock;
        product.AllowBackorder = dto.AllowBackorder;
        product.SalesUnitId = dto.SalesUnitId;
        product.Weight = dto.Weight;
        product.Length = dto.Length;
        product.Width = dto.Width;
        product.Height = dto.Height;
        product.CategoryId = dto.CategoryId;
        product.Tags = dto.Tags;
        product.DisplayOrder = dto.DisplayOrder;
        product.ProductStatusId = dto.ProductStatusId;
        product.IsFeatured = dto.IsFeatured;
        product.MetaTitle = dto.MetaTitle;
        product.MetaDescription = dto.MetaDescription;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok("Urun guncellendi");
    }

    public async Task<ServiceResult> DeleteProductAsync(int id, int vendorId)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p =>
            p.Id == id && p.VendorId == vendorId && !p.IsDeleted);

        if (product == null)
            return ServiceResult.Fail("Urun bulunamadi");

        await _unitOfWork.Products.SoftDeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok("Urun silindi");
    }

    public async Task<ServiceResult> UpdateProductStatusAsync(int id, int productStatusId, int vendorId)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p =>
            p.Id == id && p.VendorId == vendorId && !p.IsDeleted);

        if (product == null)
            return ServiceResult.Fail("Urun bulunamadi");

        // 2 = Active - Ilk kez aktif oluyorsa PublishedAt set et
        if (productStatusId == 2 && product.ProductStatusId != 2 && !product.PublishedAt.HasValue)
        {
            product.PublishedAt = DateTime.UtcNow;
        }

        product.ProductStatusId = productStatusId;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok("Urun durumu guncellendi");
    }

    public async Task<ServiceResult> BulkUpdateStatusAsync(List<int> productIds, int productStatusId, int vendorId)
    {
        var products = await _unitOfWork.Products.Query()
            .Where(p => productIds.Contains(p.Id) && p.VendorId == vendorId && !p.IsDeleted)
            .ToListAsync();

        if (!products.Any())
            return ServiceResult.Fail("Urun bulunamadi");

        foreach (var product in products)
        {
            // 2 = Active - Ilk kez aktif oluyorsa PublishedAt set et
            if (productStatusId == 2 && product.ProductStatusId != 2 && !product.PublishedAt.HasValue)
            {
                product.PublishedAt = DateTime.UtcNow;
            }
            product.ProductStatusId = productStatusId;
            product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Products.Update(product);
        }

        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok($"{products.Count} urun guncellendi");
    }

    public async Task<ServiceResult> BulkDeleteAsync(List<int> productIds, int vendorId)
    {
        var products = await _unitOfWork.Products.Query()
            .Where(p => productIds.Contains(p.Id) && p.VendorId == vendorId && !p.IsDeleted)
            .ToListAsync();

        if (!products.Any())
            return ServiceResult.Fail("Urun bulunamadi");

        foreach (var product in products)
        {
            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Products.Update(product);
        }

        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok($"{products.Count} urun silindi");
    }

    #endregion

    #region Product Images

    public async Task<ServiceResult<int>> AddProductImageAsync(int productId, ProductImageCreateDto dto, int vendorId)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p =>
            p.Id == productId && p.VendorId == vendorId && !p.IsDeleted);

        if (product == null)
            return ServiceResult<int>.Fail("Urun bulunamadi");

        // Eger ilk gorsel ve IsMain belirtilmemisse, ana gorsel yap
        var existingImages = await _unitOfWork.ProductImages.CountAsync(i => i.ProductId == productId);
        var isMain = dto.IsMain || existingImages == 0;

        // Eger yeni gorsel ana gorsel olacaksa, diger ana gorseli kaldir
        if (isMain)
        {
            var currentMainImage = await _unitOfWork.ProductImages.FirstOrDefaultAsync(i =>
                i.ProductId == productId && i.IsMain);
            if (currentMainImage != null)
            {
                currentMainImage.IsMain = false;
                _unitOfWork.ProductImages.Update(currentMainImage);
            }
        }

        var image = new ProductImage
        {
            ProductId = productId,
            Url = dto.Url,
            AltText = dto.AltText ?? product.Name,
            Title = dto.Title ?? product.Name,
            DisplayOrder = dto.DisplayOrder,
            IsMain = isMain
        };

        await _unitOfWork.ProductImages.AddAsync(image);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<int>.Ok(image.Id, "Gorsel eklendi");
    }

    public async Task<ServiceResult> DeleteProductImageAsync(int imageId, int vendorId)
    {
        var image = await _unitOfWork.ProductImages.Query()
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == imageId && i.Product.VendorId == vendorId);

        if (image == null)
            return ServiceResult.Fail("Gorsel bulunamadi");

        var wasMain = image.IsMain;
        var productId = image.ProductId;

        _unitOfWork.ProductImages.Remove(image);
        await _unitOfWork.SaveChangesAsync();

        // Eger silinen ana gorselse, baska bir gorseli ana yap
        if (wasMain)
        {
            var nextImage = await _unitOfWork.ProductImages.Query()
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.DisplayOrder)
                .FirstOrDefaultAsync();
            if (nextImage != null)
            {
                nextImage.IsMain = true;
                _unitOfWork.ProductImages.Update(nextImage);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        return ServiceResult.Ok("Gorsel silindi");
    }

    public async Task<ServiceResult> SetMainImageAsync(int imageId, int vendorId)
    {
        var image = await _unitOfWork.ProductImages.Query()
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == imageId && i.Product.VendorId == vendorId);

        if (image == null)
            return ServiceResult.Fail("Gorsel bulunamadi");

        // Mevcut ana gorseli kaldir
        var currentMain = await _unitOfWork.ProductImages.FirstOrDefaultAsync(i =>
            i.ProductId == image.ProductId && i.IsMain);
        if (currentMain != null && currentMain.Id != imageId)
        {
            currentMain.IsMain = false;
            _unitOfWork.ProductImages.Update(currentMain);
        }

        image.IsMain = true;
        _unitOfWork.ProductImages.Update(image);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok("Ana gorsel ayarlandi");
    }

    public async Task<ServiceResult> UpdateImageOrderAsync(int productId, List<int> imageIds, int vendorId)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p =>
            p.Id == productId && p.VendorId == vendorId && !p.IsDeleted);

        if (product == null)
            return ServiceResult.Fail("Urun bulunamadi");

        var images = await _unitOfWork.ProductImages.Query()
            .Where(i => i.ProductId == productId && imageIds.Contains(i.Id))
            .ToListAsync();

        for (int i = 0; i < imageIds.Count; i++)
        {
            var image = images.FirstOrDefault(img => img.Id == imageIds[i]);
            if (image != null)
            {
                image.DisplayOrder = i;
                _unitOfWork.ProductImages.Update(image);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok("Gorsel sirasi guncellendi");
    }

    #endregion

    #region Price Tiers

    public async Task<List<ProductPriceTierDto>> GetPriceTiersAsync(int productId, int vendorId)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p =>
            p.Id == productId && p.VendorId == vendorId && !p.IsDeleted);

        if (product == null)
            return new List<ProductPriceTierDto>();

        return await _context.ProductPriceTiers
            .Where(t => t.ProductId == productId && !t.IsDeleted)
            .OrderBy(t => t.MinQuantity)
            .Select(t => new ProductPriceTierDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                MinQuantity = t.MinQuantity,
                MaxQuantity = t.MaxQuantity,
                Price = t.Price,
                Description = t.Description
            })
            .ToListAsync();
    }

    public async Task<ServiceResult<int>> AddPriceTierAsync(int productId, ProductPriceTierCreateUpdateDto dto, int vendorId)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p =>
            p.Id == productId && p.VendorId == vendorId && !p.IsDeleted);

        if (product == null)
            return ServiceResult<int>.Fail("Urun bulunamadi");

        // Validasyon
        if (dto.MinQuantity < 1)
            return ServiceResult<int>.Fail("Minimum miktar 1'den kucuk olamaz");

        if (dto.MaxQuantity.HasValue && dto.MaxQuantity.Value <= dto.MinQuantity)
            return ServiceResult<int>.Fail("Maksimum miktar, minimum miktardan buyuk olmalidir");

        if (dto.Price <= 0)
            return ServiceResult<int>.Fail("Fiyat 0'dan buyuk olmalidir");

        // Ayni MinQuantity ile baska esik var mi kontrol
        var existsWithSameMin = await _context.ProductPriceTiers.AnyAsync(t =>
            t.ProductId == productId && t.MinQuantity == dto.MinQuantity && !t.IsDeleted);
        if (existsWithSameMin)
            return ServiceResult<int>.Fail("Bu minimum miktar icin zaten bir esik var");

        var tier = new ProductPriceTier
        {
            ProductId = productId,
            MinQuantity = dto.MinQuantity,
            MaxQuantity = dto.MaxQuantity,
            Price = dto.Price,
            Description = dto.Description
        };

        await _context.ProductPriceTiers.AddAsync(tier);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Ok(tier.Id, "Fiyat esigi eklendi");
    }

    public async Task<ServiceResult> UpdatePriceTierAsync(int tierId, ProductPriceTierCreateUpdateDto dto, int vendorId)
    {
        var tier = await _context.ProductPriceTiers
            .Include(t => t.Product)
            .FirstOrDefaultAsync(t => t.Id == tierId && t.Product.VendorId == vendorId && !t.IsDeleted);

        if (tier == null)
            return ServiceResult.Fail("Fiyat esigi bulunamadi");

        // Validasyon
        if (dto.MinQuantity < 1)
            return ServiceResult.Fail("Minimum miktar 1'den kucuk olamaz");

        if (dto.MaxQuantity.HasValue && dto.MaxQuantity.Value <= dto.MinQuantity)
            return ServiceResult.Fail("Maksimum miktar, minimum miktardan buyuk olmalidir");

        if (dto.Price <= 0)
            return ServiceResult.Fail("Fiyat 0'dan buyuk olmalidir");

        // Ayni MinQuantity ile baska esik var mi kontrol (kendi haric)
        var existsWithSameMin = await _context.ProductPriceTiers.AnyAsync(t =>
            t.ProductId == tier.ProductId && t.MinQuantity == dto.MinQuantity && t.Id != tierId && !t.IsDeleted);
        if (existsWithSameMin)
            return ServiceResult.Fail("Bu minimum miktar icin zaten bir esik var");

        tier.MinQuantity = dto.MinQuantity;
        tier.MaxQuantity = dto.MaxQuantity;
        tier.Price = dto.Price;
        tier.Description = dto.Description;
        tier.UpdatedAt = DateTime.UtcNow;

        _context.ProductPriceTiers.Update(tier);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Fiyat esigi guncellendi");
    }

    public async Task<ServiceResult> DeletePriceTierAsync(int tierId, int vendorId)
    {
        var tier = await _context.ProductPriceTiers
            .Include(t => t.Product)
            .FirstOrDefaultAsync(t => t.Id == tierId && t.Product.VendorId == vendorId && !t.IsDeleted);

        if (tier == null)
            return ServiceResult.Fail("Fiyat esigi bulunamadi");

        tier.IsDeleted = true;
        tier.UpdatedAt = DateTime.UtcNow;

        _context.ProductPriceTiers.Update(tier);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Fiyat esigi silindi");
    }

    public async Task<ServiceResult> SavePriceTiersAsync(int productId, List<ProductPriceTierCreateUpdateDto> tiers, int vendorId)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p =>
            p.Id == productId && p.VendorId == vendorId && !p.IsDeleted);

        if (product == null)
            return ServiceResult.Fail("Urun bulunamadi");

        // Validasyon
        foreach (var tier in tiers)
        {
            if (tier.MinQuantity < 1)
                return ServiceResult.Fail("Minimum miktar 1'den kucuk olamaz");

            if (tier.MaxQuantity.HasValue && tier.MaxQuantity.Value <= tier.MinQuantity)
                return ServiceResult.Fail("Maksimum miktar, minimum miktardan buyuk olmalidir");

            if (tier.Price <= 0)
                return ServiceResult.Fail("Fiyat 0'dan buyuk olmalidir");
        }

        // Duplicate MinQuantity kontrolu
        var minQuantities = tiers.Select(t => t.MinQuantity).ToList();
        if (minQuantities.Count != minQuantities.Distinct().Count())
            return ServiceResult.Fail("Ayni minimum miktar icin birden fazla esik tanimlanamaz");

        // Mevcut esikleri getir
        var existingTiers = await _context.ProductPriceTiers
            .Where(t => t.ProductId == productId && !t.IsDeleted)
            .ToListAsync();

        // Guncellenecek, eklenecek ve silinecek esikleri belirle
        var tierIdsToKeep = tiers.Where(t => t.Id.HasValue).Select(t => t.Id!.Value).ToList();

        // Silinecekleri soft delete yap
        foreach (var existing in existingTiers)
        {
            if (!tierIdsToKeep.Contains(existing.Id))
            {
                existing.IsDeleted = true;
                existing.UpdatedAt = DateTime.UtcNow;
                _context.ProductPriceTiers.Update(existing);
            }
        }

        // Guncelle veya ekle
        foreach (var dto in tiers)
        {
            if (dto.Id.HasValue)
            {
                // Guncelle
                var existing = existingTiers.FirstOrDefault(t => t.Id == dto.Id.Value);
                if (existing != null)
                {
                    existing.MinQuantity = dto.MinQuantity;
                    existing.MaxQuantity = dto.MaxQuantity;
                    existing.Price = dto.Price;
                    existing.Description = dto.Description;
                    existing.UpdatedAt = DateTime.UtcNow;
                    _context.ProductPriceTiers.Update(existing);
                }
            }
            else
            {
                // Yeni ekle
                var newTier = new ProductPriceTier
                {
                    ProductId = productId,
                    MinQuantity = dto.MinQuantity,
                    MaxQuantity = dto.MaxQuantity,
                    Price = dto.Price,
                    Description = dto.Description
                };
                await _context.ProductPriceTiers.AddAsync(newTier);
            }
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Fiyat esikleri kaydedildi");
    }

    #endregion

    #region Categories (Read-only - Global kategoriler Admin'de yonetiliyor)

    public async Task<List<ProductCategorySelectDto>> GetCategoriesForSelectAsync()
    {
        var categories = await _unitOfWork.ProductCategories.Query()
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.Level)
            .ThenBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new ProductCategorySelectDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentId = c.ParentId,
                Level = c.Level
            })
            .ToListAsync();

        return FlattenCategoryTree(categories, null, 0);
    }

    #endregion

    #region Types (Dropdown icin tip listeleri)

    public List<TypeSelectDto> GetProductStatuses()
    {
        return ProductStatuses.All.Select(s => new TypeSelectDto
        {
            Id = s.Id,
            Name = _localizationService.T(s.NameResourceKey),
            CssClass = s.CssClass,
            Icon = s.Icon,
            IsDefault = s.IsDefault
        }).ToList();
    }

    #endregion

    #region Stock (Depo Bazli Stok Yonetimi)

    public async Task<List<ProductWarehouseStockDto>> GetProductStockAsync(int productId, int vendorId)
    {
        // Urun bu vendor'a ait mi kontrol
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p =>
            p.Id == productId && p.VendorId == vendorId && !p.IsDeleted);

        if (product == null)
            return new List<ProductWarehouseStockDto>();

        // Vendor'in tum depolarini getir
        var warehouses = await _context.Warehouses
            .Where(w => w.VendorId == vendorId && !w.IsDeleted && w.IsActive)
            .OrderBy(w => w.Priority)
            .ThenBy(w => w.Name)
            .ToListAsync();

        // Mevcut stok kayitlarini getir
        var existingStocks = await _context.ProductWarehouseStocks
            .Where(s => s.ProductId == productId && !s.IsDeleted)
            .ToListAsync();

        // Her depo icin stok bilgisi dondur (kayit yoksa 0)
        var result = warehouses.Select(w =>
        {
            var stock = existingStocks.FirstOrDefault(s => s.WarehouseId == w.Id);
            return new ProductWarehouseStockDto
            {
                Id = stock?.Id ?? 0,
                ProductId = productId,
                WarehouseId = w.Id,
                WarehouseName = w.Name,
                WarehouseCode = w.Code,
                IsDefault = w.IsDefault,
                Quantity = stock?.Quantity ?? 0,
                ReservedQuantity = stock?.ReservedQuantity ?? 0,
                BinLocation = stock?.BinLocation
            };
        }).ToList();

        return result;
    }

    public async Task<ServiceResult> SaveProductStockAsync(int productId, List<ProductWarehouseStockUpdateDto> stocks, int vendorId)
    {
        // Urun bu vendor'a ait mi kontrol
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p =>
            p.Id == productId && p.VendorId == vendorId && !p.IsDeleted);

        if (product == null)
            return ServiceResult.Fail("Urun bulunamadi");

        // Vendor'in depolarini kontrol et
        var warehouseIds = stocks.Select(s => s.WarehouseId).ToList();
        var validWarehouses = await _context.Warehouses
            .Where(w => warehouseIds.Contains(w.Id) && w.VendorId == vendorId && !w.IsDeleted)
            .Select(w => w.Id)
            .ToListAsync();

        if (validWarehouses.Count != warehouseIds.Distinct().Count())
            return ServiceResult.Fail("Gecersiz depo ID'si");

        // Mevcut stok kayitlarini getir
        var existingStocks = await _context.ProductWarehouseStocks
            .Where(s => s.ProductId == productId && !s.IsDeleted)
            .ToListAsync();

        foreach (var dto in stocks)
        {
            var existing = existingStocks.FirstOrDefault(s => s.WarehouseId == dto.WarehouseId);
            if (existing != null)
            {
                // Guncelle
                existing.Quantity = dto.Quantity;
                existing.ReservedQuantity = dto.ReservedQuantity;
                existing.LastMovementAt = DateTime.UtcNow;
                existing.UpdatedAt = DateTime.UtcNow;
                _context.ProductWarehouseStocks.Update(existing);
            }
            else if (dto.Quantity > 0 || dto.ReservedQuantity > 0)
            {
                // Yeni olustur (sadece miktar varsa)
                var newStock = new ProductWarehouseStock
                {
                    ProductId = productId,
                    WarehouseId = dto.WarehouseId,
                    Quantity = dto.Quantity,
                    ReservedQuantity = dto.ReservedQuantity,
                    LastMovementAt = DateTime.UtcNow
                };
                await _context.ProductWarehouseStocks.AddAsync(newStock);
            }
        }

        // Urunun toplam stok miktarini guncelle
        var totalStock = stocks.Sum(s => s.Quantity - s.ReservedQuantity);
        product.StockQuantity = (int)totalStock;
        product.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Products.Update(product);

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Stok bilgileri kaydedildi");
    }

    #endregion

    #region Stock Overview (Stok Ozet Sayfasi)

    public async Task<StockOverviewDto> GetStockOverviewAsync(int vendorId)
    {
        var products = await _unitOfWork.Products.Query()
            .Where(p => p.VendorId == vendorId && !p.IsDeleted)
            .ToListAsync();

        var warehouses = await _context.Warehouses
            .Where(w => w.VendorId == vendorId && !w.IsDeleted && w.IsActive)
            .CountAsync();

        var totalProducts = products.Count;
        var productsInStock = products.Count(p => p.StockQuantity > 0);
        var productsOutOfStock = products.Count(p => p.StockQuantity <= 0);
        var lowStockProducts = products.Count(p => p.MinStockLevel.HasValue && p.StockQuantity <= p.MinStockLevel.Value && p.StockQuantity > 0);

        // Toplam stok degeri (stok * fiyat)
        var totalStockValue = products.Sum(p => p.StockQuantity * p.Price);

        return new StockOverviewDto
        {
            TotalProducts = totalProducts,
            ProductsInStock = productsInStock,
            ProductsOutOfStock = productsOutOfStock,
            LowStockProducts = lowStockProducts,
            TotalStockValue = totalStockValue,
            TotalWarehouses = warehouses
        };
    }

    public async Task<List<LowStockProductDto>> GetLowStockProductsAsync(int vendorId)
    {
        var products = await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.VendorId == vendorId && !p.IsDeleted)
            .Where(p =>
                // Min stok seviyesi belirlenmis ve altinda olan
                (p.MinStockLevel.HasValue && p.StockQuantity <= p.MinStockLevel.Value) ||
                // Veya stok 0 olan
                p.StockQuantity <= 0)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .Take(50) // En fazla 50 kayit
            .ToListAsync();

        return products.Select(p => new LowStockProductDto
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            MainImageUrl = p.Images.FirstOrDefault(i => i.IsMain)?.Url ?? p.Images.FirstOrDefault()?.Url,
            StockQuantity = p.StockQuantity,
            MinStockLevel = p.MinStockLevel,
            StockDifference = p.MinStockLevel.HasValue ? p.StockQuantity - p.MinStockLevel.Value : p.StockQuantity,
            CategoryName = p.Category?.Name
        }).ToList();
    }

    public async Task<List<WarehouseStockSummaryDto>> GetStockByWarehouseAsync(int vendorId)
    {
        var warehouses = await _context.Warehouses
            .Where(w => w.VendorId == vendorId && !w.IsDeleted && w.IsActive)
            .OrderBy(w => w.Priority)
            .ThenBy(w => w.Name)
            .ToListAsync();

        var result = new List<WarehouseStockSummaryDto>();

        foreach (var warehouse in warehouses)
        {
            var stocks = await _context.ProductWarehouseStocks
                .Include(s => s.Product)
                .Where(s => s.WarehouseId == warehouse.Id && !s.IsDeleted && !s.Product.IsDeleted)
                .ToListAsync();

            result.Add(new WarehouseStockSummaryDto
            {
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                WarehouseCode = warehouse.Code,
                IsDefault = warehouse.IsDefault,
                ProductCount = stocks.Count,
                TotalQuantity = stocks.Sum(s => s.Quantity),
                TotalReserved = stocks.Sum(s => s.ReservedQuantity),
                TotalAvailable = stocks.Sum(s => s.Quantity - s.ReservedQuantity),
                TotalValue = stocks.Sum(s => (s.Quantity - s.ReservedQuantity) * s.Product.Price)
            });
        }

        return result;
    }

    #endregion

    #region Public Search (Teklif yonetimi icin urun referansi)

    public async Task<List<ProductSearchResultDto>> SearchProductsAsync(string? search, int pageSize = 10)
    {
        // Platform genelinde aktif urunleri ara
        // ProductStatusId = 1 (Aktif) varsayilir
        var query = _unitOfWork.Products.Query()
            .Where(p => !p.IsDeleted && p.ProductStatusId == 1)
            .Include(p => p.Category)
            .Include(p => p.Vendor)
            .Include(p => p.Images.Where(i => i.IsMain))
            .AsQueryable();

        // Arama filtresi
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                (p.SKU != null && p.SKU.ToLower().Contains(searchLower)) ||
                (p.Description != null && p.Description.ToLower().Contains(searchLower)) ||
                (p.Category != null && p.Category.Name.ToLower().Contains(searchLower)));
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Take(pageSize)
            .Select(p => new ProductSearchResultDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                MainImageUrl = p.Images.FirstOrDefault(i => i.IsMain) != null
                    ? p.Images.FirstOrDefault(i => i.IsMain)!.Url
                    : p.Images.FirstOrDefault() != null
                        ? p.Images.FirstOrDefault()!.Url
                        : null,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                VendorName = p.Vendor != null ? p.Vendor.CompanyName : null,
                Price = p.Price,
                Currency = p.Currency
            })
            .ToListAsync();
    }

    #endregion

    #region Private Methods

    private string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Guid.NewGuid().ToString("N")[..8];

        var slug = name.ToLower()
            .Replace(" ", "-")
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

        // Sadece alfanumerik ve tire bırak
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        // Tekrarlı tireleri tek tire yap
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        // Basta ve sonda tire varsa kaldir
        slug = slug.Trim('-');

        // Timestamp ekle uniquelik icin
        slug = $"{slug}-{DateTime.UtcNow.Ticks.ToString()[^6..]}";

        return slug.Length > 200 ? slug[..200] : slug;
    }

    private List<ProductCategorySelectDto> FlattenCategoryTree(List<ProductCategorySelectDto> categories, int? parentId, int level)
    {
        var result = new List<ProductCategorySelectDto>();

        var children = categories
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.Level)
            .ThenBy(c => c.Name)
            .ToList();

        foreach (var category in children)
        {
            category.Level = level;
            result.Add(category);
            result.AddRange(FlattenCategoryTree(categories, category.Id, level + 1));
        }

        return result;
    }

    #endregion

    #region Packaging (Paketleme Bilgileri)

    public async Task<List<ProductPackagingDto>> GetProductPackagingAsync(int productId, int vendorId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && p.VendorId == vendorId);

        if (product == null)
            return new List<ProductPackagingDto>();

        var packagings = await _context.ProductPackagings
            .Where(p => p.ProductId == productId)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.UnitId)
            .ToListAsync();

        return packagings.Select(p =>
        {
            var unit = UnitTypes.GetById(p.UnitId);
            return new ProductPackagingDto
            {
                Id = p.Id,
                ProductId = p.ProductId,
                UnitId = p.UnitId,
                UnitName = unit != null
                    ? _localizationService.GetResource(unit.NameResourceKey, unit.Description ?? unit.SystemName)
                    : "Bilinmiyor",
                UnitIcon = unit?.Icon,
                UnitCssClass = unit?.CssClass,
                Quantity = p.Quantity,
                ContainsCount = p.ContainsCount,
                Barcode = p.Barcode,
                SKU = p.SKU,
                GrossWeight = p.GrossWeight,
                NetWeight = p.NetWeight,
                Length = p.Length,
                Width = p.Width,
                Height = p.Height,
                MinOrderQuantity = p.MinOrderQuantity,
                OrderMultiple = p.OrderMultiple,
                IsActive = p.IsActive,
                IsDefault = p.IsDefault,
                DisplayOrder = p.DisplayOrder
            };
        }).ToList();
    }

    public async Task<ServiceResult> SaveProductPackagingAsync(int productId, List<ProductPackagingCreateUpdateDto> packagings, int vendorId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && p.VendorId == vendorId);

        if (product == null)
            return ServiceResult.Fail("Urun bulunamadi.");

        // Mevcut paketlemeleri getir
        var existingPackagings = await _context.ProductPackagings
            .Where(p => p.ProductId == productId)
            .ToListAsync();

        // Gelen ID'ler (var olanlar guncellenir, olmayanlar silinir)
        var incomingIds = packagings.Where(p => p.Id.HasValue).Select(p => p.Id!.Value).ToHashSet();

        // Silinecekler
        var toDelete = existingPackagings.Where(p => !incomingIds.Contains(p.Id)).ToList();
        foreach (var del in toDelete)
        {
            del.IsDeleted = true;
            del.DeletedAt = DateTime.UtcNow;
        }

        // Yeni eklenecekler ve guncellenecekler
        foreach (var dto in packagings)
        {
            if (dto.Id.HasValue)
            {
                // Guncelle
                var existing = existingPackagings.FirstOrDefault(p => p.Id == dto.Id.Value);
                if (existing != null)
                {
                    existing.UnitId = dto.UnitId;
                    existing.Quantity = dto.Quantity;
                    existing.ContainsCount = dto.ContainsCount;
                    existing.Barcode = dto.Barcode;
                    existing.SKU = dto.SKU;
                    existing.GrossWeight = dto.GrossWeight;
                    existing.NetWeight = dto.NetWeight;
                    existing.Length = dto.Length;
                    existing.Width = dto.Width;
                    existing.Height = dto.Height;
                    existing.MinOrderQuantity = dto.MinOrderQuantity;
                    existing.OrderMultiple = dto.OrderMultiple;
                    existing.IsActive = dto.IsActive;
                    existing.IsDefault = dto.IsDefault;
                    existing.DisplayOrder = dto.DisplayOrder;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                // Yeni ekle
                var newPackaging = new ProductPackaging
                {
                    ProductId = productId,
                    UnitId = dto.UnitId,
                    Quantity = dto.Quantity,
                    ContainsCount = dto.ContainsCount,
                    Barcode = dto.Barcode,
                    SKU = dto.SKU,
                    GrossWeight = dto.GrossWeight,
                    NetWeight = dto.NetWeight,
                    Length = dto.Length,
                    Width = dto.Width,
                    Height = dto.Height,
                    MinOrderQuantity = dto.MinOrderQuantity,
                    OrderMultiple = dto.OrderMultiple,
                    IsActive = dto.IsActive,
                    IsDefault = dto.IsDefault,
                    DisplayOrder = dto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ProductPackagings.Add(newPackaging);
            }
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    #endregion
}
