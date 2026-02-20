using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Bridgo.Data;
using Bridgo.DTOs.CapabilityProfile;
using Bridgo.Models.Enums;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Capability Profile API - Her capability icin profil yonetimi
/// Tek tablo, VendorId + CapabilityId unique
/// </summary>
[ApiController]
[Route("api/capability-profile")]
[Authorize]
public class CapabilityProfileApiController : ControllerBase
{
    private readonly ICapabilityProfileService _profileService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public CapabilityProfileApiController(
        ICapabilityProfileService profileService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _profileService = profileService;
        _userManager = userManager;
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = GetUserId();
        return userId == 0 ? null : await _userManager.FindByIdAsync(userId.ToString());
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.VendorId;
    }

    /// <summary>
    /// Vendor'un sahip oldugu capability'leri getir
    /// </summary>
    [HttpGet("my-capabilities")]
    public async Task<IActionResult> GetMyCapabilities()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var capabilityIds = await _context.VendorCapabilityMappings
            .Where(m => m.VendorId == vendorId.Value && m.IsActive)
            .Select(m => m.CapabilityId)
            .ToListAsync();

        var capabilities = capabilityIds
            .Select(capId => Capabilities.GetById(capId))
            .Where(c => c != null)
            .Select(c => new
            {
                id = c!.Id,
                name = c.Description, // Turkce varsayilan
                nameResourceKey = c.NameResourceKey,
                icon = c.Icon
            })
            .ToList();

        return Ok(capabilities);
    }

    /// <summary>
    /// Belirli bir capability icin profil getir (yoksa olustur)
    /// </summary>
    [HttpGet("my-profile/{capabilityId:int}")]
    public async Task<IActionResult> GetMyProfile(int capabilityId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        // Vendor'un bu capability'si var mi?
        var hasCapability = await _context.VendorCapabilityMappings
            .AnyAsync(m => m.VendorId == vendorId.Value && m.CapabilityId == capabilityId);

        if (!hasCapability)
        {
            return Forbid();
        }

        var profile = await _profileService.GetOrCreateProfileAsync(
            vendorId.Value,
            capabilityId,
            GetUserId().ToString());

        return Ok(profile);
    }

    /// <summary>
    /// Profil olustur veya guncelle
    /// </summary>
    [HttpPost("my-profile/{capabilityId:int}")]
    public async Task<IActionResult> CreateOrUpdateProfile(int capabilityId, [FromBody] CapabilityProfileCreateUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        // Vendor'un bu capability'si var mi?
        var hasCapability = await _context.VendorCapabilityMappings
            .AnyAsync(m => m.VendorId == vendorId.Value && m.CapabilityId == capabilityId);

        if (!hasCapability)
        {
            return Forbid();
        }

        var profile = await _profileService.CreateOrUpdateProfileAsync(
            vendorId.Value,
            capabilityId,
            dto,
            GetUserId().ToString());

        return Ok(profile);
    }

    /// <summary>
    /// Profil guncelle (PUT alias)
    /// </summary>
    [HttpPut("my-profile/{capabilityId:int}")]
    public async Task<IActionResult> UpdateProfile(int capabilityId, [FromBody] CapabilityProfileCreateUpdateDto dto)
    {
        return await CreateOrUpdateProfile(capabilityId, dto);
    }

    /// <summary>
    /// Profil gorunurlugunu degistir
    /// </summary>
    [HttpPost("my-profile/{capabilityId:int}/visibility")]
    public async Task<IActionResult> SetVisibility(int capabilityId, [FromBody] SetProfileVisibilityDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _profileService.SetVisibilityAsync(
            vendorId.Value,
            capabilityId,
            dto.IsPubliclyVisible,
            GetUserId().ToString());

        if (!success)
        {
            return NotFound(new { message = "Profil bulunamadi." });
        }

        return Ok(new { message = dto.IsPubliclyVisible ? "Profil yayinlandi." : "Profil gizlendi." });
    }

    /// <summary>
    /// Profil istatistikleri
    /// </summary>
    [HttpGet("my-profile/{capabilityId:int}/stats")]
    public async Task<IActionResult> GetStats(int capabilityId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var stats = await _profileService.GetStatsAsync(vendorId.Value, capabilityId);
        return Ok(stats);
    }

    /// <summary>
    /// Vendor'un tum profillerini getir
    /// </summary>
    [HttpGet("my-profiles")]
    public async Task<IActionResult> GetMyProfiles()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var profiles = await _profileService.GetVendorProfilesAsync(vendorId.Value);
        return Ok(profiles);
    }

    /// <summary>
    /// Galeri gorseli yukle
    /// </summary>
    [HttpPost("my-profile/{capabilityId:int}/gallery")]
    public async Task<IActionResult> UploadGalleryImage(int capabilityId, IFormFile file)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya secilmedi" });

        // Dosya tipi kontrolu
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Gecersiz dosya tipi." });

        // Dosya boyutu kontrolu (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Dosya boyutu 5MB'dan buyuk olamaz" });

        try
        {
            var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var uploadsFolder = Path.Combine(env.WebRootPath, "uploads", "profiles", vendorId.Value.ToString());
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var uniqueFileName = $"{Guid.NewGuid():N}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/profiles/{vendorId.Value}/{uniqueFileName}";

            return Ok(new { url = imageUrl, message = "Gorsel yuklendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Dosya yuklenirken hata olustu: " + ex.Message });
        }
    }

    // ============================================
    // PUBLIC ENDPOINTS
    // ============================================

    /// <summary>
    /// Public profil getir (slug ile)
    /// </summary>
    [HttpGet("public/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicProfile(string slug)
    {
        var profile = await _profileService.GetBySlugAsync(slug, incrementViewCount: true);
        if (profile == null)
        {
            return NotFound(new { message = "Profil bulunamadi." });
        }
        return Ok(profile);
    }

    /// <summary>
    /// Firma profil sayfasindan iletisim mesaji gonder
    /// </summary>
    [HttpPost("public/{slug}/contact")]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitContactRequest(string slug, [FromBody] ProfileContactRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SenderName) || string.IsNullOrWhiteSpace(dto.SenderEmail)
            || string.IsNullOrWhiteSpace(dto.Subject) || string.IsNullOrWhiteSpace(dto.Message))
        {
            return BadRequest(new { message = "Tum zorunlu alanlar doldurulmalidir." });
        }

        // Giris yapmis kullanicinin vendor ID'sini al
        int? senderVendorId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            senderVendorId = (await GetCurrentUserAsync())?.VendorId;
        }

        var success = await _profileService.SubmitContactRequestAsync(slug, dto, senderVendorId);
        if (!success)
        {
            return NotFound(new { message = "Profil bulunamadi." });
        }

        return Ok(new { message = "Mesajiniz basariyla iletildi." });
    }
}

// ============================================
// HELPER DTOs
// ============================================

public class SetProfileVisibilityDto
{
    public bool IsPubliclyVisible { get; set; }
}
