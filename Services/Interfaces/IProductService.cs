using Bridgo.DTOs.Product;
using Bridgo.Handlers;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Urun yonetimi servisi
/// </summary>
public interface IProductService
{
    // === PRODUCTS ===

    /// <summary>
    /// Vendor'in urunlerini listele
    /// </summary>
    Task<List<ProductListDto>> GetProductsAsync(int vendorId, ProductFilterDto? filter = null);

    /// <summary>
    /// Urun detayi getir
    /// </summary>
    Task<ProductDetailDto?> GetProductByIdAsync(int id, int vendorId);

    /// <summary>
    /// Yeni urun olustur
    /// </summary>
    Task<ServiceResult<int>> CreateProductAsync(ProductCreateUpdateDto dto, int vendorId);

    /// <summary>
    /// Urunu guncelle
    /// </summary>
    Task<ServiceResult> UpdateProductAsync(int id, ProductCreateUpdateDto dto, int vendorId);

    /// <summary>
    /// Urunu sil (soft delete)
    /// </summary>
    Task<ServiceResult> DeleteProductAsync(int id, int vendorId);

    /// <summary>
    /// Urun durumunu degistir
    /// </summary>
    Task<ServiceResult> UpdateProductStatusAsync(int id, int productStatusId, int vendorId);

    /// <summary>
    /// Toplu durum guncelleme
    /// </summary>
    Task<ServiceResult> BulkUpdateStatusAsync(List<int> productIds, int productStatusId, int vendorId);

    /// <summary>
    /// Toplu silme
    /// </summary>
    Task<ServiceResult> BulkDeleteAsync(List<int> productIds, int vendorId);

    // === PRODUCT IMAGES ===

    /// <summary>
    /// Urune gorsel ekle
    /// </summary>
    Task<ServiceResult<int>> AddProductImageAsync(int productId, ProductImageCreateDto dto, int vendorId);

    /// <summary>
    /// Gorsel sil
    /// </summary>
    Task<ServiceResult> DeleteProductImageAsync(int imageId, int vendorId);

    /// <summary>
    /// Ana gorsel sec
    /// </summary>
    Task<ServiceResult> SetMainImageAsync(int imageId, int vendorId);

    /// <summary>
    /// Gorsel sirasini guncelle
    /// </summary>
    Task<ServiceResult> UpdateImageOrderAsync(int productId, List<int> imageIds, int vendorId);

    // === PRICE TIERS (Miktar Bazli Fiyatlandirma) ===

    /// <summary>
    /// Urunun fiyat esiklerini getir
    /// </summary>
    Task<List<ProductPriceTierDto>> GetPriceTiersAsync(int productId, int vendorId);

    /// <summary>
    /// Fiyat esigi ekle
    /// </summary>
    Task<ServiceResult<int>> AddPriceTierAsync(int productId, ProductPriceTierCreateUpdateDto dto, int vendorId);

    /// <summary>
    /// Fiyat esigini guncelle
    /// </summary>
    Task<ServiceResult> UpdatePriceTierAsync(int tierId, ProductPriceTierCreateUpdateDto dto, int vendorId);

    /// <summary>
    /// Fiyat esigini sil
    /// </summary>
    Task<ServiceResult> DeletePriceTierAsync(int tierId, int vendorId);

    /// <summary>
    /// Urunun tum fiyat esiklerini kaydet (toplu guncelleme)
    /// </summary>
    Task<ServiceResult> SavePriceTiersAsync(int productId, List<ProductPriceTierCreateUpdateDto> tiers, int vendorId);

    // === CATEGORIES (Read-only - Admin yonetimli global kategoriler) ===

    /// <summary>
    /// Dropdown icin global kategori listesi (Admin tarafindan yonetiliyor)
    /// </summary>
    Task<List<ProductCategorySelectDto>> GetCategoriesForSelectAsync();

    // === TYPES (Dropdown icin tip listeleri) ===

    /// <summary>
    /// Urun durumlari (lokalize edilmis)
    /// </summary>
    List<TypeSelectDto> GetProductStatuses();

    // === STOCK (Depo Bazli Stok Yonetimi) ===

    /// <summary>
    /// Urunun depo bazli stok bilgilerini getir
    /// </summary>
    Task<List<ProductWarehouseStockDto>> GetProductStockAsync(int productId, int vendorId);

    /// <summary>
    /// Urunun depo bazli stok bilgilerini kaydet
    /// </summary>
    Task<ServiceResult> SaveProductStockAsync(int productId, List<ProductWarehouseStockUpdateDto> stocks, int vendorId);

    // === STOCK OVERVIEW (Stok Ozet Sayfasi) ===

    /// <summary>
    /// Stok ozet bilgilerini getir
    /// </summary>
    Task<StockOverviewDto> GetStockOverviewAsync(int vendorId);

    // === PUBLIC SEARCH (Teklif yonetimi icin urun referansi) ===

    /// <summary>
    /// Platform genelinde aktif urunleri ara (Teklif referansi icin)
    /// </summary>
    Task<List<ProductSearchResultDto>> SearchProductsAsync(string? search, int pageSize = 10);

    /// <summary>
    /// Dusuk stoklu urunleri getir
    /// </summary>
    Task<List<LowStockProductDto>> GetLowStockProductsAsync(int vendorId);

    /// <summary>
    /// Depo bazli stok ozeti
    /// </summary>
    Task<List<WarehouseStockSummaryDto>> GetStockByWarehouseAsync(int vendorId);

    // === PACKAGING (Paketleme Bilgileri) ===

    /// <summary>
    /// Urunun paketleme bilgilerini getir
    /// </summary>
    Task<List<ProductPackagingDto>> GetProductPackagingAsync(int productId, int vendorId);

    /// <summary>
    /// Urunun tum paketleme bilgilerini kaydet (toplu guncelleme)
    /// </summary>
    Task<ServiceResult> SaveProductPackagingAsync(int productId, List<ProductPackagingCreateUpdateDto> packagings, int vendorId);
}

/// <summary>
/// Urun filtreleme DTO
/// </summary>
public class ProductFilterDto
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? ProductStatusId { get; set; }
    public bool? IsFeatured { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public string? SortBy { get; set; } // name, price, stock, created
    public bool SortDesc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
