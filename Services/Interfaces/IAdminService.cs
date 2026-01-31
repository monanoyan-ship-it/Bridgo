using Bridgo.Models.Entities;

namespace Bridgo.Services.Interfaces;

public interface IAdminService
{
    #region Dashboard
    Task<AdminDashboardStats> GetDashboardStatsAsync();
    Task<List<RecentVendorDto>> GetRecentVendorsAsync(int count = 5);
    Task<List<CapabilityStatDto>> GetCapabilityStatsAsync();
    #endregion

    #region Capabilities
    Task<List<AdminCapabilityDto>> GetCapabilitiesAsync();
    // Create/Update/Delete kaldirildi - Capabilities artik TypeDefinitions.cs'de static tanimli
    #endregion

    #region Modules (PlatformModules)
    /// <summary>
    /// Tum platform modullerini agac yapısında getirir
    /// </summary>
    Task<List<AdminModuleDto>> GetAllModulesAsync();

    /// <summary>
    /// Silinmis modulleri getirir (cop kutusu)
    /// </summary>
    Task<List<AdminModuleDto>> GetDeletedModulesAsync();

    /// <summary>
    /// Yeni platform modulu olusturur
    /// </summary>
    Task<ServiceResult<int>> CreateModuleAsync(AdminModuleCreateDto dto);

    /// <summary>
    /// Platform modulunu gunceller
    /// </summary>
    Task<ServiceResult> UpdateModuleAsync(int id, AdminModuleUpdateDto dto);

    /// <summary>
    /// Platform modulunu soft delete yapar
    /// </summary>
    Task<ServiceResult> DeleteModuleAsync(int id);

    /// <summary>
    /// Silinmis modulu geri yukler
    /// </summary>
    Task<ServiceResult> RestoreModuleAsync(int id);

    /// <summary>
    /// Modulu kalici olarak siler
    /// </summary>
    Task<ServiceResult> HardDeleteModuleAsync(int id);

    /// <summary>
    /// Bir modulun capability mapping'lerini getirir
    /// </summary>
    Task<List<ModuleCapabilityMappingDto>> GetModuleCapabilitiesAsync(int moduleId);

    /// <summary>
    /// Bir modulun capability mapping'lerini kaydeder
    /// </summary>
    Task<ServiceResult> SaveModuleCapabilitiesAsync(int moduleId, List<int> capabilityIds);

    /// <summary>
    /// Bir capability icin tum modulleri getirir (coklu secim icin)
    /// </summary>
    Task<List<CapabilityModuleSelectionDto>> GetCapabilityModulesAsync(int capabilityId);

    /// <summary>
    /// Bir capability'ye modulleri toplu olarak atar
    /// </summary>
    Task<ServiceResult> SaveCapabilityModulesAsync(int capabilityId, List<int> moduleIds);

    /// <summary>
    /// Capability'ye tek modul ekler (mapping olusturur)
    /// </summary>
    Task<ServiceResult> AddModuleToCapabilityAsync(int capabilityId, int moduleId);

    /// <summary>
    /// Capability'ye birden fazla modul ekler (coklu mapping)
    /// </summary>
    Task<ServiceResult> AddMultipleModulesToCapabilityAsync(int capabilityId, List<int> moduleIds);

    /// <summary>
    /// Capability'den modul cikarir (mapping siler)
    /// </summary>
    Task<ServiceResult> RemoveModuleFromCapabilityAsync(int capabilityId, int moduleId);
    #endregion

    #region Roles
    Task<List<AdminRoleDto>> GetRolesAsync();
    Task<ServiceResult<int>> CreateRoleAsync(AdminRoleCreateDto dto);
    Task<ServiceResult> UpdateRoleAsync(int id, AdminRoleCreateDto dto);
    Task<ServiceResult> DeleteRoleAsync(int id, int? targetRoleId = null);
    #endregion

    #region Role Permissions
    Task<List<ModulePermissionViewDto>> GetRolePermissionsAsync(int roleId);
    Task<List<CapabilityModulesGroupDto>> GetAllModulesGroupedByCapabilityAsync(int roleId);
    Task<ServiceResult> SaveRolePermissionsAsync(int roleId, List<PermissionSaveDto> permissions);
    #endregion

    #region Users
    Task<List<AdminUserDto>> GetUsersAsync(string? search = null, string? roleFilter = null);
    Task<AdminUserDetailDto?> GetUserByIdAsync(int id);
    Task<ServiceResult<int>> CreateUserAsync(AdminUserCreateDto dto);
    Task<ServiceResult> ToggleUserStatusAsync(int id);
    #endregion

    #region Vendors
    Task<List<AdminVendorDto>> GetVendorsAsync(string? search = null, int? statusFilter = null);
    Task<AdminVendorDetailDto?> GetVendorByIdAsync(int id);
    Task<ServiceResult> ApproveVendorAsync(int id);
    Task<ServiceResult> SuspendVendorAsync(int id);
    Task<ServiceResult> ReactivateVendorAsync(int id);
    #endregion

    #region Languages
    Task<List<AdminLanguageDto>> GetLanguagesAsync(bool onlyActive = false);
    Task<AdminLanguageDto?> GetLanguageByIdAsync(int id);
    Task<ServiceResult<int>> CreateLanguageAsync(AdminLanguageCreateDto dto);
    Task<ServiceResult> UpdateLanguageAsync(int id, AdminLanguageCreateDto dto);
    Task<ServiceResult> DeleteLanguageAsync(int id);
    Task<ServiceResult> SetDefaultLanguageAsync(int id);
    Task<ServiceResult> ToggleLanguageActiveAsync(int id);
    #endregion

    #region Localization Resources
    Task<LocalizationResourceListDto> GetLocalizationResourcesAsync(int languageId, string? search = null, int page = 1, int pageSize = 50);
    Task<ServiceResult<int>> CreateLocalizationResourceAsync(AdminLocalizationResourceCreateDto dto);
    Task<ServiceResult> UpdateLocalizationResourceAsync(int id, AdminLocalizationResourceUpdateDto dto);
    Task<ServiceResult> DeleteLocalizationResourceAsync(int id);
    Task<ServiceResult<int>> ImportLocalizationResourcesAsync(int languageId, List<LocalizationResourceImportDto> resources);
    Task<List<LocalizationResourceExportDto>> ExportLocalizationResourcesAsync(int languageId);
    Task<ServiceResult<int>> ImportLocalizationResourcesFromFileAsync(int languageId);
    #endregion

    #region Product Categories (Global - Admin yonetimli)
    Task<List<AdminCategoryDto>> GetCategoriesAsync();
    Task<AdminCategoryDto?> GetCategoryByIdAsync(int id);
    Task<ServiceResult<int>> CreateCategoryAsync(AdminCategoryCreateUpdateDto dto);
    Task<ServiceResult> UpdateCategoryAsync(int id, AdminCategoryCreateUpdateDto dto);
    Task<ServiceResult> DeleteCategoryAsync(int id, int? targetCategoryId = null, string? deletedBy = null);
    Task<ServiceResult> SeedCategoriesAsync();
    // Trash bin
    Task<List<AdminCategoryDto>> GetDeletedCategoriesAsync();
    Task<List<AdminCategoryDto>> GetPendingDeletionCategoriesAsync();
    Task<ServiceResult> RestoreCategoryAsync(int id);
    Task<ServiceResult> PermanentDeleteCategoryAsync(int id);
    Task<ServiceResult> ApproveDeletionAsync(int id, string reviewedBy, string? reviewNote = null);
    Task<ServiceResult> RejectDeletionAsync(int id, string reviewedBy, string? reviewNote = null);
    #endregion

    #region Geography (Countries & States)
    // Countries
    Task<List<AdminCountryDto>> GetCountriesAsync(string? search = null);
    Task<AdminCountryDto?> GetCountryByIdAsync(int id);
    Task<ServiceResult<int>> CreateCountryAsync(AdminCountryCreateUpdateDto dto);
    Task<ServiceResult> UpdateCountryAsync(int id, AdminCountryCreateUpdateDto dto);
    Task<ServiceResult> DeleteCountryAsync(int id);

    // States
    Task<List<AdminStateDto>> GetStatesAsync(int? countryId = null, string? search = null);
    Task<AdminStateDto?> GetStateByIdAsync(int id);
    Task<ServiceResult<int>> CreateStateAsync(AdminStateCreateUpdateDto dto);
    Task<ServiceResult> UpdateStateAsync(int id, AdminStateCreateUpdateDto dto);
    Task<ServiceResult> DeleteStateAsync(int id);

    // Country Translations & Status
    Task<AdminCountryDetailDto?> GetCountryDetailAsync(int id);
    Task<ServiceResult> UpdateCountryTranslationsAsync(int countryId, List<AdminCountryTranslationUpdateDto> translations);
    Task<ServiceResult> ToggleCountryActiveAsync(int id);

    // State Translations & Status
    Task<AdminStateDetailDto?> GetStateDetailAsync(int id);
    Task<ServiceResult> UpdateStateTranslationsAsync(int stateId, List<AdminStateTranslationUpdateDto> translations);
    Task<ServiceResult> ToggleStateActiveAsync(int id);
    #endregion

    #region Profile Approvals
    /// <summary>
    /// Onay bekleyen profilleri listele
    /// </summary>
    Task<List<AdminProfileApprovalDto>> GetPendingProfileApprovalsAsync();

    /// <summary>
    /// Profil detayini getir
    /// </summary>
    Task<AdminProfileDetailDto?> GetProfileByIdAsync(int id);

    /// <summary>
    /// Profili onayla
    /// </summary>
    Task<bool> ApproveProfileAsync(int profileId, int approvedByUserId);

    /// <summary>
    /// Profili reddet
    /// </summary>
    Task<bool> RejectProfileAsync(int profileId, string reason, int rejectedByUserId);
    #endregion
}

#region DTOs

public class ServiceResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public static ServiceResult Ok(string? message = null) => new() { Success = true, Message = message };
    public static ServiceResult Fail(string message) => new() { Success = false, Message = message };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data, string? message = null) => new() { Success = true, Data = data, Message = message };
    public new static ServiceResult<T> Fail(string message) => new() { Success = false, Message = message };
}

public class AdminDashboardStats
{
    public int VendorCount { get; set; }
    public int UserCount { get; set; }
    public int CapabilityCount { get; set; }
    public int RoleCount { get; set; }
}

public class RecentVendorDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int VendorStatusId { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

public class CapabilityStatDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int VendorCount { get; set; }
    public int Percentage { get; set; }
}

public class AdminCapabilityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int VendorCount { get; set; }
    public int ModuleCount { get; set; }
    public int RoleCount { get; set; }
}

// AdminCapabilityCreateDto kaldirildi - Capabilities artik TypeDefinitions.cs'de static tanimli

/// <summary>
/// Platform modulu goruntuleme DTO'su
/// </summary>
public class AdminModuleDto
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }

    /// <summary>
    /// Benzersiz tanimlayici - resource key (zorunlu ve unique)
    /// </summary>
    public string DisplayNameResourceKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public bool IsMenuSection { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsMenuItem { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// Bu modulu gorebilen capability ID'leri
    /// </summary>
    public List<int> CapabilityIds { get; set; } = new();

    /// <summary>
    /// Alt moduller (tree yapisi icin)
    /// </summary>
    public List<AdminModuleDto> Children { get; set; } = new();
}

/// <summary>
/// Platform modulu olusturma DTO'su
/// </summary>
public class AdminModuleCreateDto
{
    public int? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }

    /// <summary>
    /// Benzersiz tanimlayici - resource key (zorunlu ve unique)
    /// </summary>
    public string DisplayNameResourceKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public bool IsMenuSection { get; set; }
    public int DisplayOrder { get; set; } = 1;
    public bool IsMenuItem { get; set; } = true;
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Modul olusturulurken atanacak capability ID'leri
    /// </summary>
    public List<int>? CapabilityIds { get; set; }
}

/// <summary>
/// Platform modulu guncelleme DTO'su
/// </summary>
public class AdminModuleUpdateDto
{
    public int? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? DisplayNameResourceKey { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public bool IsMenuSection { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsMenuItem { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Modul-Capability mapping goruntuleme DTO'su
/// </summary>
public class ModuleCapabilityMappingDto
{
    public int CapabilityId { get; set; }
    public string CapabilityName { get; set; } = string.Empty;
    public string? CapabilityIcon { get; set; }
    public bool IsMapped { get; set; }
}

/// <summary>
/// Capability icin modul secimi DTO'su (coklu secim)
/// </summary>
public class CapabilityModuleSelectionDto
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string DisplayNameResourceKey { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public bool IsMenuSection { get; set; }
    public bool IsMenuItem { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsSelected { get; set; }
    public List<CapabilityModuleSelectionDto> Children { get; set; } = new();
}

public class AdminRoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameResourceKey { get; set; }
    /// <summary>
    /// Localized name - NameResourceKey varsa çevirisi, yoksa Name
    /// </summary>
    public string LocalizedName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CapabilityId { get; set; }
    public string CapabilityName { get; set; } = string.Empty;
    public string? CapabilityIcon { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
}

public class AdminRoleCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameResourceKey { get; set; }
    public string? Description { get; set; }
    public int CapabilityId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ModulePermissionViewDto
{
    /// <summary>
    /// PlatformModule ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Alias for Id (JavaScript compatibility)
    /// </summary>
    public int ModuleId => Id;

    public string Name { get; set; } = string.Empty;
    public string DisplayNameResourceKey { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int Level { get; set; }
    public bool IsParent { get; set; }
    public bool IsSection { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class PermissionSaveDto
{
    /// <summary>
    /// PlatformModule ID
    /// </summary>
    public int ModuleId { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class AddModuleToCapabilityDto
{
    public int ModuleId { get; set; }
}

public class AddMultipleModulesToCapabilityDto
{
    public List<int> ModuleIds { get; set; } = new();
}

/// <summary>
/// Capability bazli modul grubu - rol yetki atamasinda tum capability modulleri gruplu gosterilir
/// </summary>
public class CapabilityModulesGroupDto
{
    public int CapabilityId { get; set; }
    public string CapabilityName { get; set; } = string.Empty;
    public string? CapabilityIcon { get; set; }
    public List<ModulePermissionViewDto> Modules { get; set; } = new();
}

// ========================================
// USER DTOs
// ========================================

public class AdminUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Initials => $"{(FirstName.Length > 0 ? FirstName[0] : ' ')}{(LastName.Length > 0 ? LastName[0] : ' ')}".ToUpper();
    public int? VendorId { get; set; }
    public string? CompanyName { get; set; }
    public string? VendorRole { get; set; }
    public List<string> SystemRoles { get; set; } = new();
    public bool IsActive { get; set; }
    public bool IsSystemAdmin { get; set; }
    public string? LastLoginAt { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public class AdminUserDetailDto : AdminUserDto
{
    public string? Phone { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneConfirmed { get; set; }
    public int? AccessFailedCount { get; set; }
    public bool LockoutEnabled { get; set; }
    public string? LockoutEnd { get; set; }
}

public class AdminUserCreateDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int? VendorId { get; set; }
    public string? VendorRole { get; set; }  // Owner, Admin, Manager, Employee
    public bool IsSystemAdmin { get; set; }
    public string? SystemRole { get; set; }  // Admin role
}

// ========================================
// VENDOR DTOs
// ========================================

public class AdminVendorDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int VendorStatusId { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    public string? CapabilityName { get; set; }
    public int UserCount { get; set; }
    public bool IsVerified { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public class AdminVendorDetailDto : AdminVendorDto
{
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsProfileComplete { get; set; }
    public List<AdminUserDto> Users { get; set; } = new();
    public List<string> Capabilities { get; set; } = new();
}

// ========================================
// LANGUAGE DTOs
// ========================================

public class AdminLanguageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NativeName { get; set; }
    public string LanguageCulture { get; set; } = string.Empty;
    public string UniqueSeoCode { get; set; } = string.Empty;
    public string? Iso3Code { get; set; }
    public string? FlagEmoji { get; set; }
    public string? FlagImageFileName { get; set; }
    public bool Rtl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsGeoTranslationEnabled { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public int ResourceCount { get; set; }
}

public class AdminLanguageCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? NativeName { get; set; }
    public string LanguageCulture { get; set; } = string.Empty;
    public string UniqueSeoCode { get; set; } = string.Empty;
    public string? Iso3Code { get; set; }
    public string? FlagEmoji { get; set; }
    public string? FlagImageFileName { get; set; }
    public bool Rtl { get; set; }
    public bool IsGeoTranslationEnabled { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
}

// ========================================
// LOCALIZATION RESOURCE DTOs
// ========================================

public class AdminLocalizationResourceDto
{
    public int Id { get; set; }
    public int LanguageId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string ResourceValue { get; set; } = string.Empty;
}

public class AdminLocalizationResourceCreateDto
{
    public int LanguageId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string ResourceValue { get; set; } = string.Empty;
}

public class AdminLocalizationResourceUpdateDto
{
    public string ResourceValue { get; set; } = string.Empty;
}

public class LocalizationResourceListDto
{
    public List<AdminLocalizationResourceDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class LocalizationResourceImportDto
{
    public string ResourceName { get; set; } = string.Empty;
    public string ResourceValue { get; set; } = string.Empty;
}

public class LocalizationResourceExportDto
{
    public string ResourceName { get; set; } = string.Empty;
    public string ResourceValue { get; set; } = string.Empty;
}

// ========================================
// PRODUCT CATEGORY DTOs (Global - Admin yonetimli)
// ========================================

public class AdminCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? NameResourceKey { get; set; }
    public string? Description { get; set; }
    public string? DescriptionResourceKey { get; set; }
    public string? Icon { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int DisplayOrder { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public int ProductCount { get; set; }
    public int ChildCount { get; set; }
    public List<AdminCategoryDto> Children { get; set; } = new();
    public DateTime? DeletedAt { get; set; }  // For trash bin
    public string? DeletedBy { get; set; }  // Who deleted
    public int? MigratedProductCount { get; set; }  // How many products were migrated
    public int? MigratedToCategoryId { get; set; }  // Target category ID
    public string? MigratedToCategoryName { get; set; }  // Target category name

    // Review/Approval fields
    public int? DeletionStatus { get; set; }  // 0=Pending, 1=Approved, 2=Rejected
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }

    /// <summary>
    /// Gosterim icin kullanilacak ad (ResourceKey > DisplayName > Name sirasi)
    /// </summary>
    public string LocalizedName { get; set; } = string.Empty;
}

public class AdminCategoryCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? NameResourceKey { get; set; }
    public string? Description { get; set; }
    public string? DescriptionResourceKey { get; set; }
    public string? Icon { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentId { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}

// === GEOGRAPHY DTOs ===

public class AdminCountryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OfficialName { get; set; }
    public string Iso2Code { get; set; } = string.Empty;
    public string Iso3Code { get; set; } = string.Empty;
    public int? NumericCode { get; set; }
    public string? PhoneCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public string? CurrencySymbol { get; set; }
    public string? FlagEmoji { get; set; }
    public string? Region { get; set; }
    public string? SubRegion { get; set; }
    public string? Capital { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int StateCount { get; set; }
}

public class AdminCountryCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? OfficialName { get; set; }
    public string Iso2Code { get; set; } = string.Empty;
    public string Iso3Code { get; set; } = string.Empty;
    public int? NumericCode { get; set; }
    public string? PhoneCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public string? CurrencySymbol { get; set; }
    public string? FlagEmoji { get; set; }
    public string? Region { get; set; }
    public string? SubRegion { get; set; }
    public string? Capital { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class AdminStateDto
{
    public int Id { get; set; }
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string? CountryFlag { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Type { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class AdminStateCreateUpdateDto
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Type { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

// === COUNTRY TRANSLATION DTOs ===

public class AdminCountryDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OfficialName { get; set; }
    public string Iso2Code { get; set; } = string.Empty;
    public string Iso3Code { get; set; } = string.Empty;
    public string? PhoneCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? FlagEmoji { get; set; }
    public List<AdminCountryTranslationDto> Translations { get; set; } = new();
}

public class AdminCountryTranslationDto
{
    public int Id { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? OfficialName { get; set; }
}

public class AdminCountryTranslationUpdateDto
{
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? OfficialName { get; set; }
}

// === STATE TRANSLATION DTOs ===

public class AdminStateDetailDto
{
    public int Id { get; set; }
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
    public List<AdminStateTranslationDto> Translations { get; set; } = new();
}

public class AdminStateTranslationDto
{
    public int Id { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class AdminStateTranslationUpdateDto
{
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

// ========================================
// PROFILE APPROVAL DTOs
// ========================================

public class AdminProfileApprovalDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public int CapabilityId { get; set; }
    public string CapabilityName { get; set; } = string.Empty;
    public string? CapabilityIcon { get; set; }
    public string? DisplayName { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public DateTime? PublicationRequestedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminProfileDetailDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public int CapabilityId { get; set; }
    public string CapabilityName { get; set; } = string.Empty;
    public string? CapabilityIcon { get; set; }
    public string Slug { get; set; } = string.Empty;

    // Genel bilgiler
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? Tagline { get; set; }

    // Hizmetler
    public string? Services { get; set; }
    public string? Certifications { get; set; }
    public string? ServiceRegions { get; set; }

    // Iletisim
    public string? PublicEmail { get; set; }
    public string? PublicPhone { get; set; }
    public string? PublicWebsite { get; set; }

    // Lokasyon
    public string? CountryName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }

    // Gorseller
    public string? CoverImageUrl { get; set; }
    public string? GalleryImages { get; set; }

    // Durum
    public bool IsPubliclyVisible { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? PublicationRequestedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Istatistikler
    public int ViewCount { get; set; }
    public int ContactRequestCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion
