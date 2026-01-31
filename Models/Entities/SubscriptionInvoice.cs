namespace Bridgo.Models.Entities;

/// <summary>
/// Abonelik faturalari
/// </summary>
public class SubscriptionInvoice : BaseEntity
{
    public int VendorSubscriptionId { get; set; }
    public VendorSubscription? VendorSubscription { get; set; }

    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }

    public decimal Amount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// pending, paid, cancelled, refunded
    /// </summary>
    public string Status { get; set; } = "pending";

    public DateTime? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }

    // Stripe
    public string? StripeInvoiceId { get; set; }
    public string? StripePaymentIntentId { get; set; }

    // PDF
    public string? PdfUrl { get; set; }
}
