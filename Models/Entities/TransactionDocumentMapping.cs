namespace Bridgo.Models.Entities;

/// <summary>
/// Islem-Evrak eslestirmesi
/// Hangi islem tipinde hangi evraklar uretilmeli
/// </summary>
public class TransactionDocumentMapping : BaseEntity
{
    /// <summary>
    /// Islem tipi (TransactionTypes'dan)
    /// </summary>
    public int TransactionTypeId { get; set; }

    /// <summary>
    /// Evrak tipi (DocumentTypes'dan)
    /// </summary>
    public int DocumentTypeId { get; set; }

    /// <summary>
    /// Capability ID (null = tum capability'ler icin gecerli)
    /// </summary>
    public int? CapabilityId { get; set; }

    /// <summary>
    /// Evrak zorunlu mu?
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Olusturma sirasi
    /// </summary>
    public int GenerationOrder { get; set; }

    /// <summary>
    /// Kosullar JSON (orn: {"minAmount": 10000, "requiresCustoms": true})
    /// </summary>
    public string? ConditionsJson { get; set; }

    /// <summary>
    /// Aciklama
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
}
