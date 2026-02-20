using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

public class AppsController : DashboardBaseController
{
    public AppsController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    public Task<IActionResult> Index() => ExecuteWithViewDataAsync();

    public Task<IActionResult> Logistics() => ExecuteWithViewDataAsync();

    public Task<IActionResult> Customs() => ExecuteWithViewDataAsync();

    public Task<IActionResult> Finance() => ExecuteWithViewDataAsync();

    public Task<IActionResult> SocialMedia() => ExecuteWithViewDataAsync();

    public Task<IActionResult> Marketplace() => ExecuteWithViewDataAsync();
}
