using Bridgo.DTOs.CapabilityProfile;

namespace Bridgo.Services.Interfaces;

public interface ICapabilityProfileService
{
    /// <summary>
    /// Vendor'in belirli bir capability icin profilini getir
    /// </summary>
    Task<CapabilityProfileDto?> GetProfileAsync(int vendorId, int capabilityId);

    /// <summary>
    /// Profili yoksa olustur ve getir
    /// </summary>
    Task<CapabilityProfileDto> GetOrCreateProfileAsync(int vendorId, int capabilityId, string? createdBy);

    /// <summary>
    /// Profil olustur veya guncelle
    /// </summary>
    Task<CapabilityProfileDto> CreateOrUpdateProfileAsync(int vendorId, int capabilityId, CapabilityProfileCreateUpdateDto dto, string? updatedBy);

    /// <summary>
    /// Profil gorunurlugunu degistir
    /// </summary>
    Task<bool> SetVisibilityAsync(int vendorId, int capabilityId, bool isPubliclyVisible, string? updatedBy);

    /// <summary>
    /// Profil istatistiklerini getir
    /// </summary>
    Task<CapabilityProfileStatsDto> GetStatsAsync(int vendorId, int capabilityId);

    /// <summary>
    /// Slug ile public profil getir
    /// </summary>
    Task<CapabilityProfileDto?> GetBySlugAsync(string slug, bool incrementViewCount = false);

    /// <summary>
    /// Vendor'un tum profillerini getir (capability bazli)
    /// </summary>
    Task<List<CapabilityProfileDto>> GetVendorProfilesAsync(int vendorId);

    // === ADMIN METODLARI ===

    /// <summary>
    /// Onay bekleyen profilleri getir
    /// </summary>
    Task<List<ProfileApprovalListItemDto>> GetPendingApprovalsAsync();

    /// <summary>
    /// Profili onayla
    /// </summary>
    Task<bool> ApproveProfileAsync(int profileId, int approvedByUserId);

    /// <summary>
    /// Profili reddet
    /// </summary>
    Task<bool> RejectProfileAsync(int profileId, string reason, int rejectedByUserId);

    /// <summary>
    /// Admin icin profil detayi getir (onay sayfasi icin)
    /// </summary>
    Task<CapabilityProfileDto?> GetProfileByIdAsync(int profileId);
}
