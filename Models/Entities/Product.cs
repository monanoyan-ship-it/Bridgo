using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Satici urunleri
/// VendorId ile multi-tenant izolasyon
/// </summary>
public class Product : BaseEntity
{
    // === TEMEL BILGILER ===
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? SKU { get; set; }  // Stok Kodu

    [MaxLength(100)]
    public string? Barcode { get; set; }  // Barkod (EAN, UPC, vb.)

    [MaxLength(4000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    // === FIYAT BILGILERI ===
    public decimal Price { get; set; }  // Ana satis fiyati

    public decimal? CompareAtPrice { get; set; }  // Karsilastirma fiyati (indirimli gosterim icin)

    public decimal? CostPrice { get; set; }  // Maliyet fiyati

    // Para birimi - Country tablosundan CurrencyCode listesiyle combobox dolar
    [MaxLength(10)]
    public string Currency { get; set; } = "TRY";  // ISO 4217 (TRY, USD, EUR, vb.)

    // === STOK BILGILERI ===
    //TODO: Stok bilgileri warehouse bazinda tutulacak
    public int StockQuantity { get; set; } = 0;

    public int? MinStockLevel { get; set; }  // Minimum stok seviyesi (alarm icin)

    public bool TrackStock { get; set; } = true;  // Stok takibi yapilsin mi

    public bool AllowBackorder { get; set; } = false;  // Stokta yokken siparis alinsin mi

    // === FIZIKSEL OZELLIKLER ===
    /// <summary>
    /// Temel satis birimi (UnitTypes'tan ID)
    /// Ornek: Piece(1), Kilogram(11), Meter(20), Liter(30)
    /// </summary>
    public int? SalesUnitId { get; set; }

    public decimal? Weight { get; set; }  // Agirlik (kg)

    public decimal? Length { get; set; }  // Uzunluk (cm)

    public decimal? Width { get; set; }   // Genislik (cm)

    public decimal? Height { get; set; }  // Yukseklik (cm)

    // === MARKA VE MODEL ===
    /// <summary>
    /// Urun markasi - Gumruk beyannamesinde zorunlu alan
    /// </summary>
    [MaxLength(100)]
    public string? Brand { get; set; }

    /// <summary>
    /// Urun modeli - Gumruk beyannamesinde zorunlu alan
    /// </summary>
    [MaxLength(100)]
    public string? Model { get; set; }

    // === ULUSLARARASI TICARET ===
    /// <summary>
    /// Harmonized System Code - Gumruk tarife numarasi (GTIP)
    /// 6-10 haneli uluslararasi standart (ilk 6 hane evrensel, sonrasi ulkeye ozel)
    /// Ornek: 0901.21.00 (Kavrulmus kahve)
    /// Beyanname kutu 33
    /// </summary>
    [MaxLength(20)]
    public string? HSCode { get; set; }

    /// <summary>
    /// Global Trade Item Number - Uluslararasi urun tanimlayici
    /// EAN-13, UPC-A, ISBN, EAN-8, GTIN-14 gibi standartlari kapsar
    /// 8, 12, 13 veya 14 haneli olabilir
    /// </summary>
    [MaxLength(14)]
    public string? GTIN { get; set; }

    /// <summary>
    /// Mensei ulke (Country of Origin) - ISO 3166-1 alpha-2
    /// Gumruk beyannamesinde zorunlu alan (Beyanname kutu 34)
    /// Urunun uretildigi/imal edildigi ulke
    /// </summary>
    [MaxLength(2)]
    public string? CountryOfOrigin { get; set; }

    // === KATEGORI VE ORGANIZASYON ===
    public int? CategoryId { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }  // Virgul ile ayrilmis etiketler

    public int DisplayOrder { get; set; } = 0;

    // === DURUM ===
    /// <summary>
    /// Urun durumu (ProductStatuses static class'tan ID)
    /// </summary>
    public int ProductStatusId { get; set; } = 1; // Default: Draft

    public bool IsFeatured { get; set; } = false;  // One cikan urun

    public DateTime? PublishedAt { get; set; }  // Yayinlanma tarihi

    // === SEO ===
    [MaxLength(200)]
    public string? MetaTitle { get; set; }

    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    //TODO: Bunu bizim oluşturmamız lazım kullanıcıyı ilgilendirmemeli ve uniq olmalı
    [MaxLength(200)]
    public string? Slug { get; set; }  // URL-friendly isim

    // === MULTI-TENANT ===
    public int VendorId { get; set; }

    // === NAVIGATION PROPERTIES ===
    public Vendor Vendor { get; set; } = null!;
    public ProductCategory? Category { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductPriceTier> PriceTiers { get; set; } = new List<ProductPriceTier>();
    public ICollection<ProductWarehouseStock> WarehouseStocks { get; set; } = new List<ProductWarehouseStock>();
    public ICollection<ProductPackaging> Packagings { get; set; } = new List<ProductPackaging>();
    public ICollection<ProductAttributeMapping> AttributeMappings { get; set; } = new List<ProductAttributeMapping>();
}
