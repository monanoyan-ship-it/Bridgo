using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Finansman Talepleri (Seller/Buyer icin)
/// </summary>
public class FinancingController : DashboardBaseController
{
    public FinancingController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Finansman taleplerim
    /// </summary>
    public Task<IActionResult> MyRequests() => ExecuteWithViewDataAsync();
}
