namespace Bridgo.DTOs.ProductReview;

/// <summary>
/// Review olusturma
/// </summary>
public class ProductReviewCreateDto
{
    public int ProductId { get; set; }
    public int? OrderId { get; set; }
    public int OverallRating { get; set; }
    public int? QualityRating { get; set; }
    public int? ValueRating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public bool WouldRecommend { get; set; } = true;
}

/// <summary>
/// Review guncelleme
/// </summary>
public class ProductReviewUpdateDto
{
    public int OverallRating { get; set; }
    public int? QualityRating { get; set; }
    public int? ValueRating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public bool WouldRecommend { get; set; } = true;
}

/// <summary>
/// Review liste gorunumu
/// </summary>
public class ProductReviewListDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int BuyerVendorId { get; set; }
    public string BuyerCompanyName { get; set; } = string.Empty;
    public int SellerVendorId { get; set; }
    public string SellerCompanyName { get; set; } = string.Empty;
    public int OverallRating { get; set; }
    public int? QualityRating { get; set; }
    public int? ValueRating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public bool WouldRecommend { get; set; }
    public int StatusId { get; set; }
    public string? StatusName { get; set; }
    public string? StatusCssClass { get; set; }
    public bool IsVerified { get; set; }
    public string? SellerResponse { get; set; }
    public DateTime? SellerRespondedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Satici yaniti
/// </summary>
public class SellerReviewResponseDto
{
    public string Response { get; set; } = string.Empty;
}

/// <summary>
/// Urun review istatistikleri
/// </summary>
public class ProductReviewStatsDto
{
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public double? AverageQualityRating { get; set; }
    public double? AverageValueRating { get; set; }
    public int RecommendPercentage { get; set; }
    public int VerifiedCount { get; set; }
    public int PendingResponseCount { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
}
