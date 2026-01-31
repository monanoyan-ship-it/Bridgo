using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;
using Bridgo.Helpers;

namespace Bridgo.Controllers;

/// <summary>
/// Dashboard layout kullanan tum controller'lar icin base class.
/// Ortak ViewBag verilerini yukler.
/// </summary>
[Authorize]
public abstract class DashboardBaseController : Controller
{
    protected readonly IVendorService _vendorService;
    protected readonly ICompanyService _companyService;
    protected readonly UserManager<ApplicationUser> _userManager;

    protected DashboardBaseController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
    {
        _vendorService = vendorService;
        _companyService = companyService;
        _userManager = userManager;
    }

    protected int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    protected async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = GetUserId();
        return userId == 0 ? null : await _userManager.FindByIdAsync(userId.ToString());
    }

    /// <summary>
    /// Dashboard layout icin gerekli ViewBag verilerini yukler.
    /// Tum capability'ler ve modulleri yukler.
    /// </summary>
    protected async Task<bool> LoadViewDataAsync(ApplicationUser user, int? activeCapabilityId = null)
    {
        if (user.VendorId == null)
            return false;

        var vendor = await _vendorService.GetByIdAsync(user.VendorId.Value);
        var capabilities = await _companyService.GetVendorCapabilitiesAsync(user.VendorId.Value);
        var userRoles = await _companyService.GetUserRolesAsync(user.Id, user.VendorId.Value);

        // Aktif capability belirle (parametre, query string veya ilk capability)
        var capabilityIdStr = Request.Query["capability"].FirstOrDefault();
        int? selectedCapabilityId = activeCapabilityId;

        if (!selectedCapabilityId.HasValue && int.TryParse(capabilityIdStr, out var parsedId))
            selectedCapabilityId = parsedId;

        if (!selectedCapabilityId.HasValue && capabilities.Any())
            selectedCapabilityId = capabilities.First().Id;

        // Platform modulleri (tum capability'ler icin ortak)
        List<CapabilityModuleDto> platformModules = new List<CapabilityModuleDto>();
        var platformCapability = await _companyService.GetPlatformCapabilityAsync();
        if (platformCapability != null)
        {
            platformModules = await _companyService.GetUserAccessibleModulesAsync(
                user.Id, user.VendorId.Value, platformCapability.Id);
        }

        // Capability bazli moduller - her capability icin ayri
        var modulesByCapability = new Dictionary<int, List<CapabilityModuleDto>>();
        foreach (var cap in capabilities)
        {
            var capModules = await _companyService.GetUserAccessibleModulesAsync(
                user.Id, user.VendorId.Value, cap.Id);
            if (capModules.Any())
            {
                modulesByCapability[cap.Id] = capModules;
            }
        }

        // Geriye donuk uyumluluk icin eski Modules da doldur
        List<CapabilityModuleDto> modules = new List<CapabilityModuleDto>(platformModules);
        if (selectedCapabilityId.HasValue && modulesByCapability.ContainsKey(selectedCapabilityId.Value))
        {
            ModuleHelper.MergeModules(modules, modulesByCapability[selectedCapabilityId.Value]);
        }

        ViewBag.Vendor = vendor;
        ViewBag.User = user;
        ViewBag.Capabilities = capabilities;
        ViewBag.ActiveCapabilityId = selectedCapabilityId;
        ViewBag.Modules = modules;
        ViewBag.PlatformModules = platformModules;
        ViewBag.ModulesByCapability = modulesByCapability;
        ViewBag.UserRoles = userRoles;

        return true;
    }

    /// <summary>
    /// Kullanici ve vendor kontrolu yaparak view dondurur.
    /// </summary>
    protected async Task<IActionResult> ExecuteWithViewDataAsync(string? viewName = null)
    {
        var user = await GetCurrentUserAsync();
        if (user?.VendorId == null)
            return RedirectToAction("Index", "VendorSetup");

        if (!await LoadViewDataAsync(user))
            return RedirectToAction("Index", "VendorSetup");

        return viewName != null ? View(viewName) : View();
    }
}
