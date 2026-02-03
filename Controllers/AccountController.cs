using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.DTOs.Auth;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IRegistrationService _registrationService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IRegistrationService registrationService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _registrationService = registrationService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([FromBody] MultiStepRegisterDto model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Json(new { success = false, message = "Zaten giris yapilmis." });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.FirstOrDefault()?.ErrorMessage ?? ""
            ).Where(x => !string.IsNullOrEmpty(x.Value));

            return Json(new { success = false, message = "Lutfen tum alanlari doldurun.", errors });
        }

        // E-posta zaten kayitli mi?
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return Json(new {
                success = false,
                message = "Bu e-posta adresi zaten kayitli.",
                errors = new { email = "Bu e-posta adresi zaten kayitli." }
            });
        }

        // Kullanici olustur
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var passwordErrorCodes = new[] {
                "PasswordTooShort", "PasswordRequiresLower", "PasswordRequiresUpper",
                "PasswordRequiresDigit", "PasswordRequiresNonAlphanumeric", "PasswordRequiresUniqueChars"
            };
            var isPasswordError = result.Errors.Any(e => passwordErrorCodes.Contains(e.Code));
            var errorMessages = string.Join(" ", result.Errors.Select(e => e.Description));

            if (isPasswordError)
            {
                return Json(new {
                    success = false,
                    message = errorMessages,
                    errors = new Dictionary<string, string> { { "Password", errorMessages } }
                });
            }

            return Json(new { success = false, message = errorMessages });
        }

        // User rolunu ata (tum kayitli kullanicilar icin)
        await _userManager.AddToRoleAsync(user, "User");

        // Vendor ve iliskili verileri olustur (Service ile)
        var registrationResult = await _registrationService.RegisterVendorAsync(model, user.Id);
        if (!registrationResult.Success)
        {
            // Kullaniciyi sil (rollback)
            await _userManager.DeleteAsync(user);
            return Json(new { success = false, message = registrationResult.Message });
        }

        // Otomatik giris yap
        await _signInManager.SignInAsync(user, isPersistent: false);

        // Default dil cookie'sini ayarla
        SetLanguageCookie(null);

        return Json(new {
            success = true,
            redirectUrl = Url.Action("Index", "Dashboard")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Gecersiz giris denemesi.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Kullanicinin dil tercihini cookie'ye yaz
            SetLanguageCookie(user.LanguageId);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // VendorId yoksa VendorSetup'a yonlendir
            if (user.VendorId == null)
            {
                return RedirectToAction("Index", "VendorSetup");
            }

            return RedirectToAction("Index", "Dashboard");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Hesabiniz gecici olarak kilitlendi. Lutfen daha sonra tekrar deneyin.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Gecersiz giris denemesi.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lutfen tum alanlari doldurun." });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "Kullanici bulunamadi." });
        }

        // Mevcut sifreyi dogrula
        var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
        if (!isCurrentPasswordValid)
        {
            return Json(new { success = false, message = "Mevcut sifre yanlis." });
        }

        // Yeni sifrelerin eslestigini kontrol et
        if (model.NewPassword != model.ConfirmPassword)
        {
            return Json(new { success = false, message = "Yeni sifreler eslesmedi." });
        }

        // Sifreyi degistir
        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Json(new { success = false, message = errors });
        }

        // Yeni sifre ile oturumu yenile
        await _signInManager.RefreshSignInAsync(user);

        return Json(new { success = true, message = "Sifreniz basariyla degistirildi." });
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>
    /// Kullanicinin dil tercihini cookie'ye yazar
    /// Localization service bu cookie'yi okur
    /// </summary>
    private void SetLanguageCookie(int? languageId)
    {
        if (languageId.HasValue)
        {
            Response.Cookies.Append("LanguageId", languageId.Value.ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });
        }
        else
        {
            // Default dili kullan (Turkce - ID: 1)
            Response.Cookies.Append("LanguageId", "1", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });
        }
    }

    #region Public API for Registration

    /// <summary>
    /// Email domain'ine gore firma eslestirme
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> CheckDomainMatch(string email)
    {
        var vendor = await _registrationService.CheckDomainMatchAsync(email);

        if (vendor != null)
        {
            return Json(new
            {
                found = true,
                vendor = new
                {
                    vendor.Id,
                    vendor.CompanyName,
                    city = vendor.City
                }
            });
        }

        return Json(new { found = false });
    }

    /// <summary>
    /// Kayit sirasinda firma arama (public - auth gerektirmez)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchCompanies(string query)
    {
        var results = await _registrationService.SearchCompaniesAsync(query);

        return Json(new
        {
            results = results.Select(v => new
            {
                id = v.Id,
                companyName = v.CompanyName,
                city = v.City
            })
        });
    }

    /// <summary>
    /// Kayit sirasinda firmaya katilma istegi olustur
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterWithJoinRequest([FromBody] RegisterWithJoinDto model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Gecersiz veri" });
        }

        // Email kullaniliyor mu?
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return Json(new { success = false, message = "Bu e-posta adresi zaten kullaniliyor" });
        }

        // Kullanici olustur (VendorId olmadan)
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            VendorId = null, // Onay bekliyor
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var passwordErrorCodes = new[] {
                "PasswordTooShort", "PasswordRequiresLower", "PasswordRequiresUpper",
                "PasswordRequiresDigit", "PasswordRequiresNonAlphanumeric", "PasswordRequiresUniqueChars"
            };
            var isPasswordError = result.Errors.Any(e => passwordErrorCodes.Contains(e.Code));
            var errorMessages = string.Join(" ", result.Errors.Select(e => e.Description));

            if (isPasswordError)
            {
                return Json(new {
                    success = false,
                    message = errorMessages,
                    errors = new Dictionary<string, string> { { "Password", errorMessages } }
                });
            }

            return Json(new { success = false, message = errorMessages });
        }

        // User rolunu ata (tum kayitli kullanicilar icin)
        await _userManager.AddToRoleAsync(user, "User");

        // Join request olustur (Service ile)
        var registrationResult = await _registrationService.RegisterWithJoinRequestAsync(model, user.Id);
        if (!registrationResult.Success)
        {
            await _userManager.DeleteAsync(user);
            return Json(new { success = false, message = registrationResult.Message });
        }

        // Kullaniciyi giris yaptir
        await _signInManager.SignInAsync(user, isPersistent: false);

        return Json(new
        {
            success = true,
            message = registrationResult.Message,
            redirectUrl = Url.Action("Index", "VendorSetup")
        });
    }

    #endregion
}
