namespace Bridgo.DTOs.Product;

/// <summary>
/// Kategori listeleme DTO
/// </summary>
public class ProductCategoryListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int DisplayOrder { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public List<ProductCategoryListDto> Children { get; set; } = new();
}

/// <summary>
/// Kategori dropdown DTO (select icin)
/// </summary>
public class ProductCategorySelectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int Level { get; set; }
    public string DisplayName => Level > 0 ? new string('-', Level * 2) + " " + Name : Name;
}

/// <summary>
/// Kategori olusturma/guncelleme DTO
/// </summary>
public class ProductCategoryCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}
