using System.ComponentModel.DataAnnotations;
using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

public class SocialPostReport : BaseEntity
{
    public int SocialPostId { get; set; }
    public int ReporterUserId { get; set; }
    public int ReporterVendorId { get; set; }

    /// <summary>
    /// SocialPostReportReasons static class (TypeDefinitions.cs)
    /// </summary>
    public int ReasonId { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// SocialPostReportStatuses static class (TypeDefinitions.cs)
    /// </summary>
    public int StatusId { get; set; }

    [MaxLength(2000)]
    public string? AdminNote { get; set; }

    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Navigation properties
    public virtual SocialPost SocialPost { get; set; } = null!;
    public virtual ApplicationUser ReporterUser { get; set; } = null!;
    public virtual Vendor ReporterVendor { get; set; } = null!;
    public virtual ApplicationUser? ReviewedByUser { get; set; }
}
