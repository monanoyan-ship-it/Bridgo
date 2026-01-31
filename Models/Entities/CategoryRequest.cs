using System.ComponentModel.DataAnnotations;
using Bridgo.Models.Identity;
using Bridgo.Models.Enums;

namespace Bridgo.Models.Entities;

/// <summary>
/// Kullanicilarin yeni kategori talebi
/// Admin onayladiginda ProductCategory'ye eklenir
/// </summary>
public class CategoryRequest : BaseEntity
{
    /// <summary>
    /// Talep edilen kategori adi
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string RequestedName { get; set; } = string.Empty;

    /// <summary>
    /// Kategori aciklamasi / neden gerekli
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Onerilen ust kategori (opsiyonel)
    /// </summary>
    public int? SuggestedParentCategoryId { get; set; }

    /// <summary>
    /// Talep durumu (CategoryRequestStatuses'dan ID)
    /// </summary>
    public int StatusId { get; set; } = CategoryRequestStatuses.Pending.Id;

    /// <summary>
    /// Talebi yapan vendor
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Talebi yapan kullanici
    /// </summary>
    public int RequestedByUserId { get; set; }

    /// <summary>
    /// Inceleyen admin (onay/red sonrasi)
    /// </summary>
    public int? ReviewedByUserId { get; set; }

    /// <summary>
    /// Admin notu (ozellikle red durumunda aciklama)
    /// </summary>
    [MaxLength(500)]
    public string? ReviewNote { get; set; }

    /// <summary>
    /// Inceleme tarihi
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Onaylaninca olusturulan kategori ID'si
    /// </summary>
    public int? CreatedCategoryId { get; set; }

    // Navigation properties
    public ProductCategory? SuggestedParentCategory { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public ApplicationUser RequestedByUser { get; set; } = null!;
    public ApplicationUser? ReviewedByUser { get; set; }
    public ProductCategory? CreatedCategory { get; set; }
}
