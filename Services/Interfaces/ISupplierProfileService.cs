using Bridgo.DTOs.Supplier;

namespace Bridgo.Services.Interfaces;

public interface ISupplierProfileService
{
    // ============================================
    // SUPPLIER PROFILE - Public
    // ============================================

    /// <summary>
    /// Public tedarikci profillerini listele (SEO sayfasi icin)
    /// </summary>
    Task<SupplierSearchResultDto> GetPublicSuppliersAsync(SupplierFilterDto filter);

    /// <summary>
    /// Tedarikci profili getir (slug ile - SEO)
    /// </summary>
    Task<SupplierProfileDetailDto?> GetProfileBySlugAsync(string slug, bool incrementViewCount = false);

    /// <summary>
    /// Tedarikci profili getir (vendorId ile)
    /// </summary>
    Task<SupplierProfileDetailDto?> GetProfileByVendorIdAsync(int vendorId);

    /// <summary>
    /// Tedarikçiye iletişim mesajı gönder
    /// </summary>
    Task<bool> SendContactMessageAsync(int supplierId, SupplierContactDto dto);

    // ============================================
    // SUPPLIER PROFILE - Yonetim
    // ============================================

    /// <summary>
    /// Vendor icin profil olustur veya guncelle
    /// </summary>
    Task<SupplierProfileDetailDto> CreateOrUpdateProfileAsync(int vendorId, SupplierProfileCreateUpdateDto dto, string? updatedBy);

    /// <summary>
    /// Profil gorunurluğunu degistir
    /// </summary>
    Task<bool> SetProfileVisibilityAsync(int vendorId, bool isPubliclyVisible, string? updatedBy);

    /// <summary>
    /// Profil slug'ini getir/olustur
    /// </summary>
    Task<string> GetOrCreateSlugAsync(int vendorId);

    /// <summary>
    /// Profil istatistiklerini getir
    /// </summary>
    Task<SupplierStatsDto> GetProfileStatsAsync(int vendorId);

    // ============================================
    // CATEGORY SUBSCRIPTION - Kategori Takibi
    // ============================================

    /// <summary>
    /// Vendor'in takip ettigi kategorileri listele
    /// </summary>
    Task<List<CategorySubscriptionDto>> GetSubscriptionsAsync(int vendorId);

    /// <summary>
    /// Kategori takibi ekle
    /// </summary>
    Task<CategorySubscriptionDto> AddSubscriptionAsync(int vendorId, CategorySubscriptionCreateDto dto, string? createdBy);

    /// <summary>
    /// Kategori takibi guncelle
    /// </summary>
    Task<bool> UpdateSubscriptionAsync(int subscriptionId, int vendorId, CategorySubscriptionUpdateDto dto, string? updatedBy);

    /// <summary>
    /// Kategori takibini kaldir
    /// </summary>
    Task<bool> RemoveSubscriptionAsync(int subscriptionId, int vendorId, string? deletedBy);

    /// <summary>
    /// Toplu kategori takibi ekle (birden fazla kategori)
    /// </summary>
    Task<List<CategorySubscriptionDto>> AddBulkSubscriptionsAsync(int vendorId, List<int> categoryIds, string? createdBy);

    /// <summary>
    /// Kategori takip ediliyor mu kontrol et
    /// </summary>
    Task<bool> IsSubscribedToCategoryAsync(int vendorId, int categoryId);

    // ============================================
    // NOTIFICATION MATCHING
    // ============================================

    /// <summary>
    /// Yeni talep icin eslesen tedarikçileri bul
    /// </summary>
    Task<List<int>> FindMatchingSuppliersForDemandAsync(int demandId);

    /// <summary>
    /// Tedarikci abonelik bildirim sayisini artir
    /// </summary>
    Task IncrementNotificationCountAsync(int subscriptionId);
}
