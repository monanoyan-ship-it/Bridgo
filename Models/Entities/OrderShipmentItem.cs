namespace Bridgo.Models.Entities;

/// <summary>
/// Kargo kalemi - Hangi urun hangi kargoda
/// </summary>
public class OrderShipmentItem : BaseEntity
{
    /// <summary>
    /// Kargo ID
    /// </summary>
    public int ShipmentId { get; set; }

    /// <summary>
    /// Siparis kalemi ID
    /// </summary>
    public int OrderItemId { get; set; }

    /// <summary>
    /// Bu kargodaki miktar
    /// </summary>
    public int Quantity { get; set; }

    // Navigation properties
    public virtual OrderShipment? Shipment { get; set; }
    public virtual OrderItem? OrderItem { get; set; }
}
