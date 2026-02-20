using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

public class BackInStockSubscription : BaseEntity
{
    public int ProductId { get; set; }

    public int VendorId { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    public bool IsNotified { get; set; }

    public DateTime? NotifiedAt { get; set; }

    // Navigation
    public virtual Product Product { get; set; } = null!;
    public virtual Vendor Vendor { get; set; } = null!;
}
