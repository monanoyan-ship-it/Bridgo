using Bridgo.DTOs.Vendor;
using Bridgo.Models.Entities;
using Bridgo.Models.Identity;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Vendor islemleri servisi
/// NOT: Join request/invitation islemleri ITeamService'e tasinmistir
/// </summary>
public interface IVendorService
{
    // ========================================
    // VENDOR CRUD
    // ========================================

    /// <summary>
    /// Yeni Vendor + Address olusturur ve kullaniciyi vendor'a baglar
    /// </summary>
    Task<VendorSetupResultDto> SetupVendorAsync(VendorSetupDto dto, int userId, string? createdBy = null);

    /// <summary>
    /// Vendor bilgisini getirir
    /// </summary>
    Task<VendorDto?> GetByIdAsync(int id);

    /// <summary>
    /// Kullanicinin Vendor bilgisini getirir
    /// </summary>
    Task<VendorDto?> GetByUserIdAsync(int userId);

    /// <summary>
    /// Kullanicinin Vendor'i var mi kontrol eder
    /// </summary>
    Task<bool> UserHasVendorAsync(int userId);

    /// <summary>
    /// Vendor'a ait adresleri getirir
    /// </summary>
    Task<IEnumerable<AddressDto>> GetAddressesByVendorIdAsync(int vendorId);

    /// <summary>
    /// Email'in baska bir Vendor tarafindan kullanilip kullanilmadigini kontrol eder
    /// </summary>
    Task<bool> EmailExistsAsync(string email, int? excludeVendorId = null);

    /// <summary>
    /// Vendor'i gunceller (profil tamamlama vb.)
    /// </summary>
    Task<bool> UpdateVendorAsync(int id, VendorDto dto, string? updatedBy = null);

    // ========================================
    // SEARCH
    // ========================================

    /// <summary>
    /// Vendor arama (isim veya sehir)
    /// </summary>
    Task<IEnumerable<VendorSearchResultDto>> SearchVendorsAsync(string searchTerm, int maxResults = 20);
}
