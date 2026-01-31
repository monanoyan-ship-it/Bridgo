using Microsoft.AspNetCore.Identity;
using Bridgo.Models.Entities;

namespace Bridgo.Models.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Dil tercihi
    public int? LanguageId { get; set; }
    public Language? Language { get; set; }

    // Vendor iliskisi
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    // Sistem admini mi? (VendorId olmadan tum sisteme erisim)
    public bool IsSystemAdmin { get; set; } = false;

    public string FullName => $"{FirstName} {LastName}".Trim();

    // VendorId var mi kontrolu
    public bool HasVendor => VendorId.HasValue;
}
