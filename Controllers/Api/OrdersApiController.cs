using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.Authorization;
using Bridgo.DTOs.Order;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/orders")]
[Authorize]
[RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
public class OrdersApiController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderOrchestrationService _orchestrationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersApiController(
        IOrderService orderService,
        IOrderOrchestrationService orchestrationService,
        UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _orchestrationService = orchestrationService;
        _userManager = userManager;
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

    private async Task<string?> GetUsernameAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.UserName;
    }

    // ============================================
    // BUYER ENDPOINTS
    // ============================================

    /// <summary>
    /// Alicinin siparislerini listele
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders([FromQuery] OrderFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _orderService.GetBuyerOrdersAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Siparis detayi (alici icin)
    /// </summary>
    [HttpGet("my/{id}")]
    public async Task<IActionResult> GetMyOrderDetail(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var order = await _orderService.GetOrderForBuyerAsync(id, vendorId.Value);
        if (order == null) return NotFound(new { message = "Siparis bulunamadi." });

        return Ok(order);
    }

    /// <summary>
    /// Dogrudan siparis olustur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();

        try
        {
            var order = await _orderService.CreateDirectOrderAsync(vendorId.Value, dto, username);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Inquiry'den siparis olustur
    /// </summary>
    [HttpPost("from-inquiry")]
    public async Task<IActionResult> CreateOrderFromInquiry([FromBody] CreateOrderFromInquiryDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();

        try
        {
            var order = await _orderService.CreateOrderFromInquiryAsync(vendorId.Value, dto, username);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Demand'dan siparis olustur
    /// </summary>
    [HttpPost("from-demand")]
    public async Task<IActionResult> CreateOrderFromDemand([FromBody] CreateOrderFromDemandDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();

        try
        {
            var order = await _orderService.CreateOrderFromDemandAsync(vendorId.Value, dto, username);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Siparisi iptal et (alici)
    /// </summary>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.CancelOrderByBuyerAsync(id, vendorId.Value, dto, username);

        if (!result) return BadRequest(new { message = "Siparis iptal edilemedi." });
        return Ok(new { message = "Siparis iptal edildi." });
    }

    /// <summary>
    /// Teslim aldim onayla
    /// </summary>
    [HttpPut("{id}/confirm-delivery")]
    public async Task<IActionResult> ConfirmDelivery(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.ConfirmDeliveryAsync(id, vendorId.Value, username);

        if (!result) return BadRequest(new { message = "Teslimat onaylanamadi." });
        return Ok(new { message = "Teslimat onaylandi." });
    }

    /// <summary>
    /// Sozlesmeyi onayla
    /// </summary>
    [HttpPut("{id}/accept-contract")]
    public async Task<IActionResult> AcceptContract(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.AcceptContractAsync(id, vendorId.Value, username);

        if (!result) return BadRequest(new { message = "Sozlesme onaylanamadi." });
        return Ok(new { message = "Sozlesme onaylandi." });
    }

    /// <summary>
    /// Alici istatistikleri
    /// </summary>
    [HttpGet("my/stats")]
    public async Task<IActionResult> GetBuyerStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var stats = await _orderService.GetBuyerStatsAsync(vendorId.Value);
        return Ok(stats);
    }

    // ============================================
    // SELLER ENDPOINTS
    // ============================================

    /// <summary>
    /// Saticinin siparislerini listele
    /// </summary>
    [HttpGet("seller")]
    public async Task<IActionResult> GetSellerOrders([FromQuery] OrderFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _orderService.GetSellerOrdersAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Siparis detayi (satici icin)
    /// </summary>
    [HttpGet("seller/{id}")]
    public async Task<IActionResult> GetSellerOrderDetail(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var order = await _orderService.GetOrderForSellerAsync(id, vendorId.Value);
        if (order == null) return NotFound(new { message = "Siparis bulunamadi." });

        return Ok(order);
    }

    /// <summary>
    /// Siparisi onayla (satici)
    /// </summary>
    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> ConfirmOrder(int id, [FromBody] ConfirmOrderDto? dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.ConfirmOrderAsync(id, vendorId.Value, dto?.Notes, username);

        if (!result) return BadRequest(new { message = "Siparis onaylanamadi." });
        return Ok(new { message = "Siparis onaylandi." });
    }

    /// <summary>
    /// Siparisi reddet (satici)
    /// </summary>
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectOrder(int id, [FromBody] RejectOrderDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.RejectOrderAsync(id, vendorId.Value, dto.Reason, username);

        if (!result) return BadRequest(new { message = "Siparis reddedilemedi." });
        return Ok(new { message = "Siparis reddedildi." });
    }

    /// <summary>
    /// Siparis durumunu guncelle (satici)
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.UpdateOrderStatusAsync(id, vendorId.Value, dto, username);

        if (!result) return BadRequest(new { message = "Durum guncellenemedi." });
        return Ok(new { message = "Durum guncellendi." });
    }

    /// <summary>
    /// Kargo ekle
    /// </summary>
    [HttpPost("{id}/shipments")]
    public async Task<IActionResult> CreateShipment(int id, [FromBody] CreateShipmentDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();

        try
        {
            var shipment = await _orderService.CreateShipmentAsync(id, vendorId.Value, dto, username);
            return Ok(shipment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kargo guncelle
    /// </summary>
    [HttpPut("{id}/shipments/{shipmentId}")]
    public async Task<IActionResult> UpdateShipment(int id, int shipmentId, [FromBody] UpdateShipmentDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.UpdateShipmentAsync(id, shipmentId, vendorId.Value, dto, username);

        if (!result) return BadRequest(new { message = "Kargo guncellenemedi." });
        return Ok(new { message = "Kargo guncellendi." });
    }

    /// <summary>
    /// Satici istatistikleri
    /// </summary>
    [HttpGet("seller/stats")]
    public async Task<IActionResult> GetSellerStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var stats = await _orderService.GetSellerStatsAsync(vendorId.Value);
        return Ok(stats);
    }

    // ============================================
    // SERVICE REQUEST ENDPOINTS (Buyer)
    // ============================================

    /// <summary>
    /// Servis talebi olustur (Buyer)
    /// </summary>
    [HttpPost("{id}/service-requests")]
    public async Task<IActionResult> CreateServiceRequest(int id, [FromBody] CreateServiceRequestDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();

        try
        {
            var request = await _orderService.CreateServiceRequestAsync(id, vendorId.Value, dto, username);
            return Ok(request);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Servis teklifini kabul et (Buyer)
    /// </summary>
    [HttpPut("quotes/{quoteId}/accept")]
    public async Task<IActionResult> AcceptServiceQuote(int quoteId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.AcceptServiceQuoteAsync(quoteId, vendorId.Value, username);

        if (!result) return BadRequest(new { message = "Teklif kabul edilemedi." });
        return Ok(new { message = "Teklif kabul edildi." });
    }

    /// <summary>
    /// Servis teklifini reddet (Buyer)
    /// </summary>
    [HttpPut("quotes/{quoteId}/reject")]
    public async Task<IActionResult> RejectServiceQuote(int quoteId, [FromBody] RejectQuoteDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.RejectServiceQuoteAsync(quoteId, vendorId.Value, dto.Reason, username);

        if (!result) return BadRequest(new { message = "Teklif reddedilemedi." });
        return Ok(new { message = "Teklif reddedildi." });
    }

    // ============================================
    // FINANCING ENDPOINTS
    // ============================================

    /// <summary>
    /// Siparisin checkout ilerleme durumunu getir (Buyer)
    /// </summary>
    [HttpGet("{orderId}/checkout-progress")]
    public async Task<IActionResult> GetCheckoutProgress(int orderId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        // Sadece buyer kendi siparisini gorebilir
        var order = await _orderService.GetOrderForBuyerAsync(orderId, vendorId.Value);
        if (order == null) return NotFound(new { message = "Siparis bulunamadi." });

        var progress = await _orchestrationService.GetCheckoutProgressAsync(orderId);
        return Ok(progress);
    }

    /// <summary>
    /// Siparisin finansman durumunu getir (Buyer)
    /// </summary>
    [HttpGet("{orderId}/financing-status")]
    public async Task<IActionResult> GetFinancingStatus(int orderId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        // Sadece buyer kendi siparisini gorebilir
        var order = await _orderService.GetOrderForBuyerAsync(orderId, vendorId.Value);
        if (order == null) return NotFound(new { message = "Siparis bulunamadi." });

        var status = await _orchestrationService.GetFinancingStatusAsync(orderId);
        return Ok(status);
    }

    /// <summary>
    /// Siparis icin finansman talep et (Buyer)
    /// Tum servisler secildikten sonra cagrilabilir
    /// </summary>
    [HttpPost("{orderId}/request-financing")]
    public async Task<IActionResult> RequestFinancing(int orderId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        // Sadece buyer kendi siparisi icin finansman isteyebilir
        var order = await _orderService.GetOrderForBuyerAsync(orderId, vendorId.Value);
        if (order == null) return NotFound(new { message = "Siparis bulunamadi." });

        var financingRequestId = await _orchestrationService.RequestFinancingAsync(orderId, vendorId.Value);

        if (financingRequestId == null)
        {
            return BadRequest(new { message = "Finansman talebi olusturulamadi. Tum servisler secilmis olmali." });
        }

        return Ok(new
        {
            message = "Finansman talebi olusturuldu.",
            financingRequestId = financingRequestId
        });
    }

    // ============================================
    // SERVICE PROVIDER ENDPOINTS
    // ============================================

    /// <summary>
    /// Acik servis taleplerini listele (Service Provider)
    /// </summary>
    [HttpGet("service-requests/open")]
    public async Task<IActionResult> GetOpenServiceRequests([FromQuery] int serviceType)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var requests = await _orderService.GetOpenServiceRequestsAsync(vendorId.Value, serviceType);
        return Ok(requests);
    }

    /// <summary>
    /// Servis talebi detayi (Service Provider)
    /// </summary>
    [HttpGet("service-requests/{id}")]
    public async Task<IActionResult> GetServiceRequestDetail(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var request = await _orderService.GetServiceRequestDetailAsync(id, vendorId.Value);
        if (request == null) return NotFound(new { message = "Talep bulunamadi." });

        return Ok(request);
    }

    /// <summary>
    /// Servis teklifi ver (Service Provider)
    /// </summary>
    [HttpPost("service-requests/{id}/quote")]
    public async Task<IActionResult> CreateServiceQuote(int id, [FromBody] CreateServiceQuoteDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        dto.ServiceRequestId = id;

        try
        {
            var quote = await _orderService.CreateServiceQuoteAsync(vendorId.Value, dto, username);
            return Ok(quote);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Servis teklifi guncelle (Service Provider)
    /// </summary>
    [HttpPut("quotes/{quoteId}")]
    public async Task<IActionResult> UpdateServiceQuote(int quoteId, [FromBody] UpdateServiceQuoteDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.UpdateServiceQuoteAsync(quoteId, vendorId.Value, dto, username);

        if (!result) return BadRequest(new { message = "Teklif guncellenemedi." });
        return Ok(new { message = "Teklif guncellendi." });
    }

    /// <summary>
    /// Servis teklifi geri cek (Service Provider)
    /// </summary>
    [HttpDelete("quotes/{quoteId}")]
    public async Task<IActionResult> WithdrawServiceQuote(int quoteId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.WithdrawServiceQuoteAsync(quoteId, vendorId.Value, username);

        if (!result) return BadRequest(new { message = "Teklif geri cekilemedi." });
        return Ok(new { message = "Teklif geri cekildi." });
    }

    // ============================================
    // TASK ENDPOINTS
    // ============================================

    /// <summary>
    /// Katilimcinin gorevlerini listele
    /// </summary>
    [HttpGet("{id}/tasks")]
    public async Task<IActionResult> GetParticipantTasks(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var tasks = await _orderService.GetParticipantTasksAsync(id, vendorId.Value);
        return Ok(tasks);
    }

    /// <summary>
    /// Gorevi baslat
    /// </summary>
    [HttpPut("tasks/{taskId}/start")]
    public async Task<IActionResult> StartTask(int taskId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.StartTaskAsync(taskId, vendorId.Value, username);

        if (!result) return BadRequest(new { message = "Gorev baslatilamadi." });
        return Ok(new { message = "Gorev basladi." });
    }

    /// <summary>
    /// Gorevi tamamla
    /// </summary>
    [HttpPut("tasks/{taskId}/complete")]
    public async Task<IActionResult> CompleteTask(int taskId, [FromBody] CompleteTaskDto? dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.CompleteTaskAsync(taskId, vendorId.Value, dto?.Notes, username);

        if (!result) return BadRequest(new { message = "Gorev tamamlanamadi." });
        return Ok(new { message = "Gorev tamamlandi." });
    }

    // ============================================
    // INVESTMENT ENDPOINTS
    // ============================================

    /// <summary>
    /// Yatirim firsatlarini listele (Investor)
    /// </summary>
    [HttpGet("investments/opportunities")]
    public async Task<IActionResult> GetInvestmentOpportunities()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var opportunities = await _orderService.GetInvestmentOpportunitiesAsync(vendorId.Value);
        return Ok(opportunities);
    }

    /// <summary>
    /// Yatirim teklifi ver (Investor)
    /// </summary>
    [HttpPost("investments")]
    public async Task<IActionResult> CreateInvestment([FromBody] CreateInvestmentDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();

        try
        {
            var investment = await _orderService.CreateInvestmentAsync(vendorId.Value, dto, username);
            return Ok(investment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Yatirim teklifi kabul et (Buyer)
    /// </summary>
    [HttpPut("investments/{investmentId}/accept")]
    public async Task<IActionResult> AcceptInvestment(int investmentId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.AcceptInvestmentAsync(investmentId, vendorId.Value, username);

        if (!result) return BadRequest(new { message = "Yatirim kabul edilemedi." });
        return Ok(new { message = "Yatirim kabul edildi." });
    }

    /// <summary>
    /// Yatirim teklifi reddet (Buyer)
    /// </summary>
    [HttpPut("investments/{investmentId}/reject")]
    public async Task<IActionResult> RejectInvestment(int investmentId, [FromBody] RejectInvestmentDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var username = await GetUsernameAsync();
        var result = await _orderService.RejectInvestmentAsync(investmentId, vendorId.Value, dto.Reason, username);

        if (!result) return BadRequest(new { message = "Yatirim reddedilemedi." });
        return Ok(new { message = "Yatirim reddedildi." });
    }

    // ============================================
    // COMMON ENDPOINTS
    // ============================================

    /// <summary>
    /// Siparis durum gecmisi
    /// </summary>
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetOrderStatusHistory(int id)
    {
        var history = await _orderService.GetOrderStatusHistoryAsync(id);
        return Ok(history);
    }

    /// <summary>
    /// Kargo takip
    /// </summary>
    [HttpGet("shipments/{shipmentId}/track")]
    public async Task<IActionResult> GetShipmentTracking(int shipmentId)
    {
        var shipment = await _orderService.GetShipmentTrackingAsync(shipmentId);
        if (shipment == null) return NotFound(new { message = "Kargo bulunamadi." });

        return Ok(shipment);
    }
}

// ============================================
// HELPER DTOs
// ============================================

public class ConfirmOrderDto
{
    public string? Notes { get; set; }
}

public class RejectOrderDto
{
    public string? Reason { get; set; }
}

public class RejectQuoteDto
{
    public string? Reason { get; set; }
}

public class CompleteTaskDto
{
    public string? Notes { get; set; }
}

public class RejectInvestmentDto
{
    public string? Reason { get; set; }
}
