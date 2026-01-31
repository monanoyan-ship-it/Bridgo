using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Tedarikci yonetimi - Profil ve tedarikci iliskileri
/// Route: /SupplierManagement/*
/// </summary>
[Route("[controller]/[action]")]
public class SupplierManagementController : DashboardBaseController
{
    public SupplierManagementController(
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
    /// Tedarikçilerim - Alım yapılan tedarikçiler (Buyer gorunumu)
    /// </summary>
    public Task<IActionResult> FavoriteSuppliers() => ExecuteWithViewDataAsync();
}
