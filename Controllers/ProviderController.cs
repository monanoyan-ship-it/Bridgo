using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Servis saglayici (Tasimaci, Gumruk, Sigorta, Gozetim) sayfalari
/// </summary>
[Authorize]
public class ProviderController : DashboardBaseController
{
    public ProviderController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Gelen servis talepleri
    /// </summary>
    public async Task<IActionResult> Requests(int? serviceType = null)
    {
        ViewBag.ServiceType = serviceType;
        return await ExecuteWithViewDataAsync();
    }

    /// <summary>
    /// Verdigi teklifler
    /// </summary>
    public async Task<IActionResult> MyQuotes()
    {
        return await ExecuteWithViewDataAsync();
    }

    /// <summary>
    /// Servis saglayici profili (Capability Profile)
    /// </summary>
    public async Task<IActionResult> MyProfile()
    {
        return await ExecuteWithViewDataAsync();
    }
}
