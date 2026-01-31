namespace Bridgo.Models.Entities;

/// <summary>
/// Tedarikçi değerlendirmeleri (sipariş bazlı)
/// </summary>
public class VendorReview : BaseEntity
{
    /// <summary>
    /// Değerlendiren firma (Buyer)
    /// </summary>
    public int ReviewerVendorId { get; set; }
    public Vendor? ReviewerVendor { get; set; }

    /// <summary>
    /// Değerlendirilen firma (Seller)
    /// </summary>
    public int ReviewedVendorId { get; set; }
    public Vendor? ReviewedVendor { get; set; }

    /// <summary>
    /// İlgili sipariş (opsiyonel)
    /// </summary>
    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    /// <summary>
    /// Genel puan (1-5)
    /// </summary>
    public int OverallRating { get; set; }

    /// <summary>
    /// Ürün kalitesi puanı (1-5)
    /// </summary>
    public int? QualityRating { get; set; }

    /// <summary>
    /// Teslimat hızı puanı (1-5)
    /// </summary>
    public int? DeliveryRating { get; set; }

    /// <summary>
    /// İletişim puanı (1-5)
    /// </summary>
    public int? CommunicationRating { get; set; }

    /// <summary>
    /// Fiyat/değer oranı puanı (1-5)
    /// </summary>
    public int? ValueRating { get; set; }

    /// <summary>
    /// Yorum başlığı
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
    /// Tekrar iş yapar mısınız?
    /// </summary>
    public bool WouldRecommend { get; set; } = true;

    /// <summary>
    /// Değerlendirme tarihi
    /// </summary>
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Değerlendiren kullanıcı
    /// </summary>
    public int ReviewerUserId { get; set; }

    /// <summary>
    /// Yayınlandı mı? (admin onayı gerekebilir)
    /// </summary>
    public bool IsPublished { get; set; } = true;

    /// <summary>
    /// Satıcı yanıtı
    /// </summary>
    public string? SellerResponse { get; set; }

    /// <summary>
    /// Satıcı yanıt tarihi
    /// </summary>
    public DateTime? SellerRespondedAt { get; set; }
}
