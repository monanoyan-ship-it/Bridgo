using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.Authorization;
using Bridgo.DTOs.Supplier;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Tedarikci profilleri - Buyer arar, Seller profil olusturur
/// </summary>
[ApiController]
[Route("api/suppliers")]
[Authorize]
[RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
public class SuppliersApiController : ControllerBase
{
    private readonly ISupplierProfileService _supplierService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SuppliersApiController(
        ISupplierProfileService supplierService,
        UserManager<ApplicationUser> userManager)
    {
        _supplierService = supplierService;
        _userManager = userManager;
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

    // ============================================
    // PUBLIC ENDPOINTS (Login gerektirmez)
    // ============================================

    /// <summary>
    /// Public tedarikci profillerini listele (SEO sayfasi icin)
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicSuppliers([FromQuery] SupplierFilterDto filter)
    {
        var result = await _supplierService.GetPublicSuppliersAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Tedarikci profili getir (slug ile - SEO)
    /// </summary>
    [HttpGet("public/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProfileBySlug(string slug)
    {
        var profile = await _supplierService.GetProfileBySlugAsync(slug, incrementViewCount: true);
        if (profile == null) return NotFound(new { message = "Tedarikci bulunamadi." });
        return Ok(profile);
    }

    /// <summary>
    /// Tedarikçiye iletisim mesaji gonder
    /// </summary>
    [HttpPost("{supplierId:int}/contact")]
    [AllowAnonymous]
    public async Task<IActionResult> SendContactMessage(int supplierId, [FromBody] SupplierContactDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest(new { message = "Ad, e-posta ve mesaj zorunludur." });

        var success = await _supplierService.SendContactMessageAsync(supplierId, dto);
        if (!success) return NotFound(new { message = "Tedarikci bulunamadi." });

        return Ok(new { message = "Mesajiniz iletildi." });
    }

    // ============================================
    // AUTHENTICATED ENDPOINTS - Profil Yonetimi
    // ============================================

    /// <summary>
    /// Kendi profilimi getir
    /// </summary>
    [HttpGet("my-profile")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var profile = await _supplierService.GetProfileByVendorIdAsync(vendorId.Value);
        return Ok(profile); // null olabilir - profil henuz olusturulmamissa
    }

    /// <summary>
    /// Profil olustur veya guncelle
    /// </summary>
    [HttpPost("my-profile")]
    [Authorize]
    public async Task<IActionResult> CreateOrUpdateProfile([FromBody] SupplierProfileCreateUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var profile = await _supplierService.CreateOrUpdateProfileAsync(vendorId.Value, dto, GetUserId().ToString());
        return Ok(profile);
    }

    /// <summary>
    /// Profil guncelle (alias for POST - JS compatibility)
    /// </summary>
    [HttpPut("my-profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] SupplierProfileCreateUpdateDto dto)
    {
        return await CreateOrUpdateProfile(dto);
    }

    /// <summary>
    /// Galeri gorseli yukle
    /// </summary>
    [HttpPost("my-profile/gallery")]
    [Authorize]
    public async Task<IActionResult> UploadGalleryImage(IFormFile file)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya secilmedi" });

        // Dosya tipi kontrolu
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Gecersiz dosya tipi. Sadece JPG, PNG, GIF ve WebP desteklenir." });

        // Dosya boyutu kontrolu (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Dosya boyutu 5MB'dan buyuk olamaz" });

        try
        {
            // Klasor olustur
            var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var uploadsFolder = Path.Combine(env.WebRootPath, "uploads", "suppliers", vendorId.Value.ToString());
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Benzersiz dosya adi
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var uniqueFileName = $"{Guid.NewGuid():N}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Dosyayi kaydet
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // URL olustur
            var imageUrl = $"/uploads/suppliers/{vendorId.Value}/{uniqueFileName}";

            return Ok(new { url = imageUrl, message = "Gorsel yuklendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Dosya yuklenirken hata olustu: " + ex.Message });
        }
    }

    /// <summary>
    /// Profil gorunurluğunu degistir
    /// </summary>
    [HttpPost("my-profile/visibility")]
    [Authorize]
    public async Task<IActionResult> SetProfileVisibility([FromBody] SetVisibilityDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _supplierService.SetProfileVisibilityAsync(vendorId.Value, dto.IsPubliclyVisible, GetUserId().ToString());
        if (!success) return NotFound(new { message = "Profil bulunamadi." });

        return Ok(new { message = dto.IsPubliclyVisible ? "Profil yayinlandi." : "Profil gizlendi." });
    }

    /// <summary>
    /// Profil istatistikleri
    /// </summary>
    [HttpGet("my-profile/stats")]
    [Authorize]
    public async Task<IActionResult> GetProfileStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var stats = await _supplierService.GetProfileStatsAsync(vendorId.Value);
        return Ok(stats);
    }

    // ============================================
    // CATEGORY SUBSCRIPTION - Kategori Takibi
    // ============================================

    /// <summary>
    /// Takip edilen kategorileri listele
    /// </summary>
    [HttpGet("subscriptions")]
    [Authorize]
    public async Task<IActionResult> GetSubscriptions()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var subscriptions = await _supplierService.GetSubscriptionsAsync(vendorId.Value);
        return Ok(subscriptions);
    }

    /// <summary>
    /// Kategori takip ediliyor mu kontrol et
    /// </summary>
    [HttpGet("subscriptions/check/{categoryId:int}")]
    [Authorize]
    public async Task<IActionResult> CheckSubscription(int categoryId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var isSubscribed = await _supplierService.IsSubscribedToCategoryAsync(vendorId.Value, categoryId);
        return Ok(new { isSubscribed, categoryId });
    }

    /// <summary>
    /// Hizli kategori takibi (varsayilan ayarlarla)
    /// </summary>
    [HttpPost("subscriptions/quick")]
    [Authorize]
    public async Task<IActionResult> QuickSubscribe([FromBody] QuickSubscribeDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        // Zaten takip ediliyor mu?
        var isSubscribed = await _supplierService.IsSubscribedToCategoryAsync(vendorId.Value, dto.CategoryId);
        if (isSubscribed)
            return Ok(new { message = "Bu kategori zaten takip ediliyor.", alreadySubscribed = true });

        var subscription = await _supplierService.AddSubscriptionAsync(vendorId.Value, new CategorySubscriptionCreateDto
        {
            CategoryId = dto.CategoryId,
            NotifyInApp = true,
            NotifyByEmail = false
        }, GetUserId().ToString());

        return Ok(new { message = "Kategori takibe alindi.", subscription, alreadySubscribed = false });
    }

    /// <summary>
    /// Kategori takibi ekle
    /// </summary>
    [HttpPost("subscriptions")]
    [Authorize]
    public async Task<IActionResult> AddSubscription([FromBody] CategorySubscriptionCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        try
        {
            var subscription = await _supplierService.AddSubscriptionAsync(vendorId.Value, dto, GetUserId().ToString());
            return Ok(subscription);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Toplu kategori takibi ekle
    /// </summary>
    [HttpPost("subscriptions/bulk")]
    [Authorize]
    public async Task<IActionResult> AddBulkSubscriptions([FromBody] BulkSubscriptionDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        if (dto.CategoryIds == null || !dto.CategoryIds.Any())
            return BadRequest(new { message = "En az bir kategori secmelisiniz." });

        var subscriptions = await _supplierService.AddBulkSubscriptionsAsync(vendorId.Value, dto.CategoryIds, GetUserId().ToString());
        return Ok(subscriptions);
    }

    /// <summary>
    /// Kategori takibi guncelle
    /// </summary>
    [HttpPut("subscriptions/{subscriptionId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateSubscription(int subscriptionId, [FromBody] CategorySubscriptionUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _supplierService.UpdateSubscriptionAsync(subscriptionId, vendorId.Value, dto, GetUserId().ToString());
        if (!success) return NotFound(new { message = "Abonelik bulunamadi." });

        return Ok(new { message = "Abonelik guncellendi." });
    }

    /// <summary>
    /// Kategori takibini kaldir
    /// </summary>
    [HttpDelete("subscriptions/{subscriptionId:int}")]
    [Authorize]
    public async Task<IActionResult> RemoveSubscription(int subscriptionId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var success = await _supplierService.RemoveSubscriptionAsync(subscriptionId, vendorId.Value, GetUserId().ToString());
        if (!success) return NotFound(new { message = "Abonelik bulunamadi." });

        return Ok(new { message = "Abonelik kaldirildi." });
    }
}

// ============================================
// HELPER DTO'lar
// ============================================

public class SetVisibilityDto
{
    public bool IsPubliclyVisible { get; set; }
}

public class BulkSubscriptionDto
{
    public List<int> CategoryIds { get; set; } = new();
}

public class QuickSubscribeDto
{
    public int CategoryId { get; set; }
}
