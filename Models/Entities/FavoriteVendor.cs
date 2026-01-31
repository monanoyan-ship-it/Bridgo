namespace Bridgo.Models.Entities;

/// <summary>
/// Buyer'ın favori tedarikçileri
/// </summary>
public class FavoriteVendor : BaseEntity
{
    /// <summary>
    /// Favoriye ekleyen firma (Buyer)
    /// </summary>
    public int BuyerVendorId { get; set; }
    public Vendor? BuyerVendor { get; set; }

    /// <summary>
    /// Favori eklenen firma (Seller)
    /// </summary>
    public int SellerVendorId { get; set; }
    public Vendor? SellerVendor { get; set; }

    /// <summary>
    /// Favori notu (opsiyonel)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Etiketler (virgülle ayrılmış) - ör: "hızlı teslimat, kaliteli ürün"
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Favoriye eklenme tarihi
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Favoriye ekleyen kullanıcı
    /// </summary>
    public int AddedByUserId { get; set; }
}
