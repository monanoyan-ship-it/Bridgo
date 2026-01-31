using Bridgo.DTOs.Catalog;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Public urun katalogu servisi (login gerektirmez)
/// </summary>
public interface ICatalogService
{
    // === PRODUCTS ===

    /// <summary>
    /// Katalog urunlerini listele (public, aktif urunler)
    /// </summary>
    Task<CatalogSearchResultDto> GetProductsAsync(CatalogFilterDto filter);

    /// <summary>
    /// Urun detayi getir (slug ile)
    /// </summary>
    Task<CatalogProductDetailDto?> GetProductBySlugAsync(string slug, bool incrementViewCount = false);

    /// <summary>
    /// Urun detayi getir (ID ile)
    /// </summary>
    Task<CatalogProductDetailDto?> GetProductByIdAsync(int id);

    /// <summary>
    /// Kategori urunlerini getir
    /// </summary>
    Task<CatalogSearchResultDto> GetProductsByCategoryAsync(string categorySlug, CatalogFilterDto filter);

    /// <summary>
    /// Satici urunlerini getir
    /// </summary>
    Task<CatalogSearchResultDto> GetProductsByVendorAsync(string vendorSlug, CatalogFilterDto filter);

    /// <summary>
    /// Benzer urunleri getir
    /// </summary>
    Task<List<CatalogProductListDto>> GetRelatedProductsAsync(int productId, int count = 8);

    /// <summary>
    /// One cikan ve yeni urunler (anasayfa icin)
    /// </summary>
    Task<CatalogFeaturedDto> GetFeaturedAsync();

    // === CATEGORIES ===

    /// <summary>
    /// Tum kategorileri tree yapida getir
    /// </summary>
    Task<List<CatalogCategoryDto>> GetCategoriesAsync();

    /// <summary>
    /// Kategori detayi getir (slug ile)
    /// </summary>
    Task<CatalogCategoryDto?> GetCategoryBySlugAsync(string slug);

    /// <summary>
    /// Kategori breadcrumb getir
    /// </summary>
    Task<List<CategoryBreadcrumbDto>> GetCategoryBreadcrumbAsync(int categoryId);

    // === STATS ===

    /// <summary>
    /// Katalog istatistikleri
    /// </summary>
    Task<CatalogStatsDto> GetStatsAsync();

    // === SEARCH SUGGESTIONS ===

    /// <summary>
    /// Arama onerileri (autocomplete)
    /// </summary>
    Task<List<string>> GetSearchSuggestionsAsync(string query, int count = 10);
}
