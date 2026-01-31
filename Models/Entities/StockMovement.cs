using System.ComponentModel.DataAnnotations;
using Bridgo.Models.Enums;

namespace Bridgo.Models.Entities;

/// <summary>
/// Stok hareketi - Her stok degisikliginin kaydi
/// </summary>
public class StockMovement : BaseEntity
{
    /// <summary>
    /// Urun ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Depo ID
    /// </summary>
    public int WarehouseId { get; set; }

    /// <summary>
    /// Hareket tipi (StockMovementTypes)
    /// 1: In (Giris), 2: Out (Cikis), 3: Transfer, 4: Adjustment, 5: Reserve, 6: Unreserve
    /// </summary>
    public int MovementType { get; set; }

    /// <summary>
    /// Miktar (pozitif veya negatif)
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Onceki stok miktari
    /// </summary>
    public decimal PreviousQuantity { get; set; }

    /// <summary>
    /// Yeni stok miktari
    /// </summary>
    public decimal NewQuantity { get; set; }

    /// <summary>
    /// Referans tipi (ReferenceTypes)
    /// 1: Order, 2: Return, 3: Manual, 4: Transfer, 5: Adjustment, 6: PurchaseOrder
    /// </summary>
    public int? ReferenceType { get; set; }

    /// <summary>
    /// Referans ID (OrderId, ReturnId, TransferId vb.)
    /// </summary>
    public int? ReferenceId { get; set; }

    /// <summary>
    /// Referans numarasi (siparis no, iade no vb.)
    /// </summary>
    [MaxLength(50)]
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Transfer hedef depo (Transfer islemi icin)
    /// </summary>
    public int? TargetWarehouseId { get; set; }

    /// <summary>
    /// Aciklama/Not
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Islemi yapan kullanici
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Islem tarihi
    /// </summary>
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;

    // === MULTI-TENANT ===
    public int VendorId { get; set; }

    // === NAVIGATION PROPERTIES ===
    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
    public Warehouse? TargetWarehouse { get; set; }
    public Vendor? Vendor { get; set; }
}

/// <summary>
/// Stok hareket tipleri
/// </summary>
public static class StockMovementTypes
{
    public static readonly TypeItem In = new(1, "In", "StockMovement.Type.In");
    public static readonly TypeItem Out = new(2, "Out", "StockMovement.Type.Out");
    public static readonly TypeItem Transfer = new(3, "Transfer", "StockMovement.Type.Transfer");
    public static readonly TypeItem Adjustment = new(4, "Adjustment", "StockMovement.Type.Adjustment");
    public static readonly TypeItem Reserve = new(5, "Reserve", "StockMovement.Type.Reserve");
    public static readonly TypeItem Unreserve = new(6, "Unreserve", "StockMovement.Type.Unreserve");

    public static TypeItem[] All => new[] { In, Out, Transfer, Adjustment, Reserve, Unreserve };

    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
}

/// <summary>
/// Stok hareketi referans tipleri
/// </summary>
public static class StockReferenceTypes
{
    public static readonly TypeItem Order = new(1, "Order", "StockMovement.Reference.Order");
    public static readonly TypeItem Return = new(2, "Return", "StockMovement.Reference.Return");
    public static readonly TypeItem Manual = new(3, "Manual", "StockMovement.Reference.Manual");
    public static readonly TypeItem Transfer = new(4, "Transfer", "StockMovement.Reference.Transfer");
    public static readonly TypeItem Adjustment = new(5, "Adjustment", "StockMovement.Reference.Adjustment");
    public static readonly TypeItem PurchaseOrder = new(6, "PurchaseOrder", "StockMovement.Reference.PurchaseOrder");

    public static TypeItem[] All => new[] { Order, Return, Manual, Transfer, Adjustment, PurchaseOrder };

    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
}
