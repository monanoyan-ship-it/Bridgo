using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridgo.Controllers;

/// <summary>
/// VendorSetup MVC Controller - Sayfa gosterimi
/// </summary>
[Authorize]
public class VendorSetupController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
