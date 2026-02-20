using Microsoft.AspNetCore.Mvc;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Public sayfalar - Giris gerektirmez
/// Firma mini website: /{slug}
/// </summary>
public class PublicController : Controller
{
    private readonly ICapabilityProfileService _profileService;

    public PublicController(ICapabilityProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// Firma mini website sayfasi (root level)
    /// /{slug} route'u CompanySlugRouteConstraint ile korunur
    /// </summary>
    [Route("{slug:companySlug}")]
    public async Task<IActionResult> Profile(string slug)
    {
        // SEO meta tag'ler icin profil verisini cek
        var profile = await _profileService.GetBySlugAsync(slug, incrementViewCount: false);
        if (profile == null)
            return NotFound();

        ViewData["Slug"] = slug;
        ViewData["Title"] = profile.DisplayName ?? profile.Slug;
        ViewData["MetaDescription"] = profile.MetaDescription ?? profile.ShortDescription ?? profile.Description?.Substring(0, Math.Min(160, profile.Description?.Length ?? 0));

        // Open Graph tags
        ViewData["OgTitle"] = profile.DisplayName;
        ViewData["OgDescription"] = profile.ShortDescription ?? profile.MetaDescription;
        ViewData["OgImage"] = profile.CoverImageUrl;

        return View();
    }

}
