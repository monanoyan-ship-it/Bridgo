using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Modules;

/// <summary>
/// Dis hizmetler - Lojistik, Gumruk, Sigorta, Ekspertiz
/// </summary>
public class ServicesController : DashboardBaseController
{
    public ServicesController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    /// <summary>
    /// Lojistik talepleri
    /// </summary>
    public Task<IActionResult> Logistics() => ExecuteWithViewDataAsync("LogisticsRequests");

    /// <summary>
    /// Gumruk talepleri
    /// </summary>
    public Task<IActionResult> Customs() => ExecuteWithViewDataAsync("CustomsRequests");

    /// <summary>
    /// Sigorta talepleri
    /// </summary>
    public Task<IActionResult> Insurance() => ExecuteWithViewDataAsync("InsuranceRequests");

    /// <summary>
    /// Ekspertiz talepleri
    /// </summary>
    public Task<IActionResult> Survey() => ExecuteWithViewDataAsync("SurveyRequests");

    // ============================================================
    // AKTIF ISLER - Kabul edilen, devam eden isler
    // ============================================================

    /// <summary>
    /// Lojistik islerim (aktif + tamamlanan)
    /// </summary>
    public Task<IActionResult> MyLogisticsJobs() => ExecuteWithViewDataAsync("MyLogisticsJobs");

    /// <summary>
    /// Gumruk islerim (aktif + tamamlanan + Evrim entegrasyonu)
    /// </summary>
    public Task<IActionResult> MyCustomsJobs() => ExecuteWithViewDataAsync("MyCustomsJobs");

    /// <summary>
    /// Sigorta islerim (aktif + tamamlanan)
    /// </summary>
    public Task<IActionResult> MyInsuranceJobs() => ExecuteWithViewDataAsync("MyInsuranceJobs");

    /// <summary>
    /// Ekspertiz islerim (aktif + tamamlanan)
    /// </summary>
    public Task<IActionResult> MySurveyJobs() => ExecuteWithViewDataAsync("MySurveyJobs");
}
