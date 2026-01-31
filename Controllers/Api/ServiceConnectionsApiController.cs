using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.Authorization;
using Bridgo.Data;
using Bridgo.Models.Entities;
using Bridgo.Models.Identity;

namespace Bridgo.Controllers.Api;

[Route("api/services")]
[ApiController]
[Authorize]
[RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
public class ServiceConnectionsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ServiceConnectionsApiController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ============================================
    // CONNECTIONS
    // ============================================

    /// <summary>
    /// Tum servis baglantilerini getir
    /// </summary>
    [HttpGet("connections")]
    public async Task<ActionResult<List<ServiceConnectionDto>>> GetConnections()
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId == 0) return Unauthorized();

        var connections = await _context.VendorServiceConnections
            .Where(c => c.VendorId == vendorId && !c.IsDeleted)
            .Select(c => new ServiceConnectionDto
            {
                ServiceCode = c.ServiceCode,
                Category = c.Category,
                Status = c.Status,
                ExternalAccountId = c.ExternalAccountId,
                ChargesEnabled = c.ChargesEnabled,
                PayoutsEnabled = c.PayoutsEnabled,
                ConnectedAt = c.ConnectedAt,
                LastSyncAt = c.LastSyncAt,
                LastError = c.LastError
            })
            .ToListAsync();

        return Ok(connections);
    }

    /// <summary>
    /// Servise baglan
    /// </summary>
    [HttpPost("{serviceCode}/connect")]
    public async Task<IActionResult> Connect(string serviceCode)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId == 0) return Unauthorized();

        // Mevcut baglanti var mi kontrol et
        var existing = await _context.VendorServiceConnections
            .FirstOrDefaultAsync(c => c.VendorId == vendorId && c.ServiceCode == serviceCode && !c.IsDeleted);

        if (existing != null && existing.Status == "connected")
            return BadRequest(new { message = "Bu servis zaten bagli" });

        // Servis tipine gore islem yap
        switch (serviceCode)
        {
            case "stripe":
                return await ConnectStripe(vendorId, existing);

            case "iyzico":
            case "paypal":
                // Diger odeme servisleri icin OAuth akisi
                return Ok(new { message = "Yakin zamanda eklenecek", success = false });

            case "aras":
            case "yurtici":
            case "dhl":
            case "ups":
                // Lojistik servisleri icin API key girisi gerekebilir
                return Ok(new { message = "Yakin zamanda eklenecek", success = false });

            case "parasut":
            case "logo":
            case "efatura":
                // Muhasebe servisleri
                return Ok(new { message = "Yakin zamanda eklenecek", success = false });

            case "trendyol":
            case "hepsiburada":
            case "n11":
                // Pazar yerleri
                return Ok(new { message = "Yakin zamanda eklenecek", success = false });

            default:
                return BadRequest(new { message = "Bilinmeyen servis" });
        }
    }

    /// <summary>
    /// Servis baglantisini kes
    /// </summary>
    [HttpDelete("{serviceCode}/disconnect")]
    public async Task<IActionResult> Disconnect(string serviceCode)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId == 0) return Unauthorized();

        var connection = await _context.VendorServiceConnections
            .FirstOrDefaultAsync(c => c.VendorId == vendorId && c.ServiceCode == serviceCode && !c.IsDeleted);

        if (connection == null)
            return NotFound(new { message = "Baglanti bulunamadi" });

        connection.Status = "disconnected";
        connection.DisconnectedAt = DateTime.UtcNow;
        connection.AccessToken = null;
        connection.RefreshToken = null;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Baglanti kesildi" });
    }

    /// <summary>
    /// Servis dashboard linkini al
    /// </summary>
    [HttpPost("{serviceCode}/dashboard-link")]
    public async Task<IActionResult> GetDashboardLink(string serviceCode)
    {
        var vendorId = await GetVendorIdAsync();
        if (vendorId == 0) return Unauthorized();

        var connection = await _context.VendorServiceConnections
            .FirstOrDefaultAsync(c => c.VendorId == vendorId && c.ServiceCode == serviceCode && !c.IsDeleted);

        if (connection == null || connection.Status != "connected")
            return BadRequest(new { message = "Servis bagli degil" });

        switch (serviceCode)
        {
            case "stripe":
                // Stripe Connect dashboard linki
                // TODO: Stripe API ile login link olustur
                return Ok(new { url = "https://dashboard.stripe.com" });

            case "parasut":
                return Ok(new { url = "https://uygulama.parasut.com" });

            case "trendyol":
                return Ok(new { url = "https://partner.trendyol.com" });

            case "hepsiburada":
                return Ok(new { url = "https://merchant.hepsiburada.com" });

            default:
                return BadRequest(new { message = "Dashboard linki mevcut degil" });
        }
    }

    // ============================================
    // STRIPE CONNECT
    // ============================================

    private async Task<IActionResult> ConnectStripe(int vendorId, VendorServiceConnection? existing)
    {
        // TODO: Stripe Connect OAuth akisi baslatilacak
        // Simdilik placeholder

        if (existing == null)
        {
            existing = new VendorServiceConnection
            {
                VendorId = vendorId,
                ServiceCode = "stripe",
                Category = "payment",
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.VendorServiceConnections.Add(existing);
        }
        else
        {
            existing.Status = "pending";
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Stripe OAuth URL'ine yonlendir
        var redirectUrl = $"/Dashboard/ServiceConnections?stripe_connected=pending";

        // Gercek Stripe entegrasyonu yapildiginda:
        // var stripeUrl = await _stripeService.CreateAccountLinkAsync(vendorId);
        // return Ok(new { redirectUrl = stripeUrl });

        return Ok(new { redirectUrl, message = "Stripe entegrasyonu yakin zamanda eklenecek" });
    }

    /// <summary>
    /// Stripe OAuth callback
    /// </summary>
    [HttpGet("stripe/callback")]
    public async Task<IActionResult> StripeCallback([FromQuery] string code, [FromQuery] string state)
    {
        // TODO: Stripe OAuth callback islemleri
        // 1. code ile access token al
        // 2. Stripe hesap bilgilerini cek
        // 3. VendorServiceConnection guncelle

        return Redirect("/Dashboard/ServiceConnections?stripe_connected=true");
    }

    // ============================================
    // HELPERS
    // ============================================

    private async Task<int> GetVendorIdAsync()
    {
        var userIdStr = _userManager.GetUserId(User);
        if (!int.TryParse(userIdStr, out var userId))
            return 0;

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.VendorId })
            .FirstOrDefaultAsync();

        return user?.VendorId ?? 0;
    }
}

// ============================================
// DTOs
// ============================================

public class ServiceConnectionDto
{
    public string ServiceCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ExternalAccountId { get; set; }
    public bool ChargesEnabled { get; set; }
    public bool PayoutsEnabled { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string? LastError { get; set; }
}
