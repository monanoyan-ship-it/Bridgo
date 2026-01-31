using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Extensions;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Yatirim/Finansman API - Yatirimci capability icin
/// Finansman taleplerini goruntuleme ve teklif verme
/// </summary>
[Authorize]
[ApiController]
[Route("api/investment")]
public class InvestmentApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IRiskScoringService _riskScoringService;

    public InvestmentApiController(ApplicationDbContext context, IRiskScoringService riskScoringService)
    {
        _context = context;
        _riskScoringService = riskScoringService;
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
    // FINANCING OPPORTUNITIES (Yatirimci icin)
    // ========================================

    /// <summary>
    /// Yatirim firsatlarini getir (aktif finansman talepleri)
    /// </summary>
    [HttpGet("opportunities")]
    public async Task<IActionResult> GetOpportunities([FromQuery] int? financingType = null, [FromQuery] string? search = null)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        // Aktif durumlardaki talepleri getir (Open, OffersReceived, PartiallyFunded)
        var activeStatuses = new[] { 2, 3, 4 }; // Open, OffersReceived, PartiallyFunded

        var query = _context.FinancingRequests
            .Include(r => r.RequesterVendor)
            .Include(r => r.RelatedOrder)
            .Include(r => r.Offers)
            .Where(r => activeStatuses.Contains(r.Status))
            .Where(r => r.RequesterVendorId != vendorId.Value) // Kendi taleplerini gosterme
            .AsQueryable();

        if (financingType.HasValue)
            query = query.Where(r => r.FinancingType == financingType.Value);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(r => r.Title.Contains(search) ||
                                    (r.Description != null && r.Description.Contains(search)));

        var rawData = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.Title,
                r.Description,
                r.FinancingType,
                r.RequestedAmount,
                r.Currency,
                r.FundedAmount,
                r.MaxInterestRate,
                r.DurationDays,
                r.RepaymentDate,
                r.CollateralType,
                r.RiskScore,
                r.Status,
                r.OfferDeadline,
                r.CreatedAt,
                RequesterCompanyName = r.RequesterVendor != null ? r.RequesterVendor.CompanyName : null,
                HasRelatedOrder = r.RelatedOrderId.HasValue,
                OrderNumber = r.RelatedOrder != null ? r.RelatedOrder.OrderNumber : null,
                OfferCount = r.Offers.Count(o => !o.IsDeleted),
                HasMyOffer = r.Offers.Any(o => o.InvestorVendorId == vendorId.Value && !o.IsDeleted)
            })
            .ToListAsync();

        var requests = rawData.Select(r => new
        {
            r.Id,
            r.Title,
            r.Description,
            r.FinancingType,
            financingTypeName = FinancingTypes.GetById(r.FinancingType)?.SystemName,
            r.RequestedAmount,
            r.Currency,
            r.FundedAmount,
            remainingAmount = r.RequestedAmount - r.FundedAmount,
            fundingProgress = r.RequestedAmount > 0 ? Math.Round((r.FundedAmount / r.RequestedAmount) * 100, 2) : 0,
            r.MaxInterestRate,
            r.DurationDays,
            r.RepaymentDate,
            r.CollateralType,
            collateralTypeName = r.CollateralType.HasValue ? CollateralTypes.GetById(r.CollateralType.Value)?.SystemName : null,
            r.RiskScore,
            r.Status,
            statusName = FinancingRequestStatuses.GetById(r.Status)?.SystemName,
            statusCss = FinancingRequestStatuses.GetById(r.Status)?.CssClass,
            r.OfferDeadline,
            r.CreatedAt,
            requesterCompanyName = r.RequesterCompanyName,
            hasRelatedOrder = r.HasRelatedOrder,
            orderNumber = r.OrderNumber,
            offerCount = r.OfferCount,
            hasMyOffer = r.HasMyOffer
        }).ToList();

        return Ok(requests);
    }

    /// <summary>
    /// Finansman talebi detayi
    /// </summary>
    [HttpGet("opportunities/{id}")]
    public async Task<IActionResult> GetOpportunity(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var request = await _context.FinancingRequests
            .Include(r => r.RequesterVendor)
            .Include(r => r.RelatedOrder)
            .Include(r => r.Offers.Where(o => !o.IsDeleted))
                .ThenInclude(o => o.InvestorVendor)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound(new { error = "Talep bulunamadi" });

        // Kendi teklifi
        var myOffer = request.Offers
            .FirstOrDefault(o => o.InvestorVendorId == vendorId.Value);

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
            request.RiskScore,
            request.RiskNotes,
            request.Status,
            statusName = FinancingRequestStatuses.GetById(request.Status)?.SystemName,
            statusCss = FinancingRequestStatuses.GetById(request.Status)?.CssClass,
            request.OfferDeadline,
            request.CreatedAt,
            requesterCompanyName = request.RequesterVendor?.CompanyName,
            hasRelatedOrder = request.RelatedOrderId.HasValue,
            orderNumber = request.RelatedOrder?.OrderNumber,
            myOffer = myOffer != null ? new
            {
                myOffer.Id,
                myOffer.OfferedAmount,
                myOffer.InterestRate,
                myOffer.TotalRepaymentAmount,
                myOffer.Notes,
                myOffer.ValidUntil,
                myOffer.Status,
                statusName = InvestmentOfferStatuses.GetById(myOffer.Status)?.SystemName,
                statusCss = InvestmentOfferStatuses.GetById(myOffer.Status)?.CssClass,
                myOffer.CreatedAt
            } : null,
            // Diger teklifler (sadece kabul edilmisleri goster)
            acceptedOffers = request.Offers
                .Where(o => o.Status == 2 || o.Status == 6) // Accepted or Funded
                .Select(o => new
                {
                    o.Id,
                    o.OfferedAmount,
                    o.InterestRate,
                    investorCompanyName = o.InvestorVendor?.CompanyName,
                    o.Status,
                    statusName = InvestmentOfferStatuses.GetById(o.Status)?.SystemName
                })
                .ToList()
        });
    }

    /// <summary>
    /// Finansman talebine teklif ver
    /// </summary>
    [HttpPost("opportunities/{id}/offer")]
    public async Task<IActionResult> SubmitOffer(int id, [FromBody] SubmitOfferDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        var userId = GetUserId();

        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var request = await _context.FinancingRequests
            .Include(r => r.Offers)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound(new { error = "Talep bulunamadi" });

        // Kendi talebine teklif veremez
        if (request.RequesterVendorId == vendorId.Value)
            return BadRequest(new { error = "Kendi talebinize teklif veremezsiniz" });

        // Sadece aktif taleplere teklif verilebilir
        var activeStatuses = new[] { 2, 3, 4 }; // Open, OffersReceived, PartiallyFunded
        if (!activeStatuses.Contains(request.Status))
            return BadRequest(new { error = "Bu talep aktif degil" });

        // Daha once teklif verilmis mi?
        var existingOffer = request.Offers
            .FirstOrDefault(o => o.InvestorVendorId == vendorId.Value && !o.IsDeleted);
        if (existingOffer != null)
            return BadRequest(new { error = "Bu talebe zaten teklif verdiniz" });

        // Teklif son tarihi gecmis mi?
        if (request.OfferDeadline.HasValue && request.OfferDeadline.Value < DateTime.UtcNow)
            return BadRequest(new { error = "Teklif suresi dolmus" });

        // Teklif tutari kontrolu
        var remainingAmount = request.RequestedAmount - request.FundedAmount;
        if (dto.OfferedAmount > remainingAmount)
            return BadRequest(new { error = $"Teklif tutari kalan tutardan ({remainingAmount:N2} {request.Currency}) fazla olamaz" });

        // Max faiz orani kontrolu
        if (request.MaxInterestRate.HasValue && dto.InterestRate > request.MaxInterestRate.Value)
            return BadRequest(new { error = $"Faiz orani maksimum %{request.MaxInterestRate} olabilir" });

        // Geri odeme tutarini hesapla
        var interestAmount = dto.OfferedAmount * (dto.InterestRate / 100) * (request.DurationDays / 30m);
        var totalRepayment = dto.OfferedAmount + interestAmount;

        var offer = new InvestmentOffer
        {
            FinancingRequestId = id,
            InvestorVendorId = vendorId.Value,
            InvestorUserId = userId,
            OfferedAmount = dto.OfferedAmount,
            InterestRate = dto.InterestRate,
            TotalRepaymentAmount = totalRepayment,
            Notes = dto.Notes,
            ValidUntil = dto.ValidUntil.ToUtcSafe(),
            Status = 1 // Pending
        };

        _context.InvestmentOffers.Add(offer);

        // Talep durumunu guncelle (en az bir teklif varsa OffersReceived)
        if (request.Status == 2) // Open
        {
            request.Status = 3; // OffersReceived
        }

        await _context.SaveChangesAsync();

        return Ok(new {
            success = true,
            message = "Teklifiniz gonderildi",
            offerId = offer.Id,
            totalRepaymentAmount = totalRepayment
        });
    }

    /// <summary>
    /// Teklifi geri cek
    /// </summary>
    [HttpPost("offers/{offerId}/withdraw")]
    public async Task<IActionResult> WithdrawOffer(int offerId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var offer = await _context.InvestmentOffers
            .Include(o => o.FinancingRequest)
            .FirstOrDefaultAsync(o => o.Id == offerId);

        if (offer == null)
            return NotFound(new { error = "Teklif bulunamadi" });

        if (offer.InvestorVendorId != vendorId.Value)
            return Forbid();

        // Sadece bekleyen teklifler geri cekilebilir
        if (offer.Status != 1) // Pending
            return BadRequest(new { error = "Sadece bekleyen teklifler geri cekilebilir" });

        offer.Status = 4; // Withdrawn
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Teklif geri cekildi" });
    }

    // ========================================
    // MY OFFERS (Yatirimcinin verdigi teklifler)
    // ========================================

    /// <summary>
    /// Yatirimcinin verdigi teklifleri getir
    /// </summary>
    [HttpGet("my-offers")]
    public async Task<IActionResult> GetMyOffers([FromQuery] int? status = null)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var query = _context.InvestmentOffers
            .Include(o => o.FinancingRequest)
                .ThenInclude(r => r!.RequesterVendor)
            .Where(o => o.InvestorVendorId == vendorId.Value)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var offers = await query
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
                statusName = InvestmentOfferStatuses.GetById(o.Status)!.SystemName,
                statusCss = InvestmentOfferStatuses.GetById(o.Status)!.CssClass,
                o.ResponseDate,
                o.RejectionReason,
                o.IsFundTransferred,
                o.IsRepaid,
                o.CreatedAt,
                request = new
                {
                    o.FinancingRequest!.Id,
                    o.FinancingRequest.Title,
                    o.FinancingRequest.FinancingType,
                    financingTypeName = FinancingTypes.GetById(o.FinancingRequest.FinancingType)!.SystemName,
                    o.FinancingRequest.RequestedAmount,
                    o.FinancingRequest.Currency,
                    o.FinancingRequest.DurationDays,
                    requesterCompanyName = o.FinancingRequest.RequesterVendor != null ? o.FinancingRequest.RequesterVendor.CompanyName : null
                }
            })
            .ToListAsync();

        return Ok(offers);
    }

    // ========================================
    // MY INVESTMENTS (Aktif yatirimlar)
    // ========================================

    /// <summary>
    /// Yatirimcinin aktif yatirimlarini getir (kabul edilen/finanse edilen teklifler)
    /// </summary>
    [HttpGet("my-investments")]
    public async Task<IActionResult> GetMyInvestments([FromQuery] bool? completed = null)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        // Aktif yatirimlar: Status 2 (Accepted) veya 6 (Funded)
        // Tamamlanan: IsRepaid = true
        var query = _context.InvestmentOffers
            .Include(o => o.FinancingRequest)
                .ThenInclude(r => r!.RequesterVendor)
            .Where(o => o.InvestorVendorId == vendorId.Value)
            .Where(o => o.Status == 2 || o.Status == 6) // Accepted or Funded
            .AsQueryable();

        if (completed.HasValue)
        {
            query = query.Where(o => o.IsRepaid == completed.Value);
        }

        var rawData = await query
            .OrderByDescending(o => o.ResponseDate ?? o.CreatedAt)
            .Select(o => new
            {
                o.Id,
                o.OfferedAmount,
                o.InterestRate,
                o.TotalRepaymentAmount,
                o.Notes,
                o.ValidUntil,
                o.Status,
                o.ResponseDate,
                o.IsFundTransferred,
                o.FundTransferDate,
                o.TransferReference,
                o.IsRepaid,
                o.RepaymentDate,
                o.RepaymentReference,
                o.CreatedAt,
                Request = new
                {
                    o.FinancingRequest!.Id,
                    o.FinancingRequest.Title,
                    o.FinancingRequest.Description,
                    o.FinancingRequest.FinancingType,
                    o.FinancingRequest.RequestedAmount,
                    o.FinancingRequest.Currency,
                    o.FinancingRequest.DurationDays,
                    o.FinancingRequest.RepaymentDate,
                    o.FinancingRequest.CollateralType,
                    o.FinancingRequest.RiskScore,
                    o.FinancingRequest.Status,
                    RequesterCompanyName = o.FinancingRequest.RequesterVendor != null ? o.FinancingRequest.RequesterVendor.CompanyName : null
                }
            })
            .ToListAsync();

        var investments = rawData.Select(o => new
        {
            o.Id,
            o.OfferedAmount,
            o.InterestRate,
            o.TotalRepaymentAmount,
            interestAmount = o.TotalRepaymentAmount - o.OfferedAmount,
            o.Notes,
            o.ValidUntil,
            o.Status,
            statusName = InvestmentOfferStatuses.GetById(o.Status)?.SystemName,
            statusCss = InvestmentOfferStatuses.GetById(o.Status)?.CssClass,
            o.ResponseDate,
            o.IsFundTransferred,
            o.FundTransferDate,
            o.TransferReference,
            o.IsRepaid,
            o.RepaymentDate,
            o.RepaymentReference,
            o.CreatedAt,
            // Kalan gun hesabi
            expectedRepaymentDate = o.ResponseDate.HasValue
                ? o.ResponseDate.Value.AddDays(o.Request.DurationDays)
                : (DateTime?)null,
            daysRemaining = o.ResponseDate.HasValue
                ? Math.Max(0, (int)(o.ResponseDate.Value.AddDays(o.Request.DurationDays) - DateTime.UtcNow).TotalDays)
                : 0,
            request = new
            {
                o.Request.Id,
                o.Request.Title,
                o.Request.Description,
                o.Request.FinancingType,
                financingTypeName = FinancingTypes.GetById(o.Request.FinancingType)?.SystemName,
                financingTypeCss = FinancingTypes.GetById(o.Request.FinancingType)?.CssClass,
                o.Request.RequestedAmount,
                o.Request.Currency,
                o.Request.DurationDays,
                o.Request.RepaymentDate,
                o.Request.CollateralType,
                collateralTypeName = o.Request.CollateralType.HasValue ? CollateralTypes.GetById(o.Request.CollateralType.Value)?.SystemName : null,
                o.Request.RiskScore,
                o.Request.Status,
                requestStatusName = FinancingRequestStatuses.GetById(o.Request.Status)?.SystemName,
                requesterCompanyName = o.Request.RequesterCompanyName
            }
        }).ToList();

        return Ok(investments);
    }

    /// <summary>
    /// Yatirim istatistiklerini getir
    /// </summary>
    [HttpGet("my-investments/stats")]
    public async Task<IActionResult> GetMyInvestmentStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var investments = await _context.InvestmentOffers
            .Include(o => o.FinancingRequest)
            .Where(o => o.InvestorVendorId == vendorId.Value)
            .Where(o => o.Status == 2 || o.Status == 6) // Accepted or Funded
            .ToListAsync();

        var activeInvestments = investments.Where(o => !o.IsRepaid).ToList();
        var completedInvestments = investments.Where(o => o.IsRepaid).ToList();

        return Ok(new
        {
            activeCount = activeInvestments.Count,
            activeTotalAmount = activeInvestments.Sum(o => o.OfferedAmount),
            activeExpectedReturn = activeInvestments.Sum(o => o.TotalRepaymentAmount - o.OfferedAmount),
            completedCount = completedInvestments.Count,
            completedTotalAmount = completedInvestments.Sum(o => o.OfferedAmount),
            completedEarnedReturn = completedInvestments.Sum(o => o.TotalRepaymentAmount - o.OfferedAmount),
            pendingTransferCount = activeInvestments.Count(o => !o.IsFundTransferred)
        });
    }

    // ========================================
    // TYPES (Tip listeleri)
    // ========================================

    /// <summary>
    /// Finansman tiplerini getir
    /// </summary>
    [HttpGet("types/financing")]
    public IActionResult GetFinancingTypes()
    {
        var types = FinancingTypes.All.Select(t => new
        {
            id = t.Id,
            name = t.SystemName,
            nameResourceKey = t.NameResourceKey,
            description = t.Description,
            icon = t.Icon,
            cssClass = t.CssClass
        });
        return Ok(types);
    }

    /// <summary>
    /// Teminat tiplerini getir
    /// </summary>
    [HttpGet("types/collateral")]
    public IActionResult GetCollateralTypes()
    {
        var types = CollateralTypes.All.Select(t => new
        {
            id = t.Id,
            name = t.SystemName,
            nameResourceKey = t.NameResourceKey,
            description = t.Description,
            icon = t.Icon
        });
        return Ok(types);
    }

    /// <summary>
    /// Talep durumlarini getir
    /// </summary>
    [HttpGet("types/request-status")]
    public IActionResult GetRequestStatuses()
    {
        var types = FinancingRequestStatuses.All.Select(t => new
        {
            id = t.Id,
            name = t.SystemName,
            nameResourceKey = t.NameResourceKey,
            description = t.Description,
            icon = t.Icon,
            cssClass = t.CssClass
        });
        return Ok(types);
    }

    /// <summary>
    /// Teklif durumlarini getir
    /// </summary>
    [HttpGet("types/offer-status")]
    public IActionResult GetOfferStatuses()
    {
        var types = InvestmentOfferStatuses.All.Select(t => new
        {
            id = t.Id,
            name = t.SystemName,
            nameResourceKey = t.NameResourceKey,
            description = t.Description,
            icon = t.Icon,
            cssClass = t.CssClass
        });
        return Ok(types);
    }

    // ========================================
    // RISK SCORING (Risk Skorlama)
    // ========================================

    /// <summary>
    /// Finansman talebinin risk skorunu getir
    /// </summary>
    [HttpGet("{requestId}/risk-score")]
    public async Task<IActionResult> GetRiskScore(int requestId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var request = await _context.FinancingRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDeleted);

        if (request == null)
            return NotFound(new { error = "Finansman talebi bulunamadi" });

        var result = await _riskScoringService.CalculateRiskScoreAsync(requestId);

        return Ok(new
        {
            score = result.Score,
            level = result.Level,
            levelCss = result.LevelCssClass,
            notes = result.Notes,
            factors = result.Factors.Select(f => new
            {
                name = f.Name,
                category = f.Category,
                score = f.Score,
                weight = f.Weight,
                weightedScore = f.WeightedScore,
                description = f.Description,
                isPositive = f.IsPositive
            })
        });
    }

    /// <summary>
    /// Finansman talebinin risk skorunu yeniden hesapla ve kaydet
    /// </summary>
    [HttpPost("{requestId}/recalculate-risk")]
    public async Task<IActionResult> RecalculateRiskScore(int requestId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var request = await _context.FinancingRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDeleted);

        if (request == null)
            return NotFound(new { error = "Finansman talebi bulunamadi" });

        // Sadece talep sahibi veya admin yeniden hesaplayabilir
        if (request.RequesterVendorId != vendorId.Value)
            return Forbid();

        var newScore = await _riskScoringService.UpdateRiskScoreAsync(requestId);

        return Ok(new { score = newScore, message = "Risk skoru guncellendi" });
    }

    /// <summary>
    /// Firma risk skorunu getir
    /// </summary>
    [HttpGet("vendor/{vendorId}/risk-score")]
    public async Task<IActionResult> GetVendorRiskScore(int vendorId)
    {
        var myVendorId = await GetUserVendorIdAsync();
        if (!myVendorId.HasValue)
            return Unauthorized(new { error = "Firma bulunamadi" });

        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Id == vendorId && !v.IsDeleted);

        if (vendor == null)
            return NotFound(new { error = "Firma bulunamadi" });

        var result = await _riskScoringService.CalculateVendorRiskScoreAsync(vendorId);

        return Ok(new
        {
            vendorId = vendorId,
            companyName = vendor.CompanyName,
            score = result.Score,
            level = result.Level,
            levelCss = result.LevelCssClass,
            notes = result.Notes,
            factors = result.Factors.Select(f => new
            {
                name = f.Name,
                category = f.Category,
                score = f.Score,
                weight = f.Weight,
                weightedScore = f.WeightedScore,
                description = f.Description,
                isPositive = f.IsPositive
            })
        });
    }
}

// DTO'lar
public class SubmitOfferDto
{
    public decimal OfferedAmount { get; set; }
    public decimal InterestRate { get; set; }
    public string? Notes { get; set; }
    public DateTime? ValidUntil { get; set; }
}
