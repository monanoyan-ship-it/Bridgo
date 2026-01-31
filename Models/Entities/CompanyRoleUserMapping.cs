using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

/// <summary>
/// User'in firma icindeki rolleri
/// Bir user ayni firmada birden fazla role sahip olabilir
/// Her rol farkli bir capability altinda olabilir
/// </summary>
public class CompanyRoleUserMapping : BaseEntity
{
    public int UserId { get; set; }
    public int VendorId { get; set; }
    public int CompanyRoleId { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Bu rolu kim atadi?
    /// </summary>
    public int? AssignedByUserId { get; set; }
    public DateTime? AssignedAt { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public Vendor Vendor { get; set; } = null!;
    public CompanyRole CompanyRole { get; set; } = null!;
    public ApplicationUser? AssignedByUser { get; set; }
}
