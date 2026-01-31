using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Buyer;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class BuyerService : IBuyerService
{
    private readonly ApplicationDbContext _context;

    public BuyerService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================
    // TEDARİKÇİ KEŞFİ
    // ============================================

    public async Task<SupplierSearchResultDto> SearchSuppliersAsync(int buyerVendorId, SupplierFilterDto filter)
    {
        // Satıcı capability'sine sahip vendor'ları bul
        var query = _context.VendorCapabilityMappings
            .Where(vcm => vcm.CapabilityId == 2) // Seller capability
            .Where(vcm => vcm.VendorId != buyerVendorId) // Kendisi hariç
            .Select(vcm => vcm.Vendor!)
            .Where(v => v.VendorStatusId == VendorStatuses.Active.Id && v.IsVerified)
            .AsQueryable();

        // Arama
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(v =>
                v.CompanyName.ToLower().Contains(search) ||
                (v.TaxNumber != null && v.TaxNumber.Contains(search)));
        }

        // Kategori filtresi - Satıcının ürünü olan kategoriler
        if (filter.CategoryId.HasValue)
        {
            var categoryId = filter.CategoryId.Value;
            query = query.Where(v => _context.Products
                .Any(p => p.VendorId == v.Id && p.CategoryId == categoryId));
        }

        // Doğrulanmış mı?
        if (filter.IsVerified.HasValue)
        {
            query = query.Where(v => v.IsVerified == filter.IsVerified.Value);
        }

        // Ürünü var mı?
        if (filter.HasProducts.HasValue && filter.HasProducts.Value)
        {
            query = query.Where(v => _context.Products.Any(p => p.VendorId == v.Id && p.ProductStatusId == ProductStatuses.Active.Id));
        }

        var totalCount = await query.CountAsync();

        // Sıralama
        query = filter.SortBy?.ToLower() switch
        {
            "name" => filter.SortDescending ? query.OrderByDescending(v => v.CompanyName) : query.OrderBy(v => v.CompanyName),
            "products" => filter.SortDescending
                ? query.OrderByDescending(v => _context.Products.Count(p => p.VendorId == v.Id))
                : query.OrderBy(v => _context.Products.Count(p => p.VendorId == v.Id)),
            "rating" => filter.SortDescending
                ? query.OrderByDescending(v => _context.VendorReviews.Where(r => r.ReviewedVendorId == v.Id).Average(r => (decimal?)r.OverallRating) ?? 0)
                : query.OrderBy(v => _context.VendorReviews.Where(r => r.ReviewedVendorId == v.Id).Average(r => (decimal?)r.OverallRating) ?? 0),
            _ => query.OrderByDescending(v => v.CreatedAt)
        };

        // Sayfalama
        var vendors = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // DTO'ya dönüştür
        var items = new List<SupplierListDto>();
        foreach (var vendor in vendors)
        {
            items.Add(await MapToSupplierListDtoAsync(vendor, buyerVendorId));
        }

        return new SupplierSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        };
    }

    public async Task<SupplierDetailDto?> GetSupplierDetailAsync(int buyerVendorId, int vendorId)
    {
        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Id == vendorId && !v.IsDeleted);

        if (vendor == null) return null;

        var listDto = await MapToSupplierListDtoAsync(vendor, buyerVendorId);

        // Adresler
        var addresses = await _context.Addresses
            .Where(a => a.VendorId == vendorId)
            .Include(a => a.CountryEntity)
            .Include(a => a.StateEntity)
            .Take(5)
            .Select(a => new SupplierAddressDto
            {
                Id = a.Id,
                Title = a.Title,
                CountryName = a.CountryEntity != null ? a.CountryEntity.Name : null,
                StateName = a.StateEntity != null ? a.StateEntity.Name : null,
                City = a.City
            })
            .ToListAsync();

        // Öne çıkan ürünler
        var products = await _context.Products
            .Where(p => p.VendorId == vendorId && p.ProductStatusId == ProductStatuses.Active.Id)
            .Include(p => p.Category)
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.CreatedAt)
            .Take(6)
            .Select(p => new SupplierProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Price = p.Price,
                Currency = p.Currency,
                MainImageUrl = _context.ProductImages
                    .Where(i => i.ProductId == p.Id && i.IsMain)
                    .Select(i => i.Url)
                    .FirstOrDefault(),
                CategoryName = p.Category != null ? p.Category.Name : null
            })
            .ToListAsync();

        // Son değerlendirmeler
        var reviews = await _context.VendorReviews
            .Where(r => r.ReviewedVendorId == vendorId && r.IsPublished)
            .Include(r => r.ReviewerVendor)
            .OrderByDescending(r => r.ReviewedAt)
            .Take(5)
            .Select(r => new VendorReviewDto
            {
                Id = r.Id,
                ReviewerVendorId = r.ReviewerVendorId,
                ReviewerCompanyName = r.ReviewerVendor!.CompanyName,
                ReviewerLogoUrl = r.ReviewerVendor.LogoUrl,
                ReviewedVendorId = r.ReviewedVendorId,
                OrderId = r.OrderId,
                OverallRating = r.OverallRating,
                QualityRating = r.QualityRating,
                DeliveryRating = r.DeliveryRating,
                CommunicationRating = r.CommunicationRating,
                ValueRating = r.ValueRating,
                Title = r.Title,
                Comment = r.Comment,
                Pros = r.Pros,
                Cons = r.Cons,
                WouldRecommend = r.WouldRecommend,
                ReviewedAt = r.ReviewedAt,
                SellerResponse = r.SellerResponse,
                SellerRespondedAt = r.SellerRespondedAt
            })
            .ToListAsync();

        // SupplierProfile'dan ek bilgiler
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(p => p.VendorId == vendorId);

        return new SupplierDetailDto
        {
            Id = listDto.Id,
            CompanyName = listDto.CompanyName,
            LogoUrl = listDto.LogoUrl,
            Website = listDto.Website,
            CountryId = listDto.CountryId,
            CountryName = listDto.CountryName,
            City = listDto.City,
            ProductCount = listDto.ProductCount,
            CompletedOrderCount = listDto.CompletedOrderCount,
            AverageRating = listDto.AverageRating,
            ReviewCount = listDto.ReviewCount,
            Categories = listDto.Categories,
            CategoryIds = listDto.CategoryIds,
            IsFavorite = listDto.IsFavorite,
            FavoriteId = listDto.FavoriteId,
            ShortDescription = profile?.ShortDescription,
            YearsInBusiness = 0, // SupplierProfile'da bu alan yok
            IsVerified = listDto.IsVerified,
            Capabilities = listDto.Capabilities,
            Description = profile?.Description,
            AuthorizedPersonName = vendor.AuthorizedPersonName,
            AuthorizedPersonTitle = vendor.AuthorizedPersonTitle,
            Email = vendor.Email,
            Phone = vendor.Phone,
            Addresses = addresses,
            FeaturedProducts = products,
            RecentReviews = reviews,
            TotalProductCount = await _context.Products.CountAsync(p => p.VendorId == vendorId),
            ActiveDemandResponseCount = await _context.DemandResponses.CountAsync(r => r.SupplierVendorId == vendorId)
        };
    }

    public async Task<List<SupplierListDto>> GetRecommendedSuppliersAsync(int buyerVendorId, int count = 10)
    {
        // Alıcının ilgilendiği kategorileri bul
        var interestedCategoryIds = await _context.PublicDemands
            .Where(d => d.VendorId == buyerVendorId && d.CategoryId.HasValue)
            .Select(d => d.CategoryId!.Value)
            .Union(_context.ProductInquiries
                .Where(i => i.BuyerVendorId == buyerVendorId)
                .Include(i => i.Product)
                .Where(i => i.Product!.CategoryId.HasValue)
                .Select(i => i.Product!.CategoryId!.Value))
            .Distinct()
            .ToListAsync();

        // Bu kategorilerde ürünü olan satıcıları bul
        var vendorIds = await _context.Products
            .Where(p => interestedCategoryIds.Contains(p.CategoryId ?? 0) && p.ProductStatusId == ProductStatuses.Active.Id)
            .Select(p => p.VendorId)
            .Distinct()
            .Take(count * 2)
            .ToListAsync();

        var vendors = await _context.Vendors
            .Where(v => vendorIds.Contains(v.Id) && v.Id != buyerVendorId && v.IsVerified)
            .Take(count)
            .ToListAsync();

        var result = new List<SupplierListDto>();
        foreach (var vendor in vendors)
        {
            result.Add(await MapToSupplierListDtoAsync(vendor, buyerVendorId));
        }

        return result;
    }

    // ============================================
    // SİPARİŞ GEÇMİŞİNDEN TEDARİKÇİLER
    // ============================================

    public async Task<List<OrderSupplierDto>> GetOrderSuppliersAsync(int buyerVendorId)
    {
        // Alıcının siparişlerinden tedarikçileri grupla
        var orderSuppliers = await _context.Orders
            .Where(o => o.BuyerVendorId == buyerVendorId && !o.IsDeleted)
            .GroupBy(o => o.SellerVendorId)
            .Select(g => new
            {
                VendorId = g.Key,
                OrderCount = g.Count(),
                TotalAmount = g.Sum(o => o.TotalAmount),
                Currency = g.First().Currency ?? "TRY",
                FirstOrderDate = g.Min(o => o.CreatedAt),
                LastOrderDate = g.Max(o => o.CreatedAt)
            })
            .ToListAsync();

        if (!orderSuppliers.Any())
            return new List<OrderSupplierDto>();

        var vendorIds = orderSuppliers.Select(s => s.VendorId).ToList();

        // Vendor bilgilerini al
        var vendors = await _context.Vendors
            .Where(v => vendorIds.Contains(v.Id))
            .Include(v => v.Addresses)
                .ThenInclude(a => a.CountryEntity)
            .ToDictionaryAsync(v => v.Id);

        // Favori durumlarını kontrol et
        var favoriteVendorIds = await _context.FavoriteVendors
            .Where(f => f.BuyerVendorId == buyerVendorId && vendorIds.Contains(f.SellerVendorId))
            .Select(f => new { f.SellerVendorId, f.Id })
            .ToDictionaryAsync(f => f.SellerVendorId, f => f.Id);

        return orderSuppliers
            .Where(os => vendors.ContainsKey(os.VendorId))
            .Select(os =>
            {
                var vendor = vendors[os.VendorId];
                var primaryAddress = vendor.Addresses.FirstOrDefault(a => a.IsDefault) ?? vendor.Addresses.FirstOrDefault();
                return new OrderSupplierDto
                {
                    VendorId = os.VendorId,
                    CompanyName = vendor.CompanyName,
                    LogoUrl = vendor.LogoUrl,
                    CountryName = primaryAddress?.CountryEntity?.Name,
                    IsVerified = vendor.IsVerified,
                    OrderCount = os.OrderCount,
                    TotalAmount = os.TotalAmount,
                    Currency = os.Currency,
                    FirstOrderDate = os.FirstOrderDate,
                    LastOrderDate = os.LastOrderDate,
                    IsFavorite = favoriteVendorIds.ContainsKey(os.VendorId),
                    FavoriteId = favoriteVendorIds.ContainsKey(os.VendorId) ? favoriteVendorIds[os.VendorId] : null
                };
            })
            .OrderByDescending(s => s.LastOrderDate)
            .ToList();
    }

    // ============================================
    // FAVORİ TEDARİKÇİLER
    // ============================================

    public async Task<List<FavoriteVendorDto>> GetFavoriteVendorsAsync(int buyerVendorId)
    {
        return await _context.FavoriteVendors
            .Where(f => f.BuyerVendorId == buyerVendorId)
            .Include(f => f.SellerVendor)
            .OrderByDescending(f => f.AddedAt)
            .Select(f => new FavoriteVendorDto
            {
                Id = f.Id,
                SellerVendorId = f.SellerVendorId,
                CompanyName = f.SellerVendor!.CompanyName,
                LogoUrl = f.SellerVendor.LogoUrl,
                Notes = f.Notes,
                Tags = f.Tags,
                AddedAt = f.AddedAt,
                ProductCount = _context.Products.Count(p => p.VendorId == f.SellerVendorId && p.ProductStatusId == ProductStatuses.Active.Id),
                AverageRating = _context.VendorReviews.Where(r => r.ReviewedVendorId == f.SellerVendorId).Average(r => (decimal?)r.OverallRating) ?? 0,
                ReviewCount = _context.VendorReviews.Count(r => r.ReviewedVendorId == f.SellerVendorId),
                IsVerified = f.SellerVendor.IsVerified,
                CountryName = _context.Addresses
                    .Where(a => a.VendorId == f.SellerVendorId)
                    .Include(a => a.CountryEntity)
                    .Select(a => a.CountryEntity != null ? a.CountryEntity.Name : null)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<FavoriteVendorDto?> GetFavoriteVendorAsync(int buyerVendorId, int favoriteId)
    {
        return await _context.FavoriteVendors
            .Where(f => f.Id == favoriteId && f.BuyerVendorId == buyerVendorId)
            .Include(f => f.SellerVendor)
            .Select(f => new FavoriteVendorDto
            {
                Id = f.Id,
                SellerVendorId = f.SellerVendorId,
                CompanyName = f.SellerVendor!.CompanyName,
                LogoUrl = f.SellerVendor.LogoUrl,
                Notes = f.Notes,
                Tags = f.Tags,
                AddedAt = f.AddedAt,
                ProductCount = _context.Products.Count(p => p.VendorId == f.SellerVendorId),
                AverageRating = _context.VendorReviews.Where(r => r.ReviewedVendorId == f.SellerVendorId).Average(r => (decimal?)r.OverallRating) ?? 0,
                ReviewCount = _context.VendorReviews.Count(r => r.ReviewedVendorId == f.SellerVendorId),
                IsVerified = f.SellerVendor.IsVerified
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<FavoriteVendorDto>> AddFavoriteVendorAsync(int buyerVendorId, int userId, FavoriteVendorCreateDto dto)
    {
        // Zaten favori mi?
        var existing = await _context.FavoriteVendors
            .FirstOrDefaultAsync(f => f.BuyerVendorId == buyerVendorId && f.SellerVendorId == dto.SellerVendorId);

        if (existing != null)
            return ServiceResult<FavoriteVendorDto>.Fail("Bu tedarikçi zaten favorilerinizde.");

        // Satıcı var mı?
        var seller = await _context.Vendors.FindAsync(dto.SellerVendorId);
        if (seller == null)
            return ServiceResult<FavoriteVendorDto>.Fail("Tedarikçi bulunamadı.");

        var favorite = new FavoriteVendor
        {
            BuyerVendorId = buyerVendorId,
            SellerVendorId = dto.SellerVendorId,
            Notes = dto.Notes,
            Tags = dto.Tags,
            AddedAt = DateTime.UtcNow,
            AddedByUserId = userId
        };

        _context.FavoriteVendors.Add(favorite);
        await _context.SaveChangesAsync();

        var result = await GetFavoriteVendorAsync(buyerVendorId, favorite.Id);
        return ServiceResult<FavoriteVendorDto>.Ok(result!, "Tedarikçi favorilere eklendi.");
    }

    public async Task<ServiceResult> UpdateFavoriteVendorAsync(int buyerVendorId, int favoriteId, FavoriteVendorUpdateDto dto)
    {
        var favorite = await _context.FavoriteVendors
            .FirstOrDefaultAsync(f => f.Id == favoriteId && f.BuyerVendorId == buyerVendorId);

        if (favorite == null)
            return ServiceResult.Fail("Favori bulunamadı.");

        favorite.Notes = dto.Notes;
        favorite.Tags = dto.Tags;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Favori güncellendi.");
    }

    public async Task<ServiceResult> RemoveFavoriteVendorAsync(int buyerVendorId, int favoriteId)
    {
        var favorite = await _context.FavoriteVendors
            .FirstOrDefaultAsync(f => f.Id == favoriteId && f.BuyerVendorId == buyerVendorId);

        if (favorite == null)
            return ServiceResult.Fail("Favori bulunamadı.");

        _context.FavoriteVendors.Remove(favorite);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Tedarikçi favorilerden kaldırıldı.");
    }

    public async Task<bool> IsFavoriteAsync(int buyerVendorId, int sellerVendorId)
    {
        return await _context.FavoriteVendors
            .AnyAsync(f => f.BuyerVendorId == buyerVendorId && f.SellerVendorId == sellerVendorId);
    }

    // ============================================
    // TEDARİKÇİ DEĞERLENDİRMELERİ
    // ============================================

    public async Task<List<VendorReviewDto>> GetVendorReviewsAsync(int vendorId, int page = 1, int pageSize = 10)
    {
        return await _context.VendorReviews
            .Where(r => r.ReviewedVendorId == vendorId && r.IsPublished)
            .Include(r => r.ReviewerVendor)
            .Include(r => r.Order)
            .OrderByDescending(r => r.ReviewedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new VendorReviewDto
            {
                Id = r.Id,
                ReviewerVendorId = r.ReviewerVendorId,
                ReviewerCompanyName = r.ReviewerVendor!.CompanyName,
                ReviewerLogoUrl = r.ReviewerVendor.LogoUrl,
                ReviewedVendorId = r.ReviewedVendorId,
                OrderId = r.OrderId,
                OrderNumber = r.Order != null ? r.Order.OrderNumber : null,
                OverallRating = r.OverallRating,
                QualityRating = r.QualityRating,
                DeliveryRating = r.DeliveryRating,
                CommunicationRating = r.CommunicationRating,
                ValueRating = r.ValueRating,
                Title = r.Title,
                Comment = r.Comment,
                Pros = r.Pros,
                Cons = r.Cons,
                WouldRecommend = r.WouldRecommend,
                ReviewedAt = r.ReviewedAt,
                SellerResponse = r.SellerResponse,
                SellerRespondedAt = r.SellerRespondedAt
            })
            .ToListAsync();
    }

    public async Task<SupplierRatingStatsDto> GetVendorRatingStatsAsync(int vendorId)
    {
        var reviews = await _context.VendorReviews
            .Where(r => r.ReviewedVendorId == vendorId && r.IsPublished)
            .ToListAsync();

        if (!reviews.Any())
        {
            return new SupplierRatingStatsDto { VendorId = vendorId };
        }

        return new SupplierRatingStatsDto
        {
            VendorId = vendorId,
            AverageOverall = (decimal)reviews.Average(r => r.OverallRating),
            AverageQuality = reviews.Where(r => r.QualityRating.HasValue).Any()
                ? (decimal)reviews.Where(r => r.QualityRating.HasValue).Average(r => r.QualityRating!.Value)
                : 0,
            AverageDelivery = reviews.Where(r => r.DeliveryRating.HasValue).Any()
                ? (decimal)reviews.Where(r => r.DeliveryRating.HasValue).Average(r => r.DeliveryRating!.Value)
                : 0,
            AverageCommunication = reviews.Where(r => r.CommunicationRating.HasValue).Any()
                ? (decimal)reviews.Where(r => r.CommunicationRating.HasValue).Average(r => r.CommunicationRating!.Value)
                : 0,
            AverageValue = reviews.Where(r => r.ValueRating.HasValue).Any()
                ? (decimal)reviews.Where(r => r.ValueRating.HasValue).Average(r => r.ValueRating!.Value)
                : 0,
            TotalReviews = reviews.Count,
            RecommendCount = reviews.Count(r => r.WouldRecommend),
            RecommendPercentage = reviews.Any() ? (decimal)reviews.Count(r => r.WouldRecommend) / reviews.Count * 100 : 0,
            Rating5Count = reviews.Count(r => r.OverallRating == 5),
            Rating4Count = reviews.Count(r => r.OverallRating == 4),
            Rating3Count = reviews.Count(r => r.OverallRating == 3),
            Rating2Count = reviews.Count(r => r.OverallRating == 2),
            Rating1Count = reviews.Count(r => r.OverallRating == 1)
        };
    }

    public async Task<VendorReviewDto?> GetReviewByOrderAsync(int orderId)
    {
        return await _context.VendorReviews
            .Where(r => r.OrderId == orderId)
            .Include(r => r.ReviewerVendor)
            .Select(r => new VendorReviewDto
            {
                Id = r.Id,
                ReviewerVendorId = r.ReviewerVendorId,
                ReviewerCompanyName = r.ReviewerVendor!.CompanyName,
                ReviewedVendorId = r.ReviewedVendorId,
                OrderId = r.OrderId,
                OverallRating = r.OverallRating,
                QualityRating = r.QualityRating,
                DeliveryRating = r.DeliveryRating,
                CommunicationRating = r.CommunicationRating,
                ValueRating = r.ValueRating,
                Title = r.Title,
                Comment = r.Comment,
                Pros = r.Pros,
                Cons = r.Cons,
                WouldRecommend = r.WouldRecommend,
                ReviewedAt = r.ReviewedAt,
                SellerResponse = r.SellerResponse,
                SellerRespondedAt = r.SellerRespondedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<VendorReviewDto>> CreateReviewAsync(int reviewerVendorId, int userId, VendorReviewCreateDto dto)
    {
        // Puan kontrolü
        if (dto.OverallRating < 1 || dto.OverallRating > 5)
            return ServiceResult<VendorReviewDto>.Fail("Puan 1-5 arasında olmalıdır.");

        // Sipariş varsa, bu sipariş için değerlendirme yapılmış mı?
        if (dto.OrderId.HasValue)
        {
            var existingReview = await _context.VendorReviews
                .AnyAsync(r => r.OrderId == dto.OrderId.Value);

            if (existingReview)
                return ServiceResult<VendorReviewDto>.Fail("Bu sipariş için zaten değerlendirme yapılmış.");

            // Sipariş bu alıcıya ait mi?
            var order = await _context.Orders.FindAsync(dto.OrderId.Value);
            if (order == null || order.BuyerVendorId != reviewerVendorId)
                return ServiceResult<VendorReviewDto>.Fail("Sipariş bulunamadı veya size ait değil.");
        }

        var review = new VendorReview
        {
            ReviewerVendorId = reviewerVendorId,
            ReviewedVendorId = dto.ReviewedVendorId,
            OrderId = dto.OrderId,
            OverallRating = dto.OverallRating,
            QualityRating = dto.QualityRating,
            DeliveryRating = dto.DeliveryRating,
            CommunicationRating = dto.CommunicationRating,
            ValueRating = dto.ValueRating,
            Title = dto.Title,
            Comment = dto.Comment,
            Pros = dto.Pros,
            Cons = dto.Cons,
            WouldRecommend = dto.WouldRecommend,
            ReviewedAt = DateTime.UtcNow,
            ReviewerUserId = userId,
            IsPublished = true
        };

        _context.VendorReviews.Add(review);
        await _context.SaveChangesAsync();

        var result = await _context.VendorReviews
            .Where(r => r.Id == review.Id)
            .Include(r => r.ReviewerVendor)
            .Select(r => new VendorReviewDto
            {
                Id = r.Id,
                ReviewerVendorId = r.ReviewerVendorId,
                ReviewerCompanyName = r.ReviewerVendor!.CompanyName,
                ReviewedVendorId = r.ReviewedVendorId,
                OrderId = r.OrderId,
                OverallRating = r.OverallRating,
                QualityRating = r.QualityRating,
                DeliveryRating = r.DeliveryRating,
                CommunicationRating = r.CommunicationRating,
                ValueRating = r.ValueRating,
                Title = r.Title,
                Comment = r.Comment,
                Pros = r.Pros,
                Cons = r.Cons,
                WouldRecommend = r.WouldRecommend,
                ReviewedAt = r.ReviewedAt
            })
            .FirstAsync();

        return ServiceResult<VendorReviewDto>.Ok(result, "Değerlendirme kaydedildi.");
    }

    public async Task<ServiceResult> RespondToReviewAsync(int reviewedVendorId, int reviewId, VendorReviewResponseDto dto)
    {
        var review = await _context.VendorReviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.ReviewedVendorId == reviewedVendorId);

        if (review == null)
            return ServiceResult.Fail("Değerlendirme bulunamadı.");

        if (!string.IsNullOrEmpty(review.SellerResponse))
            return ServiceResult.Fail("Bu değerlendirmeye zaten yanıt verilmiş.");

        review.SellerResponse = dto.Response;
        review.SellerRespondedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Yanıtınız kaydedildi.");
    }

    public async Task<List<VendorReviewDto>> GetMyReviewsAsync(int buyerVendorId)
    {
        return await _context.VendorReviews
            .Where(r => r.ReviewerVendorId == buyerVendorId)
            .Include(r => r.ReviewedVendor)
            .Include(r => r.Order)
            .OrderByDescending(r => r.ReviewedAt)
            .Select(r => new VendorReviewDto
            {
                Id = r.Id,
                ReviewerVendorId = r.ReviewerVendorId,
                ReviewerCompanyName = r.ReviewerVendor!.CompanyName,
                ReviewedVendorId = r.ReviewedVendorId,
                OrderId = r.OrderId,
                OrderNumber = r.Order != null ? r.Order.OrderNumber : null,
                OverallRating = r.OverallRating,
                Title = r.Title,
                Comment = r.Comment,
                WouldRecommend = r.WouldRecommend,
                ReviewedAt = r.ReviewedAt,
                SellerResponse = r.SellerResponse,
                SellerRespondedAt = r.SellerRespondedAt
            })
            .ToListAsync();
    }

    // ============================================
    // ALICI İSTATİSTİKLERİ
    // ============================================

    public async Task<BuyerStatsDto> GetBuyerStatsAsync(int buyerVendorId)
    {
        return new BuyerStatsDto
        {
            TotalOrders = await _context.Orders.CountAsync(o => o.BuyerVendorId == buyerVendorId),
            PendingOrders = await _context.Orders.CountAsync(o => o.BuyerVendorId == buyerVendorId &&
                (o.Status == OrderStatuses.PendingConfirm.Id || o.Status == OrderStatuses.Confirmed.Id || o.Status == OrderStatuses.Processing.Id)),
            CompletedOrders = await _context.Orders.CountAsync(o => o.BuyerVendorId == buyerVendorId && o.Status == OrderStatuses.Completed.Id),
            FavoriteSupplierCount = await _context.FavoriteVendors.CountAsync(f => f.BuyerVendorId == buyerVendorId),
            ActiveDemandsCount = await _context.PublicDemands.CountAsync(d => d.VendorId == buyerVendorId && d.Status == 2), // Active
            ActiveInquiriesCount = await _context.ProductInquiries.CountAsync(i => i.BuyerVendorId == buyerVendorId &&
                i.Status == ProductInquiryStatuses.Pending.Id),
            PendingResponsesCount = await _context.DemandResponses.CountAsync(r =>
                _context.PublicDemands.Any(d => d.Id == r.DemandId && d.VendorId == buyerVendorId) &&
                r.Status == 1) // Pending
        };
    }

    // ============================================
    // TEKLİF KARŞILAŞTIRMA
    // ============================================

    public async Task<List<DemandWithProposalsDto>> GetDemandsWithProposalsAsync(int buyerVendorId)
    {
        var demands = await _context.PublicDemands
            .Where(d => d.VendorId == buyerVendorId && d.Status >= 1) // Not draft
            .Include(d => d.Category)
            .Include(d => d.Responses)
                .ThenInclude(r => r.SupplierVendor)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var result = new List<DemandWithProposalsDto>();

        foreach (var demand in demands)
        {
            var proposals = new List<ProposalDto>();

            foreach (var response in demand.Responses.OrderByDescending(r => r.CreatedAt))
            {
                decimal avgRating = 0;
                int reviewCount = 0;
                bool isFavorite = false;

                if (response.SupplierVendorId.HasValue)
                {
                    avgRating = await _context.VendorReviews
                        .Where(r => r.ReviewedVendorId == response.SupplierVendorId.Value)
                        .AverageAsync(r => (decimal?)r.OverallRating) ?? 0;
                    reviewCount = await _context.VendorReviews
                        .CountAsync(r => r.ReviewedVendorId == response.SupplierVendorId.Value);
                    isFavorite = await _context.FavoriteVendors
                        .AnyAsync(f => f.BuyerVendorId == buyerVendorId && f.SellerVendorId == response.SupplierVendorId.Value);
                }

                proposals.Add(new ProposalDto
                {
                    Id = response.Id,
                    DemandId = response.DemandId,
                    SupplierVendorId = response.SupplierVendorId,
                    CompanyName = response.SupplierVendor?.CompanyName ?? response.ExternalCompanyName,
                    LogoUrl = response.SupplierVendor?.LogoUrl,
                    IsVerified = response.SupplierVendor?.IsVerified ?? false,
                    AverageRating = avgRating,
                    ReviewCount = reviewCount,
                    ExternalCompanyName = response.ExternalCompanyName,
                    ExternalContactName = response.ExternalContactName,
                    ExternalEmail = response.ExternalEmail,
                    ExternalPhone = response.ExternalPhone,
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
                    StatusName = GetProposalStatusName(response.Status),
                    CreatedAt = response.CreatedAt,
                    ViewedAt = response.ViewedAt,
                    IsFavorite = isFavorite
                });
            }

            // En iyi teklifleri işaretle
            if (proposals.Any())
            {
                var lowestPrice = proposals.Where(p => p.UnitPrice.HasValue).OrderBy(p => p.UnitPrice).FirstOrDefault();
                var fastestDelivery = proposals.Where(p => p.LeadTimeDays.HasValue).OrderBy(p => p.LeadTimeDays).FirstOrDefault();
                var highestRated = proposals.OrderByDescending(p => p.AverageRating).FirstOrDefault();

                if (lowestPrice != null) lowestPrice.IsLowestPrice = true;
                if (fastestDelivery != null) fastestDelivery.IsFastestDelivery = true;
                if (highestRated != null && highestRated.AverageRating > 0) highestRated.IsHighestRated = true;
            }

            result.Add(new DemandWithProposalsDto
            {
                Id = demand.Id,
                Title = demand.Title,
                Description = demand.Description,
                Quantity = demand.Quantity,
                Unit = demand.Unit,
                CategoryName = demand.Category?.Name,
                Status = demand.Status,
                StatusName = GetDemandStatusName(demand.Status),
                CreatedAt = demand.CreatedAt,
                Deadline = demand.ExpiresAt,
                Proposals = proposals
            });
        }

        return result;
    }

    public async Task<List<InquiryWithProposalsDto>> GetInquiriesWithProposalsAsync(int buyerVendorId)
    {
        var inquiries = await _context.ProductInquiries
            .Where(i => i.BuyerVendorId == buyerVendorId)
            .Include(i => i.Product)
                .ThenInclude(p => p!.Vendor)
            .Include(i => i.Responses)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var result = new List<InquiryWithProposalsDto>();

        foreach (var inquiry in inquiries)
        {
            var mainImage = await _context.ProductImages
                .Where(pi => pi.ProductId == inquiry.ProductId && pi.IsMain)
                .Select(pi => pi.Url)
                .FirstOrDefaultAsync();

            var proposals = inquiry.Responses.Select(r => new InquiryProposalDto
            {
                Id = r.Id,
                InquiryId = r.InquiryId,
                UnitPrice = r.UnitPrice,
                TotalPrice = r.TotalPrice,
                Currency = r.Currency,
                OfferedQuantity = r.OfferedQuantity,
                OfferedUnit = r.OfferedUnit,
                LeadTimeDays = r.LeadTimeDays,
                EstimatedDeliveryDate = r.EstimatedDeliveryDate,
                ValidUntil = r.ValidUntil,
                Notes = r.Notes,
                TermsAndConditions = r.TermsAndConditions,
                Status = r.Status,
                StatusName = GetInquiryProposalStatusName(r.Status),
                CreatedAt = r.CreatedAt,
                IsReadByBuyer = r.IsReadByBuyer
            }).ToList();

            result.Add(new InquiryWithProposalsDto
            {
                Id = inquiry.Id,
                ProductId = inquiry.ProductId,
                ProductName = inquiry.Product?.Name ?? "",
                ProductImageUrl = mainImage,
                SellerVendorId = inquiry.SellerVendorId,
                SellerCompanyName = inquiry.Product?.Vendor?.CompanyName ?? "",
                Quantity = inquiry.Quantity,
                Message = inquiry.Message,
                Status = inquiry.Status,
                StatusName = GetInquiryStatusName(inquiry.Status),
                CreatedAt = inquiry.CreatedAt,
                Proposals = proposals
            });
        }

        return result;
    }

    public async Task<ProposalCompareStatsDto> GetProposalCompareStatsAsync(int buyerVendorId)
    {
        // Alıcının demand ID'leri
        var demandIds = await _context.PublicDemands
            .Where(d => d.VendorId == buyerVendorId)
            .Select(d => d.Id)
            .ToListAsync();

        return new ProposalCompareStatsDto
        {
            TotalDemandsWithProposals = await _context.PublicDemands
                .CountAsync(d => d.VendorId == buyerVendorId &&
                    _context.DemandResponses.Any(r => r.DemandId == d.Id)),
            TotalInquiriesWithProposals = await _context.ProductInquiries
                .CountAsync(i => i.BuyerVendorId == buyerVendorId &&
                    _context.ProductInquiryResponses.Any(r => r.InquiryId == i.Id)),
            TotalPendingProposals = await _context.DemandResponses
                .CountAsync(r => demandIds.Contains(r.DemandId) && r.Status == 1),
            TotalShortlistedProposals = await _context.DemandResponses
                .CountAsync(r => demandIds.Contains(r.DemandId) && r.Status == 3),
            TotalAcceptedProposals = await _context.DemandResponses
                .CountAsync(r => demandIds.Contains(r.DemandId) && r.Status == 4)
        };
    }

    public async Task<ServiceResult> UpdateProposalStatusAsync(int buyerVendorId, int proposalId, ProposalActionDto action)
    {
        var proposal = await _context.DemandResponses
            .Include(r => r.Demand)
            .FirstOrDefaultAsync(r => r.Id == proposalId && r.Demand.VendorId == buyerVendorId);

        if (proposal == null)
            return ServiceResult.Fail("Teklif bulunamadı.");

        switch (action.Action.ToLower())
        {
            case "view":
                if (proposal.Status == 1) // Pending
                {
                    proposal.Status = 2; // Viewed
                    proposal.ViewedAt = DateTime.UtcNow;
                }
                break;
            case "shortlist":
                proposal.Status = 3; // Shortlisted
                break;
            case "accept":
                proposal.Status = 4; // Accepted
                // Diğer teklifleri reddet
                var otherProposals = await _context.DemandResponses
                    .Where(r => r.DemandId == proposal.DemandId && r.Id != proposalId && r.Status < 4)
                    .ToListAsync();
                foreach (var other in otherProposals)
                {
                    other.Status = 5; // Rejected
                    other.RejectionReason = "Başka bir teklif kabul edildi.";
                }
                // Talebi kapat
                proposal.Demand.Status = 3; // Closed
                break;
            case "reject":
                proposal.Status = 5; // Rejected
                proposal.RejectionReason = action.RejectionReason;
                break;
            default:
                return ServiceResult.Fail("Geçersiz işlem.");
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Teklif durumu güncellendi.");
    }

    public async Task<ServiceResult> UpdateInquiryProposalStatusAsync(int buyerVendorId, int proposalId, ProposalActionDto action)
    {
        var proposal = await _context.ProductInquiryResponses
            .Include(r => r.Inquiry)
            .FirstOrDefaultAsync(r => r.Id == proposalId && r.Inquiry!.BuyerVendorId == buyerVendorId);

        if (proposal == null)
            return ServiceResult.Fail("Teklif bulunamadı.");

        switch (action.Action.ToLower())
        {
            case "view":
                if (!proposal.IsReadByBuyer)
                {
                    proposal.IsReadByBuyer = true;
                    proposal.ReadByBuyerAt = DateTime.UtcNow;
                }
                break;
            case "accept":
                proposal.Status = ProductInquiryResponseStatuses.Accepted.Id;
                // İsteği güncelle
                if (proposal.Inquiry != null)
                {
                    proposal.Inquiry.Status = ProductInquiryStatuses.Accepted.Id;
                }
                break;
            case "reject":
                proposal.Status = ProductInquiryResponseStatuses.Rejected.Id;
                break;
            default:
                return ServiceResult.Fail("Geçersiz işlem.");
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Teklif durumu güncellendi.");
    }

    private static string GetProposalStatusName(int status)
    {
        return status switch
        {
            1 => "Beklemede",
            2 => "Görüldü",
            3 => "Kısa Liste",
            4 => "Kabul Edildi",
            5 => "Reddedildi",
            _ => "Bilinmiyor"
        };
    }

    private static string GetDemandStatusName(int status)
    {
        return status switch
        {
            0 => "Taslak",
            1 => "Onay Bekliyor",
            2 => "Aktif",
            3 => "Kapatıldı",
            4 => "İptal Edildi",
            5 => "Süresi Doldu",
            _ => "Bilinmiyor"
        };
    }

    private static string GetInquiryStatusName(int status)
    {
        return status switch
        {
            1 => "Beklemede",
            2 => "Yanıt Verildi",
            3 => "Kabul Edildi",
            4 => "Reddedildi",
            5 => "İptal Edildi",
            _ => "Bilinmiyor"
        };
    }

    private static string GetInquiryProposalStatusName(int status)
    {
        return status switch
        {
            1 => "Beklemede",
            2 => "Kabul Edildi",
            3 => "Reddedildi",
            _ => "Bilinmiyor"
        };
    }

    // ============================================
    // HELPER METODLAR
    // ============================================

    private async Task<SupplierListDto> MapToSupplierListDtoAsync(Vendor vendor, int buyerVendorId)
    {
        // Kategorileri bul
        var categories = await _context.Products
            .Where(p => p.VendorId == vendor.Id && p.CategoryId.HasValue)
            .Include(p => p.Category)
            .Select(p => new { p.CategoryId, p.Category!.Name })
            .Distinct()
            .Take(5)
            .ToListAsync();

        // Adres bilgisi
        var address = await _context.Addresses
            .Where(a => a.VendorId == vendor.Id)
            .Include(a => a.CountryEntity)
            .FirstOrDefaultAsync();

        // Rating
        var avgRating = await _context.VendorReviews
            .Where(r => r.ReviewedVendorId == vendor.Id)
            .AverageAsync(r => (decimal?)r.OverallRating) ?? 0;

        var reviewCount = await _context.VendorReviews
            .CountAsync(r => r.ReviewedVendorId == vendor.Id);

        // Favori mi?
        var favorite = await _context.FavoriteVendors
            .FirstOrDefaultAsync(f => f.BuyerVendorId == buyerVendorId && f.SellerVendorId == vendor.Id);

        // Capability'ler
        var capabilityIds = await _context.VendorCapabilityMappings
            .Where(m => m.VendorId == vendor.Id)
            .Select(m => m.CapabilityId)
            .ToListAsync();
        var capabilities = capabilityIds
            .Select(id => Capabilities.GetById(id)?.SystemName)
            .Where(name => name != null)
            .ToList();

        // Supplier profile
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(p => p.VendorId == vendor.Id);

        return new SupplierListDto
        {
            Id = vendor.Id,
            CompanyName = vendor.CompanyName,
            LogoUrl = vendor.LogoUrl,
            Website = vendor.Website,
            CountryId = address?.CountryId,
            CountryName = address?.CountryEntity?.Name,
            City = address?.City,
            ProductCount = await _context.Products.CountAsync(p => p.VendorId == vendor.Id && p.ProductStatusId == ProductStatuses.Active.Id),
            CompletedOrderCount = await _context.Orders.CountAsync(o => o.SellerVendorId == vendor.Id && o.Status == OrderStatuses.Completed.Id),
            AverageRating = avgRating,
            ReviewCount = reviewCount,
            Categories = categories.Select(c => c.Name).ToList(),
            CategoryIds = categories.Where(c => c.CategoryId.HasValue).Select(c => c.CategoryId!.Value).ToList(),
            IsFavorite = favorite != null,
            FavoriteId = favorite?.Id,
            ShortDescription = profile?.ShortDescription,
            YearsInBusiness = 0, // SupplierProfile'da bu alan yok
            IsVerified = vendor.IsVerified,
            Capabilities = capabilities
        };
    }
}
