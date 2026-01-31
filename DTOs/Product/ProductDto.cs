namespace Bridgo.DTOs.Product;

/// <summary>
/// Urun listeleme DTO
/// </summary>
public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public int StockQuantity { get; set; }
    public int ProductStatusId { get; set; }
    public string? ProductStatusCode { get; set; }
    public string? ProductStatusName { get; set; }
    public bool IsFeatured { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? MainImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Urun detay DTO
/// </summary>
public class ProductDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }

    // Fiyat
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string Currency { get; set; } = "TRY";

    // Stok
    public int StockQuantity { get; set; }
    public int? MinStockLevel { get; set; }
    public bool TrackStock { get; set; }
    public bool AllowBackorder { get; set; }

    // Fiziksel
    public int? SalesUnitId { get; set; }
    public string? SalesUnitName { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }

    // Marka ve Model (Gümrük için zorunlu)
    public string? Brand { get; set; }
    public string? Model { get; set; }

    // Uluslararası Ticaret
    public string? HSCode { get; set; }          // GTİP kodu
    public string? GTIN { get; set; }            // Global Trade Item Number
    public string? CountryOfOrigin { get; set; } // Menşe ülke (ISO 2)

    // Kategori
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Tags { get; set; }
    public int DisplayOrder { get; set; }

    // Durum
    public int ProductStatusId { get; set; }
    public string? ProductStatusCode { get; set; }
    public string? ProductStatusName { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? PublishedAt { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Slug { get; set; }

    // Images
    public List<ProductImageDto> Images { get; set; } = new();

    // Price Tiers (Miktar bazli fiyat esikleri)
    public List<ProductPriceTierDto> PriceTiers { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Urun olusturma/guncelleme DTO
/// </summary>
public class ProductCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }

    // Fiyat
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string Currency { get; set; } = "TRY";

    // Stok
    public int StockQuantity { get; set; }
    public int? MinStockLevel { get; set; }
    public bool TrackStock { get; set; } = true;
    public bool AllowBackorder { get; set; }

    // Fiziksel
    public int? SalesUnitId { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }

    // Marka ve Model (Gümrük için zorunlu)
    public string? Brand { get; set; }
    public string? Model { get; set; }

    // Uluslararası Ticaret
    public string? HSCode { get; set; }          // GTİP kodu
    public string? GTIN { get; set; }            // Global Trade Item Number
    public string? CountryOfOrigin { get; set; } // Menşe ülke (ISO 2)

    // Kategori
    public int? CategoryId { get; set; }
    public string? Tags { get; set; }
    public int DisplayOrder { get; set; }

    // Durum
    public int ProductStatusId { get; set; } = 1; // Default: Draft
    public bool IsFeatured { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}

/// <summary>
/// Urun gorseli DTO
/// </summary>
public class ProductImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Title { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsMain { get; set; }
}

/// <summary>
/// Gorsel ekleme DTO
/// </summary>
public class ProductImageCreateDto
{
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Title { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsMain { get; set; }
}

/// <summary>
/// Fiyat esigi DTO (Miktar bazli fiyatlandirma)
/// Ornek: 1-99 adet = 25$, 100-999 = 24$, 1000+ = 23$
/// </summary>
public class ProductPriceTierDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Fiyat esigi olusturma/guncelleme DTO
/// </summary>
public class ProductPriceTierCreateUpdateDto
{
    public int? Id { get; set; }  // Guncelleme icin
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Depo bazli stok DTO
/// </summary>
public class ProductWarehouseStockDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity => Quantity - ReservedQuantity;
    public string? BinLocation { get; set; }
}

/// <summary>
/// Depo bazli stok guncelleme DTO
/// </summary>
public class ProductWarehouseStockUpdateDto
{
    public int WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
}

/// <summary>
/// Stok ozet DTO
/// </summary>
public class StockOverviewDto
{
    public int TotalProducts { get; set; }
    public int ProductsInStock { get; set; }
    public int ProductsOutOfStock { get; set; }
    public int LowStockProducts { get; set; }
    public decimal TotalStockValue { get; set; }
    public int TotalWarehouses { get; set; }
}

/// <summary>
/// Dusuk stoklu urun DTO
/// </summary>
public class LowStockProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? MainImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public int? MinStockLevel { get; set; }
    public int StockDifference { get; set; } // StockQuantity - MinStockLevel
    public string? CategoryName { get; set; }
}

/// <summary>
/// Depo bazli stok ozet DTO
/// </summary>
public class WarehouseStockSummaryDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public int ProductCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalReserved { get; set; }
    public decimal TotalAvailable { get; set; }
    public decimal TotalValue { get; set; }
}

/// <summary>
/// Teklif yonetimi icin basit urun arama sonucu DTO
/// </summary>
public class ProductSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? MainImageUrl { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? VendorName { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
}

/// <summary>
/// Dropdown icin tip secim DTO (Status, Type vb. icin)
/// </summary>
public class TypeSelectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CssClass { get; set; }
    public string? Icon { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// Urun paketleme bilgisi DTO (GS1 Standardina uygun)
/// </summary>
public class ProductPackagingDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string? UnitIcon { get; set; }
    public string? UnitCssClass { get; set; }
    public int Quantity { get; set; }
    public int ContainsCount { get; set; }
    public string? Barcode { get; set; }
    public string? SKU { get; set; }
    public decimal? GrossWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public int? MinOrderQuantity { get; set; }
    public int? OrderMultiple { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Urun paketleme olusturma/guncelleme DTO
/// </summary>
public class ProductPackagingCreateUpdateDto
{
    public int? Id { get; set; }
    public int UnitId { get; set; }
    public int Quantity { get; set; }
    public int ContainsCount { get; set; }
    public string? Barcode { get; set; }
    public string? SKU { get; set; }
    public decimal? GrossWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public int? MinOrderQuantity { get; set; }
    public int? OrderMultiple { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public int DisplayOrder { get; set; }
}
