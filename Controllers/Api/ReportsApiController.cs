using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.Authorization;
using Bridgo.DTOs.Report;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/reports")]
[Authorize]
[RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
public class ReportsApiController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportsApiController(
        IReportService reportService,
        UserManager<ApplicationUser> userManager)
    {
        _reportService = reportService;
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
    // SELLER REPORTS
    // ============================================

    /// <summary>
    /// Satici raporu - Ozet
    /// </summary>
    [HttpGet("seller")]
    public async Task<IActionResult> GetSellerReport([FromQuery] ReportFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _reportService.GetSellerReportAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Satici genel bakis
    /// </summary>
    [HttpGet("seller/overview")]
    public async Task<IActionResult> GetSellerOverview()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _reportService.GetSellerOverviewAsync(vendorId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Satislar - donem bazli
    /// </summary>
    [HttpGet("seller/sales-by-period")]
    public async Task<IActionResult> GetSalesByPeriod([FromQuery] ReportFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _reportService.GetSalesByPeriodAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// En cok satan urunler
    /// </summary>
    [HttpGet("seller/top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int count = 10)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _reportService.GetTopProductsAsync(vendorId.Value, count);
        return Ok(result);
    }

    /// <summary>
    /// En iyi musteriler
    /// </summary>
    [HttpGet("seller/top-customers")]
    public async Task<IActionResult> GetTopCustomers([FromQuery] int count = 10)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _reportService.GetTopCustomersAsync(vendorId.Value, count);
        return Ok(result);
    }

    // ============================================
    // BUYER REPORTS
    // ============================================

    /// <summary>
    /// Alici raporu - Ozet
    /// </summary>
    [HttpGet("buyer")]
    public async Task<IActionResult> GetBuyerReport([FromQuery] ReportFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _reportService.GetBuyerReportAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Alici genel bakis
    /// </summary>
    [HttpGet("buyer/overview")]
    public async Task<IActionResult> GetBuyerOverview()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _reportService.GetBuyerOverviewAsync(vendorId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Satin almalar - donem bazli
    /// </summary>
    [HttpGet("buyer/purchases-by-period")]
    public async Task<IActionResult> GetPurchasesByPeriod([FromQuery] ReportFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _reportService.GetPurchasesByPeriodAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// En cok alisveris yapilan tedarikciler
    /// </summary>
    [HttpGet("buyer/top-suppliers")]
    public async Task<IActionResult> GetTopSuppliers([FromQuery] int count = 10)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _reportService.GetTopSuppliersAsync(vendorId.Value, count);
        return Ok(result);
    }

    /// <summary>
    /// En cok alinan urunler
    /// </summary>
    [HttpGet("buyer/top-products")]
    public async Task<IActionResult> GetTopPurchasedProducts([FromQuery] int count = 10)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var result = await _reportService.GetTopPurchasedProductsAsync(vendorId.Value, count);
        return Ok(result);
    }
}
