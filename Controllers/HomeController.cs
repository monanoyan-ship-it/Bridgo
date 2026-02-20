using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models;

namespace Bridgo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly bool _isLandingOnly;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _isLandingOnly = configuration.GetValue<bool>("LandingOnly");
    }

    public IActionResult Index()
    {
        // Landing-only modda (production deploy) sadece waitlist sayfasi
        if (_isLandingOnly)
            return View();

        // Normal mod: her zaman ana sayfa
        return View("Index2");
    }

    public IActionResult Index2()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
