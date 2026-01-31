using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Sepet sayfasi controller
/// </summary>
public class CartController : DashboardBaseController
{
    public CartController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Sepet sayfasi - tum sepetleri goster
    /// </summary>
    public async Task<IActionResult> Index()
    {
        return await ExecuteWithViewDataAsync();
    }
}
