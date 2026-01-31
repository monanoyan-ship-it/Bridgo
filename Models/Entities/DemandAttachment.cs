using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Talep eki (teknik cizim, gorsel, PDF, vb.)
/// </summary>
public class DemandAttachment : BaseEntity
{
    public int DemandId { get; set; }

    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MimeType { get; set; }

    public long? FileSize { get; set; }  // Byte

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; } = 0;

    // === NAVIGATION ===
    public PublicDemand Demand { get; set; } = null!;
}
