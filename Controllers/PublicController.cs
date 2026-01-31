using Microsoft.AspNetCore.Mvc;

namespace Bridgo.Controllers;

/// <summary>
/// Public sayfalar - Giris gerektirmez
/// </summary>
public class PublicController : Controller
{
    /// <summary>
    /// Public profil sayfasi
    /// slug ile profil goruntuleme
    /// </summary>
    [Route("p/{slug}")]
    public IActionResult Profile(string slug)
    {
        ViewData["Slug"] = slug;
        return View();
    }
}
