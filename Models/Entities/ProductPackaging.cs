using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Urun paketleme bilgileri (GS1 Standardina uygun)
/// Her urun icin farkli paketleme seviyeleri tanimlanabilir
/// Ornek: 500 Adet = 1 Paket, 10 Paket = 1 Koli, 20 Koli = 1 Palet
/// </summary>
public class ProductPackaging : BaseEntity
{
    public int ProductId { get; set; }

    /// <summary>
    /// Paketleme birimi (UnitTypes'tan: Pack=4, Box=5, Carton=6, Pallet=7, vb.)
    /// </summary>
    public int UnitId { get; set; }

    /// <summary>
    /// Bu birimde kac adet temel birim (Product.SalesUnitId) var
    /// Ornek: SalesUnit=Piece ise, 1 Pack = 500 Piece
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Bir onceki seviyeden kac tane bu seviyeyi olusturur
    /// Ornek: 10 Pack = 1 Carton (ContainsCount = 10)
    /// Ilk seviye icin bu deger Quantity ile aynidir
    /// </summary>
    public int ContainsCount { get; set; }

    /// <summary>
    /// Bu seviyeye ozel barkod (EAN-13, GTIN-14, vb.)
    /// </summary>
    [MaxLength(50)]
    public string? Barcode { get; set; }

    /// <summary>
    /// Bu seviyeye ozel SKU (Stok Kodu)
    /// </summary>
    [MaxLength(50)]
    public string? SKU { get; set; }

    /// <summary>
    /// Brut agirlik (kg) - paket dahil
    /// </summary>
    public decimal? GrossWeight { get; set; }

    /// <summary>
    /// Net agirlik (kg) - sadece urun
    /// </summary>
    public decimal? NetWeight { get; set; }

    /// <summary>
    /// Uzunluk (cm)
    /// </summary>
    public decimal? Length { get; set; }

    /// <summary>
    /// Genislik (cm)
    /// </summary>
    public decimal? Width { get; set; }

    /// <summary>
    /// Yukseklik (cm)
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// Bu seviyede minimum siparis miktari
    /// Ornek: En az 1 koli siparis verilmeli
    /// </summary>
    public int? MinOrderQuantity { get; set; }

    /// <summary>
    /// Bu seviyede siparis kati (siparis bu miktarin katlari olarak verilmeli)
    /// Ornek: 5'in katlari seklinde siparis (5, 10, 15...)
    /// </summary>
    public int? OrderMultiple { get; set; }

    /// <summary>
    /// Bu seviye satis icin aktif mi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Varsayilan satis birimi mi
    /// Urun bu paketleme seviyesiyle satilir
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Gosterim sirasi
    /// </summary>
    public int DisplayOrder { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}
