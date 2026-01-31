using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Checkout;
using Bridgo.DTOs.Notification;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Checkout islemleri - Sepetten Siparise donusum
/// </summary>
public class CheckoutService : ICheckoutService
{
    private readonly ApplicationDbContext _context;
    private readonly IOrderService _orderService;
    private readonly INotificationService _notificationService;

    public CheckoutService(
        ApplicationDbContext context,
        IOrderService orderService,
        INotificationService notificationService)
    {
        _context = context;
        _orderService = orderService;
        _notificationService = notificationService;
    }

    public async Task<CheckoutSummaryDto?> GetCheckoutSummaryAsync(int vendorId, int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images.Where(img => img.IsMain))
            .Include(c => c.Items)
                .ThenInclude(i => i.SellerVendor)
                    .ThenInclude(v => v.Warehouses.Where(w => w.IsDefault && !w.IsDeleted))
                        .ThenInclude(w => w.Address)
                            .ThenInclude(a => a!.CountryEntity)
            .Include(c => c.Items)
                .ThenInclude(i => i.DeliveryAddress)
                    .ThenInclude(a => a!.CountryEntity)
            .FirstOrDefaultAsync(c => c.VendorId == vendorId && c.UserId == userId &&
                c.Status == CartStatus.Active && !c.IsDeleted);

        if (cart == null || !cart.Items.Any(i => !i.IsDeleted))
            return null;

        var items = cart.Items.Where(i => !i.IsDeleted).ToList();

        // Satici bazli gruplama
        var vendorGroups = items
            .GroupBy(i => new { i.SellerVendorId, i.SellerVendor.CompanyName })
            .Select(g =>
            {
                var vendor = g.First().SellerVendor;
                var defaultWarehouse = vendor.Warehouses?.FirstOrDefault();

                return new CheckoutVendorGroupDto
                {
                    VendorId = g.Key.SellerVendorId,
                    VendorName = g.Key.CompanyName,
                    Items = g.Select(i => new CheckoutItemDto
                    {
                        CartItemId = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        ProductSku = i.Product.SKU,
                        ProductImageUrl = i.Product.Images.FirstOrDefault()?.Url,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice,
                        Currency = i.Currency,
                        WeightKg = i.Product.Weight,
                        VolumeM3 = null, // Product'ta volume yoksa
                        HsCode = i.Product.HSCode
                    }).ToList(),
                    SubTotal = g.Sum(i => i.TotalPrice),
                    Origin = defaultWarehouse != null ? new CheckoutOriginDto
                    {
                        VendorId = g.Key.SellerVendorId,
                        VendorName = g.Key.CompanyName,
                        WarehouseId = defaultWarehouse.Id,
                        WarehouseName = defaultWarehouse.Name,
                        City = defaultWarehouse.Address?.City,
                        CountryId = defaultWarehouse.Address?.CountryId,
                        CountryName = defaultWarehouse.Address?.CountryEntity?.Name,
                        Address = defaultWarehouse.Address?.AddressLine
                    } : null
                };
            }).ToList();

        // Kullanici adresleri
        var addresses = await _context.Addresses
            .Include(a => a.CountryEntity)
            .Where(a => a.VendorId == vendorId && !a.IsDeleted && a.IsActive)
            .OrderByDescending(a => a.IsDefault)
            .Select(a => new CheckoutAddressDto
            {
                Id = a.Id,
                Title = a.Title,
                FullAddress = a.AddressLine + ", " + a.City,
                City = a.City,
                CountryId = a.CountryId,
                CountryName = a.CountryEntity != null ? a.CountryEntity.Name : null,
                IsDefault = a.IsDefault
            })
            .ToListAsync();

        // Varsayilan teslimat adresi
        var shippingAddress = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses.FirstOrDefault();

        // Hizmet secenekleri
        var serviceOptions = new List<ServiceOptionDto>
        {
            new() {
                ServiceType = 1,
                ServiceTypeName = "Lojistik",
                ServiceTypeIcon = "bi-truck",
                Description = "Urunlerin satici deposundan teslimat adresinize tasinmasi",
                IsRequired = true,
                IsSelected = true
            },
            new() {
                ServiceType = 2,
                ServiceTypeName = "Gumruk",
                ServiceTypeIcon = "bi-flag",
                Description = "Ithalat/ihracat gumruk islemleri",
                IsRequired = false,
                IsSelected = false
            },
            new() {
                ServiceType = 4,
                ServiceTypeName = "Gozetim",
                ServiceTypeIcon = "bi-search",
                Description = "Urun kalite kontrolu ve ekspertiz",
                IsRequired = false,
                IsSelected = false
            },
            new() {
                ServiceType = 3,
                ServiceTypeName = "Sigorta",
                ServiceTypeIcon = "bi-shield-check",
                Description = "Nakliye sigortasi",
                IsRequired = false,
                IsSelected = false
            }
        };

        return new CheckoutSummaryDto
        {
            CartId = cart.Id,
            VendorGroups = vendorGroups,
            TotalProductAmount = items.Sum(i => i.TotalPrice),
            Currency = items.FirstOrDefault()?.Currency ?? "TRY",
            TotalItemCount = items.Sum(i => i.Quantity),
            ShippingAddress = shippingAddress,
            AvailableAddresses = addresses,
            ServiceOptions = serviceOptions
        };
    }

    public async Task<CheckoutResultDto> InitiateCheckoutAsync(int vendorId, int userId, InitiateCheckoutDto dto)
    {
        // Sepeti al
        var cart = await _context.Carts
            .Include(c => c.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Product)
            .Include(c => c.Items)
                .ThenInclude(i => i.SellerVendor)
                    .ThenInclude(v => v.Warehouses.Where(w => w.IsDefault && !w.IsDeleted))
                        .ThenInclude(w => w.Address)
                            .ThenInclude(a => a!.CountryEntity)
            .FirstOrDefaultAsync(c => c.VendorId == vendorId && c.UserId == userId &&
                c.Status == CartStatus.Active && !c.IsDeleted);

        if (cart == null || !cart.Items.Any(i => !i.IsDeleted))
            return new CheckoutResultDto { Success = false, Message = "Sepet bos" };

        var items = cart.Items.Where(i => !i.IsDeleted).ToList();

        // Teslimat adresi kontrolu
        var shippingAddress = await _context.Addresses
            .Include(a => a.CountryEntity)
            .FirstOrDefaultAsync(a => a.Id == dto.ShippingAddressId && a.VendorId == vendorId && !a.IsDeleted);

        if (shippingAddress == null)
            return new CheckoutResultDto { Success = false, Message = "Gecerli bir teslimat adresi secin" };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var serviceRequestIds = new List<int>();
            var orderIds = new List<int>();

            // Satici bazli siparisler olustur
            var vendorGroups = items.GroupBy(i => i.SellerVendorId);

            foreach (var group in vendorGroups)
            {
                var sellerVendorId = group.Key;
                var sellerItems = group.ToList();
                var firstItem = sellerItems.First();
                var sellerVendor = firstItem.SellerVendor;

                // Siparis olustur
                var orderHasServices = dto.Services != null && dto.Services.Any();
                var order = new Order
                {
                    OrderNumber = await _orderService.GenerateOrderNumberAsync(),
                    BuyerVendorId = vendorId,
                    SellerVendorId = sellerVendorId,
                    SourceType = 1, // Direct
                    SubTotal = sellerItems.Sum(i => i.TotalPrice),
                    ShippingCost = 0, // Lojistik teklifiyle belirlenecek
                    TaxAmount = 0,
                    TotalAmount = sellerItems.Sum(i => i.TotalPrice),
                    Currency = firstItem.Currency,
                    ShippingAddressId = dto.ShippingAddressId,
                    BillingAddressId = dto.BillingAddressId ?? dto.ShippingAddressId,
                    Status = 1, // Pending
                    PaymentStatus = 1, // Pending
                    Notes = dto.Notes,
                    // Incoterm bilgileri
                    IncotermId = dto.IncotermId,
                    IncotermLocation = dto.IncotermLocation,
                    // Checkout adimi - servis varsa teklif bekle, yoksa odeme bekle
                    CheckoutStep = orderHasServices ? CheckoutSteps.ServiceRequested.Id : CheckoutSteps.PaymentPending.Id
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                orderIds.Add(order.Id);

                // Siparis kalemleri
                foreach (var item in sellerItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    };
                    _context.OrderItems.Add(orderItem);
                }

                // Satici participant olarak ekle
                var sellerParticipant = new OrderParticipant
                {
                    OrderId = order.Id,
                    VendorId = sellerVendorId,
                    Role = 1, // Seller
                    Amount = order.SubTotal,
                    Currency = order.Currency,
                    Status = 1 // Pending
                };
                _context.OrderParticipants.Add(sellerParticipant);

                // Kaynak adres (satici deposu)
                var defaultWarehouse = sellerVendor.Warehouses?.FirstOrDefault();
                var originAddress = defaultWarehouse?.Address;

                // Secilen hizmetler icin ServiceRequest olustur
                // Gozetim (Survey) ozel mantigi: Lojistik varsa, Survey lojistik secildikten sonra olusturulacak
                var hasLogistics = dto.Services.Any(s => s.ServiceType == ServiceTypes.Logistics.Id);
                var hasSurvey = dto.Services.Any(s => s.ServiceType == ServiceTypes.Survey.Id);

                if (hasSurvey && hasLogistics)
                {
                    // Survey, Lojistik secildikten sonra otomatik olusturulacak
                    order.SurveyTriggerStatus = SurveyTriggerStatuses.WaitingForLogistics.Id;
                }
                else if (hasSurvey && !hasLogistics)
                {
                    // Lojistik yok ama Survey isteniyor - hemen olustur
                    order.SurveyTriggerStatus = SurveyTriggerStatuses.NotRequired.Id;
                }
                else
                {
                    order.SurveyTriggerStatus = SurveyTriggerStatuses.NotRequired.Id;
                }

                foreach (var service in dto.Services)
                {
                    // Gozetim (Survey) lojistik ile birlikte secildiyse simdi olusturma
                    // Lojistik teklifi kabul edildiginde otomatik olusturulacak
                    if (service.ServiceType == ServiceTypes.Survey.Id && hasLogistics)
                    {
                        continue; // Survey'i atla, sonra olusturulacak
                    }

                    var totalWeight = sellerItems.Sum(i => (i.Product.Weight ?? 0) * i.Quantity);
                    var totalValue = sellerItems.Sum(i => i.TotalPrice);

                    var serviceRequest = new OrderServiceRequest
                    {
                        OrderId = order.Id,
                        ServiceType = service.ServiceType,
                        Title = $"{ServiceTypes.GetById(service.ServiceType)?.Description} Talebi",
                        WeightKg = totalWeight > 0 ? totalWeight : null,
                        CargoValue = totalValue,
                        Currency = order.Currency,
                        OriginCountryId = originAddress?.CountryId,
                        OriginCity = originAddress?.City ?? defaultWarehouse?.Address?.City,
                        OriginAddress = originAddress?.AddressLine,
                        DestinationCountryId = shippingAddress.CountryId,
                        DestinationCity = shippingAddress.City,
                        DestinationAddress = shippingAddress.AddressLine,
                        TransportMode = service.TransportMode,
                        Incoterms = service.Incoterms,
                        CustomsOperationType = service.CustomsOperationType,
                        InsuranceType = service.InsuranceType,
                        DesiredPickupDate = service.DesiredPickupDate,
                        DesiredDeliveryDate = service.DesiredDeliveryDate,
                        Status = 1 // Open
                    };

                    // HS kodlarini topla (gumruk icin)
                    if (service.ServiceType == 2)
                    {
                        var hsCodes = sellerItems
                            .Where(i => !string.IsNullOrEmpty(i.Product.HSCode))
                            .Select(i => i.Product.HSCode)
                            .Distinct();
                        serviceRequest.HsCode = string.Join(", ", hsCodes);
                    }

                    _context.OrderServiceRequests.Add(serviceRequest);
                    await _context.SaveChangesAsync();

                    serviceRequestIds.Add(serviceRequest.Id);

                    // Servis saglayicilara bildirim gonder
                    await NotifyServiceProvidersAsync(order.Id, service.ServiceType);
                }

                // Finansman talepleri
                int? financingRequestId = null;

                // Yeni coklu finansman talebi sistemi
                var financingRequests = dto.FinancingRequests ?? new List<FinancingRequestDto>();

                // Eski uyumluluk: tek finansman talebi varsa listeye ekle
                if (dto.RequestFinancing && dto.FinancingDetails != null && !financingRequests.Any())
                {
                    financingRequests.Add(dto.FinancingDetails);
                }

                foreach (var finDto in financingRequests)
                {
                    // Talep tutari: belirtilmisse o, yoksa siparis toplami
                    var requestedAmount = finDto.RequestedAmount ?? order.TotalAmount;
                    // Siparis toplamini gecemez
                    if (requestedAmount > order.TotalAmount)
                        requestedAmount = order.TotalAmount;

                    // Finansman konusuna gore baslik belirle
                    var subjectName = finDto.FinancingSubject.HasValue
                        ? FinancingSubjects.GetById(finDto.FinancingSubject.Value)?.Description ?? "Siparis"
                        : "Siparis";
                    var title = $"{subjectName} Finansmani - {order.OrderNumber}";

                    var financingRequest = new FinancingRequest
                    {
                        RequesterVendorId = vendorId,
                        FinancingType = 2, // Order financing
                        FinancingSubject = finDto.FinancingSubject,
                        Title = title,
                        Description = finDto.Description,
                        RequestedAmount = requestedAmount,
                        Currency = order.Currency,
                        TotalValue = order.TotalAmount,
                        MaxInterestRate = finDto.MaxInterestRate,
                        DurationDays = finDto.DurationDays,
                        RepaymentDate = DateTime.UtcNow.AddDays(finDto.DurationDays),
                        CollateralType = 2, // Order
                        CollateralDescription = $"Siparis No: {order.OrderNumber}",
                        RelatedOrderId = order.Id,
                        OfferDeadline = finDto.OfferDeadline,
                        Status = 2 // Open
                    };

                    _context.FinancingRequests.Add(financingRequest);
                    await _context.SaveChangesAsync();

                    if (financingRequestId == null)
                        financingRequestId = financingRequest.Id;

                    // Yatirimcilara bildirim gonder
                    await NotifyInvestorsAsync(financingRequest.Id);
                }
            }

            // Sepeti converted olarak isaretle
            cart.Status = CartStatus.Converted;
            cart.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Hizmet secildi mi?
            var hasServices = dto.Services.Any();
            var nextStep = hasServices ? "AwaitingQuotes" : "Payment";

            return new CheckoutResultDto
            {
                Success = true,
                Message = "Siparis olusturuldu",
                OrderId = orderIds.FirstOrDefault(),
                OrderNumber = (await _context.Orders.FindAsync(orderIds.FirstOrDefault()))?.OrderNumber,
                ServiceRequestIds = serviceRequestIds,
                NextStep = nextStep
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new CheckoutResultDto { Success = false, Message = "Siparis olusturulurken hata: " + ex.Message };
        }
    }

    public async Task<OrderQuotesDto?> GetOrderQuotesAsync(int orderId, int vendorId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.ServiceRequests.Where(sr => !sr.IsDeleted))
                .ThenInclude(sr => sr.Quotes.Where(q => !q.IsDeleted))
                    .ThenInclude(q => q.ProviderVendor)
            .Include(o => o.Investments.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.InvestorVendor)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerVendorId == vendorId && !o.IsDeleted);

        if (order == null)
            return null;

        var serviceRequests = order.ServiceRequests.Select(sr => new ServiceRequestQuotesDto
        {
            Id = sr.Id,
            ServiceType = sr.ServiceType,
            ServiceTypeName = ServiceTypes.GetById(sr.ServiceType)?.Description ?? "",
            ServiceTypeIcon = ServiceTypes.GetById(sr.ServiceType)?.Icon ?? "",
            Status = sr.Status,
            StatusName = ServiceRequestStatuses.GetById(sr.Status)?.Description ?? "",
            SelectedQuoteId = sr.SelectedQuoteId,
            SelectedQuoteAmount = sr.SelectedQuote?.QuoteAmount,
            Quotes = sr.Quotes.Select(q => new ServiceQuoteItemDto
            {
                Id = q.Id,
                ProviderVendorId = q.ProviderVendorId,
                ProviderName = q.ProviderVendor?.CompanyName ?? "",
                QuoteAmount = q.QuoteAmount,
                Currency = q.Currency,
                EstimatedDays = q.EstimatedDays,
                IncludedServices = q.IncludedServices,
                Notes = q.Notes,
                ValidUntil = q.ValidUntil,
                Status = q.Status,
                StatusName = ServiceQuoteStatuses.GetById(q.Status)?.Description ?? "",
                IsSelected = sr.SelectedQuoteId == q.Id
            }).OrderBy(q => q.QuoteAmount).ToList()
        }).ToList();

        var investmentOffers = order.Investments.Select(i => new InvestmentOfferDto
        {
            Id = i.Id,
            InvestorVendorId = i.InvestorVendorId,
            InvestorName = i.InvestorVendor?.CompanyName ?? "",
            OfferedAmount = i.Amount,
            InterestRate = i.ReturnRate ?? 0,
            TotalRepaymentAmount = i.ExpectedReturn ?? i.Amount,
            Currency = i.Currency,
            Notes = i.Notes,
            Status = i.Status,
            StatusName = "", // TODO: InvestmentStatuses
            IsSelected = i.Status == 2 // Active
        }).ToList();

        var selectedServicesTotal = serviceRequests
            .Where(sr => sr.SelectedQuoteId.HasValue)
            .Sum(sr => sr.SelectedQuoteAmount ?? 0);

        var allSelected = serviceRequests.All(sr => sr.SelectedQuoteId.HasValue);

        return new OrderQuotesDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            OrderStatus = order.Status,
            OrderStatusName = OrderStatuses.GetById(order.Status)?.Description ?? "",
            ProductTotal = order.SubTotal,
            Currency = order.Currency,
            ServiceRequests = serviceRequests,
            InvestmentOffers = investmentOffers,
            TotalWithServices = order.SubTotal + selectedServicesTotal,
            AllServicesSelected = allSelected,
            CanProceedToPayment = allSelected || !serviceRequests.Any()
        };
    }

    public async Task<ServiceResult> SelectQuoteAsync(int orderId, int quoteId, int vendorId)
    {
        var quote = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
                .ThenInclude(sr => sr.Order)
            .FirstOrDefaultAsync(q => q.Id == quoteId && !q.IsDeleted);

        if (quote == null)
            return ServiceResult.Fail("Teklif bulunamadi");

        if (quote.ServiceRequest?.Order?.BuyerVendorId != vendorId)
            return ServiceResult.Fail("Bu teklifi secme yetkiniz yok");

        if (quote.Status != 1) // Pending
            return ServiceResult.Fail("Bu teklif artik gecerli degil");

        // Diger teklifleri reddet
        var otherQuotes = await _context.OrderServiceQuotes
            .Where(q => q.ServiceRequestId == quote.ServiceRequestId && q.Id != quoteId && q.Status == 1)
            .ToListAsync();

        foreach (var other in otherQuotes)
        {
            other.Status = 3; // Rejected
            other.RejectionReason = "Baska teklif secildi";
        }

        // Bu teklifi kabul et
        quote.Status = 2; // Accepted

        // ServiceRequest'i guncelle
        quote.ServiceRequest!.SelectedQuoteId = quoteId;
        quote.ServiceRequest.Status = 3; // QuoteSelected

        // Participant ekle
        var participant = new OrderParticipant
        {
            OrderId = quote.ServiceRequest.OrderId,
            VendorId = quote.ProviderVendorId,
            Role = GetParticipantRole(quote.ServiceRequest.ServiceType),
            ServiceQuoteId = quoteId,
            Amount = quote.QuoteAmount,
            Currency = quote.Currency,
            Status = 1 // Pending
        };
        _context.OrderParticipants.Add(participant);

        // Order toplam tutarini guncelle
        var order = quote.ServiceRequest.Order!;
        order.ShippingCost = await CalculateServicesTotalAsync(order.Id);
        order.TotalAmount = order.SubTotal + order.ShippingCost + order.TaxAmount;

        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<bool> AreAllServicesSelectedAsync(int orderId)
    {
        var serviceRequests = await _context.OrderServiceRequests
            .Where(sr => sr.OrderId == orderId && !sr.IsDeleted)
            .ToListAsync();

        if (!serviceRequests.Any())
            return true;

        return serviceRequests.All(sr => sr.SelectedQuoteId.HasValue);
    }

    public async Task<CheckoutCompleteDto> CompleteCheckoutAsync(int orderId, int vendorId)
    {
        var order = await _context.Orders
            .Include(o => o.ServiceRequests)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerVendorId == vendorId && !o.IsDeleted);

        if (order == null)
            return new CheckoutCompleteDto { Success = false, Message = "Siparis bulunamadi" };

        // Tum hizmetler secildi mi kontrol et
        if (!await AreAllServicesSelectedAsync(orderId))
            return new CheckoutCompleteDto { Success = false, Message = "Lutfen tum hizmetler icin teklif secin" };

        // Siparis durumunu guncelle
        order.Status = 2; // Confirmed (odeme bekleniyor)
        order.IsContractAccepted = true;
        order.ContractAcceptedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new CheckoutCompleteDto
        {
            Success = true,
            Message = "Siparis onaylandi",
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency
            // TODO: Stripe PaymentIntent olustur
        };
    }

    public async Task NotifyServiceProvidersAsync(int orderId, int serviceType)
    {
        // Ilgili capability ID'sini al
        var capabilityId = GetCapabilityIdForServiceType(serviceType);
        if (capabilityId == null)
            return;

        // Bu capability'e sahip firmalar
        var providerVendorIds = await _context.VendorCapabilityMappings
            .Where(m => m.CapabilityId == capabilityId.Value && m.IsActive)
            .Select(m => m.VendorId)
            .ToListAsync();

        var order = await _context.Orders
            .Include(o => o.BuyerVendor)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return;

        var serviceTypeName = ServiceTypes.GetById(serviceType)?.Description ?? "Hizmet";

        // Her firmaya bildirim gonder
        foreach (var providerVendorId in providerVendorIds)
        {
            await _notificationService.CreateAsync(new NotificationCreateDto
            {
                VendorId = providerVendorId,
                UserId = null,
                Type = NotificationType.NewServiceRequest,
                Title = $"Yeni {serviceTypeName} Talebi",
                Message = $"{order.BuyerVendor?.CompanyName} firmasindan yeni bir {serviceTypeName.ToLower()} talebi var. Teklif vermek icin inceleyin.",
                ActionUrl = $"/Provider/Requests?serviceType={serviceType}",
                Icon = "bi-clipboard-check"
            });
        }
    }

    private async Task<decimal> CalculateServicesTotalAsync(int orderId)
    {
        return await _context.OrderServiceRequests
            .Where(sr => sr.OrderId == orderId && sr.SelectedQuoteId.HasValue && !sr.IsDeleted)
            .Include(sr => sr.SelectedQuote)
            .SumAsync(sr => sr.SelectedQuote!.QuoteAmount);
    }

    private int GetParticipantRole(int serviceType)
    {
        return serviceType switch
        {
            1 => 2, // LogisticsProvider
            2 => 3, // CustomsBroker
            3 => 4, // InsuranceProvider
            4 => 6, // Surveyor (yeni eklenebilir)
            _ => 0
        };
    }

    private int? GetCapabilityIdForServiceType(int serviceType)
    {
        return serviceType switch
        {
            1 => Capabilities.Ids.Carrier,    // Tasimaci
            2 => Capabilities.Ids.Customs,    // Gumruk
            3 => Capabilities.Ids.Insurance,  // Sigorta
            4 => Capabilities.Ids.Survey,     // Gozetim
            _ => null
        };
    }

    private async Task NotifyInvestorsAsync(int financingRequestId)
    {
        // Yatirimci capability'sine sahip firmalari bul
        var investorVendorIds = await _context.VendorCapabilityMappings
            .Where(m => m.CapabilityId == Capabilities.Ids.Investor && m.IsActive)
            .Select(m => m.VendorId)
            .ToListAsync();

        var financingRequest = await _context.FinancingRequests
            .Include(r => r.RequesterVendor)
            .FirstOrDefaultAsync(r => r.Id == financingRequestId);

        if (financingRequest == null)
            return;

        // Her yatirimciya bildirim gonder
        foreach (var investorVendorId in investorVendorIds)
        {
            // Kendi talebine bildirim gitmesin
            if (investorVendorId == financingRequest.RequesterVendorId)
                continue;

            await _notificationService.CreateAsync(new NotificationCreateDto
            {
                VendorId = investorVendorId,
                UserId = null,
                Type = NotificationType.NewFinancingRequest,
                Title = "Yeni Finansman Firsati",
                Message = $"{financingRequest.RequesterVendor?.CompanyName} firmasindan {financingRequest.RequestedAmount:N0} {financingRequest.Currency} tutarinda finansman talebi.",
                ActionUrl = "/Investment/Opportunities",
                Icon = "bi-bank"
            });
        }
    }
}
