using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Siparis yonetimi - Alici ve Satici siparisleri
/// </summary>
public class OrdersController : DashboardBaseController
{
    public OrdersController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Alici siparisleri (MyOrders)
    /// </summary>
    public Task<IActionResult> MyOrders() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Satici siparisleri (SellerOrders)
    /// </summary>
    public Task<IActionResult> SellerOrders() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Siparis timeline/detay sayfasi
    /// </summary>
    public async Task<IActionResult> Timeline(int id)
    {
        ViewBag.Id = id;
        return await ExecuteWithViewDataAsync();
    }
}
