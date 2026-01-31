using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Ekip yonetimi MVC Controller
/// NOT: API islemleri TeamApiController'da
/// </summary>
public class TeamController : DashboardBaseController
{
    public TeamController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Ekip yonetim sayfasi
    /// Sadece Owner ve Admin erisebilir
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
