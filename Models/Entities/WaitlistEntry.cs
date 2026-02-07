using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

public class WaitlistEntry : BaseEntity
{
    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(50)]
    public string? Source { get; set; } = "landing-page";
}
