using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Tedarikci yonetimi - Profil, kesif, favoriler
/// </summary>
public class SuppliersController : DashboardBaseController
{
    public SuppliersController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Tedarikci profili (Seller gorunumu)
    /// </summary>
    public Task<IActionResult> SupplierProfile() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Tedarikci kesfet (Buyer gorunumu)
    /// </summary>
    public Task<IActionResult> DiscoverSuppliers() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Favori tedarikciler (Buyer gorunumu)
    /// </summary>
    public Task<IActionResult> FavoriteSuppliers() => ExecuteWithViewDataAsync();
}
