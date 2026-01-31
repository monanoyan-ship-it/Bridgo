using Bridgo.DTOs.Category;

namespace Bridgo.Services.Interfaces;

public interface ICategoryRequestService
{
    // ========== Kullanici islemleri ==========

    /// <summary>
    /// Vendor'in talep listesi
    /// </summary>
    Task<List<CategoryRequestDto>> GetVendorRequestsAsync(int vendorId, int? statusId = null);

    /// <summary>
    /// Talep detayi
    /// </summary>
    Task<CategoryRequestDto?> GetByIdAsync(int id, int? vendorId = null);

    /// <summary>
    /// Yeni talep olustur
    /// </summary>
    Task<CategoryRequestDto> CreateAsync(CategoryRequestCreateDto dto, int vendorId, int userId);

    /// <summary>
    /// Talep iptal et (sadece Pending durumunda)
    /// </summary>
    Task<bool> CancelAsync(int id, int vendorId);

    /// <summary>
    /// Kategori arama (mevcut kategoriler icinde)
    /// </summary>
    Task<List<CategorySearchResultDto>> SearchCategoriesAsync(string query, int? parentId = null);

    // ========== Admin islemleri ==========

    /// <summary>
    /// Tum talepler (admin)
    /// </summary>
    Task<List<CategoryRequestDto>> GetAllRequestsAsync(int? statusId = null);

    /// <summary>
    /// Bekleyen talep sayisi (admin)
    /// </summary>
    Task<int> GetPendingCountAsync();

    /// <summary>
    /// Talebi incele (onay/red)
    /// </summary>
    Task<ServiceResult<CategoryRequestDto>> ReviewRequestAsync(CategoryRequestReviewDto dto, int reviewedByUserId);

    /// <summary>
    /// Benzer kategori var mi kontrol et
    /// </summary>
    Task<List<CategorySearchResultDto>> FindSimilarCategoriesAsync(string name);
}
