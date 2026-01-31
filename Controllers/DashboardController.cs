using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Dashboard ana sayfa.
/// Diger modul sayfalari Controllers/Modules altinda.
/// </summary>
public class DashboardController : DashboardBaseController
{
    public DashboardController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Genel Bakis - Dashboard ana sayfa
    /// </summary>
    public Task<IActionResult> Index() => ExecuteWithViewDataAsync();
}
