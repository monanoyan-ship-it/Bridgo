using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Davet sayfalari MVC Controller
/// NOT: API islemleri TeamApiController'da
/// </summary>
public class InvitationsController : Controller
{
    private readonly ITeamService _teamService;

    public InvitationsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    /// <summary>
    /// Davet kabul sayfasi (public - davet linki ile gelenler icin)
    /// </summary>
    [AllowAnonymous]
    public async Task<IActionResult> Accept(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Index", "Home");
        }

        var validation = await _teamService.ValidateInvitationTokenAsync(token);
        if (!validation.IsValid)
        {
            ViewBag.Error = validation.Message;
            ViewBag.IsValid = false;
            return View();
        }

        ViewBag.Token = token;
        ViewBag.IsValid = true;
        ViewBag.Member = validation.Member;
        return View();
    }
}
