namespace Bridgo.Models.Entities;

/// <summary>
/// Vendor abonelik kaydi
/// </summary>
public class VendorSubscription : BaseEntity
{
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public int PlanId { get; set; }
    public SubscriptionPlan? Plan { get; set; }

    /// <summary>
    /// active, cancelled, expired, past_due
    /// </summary>
    public string Status { get; set; } = "active";

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Stripe
    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }

    // Faturalama
    public decimal? LastPaymentAmount { get; set; }
    public DateTime? LastPaymentDate { get; set; }
}
