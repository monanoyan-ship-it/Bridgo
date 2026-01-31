using System.ComponentModel.DataAnnotations;
using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

/// <summary>
/// Depo - Stok tutulacak fiziksel veya sanal lokasyon
/// Her Vendor'in bir veya daha fazla deposu olabilir
/// </summary>
public class Warehouse : BaseEntity
{
    /// <summary>
    /// Depo kodu (benzersiz - vendor icinde)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Depo adi
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Depo tipi (WarehouseTypes static class'tan ID)
    /// </summary>
    public int WarehouseTypeId { get; set; } = 1; // Default: Main

    /// <summary>
    /// Aciklama
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    // === LOKASYON ===

    /// <summary>
    /// Fiziksel adres (Address tablosundan)
    /// </summary>
    public int? AddressId { get; set; }

    // === KAPASITE ===

    /// <summary>
    /// Toplam kapasite (m3 veya palet sayisi)
    /// </summary>
    public decimal? TotalCapacity { get; set; }

    /// <summary>
    /// Kapasite birimi (m3, pallet, unit)
    /// </summary>
    [MaxLength(20)]
    public string? CapacityUnit { get; set; }

    // === ILETISIM ===

    /// <summary>
    /// Depo sorumlusu
    /// </summary>
    public int? ManagerUserId { get; set; }

    /// <summary>
    /// Iletisim telefonu
    /// </summary>
    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Iletisim e-postasi
    /// </summary>
    [MaxLength(100)]
    public string? ContactEmail { get; set; }

    // === CALISMA SAATLERI ===

    /// <summary>
    /// Calisma saatleri (JSON format)
    /// Ornek: {"mon":"09:00-18:00","tue":"09:00-18:00",...}
    /// </summary>
    [MaxLength(500)]
    public string? OperatingHours { get; set; }

    // === ONCELIK VE DURUM ===

    /// <summary>
    /// Oncelik sirasi (stok seciminde kullanilir)
    /// Dusuk deger = yuksek oncelik
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Varsayilan depo mu?
    /// Her vendor'in bir varsayilan deposu olmali
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;

    // === VENDOR ILISKI ===

    /// <summary>
    /// Deponun ait oldugu vendor
    /// </summary>
    public int VendorId { get; set; }

    // === NAVIGATION PROPERTIES ===

    public Vendor? Vendor { get; set; }
    public Address? Address { get; set; }
    public ApplicationUser? Manager { get; set; }
    public ICollection<ProductWarehouseStock> ProductStocks { get; set; } = new List<ProductWarehouseStock>();
}
