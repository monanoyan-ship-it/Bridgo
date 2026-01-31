using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Depolar MVC Controller
/// NOT: API islemleri WarehousesApiController'da
/// </summary>
public class WarehousesController : DashboardBaseController
{
    public WarehousesController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Depolar sayfasi
    /// Manager ve ustu erisebilir
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var user = await GetCurrentUserAsync();
        if (user?.VendorId == null)
            return RedirectToAction("Index", "VendorSetup");

        if (!await LoadViewDataAsync(user))
            return RedirectToAction("Index", "VendorSetup");

        return View();
    }
}
