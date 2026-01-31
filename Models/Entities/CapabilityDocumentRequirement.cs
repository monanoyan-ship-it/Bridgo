namespace Bridgo.Models.Entities;

/// <summary>
/// Capability bazli evrak gereksinimleri
/// Her capability icin gerekli evraklarin tanimlanmasi
/// </summary>
public class CapabilityDocumentRequirement : BaseEntity
{
    /// <summary>
    /// Capability ID (VendorCapabilities FK)
    /// </summary>
    public int CapabilityId { get; set; }

    /// <summary>
    /// Evrak tipi (DocumentTypes'dan)
    /// </summary>
    public int DocumentTypeId { get; set; }

    /// <summary>
    /// Zorunlu mu?
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Aciklama
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gosterim sirasi
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
}
