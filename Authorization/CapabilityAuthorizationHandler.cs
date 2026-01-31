using Microsoft.AspNetCore.Authorization;

namespace Bridgo.Authorization;

/// <summary>
/// RequireCapabilityAttribute için authorization handler.
/// Kullanıcının claim'lerinden capability kontrolü yapar.
/// </summary>
public class CapabilityAuthorizationHandler : AuthorizationHandler<RequireCapabilityAttribute>
{
    public const string CapabilityClaimType = "VendorCapability";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequireCapabilityAttribute requirement)
    {
        // Kullanıcı giriş yapmamışsa
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return Task.CompletedTask;
        }

        // System Admin her şeye erişebilir
        if (context.User.HasClaim("IsSystemAdmin", "true"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Kullanıcının capability claim'lerini al
        var userCapabilities = context.User.Claims
            .Where(c => c.Type == CapabilityClaimType)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Gerekli capability'lerden en az biri var mı?
        var hasRequiredCapability = requirement.Capabilities
            .Any(required => userCapabilities.Contains(required));

        if (hasRequiredCapability)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
