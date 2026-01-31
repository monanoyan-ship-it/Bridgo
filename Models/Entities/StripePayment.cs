namespace Bridgo.Models.Entities;

/// <summary>
/// Stripe odeme kayitlari
/// </summary>
public class StripePayment : BaseEntity
{
    /// <summary>
    /// Siparis ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Stripe PaymentIntent ID
    /// </summary>
    public string? PaymentIntentId { get; set; }

    /// <summary>
    /// Stripe Charge ID
    /// </summary>
    public string? ChargeId { get; set; }

    /// <summary>
    /// Odeme tutari
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Para birimi
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Stripe durum (succeeded, pending, failed)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Hata kodu
    /// </summary>
    public string? FailureCode { get; set; }

    /// <summary>
    /// Hata mesaji
    /// </summary>
    public string? FailureMessage { get; set; }

    /// <summary>
    /// Makbuz URL'i
    /// </summary>
    public string? ReceiptUrl { get; set; }

    /// <summary>
    /// Odeme tarihi
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Iade tutari
    /// </summary>
    public decimal? RefundedAmount { get; set; }

    /// <summary>
    /// Iade tarihi
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    // Navigation properties
    public virtual Order? Order { get; set; }
}
