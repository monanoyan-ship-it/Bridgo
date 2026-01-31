using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Urun ozellik tanimlari (Renk, Boyut, Malzeme, Marka vb.)
/// Kategori bazli veya global olabilir
/// </summary>
public class ProductAttribute : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Cevirilmis ad icin resource key (ProductAttribute.Color, vb.)
    /// </summary>
    [MaxLength(200)]
    public string? NameResourceKey { get; set; }

    /// <summary>
    /// Ozellik tipi: Text, Number, Select, MultiSelect, Boolean
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string AttributeType { get; set; } = "Text";

    /// <summary>
    /// Bu ozellik hangi kategorilere ait? (null = global, tum kategoriler)
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Siralama
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Filtrede gosterilsin mi?
    /// </summary>
    public bool IsFilterable { get; set; } = true;

    /// <summary>
    /// Urun detayinda gosterilsin mi?
    /// </summary>
    public bool IsVisibleOnProductPage { get; set; } = true;

    /// <summary>
    /// Zorunlu mu?
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Birim (opsiyonel - cm, kg, ml vb.)
    /// </summary>
    [MaxLength(20)]
    public string? Unit { get; set; }

    /// <summary>
    /// Ozellik ikonu (bi bi-palette vb.)
    /// </summary>
    [MaxLength(50)]
    public string? Icon { get; set; }

    // Navigation
    public ProductCategory? Category { get; set; }
    public ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();
    public ICollection<ProductAttributeMapping> ProductMappings { get; set; } = new List<ProductAttributeMapping>();
}

/// <summary>
/// Ozellik degerleri (Select/MultiSelect icin predefined degerler)
/// Ornek: Renk -> Kirmizi, Mavi, Yesil
/// </summary>
public class ProductAttributeValue : BaseEntity
{
    public int AttributeId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Cevirilmis deger icin resource key
    /// </summary>
    [MaxLength(200)]
    public string? ValueResourceKey { get; set; }

    /// <summary>
    /// Ek veri (renk kodu, resim URL vb.)
    /// </summary>
    [MaxLength(500)]
    public string? AdditionalData { get; set; }

    /// <summary>
    /// Siralama
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation
    public ProductAttribute Attribute { get; set; } = null!;
}

/// <summary>
/// Urun-Ozellik eslesmesi
/// Her urunun hangi ozelliklere hangi degerleri verdigini tutar
/// </summary>
public class ProductAttributeMapping : BaseEntity
{
    public int ProductId { get; set; }
    public int AttributeId { get; set; }

    /// <summary>
    /// Select/MultiSelect icin predefined value ID
    /// </summary>
    public int? AttributeValueId { get; set; }

    /// <summary>
    /// Text/Number icin serbest deger
    /// </summary>
    [MaxLength(500)]
    public string? CustomValue { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public ProductAttribute Attribute { get; set; } = null!;
    public ProductAttributeValue? AttributeValue { get; set; }
}
