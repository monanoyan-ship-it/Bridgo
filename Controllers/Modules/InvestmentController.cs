using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Yatirim ve Finansman
/// </summary>
public class InvestmentController : DashboardBaseController
{
    public InvestmentController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Yatirim firsatlari
    /// </summary>
    public Task<IActionResult> Opportunities() => ExecuteWithViewDataAsync("InvestmentOpportunities");

    /// <summary>
    /// Yatirimlarim (kabul edilen teklifler)
    /// </summary>
    public Task<IActionResult> MyInvestments() => ExecuteWithViewDataAsync();
}
