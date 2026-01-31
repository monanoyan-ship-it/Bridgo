using Bridgo.Models.Entities;

namespace Bridgo.Services.Interfaces;

public interface ICompanyService
{
    /// <summary>
    /// Vendor'in sahip oldugu aktif capability'leri getirir
    /// </summary>
    Task<List<VendorCapabilityDto>> GetVendorCapabilitiesAsync(int vendorId);

    /// <summary>
    /// Belirli bir capability'nin modullerini getirir (hiyerarsik)
    /// </summary>
    Task<List<CapabilityModuleDto>> GetCapabilityModulesAsync(int capabilityId);

    /// <summary>
    /// User'in belirli bir capability altindaki rollerini getirir
    /// </summary>
    Task<List<UserCompanyRoleDto>> GetUserRolesAsync(int userId, int vendorId, int? capabilityId = null);

    /// <summary>
    /// User'in belirli bir moduldeki izinlerini kontrol eder
    /// </summary>
    Task<ModulePermissionDto?> GetUserModulePermissionAsync(int userId, int vendorId, int moduleId);

    /// <summary>
    /// User'in erisebildigi modulleri getirir (izin kontrolu ile)
    /// </summary>
    Task<List<CapabilityModuleDto>> GetUserAccessibleModulesAsync(int userId, int vendorId, int capabilityId);

    /// <summary>
    /// Platform (ortak) modullerini getirir - tum vendor'lar icin
    /// </summary>
    Task<List<CapabilityModuleDto>> GetPlatformModulesAsync();

    /// <summary>
    /// Platform capability'sini getirir
    /// </summary>
    Task<VendorCapabilityDto?> GetPlatformCapabilityAsync();

    /// <summary>
    /// Vendor'a capability ekler
    /// </summary>
    Task<bool> AddVendorCapabilityAsync(int vendorId, int capabilityId);

    /// <summary>
    /// Vendor'a birden fazla capability ekler (ID listesi ile)
    /// </summary>
    Task<int> AddVendorCapabilitiesAsync(int vendorId, IEnumerable<int> capabilityIds);

    /// <summary>
    /// Vendor'dan capability kaldirir
    /// </summary>
    Task<bool> RemoveVendorCapabilityAsync(int vendorId, int capabilityId);

    /// <summary>
    /// User'a rol atar
    /// </summary>
    Task<bool> AssignUserRoleAsync(int userId, int vendorId, int companyRoleId, int assignedByUserId);

    /// <summary>
    /// User'dan rol kaldirir
    /// </summary>
    Task<bool> RemoveUserRoleAsync(int userId, int vendorId, int companyRoleId);

    /// <summary>
    /// Vendor'a capability ekler ve owner'a default role atar (yoksa)
    /// Odeme sonrasi Buyer capability otomatik eklemek icin kullanilir
    /// </summary>
    Task<bool> EnsureVendorCapabilityWithDefaultRoleAsync(int vendorId, int capabilityId);
}

// DTOs
public class VendorCapabilityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // Lowercase slug (seller, buyer, carrier, etc.)
    public string? NameResourceKey { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? CssClass { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Platform modulu DTO'su - artik moduller global (capability'ye gore degil)
/// </summary>
public class CapabilityModuleDto
{
    /// <summary>
    /// PlatformModule ID
    /// </summary>
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string DisplayNameResourceKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsMenuItem { get; set; }
    public bool IsMenuSection { get; set; }
    public List<CapabilityModuleDto> Children { get; set; } = new();
}

public class UserCompanyRoleDto
{
    public int Id { get; set; }
    public int CompanyRoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? RoleNameResourceKey { get; set; }
    public int CapabilityId { get; set; }
    public string CapabilityName { get; set; } = string.Empty;
    public string? CapabilityNameResourceKey { get; set; }
}

public class ModulePermissionDto
{
    public int ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
