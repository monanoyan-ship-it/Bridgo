using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Raporlama
/// </summary>
public class ReportsController : DashboardBaseController
{
    public ReportsController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Raporlar sayfasi
    /// </summary>
    public Task<IActionResult> Index() => ExecuteWithViewDataAsync("Reports");
}
