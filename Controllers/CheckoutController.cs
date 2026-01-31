using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Checkout sayfasi controller
/// </summary>
[Authorize]
public class CheckoutController : DashboardBaseController
{
    public CheckoutController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Checkout ana sayfasi
    /// Sepet ozeti, adres secimi ve hizmet secenekleri
    /// </summary>
    public async Task<IActionResult> Index()
    {
        return await ExecuteWithViewDataAsync();
    }
}
