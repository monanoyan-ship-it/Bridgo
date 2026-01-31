namespace Bridgo.Models.Entities;

/// <summary>
/// Platform genelindeki moduller (Master Liste)
/// Tum capability'ler bu tablodan modul kullanir
/// ID tek ve benzersiz - kopyalama yok
/// </summary>
public class PlatformModule : BaseEntity
{
    public int? ParentId { get; set; } // Hiyerarsi icin (section altindaki moduller)

    /// <summary>
    /// Teknik isim (tanimlama amacli)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Menude gorunecek ad (fallback)
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Menude gorunecek ad icin resource key (orn: "Module.Orders")
    /// Dashboard menusunde T(DisplayNameResourceKey) ile gosterilir
    /// BENZERSIZ - modul tanımlama icin kullanilir
    /// </summary>
    public string DisplayNameResourceKey { get; set; } = string.Empty;

    public string? Icon { get; set; } // Bootstrap icon (bi-box, bi-cart, vb.)
    public string? Route { get; set; } // /Dashboard/Orders, /Products, vb.

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsMenuItem { get; set; } = true; // Menude gorunecek mi?

    /// <summary>
    /// Bu modul bir menu bolumu mu? (Section header)
    /// true ise: Alt moduller bu modulun altinda gruplanir
    /// false ise: Normal tiklanabilir menu ogesi
    /// </summary>
    public bool IsMenuSection { get; set; } = false;

    // Navigation
    public PlatformModule? Parent { get; set; }
    public ICollection<PlatformModule> Children { get; set; } = new List<PlatformModule>();
    public ICollection<CapabilityModuleMapping> CapabilityMappings { get; set; } = new List<CapabilityModuleMapping>();
    public ICollection<CompanyRoleModulePermission> RolePermissions { get; set; } = new List<CompanyRoleModulePermission>();
}
