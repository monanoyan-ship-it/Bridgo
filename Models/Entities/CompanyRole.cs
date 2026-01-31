namespace Bridgo.Models.Entities;

/// <summary>
/// Company ici roller - Capability bazli
/// Admin Panel'den tanimlanir ve yonetilir
/// </summary>
public class CompanyRole : BaseEntity
{
    /// <summary>
    /// Bu rol hangi capability altinda?
    /// Ornek: "Satis Temsilcisi" -> "Satici" capability'si
    /// Capabilities.Ids'den bir deger (Seller=2, Buyer=3, vb.)
    /// </summary>
    public int CapabilityId { get; set; }

    /// <summary>
    /// Fallback isim (ResourceKey yoksa kullanilir)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Cok dilli isim icin resource key (orn: "Role.SalesRepresentative")
    /// Listelerde T(NameResourceKey) ile gosterilir
    /// </summary>
    public string? NameResourceKey { get; set; }

    /// <summary>
    /// Firma olusturulurken Owner'a otomatik atanir mi?
    /// </summary>
    public bool IsDefault { get; set; } = false;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<CompanyRoleUserMapping> UserMappings { get; set; } = new List<CompanyRoleUserMapping>();
    public ICollection<CompanyRoleModulePermission> ModulePermissions { get; set; } = new List<CompanyRoleModulePermission>();
}
