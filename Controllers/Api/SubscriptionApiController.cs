using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Abonelik API - Kullanicinin capability'sine gore abonelik bilgileri
/// </summary>
[Authorize]
[ApiController]
[Route("api/subscription")]
public class SubscriptionApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SubscriptionApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Mevcut abonelik bilgisi
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentSubscription()
    {
        var userId = GetUserId();
        var user = await _context.Users
            .Include(u => u.Vendor)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.Vendor == null)
            return Ok(new { });

        // Firmanin ilk capability'sini al
        var capability = await _context.VendorCapabilityMappings
            .Where(m => m.VendorId == user.VendorId)
            .Select(m => m.CapabilityId)
            .FirstOrDefaultAsync();

        // Simdilik sabit degerler dondur - gercek subscription sistemi kurulunca guncellenecek
        var limits = GetDefaultLimits(capability);

        return Ok(new
        {
            name = "Baslangic",
            planId = 1,
            price = 0,
            isActive = true,
            isFree = true,
            renewalDate = (DateTime?)null,
            capabilityId = capability > 0 ? capability : 2, // Default: Seller
            limits
        });
    }

    /// <summary>
    /// Mevcut planlar
    /// </summary>
    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
    {
        var userId = GetUserId();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.VendorId == null)
            return Ok(new List<object>());

        // Firmanin capability'sini al
        var capability = await _context.VendorCapabilityMappings
            .Where(m => m.VendorId == user.VendorId)
            .Select(m => m.CapabilityId)
            .FirstOrDefaultAsync();

        // Simdilik bos dondur - JS tarafinda default planlar kullanilacak
        // Gercek plan sistemi kurulunca burasi guncellenecek
        return Ok(new List<object>());
    }

    /// <summary>
    /// Fatura gecmisi
    /// </summary>
    [HttpGet("invoices")]
    public IActionResult GetInvoices()
    {
        // Simdilik bos - fatura sistemi kurulunca guncellenecek
        return Ok(new List<object>());
    }

    /// <summary>
    /// Plan degistir
    /// </summary>
    [HttpPost("change-plan")]
    public IActionResult ChangePlan([FromBody] ChangePlanDto dto)
    {
        // Simdilik sadece mesaj dondur - odeme sistemi kurulunca guncellenecek
        return Ok(new { message = "Plan degisikligi icin odeme sayfasina yonlendirileceksiniz", checkoutUrl = (string?)null });
    }

    /// <summary>
    /// Abonelik iptal
    /// </summary>
    [HttpPost("cancel")]
    public IActionResult CancelSubscription()
    {
        // Simdilik sadece mesaj dondur
        return Ok(new { message = "Abonelik iptal edildi" });
    }

    // Capability'ye gore default limitler
    private object GetDefaultLimits(int capabilityId)
    {
        return capabilityId switch
        {
            2 => new // Seller
            {
                productLimit = "10",
                orderLimit = "50",
                teamMemberLimit = "2",
                storageLimit = "100 MB"
            },
            3 => new // Buyer
            {
                demandLimit = "5",
                supplierLimit = "10",
                teamMemberLimit = "2",
                rfqLimit = "20"
            },
            4 => new // Carrier
            {
                quoteLimit = "10",
                shipmentLimit = "5",
                vehicleLimit = "3",
                routeLimit = "Yerel"
            },
            5 => new // Insurance
            {
                policyLimit = "10",
                coverageLimit = "100K USD",
                sectorLimit = "3 Sektor",
                teamMemberLimit = "2"
            },
            6 => new // Customs
            {
                declarationLimit = "10",
                portLimit = "3 Nokta",
                hsCodeLimit = "50 Kod",
                teamMemberLimit = "2"
            },
            7 => new // Survey
            {
                inspectionLimit = "5",
                fieldTeamLimit = "2",
                geoLimit = "Yerel",
                reportLimit = "10"
            },
            8 => new // Investor
            {
                investmentLimit = "5",
                portfolioLimit = "100K USD",
                sectorLimit = "3 Sektor",
                analysisLimit = "10"
            },
            _ => new // Default
            {
                teamMemberLimit = "2",
                storageLimit = "100 MB",
                apiLimit = "1000/ay",
                supportLevel = "Email"
            }
        };
    }
}

public class ChangePlanDto
{
    public int PlanId { get; set; }
}
