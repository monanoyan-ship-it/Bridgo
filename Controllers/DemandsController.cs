using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Public Demands sayfalari (SEO-optimized, login gerektirmez)
/// </summary>
public class DemandsController : Controller
{
    private readonly IDemandService _demandService;

    public DemandsController(IDemandService demandService)
    {
        _demandService = demandService;
    }

    /// <summary>
    /// Talep listesi - Public, SEO
    /// /Demands
    /// </summary>
    [AllowAnonymous]
    public IActionResult Index()
    {
        ViewData["Title"] = "Talepler | Bridgo B2B";
        ViewData["MetaDescription"] = "B2B alici talepleri. Urun ve hizmet arayislari. Teklif verin, is firsatlarini yakalayin.";
        return View();
    }

    /// <summary>
    /// Talep detay - Public, SEO
    /// /Demands/{slug}
    /// </summary>
    [AllowAnonymous]
    [Route("Demands/{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var demand = await _demandService.GetDemandBySlugAsync(slug, incrementViewCount: true);

        if (demand == null)
            return NotFound();

        ViewData["Title"] = $"{demand.Title} | Bridgo B2B";
        ViewData["MetaDescription"] = demand.MetaDescription ?? demand.Description?.Substring(0, Math.Min(demand.Description.Length, 160));
        ViewData["Demand"] = demand;

        return View(demand);
    }
}
