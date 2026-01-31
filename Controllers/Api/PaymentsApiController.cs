using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.Authorization;
using Bridgo.DTOs.Order;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;
using Bridgo.Data;
using Microsoft.EntityFrameworkCore;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/payments")]
[Authorize]
[RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
public class PaymentsApiController : ControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly IOrderService _orderService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PaymentsApiController> _logger;

    public PaymentsApiController(
        IStripeService stripeService,
        IOrderService orderService,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<PaymentsApiController> logger)
    {
        _stripeService = stripeService;
        _orderService = orderService;
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = GetUserId();
        return userId == 0 ? null : await _userManager.FindByIdAsync(userId.ToString());
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.VendorId;
    }

    // =====================================================
    // PAYMENT INTENT OPERATIONS
    // =====================================================

    /// <summary>
    /// Siparis icin PaymentIntent olustur
    /// POST /api/payments/create-intent
    /// </summary>
    [HttpPost("create-intent")]
    [Authorize]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentDto dto)
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Kullanici firma bilgisi bulunamadi." });

            // Order'i bul ve yetki kontrol
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.BuyerVendorId == vendorId.Value);

            if (order == null)
                return NotFound(new { message = "Siparis bulunamadi." });

            // PaymentIntent olustur
            var result = await _stripeService.CreatePaymentIntentAsync(order);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePaymentIntent failed for OrderId: {OrderId}", dto.OrderId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Odeme durumunu kontrol et
    /// GET /api/payments/{orderId}/status
    /// </summary>
    [HttpGet("{orderId}/status")]
    [Authorize]
    public async Task<IActionResult> GetPaymentStatus(int orderId)
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Kullanici firma bilgisi bulunamadi." });

            // Order'i bul
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId &&
                    (o.BuyerVendorId == vendorId.Value || o.SellerVendorId == vendorId.Value));

            if (order == null)
                return NotFound(new { message = "Siparis bulunamadi." });

            if (string.IsNullOrEmpty(order.StripePaymentIntentId))
                return BadRequest(new { message = "Bu siparis icin odeme baslatilmamis." });

            var result = await _stripeService.GetPaymentStatusAsync(order.StripePaymentIntentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPaymentStatus failed for OrderId: {OrderId}", orderId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PaymentIntent'i iptal et
    /// POST /api/payments/{orderId}/cancel
    /// </summary>
    [HttpPost("{orderId}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelPaymentIntent(int orderId)
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Kullanici firma bilgisi bulunamadi." });

            // Order'i bul
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerVendorId == vendorId.Value);

            if (order == null)
                return NotFound(new { message = "Siparis bulunamadi." });

            if (string.IsNullOrEmpty(order.StripePaymentIntentId))
                return BadRequest(new { message = "Bu siparis icin odeme baslatilmamis." });

            var success = await _stripeService.CancelPaymentIntentAsync(order.StripePaymentIntentId);
            if (!success)
                return BadRequest(new { message = "Odeme iptal edilemedi." });

            return Ok(new { message = "Odeme iptal edildi." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CancelPaymentIntent failed for OrderId: {OrderId}", orderId);
            return BadRequest(new { message = ex.Message });
        }
    }

    // =====================================================
    // REFUND OPERATIONS
    // =====================================================

    /// <summary>
    /// Iade yap (tam veya kismi)
    /// POST /api/payments/{orderId}/refund
    /// </summary>
    [HttpPost("{orderId}/refund")]
    [Authorize]
    public async Task<IActionResult> CreateRefund(int orderId, [FromBody] RefundRequestDto dto)
    {
        try
        {
            var vendorId = await GetUserVendorIdAsync();
            if (!vendorId.HasValue)
                return Unauthorized(new { message = "Kullanici firma bilgisi bulunamadi." });

            // Order'i bul - sadece satici iade yapabilir
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.SellerVendorId == vendorId.Value);

            if (order == null)
                return NotFound(new { message = "Siparis bulunamadi." });

            if (string.IsNullOrEmpty(order.StripeChargeId))
                return BadRequest(new { message = "Bu siparis icin tamamlanmis odeme bulunamadi." });

            var result = await _stripeService.CreateRefundAsync(
                order.StripeChargeId,
                dto.Amount,
                dto.Reason);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage ?? "Iade islemi basarisiz." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateRefund failed for OrderId: {OrderId}", orderId);
            return BadRequest(new { message = ex.Message });
        }
    }

    // =====================================================
    // STRIPE WEBHOOK
    // =====================================================

    /// <summary>
    /// Stripe webhook handler
    /// POST /api/payments/webhook
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook()
    {
        try
        {
            // Request body'yi oku
            string json;
            using (var reader = new StreamReader(Request.Body))
            {
                json = await reader.ReadToEndAsync();
            }

            // Stripe signature header'i al
            var signature = Request.Headers["Stripe-Signature"].ToString();

            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Stripe webhook received without signature");
                return BadRequest(new { message = "Missing Stripe-Signature header" });
            }

            // Webhook'u isle
            var result = await _stripeService.HandleWebhookAsync(json, signature);

            if (!result.Success)
            {
                _logger.LogWarning("Stripe webhook processing failed: {Message}", result.Message);
                return BadRequest(new { message = result.Message });
            }

            _logger.LogInformation("Stripe webhook processed: {EventType} - {Message}",
                result.EventType, result.Message);

            return Ok(new { received = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook handler error");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

/// <summary>
/// PaymentIntent olusturma istegi
/// </summary>
public class CreatePaymentIntentDto
{
    public int OrderId { get; set; }
}
