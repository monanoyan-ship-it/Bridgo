using Microsoft.AspNetCore.Authorization;

namespace Bridgo.Authorization;

/// <summary>
/// Belirtilen capability'lerden en az birine sahip olmayı gerektirir.
/// Vendor'ın ilgili capability'si yoksa 403 Forbidden döner.
/// </summary>
/// <example>
/// [RequireCapability("Seller")]
/// [RequireCapability("Seller", "Buyer")]
/// [RequireCapability(VendorCapabilities.Seller)]
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireCapabilityAttribute : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
    public string[] Capabilities { get; }

    public RequireCapabilityAttribute(params string[] capabilities)
    {
        Capabilities = capabilities;
        Policy = $"Capability_{string.Join("_", capabilities)}";
    }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}

/// <summary>
/// Capability sabitleri - magic string kullanımını önler
/// Veritabanındaki VendorCapabilities.Code değerleri ile eşleşir
/// </summary>
public static class VendorCapabilities
{
    public const string Seller = "seller";
    public const string Buyer = "buyer";
    public const string Carrier = "carrier";
    public const string Insurance = "insurance";
    public const string Customs = "customs";
    public const string Survey = "survey";
    public const string Investor = "investor";
}
