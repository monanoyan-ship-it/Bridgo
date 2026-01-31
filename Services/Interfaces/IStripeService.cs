using Bridgo.DTOs.Order;
using Bridgo.Models.Entities;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Stripe odeme islemleri servisi
/// </summary>
public interface IStripeService
{
    /// <summary>
    /// Siparis icin PaymentIntent olustur
    /// </summary>
    Task<PaymentIntentResultDto> CreatePaymentIntentAsync(Order order);

    /// <summary>
    /// PaymentIntent'i onayla
    /// </summary>
    Task<PaymentIntentResultDto> ConfirmPaymentIntentAsync(string paymentIntentId);

    /// <summary>
    /// PaymentIntent'i iptal et
    /// </summary>
    Task<bool> CancelPaymentIntentAsync(string paymentIntentId);

    /// <summary>
    /// Odeme durumunu kontrol et
    /// </summary>
    Task<PaymentStatusDto> GetPaymentStatusAsync(string paymentIntentId);

    /// <summary>
    /// Iade islemi yap
    /// </summary>
    Task<RefundResultDto> CreateRefundAsync(string chargeId, decimal? amount = null, string? reason = null);

    /// <summary>
    /// Stripe webhook'u isle
    /// </summary>
    Task<WebhookResult> HandleWebhookAsync(string json, string signature);
}

/// <summary>
/// Webhook isleme sonucu
/// </summary>
public class WebhookResult
{
    public bool Success { get; set; }
    public string? EventType { get; set; }
    public string? Message { get; set; }
    public int? OrderId { get; set; }

    public static WebhookResult Ok(string eventType, string message, int? orderId = null)
        => new() { Success = true, EventType = eventType, Message = message, OrderId = orderId };

    public static WebhookResult Fail(string message)
        => new() { Success = false, Message = message };
}
