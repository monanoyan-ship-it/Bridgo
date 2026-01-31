using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Public Suppliers sayfalari (SEO-optimized, login gerektirmez)
/// </summary>
public class SuppliersController : Controller
{
    private readonly ISupplierProfileService _supplierService;

    public SuppliersController(ISupplierProfileService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// Tedarikci listesi - Public, SEO
    /// /Suppliers
    /// </summary>
    [AllowAnonymous]
    public IActionResult Index()
    {
        ViewData["Title"] = "Tedarikciler | Bridgo B2B";
        ViewData["MetaDescription"] = "B2B tedarikciler ve ureticiler. Guvenilir is ortaklari bulun, teklif isteyin.";
        return View();
    }

    /// <summary>
    /// Tedarikci profil detay - Public, SEO
    /// /Suppliers/{slug}
    /// </summary>
    [AllowAnonymous]
    [Route("Suppliers/{slug}")]
    public async Task<IActionResult> Profile(string slug)
    {
        var profile = await _supplierService.GetProfileBySlugAsync(slug, incrementViewCount: true);

        if (profile == null)
            return NotFound();

        ViewData["Title"] = $"{profile.DisplayName ?? profile.CompanyName} | Bridgo B2B";
        ViewData["MetaDescription"] = profile.MetaDescription ?? profile.ShortDescription;

        return View(profile);
    }
}
