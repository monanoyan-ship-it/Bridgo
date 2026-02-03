using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Urun degerlendirmeleri - Buyer yapar, Seller gorur ve cevaplar
/// </summary>
public class ProductReviewsController : DashboardBaseController
{
    public ProductReviewsController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Urun degerlendirmeleri sayfasi
    /// </summary>
    public Task<IActionResult> Index() => ExecuteWithViewDataAsync();
}
