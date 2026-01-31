using Bridgo.Data;
using Bridgo.DTOs.Order;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Bridgo.Services;

/// <summary>
/// Stripe odeme islemleri servisi
/// </summary>
public class StripeService : IStripeService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeService> _logger;
    private readonly ICompanyService _companyService;
    private readonly string _webhookSecret;

    public StripeService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<StripeService> logger,
        ICompanyService companyService)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _companyService = companyService;

        // Stripe API key ayarla
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        _webhookSecret = _configuration["Stripe:WebhookSecret"] ?? string.Empty;
    }

    /// <summary>
    /// Siparis icin PaymentIntent olustur
    /// </summary>
    public async Task<PaymentIntentResultDto> CreatePaymentIntentAsync(Order order)
    {
        try
        {
            // Stripe tutari cent/kurus cinsinden bekler
            var amountInCents = (long)(order.TotalAmount * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = order.Currency.ToLower(),
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", order.Id.ToString() },
                    { "order_number", order.OrderNumber }
                },
                Description = $"Siparis: {order.OrderNumber}",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            // Order'a PaymentIntent ID kaydet
            order.StripePaymentIntentId = paymentIntent.Id;
            order.PaymentStatus = PaymentStatuses.Processing.Id;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Ayrica StripePayment kaydı olustur
            var stripePayment = new StripePayment
            {
                OrderId = order.Id,
                PaymentIntentId = paymentIntent.Id,
                Amount = order.TotalAmount,
                Currency = order.Currency,
                Status = paymentIntent.Status,
                CreatedAt = DateTime.UtcNow
            };
            _context.StripePayments.Add(stripePayment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("PaymentIntent created: {PaymentIntentId} for Order: {OrderNumber}",
                paymentIntent.Id, order.OrderNumber);

            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Amount = order.TotalAmount,
                Currency = order.Currency
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe PaymentIntent creation failed for Order: {OrderNumber}", order.OrderNumber);
            throw new Exception($"Odeme islemi baslatilirken hata olustu: {ex.Message}");
        }
    }

    /// <summary>
    /// PaymentIntent'i onayla
    /// </summary>
    public async Task<PaymentIntentResultDto> ConfirmPaymentIntentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.ConfirmAsync(paymentIntentId);

            return new PaymentIntentResultDto
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Amount = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency.ToUpper()
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe PaymentIntent confirmation failed: {PaymentIntentId}", paymentIntentId);
            throw new Exception($"Odeme onaylanirken hata olustu: {ex.Message}");
        }
    }

    /// <summary>
    /// PaymentIntent'i iptal et
    /// </summary>
    public async Task<bool> CancelPaymentIntentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            await service.CancelAsync(paymentIntentId);

            // Veritabanindaki kaydı guncelle
            var stripePayment = await _context.StripePayments
                .FirstOrDefaultAsync(sp => sp.PaymentIntentId == paymentIntentId);

            if (stripePayment != null)
            {
                stripePayment.Status = "canceled";
                stripePayment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("PaymentIntent cancelled: {PaymentIntentId}", paymentIntentId);
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe PaymentIntent cancellation failed: {PaymentIntentId}", paymentIntentId);
            return false;
        }
    }

    /// <summary>
    /// Odeme durumunu kontrol et
    /// </summary>
    public async Task<PaymentStatusDto> GetPaymentStatusAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            // Order'i bul
            var orderId = int.Parse(paymentIntent.Metadata["order_id"]);
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                throw new Exception("Siparis bulunamadi");
            }

            return new PaymentStatusDto
            {
                OrderId = orderId,
                OrderNumber = order.OrderNumber,
                PaymentStatus = (int)order.PaymentStatus,
                PaymentStatusText = order.PaymentStatus.ToString(),
                Amount = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency.ToUpper(),
                PaidAt = order.PaidAt
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe GetPaymentStatus failed: {PaymentIntentId}", paymentIntentId);
            throw new Exception($"Odeme durumu alinamadi: {ex.Message}");
        }
    }

    /// <summary>
    /// Iade islemi yap
    /// </summary>
    public async Task<RefundResultDto> CreateRefundAsync(string chargeId, decimal? amount = null, string? reason = null)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                Charge = chargeId
            };

            if (amount.HasValue)
            {
                options.Amount = (long)(amount.Value * 100);
            }

            if (!string.IsNullOrEmpty(reason))
            {
                options.Reason = reason switch
                {
                    "duplicate" => "duplicate",
                    "fraudulent" => "fraudulent",
                    _ => "requested_by_customer"
                };
            }

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            _logger.LogInformation("Refund created: {RefundId} for Charge: {ChargeId}", refund.Id, chargeId);

            return new RefundResultDto
            {
                Success = true,
                RefundId = refund.Id,
                RefundedAmount = refund.Amount / 100m
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe Refund failed for Charge: {ChargeId}", chargeId);
            return new RefundResultDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Stripe webhook'u isle
    /// </summary>
    public async Task<WebhookResult> HandleWebhookAsync(string json, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, signature, _webhookSecret);

            _logger.LogInformation("Stripe webhook received: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    return await HandlePaymentSucceededAsync(stripeEvent);

                case "payment_intent.payment_failed":
                    return await HandlePaymentFailedAsync(stripeEvent);

                case "charge.refunded":
                    return await HandleChargeRefundedAsync(stripeEvent);

                default:
                    _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    return WebhookResult.Ok(stripeEvent.Type, "Event received but not processed");
            }
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed");
            return WebhookResult.Fail($"Webhook signature verification failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook processing failed");
            return WebhookResult.Fail($"Webhook processing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Odeme basarili eventi
    /// </summary>
    private async Task<WebhookResult> HandlePaymentSucceededAsync(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            return WebhookResult.Fail("PaymentIntent object not found in event");
        }

        // Order'i bul
        if (!paymentIntent.Metadata.TryGetValue("order_id", out var orderIdStr) ||
            !int.TryParse(orderIdStr, out var orderId))
        {
            return WebhookResult.Fail("Order ID not found in PaymentIntent metadata");
        }

        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return WebhookResult.Fail($"Order not found: {orderId}");
        }

        // Order durumunu guncelle
        order.PaymentStatus = PaymentStatuses.Succeeded.Id;
        order.Status = OrderStatuses.PendingConfirm.Id; // Odeme alindi, satici onayı bekliyor
        order.StripeChargeId = paymentIntent.LatestChargeId;
        order.PaidAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // StripePayment kaydını guncelle
        var stripePayment = await _context.StripePayments
            .FirstOrDefaultAsync(sp => sp.PaymentIntentId == paymentIntent.Id);

        if (stripePayment != null)
        {
            stripePayment.ChargeId = paymentIntent.LatestChargeId;
            stripePayment.Status = "succeeded";
            stripePayment.PaidAt = DateTime.UtcNow;
            stripePayment.UpdatedAt = DateTime.UtcNow;
        }

        // Durum gecmisi ekle
        var history = new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = OrderStatuses.PendingPayment.Id,
            NewStatus = OrderStatuses.PendingConfirm.Id,
            Notes = "Odeme alindi (Stripe)",
            CreatedAt = DateTime.UtcNow
        };
        _context.OrderStatusHistory.Add(history);

        await _context.SaveChangesAsync();

        // Aliciya Buyer capability ekle (yoksa)
        try
        {
            await EnsureBuyerCapabilityAsync(order.BuyerVendorId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add Buyer capability for Vendor: {VendorId}", order.BuyerVendorId);
            // Odeme basarili, capability ekleme hatasi kritik degil, devam et
        }

        _logger.LogInformation("Payment succeeded for Order: {OrderNumber}", order.OrderNumber);
        return WebhookResult.Ok("payment_intent.succeeded", $"Order {order.OrderNumber} payment succeeded", orderId);
    }

    /// <summary>
    /// Odeme basarisiz eventi
    /// </summary>
    private async Task<WebhookResult> HandlePaymentFailedAsync(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            return WebhookResult.Fail("PaymentIntent object not found in event");
        }

        // Order'i bul
        if (!paymentIntent.Metadata.TryGetValue("order_id", out var orderIdStr) ||
            !int.TryParse(orderIdStr, out var orderId))
        {
            return WebhookResult.Fail("Order ID not found in PaymentIntent metadata");
        }

        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return WebhookResult.Fail($"Order not found: {orderId}");
        }

        // Order durumunu guncelle
        order.PaymentStatus = PaymentStatuses.Failed.Id;
        order.Status = OrderStatuses.PaymentFailed.Id;
        order.UpdatedAt = DateTime.UtcNow;

        // StripePayment kaydını guncelle
        var stripePayment = await _context.StripePayments
            .FirstOrDefaultAsync(sp => sp.PaymentIntentId == paymentIntent.Id);

        if (stripePayment != null)
        {
            stripePayment.Status = "failed";
            stripePayment.FailureCode = paymentIntent.LastPaymentError?.Code;
            stripePayment.FailureMessage = paymentIntent.LastPaymentError?.Message;
            stripePayment.UpdatedAt = DateTime.UtcNow;
        }

        // Durum gecmisi ekle
        var history = new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = OrderStatuses.PendingPayment.Id,
            NewStatus = OrderStatuses.PaymentFailed.Id,
            Notes = $"Odeme basarisiz: {paymentIntent.LastPaymentError?.Message}",
            CreatedAt = DateTime.UtcNow
        };
        _context.OrderStatusHistory.Add(history);

        await _context.SaveChangesAsync();

        _logger.LogWarning("Payment failed for Order: {OrderNumber}", order.OrderNumber);
        return WebhookResult.Ok("payment_intent.payment_failed", $"Order {order.OrderNumber} payment failed", orderId);
    }

    /// <summary>
    /// Iade eventi
    /// </summary>
    private async Task<WebhookResult> HandleChargeRefundedAsync(Event stripeEvent)
    {
        var charge = stripeEvent.Data.Object as Charge;
        if (charge == null)
        {
            return WebhookResult.Fail("Charge object not found in event");
        }

        // StripePayment kaydını ChargeId ile bul
        var stripePayment = await _context.StripePayments
            .Include(sp => sp.Order)
            .FirstOrDefaultAsync(sp => sp.ChargeId == charge.Id);

        if (stripePayment == null)
        {
            _logger.LogWarning("StripePayment not found for Charge: {ChargeId}", charge.Id);
            return WebhookResult.Fail($"StripePayment not found for Charge: {charge.Id}");
        }

        var order = stripePayment.Order;

        // Tam iade mi kismi iade mi?
        var refundedAmount = charge.AmountRefunded / 100m;
        var isFullRefund = charge.AmountRefunded == charge.Amount;

        // Order durumunu guncelle
        order.PaymentStatus = isFullRefund ? PaymentStatuses.Refunded.Id : PaymentStatuses.PartiallyRefunded.Id;
        order.Status = isFullRefund ? OrderStatuses.Refunded.Id : order.Status;
        order.UpdatedAt = DateTime.UtcNow;

        // StripePayment kaydını guncelle
        stripePayment.Status = isFullRefund ? "refunded" : "partially_refunded";
        stripePayment.UpdatedAt = DateTime.UtcNow;

        // Durum gecmisi ekle
        var history = new OrderStatusHistory
        {
            OrderId = order.Id,
            OldStatus = order.Status,
            NewStatus = isFullRefund ? OrderStatuses.Refunded.Id : order.Status,
            Notes = $"Iade yapildi: {refundedAmount} {order.Currency}",
            CreatedAt = DateTime.UtcNow
        };
        _context.OrderStatusHistory.Add(history);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Refund processed for Order: {OrderNumber}, Amount: {Amount}",
            order.OrderNumber, refundedAmount);

        return WebhookResult.Ok("charge.refunded",
            $"Order {order.OrderNumber} refunded: {refundedAmount} {order.Currency}",
            order.Id);
    }

    /// <summary>
    /// Buyer capability'si yoksa ekle ve default role ata
    /// </summary>
    private async Task EnsureBuyerCapabilityAsync(int vendorId)
    {
        var added = await _companyService.EnsureVendorCapabilityWithDefaultRoleAsync(vendorId, Capabilities.Ids.Buyer);
        if (added)
        {
            _logger.LogInformation("Buyer capability and default role added for Vendor: {VendorId} after successful payment", vendorId);
        }
    }
}
