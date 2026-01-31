using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Hesap ayarlari
/// </summary>
public class SettingsController : DashboardBaseController
{
    public SettingsController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Kisisel bilgiler
    /// </summary>
    public Task<IActionResult> PersonalInfo() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Belgeler
    /// </summary>
    public Task<IActionResult> Documents() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Sozlesmeler
    /// </summary>
    public Task<IActionResult> Contracts() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Dogrulamalar
    /// </summary>
    public Task<IActionResult> Verifications() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Fatura bilgileri
    /// </summary>
    public Task<IActionResult> Billing() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Servis baglantilari
    /// </summary>
    public Task<IActionResult> ServiceConnections() => ExecuteWithViewDataAsync();

    /// <summary>
    /// Bildirim ayarlari
    /// </summary>
    public Task<IActionResult> Notifications() => ExecuteWithViewDataAsync("NotificationSettings");

    /// <summary>
    /// Abonelik ayarlari
    /// </summary>
    public Task<IActionResult> Subscription() => ExecuteWithViewDataAsync("SubscriptionSettings");

    /// <summary>
    /// Guvenlik ayarlari
    /// </summary>
    public Task<IActionResult> Security() => ExecuteWithViewDataAsync("SecuritySettings");
}
