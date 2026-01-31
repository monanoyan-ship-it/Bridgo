using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Teklif yaniti eki (fiyat teklifi PDF, urun ornegi gorseli, vb.)
/// </summary>
public class DemandResponseAttachment : BaseEntity
{
    public int ResponseId { get; set; }

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

    public int DisplayOrder { get; set; } = 0;

    // === NAVIGATION ===
    public DemandResponse Response { get; set; } = null!;
}
