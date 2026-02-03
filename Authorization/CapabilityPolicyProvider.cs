using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Bridgo.Authorization;

/// <summary>
/// Capability policy'lerini dinamik olarak olusturur.
/// "Capability_seller", "Capability_seller_buyer" gibi policy'leri otomatik handle eder.
/// Dual auth: Cookie + JWT Bearer scheme destegi.
/// </summary>
public class CapabilityPolicyProvider : IAuthorizationPolicyProvider
{
    private const string PolicyPrefix = "Capability_";
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public CapabilityPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            // "Capability_seller_buyer" -> ["seller", "buyer"]
            var capabilities = policyName
                .Substring(PolicyPrefix.Length)
                .Split('_', StringSplitOptions.RemoveEmptyEntries);

            var policy = new AuthorizationPolicyBuilder(
                    IdentityConstants.ApplicationScheme,
                    JwtBearerDefaults.AuthenticationScheme)
                .AddRequirements(new RequireCapabilityAttribute(capabilities))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Diger policy'ler icin default provider kullan
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
