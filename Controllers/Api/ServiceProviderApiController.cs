using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.ServiceProvider;
using Bridgo.DTOs.Notification;
using Bridgo.Extensions;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Servis saglayici (Tasimaci, Gumruk, Sigorta, Gozetim) API
/// Talep goruntuleme ve teklif verme
/// </summary>
[Authorize]
[ApiController]
[Route("api/provider")]
public class ServiceProviderApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public ServiceProviderApiController(
        ApplicationDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
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

    private int GetVendorId()
    {
        return GetUserVendorIdAsync().GetAwaiter().GetResult() ?? 0;
    }

    /// <summary>
    /// Capability'e gore servis saglayici tipi getir
    /// </summary>
    private async Task<int?> GetProviderServiceTypeAsync(int vendorId)
    {
        var capabilityIds = await _context.VendorCapabilityMappings
            .Where(m => m.VendorId == vendorId)
            .Select(m => m.CapabilityId)
            .ToListAsync();

        // Capability ID'ye gore hizmet tipi
        if (capabilityIds.Contains(Capabilities.Ids.Carrier)) return 1;   // Logistics
        if (capabilityIds.Contains(Capabilities.Ids.Customs)) return 2;   // Customs
        if (capabilityIds.Contains(Capabilities.Ids.Insurance)) return 3; // Insurance
        if (capabilityIds.Contains(Capabilities.Ids.Survey)) return 4;    // Survey

        return null;
    }

    /// <summary>
    /// Bana gelen servis taleplerini listele
    /// Capability'e gore filtrelenir
    /// </summary>
    [HttpGet("requests")]
    public async Task<IActionResult> GetServiceRequests([FromQuery] int? serviceType = null)
    {
        var vendorId = GetVendorId();
        if (vendorId == 0)
            return Unauthorized();

        // Hangi hizmet tiplerini verebiliyoruz?
        var capabilityIds = await _context.VendorCapabilityMappings
            .Where(m => m.VendorId == vendorId)
            .Select(m => m.CapabilityId)
            .ToListAsync();

        var allowedServiceTypes = new List<int>();
        if (capabilityIds.Contains(Capabilities.Ids.Carrier)) allowedServiceTypes.Add(1);
        if (capabilityIds.Contains(Capabilities.Ids.Customs)) allowedServiceTypes.Add(2);
        if (capabilityIds.Contains(Capabilities.Ids.Insurance)) allowedServiceTypes.Add(3);
        if (capabilityIds.Contains(Capabilities.Ids.Survey)) allowedServiceTypes.Add(4);

        if (!allowedServiceTypes.Any())
            return Ok(new { success = true, data = new List<ServiceRequestListDto>() });

        // Belirli bir servis tipi secildiyse
        if (serviceType.HasValue && allowedServiceTypes.Contains(serviceType.Value))
            allowedServiceTypes = new List<int> { serviceType.Value };

        var query = _context.OrderServiceRequests
            .Include(sr => sr.Order)
                .ThenInclude(o => o!.BuyerVendor)
            .Include(sr => sr.Order)
                .ThenInclude(o => o!.ShippingAddress)
                    .ThenInclude(a => a!.CountryEntity)
            .Include(sr => sr.Quotes.Where(q => !q.IsDeleted))
            .Where(sr => !sr.IsDeleted &&
                        allowedServiceTypes.Contains(sr.ServiceType) &&
                        sr.Status <= 2) // Open veya QuotesReceived
            .OrderByDescending(sr => sr.CreatedAt);

        var requests = await query.Select(sr => new ServiceRequestListDto
        {
            Id = sr.Id,
            OrderId = sr.OrderId,
            OrderNumber = sr.Order!.OrderNumber,
            ServiceType = sr.ServiceType,
            ServiceTypeName = ServiceTypes.GetById(sr.ServiceType)!.Description,
            ServiceTypeIcon = ServiceTypes.GetById(sr.ServiceType)!.Icon,

            BuyerVendorId = sr.Order.BuyerVendorId,
            BuyerName = sr.Order.BuyerVendor!.CompanyName,

            OriginCity = sr.OriginCity,
            OriginCountry = sr.OriginCountryId.HasValue ?
                _context.Countries.FirstOrDefault(c => c.Id == sr.OriginCountryId)!.Name : null,
            DestinationCity = sr.DestinationCity,
            DestinationCountry = sr.Order.ShippingAddress != null && sr.Order.ShippingAddress.CountryEntity != null ?
                sr.Order.ShippingAddress.CountryEntity.Name : null,

            WeightKg = sr.WeightKg,
            VolumeM3 = sr.VolumeM3,
            CargoValue = sr.CargoValue,
            Currency = sr.Currency,
            HsCode = sr.HsCode,

            TransportMode = sr.TransportMode,
            Incoterms = sr.Incoterms,

            DesiredPickupDate = sr.DesiredPickupDate,
            DesiredDeliveryDate = sr.DesiredDeliveryDate,

            Status = sr.Status,
            QuoteCount = sr.Quotes.Count(q => !q.IsDeleted),
            HasMyQuote = sr.Quotes.Any(q => q.ProviderVendorId == vendorId && !q.IsDeleted),
            MyQuoteId = sr.Quotes.Where(q => q.ProviderVendorId == vendorId && !q.IsDeleted).Select(q => (int?)q.Id).FirstOrDefault(),
            MyQuoteAmount = sr.Quotes.Where(q => q.ProviderVendorId == vendorId && !q.IsDeleted).Select(q => (decimal?)q.QuoteAmount).FirstOrDefault(),

            CreatedAt = sr.CreatedAt
        }).ToListAsync();

        // Post-process to add derived fields
        foreach (var r in requests)
        {
            r.TransportModeName = r.TransportMode.HasValue ? TransportModes.GetById(r.TransportMode.Value)?.Description : null;
            r.StatusName = ServiceRequestStatuses.GetById(r.Status)?.Description ?? "";
        }

        return Ok(new { success = true, data = requests });
    }

    /// <summary>
    /// Servis talebi detayi
    /// </summary>
    [HttpGet("requests/{id}")]
    public async Task<IActionResult> GetServiceRequest(int id)
    {
        var vendorId = GetVendorId();
        if (vendorId == 0)
            return Unauthorized();

        var sr = await _context.OrderServiceRequests
            .Include(r => r.Order)
                .ThenInclude(o => o!.BuyerVendor)
            .Include(r => r.Order)
                .ThenInclude(o => o!.Items)
                    .ThenInclude(i => i.Product)
            .Include(r => r.Order)
                .ThenInclude(o => o!.ShippingAddress)
                    .ThenInclude(a => a!.CountryEntity)
            .Include(r => r.Quotes.Where(q => !q.IsDeleted))
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (sr == null)
            return NotFound(new { success = false, message = "Talep bulunamadi" });

        // Yetkili mi kontrol et
        var allowedServiceTypes = await GetAllowedServiceTypesAsync(vendorId);
        if (!allowedServiceTypes.Contains(sr.ServiceType))
            return Forbid();

        var detail = new ServiceRequestDetailDto
        {
            Id = sr.Id,
            OrderId = sr.OrderId,
            OrderNumber = sr.Order!.OrderNumber,
            ServiceType = sr.ServiceType,
            ServiceTypeName = ServiceTypes.GetById(sr.ServiceType)!.Description,
            ServiceTypeIcon = ServiceTypes.GetById(sr.ServiceType)!.Icon,

            BuyerVendorId = sr.Order.BuyerVendorId,
            BuyerName = sr.Order.BuyerVendor!.CompanyName,

            OriginCity = sr.OriginCity,
            OriginCountry = sr.OriginCountryId.HasValue ?
                (await _context.Countries.FirstOrDefaultAsync(c => c.Id == sr.OriginCountryId))?.Name : null,
            OriginFullAddress = sr.OriginAddress,
            DestinationCity = sr.DestinationCity,
            DestinationCountry = sr.Order.ShippingAddress?.CountryEntity?.Name,
            DestinationFullAddress = sr.DestinationAddress,

            WeightKg = sr.WeightKg,
            VolumeM3 = sr.VolumeM3,
            CargoValue = sr.CargoValue,
            Currency = sr.Currency,
            HsCode = sr.HsCode,

            TransportMode = sr.TransportMode,
            TransportModeName = sr.TransportMode.HasValue ?
                TransportModes.GetById(sr.TransportMode.Value)?.Description : null,
            Incoterms = sr.Incoterms,

            DesiredPickupDate = sr.DesiredPickupDate,
            DesiredDeliveryDate = sr.DesiredDeliveryDate,
            Notes = sr.Description,

            CustomsOperationType = sr.CustomsOperationType,
            CustomsOperationTypeName = sr.CustomsOperationType.HasValue ?
                CustomsOperationTypes.GetById(sr.CustomsOperationType.Value)?.Description : null,
            InsuranceType = sr.InsuranceType,
            InsuranceTypeName = sr.InsuranceType.HasValue ?
                InsuranceTypes.GetById(sr.InsuranceType.Value)?.Description : null,

            Status = sr.Status,
            StatusName = ServiceRequestStatuses.GetById(sr.Status)?.Description ?? "",
            QuoteCount = sr.Quotes.Count(q => !q.IsDeleted),
            HasMyQuote = sr.Quotes.Any(q => q.ProviderVendorId == vendorId && !q.IsDeleted),
            MyQuoteId = sr.Quotes.FirstOrDefault(q => q.ProviderVendorId == vendorId && !q.IsDeleted)?.Id,
            MyQuoteAmount = sr.Quotes.FirstOrDefault(q => q.ProviderVendorId == vendorId && !q.IsDeleted)?.QuoteAmount,

            CreatedAt = sr.CreatedAt,

            OrderItems = sr.Order.Items.Select(i => new ServiceRequestItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "",
                ProductSku = i.Product?.SKU,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                Currency = sr.Currency ?? "TRY"
            }).ToList()
        };

        return Ok(new { success = true, data = detail });
    }

    /// <summary>
    /// Teklif ver
    /// </summary>
    [HttpPost("quotes")]
    public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteDto dto)
    {
        var vendorId = GetVendorId();
        if (vendorId == 0)
            return Unauthorized();

        var serviceRequest = await _context.OrderServiceRequests
            .Include(sr => sr.Order)
                .ThenInclude(o => o!.BuyerVendor)
            .Include(sr => sr.Quotes)
            .FirstOrDefaultAsync(sr => sr.Id == dto.ServiceRequestId && !sr.IsDeleted);

        if (serviceRequest == null)
            return NotFound(new { success = false, message = "Talep bulunamadi" });

        // Yetkili mi kontrol et
        var allowedServiceTypes = await GetAllowedServiceTypesAsync(vendorId);
        if (!allowedServiceTypes.Contains(serviceRequest.ServiceType))
            return Forbid();

        // Daha once teklif verilmis mi?
        if (serviceRequest.Quotes.Any(q => q.ProviderVendorId == vendorId && !q.IsDeleted))
            return BadRequest(new { success = false, message = "Bu talep icin zaten teklif vermissiniz" });

        // Talep kapali mi?
        if (serviceRequest.Status >= 3)
            return BadRequest(new { success = false, message = "Bu talep artik teklif kabul etmiyor" });

        var quote = new OrderServiceQuote
        {
            ServiceRequestId = dto.ServiceRequestId,
            ProviderVendorId = vendorId,
            QuoteAmount = dto.QuoteAmount,
            Currency = dto.Currency,
            EstimatedDays = dto.EstimatedDays,
            IncludedServices = dto.IncludedServices,
            Notes = dto.Notes,
            ValidUntil = dto.ValidUntil?.ToUtcSafe() ?? DateTime.UtcNow.AddDays(7),
            Status = 1 // Pending
        };

        _context.OrderServiceQuotes.Add(quote);

        // ServiceRequest durumunu guncelle
        if (serviceRequest.Status == 1) // Open
        {
            serviceRequest.Status = 2; // QuotesReceived
        }

        await _context.SaveChangesAsync();

        // Aliciya bildirim gonder
        var serviceTypeName = ServiceTypes.GetById(serviceRequest.ServiceType)?.Description ?? "Hizmet";
        var providerVendor = await _context.Vendors.FindAsync(vendorId);

        await _notificationService.CreateAsync(new NotificationCreateDto
        {
            VendorId = serviceRequest.Order!.BuyerVendorId,
            Type = NotificationType.NewServiceQuote,
            Title = $"Yeni {serviceTypeName} Teklifi",
            Message = $"{providerVendor?.CompanyName} firmasindan yeni bir {serviceTypeName.ToLower()} teklifi geldi.",
            ActionUrl = $"/Checkout/Quotes/{serviceRequest.OrderId}",
            Icon = "bi-cash-coin"
        });

        return Ok(new { success = true, message = "Teklif gonderildi", data = new { id = quote.Id } });
    }

    /// <summary>
    /// Teklif guncelle
    /// </summary>
    [HttpPut("quotes/{id}")]
    public async Task<IActionResult> UpdateQuote(int id, [FromBody] UpdateQuoteDto dto)
    {
        var vendorId = GetVendorId();
        if (vendorId == 0)
            return Unauthorized();

        var quote = await _context.OrderServiceQuotes
            .FirstOrDefaultAsync(q => q.Id == id && q.ProviderVendorId == vendorId && !q.IsDeleted);

        if (quote == null)
            return NotFound(new { success = false, message = "Teklif bulunamadi" });

        // Teklif durumu uygun mu?
        if (quote.Status != 1) // Pending degilse
            return BadRequest(new { success = false, message = "Bu teklif artik guncellenemez" });

        quote.QuoteAmount = dto.QuoteAmount;
        quote.Currency = dto.Currency;
        quote.EstimatedDays = dto.EstimatedDays;
        quote.IncludedServices = dto.IncludedServices;
        quote.Notes = dto.Notes;
        quote.ValidUntil = dto.ValidUntil?.ToUtcSafe() ?? DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Teklif guncellendi" });
    }

    /// <summary>
    /// Teklif geri cek
    /// </summary>
    [HttpPost("quotes/{id}/withdraw")]
    public async Task<IActionResult> WithdrawQuote(int id)
    {
        var vendorId = GetVendorId();
        if (vendorId == 0)
            return Unauthorized();

        var quote = await _context.OrderServiceQuotes
            .FirstOrDefaultAsync(q => q.Id == id && q.ProviderVendorId == vendorId && !q.IsDeleted);

        if (quote == null)
            return NotFound(new { success = false, message = "Teklif bulunamadi" });

        if (quote.Status != 1) // Pending degilse
            return BadRequest(new { success = false, message = "Bu teklif geri cekilemez" });

        quote.Status = 5; // Withdrawn

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Teklif geri cekildi" });
    }

    /// <summary>
    /// Verdigim teklifleri listele
    /// </summary>
    [HttpGet("my-quotes")]
    public async Task<IActionResult> GetMyQuotes([FromQuery] int? status = null)
    {
        var vendorId = GetVendorId();
        if (vendorId == 0)
            return Unauthorized();

        var query = _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
                .ThenInclude(sr => sr!.Order)
                    .ThenInclude(o => o!.BuyerVendor)
            .Where(q => q.ProviderVendorId == vendorId && !q.IsDeleted);

        if (status.HasValue)
            query = query.Where(q => q.Status == status.Value);

        var quotes = await query
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new MyQuoteListDto
            {
                Id = q.Id,
                ServiceRequestId = q.ServiceRequestId,
                OrderId = q.ServiceRequest!.OrderId,
                OrderNumber = q.ServiceRequest.Order!.OrderNumber,
                ServiceType = q.ServiceRequest.ServiceType,
                ServiceTypeName = ServiceTypes.GetById(q.ServiceRequest.ServiceType)!.Description,

                BuyerName = q.ServiceRequest.Order.BuyerVendor!.CompanyName,

                QuoteAmount = q.QuoteAmount,
                Currency = q.Currency,
                EstimatedDays = q.EstimatedDays,

                Status = q.Status,
                StatusName = ServiceQuoteStatuses.GetById(q.Status)!.Description,
                IsSelected = q.ServiceRequest.SelectedQuoteId == q.Id,

                CreatedAt = q.CreatedAt,
                ValidUntil = q.ValidUntil
            })
            .ToListAsync();

        return Ok(new { success = true, data = quotes });
    }

    /// <summary>
    /// Aktif ve tamamlanmis islerimi listele
    /// OrderParticipant uzerinden sorgulanir
    /// </summary>
    [HttpGet("my-jobs")]
    public async Task<IActionResult> GetMyJobs(
        [FromQuery] int? serviceType = null,
        [FromQuery] string? status = null) // "active" veya "completed"
    {
        var vendorId = GetVendorId();
        if (vendorId == 0)
            return Unauthorized();

        // ServiceType'a gore ParticipantRole eslestir
        // ServiceType: 1=Logistics, 2=Customs, 3=Insurance, 4=Survey
        // ParticipantRole: 2=LogisticsProvider, 3=CustomsBroker, 4=InsuranceProvider, 5=SurveyProvider
        var roleMapping = new Dictionary<int, int>
        {
            { 1, 2 }, // Logistics -> LogisticsProvider
            { 2, 3 }, // Customs -> CustomsBroker
            { 3, 4 }, // Insurance -> InsuranceProvider
            { 4, 5 }  // Survey -> SurveyProvider
        };

        var query = _context.OrderParticipants
            .Include(p => p.Order)
                .ThenInclude(o => o!.BuyerVendor)
            .Include(p => p.Order)
                .ThenInclude(o => o!.SellerVendor)
            .Include(p => p.Order)
                .ThenInclude(o => o!.ShippingAddress)
                    .ThenInclude(a => a!.CountryEntity)
            .Include(p => p.ServiceQuote)
                .ThenInclude(q => q!.ServiceRequest)
            .Where(p => p.VendorId == vendorId && !p.IsDeleted && p.Role >= 2); // Seller degil

        // ServiceType filtresi
        if (serviceType.HasValue && roleMapping.ContainsKey(serviceType.Value))
        {
            var targetRole = roleMapping[serviceType.Value];
            query = query.Where(p => p.Role == targetRole);
        }

        // Status filtresi
        if (status == "active")
        {
            // Pending(1) veya Active(2)
            query = query.Where(p => p.Status <= 2 && !p.IsTaskCompleted);
        }
        else if (status == "completed")
        {
            // Completed(3) veya IsTaskCompleted
            query = query.Where(p => p.Status == 3 || p.IsTaskCompleted);
        }

        var jobs = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProviderJobListDto
            {
                Id = p.Id,
                OrderId = p.OrderId,
                OrderNumber = p.Order!.OrderNumber,
                Role = p.Role,

                BuyerVendorId = p.Order.BuyerVendorId,
                BuyerName = p.Order.BuyerVendor!.CompanyName,
                SellerName = p.Order.SellerVendor!.CompanyName,

                DestinationCity = p.Order.ShippingAddress != null ? p.Order.ShippingAddress.City : null,
                DestinationCountry = p.Order.ShippingAddress != null && p.Order.ShippingAddress.CountryEntity != null
                    ? p.Order.ShippingAddress.CountryEntity.Name : null,

                Amount = p.Amount,
                Currency = p.Currency,
                NetAmount = p.NetAmount,

                Status = p.Status,
                IsTaskCompleted = p.IsTaskCompleted,
                TaskCompletedAt = p.TaskCompletedAt,

                // ServiceRequest bilgileri
                ServiceRequestId = p.ServiceQuote != null ? (int?)p.ServiceQuote.ServiceRequestId : null,
                ServiceType = p.ServiceQuote != null && p.ServiceQuote.ServiceRequest != null
                    ? (int?)p.ServiceQuote.ServiceRequest.ServiceType : null,

                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        // Post-process: TypeDefinitions lookup'lari
        foreach (var job in jobs)
        {
            job.RoleName = ParticipantRoles.GetById(job.Role)?.Description ?? "";
            job.StatusName = ParticipantStatuses.GetById(job.Status)?.Description ?? "";
            job.StatusCssClass = ParticipantStatuses.GetById(job.Status)?.CssClass ?? "bg-secondary";
            if (job.ServiceType.HasValue)
                job.ServiceTypeName = ServiceTypes.GetById(job.ServiceType.Value)?.Description;
        }

        return Ok(new { success = true, data = jobs });
    }

    /// <summary>
    /// Is detayini getir
    /// </summary>
    [HttpGet("my-jobs/{id}")]
    public async Task<IActionResult> GetJobDetail(int id)
    {
        var vendorId = GetVendorId();
        if (vendorId == 0)
            return Unauthorized();

        var participant = await _context.OrderParticipants
            .Include(p => p.Order)
                .ThenInclude(o => o!.BuyerVendor)
            .Include(p => p.Order)
                .ThenInclude(o => o!.SellerVendor)
            .Include(p => p.Order)
                .ThenInclude(o => o!.Items)
                    .ThenInclude(i => i.Product)
            .Include(p => p.Order)
                .ThenInclude(o => o!.ShippingAddress)
                    .ThenInclude(a => a!.CountryEntity)
            .Include(p => p.ServiceQuote)
                .ThenInclude(q => q!.ServiceRequest)
            .Include(p => p.Tasks.Where(t => !t.IsDeleted))
            .FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendorId && !p.IsDeleted);

        if (participant == null)
            return NotFound(new { success = false, message = "Is bulunamadi" });

        var order = participant.Order!;
        var serviceRequest = participant.ServiceQuote?.ServiceRequest;

        var detail = new ProviderJobDetailDto
        {
            Id = participant.Id,
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Role = participant.Role,
            RoleName = ParticipantRoles.GetById(participant.Role)?.Description ?? "",

            // Firma bilgileri
            BuyerVendorId = order.BuyerVendorId,
            BuyerName = order.BuyerVendor!.CompanyName,
            BuyerEmail = order.BuyerVendor.Email,
            BuyerPhone = order.BuyerVendor.Phone,

            SellerVendorId = order.SellerVendorId,
            SellerName = order.SellerVendor!.CompanyName,

            // Adres
            DestinationAddress = order.ShippingAddress != null
                ? $"{order.ShippingAddress.AddressLine}, {order.ShippingAddress.City}, {order.ShippingAddress.CountryEntity?.Name}"
                : null,

            // Tutar
            Amount = participant.Amount,
            Currency = participant.Currency,
            CommissionAmount = participant.CommissionAmount,
            NetAmount = participant.NetAmount,

            // Durum
            Status = participant.Status,
            StatusName = ParticipantStatuses.GetById(participant.Status)?.Description ?? "",
            IsTaskCompleted = participant.IsTaskCompleted,
            TaskCompletedAt = participant.TaskCompletedAt,
            IsPaid = participant.IsPaid,
            PaidAt = participant.PaidAt,

            // Servis detaylari
            ServiceRequestId = serviceRequest?.Id,
            ServiceType = serviceRequest?.ServiceType,
            ServiceTypeName = serviceRequest != null
                ? ServiceTypes.GetById(serviceRequest.ServiceType)!.Description : null,

            // Gumruk ozel
            CustomsOperationType = serviceRequest?.CustomsOperationType,
            CustomsOperationTypeName = serviceRequest?.CustomsOperationType != null
                ? CustomsOperationTypes.GetById(serviceRequest.CustomsOperationType.Value)?.Description : null,

            // Sigorta ozel
            InsuranceType = serviceRequest?.InsuranceType,
            InsuranceTypeName = serviceRequest?.InsuranceType != null
                ? InsuranceTypes.GetById(serviceRequest.InsuranceType.Value)?.Description : null,
            CoverageAmount = participant.ServiceQuote?.CoverageAmount,

            // Kargo bilgileri
            CargoValue = serviceRequest?.CargoValue,
            WeightKg = serviceRequest?.WeightKg,
            HsCode = serviceRequest?.HsCode,

            Notes = serviceRequest?.Description,
            CreatedAt = participant.CreatedAt,

            // Siparis kalemleri
            OrderItems = order.Items.Select(i => new ProviderJobOrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "",
                ProductSku = i.Product?.SKU,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList(),

            // Gorevler
            Tasks = participant.Tasks.Select(t => new ProviderJobTaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                DueDate = t.DueDate,
                CompletedAt = t.CompletedAt
            }).ToList()
        };

        return Ok(new { success = true, data = detail });
    }

    /// <summary>
    /// Gorevi tamamla olarak isaretle
    /// </summary>
    [HttpPost("my-jobs/{id}/complete")]
    public async Task<IActionResult> CompleteJob(int id)
    {
        var vendorId = GetVendorId();
        if (vendorId == 0)
            return Unauthorized();

        var participant = await _context.OrderParticipants
            .FirstOrDefaultAsync(p => p.Id == id && p.VendorId == vendorId && !p.IsDeleted);

        if (participant == null)
            return NotFound(new { success = false, message = "Is bulunamadi" });

        if (participant.IsTaskCompleted)
            return BadRequest(new { success = false, message = "Bu is zaten tamamlanmis" });

        participant.IsTaskCompleted = true;
        participant.TaskCompletedAt = DateTime.UtcNow;
        participant.Status = 3; // Completed

        await _context.SaveChangesAsync();

        // Bildirim gonder
        var order = await _context.Orders
            .Include(o => o.BuyerVendor)
            .FirstOrDefaultAsync(o => o.Id == participant.OrderId);

        if (order != null)
        {
            var providerVendor = await _context.Vendors.FindAsync(vendorId);
            await _notificationService.CreateAsync(new NotificationCreateDto
            {
                VendorId = order.BuyerVendorId,
                Type = NotificationType.Info,
                Title = "Servis Tamamlandi",
                Message = $"{providerVendor?.CompanyName} siparisinizdeki gorevini tamamladi",
                ActionUrl = $"/Orders/Detail/{order.Id}"
            });
        }

        return Ok(new { success = true, message = "Is tamamlandi olarak isaretlendi" });
    }

    /// <summary>
    /// Yetkili servis tiplerini getir
    /// </summary>
    private async Task<List<int>> GetAllowedServiceTypesAsync(int vendorId)
    {
        var capabilityIds = await _context.VendorCapabilityMappings
            .Where(m => m.VendorId == vendorId)
            .Select(m => m.CapabilityId)
            .ToListAsync();

        var allowedServiceTypes = new List<int>();
        if (capabilityIds.Contains(Capabilities.Ids.Carrier)) allowedServiceTypes.Add(1);
        if (capabilityIds.Contains(Capabilities.Ids.Customs)) allowedServiceTypes.Add(2);
        if (capabilityIds.Contains(Capabilities.Ids.Insurance)) allowedServiceTypes.Add(3);
        if (capabilityIds.Contains(Capabilities.Ids.Survey)) allowedServiceTypes.Add(4);

        return allowedServiceTypes;
    }
}
