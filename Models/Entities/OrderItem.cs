namespace Bridgo.Models.Entities;

/// <summary>
/// Siparis kalemi
/// </summary>
public class OrderItem : BaseEntity
{
    /// <summary>
    /// Siparis ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Urun ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Depo ID (opsiyonel)
    /// </summary>
    public int? WarehouseId { get; set; }

    /// <summary>
    /// Siparis miktari
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Birim (Adet, Kg, vb.)
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Birim fiyat
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Toplam fiyat (Quantity * UnitPrice)
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Kargoya verilen miktar (kismi teslimat takibi)
    /// </summary>
    public int ShippedQuantity { get; set; }

    /// <summary>
    /// Teslim edilen miktar
    /// </summary>
    public int DeliveredQuantity { get; set; }

    // === GÜMRÜK BEYANNAMESI ALANLARI (Kalem Bazlı) ===

    /// <summary>
    /// GTİP Kodu (HS Code) - Üründen kopyalanır, override edilebilir
    /// Beyanname kutu 33
    /// </summary>
    public string? HSCode { get; set; }

    /// <summary>
    /// Menşe ülke kodu - Üründen kopyalanır, override edilebilir
    /// ISO 3166-1 alpha-2 (TR, CN, DE, vb.)
    /// Beyanname kutu 34
    /// </summary>
    public string? CountryOfOrigin { get; set; }

    /// <summary>
    /// Ürün ticari tanımı - Sipariş anında snapshot
    /// Gümrük beyannamesinde kullanılır
    /// </summary>
    public string? ProductDescription { get; set; }

    /// <summary>
    /// Marka - Sipariş anında snapshot
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Model - Sipariş anında snapshot
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Brüt ağırlık (kg) - Ambalaj dahil
    /// </summary>
    public decimal? GrossWeight { get; set; }

    /// <summary>
    /// Net ağırlık (kg) - Sadece ürün
    /// </summary>
    public decimal? NetWeight { get; set; }

    /// <summary>
    /// FOB birim fiyatı
    /// </summary>
    public decimal? FobUnitPrice { get; set; }

    /// <summary>
    /// Kalem FOB tutarı
    /// </summary>
    public decimal? FobTotalPrice { get; set; }

    /// <summary>
    /// Kalem CIF tutarı (navlun ve sigorta payı dahil)
    /// </summary>
    public decimal? CifTotalPrice { get; set; }

    /// <summary>
    /// İstatistiki kıymet (USD)
    /// </summary>
    public decimal? StatisticalValue { get; set; }

    /// <summary>
    /// Muafiyet kodu (varsa)
    /// Beyanname kutu 36 - Tercihli tarife uygulaması
    /// </summary>
    public string? ExemptionCode { get; set; }

    /// <summary>
    /// Tamamlayıcı ölçü birimi miktarı
    /// Bazı GTİP'ler için zorunlu (litre, adet, m2, vb.)
    /// </summary>
    public decimal? SupplementaryQuantity { get; set; }

    /// <summary>
    /// Tamamlayıcı ölçü birimi
    /// </summary>
    public string? SupplementaryUnit { get; set; }

    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
    public virtual Warehouse? Warehouse { get; set; }
    public virtual ICollection<OrderShipmentItem> ShipmentItems { get; set; } = new List<OrderShipmentItem>();
}
