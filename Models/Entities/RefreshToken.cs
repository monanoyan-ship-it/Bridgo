namespace Bridgo.Models.Entities;

public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }

    // Navigation
    public Models.Identity.ApplicationUser User { get; set; } = null!;
}
