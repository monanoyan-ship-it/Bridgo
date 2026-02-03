using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.ProductReview;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Urun degerlendirme servisi implementation
/// </summary>
public class ProductReviewService : IProductReviewService
{
    private readonly ApplicationDbContext _context;

    public ProductReviewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductReviewListDto>> GetMyReviewsAsync(int buyerVendorId)
    {
        var reviews = await _context.ProductReviews
            .Include(r => r.Product).ThenInclude(p => p!.Images)
            .Include(r => r.SellerVendor)
            .Where(r => r.BuyerVendorId == buyerVendorId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(MapToListDto).ToList();
    }

    public async Task<ProductReviewListDto> CreateAsync(ProductReviewCreateDto dto, int buyerVendorId, int reviewerUserId)
    {
        if (dto.OverallRating < 1 || dto.OverallRating > 5)
            throw new InvalidOperationException("Puan 1-5 arasinda olmalidir.");

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId && !p.IsDeleted);

        if (product == null)
            throw new InvalidOperationException("Urun bulunamadi.");

        if (product.VendorId == buyerVendorId)
            throw new InvalidOperationException("Kendi urunlerinize degerlendirme yapamazsiniz.");

        var existingReview = await _context.ProductReviews
            .AnyAsync(r => r.ProductId == dto.ProductId && r.BuyerVendorId == buyerVendorId && !r.IsDeleted);

        if (existingReview)
            throw new InvalidOperationException("Bu urun icin zaten bir degerlendirmeniz var.");

        // Siparis dogrulama
        bool isVerified = false;
        if (dto.OrderId.HasValue)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId.Value
                    && o.BuyerVendorId == buyerVendorId
                    && o.Items.Any(i => i.ProductId == dto.ProductId));

            isVerified = order != null;
        }

        var review = new ProductReview
        {
            ProductId = dto.ProductId,
            BuyerVendorId = buyerVendorId,
            SellerVendorId = product.VendorId,
            OrderId = dto.OrderId,
            OverallRating = dto.OverallRating,
            QualityRating = dto.QualityRating,
            ValueRating = dto.ValueRating,
            Title = dto.Title?.Trim(),
            Comment = dto.Comment?.Trim(),
            Pros = dto.Pros?.Trim(),
            Cons = dto.Cons?.Trim(),
            WouldRecommend = dto.WouldRecommend,
            StatusId = ProductReviewStatuses.Ids.Published,
            IsVerified = isVerified,
            ReviewerUserId = reviewerUserId
        };

        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync();

        // Reload with includes for response
        var saved = await _context.ProductReviews
            .Include(r => r.Product).ThenInclude(p => p!.Images)
            .Include(r => r.BuyerVendor)
            .Include(r => r.SellerVendor)
            .FirstAsync(r => r.Id == review.Id);

        return MapToListDto(saved);
    }

    public async Task<bool> UpdateAsync(int id, ProductReviewUpdateDto dto, int buyerVendorId)
    {
        if (dto.OverallRating < 1 || dto.OverallRating > 5)
            throw new InvalidOperationException("Puan 1-5 arasinda olmalidir.");

        var review = await _context.ProductReviews
            .FirstOrDefaultAsync(r => r.Id == id && r.BuyerVendorId == buyerVendorId && !r.IsDeleted);

        if (review == null)
            return false;

        review.OverallRating = dto.OverallRating;
        review.QualityRating = dto.QualityRating;
        review.ValueRating = dto.ValueRating;
        review.Title = dto.Title?.Trim();
        review.Comment = dto.Comment?.Trim();
        review.Pros = dto.Pros?.Trim();
        review.Cons = dto.Cons?.Trim();
        review.WouldRecommend = dto.WouldRecommend;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id, int buyerVendorId)
    {
        var review = await _context.ProductReviews
            .FirstOrDefaultAsync(r => r.Id == id && r.BuyerVendorId == buyerVendorId && !r.IsDeleted);

        if (review == null)
            return false;

        review.IsDeleted = true;
        review.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CanReviewProductAsync(int productId, int buyerVendorId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);

        if (product == null || product.VendorId == buyerVendorId)
            return false;

        var hasReview = await _context.ProductReviews
            .AnyAsync(r => r.ProductId == productId && r.BuyerVendorId == buyerVendorId && !r.IsDeleted);

        return !hasReview;
    }

    public async Task<List<ProductReviewListDto>> GetIncomingReviewsAsync(int sellerVendorId)
    {
        var reviews = await _context.ProductReviews
            .Include(r => r.Product).ThenInclude(p => p!.Images)
            .Include(r => r.BuyerVendor)
            .Where(r => r.SellerVendorId == sellerVendorId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(MapToListDto).ToList();
    }

    public async Task<ProductReviewStatsDto> GetStatsAsync(int sellerVendorId)
    {
        var reviews = await _context.ProductReviews
            .Where(r => r.SellerVendorId == sellerVendorId
                && !r.IsDeleted
                && r.StatusId == ProductReviewStatuses.Ids.Published)
            .ToListAsync();

        if (!reviews.Any())
        {
            return new ProductReviewStatsDto
            {
                RatingDistribution = new Dictionary<int, int>
                {
                    { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
                }
            };
        }

        var stats = new ProductReviewStatsDto
        {
            TotalReviews = reviews.Count,
            AverageRating = Math.Round(reviews.Average(r => r.OverallRating), 1),
            RecommendPercentage = (int)Math.Round(reviews.Count(r => r.WouldRecommend) * 100.0 / reviews.Count),
            VerifiedCount = reviews.Count(r => r.IsVerified),
            PendingResponseCount = reviews.Count(r => r.SellerResponse == null),
            RatingDistribution = new Dictionary<int, int>
            {
                { 1, reviews.Count(r => r.OverallRating == 1) },
                { 2, reviews.Count(r => r.OverallRating == 2) },
                { 3, reviews.Count(r => r.OverallRating == 3) },
                { 4, reviews.Count(r => r.OverallRating == 4) },
                { 5, reviews.Count(r => r.OverallRating == 5) }
            }
        };

        var qualityRatings = reviews.Where(r => r.QualityRating.HasValue).ToList();
        if (qualityRatings.Any())
            stats.AverageQualityRating = Math.Round(qualityRatings.Average(r => r.QualityRating!.Value), 1);

        var valueRatings = reviews.Where(r => r.ValueRating.HasValue).ToList();
        if (valueRatings.Any())
            stats.AverageValueRating = Math.Round(valueRatings.Average(r => r.ValueRating!.Value), 1);

        return stats;
    }

    public async Task<bool> RespondToReviewAsync(int id, SellerReviewResponseDto dto, int sellerVendorId)
    {
        if (string.IsNullOrWhiteSpace(dto.Response))
            throw new InvalidOperationException("Yanit metni zorunludur.");

        var review = await _context.ProductReviews
            .FirstOrDefaultAsync(r => r.Id == id && r.SellerVendorId == sellerVendorId && !r.IsDeleted);

        if (review == null)
            return false;

        review.SellerResponse = dto.Response.Trim();
        review.SellerRespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private ProductReviewListDto MapToListDto(ProductReview review)
    {
        var status = ProductReviewStatuses.GetById(review.StatusId);
        var mainImage = review.Product?.Images?.FirstOrDefault(i => i.IsMain)
                        ?? review.Product?.Images?.FirstOrDefault();

        return new ProductReviewListDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            ProductName = review.Product?.Name ?? "",
            ProductImageUrl = mainImage?.Url,
            BuyerVendorId = review.BuyerVendorId,
            BuyerCompanyName = review.BuyerVendor?.CompanyName ?? "",
            SellerVendorId = review.SellerVendorId,
            SellerCompanyName = review.SellerVendor?.CompanyName ?? "",
            OverallRating = review.OverallRating,
            QualityRating = review.QualityRating,
            ValueRating = review.ValueRating,
            Title = review.Title,
            Comment = review.Comment,
            Pros = review.Pros,
            Cons = review.Cons,
            WouldRecommend = review.WouldRecommend,
            StatusId = review.StatusId,
            StatusName = status?.NameResourceKey,
            StatusCssClass = status?.CssClass,
            IsVerified = review.IsVerified,
            SellerResponse = review.SellerResponse,
            SellerRespondedAt = review.SellerRespondedAt,
            CreatedAt = review.CreatedAt
        };
    }
}
