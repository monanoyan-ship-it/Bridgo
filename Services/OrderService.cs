using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Notification;
using Bridgo.DTOs.Order;
using Bridgo.Extensions;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IOrderOrchestrationService _orchestrationService;

    public OrderService(
        ApplicationDbContext context,
        INotificationService notificationService,
        IOrderOrchestrationService orchestrationService)
    {
        _context = context;
        _notificationService = notificationService;
        _orchestrationService = orchestrationService;
    }

    // ============================================
    // BUYER OPERATIONS
    // ============================================

    public async Task<OrderSearchResultDto> GetBuyerOrdersAsync(int buyerVendorId, OrderFilterDto filter)
    {
        var query = _context.Orders
            .Include(o => o.SellerVendor)
            .Include(o => o.Items)
            .Include(o => o.Shipments)
            .Include(o => o.ServiceRequests).ThenInclude(sr => sr.Quotes)
            .Where(o => o.BuyerVendorId == buyerVendorId);

        // Filters
        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (filter.PaymentStatus.HasValue)
            query = query.Where(o => o.PaymentStatus == filter.PaymentStatus.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(o => o.OrderNumber.Contains(filter.Search) ||
                                    o.SellerVendor!.CompanyName.Contains(filter.Search));

        if (filter.DateFrom.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.DateTo.Value);

        var totalCount = await query.CountAsync();

        query = filter.SortDescending
            ? query.OrderByDescending(o => o.CreatedAt)
            : query.OrderBy(o => o.CreatedAt);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(o => MapToListDto(o))
            .ToListAsync();

        return new OrderSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<OrderDetailDto?> GetOrderForBuyerAsync(int orderId, int buyerVendorId)
    {
        var order = await _context.Orders
            .Include(o => o.BuyerVendor)
            .Include(o => o.SellerVendor)
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Images)
            .Include(o => o.Items).ThenInclude(i => i.Warehouse)
            .Include(o => o.Shipments).ThenInclude(s => s.Items).ThenInclude(si => si.OrderItem)
            .Include(o => o.StatusHistory)
            .Include(o => o.ServiceRequests).ThenInclude(r => r.Quotes).ThenInclude(q => q.ProviderVendor)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerVendorId == buyerVendorId);

        return order == null ? null : MapToDetailDto(order);
    }

    public async Task<OrderDetailDto> CreateDirectOrderAsync(int buyerVendorId, CreateOrderDto dto, string? createdBy)
    {
        var product = await _context.Products
            .Include(p => p.Vendor)
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId && !p.IsDeleted);

        if (product == null)
            throw new InvalidOperationException("Ürün bulunamadı");

        var order = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(),
            BuyerVendorId = buyerVendorId,
            SellerVendorId = product.VendorId,
            SourceType = OrderSourceTypes.Direct.Id,
            ShippingAddressId = dto.ShippingAddressId,
            BillingAddressId = dto.BillingAddressId,
            Status = OrderStatuses.Draft.Id,
            PaymentStatus = PaymentStatuses.Pending.Id,
            Currency = product.Currency ?? "TRY",
            Notes = dto.Notes,
            CreatedBy = createdBy
        };

        var unitPrice = product.Price;
        var totalPrice = unitPrice * dto.Quantity;

        var orderItem = new OrderItem
        {
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            Quantity = dto.Quantity,
            Unit = dto.Unit ?? "Adet",
            UnitPrice = unitPrice,
            TotalPrice = totalPrice,
            CreatedBy = createdBy
        };

        order.Items.Add(orderItem);
        order.SubTotal = totalPrice;
        order.TotalAmount = totalPrice;

        // Add seller as participant
        order.Participants.Add(new OrderParticipant
        {
            VendorId = product.VendorId,
            Role = ParticipantRoles.Seller.Id,
            Amount = totalPrice,
            Currency = order.Currency,
            Status = ParticipantStatuses.Pending.Id,
            CreatedBy = createdBy
        });

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Notify seller
        await _notificationService.CreateAsync(new NotificationCreateDto
        {
            VendorId = product.VendorId,
            Type = NotificationType.NewOrder,
            Title = "Yeni Sipariş",
            Message = $"Yeni bir sipariş aldınız: {order.OrderNumber}",
            EntityType = "Order",
            EntityId = order.Id,
            ActionUrl = $"/Dashboard/SellerOrders?id={order.Id}"
        });

        return (await GetOrderForBuyerAsync(order.Id, buyerVendorId))!;
    }

    public async Task<OrderDetailDto> CreateOrderFromInquiryAsync(int buyerVendorId, CreateOrderFromInquiryDto dto, string? createdBy)
    {
        var response = await _context.ProductInquiryResponses
            .Include(r => r.Inquiry).ThenInclude(i => i!.Product)
            .Include(r => r.Inquiry).ThenInclude(i => i!.SellerVendor)
            .FirstOrDefaultAsync(r => r.Id == dto.ResponseId &&
                                     r.Inquiry!.BuyerVendorId == buyerVendorId &&
                                     r.Status == ProductInquiryResponseStatuses.Accepted.Id);

        if (response == null)
            throw new InvalidOperationException("Kabul edilmiş teklif bulunamadı");

        var inquiry = response.Inquiry!;

        var order = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(),
            BuyerVendorId = buyerVendorId,
            SellerVendorId = inquiry.SellerVendorId,
            SourceType = OrderSourceTypes.FromInquiry.Id,
            SourceInquiryId = inquiry.Id,
            ShippingAddressId = dto.ShippingAddressId,
            BillingAddressId = dto.BillingAddressId,
            Status = OrderStatuses.Draft.Id,
            PaymentStatus = PaymentStatuses.Pending.Id,
            Currency = response.Currency ?? "TRY",
            Notes = dto.Notes,
            CreatedBy = createdBy
        };

        var orderItem = new OrderItem
        {
            ProductId = inquiry.ProductId,
            Quantity = response.OfferedQuantity ?? inquiry.Quantity,
            Unit = response.OfferedUnit ?? inquiry.Unit ?? "Adet",
            UnitPrice = response.UnitPrice ?? 0,
            TotalPrice = response.TotalPrice ?? 0,
            CreatedBy = createdBy
        };

        order.Items.Add(orderItem);
        order.SubTotal = response.TotalPrice ?? 0;
        order.TotalAmount = response.TotalPrice ?? 0;

        // Add seller as participant
        order.Participants.Add(new OrderParticipant
        {
            VendorId = inquiry.SellerVendorId,
            Role = ParticipantRoles.Seller.Id,
            Amount = order.TotalAmount,
            Currency = order.Currency,
            Status = ParticipantStatuses.Pending.Id,
            CreatedBy = createdBy
        });

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return (await GetOrderForBuyerAsync(order.Id, buyerVendorId))!;
    }

    public async Task<OrderDetailDto> CreateOrderFromDemandAsync(int buyerVendorId, CreateOrderFromDemandDto dto, string? createdBy)
    {
        var response = await _context.DemandResponses
            .Include(r => r.Demand)
            .Include(r => r.SupplierVendor)
            .FirstOrDefaultAsync(r => r.Id == dto.ResponseId &&
                                     r.Demand!.VendorId == buyerVendorId &&
                                     r.Status == 3); // Accepted

        if (response == null)
            throw new InvalidOperationException("Kabul edilmiş teklif bulunamadı");

        var demand = response.Demand!;

        var order = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(),
            BuyerVendorId = buyerVendorId,
            SellerVendorId = response.SupplierVendorId ?? 0,
            SourceType = OrderSourceTypes.FromDemand.Id,
            SourceDemandId = demand.Id,
            SourceDemandResponseId = response.Id,
            ShippingAddressId = dto.ShippingAddressId,
            BillingAddressId = dto.BillingAddressId,
            Status = OrderStatuses.Draft.Id,
            PaymentStatus = PaymentStatuses.Pending.Id,
            Currency = response.Currency ?? "TRY",
            Notes = dto.Notes,
            CreatedBy = createdBy
        };

        // Update demand remaining quantity
        if (demand.Quantity.HasValue && response.Quantity.HasValue)
        {
            demand.RemainingQuantity = (demand.RemainingQuantity ?? demand.Quantity) - response.Quantity;
            if (demand.RemainingQuantity <= 0)
            {
                demand.FulfillmentStatus = DemandFulfillmentStatuses.FullyFulfilled.Id;
                demand.RemainingQuantity = 0;
            }
            else
            {
                demand.FulfillmentStatus = DemandFulfillmentStatuses.PartiallyFulfilled.Id;
            }
        }

        var orderItem = new OrderItem
        {
            ProductId = demand.ReferenceProductId ?? 0,
            Quantity = response.Quantity ?? 0,
            Unit = response.Unit ?? "Adet",
            UnitPrice = response.UnitPrice ?? 0,
            TotalPrice = response.TotalPrice ?? 0,
            CreatedBy = createdBy
        };

        if (orderItem.ProductId > 0)
            order.Items.Add(orderItem);

        order.SubTotal = response.TotalPrice ?? 0;
        order.TotalAmount = response.TotalPrice ?? 0;

        // Add seller as participant
        if (response.SupplierVendorId.HasValue)
        {
            order.Participants.Add(new OrderParticipant
            {
                VendorId = response.SupplierVendorId.Value,
                Role = ParticipantRoles.Seller.Id,
                Amount = order.TotalAmount,
                Currency = order.Currency,
                Status = ParticipantStatuses.Pending.Id,
                CreatedBy = createdBy
            });
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return (await GetOrderForBuyerAsync(order.Id, buyerVendorId))!;
    }

    public async Task<bool> CancelOrderByBuyerAsync(int orderId, int buyerVendorId, CancelOrderDto dto, string? updatedBy)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerVendorId == buyerVendorId);

        if (order == null) return false;

        // Can only cancel before payment
        if (order.PaymentStatus == PaymentStatuses.Succeeded.Id)
            throw new InvalidOperationException("Ödeme yapılmış siparişler iptal edilemez. İade talep edin.");

        var oldStatus = order.Status;
        order.Status = OrderStatuses.Cancelled.Id;
        order.CancellationReason = dto.Reason;
        order.UpdatedBy = updatedBy;

        // Log status change
        _context.OrderStatusHistory.Add(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = order.Status,
            Notes = dto.Reason,
            CreatedBy = updatedBy
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ConfirmDeliveryAsync(int orderId, int buyerVendorId, string? updatedBy)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Participants)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerVendorId == buyerVendorId);

        if (order == null) return false;

        var oldStatus = order.Status;
        order.Status = OrderStatuses.Completed.Id;
        order.UpdatedBy = updatedBy;

        // Mark all items as delivered
        foreach (var item in order.Items)
        {
            item.DeliveredQuantity = item.Quantity;
            item.UpdatedBy = updatedBy;
        }

        // Complete participant tasks
        foreach (var participant in order.Participants)
        {
            participant.IsTaskCompleted = true;
            participant.TaskCompletedAt = DateTime.UtcNow;
            participant.Status = ParticipantStatuses.Completed.Id;
        }

        _context.OrderStatusHistory.Add(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = order.Status,
            Notes = "Alıcı teslim aldığını onayladı",
            CreatedBy = updatedBy
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AcceptContractAsync(int orderId, int buyerVendorId, string? updatedBy)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerVendorId == buyerVendorId);

        if (order == null) return false;

        order.IsContractAccepted = true;
        order.ContractAcceptedAt = DateTime.UtcNow;
        order.Status = OrderStatuses.PendingPayment.Id;
        order.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<BuyerOrderStatsDto> GetBuyerStatsAsync(int buyerVendorId)
    {
        var orders = await _context.Orders
            .Where(o => o.BuyerVendorId == buyerVendorId)
            .ToListAsync();

        var activeStatuses = OrderStatuses.Active.Select(s => s.Id).ToList();
        var completedStatuses = OrderStatuses.CompletedOrders.Select(s => s.Id).ToList();

        return new BuyerOrderStatsDto
        {
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(o => o.Status == OrderStatuses.PendingPayment.Id || o.Status == OrderStatuses.PendingConfirm.Id),
            ActiveOrders = orders.Count(o => activeStatuses.Contains(o.Status)),
            CompletedOrders = orders.Count(o => completedStatuses.Contains(o.Status)),
            TotalSpent = orders.Where(o => o.PaymentStatus == PaymentStatuses.Succeeded.Id).Sum(o => o.TotalAmount),
            Currency = "TRY"
        };
    }

    // ============================================
    // SELLER OPERATIONS
    // ============================================

    public async Task<OrderSearchResultDto> GetSellerOrdersAsync(int sellerVendorId, OrderFilterDto filter)
    {
        var query = _context.Orders
            .Include(o => o.BuyerVendor)
            .Include(o => o.Items)
            .Include(o => o.Shipments)
            .Where(o => o.SellerVendorId == sellerVendorId);

        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(o => o.OrderNumber.Contains(filter.Search) ||
                                    o.BuyerVendor!.CompanyName.Contains(filter.Search));

        var totalCount = await query.CountAsync();

        query = filter.SortDescending
            ? query.OrderByDescending(o => o.CreatedAt)
            : query.OrderBy(o => o.CreatedAt);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(o => MapToListDto(o))
            .ToListAsync();

        return new OrderSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<OrderDetailDto?> GetOrderForSellerAsync(int orderId, int sellerVendorId)
    {
        var order = await _context.Orders
            .Include(o => o.BuyerVendor)
            .Include(o => o.SellerVendor)
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Images)
            .Include(o => o.Items).ThenInclude(i => i.Warehouse)
            .Include(o => o.Shipments).ThenInclude(s => s.Items)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.SellerVendorId == sellerVendorId);

        return order == null ? null : MapToDetailDto(order);
    }

    public async Task<bool> ConfirmOrderAsync(int orderId, int sellerVendorId, string? notes, string? updatedBy)
    {
        var order = await _context.Orders
            .Include(o => o.Participants)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.SellerVendorId == sellerVendorId);

        if (order == null) return false;

        if (order.Status != OrderStatuses.PendingConfirm.Id)
            throw new InvalidOperationException("Bu sipariş onay bekliyor durumunda değil");

        var oldStatus = order.Status;
        order.Status = OrderStatuses.Confirmed.Id;
        order.UpdatedBy = updatedBy;

        // Activate seller participant
        var sellerParticipant = order.Participants.FirstOrDefault(p => p.Role == ParticipantRoles.Seller.Id);
        if (sellerParticipant != null)
        {
            sellerParticipant.Status = ParticipantStatuses.Active.Id;
        }

        // Create tasks for seller
        await CreateSellerTasksAsync(order, updatedBy);

        _context.OrderStatusHistory.Add(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = order.Status,
            Notes = notes ?? "Satıcı siparişi onayladı",
            CreatedBy = updatedBy
        });

        await _context.SaveChangesAsync();

        // Notify buyer
        await _notificationService.CreateAsync(new NotificationCreateDto
        {
            VendorId = order.BuyerVendorId,
            Type = NotificationType.OrderConfirmed,
            Title = "Sipariş Onaylandı",
            Message = $"Siparişiniz onaylandı: {order.OrderNumber}",
            EntityType = "Order",
            EntityId = order.Id,
            ActionUrl = $"/Dashboard/MyOrders?id={order.Id}"
        });

        return true;
    }

    public async Task<bool> RejectOrderAsync(int orderId, int sellerVendorId, string? reason, string? updatedBy)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.SellerVendorId == sellerVendorId);

        if (order == null) return false;

        var oldStatus = order.Status;
        order.Status = OrderStatuses.Cancelled.Id;
        order.CancellationReason = reason ?? "Satıcı tarafından reddedildi";
        order.UpdatedBy = updatedBy;

        _context.OrderStatusHistory.Add(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = order.Status,
            Notes = reason,
            CreatedBy = updatedBy
        });

        await _context.SaveChangesAsync();

        // TODO: Trigger refund if payment was made

        return true;
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, int sellerVendorId, UpdateOrderStatusDto dto, string? updatedBy)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.SellerVendorId == sellerVendorId);

        if (order == null) return false;

        var oldStatus = order.Status;
        order.Status = dto.Status;
        order.UpdatedBy = updatedBy;

        _context.OrderStatusHistory.Add(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = dto.Status,
            Notes = dto.Notes,
            CreatedBy = updatedBy
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<OrderShipmentDto> CreateShipmentAsync(int orderId, int sellerVendorId, CreateShipmentDto dto, string? createdBy)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Shipments)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.SellerVendorId == sellerVendorId);

        if (order == null)
            throw new InvalidOperationException("Sipariş bulunamadı");

        var shipment = new OrderShipment
        {
            OrderId = orderId,
            ShipmentNumber = await GenerateShipmentNumberAsync(orderId),
            CarrierCode = dto.CarrierCode,
            CarrierName = dto.CarrierName,
            TrackingNumber = dto.TrackingNumber,
            TrackingUrl = dto.TrackingUrl,
            Status = ShipmentStatuses.Preparing.Id,
            EstimatedDeliveryDate = dto.EstimatedDeliveryDate.ToUtcSafe(),
            Notes = dto.Notes,
            CreatedBy = createdBy
        };

        foreach (var itemDto in dto.Items)
        {
            var orderItem = order.Items.FirstOrDefault(i => i.Id == itemDto.OrderItemId);
            if (orderItem != null)
            {
                shipment.Items.Add(new OrderShipmentItem
                {
                    OrderItemId = itemDto.OrderItemId,
                    Quantity = itemDto.Quantity,
                    CreatedBy = createdBy
                });

                orderItem.ShippedQuantity += itemDto.Quantity;
            }
        }

        _context.OrderShipments.Add(shipment);

        // Update order status
        var allShipped = order.Items.All(i => i.ShippedQuantity >= i.Quantity);
        var someShipped = order.Items.Any(i => i.ShippedQuantity > 0);

        if (allShipped)
            order.Status = OrderStatuses.Shipped.Id;
        else if (someShipped)
            order.Status = OrderStatuses.PartiallyShipped.Id;

        await _context.SaveChangesAsync();

        return MapToShipmentDto(shipment);
    }

    public async Task<bool> UpdateShipmentAsync(int orderId, int shipmentId, int sellerVendorId, UpdateShipmentDto dto, string? updatedBy)
    {
        var shipment = await _context.OrderShipments
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == shipmentId && s.OrderId == orderId && s.Order!.SellerVendorId == sellerVendorId);

        if (shipment == null) return false;

        shipment.Status = dto.Status;
        if (!string.IsNullOrEmpty(dto.TrackingNumber))
            shipment.TrackingNumber = dto.TrackingNumber;
        if (!string.IsNullOrEmpty(dto.TrackingUrl))
            shipment.TrackingUrl = dto.TrackingUrl;
        if (dto.EstimatedDeliveryDate.HasValue)
            shipment.EstimatedDeliveryDate = dto.EstimatedDeliveryDate.ToUtcSafe();
        if (dto.ActualDeliveryDate.HasValue)
            shipment.ActualDeliveryDate = dto.ActualDeliveryDate.ToUtcSafe();
        if (!string.IsNullOrEmpty(dto.Notes))
            shipment.Notes = dto.Notes;

        shipment.UpdatedBy = updatedBy;

        if (dto.Status == ShipmentStatuses.Shipped.Id)
            shipment.ShippedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<SellerOrderStatsDto> GetSellerStatsAsync(int sellerVendorId)
    {
        var orders = await _context.Orders
            .Where(o => o.SellerVendorId == sellerVendorId)
            .ToListAsync();

        var activeStatuses = OrderStatuses.Active.Select(s => s.Id).ToList();
        var completedStatuses = OrderStatuses.CompletedOrders.Select(s => s.Id).ToList();

        return new SellerOrderStatsDto
        {
            TotalOrders = orders.Count,
            PendingConfirmation = orders.Count(o => o.Status == OrderStatuses.PendingConfirm.Id),
            ActiveOrders = orders.Count(o => activeStatuses.Contains(o.Status)),
            CompletedOrders = orders.Count(o => completedStatuses.Contains(o.Status)),
            TotalRevenue = orders.Where(o => o.PaymentStatus == PaymentStatuses.Succeeded.Id).Sum(o => o.TotalAmount),
            Currency = "TRY"
        };
    }

    // ============================================
    // SERVICE PROVIDER OPERATIONS
    // ============================================

    public async Task<List<ServiceRequestListDto>> GetOpenServiceRequestsAsync(int providerVendorId, int serviceType)
    {
        var requests = await _context.OrderServiceRequests
            .Include(r => r.Order).ThenInclude(o => o!.BuyerVendor)
            .Include(r => r.Quotes)
            .Include(r => r.OriginCountry)
            .Include(r => r.DestinationCountry)
            .Where(r => r.ServiceType == serviceType &&
                       r.Status == ServiceRequestStatuses.Open.Id &&
                       !r.Quotes.Any(q => q.ProviderVendorId == providerVendorId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return requests.Select(r => new ServiceRequestListDto
        {
            Id = r.Id,
            OrderId = r.OrderId,
            OrderNumber = r.Order?.OrderNumber,
            ServiceType = r.ServiceType,
            ServiceTypeText = ServiceTypes.GetById(r.ServiceType)?.Description,
            Title = r.Title,
            BuyerVendorId = r.Order?.BuyerVendorId ?? 0,
            BuyerCompanyName = r.Order?.BuyerVendor?.CompanyName,
            CargoValue = r.CargoValue,
            Currency = r.Currency,
            OriginCountry = r.OriginCountry?.Name,
            DestinationCountry = r.DestinationCountry?.Name,
            Status = r.Status,
            StatusText = ServiceRequestStatuses.GetById(r.Status)?.Description,
            QuoteCount = r.Quotes.Count,
            QuoteDeadline = r.QuoteDeadline,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<ServiceRequestDetailDto?> GetServiceRequestDetailAsync(int requestId, int providerVendorId)
    {
        var request = await _context.OrderServiceRequests
            .Include(r => r.Order).ThenInclude(o => o!.BuyerVendor)
            .Include(r => r.Quotes).ThenInclude(q => q.ProviderVendor)
            .Include(r => r.OriginCountry)
            .Include(r => r.DestinationCountry)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null) return null;

        return new ServiceRequestDetailDto
        {
            Id = request.Id,
            OrderId = request.OrderId,
            OrderNumber = request.Order?.OrderNumber,
            ServiceType = request.ServiceType,
            ServiceTypeText = ServiceTypes.GetById(request.ServiceType)?.Description,
            Title = request.Title,
            Description = request.Description,
            BuyerVendorId = request.Order?.BuyerVendorId ?? 0,
            BuyerCompanyName = request.Order?.BuyerVendor?.CompanyName,
            WeightKg = request.WeightKg,
            VolumeM3 = request.VolumeM3,
            PackageCount = request.PackageCount,
            CargoValue = request.CargoValue,
            Currency = request.Currency,
            OriginCountryId = request.OriginCountryId,
            OriginCountryName = request.OriginCountry?.Name,
            OriginCity = request.OriginCity,
            OriginAddress = request.OriginAddress,
            DestinationCountryId = request.DestinationCountryId,
            DestinationCountryName = request.DestinationCountry?.Name,
            DestinationCity = request.DestinationCity,
            DestinationAddress = request.DestinationAddress,
            TransportMode = request.TransportMode,
            TransportModeText = request.TransportMode.HasValue ? TransportModes.GetById(request.TransportMode.Value)?.Description : null,
            Incoterms = request.Incoterms,
            CustomsOperationType = request.CustomsOperationType,
            CustomsOperationTypeText = request.CustomsOperationType.HasValue ? CustomsOperationTypes.GetById(request.CustomsOperationType.Value)?.Description : null,
            HsCode = request.HsCode,
            InsuranceType = request.InsuranceType,
            InsuranceTypeText = request.InsuranceType.HasValue ? InsuranceTypes.GetById(request.InsuranceType.Value)?.Description : null,
            DesiredPickupDate = request.DesiredPickupDate,
            DesiredDeliveryDate = request.DesiredDeliveryDate,
            QuoteDeadline = request.QuoteDeadline,
            Status = request.Status,
            StatusText = ServiceRequestStatuses.GetById(request.Status)?.Description,
            SelectedQuoteId = request.SelectedQuoteId,
            Quotes = request.Quotes.Select(q => MapToServiceQuoteDto(q)).ToList(),
            CreatedAt = request.CreatedAt
        };
    }

    public async Task<ServiceQuoteDto> CreateServiceQuoteAsync(int providerVendorId, CreateServiceQuoteDto dto, string? createdBy)
    {
        var request = await _context.OrderServiceRequests
            .Include(r => r.Order)
            .FirstOrDefaultAsync(r => r.Id == dto.ServiceRequestId && r.Status == ServiceRequestStatuses.Open.Id);

        if (request == null)
            throw new InvalidOperationException("Servis talebi bulunamadı veya teklif kabul etmiyor");

        var existingQuote = await _context.OrderServiceQuotes
            .AnyAsync(q => q.ServiceRequestId == dto.ServiceRequestId && q.ProviderVendorId == providerVendorId);

        if (existingQuote)
            throw new InvalidOperationException("Bu talep için zaten teklif verdiniz");

        // Provider vendor name
        var providerVendor = await _context.Vendors.FindAsync(providerVendorId);
        var providerName = providerVendor?.CompanyName ?? "Servis Saglayici";

        var quote = new OrderServiceQuote
        {
            ServiceRequestId = dto.ServiceRequestId,
            ProviderVendorId = providerVendorId,
            QuoteAmount = dto.QuoteAmount,
            Currency = dto.Currency,
            IncludedServices = dto.IncludedServices,
            AdditionalCosts = dto.AdditionalCosts,
            EstimatedDays = dto.EstimatedDays,
            ValidUntil = dto.ValidUntil.ToUtcSafe(),
            Notes = dto.Notes,
            TermsAndConditions = dto.TermsAndConditions,
            TransportModes = dto.TransportModes != null && dto.TransportModes.Any()
                ? string.Join(",", dto.TransportModes)
                : null,
            CarrierName = dto.CarrierName,
            TransitStops = dto.TransitStops,
            CoverageDetails = dto.CoverageDetails,
            DeductiblePercent = dto.DeductiblePercent,
            Status = ServiceQuoteStatuses.Pending.Id,
            CreatedBy = createdBy
        };

        _context.OrderServiceQuotes.Add(quote);

        // Update request status
        request.Status = ServiceRequestStatuses.QuotesReceived.Id;

        await _context.SaveChangesAsync();

        // Send real-time notification to buyer
        if (request.Order != null)
        {
            var serviceTypeName = ServiceTypes.GetById(request.ServiceType)?.Description ?? "Hizmet";
            await _notificationService.CreateAsync(new DTOs.Notification.NotificationCreateDto
            {
                VendorId = request.Order.BuyerVendorId,
                Type = NotificationType.NewServiceQuote,
                Title = $"Yeni {serviceTypeName} Teklifi",
                Message = $"{providerName} firmasindan {dto.QuoteAmount:N0} {dto.Currency} tutarinda teklif geldi.",
                EntityType = "OrderServiceQuote",
                EntityId = quote.Id,
                ActionUrl = "/Orders/MyOrders",
                Icon = "bi-clipboard-check"
            });
        }

        return MapToServiceQuoteDto(quote);
    }

    public async Task<bool> UpdateServiceQuoteAsync(int quoteId, int providerVendorId, UpdateServiceQuoteDto dto, string? updatedBy)
    {
        var quote = await _context.OrderServiceQuotes
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.ProviderVendorId == providerVendorId);

        if (quote == null) return false;

        if (dto.QuoteAmount.HasValue)
            quote.QuoteAmount = dto.QuoteAmount.Value;
        if (dto.EstimatedDays.HasValue)
            quote.EstimatedDays = dto.EstimatedDays;
        if (dto.ValidUntil.HasValue)
            quote.ValidUntil = dto.ValidUntil.ToUtcSafe();
        if (!string.IsNullOrEmpty(dto.Notes))
            quote.Notes = dto.Notes;

        quote.UpdatedBy = updatedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> WithdrawServiceQuoteAsync(int quoteId, int providerVendorId, string? updatedBy)
    {
        var quote = await _context.OrderServiceQuotes
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.ProviderVendorId == providerVendorId);

        if (quote == null) return false;

        quote.Status = ServiceQuoteStatuses.Withdrawn.Id;
        quote.UpdatedBy = updatedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    // ============================================
    // BUYER - SERVICE REQUEST OPERATIONS
    // ============================================

    public async Task<ServiceRequestDetailDto> CreateServiceRequestAsync(int orderId, int buyerVendorId, CreateServiceRequestDto dto, string? createdBy)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerVendorId == buyerVendorId);

        if (order == null)
            throw new InvalidOperationException("Sipariş bulunamadı");

        var request = new OrderServiceRequest
        {
            OrderId = orderId,
            ServiceType = dto.ServiceType,
            Title = dto.Title,
            Description = dto.Description,
            WeightKg = dto.WeightKg,
            VolumeM3 = dto.VolumeM3,
            PackageCount = dto.PackageCount,
            CargoValue = dto.CargoValue,
            Currency = dto.Currency,
            OriginCountryId = dto.OriginCountryId,
            OriginCity = dto.OriginCity,
            OriginAddress = dto.OriginAddress,
            DestinationCountryId = dto.DestinationCountryId,
            DestinationCity = dto.DestinationCity,
            DestinationAddress = dto.DestinationAddress,
            TransportMode = dto.TransportMode,
            Incoterms = dto.Incoterms,
            CustomsOperationType = dto.CustomsOperationType,
            HsCode = dto.HsCode,
            InsuranceType = dto.InsuranceType,
            DesiredPickupDate = dto.DesiredPickupDate.ToUtcSafe(),
            DesiredDeliveryDate = dto.DesiredDeliveryDate.ToUtcSafe(),
            QuoteDeadline = dto.QuoteDeadline.ToUtcSafe(),
            Status = ServiceRequestStatuses.Open.Id,
            CreatedBy = createdBy
        };

        _context.OrderServiceRequests.Add(request);
        await _context.SaveChangesAsync();

        return (await GetServiceRequestDetailAsync(request.Id, 0))!;
    }

    public async Task<bool> AcceptServiceQuoteAsync(int quoteId, int buyerVendorId, string? updatedBy)
    {
        var quote = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest).ThenInclude(r => r!.Order)
            .Include(q => q.ProviderVendor)
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.ServiceRequest!.Order!.BuyerVendorId == buyerVendorId);

        if (quote == null) return false;

        quote.Status = ServiceQuoteStatuses.Accepted.Id;
        quote.UpdatedBy = updatedBy;

        var request = quote.ServiceRequest!;
        request.SelectedQuoteId = quoteId;
        request.Status = ServiceRequestStatuses.QuoteSelected.Id;

        // Add service provider as participant
        var order = request.Order!;
        var participantRole = request.ServiceType switch
        {
            1 => ParticipantRoles.LogisticsProvider.Id,
            2 => ParticipantRoles.CustomsBroker.Id,
            3 => ParticipantRoles.InsuranceProvider.Id,
            4 => ParticipantRoles.SurveyProvider.Id,
            _ => ParticipantRoles.LogisticsProvider.Id
        };

        order.Participants.Add(new OrderParticipant
        {
            VendorId = quote.ProviderVendorId,
            Role = participantRole,
            ServiceQuoteId = quoteId,
            Amount = quote.QuoteAmount,
            Currency = quote.Currency,
            Status = ParticipantStatuses.Pending.Id,
            CreatedBy = updatedBy
        });

        // Update order totals
        order.TotalAmount += quote.QuoteAmount;

        // Reject other quotes
        var otherQuotes = await _context.OrderServiceQuotes
            .Where(q => q.ServiceRequestId == request.Id && q.Id != quoteId && q.Status == ServiceQuoteStatuses.Pending.Id)
            .ToListAsync();

        foreach (var otherQuote in otherQuotes)
        {
            otherQuote.Status = ServiceQuoteStatuses.Rejected.Id;
            otherQuote.RejectionReason = "Başka bir teklif kabul edildi";
        }

        await _context.SaveChangesAsync();

        // Orchestration: Gozetim tetikleme ve tum servisler secildi mi kontrolu
        await _orchestrationService.OnServiceQuoteAcceptedAsync(order.Id, request.Id, quoteId);

        return true;
    }

    public async Task<bool> RejectServiceQuoteAsync(int quoteId, int buyerVendorId, string? reason, string? updatedBy)
    {
        var quote = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest).ThenInclude(r => r!.Order)
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.ServiceRequest!.Order!.BuyerVendorId == buyerVendorId);

        if (quote == null) return false;

        quote.Status = ServiceQuoteStatuses.Rejected.Id;
        quote.RejectionReason = reason;
        quote.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    // ============================================
    // TASK OPERATIONS
    // ============================================

    public async Task<List<OrderTaskDto>> GetParticipantTasksAsync(int orderId, int vendorId)
    {
        var participant = await _context.OrderParticipants
            .Include(p => p.Tasks)
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.VendorId == vendorId);

        if (participant == null) return new List<OrderTaskDto>();

        return participant.Tasks.OrderBy(t => t.SortOrder).Select(t => new OrderTaskDto
        {
            Id = t.Id,
            OrderId = t.OrderId,
            OrderNumber = participant.Order?.OrderNumber,
            ParticipantId = t.ParticipantId,
            Title = t.Title,
            Description = t.Description,
            TaskType = t.TaskType,
            TaskTypeText = TaskTypes.GetById(t.TaskType)?.Description,
            SortOrder = t.SortOrder,
            DependsOnTaskId = t.DependsOnTaskId,
            CanStart = !t.DependsOnTaskId.HasValue || _context.OrderTasks.Any(dt => dt.Id == t.DependsOnTaskId && dt.Status == TaskStatuses.Completed.Id),
            DueDate = t.DueDate,
            Status = t.Status,
            StatusText = TaskStatuses.GetById(t.Status)?.Description,
            StatusCssClass = TaskStatuses.GetById(t.Status)?.CssClass,
            CompletedAt = t.CompletedAt,
            CompletionNotes = t.CompletionNotes,
            IsRequired = t.IsRequired,
            CreatedAt = t.CreatedAt
        }).ToList();
    }

    public async Task<bool> StartTaskAsync(int taskId, int vendorId, string? updatedBy)
    {
        var task = await _context.OrderTasks
            .Include(t => t.Participant)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.Participant!.VendorId == vendorId);

        if (task == null) return false;

        task.Status = TaskStatuses.InProgress.Id;
        task.UpdatedBy = updatedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteTaskAsync(int taskId, int vendorId, string? notes, string? updatedBy)
    {
        var task = await _context.OrderTasks
            .Include(t => t.Participant)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.Participant!.VendorId == vendorId);

        if (task == null) return false;

        task.Status = TaskStatuses.Completed.Id;
        task.CompletedAt = DateTime.UtcNow;
        task.CompletionNotes = notes;
        task.UpdatedBy = updatedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    // ============================================
    // INVESTMENT OPERATIONS
    // ============================================

    public async Task<List<InvestmentOpportunityDto>> GetInvestmentOpportunitiesAsync(int investorVendorId)
    {
        // Orders that need investment and investor hasn't already invested
        var orders = await _context.Orders
            .Include(o => o.BuyerVendor)
            .Include(o => o.Investments)
            .Where(o => o.Status == OrderStatuses.Draft.Id || o.Status == OrderStatuses.PendingPayment.Id)
            .Where(o => !o.Investments.Any(i => i.InvestorVendorId == investorVendorId))
            .ToListAsync();

        return orders.Select(o => new InvestmentOpportunityDto
        {
            OrderId = o.Id,
            OrderNumber = o.OrderNumber,
            BuyerVendorId = o.BuyerVendorId,
            BuyerCompanyName = o.BuyerVendor?.CompanyName,
            TotalAmount = o.TotalAmount,
            Currency = o.Currency,
            RequiredInvestment = o.TotalAmount,
            CurrentInvestment = o.Investments.Where(i => i.Status == InvestmentStatuses.Active.Id).Sum(i => i.Amount),
            RemainingInvestment = o.TotalAmount - o.Investments.Where(i => i.Status == InvestmentStatuses.Active.Id).Sum(i => i.Amount),
            CreatedAt = o.CreatedAt
        }).Where(o => o.RemainingInvestment > 0).ToList();
    }

    public async Task<OrderInvestmentDto> CreateInvestmentAsync(int investorVendorId, CreateInvestmentDto dto, string? createdBy)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

        if (order == null)
            throw new InvalidOperationException("Sipariş bulunamadı");

        var investment = new OrderInvestment
        {
            OrderId = dto.OrderId,
            InvestorVendorId = investorVendorId,
            Amount = dto.Amount,
            Currency = dto.Currency,
            PercentageOfTotal = order.TotalAmount > 0 ? (dto.Amount / order.TotalAmount) * 100 : 0,
            InvestmentType = dto.InvestmentType,
            ReturnRate = dto.ReturnRate,
            ExpectedReturn = dto.ReturnRate.HasValue ? dto.Amount * (1 + dto.ReturnRate.Value / 100) : dto.Amount,
            RepaymentDueDate = dto.RepaymentDueDate.ToUtcSafe(),
            TermsAndConditions = dto.TermsAndConditions,
            Notes = dto.Notes,
            Status = InvestmentStatuses.Pending.Id,
            CreatedBy = createdBy
        };

        _context.OrderInvestments.Add(investment);
        await _context.SaveChangesAsync();

        return MapToInvestmentDto(investment);
    }

    public async Task<bool> AcceptInvestmentAsync(int investmentId, int buyerVendorId, string? updatedBy)
    {
        var investment = await _context.OrderInvestments
            .Include(i => i.Order)
            .FirstOrDefaultAsync(i => i.Id == investmentId && i.Order!.BuyerVendorId == buyerVendorId);

        if (investment == null) return false;

        investment.Status = InvestmentStatuses.Active.Id;
        investment.UpdatedBy = updatedBy;

        // Add investor as participant
        investment.Order!.Participants.Add(new OrderParticipant
        {
            VendorId = investment.InvestorVendorId,
            Role = ParticipantRoles.Investor.Id,
            Amount = investment.ExpectedReturn ?? investment.Amount,
            Currency = investment.Currency,
            Status = ParticipantStatuses.Pending.Id,
            CreatedBy = updatedBy
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectInvestmentAsync(int investmentId, int buyerVendorId, string? reason, string? updatedBy)
    {
        var investment = await _context.OrderInvestments
            .Include(i => i.Order)
            .FirstOrDefaultAsync(i => i.Id == investmentId && i.Order!.BuyerVendorId == buyerVendorId);

        if (investment == null) return false;

        investment.Status = InvestmentStatuses.Cancelled.Id;
        investment.Notes = reason;
        investment.UpdatedBy = updatedBy;
        await _context.SaveChangesAsync();
        return true;
    }

    // ============================================
    // COMMON OPERATIONS
    // ============================================

    public async Task<string> GenerateOrderNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Orders
            .CountAsync(o => o.CreatedAt.Year == year);
        return $"ORD-{year}-{(count + 1):D5}";
    }

    public async Task<string> GenerateShipmentNumberAsync(int orderId)
    {
        var count = await _context.OrderShipments
            .CountAsync(s => s.OrderId == orderId);
        return $"SHP-{(count + 1):D3}";
    }

    public async Task<List<OrderStatusHistoryDto>> GetOrderStatusHistoryAsync(int orderId)
    {
        var history = await _context.OrderStatusHistory
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        return history.Select(h => new OrderStatusHistoryDto
        {
            Id = h.Id,
            OrderId = h.OrderId,
            OldStatus = h.OldStatus,
            OldStatusText = OrderStatuses.GetById(h.OldStatus)?.Description,
            NewStatus = h.NewStatus,
            NewStatusText = OrderStatuses.GetById(h.NewStatus)?.Description,
            Notes = h.Notes,
            ChangedByUserId = h.ChangedByUserId,
            CreatedAt = h.CreatedAt
        }).ToList();
    }

    public async Task<OrderShipmentDto?> GetShipmentTrackingAsync(int shipmentId)
    {
        var shipment = await _context.OrderShipments
            .Include(s => s.Items).ThenInclude(i => i.OrderItem).ThenInclude(oi => oi!.Product)
            .FirstOrDefaultAsync(s => s.Id == shipmentId);

        return shipment == null ? null : MapToShipmentDto(shipment);
    }

    // ============================================
    // PRIVATE HELPER METHODS
    // ============================================

    private async Task CreateSellerTasksAsync(Order order, string? createdBy)
    {
        var sellerParticipant = await _context.OrderParticipants
            .FirstOrDefaultAsync(p => p.OrderId == order.Id && p.Role == ParticipantRoles.Seller.Id);

        if (sellerParticipant == null) return;

        var tasks = new List<OrderTask>
        {
            new OrderTask
            {
                OrderId = order.Id,
                ParticipantId = sellerParticipant.Id,
                Title = "Siparişi Hazırla",
                Description = "Sipariş ürünlerini paketleyin",
                TaskType = TaskTypes.PrepareOrder.Id,
                SortOrder = 1,
                IsRequired = true,
                Status = TaskStatuses.Pending.Id,
                CreatedBy = createdBy
            },
            new OrderTask
            {
                OrderId = order.Id,
                ParticipantId = sellerParticipant.Id,
                Title = "Kargoya Ver",
                Description = "Paketleri kargo firmasına teslim edin",
                TaskType = TaskTypes.ShipOrder.Id,
                SortOrder = 2,
                IsRequired = true,
                Status = TaskStatuses.Pending.Id,
                CreatedBy = createdBy
            },
            new OrderTask
            {
                OrderId = order.Id,
                ParticipantId = sellerParticipant.Id,
                Title = "Takip Bilgisi Gir",
                Description = "Kargo takip numarasını sisteme girin",
                TaskType = TaskTypes.ProvideTracking.Id,
                SortOrder = 3,
                IsRequired = true,
                Status = TaskStatuses.Pending.Id,
                CreatedBy = createdBy
            }
        };

        _context.OrderTasks.AddRange(tasks);
    }

    private static OrderListDto MapToListDto(Order o)
    {
        var status = OrderStatuses.GetById(o.Status);
        return new OrderListDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            BuyerVendorId = o.BuyerVendorId,
            BuyerCompanyName = o.BuyerVendor?.CompanyName,
            SellerVendorId = o.SellerVendorId,
            SellerCompanyName = o.SellerVendor?.CompanyName,
            SourceType = o.SourceType,
            SourceTypeText = OrderSourceTypes.GetById(o.SourceType)?.Description,
            TotalAmount = o.TotalAmount,
            Currency = o.Currency,
            Status = o.Status,
            StatusText = status?.Description,
            StatusCssClass = status?.CssClass,
            StatusIcon = status?.Icon,
            PaymentStatus = o.PaymentStatus,
            PaymentStatusText = PaymentStatuses.GetById(o.PaymentStatus)?.Description,
            ItemCount = o.Items.Count,
            ShipmentCount = o.Shipments.Count,
            HasActiveShipment = o.Shipments.Any(s => s.Status < ShipmentStatuses.Delivered.Id),
            ServiceRequestCount = o.ServiceRequests?.Count ?? 0,
            PendingQuotesCount = o.ServiceRequests?.Count(sr => sr.SelectedQuoteId == null && sr.Quotes.Any()) ?? 0,
            HasPendingQuotes = o.ServiceRequests?.Any(sr => sr.SelectedQuoteId == null) ?? false,
            CreatedAt = o.CreatedAt,
            PaidAt = o.PaidAt
        };
    }

    private static OrderDetailDto MapToDetailDto(Order o)
    {
        var status = OrderStatuses.GetById(o.Status);
        return new OrderDetailDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            BuyerVendorId = o.BuyerVendorId,
            BuyerCompanyName = o.BuyerVendor?.CompanyName,
            BuyerEmail = o.BuyerVendor?.Email,
            BuyerPhone = o.BuyerVendor?.Phone,
            SellerVendorId = o.SellerVendorId,
            SellerCompanyName = o.SellerVendor?.CompanyName,
            SellerEmail = o.SellerVendor?.Email,
            SellerPhone = o.SellerVendor?.Phone,
            SourceType = o.SourceType,
            SourceTypeText = OrderSourceTypes.GetById(o.SourceType)?.Description,
            SourceInquiryId = o.SourceInquiryId,
            SourceDemandId = o.SourceDemandId,
            SourceDemandResponseId = o.SourceDemandResponseId,
            SubTotal = o.SubTotal,
            ShippingCost = o.ShippingCost,
            TaxAmount = o.TaxAmount,
            TotalAmount = o.TotalAmount,
            Currency = o.Currency,
            ShippingAddress = o.ShippingAddress == null ? null : MapToAddressDto(o.ShippingAddress),
            BillingAddress = o.BillingAddress == null ? null : MapToAddressDto(o.BillingAddress),
            Status = o.Status,
            StatusText = status?.Description,
            StatusCssClass = status?.CssClass,
            StatusIcon = status?.Icon,
            Notes = o.Notes,
            CancellationReason = o.CancellationReason,
            PaymentStatus = o.PaymentStatus,
            PaymentStatusText = PaymentStatuses.GetById(o.PaymentStatus)?.Description,
            StripePaymentIntentId = o.StripePaymentIntentId,
            PaidAt = o.PaidAt,
            Items = o.Items.Select(i => MapToItemDto(i)).ToList(),
            Shipments = o.Shipments.Select(s => MapToShipmentDto(s)).ToList(),
            StatusHistory = o.StatusHistory.OrderByDescending(h => h.CreatedAt).Select(h => new OrderStatusHistoryDto
            {
                Id = h.Id,
                OrderId = h.OrderId,
                OldStatus = h.OldStatus,
                OldStatusText = OrderStatuses.GetById(h.OldStatus)?.Description,
                NewStatus = h.NewStatus,
                NewStatusText = OrderStatuses.GetById(h.NewStatus)?.Description,
                Notes = h.Notes,
                ChangedByUserId = h.ChangedByUserId,
                CreatedAt = h.CreatedAt
            }).ToList(),
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt
        };
    }

    private static OrderAddressDto MapToAddressDto(Address a)
    {
        return new OrderAddressDto
        {
            Id = a.Id,
            Title = a.Title,
            ContactName = a.ContactName,
            ContactPhone = a.ContactPhone,
            Country = a.CountryEntity?.Name,
            City = a.City,
            District = a.District,
            AddressLine = a.AddressLine,
            PostalCode = a.PostalCode
        };
    }

    private static OrderItemDto MapToItemDto(OrderItem i)
    {
        return new OrderItemDto
        {
            Id = i.Id,
            OrderId = i.OrderId,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name,
            ProductSku = i.Product?.SKU,
            ProductImageUrl = i.Product?.Images.FirstOrDefault()?.Url,
            WarehouseId = i.WarehouseId,
            WarehouseName = i.Warehouse?.Name,
            Quantity = i.Quantity,
            Unit = i.Unit,
            UnitPrice = i.UnitPrice,
            TotalPrice = i.TotalPrice,
            ShippedQuantity = i.ShippedQuantity,
            DeliveredQuantity = i.DeliveredQuantity,
            RemainingToShip = i.Quantity - i.ShippedQuantity
        };
    }

    private static OrderShipmentDto MapToShipmentDto(OrderShipment s)
    {
        var status = ShipmentStatuses.GetById(s.Status);
        return new OrderShipmentDto
        {
            Id = s.Id,
            OrderId = s.OrderId,
            ShipmentNumber = s.ShipmentNumber,
            CarrierCode = s.CarrierCode,
            CarrierName = s.CarrierName,
            TrackingNumber = s.TrackingNumber,
            TrackingUrl = s.TrackingUrl,
            Status = s.Status,
            StatusText = status?.Description,
            StatusCssClass = status?.CssClass,
            StatusIcon = status?.Icon,
            ShippedAt = s.ShippedAt,
            EstimatedDeliveryDate = s.EstimatedDeliveryDate,
            ActualDeliveryDate = s.ActualDeliveryDate,
            Notes = s.Notes,
            Items = s.Items.Select(si => new OrderShipmentItemDto
            {
                Id = si.Id,
                ShipmentId = si.ShipmentId,
                OrderItemId = si.OrderItemId,
                ProductName = si.OrderItem?.Product?.Name,
                ProductSku = si.OrderItem?.Product?.SKU,
                Quantity = si.Quantity
            }).ToList(),
            CreatedAt = s.CreatedAt
        };
    }

    private static ServiceQuoteDto MapToServiceQuoteDto(OrderServiceQuote q)
    {
        return new ServiceQuoteDto
        {
            Id = q.Id,
            ServiceRequestId = q.ServiceRequestId,
            ProviderVendorId = q.ProviderVendorId,
            ProviderCompanyName = q.ProviderVendor?.CompanyName,
            QuoteAmount = q.QuoteAmount,
            Currency = q.Currency,
            IncludedServices = q.IncludedServices,
            AdditionalCosts = q.AdditionalCosts,
            EstimatedDays = q.EstimatedDays,
            ValidUntil = q.ValidUntil,
            Notes = q.Notes,
            TermsAndConditions = q.TermsAndConditions,
            TransportModes = q.TransportModes,
            TransportModesText = GetTransportModesText(q.TransportModes),
            CarrierName = q.CarrierName,
            TransitStops = q.TransitStops,
            CoverageDetails = q.CoverageDetails,
            DeductiblePercent = q.DeductiblePercent,
            Status = q.Status,
            StatusText = ServiceQuoteStatuses.GetById(q.Status)?.Description,
            RejectionReason = q.RejectionReason,
            IsReadByBuyer = q.IsReadByBuyer,
            CreatedAt = q.CreatedAt
        };
    }

    private static OrderInvestmentDto MapToInvestmentDto(OrderInvestment i)
    {
        return new OrderInvestmentDto
        {
            Id = i.Id,
            OrderId = i.OrderId,
            InvestorVendorId = i.InvestorVendorId,
            InvestorCompanyName = i.InvestorVendor?.CompanyName,
            Amount = i.Amount,
            Currency = i.Currency,
            PercentageOfTotal = i.PercentageOfTotal,
            InvestmentType = i.InvestmentType,
            InvestmentTypeText = InvestmentTypes.GetById(i.InvestmentType)?.Description,
            ReturnRate = i.ReturnRate,
            ExpectedReturn = i.ExpectedReturn,
            RepaymentDueDate = i.RepaymentDueDate,
            Status = i.Status,
            StatusText = InvestmentStatuses.GetById(i.Status)?.Description,
            IsFunded = i.IsFunded,
            FundedAt = i.FundedAt,
            IsRepaid = i.IsRepaid,
            RepaidAt = i.RepaidAt,
            RepaidAmount = i.RepaidAmount,
            TermsAndConditions = i.TermsAndConditions,
            Notes = i.Notes,
            CreatedAt = i.CreatedAt
        };
    }

    // Helper: Transport modes string'ini okunabilir metne cevir
    private static string? GetTransportModesText(string? transportModes)
    {
        if (string.IsNullOrEmpty(transportModes)) return null;

        var ids = transportModes.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var names = new List<string>();
        foreach (var idStr in ids)
        {
            if (int.TryParse(idStr.Trim(), out var id))
            {
                var mode = TransportModes.GetById(id);
                if (mode != null)
                    names.Add(mode.Description ?? mode.SystemName);
            }
        }
        return names.Count > 0 ? string.Join(", ", names) : null;
    }
}

// Notification Types extension for Order
public static class NotificationTypes
{
    public static readonly TypeItem NewOrder = new(100, "NewOrder", "NotificationType.NewOrder", "Yeni sipariş", "bi-cart-plus", "bg-success", 100);
    public static readonly TypeItem OrderConfirmed = new(101, "OrderConfirmed", "NotificationType.OrderConfirmed", "Sipariş onaylandı", "bi-check-circle", "bg-primary", 101);
    public static readonly TypeItem OrderShipped = new(102, "OrderShipped", "NotificationType.OrderShipped", "Sipariş kargoda", "bi-truck", "bg-info", 102);
    public static readonly TypeItem OrderDelivered = new(103, "OrderDelivered", "NotificationType.OrderDelivered", "Sipariş teslim edildi", "bi-house-check", "bg-success", 103);
}
