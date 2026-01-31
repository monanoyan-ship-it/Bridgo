using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Urun-Depo Stok - Her urunun her depodaki stok bilgisi
/// </summary>
public class ProductWarehouseStock : BaseEntity
{
    /// <summary>
    /// Urun ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Depo ID
    /// </summary>
    public int WarehouseId { get; set; }

    // === STOK MIKTARLARI ===

    /// <summary>
    /// Fiziksel stok miktari
    /// </summary>
    public decimal Quantity { get; set; } = 0;

    /// <summary>
    /// Rezerve edilen miktar (siparisler icin ayrilan)
    /// </summary>
    public decimal ReservedQuantity { get; set; } = 0;

    /// <summary>
    /// Satin alinabilir miktar (Quantity - ReservedQuantity)
    /// </summary>
    public decimal AvailableQuantity => Quantity - ReservedQuantity;

    /// <summary>
    /// Minimum stok seviyesi (bu depo icin)
    /// </summary>
    public decimal? MinStockLevel { get; set; }

    /// <summary>
    /// Maksimum stok seviyesi (bu depo icin)
    /// </summary>
    public decimal? MaxStockLevel { get; set; }

    /// <summary>
    /// Yeniden siparis noktasi
    /// </summary>
    public decimal? ReorderPoint { get; set; }

    /// <summary>
    /// Yeniden siparis miktari
    /// </summary>
    public decimal? ReorderQuantity { get; set; }

    // === LOKASYON DETAY ===

    /// <summary>
    /// Raf/Bin lokasyonu (ornek: A-01-02)
    /// </summary>
    [MaxLength(50)]
    public string? BinLocation { get; set; }

    /// <summary>
    /// Zone/Bolge (ornek: FROZEN, HAZMAT, GENERAL)
    /// </summary>
    [MaxLength(50)]
    public string? Zone { get; set; }

    // === SAYIM VE TAKIP ===

    /// <summary>
    /// Son stok sayim tarihi
    /// </summary>
    public DateTime? LastStockCheckAt { get; set; }

    /// <summary>
    /// Son stok hareketi tarihi
    /// </summary>
    public DateTime? LastMovementAt { get; set; }

    /// <summary>
    /// Notlar
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    // === NAVIGATION PROPERTIES ===

    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
}
