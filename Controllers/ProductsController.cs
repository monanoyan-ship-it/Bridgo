using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Urunler MVC Controller
/// NOT: API islemleri ProductsApiController'da
/// </summary>
public class ProductsController : DashboardBaseController
{
    public ProductsController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Urunler sayfasi
    /// </summary>
    public Task<IActionResult> Index() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Kategoriler sayfasi
    /// </summary>
    public Task<IActionResult> Categories() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Stok Yonetimi sayfasi
    /// </summary>
    public Task<IActionResult> Stock() => ExecuteWithViewDataAsync();
}
