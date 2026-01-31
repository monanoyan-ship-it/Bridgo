using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Urun gorselleri
/// Bir urunun birden fazla gorseli olabilir
/// </summary>
public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    //TODO: Url degilde S3 veya Azure Blob Storage gibi bir servis kullanabiliriz burada hazirligi olmasi lazim. Url degil de yani dosyanin temsili ismi daha anlamli.
    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;  // Gorsel URL veya path
    //TODO: Alt text i kullaniciya birakmayalim. Seo adina burayi da bizim olusturmamiz lazim
    [MaxLength(200)]
    public string? AltText { get; set; }  // SEO icin alt text
    //TODO: Title i kullaniciya birakmayalim. Seo adina burayi da bizim olusturmamiz lazim
    [MaxLength(150)]
    public string? Title { get; set; }  // Gorsel basligi

    public int DisplayOrder { get; set; } = 0;

    public bool IsMain { get; set; } = false;  // Ana gorsel mi

    // === GORSEL BOYUTLARI === 
    //TODO: Burasinin bizim tarafimizdan hesaplanmasi gerekiyor. Kullaniciya birakilmamali
    public int? Width { get; set; }

    public int? Height { get; set; }

    public long? FileSize { get; set; }  // Byte cinsinden

    [MaxLength(50)]
    public string? MimeType { get; set; }  // image/jpeg, image/png, vb.

    // === NAVIGATION PROPERTIES ===
    public Product Product { get; set; } = null!;
}
