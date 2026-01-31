using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Bridgo.Authorization;
using Bridgo.Data;
using Bridgo.Models.Enums;
using Bridgo.Models.Identity;

namespace Bridgo.Services;

/// <summary>
/// Custom claims factory - IsSystemAdmin ve VendorCapability claim'leri ekler
/// </summary>
public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    private readonly ApplicationDbContext _context;

    public CustomUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> options,
        ApplicationDbContext context)
        : base(userManager, roleManager, options)
    {
        _context = context;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // IsSystemAdmin claim'i ekle
        identity.AddClaim(new Claim("IsSystemAdmin", user.IsSystemAdmin.ToString().ToLower()));

        // VendorId claim'i ekle (varsa)
        if (user.VendorId.HasValue)
        {
            identity.AddClaim(new Claim("VendorId", user.VendorId.Value.ToString()));

            // Vendor'ın capability ID'lerini al ve claim olarak ekle
            var capabilityIds = await _context.VendorCapabilityMappings
                .Where(m => m.VendorId == user.VendorId.Value && m.IsActive)
                .Select(m => m.CapabilityId)
                .ToListAsync();

            foreach (var capabilityId in capabilityIds)
            {
                // Capability'nin SystemName'ini claim olarak ekle
                var capability = Capabilities.GetById(capabilityId);
                if (capability != null)
                {
                    identity.AddClaim(new Claim(CapabilityAuthorizationHandler.CapabilityClaimType, capability.SystemName));
                }
            }
        }

        return identity;
    }
}
