using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Urun kategorileri - hiyerarsik yapi
/// Platform genelinde Admin tarafindan yonetilir (global kategoriler)
/// Seller'lar urun eklerken bu kategorilerden secer
/// </summary>
public class ProductCategory : BaseEntity
{
    /// <summary>
    /// Teknik isim (tanimlama amacli, benzersiz olmali)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gosterim adi (fallback - ResourceKey yoksa veya ceviri bulunamazsa kullanilir)
    /// </summary>
    [MaxLength(200)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gosterim adi icin localization key (orn: "Category.Electronics")
    /// </summary>
    [MaxLength(200)]
    public string? NameResourceKey { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Aciklama icin localization key (orn: "Category.Electronics.Description")
    /// </summary>
    [MaxLength(200)]
    public string? DescriptionResourceKey { get; set; }

    [MaxLength(100)]
    public string? Icon { get; set; }  // Bootstrap icon class (bi-box, vb.)

    [MaxLength(200)]
    public string? ImageUrl { get; set; }  // Kategori gorseli

    // === HIYERARSI ===
    public int? ParentId { get; set; }  // Ust kategori (null = root)

    public int DisplayOrder { get; set; } = 0;

    public int Level { get; set; } = 0;  // Derinlik seviyesi (0 = root)

    // === DURUM ===
    public bool IsActive { get; set; } = true;

    // === SEO ===
    [MaxLength(200)]
    public string? Slug { get; set; }

    [MaxLength(200)]
    public string? MetaTitle { get; set; }

    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    // === SILME DETAYLARI ===
    /// <summary>
    /// Silinirken kac urun tasindi
    /// </summary>
    public int? MigratedProductCount { get; set; }

    /// <summary>
    /// Urunler hangi kategoriye tasindi
    /// </summary>
    public int? MigratedToCategoryId { get; set; }

    /// <summary>
    /// Tasinan urun ID'leri (JSON array) - geri alma icin
    /// </summary>
    public string? MigratedProductIds { get; set; }

    // === SILME ONAY SURECI ===
    /// <summary>
    /// Silme durumu: 0=Onay Bekliyor, 1=Onaylandi, 2=Reddedildi
    /// </summary>
    public int? DeletionStatus { get; set; }

    /// <summary>
    /// Silmeyi inceleyen/onaylayan kullanici
    /// </summary>
    [MaxLength(200)]
    public string? ReviewedBy { get; set; }

    /// <summary>
    /// Inceleme/onay tarihi
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Inceleme notu
    /// </summary>
    [MaxLength(500)]
    public string? ReviewNote { get; set; }

    // === NAVIGATION PROPERTIES ===
    public ProductCategory? Parent { get; set; }
    public ICollection<ProductCategory> Children { get; set; } = new List<ProductCategory>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
