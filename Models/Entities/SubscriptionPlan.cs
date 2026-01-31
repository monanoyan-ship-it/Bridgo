namespace Bridgo.Models.Entities;

/// <summary>
/// Abonelik planlari
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? NameResourceKey { get; set; }
    public string? Description { get; set; }

    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// monthly, yearly
    /// </summary>
    public string BillingPeriod { get; set; } = "monthly";

    // Limitler
    public int? ProductLimit { get; set; }
    public int? OrderLimit { get; set; }
    public int? TeamMemberLimit { get; set; }
    public int? StorageLimitMB { get; set; }

    // Ozellikler (JSON olarak tutulabilir veya ayri tablo)
    public string? FeaturesJson { get; set; }

    public bool IsFree { get; set; }
    public bool IsPopular { get; set; }
    public int DisplayOrder { get; set; }

    // Stripe
    public string? StripePriceId { get; set; }
    public string? StripeProductId { get; set; }

    public ICollection<VendorSubscription>? VendorSubscriptions { get; set; }
}
