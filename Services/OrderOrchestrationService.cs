using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Siparis akisi orkestrasyon servisi implementasyonu
/// </summary>
public class OrderOrchestrationService : IOrderOrchestrationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderOrchestrationService> _logger;
    private readonly IRiskScoringService _riskScoringService;

    public OrderOrchestrationService(
        ApplicationDbContext context,
        ILogger<OrderOrchestrationService> logger,
        IRiskScoringService riskScoringService)
    {
        _context = context;
        _logger = logger;
        _riskScoringService = riskScoringService;
    }

    /// <inheritdoc />
    public async Task<QuoteAcceptedResult> OnServiceQuoteAcceptedAsync(int orderId, int serviceRequestId, int quoteId)
    {
        var result = new QuoteAcceptedResult();

        var order = await _context.Orders
            .Include(o => o.ServiceRequests)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} bulunamadi", orderId);
            return result;
        }

        var request = order.ServiceRequests.FirstOrDefault(r => r.Id == serviceRequestId);
        if (request == null)
        {
            _logger.LogWarning("ServiceRequest {RequestId} bulunamadi", serviceRequestId);
            return result;
        }

        // Lojistik teklifi kabul edildi mi?
        if (request.ServiceType == ServiceTypes.Logistics.Id)
        {
            _logger.LogInformation("Lojistik teklifi kabul edildi. Order: {OrderId}, Quote: {QuoteId}", orderId, quoteId);

            // Gozetim bekliyorsa, Survey request olustur
            if (order.SurveyTriggerStatus == SurveyTriggerStatuses.WaitingForLogistics.Id)
            {
                result.CreatedSurveyRequestId = await CreateSurveyRequestFromLogisticsAsync(orderId, serviceRequestId, quoteId);
                _logger.LogInformation("Survey request otomatik olusturuldu. SurveyRequestId: {SurveyRequestId}", result.CreatedSurveyRequestId);
            }
        }

        // Tum servisler secildi mi kontrol et
        result.AllServicesSelected = await CheckAllServicesSelectedAsync(orderId);

        if (result.AllServicesSelected)
        {
            _logger.LogInformation("Tum servisler secildi. Order: {OrderId}", orderId);

            // Finansman gerekiyorsa otomatik tetikle
            if (order.RequiresFinancing && !order.FinancingRequestId.HasValue)
            {
                result.FinancingRequestId = await TriggerFinancingIfNeededAsync(orderId);
                result.FinancingAvailable = false; // Zaten tetiklendi
            }
            else if (!order.RequiresFinancing)
            {
                // Kullaniciya finansman isteyip istemedigini sor
                result.FinancingAvailable = true;

                // Toplam tutari hesapla
                var itemsTotal = order.Items.Sum(i => i.TotalPrice);
                var servicesTotal = await _context.OrderServiceRequests
                    .Where(r => r.OrderId == orderId && r.SelectedQuoteId.HasValue && !r.IsDeleted)
                    .Include(r => r.SelectedQuote)
                    .SumAsync(r => r.SelectedQuote!.QuoteAmount);

                result.TotalAmount = itemsTotal + servicesTotal;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<int> CreateSurveyRequestFromLogisticsAsync(int orderId, int logisticsRequestId, int logisticsQuoteId)
    {
        var logisticsRequest = await _context.OrderServiceRequests
            .Include(r => r.Order)
            .FirstOrDefaultAsync(r => r.Id == logisticsRequestId);

        if (logisticsRequest == null)
            throw new InvalidOperationException($"Logistics request {logisticsRequestId} bulunamadi");

        var logisticsQuote = await _context.OrderServiceQuotes
            .Include(q => q.ProviderVendor)
            .FirstOrDefaultAsync(q => q.Id == logisticsQuoteId);

        if (logisticsQuote == null)
            throw new InvalidOperationException($"Logistics quote {logisticsQuoteId} bulunamadi");

        // Survey request olustur - Lojistik bilgilerini kullan
        var surveyRequest = new OrderServiceRequest
        {
            OrderId = orderId,
            ServiceType = ServiceTypes.Survey.Id,
            Title = $"Gozetim - {logisticsRequest.Order?.OrderNumber}",
            Description = $"Lojistik firması: {logisticsQuote.ProviderVendor?.CompanyName}\n" +
                         $"Taşıma modu: {GetTransportModeName(logisticsRequest.TransportMode)}\n" +
                         $"Rota: {logisticsRequest.OriginCity} → {logisticsRequest.DestinationCity}",

            // Lokasyon bilgileri lojistikten al
            OriginCountryId = logisticsRequest.OriginCountryId,
            OriginCity = logisticsRequest.OriginCity,
            OriginAddress = logisticsRequest.OriginAddress,
            DestinationCountryId = logisticsRequest.DestinationCountryId,
            DestinationCity = logisticsRequest.DestinationCity,
            DestinationAddress = logisticsRequest.DestinationAddress,

            // Kargo bilgileri
            WeightKg = logisticsRequest.WeightKg,
            VolumeM3 = logisticsRequest.VolumeM3,
            PackageCount = logisticsRequest.PackageCount,
            CargoValue = logisticsRequest.CargoValue,
            Currency = logisticsRequest.Currency,

            // Varsayilan gozetim tipleri (PreLoading + Loading)
            SurveyTypes = "1,2", // PreLoading, Loading
            SurveyLocation = logisticsRequest.OriginCity,
            PreferredSurveyDate = logisticsRequest.DesiredPickupDate,

            // Tarihler
            DesiredPickupDate = logisticsRequest.DesiredPickupDate,
            DesiredDeliveryDate = logisticsRequest.DesiredDeliveryDate,

            // Durum
            Status = ServiceRequestStatuses.Open.Id,

            // Bagimlilik bilgileri
            DependsOnServiceRequestId = logisticsRequestId,
            AutoCreated = true,
            TriggerSource = "LogisticsQuoteAccepted",

            CreatedBy = "System"
        };

        _context.OrderServiceRequests.Add(surveyRequest);

        // Order'in SurveyTriggerStatus'unu guncelle
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.SurveyTriggerStatus = SurveyTriggerStatuses.Created.Id;
        }

        await _context.SaveChangesAsync();

        return surveyRequest.Id;
    }

    /// <inheritdoc />
    public async Task<bool> CheckAllServicesSelectedAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.ServiceRequests)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return false;

        // Aktif (iptal edilmemis) servis request'leri kontrol et
        var activeRequests = order.ServiceRequests
            .Where(r => r.Status != ServiceRequestStatuses.Cancelled.Id && !r.IsDeleted)
            .ToList();

        if (!activeRequests.Any()) return true; // Servis yok, hepsi secili sayilir

        // Tum request'lerde teklif secili mi?
        var allSelected = activeRequests.All(r => r.Status == ServiceRequestStatuses.QuoteSelected.Id);

        if (allSelected && order.AllServicesSelectedAt == null)
        {
            order.AllServicesSelectedAt = DateTime.UtcNow;
            order.CheckoutStep = CheckoutSteps.QuotesSelected.Id;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tum servisler secildi. Order: {OrderId}, Tarih: {Date}, Step: QuotesSelected",
                orderId, order.AllServicesSelectedAt);
        }

        return allSelected;
    }

    /// <inheritdoc />
    public async Task<int?> TriggerFinancingIfNeededAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.ServiceRequests)
                .ThenInclude(r => r.SelectedQuote)
            .Include(o => o.BuyerVendor)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} bulunamadi", orderId);
            return null;
        }

        // Zaten finansman request var mi?
        if (order.FinancingRequestId.HasValue)
        {
            _logger.LogDebug("Order {OrderId} icin zaten finansman request mevcut: {FinancingRequestId}",
                orderId, order.FinancingRequestId);
            return order.FinancingRequestId;
        }

        // Tum servisler secildi mi?
        if (order.AllServicesSelectedAt == null)
        {
            _logger.LogDebug("Order {OrderId} icin henuz tum servisler secilmedi", orderId);
            return null;
        }

        // Toplam tutari hesapla
        var itemsTotal = order.Items.Sum(i => i.TotalPrice);
        var servicesTotal = order.ServiceRequests
            .Where(r => r.SelectedQuote != null)
            .Sum(r => r.SelectedQuote!.QuoteAmount);

        var totalAmount = itemsTotal + servicesTotal;

        // FinancingRequest olustur
        var financingRequest = new FinancingRequest
        {
            RequesterVendorId = order.BuyerVendorId,
            FinancingType = FinancingTypes.Order.Id,
            FinancingSubject = FinancingSubjects.ProductCost.Id,
            Title = $"Siparis Finansmani - {order.OrderNumber}",
            Description = $"Siparis No: {order.OrderNumber}\n" +
                         $"Urun Tutari: {itemsTotal:N2} {order.Currency}\n" +
                         $"Servis Tutari: {servicesTotal:N2} {order.Currency}\n" +
                         $"Toplam: {totalAmount:N2} {order.Currency}",
            RequestedAmount = totalAmount,
            Currency = order.Currency,
            TotalValue = totalAmount,
            RelatedOrderId = orderId,
            DurationDays = 30, // Varsayilan 30 gun
            Status = FinancingRequestStatuses.Open.Id,
            CollateralType = CollateralTypes.Order.Id,
            CollateralDescription = $"Siparis No: {order.OrderNumber}",
            CreatedBy = "System"
        };

        _context.FinancingRequests.Add(financingRequest);
        await _context.SaveChangesAsync();

        // Risk skorunu hesapla ve kaydet
        try
        {
            var riskScore = await _riskScoringService.UpdateRiskScoreAsync(financingRequest.Id);
            _logger.LogInformation("Risk skoru hesaplandi. FinancingRequest: {FinancingRequestId}, Score: {Score}",
                financingRequest.Id, riskScore);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Risk skoru hesaplanamadi. FinancingRequest: {FinancingRequestId}", financingRequest.Id);
        }

        // Order'a FinancingRequest'i bagla
        order.FinancingRequestId = financingRequest.Id;
        order.RequiresFinancing = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("FinancingRequest olusturuldu. Order: {OrderId}, FinancingRequest: {FinancingRequestId}, Tutar: {Amount} {Currency}",
            orderId, financingRequest.Id, totalAmount, order.Currency);

        return financingRequest.Id;
    }

    /// <inheritdoc />
    public async Task<int?> RequestFinancingAsync(int orderId, int buyerVendorId)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerVendorId == buyerVendorId);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} bulunamadi veya yetki yok", orderId);
            return null;
        }

        // Zaten finansman var mi?
        if (order.FinancingRequestId.HasValue)
        {
            return order.FinancingRequestId;
        }

        // Tum servisler secildi mi kontrol et
        var allSelected = await CheckAllServicesSelectedAsync(orderId);
        if (!allSelected)
        {
            _logger.LogWarning("Order {OrderId} icin henuz tum servisler secilmedi, finansman istenemez", orderId);
            return null;
        }

        // RequiresFinancing'i true yap ve finansman olustur
        order.RequiresFinancing = true;
        await _context.SaveChangesAsync();

        return await TriggerFinancingIfNeededAsync(orderId);
    }

    /// <inheritdoc />
    public async Task<FinancingStatusDto> GetFinancingStatusAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.ServiceRequests)
                .ThenInclude(r => r.SelectedQuote)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return new FinancingStatusDto();
        }

        var activeRequests = order.ServiceRequests
            .Where(r => r.Status != ServiceRequestStatuses.Cancelled.Id && !r.IsDeleted)
            .ToList();

        var selectedRequests = activeRequests
            .Where(r => r.Status == ServiceRequestStatuses.QuoteSelected.Id)
            .ToList();

        var itemsTotal = order.Items.Sum(i => i.TotalPrice);
        var servicesTotal = selectedRequests
            .Where(r => r.SelectedQuote != null)
            .Sum(r => r.SelectedQuote!.QuoteAmount);

        var allSelected = activeRequests.Count == selectedRequests.Count && activeRequests.Count > 0;

        return new FinancingStatusDto
        {
            RequiresFinancing = order.RequiresFinancing,
            AllServicesSelected = order.AllServicesSelectedAt.HasValue || (activeRequests.Count == 0),
            CanTriggerFinancing = (order.AllServicesSelectedAt.HasValue || activeRequests.Count == 0)
                                  && !order.FinancingRequestId.HasValue,
            ExistingFinancingRequestId = order.FinancingRequestId,
            ItemsTotal = itemsTotal,
            ServicesTotal = servicesTotal,
            TotalAmount = itemsTotal + servicesTotal,
            Currency = order.Currency,
            PendingServiceRequests = activeRequests.Count - selectedRequests.Count,
            SelectedServiceRequests = selectedRequests.Count
        };
    }

    /// <inheritdoc />
    public async Task<CheckoutProgressDto> GetCheckoutProgressAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.ServiceRequests)
                .ThenInclude(r => r.Quotes)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return new CheckoutProgressDto { CurrentStep = 0, StepName = "Siparis bulunamadi" };
        }

        var currentStep = CheckoutSteps.GetById(order.CheckoutStep) ?? CheckoutSteps.Default;

        var activeRequests = order.ServiceRequests
            .Where(r => r.Status != ServiceRequestStatuses.Cancelled.Id && !r.IsDeleted)
            .ToList();

        var requestsWithQuotes = activeRequests.Count(r => r.Quotes != null && r.Quotes.Any(q => !q.IsDeleted));
        var selectedRequests = activeRequests.Count(r => r.Status == ServiceRequestStatuses.QuoteSelected.Id);

        var itemsTotal = order.Items.Sum(i => i.TotalPrice);
        var servicesTotal = activeRequests
            .Where(r => r.SelectedQuote != null)
            .Sum(r => r.SelectedQuote!.QuoteAmount);

        var result = new CheckoutProgressDto
        {
            CurrentStep = order.CheckoutStep,
            StepName = currentStep.SystemName,
            StepDescription = currentStep.Description ?? "",
            StepCssClass = currentStep.CssClass ?? "",
            TotalAmount = itemsTotal + servicesTotal,
            Currency = order.Currency,
            TotalServiceRequests = activeRequests.Count,
            ServiceRequestsWithQuotes = requestsWithQuotes,
            SelectedServiceRequests = selectedRequests
        };

        // Tum adimlari olustur
        foreach (var step in CheckoutSteps.All)
        {
            result.Steps.Add(new CheckoutStepInfo
            {
                StepNumber = step.Id,
                Name = step.SystemName,
                Description = step.Description ?? "",
                Icon = step.Icon ?? "",
                CssClass = step.CssClass ?? "",
                IsCompleted = step.Id < order.CheckoutStep,
                IsCurrent = step.Id == order.CheckoutStep,
                IsPending = step.Id > order.CheckoutStep,
                CompletedAt = step.Id < order.CheckoutStep ? order.UpdatedAt : null
            });
        }

        return result;
    }

    /// <inheritdoc />
    public async Task UpdateCheckoutStepOnQuoteReceivedAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.ServiceRequests)
                .ThenInclude(r => r.Quotes)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return;

        var activeRequests = order.ServiceRequests
            .Where(r => r.Status != ServiceRequestStatuses.Cancelled.Id && !r.IsDeleted)
            .ToList();

        if (activeRequests.Count == 0) return;

        var requestsWithQuotes = activeRequests.Count(r => r.Quotes != null && r.Quotes.Any(q => !q.IsDeleted));

        // En az bir teklif geldiyse
        if (requestsWithQuotes > 0 && order.CheckoutStep < CheckoutSteps.QuotesReceived.Id)
        {
            order.CheckoutStep = CheckoutSteps.QuotesReceived.Id;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Checkout adimi guncellendi. Order: {OrderId}, Step: QuotesReceived", orderId);
        }
    }

    /// <inheritdoc />
    public async Task UpdateCheckoutStepAsync(int orderId, int step)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return;

        order.CheckoutStep = step;
        await _context.SaveChangesAsync();

        var stepInfo = CheckoutSteps.GetById(step);
        _logger.LogInformation("Checkout adimi guncellendi. Order: {OrderId}, Step: {StepName}",
            orderId, stepInfo?.SystemName ?? step.ToString());
    }

    // Helper: Transport mode adini getir
    private static string GetTransportModeName(int? transportMode)
    {
        if (!transportMode.HasValue) return "Belirtilmedi";
        return TransportModes.GetById(transportMode.Value)?.SystemName ?? "Bilinmiyor";
    }
}
