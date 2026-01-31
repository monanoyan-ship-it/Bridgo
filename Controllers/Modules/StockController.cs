using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Stok yonetimi
/// </summary>
public class StockController : DashboardBaseController
{
    public StockController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Stok hareketleri
    /// </summary>
    public Task<IActionResult> Movements() => ExecuteWithViewDataAsync("StockMovements");
}
