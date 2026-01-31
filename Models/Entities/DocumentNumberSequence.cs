namespace Bridgo.Models.Entities;

/// <summary>
/// Evrak numaralandirma sira tablosu
/// Her evrak tipi ve yil icin ayri sira numarasi
/// </summary>
public class DocumentNumberSequence : BaseEntity
{
    /// <summary>
    /// Evrak tipi (DocumentTypes'dan)
    /// </summary>
    public int DocumentTypeId { get; set; }

    /// <summary>
    /// Yil
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Son kullanilan numara
    /// </summary>
    public int LastNumber { get; set; }

    /// <summary>
    /// Evrak prefix'i (SZL, FTR, KNS vb.)
    /// </summary>
    public string Prefix { get; set; } = string.Empty;
}
