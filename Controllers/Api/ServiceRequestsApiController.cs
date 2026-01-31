using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Extensions;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Servis Talepleri API - Servis saglayicilar icin
/// Her servis tipi icin ayri endpoint'ler
/// </summary>
[Authorize]
[ApiController]
[Route("api/service-requests")]
public class ServiceRequestsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILocalizationService _localization;
    private readonly IOrderOrchestrationService _orchestrationService;

    public ServiceRequestsApiController(
        ApplicationDbContext context,
        ILocalizationService localization,
        IOrderOrchestrationService orchestrationService)
    {
        _context = context;
        _localization = localization;
        _orchestrationService = orchestrationService;
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
    // LOOKUP DATA
    // ========================================

    /// <summary>
    /// Teklif durumlarini getir
    /// </summary>
    [HttpGet("quote-statuses")]
    [AllowAnonymous]
    public IActionResult GetQuoteStatuses()
    {
        var statuses = ServiceQuoteStatuses.All.Select(s => new
        {
            id = s.Id.ToString(),
            name = L(s.NameResourceKey)
        });
        return Ok(statuses);
    }

    // ========================================
    // LOGISTICS REQUESTS (ServiceType = 1)
    // ========================================

    /// <summary>
    /// Lojistik taleplerini getir
    /// </summary>
    [HttpGet("logistics")]
    public async Task<IActionResult> GetLogisticsRequests([FromQuery] string? status = null)
    {
        return await GetRequestsByType(ServiceTypes.Logistics.Id, status);
    }

    /// <summary>
    /// Lojistik talebi detayi
    /// </summary>
    [HttpGet("logistics/{id}")]
    public async Task<IActionResult> GetLogisticsRequest(int id)
    {
        return await GetRequestDetail(id, ServiceTypes.Logistics.Id);
    }

    /// <summary>
    /// Lojistik talebine teklif ver
    /// </summary>
    [HttpPost("logistics/{id}/quote")]
    public async Task<IActionResult> SubmitLogisticsQuote(int id, [FromBody] SubmitQuoteDto dto)
    {
        return await SubmitQuote(id, ServiceTypes.Logistics.Id, dto);
    }

    // ========================================
    // CUSTOMS REQUESTS (ServiceType = 2)
    // ========================================

    /// <summary>
    /// Gumruk taleplerini getir
    /// </summary>
    [HttpGet("customs")]
    public async Task<IActionResult> GetCustomsRequests([FromQuery] string? status = null)
    {
        return await GetRequestsByType(ServiceTypes.Customs.Id, status);
    }

    /// <summary>
    /// Gumruk talebi detayi
    /// </summary>
    [HttpGet("customs/{id}")]
    public async Task<IActionResult> GetCustomsRequest(int id)
    {
        return await GetRequestDetail(id, ServiceTypes.Customs.Id);
    }

    /// <summary>
    /// Gumruk talebine teklif ver
    /// </summary>
    [HttpPost("customs/{id}/quote")]
    public async Task<IActionResult> SubmitCustomsQuote(int id, [FromBody] SubmitQuoteDto dto)
    {
        return await SubmitQuote(id, ServiceTypes.Customs.Id, dto);
    }

    // ========================================
    // INSURANCE REQUESTS (ServiceType = 3)
    // ========================================

    /// <summary>
    /// Sigorta taleplerini getir
    /// </summary>
    [HttpGet("insurance")]
    public async Task<IActionResult> GetInsuranceRequests([FromQuery] string? status = null)
    {
        return await GetRequestsByType(ServiceTypes.Insurance.Id, status);
    }

    /// <summary>
    /// Sigorta talebi detayi
    /// </summary>
    [HttpGet("insurance/{id}")]
    public async Task<IActionResult> GetInsuranceRequest(int id)
    {
        return await GetRequestDetail(id, ServiceTypes.Insurance.Id);
    }

    /// <summary>
    /// Sigorta talebine teklif ver
    /// </summary>
    [HttpPost("insurance/{id}/quote")]
    public async Task<IActionResult> SubmitInsuranceQuote(int id, [FromBody] SubmitQuoteDto dto)
    {
        return await SubmitQuote(id, ServiceTypes.Insurance.Id, dto);
    }

    // ========================================
    // SURVEY REQUESTS (ServiceType = 4)
    // ========================================

    /// <summary>
    /// Gozetim taleplerini getir
    /// </summary>
    [HttpGet("survey")]
    public async Task<IActionResult> GetSurveyRequests([FromQuery] string? status = null)
    {
        return await GetRequestsByType(ServiceTypes.Survey.Id, status);
    }

    /// <summary>
    /// Gozetim talebi detayi
    /// </summary>
    [HttpGet("survey/{id}")]
    public async Task<IActionResult> GetSurveyRequest(int id)
    {
        return await GetRequestDetail(id, ServiceTypes.Survey.Id);
    }

    /// <summary>
    /// Gozetim talebine teklif ver
    /// </summary>
    [HttpPost("survey/{id}/quote")]
    public async Task<IActionResult> SubmitSurveyQuote(int id, [FromBody] SubmitQuoteDto dto)
    {
        return await SubmitQuote(id, ServiceTypes.Survey.Id, dto);
    }

    // ========================================
    // MY QUOTES (Verdigim teklifler)
    // ========================================

    /// <summary>
    /// Verdigim teklifleri getir
    /// </summary>
    [HttpGet("my-quotes")]
    public async Task<IActionResult> GetMyQuotes([FromQuery] int? serviceType = null)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Ok(new List<object>());

        var query = _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
                .ThenInclude(r => r!.Order)
                    .ThenInclude(o => o!.BuyerVendor)
            .Include(q => q.ServiceRequest)
                .ThenInclude(r => r!.OriginCountry)
            .Include(q => q.ServiceRequest)
                .ThenInclude(r => r!.DestinationCountry)
            .Where(q => q.ProviderVendorId == vendorId && !q.IsDeleted);

        if (serviceType.HasValue)
            query = query.Where(q => q.ServiceRequest!.ServiceType == serviceType.Value);

        var rawData = await query
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new
            {
                q.Id,
                q.ServiceRequestId,
                serviceType = q.ServiceRequest!.ServiceType,
                orderNumber = q.ServiceRequest.Order!.OrderNumber,
                buyerName = q.ServiceRequest.Order.BuyerVendor!.CompanyName,
                title = q.ServiceRequest.Title,
                origin = q.ServiceRequest.OriginCity + ", " + (q.ServiceRequest.OriginCountry != null ? q.ServiceRequest.OriginCountry.Name : ""),
                destination = q.ServiceRequest.DestinationCity + ", " + (q.ServiceRequest.DestinationCountry != null ? q.ServiceRequest.DestinationCountry.Name : ""),
                q.ServiceRequest.SurveyLocation,
                q.QuoteAmount,
                q.Currency,
                q.EstimatedDays,
                q.Status,
                q.CreatedAt
            })
            .ToListAsync();

        // Memory'de TypeItem lookup ve localization uygula
        var quotes = rawData.Select(q => new
        {
            q.Id,
            q.ServiceRequestId,
            q.serviceType,
            serviceTypeName = L(ServiceTypes.GetById(q.serviceType)?.NameResourceKey),
            q.orderNumber,
            q.buyerName,
            q.title,
            q.origin,
            q.destination,
            surveyLocation = q.SurveyLocation,
            q.QuoteAmount,
            q.Currency,
            q.EstimatedDays,
            q.Status,
            statusName = L(ServiceQuoteStatuses.GetById(q.Status)?.NameResourceKey),
            statusClass = ServiceQuoteStatuses.GetById(q.Status)?.CssClass ?? "bg-secondary",
            q.CreatedAt
        });

        return Ok(quotes);
    }

    /// <summary>
    /// Teklif geri cek
    /// </summary>
    [HttpPost("my-quotes/{id}/withdraw")]
    public async Task<IActionResult> WithdrawQuote(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized(new { message = "Vendor bulunamadi" });

        var quote = await _context.OrderServiceQuotes
            .FirstOrDefaultAsync(q => q.Id == id && q.ProviderVendorId == vendorId && !q.IsDeleted);

        if (quote == null)
            return NotFound(new { message = "Teklif bulunamadi" });

        if (quote.Status != ServiceQuoteStatuses.Pending.Id)
            return BadRequest(new { message = "Sadece bekleyen teklifler geri cekilebilir" });

        quote.Status = ServiceQuoteStatuses.Withdrawn.Id;
        quote.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Teklif geri cekildi" });
    }

    // ========================================
    // PRIVATE HELPER METHODS
    // ========================================

    private async Task<IActionResult> GetRequestsByType(int serviceType, string? status)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Ok(new List<object>());

        var query = _context.OrderServiceRequests
            .Include(r => r.Order)
                .ThenInclude(o => o!.BuyerVendor)
            .Include(r => r.OriginCountry)
            .Include(r => r.DestinationCountry)
            .Include(r => r.Quotes.Where(q => !q.IsDeleted))
            .Where(r => r.ServiceType == serviceType && !r.IsDeleted);

        // Status filter
        if (!string.IsNullOrEmpty(status))
        {
            var statusId = status.ToLower() switch
            {
                "open" => ServiceRequestStatuses.Open.Id,
                "quoted" => ServiceRequestStatuses.QuotesReceived.Id,
                "selected" => ServiceRequestStatuses.QuoteSelected.Id,
                _ => (int?)null
            };
            if (statusId.HasValue)
                query = query.Where(r => r.Status == statusId.Value);
        }
        else
        {
            // Default: Sadece acik talepleri goster
            query = query.Where(r => r.Status == ServiceRequestStatuses.Open.Id
                                  || r.Status == ServiceRequestStatuses.QuotesReceived.Id);
        }

        var rawData = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.OrderId,
                orderNumber = r.Order!.OrderNumber,
                buyerName = r.Order.BuyerVendor!.CompanyName,
                r.Title,
                r.Description,
                origin = r.OriginCity + ", " + (r.OriginCountry != null ? r.OriginCountry.Name : ""),
                destination = r.DestinationCity + ", " + (r.DestinationCountry != null ? r.DestinationCountry.Name : ""),
                r.WeightKg,
                r.VolumeM3,
                r.PackageCount,
                r.CargoValue,
                r.Currency,
                r.TransportMode,
                r.Incoterms,
                r.CustomsOperationType,
                r.HsCode,
                r.InsuranceType,
                r.SurveyTypes,
                r.SurveyLocation,
                r.PreferredSurveyDate,
                r.DesiredPickupDate,
                r.DesiredDeliveryDate,
                r.QuoteDeadline,
                r.Status,
                quoteCount = r.Quotes.Count(q => !q.IsDeleted && q.Status != ServiceQuoteStatuses.Withdrawn.Id),
                // Benim aktif tekliflerim (Pending veya Accepted)
                myQuotes = r.Quotes
                    .Where(q => q.ProviderVendorId == vendorId && !q.IsDeleted && q.Status != ServiceQuoteStatuses.Withdrawn.Id)
                    .Select(q => new {
                        q.Id,
                        q.QuoteAmount,
                        q.Currency,
                        q.CoverageAmount,
                        q.DeductiblePercent,
                        q.IncludedServices,
                        q.AdditionalCosts,
                        q.ValidUntil,
                        q.Notes,
                        q.Status
                    }).ToList(),
                // Aktif bekleyen teklif var mi
                hasActiveQuote = r.Quotes.Any(q => q.ProviderVendorId == vendorId && !q.IsDeleted && q.Status == ServiceQuoteStatuses.Pending.Id),
                r.CreatedAt
            })
            .ToListAsync();

        // Memory'de TypeItem lookup ve localization uygula
        var requests = rawData.Select(r => new
        {
            r.Id,
            r.OrderId,
            r.orderNumber,
            r.buyerName,
            r.Title,
            r.Description,
            r.origin,
            r.destination,
            r.WeightKg,
            r.VolumeM3,
            r.PackageCount,
            r.CargoValue,
            r.Currency,
            r.TransportMode,
            transportModeName = r.TransportMode.HasValue ? L(TransportModes.GetById(r.TransportMode.Value)?.NameResourceKey) : "",
            r.Incoterms,
            r.CustomsOperationType,
            customsOperationName = r.CustomsOperationType.HasValue ? L(CustomsOperationTypes.GetById(r.CustomsOperationType.Value)?.NameResourceKey) : "",
            r.HsCode,
            r.InsuranceType,
            insuranceTypeName = r.InsuranceType.HasValue ? L(InsuranceTypes.GetById(r.InsuranceType.Value)?.NameResourceKey) : "",
            r.SurveyTypes,
            surveyTypeName = GetSurveyTypeNames(r.SurveyTypes),
            r.SurveyLocation,
            preferredDate = r.PreferredSurveyDate,
            r.DesiredPickupDate,
            r.DesiredDeliveryDate,
            r.QuoteDeadline,
            r.Status,
            statusName = L(ServiceRequestStatuses.GetById(r.Status)?.NameResourceKey),
            statusClass = ServiceRequestStatuses.GetById(r.Status)?.CssClass ?? "bg-secondary",
            r.quoteCount,
            r.myQuotes,
            r.hasActiveQuote,
            r.CreatedAt
        });

        return Ok(requests);
    }

    private async Task<IActionResult> GetRequestDetail(int id, int serviceType)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized(new { message = "Vendor bulunamadi" });

        var request = await _context.OrderServiceRequests
            .Include(r => r.Order)
                .ThenInclude(o => o!.BuyerVendor)
            .Include(r => r.Order)
                .ThenInclude(o => o!.Items)
                    .ThenInclude(i => i.Product)
            .Include(r => r.OriginCountry)
            .Include(r => r.DestinationCountry)
            .Include(r => r.Quotes.Where(q => q.ProviderVendorId == vendorId && !q.IsDeleted))
            .FirstOrDefaultAsync(r => r.Id == id && r.ServiceType == serviceType && !r.IsDeleted);

        if (request == null)
            return NotFound(new { message = "Talep bulunamadi" });

        var result = new
        {
            request.Id,
            request.OrderId,
            orderNumber = request.Order?.OrderNumber,
            buyerName = request.Order?.BuyerVendor?.CompanyName,
            request.Title,
            request.Description,
            request.WeightKg,
            request.VolumeM3,
            request.PackageCount,
            request.CargoValue,
            request.Currency,
            originCountry = request.OriginCountry?.Name,
            request.OriginCity,
            request.OriginAddress,
            destinationCountry = request.DestinationCountry?.Name,
            request.DestinationCity,
            request.DestinationAddress,
            request.TransportMode,
            transportModeName = request.TransportMode.HasValue ? L(TransportModes.GetById(request.TransportMode.Value)?.NameResourceKey) : "",
            request.Incoterms,
            request.CustomsOperationType,
            customsOperationName = request.CustomsOperationType.HasValue ? L(CustomsOperationTypes.GetById(request.CustomsOperationType.Value)?.NameResourceKey) : "",
            request.HsCode,
            request.InsuranceType,
            insuranceTypeName = request.InsuranceType.HasValue ? L(InsuranceTypes.GetById(request.InsuranceType.Value)?.NameResourceKey) : "",
            request.SurveyTypes,
            surveyTypeName = GetSurveyTypeNames(request.SurveyTypes),
            request.SurveyLocation,
            preferredDate = request.PreferredSurveyDate,
            request.DesiredPickupDate,
            request.DesiredDeliveryDate,
            request.QuoteDeadline,
            request.Status,
            statusName = L(ServiceRequestStatuses.GetById(request.Status)?.NameResourceKey),
            request.CreatedAt,
            orderItems = request.Order?.Items?.Select(i => new
            {
                productName = i.Product?.Name,
                i.Quantity,
                i.Unit
            }),
            myQuote = request.Quotes.FirstOrDefault()
        };

        return Ok(result);
    }

    private async Task<IActionResult> SubmitQuote(int requestId, int serviceType, SubmitQuoteDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized(new { message = "Vendor bulunamadi" });

        var request = await _context.OrderServiceRequests
            .Include(r => r.Quotes)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.ServiceType == serviceType && !r.IsDeleted);

        if (request == null)
            return NotFound(new { message = "Talep bulunamadi" });

        if (request.Status == ServiceRequestStatuses.QuoteSelected.Id)
            return BadRequest(new { message = "Bu talep icin zaten teklif secilmis" });

        // Yeni teklif olustur (ayni talebe birden fazla teklif verilebilir)
        var quote = new OrderServiceQuote
        {
            ServiceRequestId = requestId,
            ProviderVendorId = vendorId.Value,
            QuoteAmount = dto.QuoteAmount,
            Currency = dto.Currency ?? "TRY",
            EstimatedDays = dto.EstimatedDays,
            Notes = dto.Notes,
            IncludedServices = dto.IncludedServices,
            AdditionalCosts = dto.AdditionalCosts,
            TermsAndConditions = dto.TermsAndConditions,
            ValidUntil = dto.ValidUntil.ToUtcSafe(),
            TransportModes = dto.TransportModes != null && dto.TransportModes.Any()
                ? string.Join(",", dto.TransportModes)
                : null,
            CarrierName = dto.CarrierName,
            TransitStops = dto.TransitStops,
            CoverageAmount = dto.CoverageAmount,
            DeductiblePercent = dto.DeductiblePercent,
            CoverageDetails = dto.CoverageDetails,
            SurveyTypes = dto.SurveyTypes != null && dto.SurveyTypes.Any()
                ? string.Join(",", dto.SurveyTypes)
                : null,
            Status = ServiceQuoteStatuses.Pending.Id
        };
        _context.OrderServiceQuotes.Add(quote);

        // Talep durumunu guncelle
        if (request.Status == ServiceRequestStatuses.Open.Id)
        {
            request.Status = ServiceRequestStatuses.QuotesReceived.Id;
        }

        await _context.SaveChangesAsync();

        // Checkout adimini guncelle (teklif geldi)
        if (request.OrderId > 0)
        {
            await _orchestrationService.UpdateCheckoutStepOnQuoteReceivedAsync(request.OrderId);
        }

        return Ok(new { message = "Teklifiniz gonderildi" });
    }

    // Localization helper
    private string L(string? key) => !string.IsNullOrEmpty(key) ? _localization.T(key) : "";

    // SurveyTypes helper - comma-separated IDs to localized names
    private string GetSurveyTypeNames(string? surveyTypes)
    {
        if (string.IsNullOrEmpty(surveyTypes)) return "";
        var ids = surveyTypes.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var names = ids
            .Select(id => int.TryParse(id.Trim(), out var typeId) ? SurveyTypes.GetById(typeId) : null)
            .Where(t => t != null)
            .Select(t => L(t!.NameResourceKey));
        return string.Join(", ", names);
    }
}

/// <summary>
/// Teklif gonderme DTO
/// </summary>
public class SubmitQuoteDto
{
    public decimal QuoteAmount { get; set; }
    public string? Currency { get; set; }
    public int? EstimatedDays { get; set; }
    public string? Notes { get; set; }
    public string? IncludedServices { get; set; }
    public string? AdditionalCosts { get; set; }
    public string? TermsAndConditions { get; set; }
    public DateTime? ValidUntil { get; set; }
    // Lojistik ozel
    public List<int>? TransportModes { get; set; }
    public string? CarrierName { get; set; }
    public int? TransitStops { get; set; }
    // Sigorta ozel
    public decimal? CoverageAmount { get; set; }
    public decimal? DeductiblePercent { get; set; }
    public string? CoverageDetails { get; set; }
    // Gozetim ozel
    public List<int>? SurveyTypes { get; set; }
}
