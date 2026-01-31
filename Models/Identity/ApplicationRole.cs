using Microsoft.AspNetCore.Identity;

namespace Bridgo.Models.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
