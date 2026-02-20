using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

public class RewardPointHistory : BaseEntity
{
    public int VendorId { get; set; }

    public int? UserId { get; set; }

    public int Points { get; set; }

    /// <summary>
    /// RewardPointActionTypes (TypeDefinitions.cs)
    /// </summary>
    public int ActionTypeId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? OrderId { get; set; }

    public int BalanceAfter { get; set; }

    // Navigation
    public virtual Vendor Vendor { get; set; } = null!;
}
