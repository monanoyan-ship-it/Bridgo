using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Talep yonetimi - Alici talepleri ve satici teklifleri
/// </summary>
[Route("Demands")]
public class DemandsDashboardController : DashboardBaseController
{
    public DemandsDashboardController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Alici talepleri
    /// </summary>
    [Route("MyDemands")]
    public Task<IActionResult> MyDemands() => ExecuteWithViewDataAsync("~/Views/Demands/MyDemands.cshtml");

    /// <summary>
    /// Satici teklifleri - Gelen talepler ve firsatlar
    /// </summary>
    [Route("SupplierOffers")]
    public Task<IActionResult> SupplierOffers() => ExecuteWithViewDataAsync("~/Views/Demands/SupplierOffers.cshtml");
}
