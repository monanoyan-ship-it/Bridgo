namespace Bridgo.Models.Entities;

/// <summary>
/// Evrak sablonlari - PDF form field mapping ile
/// </summary>
public class DocumentTemplate : BaseEntity
{
    /// <summary>
    /// Evrak tipi (DocumentTypes'dan)
    /// </summary>
    public int DocumentTypeId { get; set; }

    /// <summary>
    /// Sablon adi
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Aciklama
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// PDF sablon dosya yolu
    /// </summary>
    public string? TemplatePdfPath { get; set; }

    /// <summary>
    /// Form field mapping JSON
    /// {"seller_company": "Text1", "buyer_company": "Text2", ...}
    /// </summary>
    public string? FieldMappingJson { get; set; }

    /// <summary>
    /// Imza gerekli mi?
    /// </summary>
    public bool RequiresSignature { get; set; }

    /// <summary>
    /// Kac imza gerekli? (1=tek taraf, 2=cift taraf)
    /// </summary>
    public int SignatureCount { get; set; } = 1;

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Siralama
    /// </summary>
    public int DisplayOrder { get; set; }
}
