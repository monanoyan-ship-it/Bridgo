using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Authorization;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardApiController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardApiController(
        IDashboardService dashboardService,
        UserManager<ApplicationUser> userManager)
    {
        _dashboardService = dashboardService;
        _userManager = userManager;
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.VendorId;
    }

    // ============================================
    // FIRMA GENEL BILGILERI DASHBOARD
    // ============================================

    /// <summary>
    /// Firma genel bilgileri dashboard verisi
    /// Capability parametresi olmadan /Dashboard'a gelindiginde kullanilir
    /// </summary>
    [HttpGet("company")]
    public async Task<IActionResult> GetCompanyDashboard()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var data = await _dashboardService.GetCompanyDashboardAsync(vendorId.Value);
        return Ok(data);
    }

    // ============================================
    // SATICI DASHBOARD (ID: 2)
    // ============================================

    /// <summary>
    /// Satici dashboard verisi
    /// </summary>
    [HttpGet("seller")]
    [RequireCapability(VendorCapabilities.Seller)]
    public async Task<IActionResult> GetSellerDashboard()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var data = await _dashboardService.GetSellerDashboardAsync(vendorId.Value);
        return Ok(data);
    }

    // ============================================
    // ALICI DASHBOARD (ID: 3)
    // ============================================

    /// <summary>
    /// Alici dashboard verisi
    /// </summary>
    [HttpGet("buyer")]
    [RequireCapability(VendorCapabilities.Buyer)]
    public async Task<IActionResult> GetBuyerDashboard()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var data = await _dashboardService.GetBuyerDashboardAsync(vendorId.Value);
        return Ok(data);
    }

    // ============================================
    // TASIMACI DASHBOARD (ID: 4)
    // ============================================

    /// <summary>
    /// Tasimaci dashboard verisi
    /// </summary>
    [HttpGet("carrier")]
    [RequireCapability(VendorCapabilities.Carrier)]
    public async Task<IActionResult> GetCarrierDashboard()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var data = await _dashboardService.GetCarrierDashboardAsync(vendorId.Value);
        return Ok(data);
    }

    // ============================================
    // SIGORTA DASHBOARD (ID: 5)
    // ============================================

    /// <summary>
    /// Sigorta dashboard verisi
    /// </summary>
    [HttpGet("insurance")]
    [RequireCapability(VendorCapabilities.Insurance)]
    public async Task<IActionResult> GetInsuranceDashboard()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var data = await _dashboardService.GetInsuranceDashboardAsync(vendorId.Value);
        return Ok(data);
    }

    // ============================================
    // GUMRUK DASHBOARD (ID: 6)
    // ============================================

    /// <summary>
    /// Gumruk dashboard verisi
    /// </summary>
    [HttpGet("customs")]
    [RequireCapability(VendorCapabilities.Customs)]
    public async Task<IActionResult> GetCustomsDashboard()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var data = await _dashboardService.GetCustomsDashboardAsync(vendorId.Value);
        return Ok(data);
    }

    // ============================================
    // GOZETIM DASHBOARD (ID: 7)
    // ============================================

    /// <summary>
    /// Gozetim dashboard verisi
    /// </summary>
    [HttpGet("survey")]
    [RequireCapability(VendorCapabilities.Survey)]
    public async Task<IActionResult> GetSurveyDashboard()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var data = await _dashboardService.GetSurveyDashboardAsync(vendorId.Value);
        return Ok(data);
    }

    // ============================================
    // YATIRIMCI DASHBOARD (ID: 8)
    // ============================================

    /// <summary>
    /// Yatirimci dashboard verisi
    /// </summary>
    [HttpGet("investor")]
    [RequireCapability(VendorCapabilities.Investor)]
    public async Task<IActionResult> GetInvestorDashboard()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var data = await _dashboardService.GetInvestorDashboardAsync(vendorId.Value);
        return Ok(data);
    }
}
