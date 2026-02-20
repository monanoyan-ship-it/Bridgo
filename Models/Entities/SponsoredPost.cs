using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

public class SponsoredPost : BaseEntity
{
    public int SocialPostId { get; set; }
    public int VendorId { get; set; }

    /// <summary>
    /// SponsoredPostStatuses static class (TypeDefinitions.cs)
    /// </summary>
    public int StatusId { get; set; }

    public decimal BudgetAmount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public decimal SpentAmount { get; set; }
    public int TargetImpressions { get; set; }
    public int CurrentImpressions { get; set; }
    public int CurrentClicks { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Navigation properties
    public virtual SocialPost SocialPost { get; set; } = null!;
    public virtual Vendor Vendor { get; set; } = null!;
}
