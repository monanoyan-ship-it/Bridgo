using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Talep yonetimi - Alici talepleri ve satici teklifleri
/// </summary>
public class DemandsController : DashboardBaseController
{
    public DemandsController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Alici talepleri
    /// </summary>
    public Task<IActionResult> MyDemands() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Satici teklifleri - Gelen talepler ve firsatlar
    /// </summary>
    public Task<IActionResult> SupplierOffers() => ExecuteWithViewDataAsync();
}
