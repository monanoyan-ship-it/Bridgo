namespace Bridgo.Models.Entities;

/// <summary>
/// Urun degerlendirmeleri (buyer -> urun bazli)
/// </summary>
public class ProductReview : BaseEntity
{
    /// <summary>
    /// Degerlendirilen urun
    /// </summary>
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>
    /// Degerlendiren firma (Buyer)
    /// </summary>
    public int BuyerVendorId { get; set; }
    public Vendor? BuyerVendor { get; set; }

    /// <summary>
    /// Urunun sahibi firma (Seller)
    /// </summary>
    public int SellerVendorId { get; set; }
    public Vendor? SellerVendor { get; set; }

    /// <summary>
    /// Ilgili siparis (opsiyonel - varsa IsVerified = true)
    /// </summary>
    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    /// <summary>
    /// Genel puan (1-5)
    /// </summary>
    public int OverallRating { get; set; }

    /// <summary>
    /// Urun kalitesi puani (1-5)
    /// </summary>
    public int? QualityRating { get; set; }

    /// <summary>
    /// Fiyat/deger orani puani (1-5)
    /// </summary>
    public int? ValueRating { get; set; }

    /// <summary>
    /// Yorum basligi
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Yorum metni
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Avantajlar
    /// </summary>
    public string? Pros { get; set; }

    /// <summary>
    /// Dezavantajlar
    /// </summary>
    public string? Cons { get; set; }

    /// <summary>
    /// Tavsiye eder misiniz?
    /// </summary>
    public bool WouldRecommend { get; set; } = true;

    /// <summary>
    /// Durum (TypeDefinitions - ProductReviewStatuses)
    /// </summary>
    public int StatusId { get; set; } = 1;

    /// <summary>
    /// Dogrulanmis satin alma (siparis varsa true)
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Satici yaniti
    /// </summary>
    public string? SellerResponse { get; set; }

    /// <summary>
    /// Satici yanit tarihi
    /// </summary>
    public DateTime? SellerRespondedAt { get; set; }

    /// <summary>
    /// Degerlendiren kullanici
    /// </summary>
    public int ReviewerUserId { get; set; }
}
