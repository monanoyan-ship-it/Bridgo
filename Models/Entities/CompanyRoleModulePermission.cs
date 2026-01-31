namespace Bridgo.Models.Entities;

/// <summary>
/// Rol-Modul izinleri
/// Hangi rol, hangi modülde ne yapabilir?
/// Admin Panel'den yönetilir
/// </summary>
public class CompanyRoleModulePermission : BaseEntity
{
    public int CompanyRoleId { get; set; }
    public int PlatformModuleId { get; set; }

    /// <summary>
    /// Bu modülü görebilir mi?
    /// </summary>
    public bool CanView { get; set; }

    /// <summary>
    /// Bu modülde yeni kayıt oluşturabilir mi?
    /// </summary>
    public bool CanCreate { get; set; }

    /// <summary>
    /// Bu modüldeki kayıtları düzenleyebilir mi?
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Bu modüldeki kayıtları silebilir mi?
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// Özel izinler (JSON formatında)
    /// Örnek: {"canApprove": true, "canExport": true}
    /// </summary>
    public string? CustomPermissions { get; set; }

    // Navigation
    public CompanyRole CompanyRole { get; set; } = null!;
    public PlatformModule PlatformModule { get; set; } = null!;
}
