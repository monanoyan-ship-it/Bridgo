using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Referans urun uzerinde istenen modifikasyonlar
/// Ornek: "Boyut: 50cm yerine 75cm", "Malzeme: Plastik yerine Paslanmaz Celik"
/// </summary>
public class DemandModification : BaseEntity
{
    public int DemandId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PropertyName { get; set; } = string.Empty;  // Boyut, Malzeme, Renk, vb.

    [MaxLength(200)]
    public string? OriginalValue { get; set; }  // Referans urundeki deger

    [Required]
    [MaxLength(200)]
    public string DesiredValue { get; set; } = string.Empty;  // Istenen deger

    [MaxLength(500)]
    public string? Notes { get; set; }

    public int DisplayOrder { get; set; } = 0;

    // === NAVIGATION ===
    public PublicDemand Demand { get; set; } = null!;
}
