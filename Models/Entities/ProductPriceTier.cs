using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Urun fiyat esikleri - miktar bazli fiyatlandirma
/// Ornek: 1-99 adet = 25$, 100-999 = 24$, 1000+ = 23$
/// </summary>
public class ProductPriceTier : BaseEntity
{
    public int ProductId { get; set; }

    /// <summary>
    /// Minimum miktar (dahil)
    /// </summary>
    public int MinQuantity { get; set; }

    /// <summary>
    /// Maximum miktar (dahil). Null = sinirsiz (ve uzeri)
    /// </summary>
    public int? MaxQuantity { get; set; }

    /// <summary>
    /// Bu esikteki birim fiyat
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Aciklama (opsiyonel) - ornek: "Toptan fiyat"
    /// </summary>
    [MaxLength(100)]
    public string? Description { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}
