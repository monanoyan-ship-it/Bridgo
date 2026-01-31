namespace Bridgo.DTOs.Category;

/// <summary>
/// Kategori talebi listesi icin DTO
/// </summary>
public class CategoryRequestDto
{
    public int Id { get; set; }
    public string RequestedName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? SuggestedParentCategoryId { get; set; }
    public string? SuggestedParentCategoryName { get; set; }
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCssClass { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public int? ReviewedByUserId { get; set; }
    public string? ReviewedByUserName { get; set; }
    public string? ReviewNote { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? CreatedCategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Yeni kategori talebi olusturma DTO
/// </summary>
public class CategoryRequestCreateDto
{
    public string RequestedName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? SuggestedParentCategoryId { get; set; }
}

/// <summary>
/// Admin tarafindan talep inceleme DTO
/// </summary>
public class CategoryRequestReviewDto
{
    public int Id { get; set; }
    public int StatusId { get; set; }
    public string? ReviewNote { get; set; }

    /// <summary>
    /// Onay durumunda ust kategori ID'si (degistirilmis olabilir)
    /// </summary>
    public int? ParentCategoryId { get; set; }

    /// <summary>
    /// Onay durumunda kategori adi (degistirilmis olabilir)
    /// </summary>
    public string? ApprovedName { get; set; }
}

/// <summary>
/// Kategori arama sonucu DTO
/// </summary>
public class CategorySearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty; // "Elektronik > Telefonlar > Akilli Telefonlar"
    public int Level { get; set; }
    public bool HasChildren { get; set; }
}
