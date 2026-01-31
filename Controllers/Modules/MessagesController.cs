using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Mesajlasma
/// </summary>
public class MessagesController : DashboardBaseController
{
    public MessagesController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Mesajlar sayfasi
    /// </summary>
    public Task<IActionResult> Index() => ExecuteWithViewDataAsync("Messages");
}
