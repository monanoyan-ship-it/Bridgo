using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Teklif yonetimi
/// </summary>
public class ProposalsController : DashboardBaseController
{
    public ProposalsController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Teklifler listesi
    /// </summary>
    public Task<IActionResult> Index() => ExecuteWithViewDataAsync("Proposals");

    /// <summary>
    /// Teklif karsilastirma
    /// </summary>
    public Task<IActionResult> Compare() => ExecuteWithViewDataAsync("CompareProposals");
}
