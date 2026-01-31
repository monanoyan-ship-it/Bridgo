using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Company Profile ve Addresses Controller
/// </summary>
public class CompanyController : DashboardBaseController
{
    public CompanyController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Index - Dashboard'a yonlendir
    /// </summary>
    public IActionResult Index() => RedirectToAction("Index", "Dashboard");

    /// <summary>
    /// Firma profili
    /// </summary>
    public async Task<IActionResult> Profile()
    {
        var user = await GetCurrentUserAsync();
        if (user?.VendorId == null)
            return RedirectToAction("Index", "VendorSetup");

        if (!await LoadViewDataAsync(user))
            return RedirectToAction("Index", "VendorSetup");

        return View();
    }

    /// <summary>
    /// Adres yonetimi
    /// </summary>
    public async Task<IActionResult> Addresses()
    {
        var user = await GetCurrentUserAsync();
        if (user?.VendorId == null)
            return RedirectToAction("Index", "VendorSetup");

        if (!await LoadViewDataAsync(user))
            return RedirectToAction("Index", "VendorSetup");

        return View();
    }

    /// <summary>
    /// Ayarlar
    /// </summary>
    public async Task<IActionResult> Settings()
    {
        var user = await GetCurrentUserAsync();
        if (user?.VendorId == null)
            return RedirectToAction("Index", "VendorSetup");

        if (!await LoadViewDataAsync(user))
            return RedirectToAction("Index", "VendorSetup");

        return View();
    }
}
