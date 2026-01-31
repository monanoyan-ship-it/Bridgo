using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.Authorization;
using Bridgo.DTOs.Stock;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/stock")]
[Authorize]
[RequireCapability(VendorCapabilities.Seller)]
public class StockApiController : ControllerBase
{
    private readonly IStockService _stockService;
    private readonly UserManager<ApplicationUser> _userManager;

    public StockApiController(
        IStockService stockService,
        UserManager<ApplicationUser> userManager)
    {
        _stockService = stockService;
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

    // ============================================
    // STOK HAREKETLERI
    // ============================================

    /// <summary>
    /// Stok hareketlerini listele
    /// </summary>
    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements([FromQuery] StockMovementFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _stockService.GetMovementsAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Urun stok hareketlerini getir
    /// </summary>
    [HttpGet("products/{productId}/movements")]
    public async Task<IActionResult> GetProductMovements(int productId, [FromQuery] StockMovementFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _stockService.GetProductMovementsAsync(vendorId.Value, productId, filter);
        return Ok(result);
    }

    /// <summary>
    /// Stok girisi yap
    /// </summary>
    [HttpPost("in")]
    public async Task<IActionResult> AddStock([FromBody] CreateStockMovementDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var userId = GetUserId();
        var result = await _stockService.AddStockAsync(vendorId.Value, userId, dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    /// <summary>
    /// Stok cikisi yap
    /// </summary>
    [HttpPost("out")]
    public async Task<IActionResult> RemoveStock([FromBody] CreateStockMovementDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var userId = GetUserId();
        var result = await _stockService.RemoveStockAsync(vendorId.Value, userId, dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    /// <summary>
    /// Stok transferi yap
    /// </summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> TransferStock([FromBody] StockTransferDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var userId = GetUserId();
        var result = await _stockService.TransferStockAsync(vendorId.Value, userId, dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Stok duzeltmesi yap
    /// </summary>
    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var userId = GetUserId();
        var result = await _stockService.AdjustStockAsync(vendorId.Value, userId, dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    // ============================================
    // STOK SORGULARI
    // ============================================

    /// <summary>
    /// Urun stok ozetini getir
    /// </summary>
    [HttpGet("products/{productId}/summary")]
    public async Task<IActionResult> GetProductStockSummary(int productId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _stockService.GetProductStockSummaryAsync(vendorId.Value, productId);
        if (result == null) return NotFound(new { message = "Urun bulunamadi." });

        return Ok(result);
    }

    /// <summary>
    /// Tum urunlerin stok ozetini getir
    /// </summary>
    [HttpGet("products/summary")]
    public async Task<IActionResult> GetAllProductStockSummaries()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _stockService.GetAllProductStockSummariesAsync(vendorId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Dusuk stok alarmlarini getir
    /// </summary>
    [HttpGet("alerts/low-stock")]
    public async Task<IActionResult> GetLowStockAlerts()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _stockService.GetLowStockAlertsAsync(vendorId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Stok istatistiklerini getir
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStockStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _stockService.GetStockStatsAsync(vendorId.Value);
        return Ok(result);
    }
}
