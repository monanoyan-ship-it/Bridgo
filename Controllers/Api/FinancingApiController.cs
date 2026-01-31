using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Extensions;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Finansman Talep API - Seller/Buyer capability icin
/// Finansman talebi olusturma, yonetme ve teklifleri degerlendirme
/// </summary>
[Authorize]
[ApiController]
[Route("api/financing")]
public class FinancingApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FinancingApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var userId = GetUserId();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user?.VendorId;
    }

    // ========================================
    // MY REQUESTS (Benim taleplerim)
    // ========================================

    /// <summary>
    /// Benim finansman taleplerim
    /// </summary>
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests([FromQuery] int? status = null, [FromQuery] int? financingType = null)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var query = _context.FinancingRequests
            .Include(r => r.Offers.Where(o => !o.IsDeleted))
            .Where(r => r.RequesterVendorId == vendorId.Value && !r.IsDeleted)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (financingType.HasValue)
            query = query.Where(r => r.FinancingType == financingType.Value);

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.Title,
                r.Description,
                r.FinancingType,
                financingTypeName = FinancingTypes.GetById(r.FinancingType)!.SystemName,
                financingTypeCss = FinancingTypes.GetById(r.FinancingType)!.CssClass,
                r.RequestedAmount,
                r.Currency,
                r.FundedAmount,
                remainingAmount = r.RequestedAmount - r.FundedAmount,
                fundingProgress = r.RequestedAmount > 0 ? Math.Round((r.FundedAmount / r.RequestedAmount) * 100, 2) : 0,
                r.MaxInterestRate,
                r.DurationDays,
                r.RepaymentDate,
                r.CollateralType,
                collateralTypeName = r.CollateralType.HasValue ? CollateralTypes.GetById(r.CollateralType.Value).SystemName : null,
                r.RiskScore,
                r.Status,
                statusName = FinancingRequestStatuses.GetById(r.Status)!.SystemName,
                statusCss = FinancingRequestStatuses.GetById(r.Status)!.CssClass,
                r.OfferDeadline,
                r.CreatedAt,
                offerCount = r.Offers.Count(o => !o.IsDeleted),
                pendingOfferCount = r.Offers.Count(o => o.Status == 1 && !o.IsDeleted),
                acceptedOfferCount = r.Offers.Count(o => o.Status == 2 && !o.IsDeleted)
            })
            .ToListAsync();

        return Ok(requests);
    }

    /// <summary>
    /// Talep detayi
    /// </summary>
    [HttpGet("my-requests/{id}")]
    public async Task<IActionResult> GetMyRequest(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var request = await _context.FinancingRequests
            .Include(r => r.RelatedOrder)
            .Include(r => r.Offers.Where(o => !o.IsDeleted))
                .ThenInclude(o => o.InvestorVendor)
            .FirstOrDefaultAsync(r => r.Id == id && r.RequesterVendorId == vendorId.Value && !r.IsDeleted);

        if (request == null)
            return NotFound(new { error = "Talep bulunamadi" });

        return Ok(new
        {
            request.Id,
            request.Title,
            request.Description,
            request.FinancingType,
            financingTypeName = FinancingTypes.GetById(request.FinancingType)?.SystemName,
            financingTypeCss = FinancingTypes.GetById(request.FinancingType)?.CssClass,
            request.RequestedAmount,
            request.Currency,
            request.TotalValue,
            request.FundedAmount,
            remainingAmount = request.RequestedAmount - request.FundedAmount,
            fundingProgress = request.RequestedAmount > 0 ? Math.Round((request.FundedAmount / request.RequestedAmount) * 100, 2) : 0,
            request.MaxInterestRate,
            request.DurationDays,
            request.RepaymentDate,
            request.CollateralType,
            collateralTypeName = request.CollateralType.HasValue ? CollateralTypes.GetById(request.CollateralType.Value)?.SystemName : null,
            request.CollateralDescription,
            request.InvoiceNumber,
            request.InvoiceDate,
            request.DebtorName,
            request.DebtorTaxNumber,
            request.RelatedOrderId,
            orderNumber = request.RelatedOrder?.OrderNumber,
            request.RiskScore,
            request.RiskNotes,
            request.Status,
            statusName = FinancingRequestStatuses.GetById(request.Status)?.SystemName,
            statusCss = FinancingRequestStatuses.GetById(request.Status)?.CssClass,
            request.OfferDeadline,
            request.CreatedAt,
            offers = request.Offers
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.OfferedAmount,
                    o.InterestRate,
                    o.TotalRepaymentAmount,
                    o.Notes,
                    o.ValidUntil,
                    o.Status,
                    statusName = InvestmentOfferStatuses.GetById(o.Status)?.SystemName,
                    statusCss = InvestmentOfferStatuses.GetById(o.Status)?.CssClass,
                    investorCompanyName = o.InvestorVendor?.CompanyName,
                    o.IsFundTransferred,
                    o.FundTransferDate,
                    o.IsRepaid,
                    o.RepaymentDate,
                    o.CreatedAt
                })
                .ToList()
        });
    }

    /// <summary>
    /// Yeni finansman talebi olustur
    /// </summary>
    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateFinancingRequestDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        var userId = GetUserId();

        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        // Validasyon
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { error = "Baslik zorunludur" });

        if (dto.RequestedAmount <= 0)
            return BadRequest(new { error = "Talep edilen tutar sifirdan buyuk olmalidir" });

        if (dto.DurationDays <= 0)
            return BadRequest(new { error = "Vade suresi sifirdan buyuk olmalidir" });

        // Siparis kontrolu (siparis finansmani icin)
        Order? relatedOrder = null;
        if (dto.FinancingType == 2 && dto.RelatedOrderId.HasValue) // Order financing
        {
            relatedOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == dto.RelatedOrderId.Value &&
                    (o.BuyerVendorId == vendorId.Value || o.SellerVendorId == vendorId.Value));

            if (relatedOrder == null)
                return BadRequest(new { error = "Siparis bulunamadi veya erisim yetkiniz yok" });
        }

        var request = new FinancingRequest
        {
            RequesterVendorId = vendorId.Value,
            FinancingType = dto.FinancingType,
            Title = dto.Title,
            Description = dto.Description,
            RequestedAmount = dto.RequestedAmount,
            Currency = dto.Currency ?? "TRY",
            TotalValue = dto.TotalValue,
            MaxInterestRate = dto.MaxInterestRate,
            DurationDays = dto.DurationDays,
            RepaymentDate = DateTime.UtcNow.AddDays(dto.DurationDays),
            CollateralType = dto.CollateralType,
            CollateralDescription = dto.CollateralDescription,
            RelatedOrderId = dto.RelatedOrderId,
            InvoiceNumber = dto.InvoiceNumber,
            InvoiceDate = dto.InvoiceDate.ToUtcSafe(),
            DebtorName = dto.DebtorName,
            DebtorTaxNumber = dto.DebtorTaxNumber,
            OfferDeadline = dto.OfferDeadline.ToUtcSafe(),
            Status = dto.SaveAsDraft ? 1 : 2 // Draft or Open
        };

        _context.FinancingRequests.Add(request);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = dto.SaveAsDraft ? "Talep taslak olarak kaydedildi" : "Talep basariyla olusturuldu",
            requestId = request.Id
        });
    }

    /// <summary>
    /// Talebi guncelle (sadece Draft durumunda)
    /// </summary>
    [HttpPut("requests/{id}")]
    public async Task<IActionResult> UpdateRequest(int id, [FromBody] CreateFinancingRequestDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        var userId = GetUserId();

        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var request = await _context.FinancingRequests
            .FirstOrDefaultAsync(r => r.Id == id && r.RequesterVendorId == vendorId.Value && !r.IsDeleted);

        if (request == null)
            return NotFound(new { error = "Talep bulunamadi" });

        // Sadece Draft durumundaki talepler guncellenebilir
        if (request.Status != 1) // Draft
            return BadRequest(new { error = "Sadece taslak durumdaki talepler guncellenebilir" });

        // Validasyon
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { error = "Baslik zorunludur" });

        if (dto.RequestedAmount <= 0)
            return BadRequest(new { error = "Talep edilen tutar sifirdan buyuk olmalidir" });

        if (dto.DurationDays <= 0)
            return BadRequest(new { error = "Vade suresi sifirdan buyuk olmalidir" });

        request.FinancingType = dto.FinancingType;
        request.Title = dto.Title;
        request.Description = dto.Description;
        request.RequestedAmount = dto.RequestedAmount;
        request.Currency = dto.Currency ?? "TRY";
        request.TotalValue = dto.TotalValue;
        request.MaxInterestRate = dto.MaxInterestRate;
        request.DurationDays = dto.DurationDays;
        request.RepaymentDate = DateTime.UtcNow.AddDays(dto.DurationDays);
        request.CollateralType = dto.CollateralType;
        request.CollateralDescription = dto.CollateralDescription;
        request.RelatedOrderId = dto.RelatedOrderId;
        request.InvoiceNumber = dto.InvoiceNumber;
        request.InvoiceDate = dto.InvoiceDate.ToUtcSafe();
        request.DebtorName = dto.DebtorName;
        request.DebtorTaxNumber = dto.DebtorTaxNumber;
        request.OfferDeadline = dto.OfferDeadline.ToUtcSafe();
        request.UpdatedAt = DateTime.UtcNow;

        // Yayinla secildiyse durumu Open yap
        if (!dto.SaveAsDraft)
            request.Status = 2; // Open

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = dto.SaveAsDraft ? "Talep guncellendi" : "Talep guncellendi ve yayinlandi"
        });
    }

    /// <summary>
    /// Taslak talebi yayinla
    /// </summary>
    [HttpPost("requests/{id}/publish")]
    public async Task<IActionResult> PublishRequest(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        var userId = GetUserId();

        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var request = await _context.FinancingRequests
            .FirstOrDefaultAsync(r => r.Id == id && r.RequesterVendorId == vendorId.Value && !r.IsDeleted);

        if (request == null)
            return NotFound(new { error = "Talep bulunamadi" });

        if (request.Status != 1) // Draft
            return BadRequest(new { error = "Sadece taslak durumdaki talepler yayinlanabilir" });

        request.Status = 2; // Open
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Talep yayinlandi" });
    }

    /// <summary>
    /// Talebi iptal et
    /// </summary>
    [HttpPost("requests/{id}/cancel")]
    public async Task<IActionResult> CancelRequest(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        var userId = GetUserId();

        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var request = await _context.FinancingRequests
            .Include(r => r.Offers)
            .FirstOrDefaultAsync(r => r.Id == id && r.RequesterVendorId == vendorId.Value && !r.IsDeleted);

        if (request == null)
            return NotFound(new { error = "Talep bulunamadi" });

        // Kabul edilmis veya finanse edilmis teklif varsa iptal edilemez
        var hasAcceptedOffer = request.Offers.Any(o => o.Status == 2 || o.Status == 6); // Accepted or Funded
        if (hasAcceptedOffer)
            return BadRequest(new { error = "Kabul edilmis veya finanse edilmis teklif olan talepler iptal edilemez" });

        // Sadece Draft, Open, OffersReceived durumundaki talepler iptal edilebilir
        var cancellableStatuses = new[] { 1, 2, 3 }; // Draft, Open, OffersReceived
        if (!cancellableStatuses.Contains(request.Status))
            return BadRequest(new { error = "Bu durumdaki talep iptal edilemez" });

        request.Status = 8; // Cancelled
        request.UpdatedAt = DateTime.UtcNow;

        // Bekleyen teklifleri de iptal et
        foreach (var offer in request.Offers.Where(o => o.Status == 1)) // Pending
        {
            offer.Status = 5; // Expired
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Talep iptal edildi" });
    }

    // ========================================
    // OFFER MANAGEMENT (Teklif Yonetimi)
    // ========================================

    /// <summary>
    /// Teklifi kabul et
    /// </summary>
    [HttpPost("offers/{offerId}/accept")]
    public async Task<IActionResult> AcceptOffer(int offerId)
    {
        var vendorId = await GetUserVendorIdAsync();
        var userId = GetUserId();

        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var offer = await _context.InvestmentOffers
            .Include(o => o.FinancingRequest)
            .FirstOrDefaultAsync(o => o.Id == offerId && !o.IsDeleted);

        if (offer == null)
            return NotFound(new { error = "Teklif bulunamadi" });

        // Sadece kendi talebine gelen teklifleri kabul edebilir
        if (offer.FinancingRequest?.RequesterVendorId != vendorId.Value)
            return Forbid();

        // Sadece Pending teklifler kabul edilebilir
        if (offer.Status != 1)
            return BadRequest(new { error = "Sadece bekleyen teklifler kabul edilebilir" });

        // Kalan tutar kontrolu
        var remainingAmount = offer.FinancingRequest.RequestedAmount - offer.FinancingRequest.FundedAmount;
        if (offer.OfferedAmount > remainingAmount)
            return BadRequest(new { error = $"Teklif tutari ({offer.OfferedAmount:N2}) kalan tutardan ({remainingAmount:N2}) fazla" });

        offer.Status = 2; // Accepted
        offer.ResponseDate = DateTime.UtcNow;
        offer.ResponseByUserId = userId;

        // FundedAmount guncelle
        offer.FinancingRequest.FundedAmount += offer.OfferedAmount;

        // Talep durumunu guncelle
        if (offer.FinancingRequest.FundedAmount >= offer.FinancingRequest.RequestedAmount)
        {
            offer.FinancingRequest.Status = 5; // FullyFunded
        }
        else
        {
            offer.FinancingRequest.Status = 4; // PartiallyFunded
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Teklif kabul edildi" });
    }

    /// <summary>
    /// Teklifi reddet
    /// </summary>
    [HttpPost("offers/{offerId}/reject")]
    public async Task<IActionResult> RejectOffer(int offerId, [FromBody] RejectOfferDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        var userId = GetUserId();

        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var offer = await _context.InvestmentOffers
            .Include(o => o.FinancingRequest)
            .FirstOrDefaultAsync(o => o.Id == offerId && !o.IsDeleted);

        if (offer == null)
            return NotFound(new { error = "Teklif bulunamadi" });

        // Sadece kendi talebine gelen teklifleri reddedebilir
        if (offer.FinancingRequest?.RequesterVendorId != vendorId.Value)
            return Forbid();

        // Sadece Pending teklifler reddedilebilir
        if (offer.Status != 1)
            return BadRequest(new { error = "Sadece bekleyen teklifler reddedilebilir" });

        offer.Status = 3; // Rejected
        offer.ResponseDate = DateTime.UtcNow;
        offer.ResponseByUserId = userId;
        offer.RejectionReason = dto.Reason;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Teklif reddedildi" });
    }

    /// <summary>
    /// Transfer onayini bildir (Yatirimci paraya gonderdi)
    /// </summary>
    [HttpPost("offers/{offerId}/confirm-transfer")]
    public async Task<IActionResult> ConfirmTransfer(int offerId, [FromBody] ConfirmTransferDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        var userId = GetUserId();

        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var offer = await _context.InvestmentOffers
            .Include(o => o.FinancingRequest)
            .FirstOrDefaultAsync(o => o.Id == offerId && !o.IsDeleted);

        if (offer == null)
            return NotFound(new { error = "Teklif bulunamadi" });

        // Sadece kendi talebine gelen tekliflerin transferini onaylayabilir
        if (offer.FinancingRequest?.RequesterVendorId != vendorId.Value)
            return Forbid();

        // Sadece Accepted teklifler icin transfer onaylanabilir
        if (offer.Status != 2)
            return BadRequest(new { error = "Sadece kabul edilmis teklifler icin transfer onaylanabilir" });

        offer.Status = 6; // Funded
        offer.IsFundTransferred = true;
        offer.FundTransferDate = DateTime.UtcNow;
        offer.TransferReference = dto.TransferReference;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Transfer onayi kaydedildi" });
    }

    /// <summary>
    /// Geri odeme bildir
    /// </summary>
    [HttpPost("offers/{offerId}/confirm-repayment")]
    public async Task<IActionResult> ConfirmRepayment(int offerId, [FromBody] ConfirmRepaymentDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        var userId = GetUserId();

        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var offer = await _context.InvestmentOffers
            .Include(o => o.FinancingRequest)
            .FirstOrDefaultAsync(o => o.Id == offerId && !o.IsDeleted);

        if (offer == null)
            return NotFound(new { error = "Teklif bulunamadi" });

        // Sadece kendi talebine gelen tekliflerin geri odemesini bildirebilir
        if (offer.FinancingRequest?.RequesterVendorId != vendorId.Value)
            return Forbid();

        // Sadece Funded teklifler icin geri odeme bildirilebilir
        if (offer.Status != 6)
            return BadRequest(new { error = "Sadece finanse edilmis teklifler icin geri odeme bildirilebilir" });

        offer.IsRepaid = true;
        offer.RepaymentDate = DateTime.UtcNow;
        offer.RepaymentReference = dto.RepaymentReference;

        // Tum kabul edilen teklifler odendiyse talebi Repaid yap
        var allOffersRepaid = await _context.InvestmentOffers
            .Where(o => o.FinancingRequestId == offer.FinancingRequestId && o.Status == 6 && !o.IsDeleted)
            .AllAsync(o => o.IsRepaid);

        if (allOffersRepaid)
        {
            offer.FinancingRequest!.Status = 6; // Repaid
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Geri odeme kaydedildi" });
    }

    // ========================================
    // ORDERS (Siparis listesi - siparis finansmani icin)
    // ========================================

    /// <summary>
    /// Finansman icin uygun siparisleri getir
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrdersForFinancing()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        // Aktif siparisleri getir (finansmana uygun)
        var orders = await _context.Orders
            .Where(o => (o.BuyerVendorId == vendorId.Value || o.SellerVendorId == vendorId.Value) && !o.IsDeleted)
            .Where(o => o.Status >= 2 && o.Status <= 10) // Confirmed'dan Delivered'a kadar
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.TotalAmount,
                o.Currency,
                o.Status,
                o.CreatedAt,
                isBuyer = o.BuyerVendorId == vendorId.Value
            })
            .Take(50)
            .ToListAsync();

        return Ok(orders);
    }
}

// DTO'lar
public class CreateFinancingRequestDto
{
    public int FinancingType { get; set; } = 1;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal RequestedAmount { get; set; }
    public string? Currency { get; set; }
    public decimal? TotalValue { get; set; }
    public decimal? MaxInterestRate { get; set; }
    public int DurationDays { get; set; }
    public int? CollateralType { get; set; }
    public string? CollateralDescription { get; set; }
    public int? RelatedOrderId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? DebtorName { get; set; }
    public string? DebtorTaxNumber { get; set; }
    public DateTime? OfferDeadline { get; set; }
    public bool SaveAsDraft { get; set; }
}

public class RejectOfferDto
{
    public string? Reason { get; set; }
}

public class ConfirmTransferDto
{
    public string? TransferReference { get; set; }
}

public class ConfirmRepaymentDto
{
    public string? RepaymentReference { get; set; }
}
