namespace Bridgo.DTOs.Catalog;

// ============================================
// PUBLIC CATALOG DTO'LARI
// ============================================

/// <summary>
/// Katalog urun listesi icin DTO
/// </summary>
public class CatalogProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public bool InStock { get; set; }
    public int StockQuantity { get; set; }
    public bool IsFeatured { get; set; }

    // Kategori
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategorySlug { get; set; }

    // Satici
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? VendorSlug { get; set; }
    public string? VendorCountry { get; set; }

    // Ana gorsel
    public string? MainImageUrl { get; set; }

    // Fiyat esikleri varsa min fiyat
    public decimal? MinBulkPrice { get; set; }
    public int? MinBulkQuantity { get; set; }
}

/// <summary>
/// Katalog urun detay DTO
/// </summary>
public class CatalogProductDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }

    // Fiyat
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string Currency { get; set; } = "TRY";

    // Durum
    public bool IsFeatured { get; set; }

    // Stok
    public bool InStock { get; set; }
    public int StockQuantity { get; set; }
    public bool AllowBackorder { get; set; }

    // Fiziksel
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }

    // Uluslararasi
    public string? HSCode { get; set; }
    public string? CountryOfOrigin { get; set; }

    // Kategori
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategorySlug { get; set; }
    public List<CategoryBreadcrumbDto> CategoryBreadcrumb { get; set; } = new();

    // Satici
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? VendorSlug { get; set; }
    public string? VendorCountry { get; set; }
    public string? VendorCity { get; set; }
    public bool VendorIsVerified { get; set; }

    // Gorseller
    public List<CatalogProductImageDto> Images { get; set; } = new();

    // Fiyat Esikleri
    public List<CatalogPriceTierDto> PriceTiers { get; set; } = new();

    // Benzer urunler
    public List<CatalogProductListDto> RelatedProducts { get; set; } = new();

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    // Etiketler
    public List<string> Tags { get; set; } = new();

    // Paketleme Bilgileri
    public List<CatalogPackagingDto> Packagings { get; set; } = new();

    /// <summary>
    /// Varsayılan satış birimi (IsDefault=true olan paketleme)
    /// </summary>
    public CatalogPackagingDto? SalesUnit { get; set; }

    public DateTime? PublishedAt { get; set; }
}

/// <summary>
/// Urun gorseli DTO
/// </summary>
public class CatalogProductImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Fiyat esigi DTO
/// </summary>
public class CatalogPriceTierDto
{
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public decimal DiscountPercent { get; set; }
}

/// <summary>
/// Paketleme bilgisi DTO (Catalog icin - GS1 Standardina uygun)
/// </summary>
public class CatalogPackagingDto
{
    public int Id { get; set; }
    public int UnitId { get; set; }

    /// <summary>
    /// Birim adi (fallback - ceviri yoksa kullanilir)
    /// </summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// Birim cevirisi icin resource key (UnitType.Pack, UnitType.Box, vb.)
    /// View'da Html.T(UnitResourceKey, UnitName) ile kullanilir
    /// </summary>
    public string UnitResourceKey { get; set; } = string.Empty;

    public string? UnitIcon { get; set; }

    /// <summary>
    /// Bu birimde kac adet temel birim var
    /// Ornek: 1 Carton = 500 Piece
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Varsayilan satis birimi mi
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Minimum siparis miktari (bu birim cinsinden)
    /// </summary>
    public int? MinOrderQuantity { get; set; }
}

/// <summary>
/// Kategori breadcrumb DTO
/// </summary>
public class CategoryBreadcrumbDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

/// <summary>
/// Katalog kategori DTO (sidebar icin)
/// </summary>
public class CatalogCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? ImageUrl { get; set; }
    public int ProductCount { get; set; }
    public int? ParentId { get; set; }
    public int Level { get; set; }
    public List<CatalogCategoryDto> Children { get; set; } = new();
}

/// <summary>
/// Katalog filtre DTO
/// </summary>
public class CatalogFilterDto
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public List<int>? CategoryIds { get; set; } // Alt kategoriler dahil (internal kullanim)
    public int? VendorId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Currency { get; set; }
    public bool? InStock { get; set; }
    public bool? IsFeatured { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? SortBy { get; set; } = "newest"; // newest, price_asc, price_desc, name, popular
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 24;

    /// <summary>
    /// Secilen ozellik degerleri (faceted filtering)
    /// </summary>
    public List<int>? AttributeValueIds { get; set; }
}

/// <summary>
/// Sayfalamali katalog sonucu
/// </summary>
public class CatalogSearchResultDto
{
    public List<CatalogProductListDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    // Aktif filtreler
    public CatalogFilterDto AppliedFilters { get; set; } = new();

    // Fiyat araligi (filtreleme icin)
    public decimal? MinPriceInResults { get; set; }
    public decimal? MaxPriceInResults { get; set; }

    /// <summary>
    /// Faceted filtering icin ozellik filtreleri
    /// </summary>
    public List<CatalogFacetDto> Facets { get; set; } = new();
}

/// <summary>
/// Katalog istatistikleri
/// </summary>
public class CatalogStatsDto
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalVendors { get; set; }
    public int FeaturedProducts { get; set; }
}

/// <summary>
/// One cikan urunler ve kategoriler (anasayfa icin)
/// </summary>
public class CatalogFeaturedDto
{
    public List<CatalogProductListDto> FeaturedProducts { get; set; } = new();
    public List<CatalogProductListDto> NewProducts { get; set; } = new();
    public List<CatalogCategoryDto> PopularCategories { get; set; } = new();
}

// ============================================
// FACETED FILTERING DTO'LARI
// ============================================

/// <summary>
/// Faceted filtering icin ozellik filtresi
/// </summary>
public class CatalogFacetDto
{
    public int AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string? AttributeNameResourceKey { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public List<CatalogFacetValueDto> Values { get; set; } = new();
}

/// <summary>
/// Facet degeri ve urun sayisi
/// </summary>
public class CatalogFacetValueDto
{
    public int ValueId { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? ValueResourceKey { get; set; }
    public string? AdditionalData { get; set; }
    public int ProductCount { get; set; }
}
