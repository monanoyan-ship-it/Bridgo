using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.DTOs.Account;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[Route("api/account")]
[ApiController]
[Authorize]
public class AccountApiController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountApiController(
        IAccountService accountService,
        UserManager<ApplicationUser> userManager)
    {
        _accountService = accountService;
        _userManager = userManager;
    }

    // ============================================
    // PUBLIC ENDPOINTS (No Auth Required)
    // ============================================

    /// <summary>
    /// E-posta adresinin kayitli olup olmadigini kontrol et
    /// </summary>
    [HttpGet("check-email")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Ok(new { exists = false });

        var user = await _userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        return Ok(new { exists = user != null });
    }

    // ============================================
    // PERSONAL INFO
    // ============================================

    /// <summary>
    /// Kisisel bilgileri getir
    /// </summary>
    [HttpGet("personal-info")]
    public async Task<ActionResult<PersonalInfoDto>> GetPersonalInfo()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var info = await _accountService.GetPersonalInfoAsync(userId);
        if (info == null) return NotFound();

        return Ok(info);
    }

    /// <summary>
    /// Kisisel bilgileri guncelle
    /// </summary>
    [HttpPut("personal-info")]
    public async Task<IActionResult> UpdatePersonalInfo([FromBody] PersonalInfoUpdateDto dto)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var (success, error) = await _accountService.UpdatePersonalInfoAsync(userId, dto);

        if (!success)
            return BadRequest(new { message = error });

        // Dil tercihini cookie'ye yaz
        if (dto.LanguageId.HasValue)
        {
            Response.Cookies.Append("LanguageId", dto.LanguageId.Value.ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });
        }

        return Ok(new { message = "Bilgiler guncellendi" });
    }

    /// <summary>
    /// Profil resmini guncelle
    /// </summary>
    [HttpPut("profile-image")]
    public async Task<IActionResult> UpdateProfileImage([FromBody] ProfileImageUpdateDto dto)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var (success, error) = await _accountService.UpdateProfileImageAsync(userId, dto.ProfileImageUrl ?? "");

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "Profil resmi guncellendi" });
    }

    /// <summary>
    /// Profil resmini sil
    /// </summary>
    [HttpDelete("profile-image")]
    public async Task<IActionResult> DeleteProfileImage()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var (success, error) = await _accountService.DeleteProfileImageAsync(userId);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "Profil resmi silindi" });
    }

    // ============================================
    // PASSWORD & SECURITY
    // ============================================

    /// <summary>
    /// Guvenlik bilgilerini getir
    /// </summary>
    [HttpGet("security")]
    public async Task<ActionResult<SecurityInfoDto>> GetSecurityInfo()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var info = await _accountService.GetSecurityInfoAsync(userId);
        return Ok(info);
    }

    /// <summary>
    /// Sifre degistir
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var (success, error) = await _accountService.ChangePasswordAsync(userId, dto);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "Sifre basariyla degistirildi" });
    }

    // ============================================
    // NOTIFICATION SETTINGS
    // ============================================

    /// <summary>
    /// Bildirim tercihlerini getir
    /// </summary>
    [HttpGet("notifications")]
    public async Task<ActionResult<NotificationSettingsDto>> GetNotificationSettings()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var settings = await _accountService.GetNotificationSettingsAsync(userId);
        return Ok(settings);
    }

    /// <summary>
    /// Bildirim tercihlerini guncelle
    /// </summary>
    [HttpPut("notifications")]
    public async Task<IActionResult> UpdateNotificationSettings([FromBody] NotificationSettingsUpdateDto dto)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var (success, error) = await _accountService.UpdateNotificationSettingsAsync(userId, dto);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "Bildirim tercihleri guncellendi" });
    }

    // ============================================
    // LANGUAGES
    // ============================================

    /// <summary>
    /// Aktif dilleri getir
    /// </summary>
    [HttpGet("languages")]
    public async Task<ActionResult<List<LanguageOptionDto>>> GetLanguages()
    {
        var languages = await _accountService.GetLanguagesAsync();
        return Ok(languages);
    }

    /// <summary>
    /// Kullanici dil tercihini guncelle
    /// </summary>
    [HttpPut("language")]
    public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguagePreferenceDto dto)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        user.LanguageId = dto.LanguageId;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(new { message = "Dil tercihi guncellenemedi" });

        // Cookie'yi de guncelle
        Response.Cookies.Append("LanguageId", dto.LanguageId.ToString(), new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax
        });

        return Ok(new { message = "Dil tercihi guncellendi" });
    }

    // ============================================
    // VERIFICATIONS
    // ============================================

    /// <summary>
    /// Dogrulama durumlarini getir
    /// </summary>
    [HttpGet("verifications")]
    public async Task<ActionResult<VerificationStatusDto>> GetVerifications()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var status = await _accountService.GetVerificationStatusAsync(userId);
        return Ok(status);
    }

    /// <summary>
    /// E-posta dogrulama gonder
    /// </summary>
    [HttpPost("send-email-verification")]
    public async Task<IActionResult> SendEmailVerification()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var (success, error) = await _accountService.SendEmailVerificationAsync(userId);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "Dogrulama e-postasi gonderildi" });
    }

    // ============================================
    // HELPERS
    // ============================================

    private int GetUserId()
    {
        var userIdStr = _userManager.GetUserId(User);
        return int.TryParse(userIdStr, out var userId) ? userId : 0;
    }
}
