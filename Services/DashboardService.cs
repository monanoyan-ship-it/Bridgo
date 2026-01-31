using Bridgo.Data;
using Bridgo.DTOs.Dashboard;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bridgo.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    // PublicDemand Status values (no TypeDefinitions class)
    private const int DemandStatusActive = 2;

    // DemandResponse Status values (no TypeDefinitions class)
    private const int DemandResponseStatusPending = 1;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================
    // SATICI DASHBOARD (ID: 2)
    // ============================================
    public async Task<SellerDashboardDto> GetSellerDashboardAsync(int vendorId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Order stats
        var pendingOrders = await _context.Orders
            .CountAsync(o => o.SellerVendorId == vendorId && !o.IsDeleted &&
                o.Status == OrderStatuses.PendingConfirm.Id);

        var processingOrders = await _context.Orders
            .CountAsync(o => o.SellerVendorId == vendorId && !o.IsDeleted &&
                (o.Status == OrderStatuses.Confirmed.Id || o.Status == OrderStatuses.Processing.Id));

        // Monthly revenue (confirmed+ orders)
        var confirmedStatusIds = new[] {
            OrderStatuses.Confirmed.Id, OrderStatuses.Processing.Id, OrderStatuses.Shipped.Id,
            OrderStatuses.InTransit.Id, OrderStatuses.Delivered.Id, OrderStatuses.Completed.Id
        };
        var monthlyRevenue = await _context.Orders
            .Where(o => o.SellerVendorId == vendorId && !o.IsDeleted &&
                confirmedStatusIds.Contains(o.Status) &&
                o.CreatedAt >= monthStart)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        // Product stats
        var activeProducts = await _context.Products
            .CountAsync(p => p.VendorId == vendorId && !p.IsDeleted &&
                p.ProductStatusId == ProductStatuses.Active.Id);

        var lowStockProducts = await _context.Products
            .CountAsync(p => p.VendorId == vendorId && !p.IsDeleted &&
                p.ProductStatusId == ProductStatuses.Active.Id &&
                p.TrackStock && p.MinStockLevel.HasValue &&
                p.StockQuantity <= p.MinStockLevel);

        // Inquiry stats
        var newInquiries = await _context.ProductInquiries
            .CountAsync(i => i.SellerVendorId == vendorId && !i.IsDeleted && !i.IsReadBySeller);

        // Demand response stats - seller's responses that are pending
        var pendingDemandResponses = await _context.DemandResponses
            .CountAsync(r => r.SupplierVendorId == vendorId && !r.IsDeleted &&
                r.Status == DemandResponseStatusPending);

        // Recent orders
        var recentOrders = await _context.Orders
            .Include(o => o.BuyerVendor)
            .Where(o => o.SellerVendorId == vendorId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new RecentOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.BuyerVendor != null ? o.BuyerVendor.CompanyName : "",
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                StatusId = o.Status,
                StatusName = OrderStatuses.GetById(o.Status) != null ? OrderStatuses.GetById(o.Status)!.Description : "",
                StatusCssClass = OrderStatuses.GetById(o.Status) != null ? OrderStatuses.GetById(o.Status)!.CssClass ?? "" : "",
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        // Recent inquiries
        var recentInquiries = await _context.ProductInquiries
            .Include(i => i.Product)
            .Include(i => i.BuyerVendor)
            .Where(i => i.SellerVendorId == vendorId && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .Take(5)
            .Select(i => new RecentInquiryDto
            {
                Id = i.Id,
                ProductName = i.Product != null ? i.Product.Name : "",
                BuyerName = i.BuyerVendor != null ? i.BuyerVendor.CompanyName : "",
                Quantity = i.Quantity,
                StatusId = i.Status,
                StatusName = ProductInquiryStatuses.GetById(i.Status) != null ? ProductInquiryStatuses.GetById(i.Status)!.Description : "",
                IsRead = i.IsReadBySeller,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        // Top products (by order count this month)
        var topProducts = await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .ThenInclude(p => p!.Images)
            .Where(oi => oi.Order != null && oi.Order.SellerVendorId == vendorId && !oi.Order.IsDeleted &&
                confirmedStatusIds.Contains(oi.Order.Status) &&
                oi.Order.CreatedAt >= monthStart)
            .GroupBy(oi => new { oi.ProductId, ProductName = oi.Product != null ? oi.Product.Name : "" })
            .Select(g => new TopProductDto
            {
                Id = g.Key.ProductId,
                Name = g.Key.ProductName,
                ImageUrl = g.First().Product != null ? g.First().Product!.Images.Where(i => i.IsMain && !i.IsDeleted).Select(i => i.Url).FirstOrDefault() : null,
                SoldQuantity = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.TotalPrice),
                Currency = "TRY"
            })
            .OrderByDescending(p => p.Revenue)
            .Take(5)
            .ToListAsync();

        return new SellerDashboardDto
        {
            PendingOrders = pendingOrders,
            ProcessingOrders = processingOrders,
            MonthlyRevenue = monthlyRevenue,
            ActiveProducts = activeProducts,
            LowStockProducts = lowStockProducts,
            NewInquiries = newInquiries,
            PendingDemandResponses = pendingDemandResponses,
            RecentOrders = recentOrders,
            RecentInquiries = recentInquiries,
            TopProducts = topProducts
        };
    }

    // ============================================
    // ALICI DASHBOARD (ID: 3)
    // ============================================
    public async Task<BuyerDashboardDto> GetBuyerDashboardAsync(int vendorId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Active orders (not delivered/completed/cancelled)
        var activeStatusIds = new[] {
            OrderStatuses.PendingPayment.Id, OrderStatuses.PendingConfirm.Id,
            OrderStatuses.Confirmed.Id, OrderStatuses.Processing.Id,
            OrderStatuses.Shipped.Id, OrderStatuses.InTransit.Id
        };
        var activeOrders = await _context.Orders
            .CountAsync(o => o.BuyerVendorId == vendorId && !o.IsDeleted &&
                activeStatusIds.Contains(o.Status));

        // Pending deliveries (shipped, in transit)
        var deliveryStatusIds = new[] { OrderStatuses.Shipped.Id, OrderStatuses.InTransit.Id };
        var pendingDeliveries = await _context.Orders
            .CountAsync(o => o.BuyerVendorId == vendorId && !o.IsDeleted &&
                deliveryStatusIds.Contains(o.Status));

        // Active demands
        var activeDemands = await _context.PublicDemands
            .CountAsync(d => d.VendorId == vendorId && !d.IsDeleted &&
                d.Status == DemandStatusActive);

        // Pending proposals (responses to my demands)
        var pendingProposals = await _context.DemandResponses
            .Include(r => r.Demand)
            .CountAsync(r => r.Demand.VendorId == vendorId && !r.IsDeleted &&
                r.Status == DemandResponseStatusPending);

        // Monthly spending
        var completedStatusIds = new[] {
            OrderStatuses.Delivered.Id, OrderStatuses.Completed.Id
        };
        var monthlySpending = await _context.Orders
            .Where(o => o.BuyerVendorId == vendorId && !o.IsDeleted &&
                completedStatusIds.Contains(o.Status) &&
                o.CreatedAt >= monthStart)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        // Recent orders
        var recentOrders = await _context.Orders
            .Include(o => o.SellerVendor)
            .Where(o => o.BuyerVendorId == vendorId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new RecentOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                VendorName = o.SellerVendor != null ? o.SellerVendor.CompanyName : "",
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                StatusId = o.Status,
                StatusName = OrderStatuses.GetById(o.Status) != null ? OrderStatuses.GetById(o.Status)!.Description : "",
                StatusCssClass = OrderStatuses.GetById(o.Status) != null ? OrderStatuses.GetById(o.Status)!.CssClass ?? "" : "",
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        // Recent demands
        var recentDemands = await _context.PublicDemands
            .Include(d => d.Category)
            .Include(d => d.Responses)
            .Where(d => d.VendorId == vendorId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Take(5)
            .Select(d => new RecentDemandDto
            {
                Id = d.Id,
                Title = d.Title,
                CategoryName = d.Category != null ? d.Category.Name : null,
                Quantity = d.Quantity ?? 0,
                StatusId = d.Status,
                StatusName = GetDemandStatusName(d.Status),
                StatusCssClass = GetDemandStatusCssClass(d.Status),
                ResponseCount = d.Responses.Count(r => !r.IsDeleted),
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();

        // Pending proposals list
        var pendingProposalsList = await _context.DemandResponses
            .Include(r => r.Demand)
            .Include(r => r.SupplierVendor)
            .Where(r => r.Demand.VendorId == vendorId && !r.IsDeleted &&
                r.Status == DemandResponseStatusPending)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new PendingProposalDto
            {
                Id = r.Id,
                DemandTitle = r.Demand.Title,
                VendorName = r.SupplierVendor != null ? r.SupplierVendor.CompanyName : "",
                UnitPrice = r.UnitPrice ?? 0,
                TotalPrice = r.TotalPrice ?? 0,
                Currency = r.Currency ?? "TRY",
                ValidUntil = r.ValidUntil ?? DateTime.MaxValue,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new BuyerDashboardDto
        {
            ActiveOrders = activeOrders,
            PendingDeliveries = pendingDeliveries,
            ActiveDemands = activeDemands,
            PendingProposals = pendingProposals,
            MonthlySpending = monthlySpending,
            RecentOrders = recentOrders,
            RecentDemands = recentDemands,
            PendingProposalsList = pendingProposalsList
        };
    }

    // ============================================
    // TASIMACI DASHBOARD (ID: 4)
    // ============================================
    public async Task<CarrierDashboardDto> GetCarrierDashboardAsync(int vendorId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Open logistics requests
        var openRequests = await _context.OrderServiceRequests
            .CountAsync(r => r.ServiceType == ServiceTypes.Logistics.Id &&
                !r.IsDeleted && r.Status == ServiceRequestStatuses.Open.Id);

        // Service provider's active shipments (quotes accepted, request in progress)
        var activeShipments = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .CountAsync(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Logistics.Id &&
                q.Status == ServiceQuoteStatuses.Accepted.Id &&
                q.ServiceRequest.Status == ServiceRequestStatuses.QuoteSelected.Id);

        // Pending quotes (submitted but not decided)
        var pendingQuotes = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .CountAsync(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Logistics.Id &&
                q.Status == ServiceQuoteStatuses.Pending.Id);

        // Completed this month (quotes accepted this month - no explicit completed status)
        var completedThisMonth = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .CountAsync(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Logistics.Id &&
                q.Status == ServiceQuoteStatuses.Accepted.Id &&
                q.UpdatedAt >= monthStart);

        // Monthly earnings
        var monthlyEarnings = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .Where(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Logistics.Id &&
                q.Status == ServiceQuoteStatuses.Accepted.Id &&
                q.UpdatedAt >= monthStart)
            .SumAsync(q => (decimal?)q.QuoteAmount) ?? 0;

        // Recent requests
        var recentRequests = await _context.OrderServiceRequests
            .Include(r => r.Order).ThenInclude(o => o!.BuyerVendor)
            .Where(r => r.ServiceType == ServiceTypes.Logistics.Id && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new ServiceRequestSummaryDto
            {
                Id = r.Id,
                Title = "Lojistik Talebi #" + r.Id,
                RequestorName = r.Order != null && r.Order.BuyerVendor != null ? r.Order.BuyerVendor.CompanyName : "",
                StatusId = r.Status,
                StatusName = ServiceRequestStatuses.GetById(r.Status) != null ? ServiceRequestStatuses.GetById(r.Status)!.Description : "",
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new CarrierDashboardDto
        {
            OpenRequests = openRequests,
            ActiveShipments = activeShipments,
            PendingQuotes = pendingQuotes,
            CompletedThisMonth = completedThisMonth,
            MonthlyEarnings = monthlyEarnings,
            RecentRequests = recentRequests
        };
    }

    // ============================================
    // SIGORTA DASHBOARD (ID: 5)
    // ============================================
    public async Task<InsuranceDashboardDto> GetInsuranceDashboardAsync(int vendorId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Open insurance requests
        var openRequests = await _context.OrderServiceRequests
            .CountAsync(r => r.ServiceType == ServiceTypes.Insurance.Id &&
                !r.IsDeleted && r.Status == ServiceRequestStatuses.Open.Id);

        // Active policies (accepted quotes, in progress)
        var activePolicies = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .CountAsync(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Insurance.Id &&
                q.Status == ServiceQuoteStatuses.Accepted.Id &&
                q.ServiceRequest.Status == ServiceRequestStatuses.QuoteSelected.Id);

        // Pending quotes
        var pendingQuotes = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .CountAsync(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Insurance.Id &&
                q.Status == ServiceQuoteStatuses.Pending.Id);

        // Monthly premium (accepted quotes this month)
        var monthlyPremium = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .Where(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Insurance.Id &&
                q.Status == ServiceQuoteStatuses.Accepted.Id &&
                q.UpdatedAt >= monthStart)
            .SumAsync(q => (decimal?)q.QuoteAmount) ?? 0;

        // Recent requests
        var recentRequests = await _context.OrderServiceRequests
            .Include(r => r.Order).ThenInclude(o => o!.BuyerVendor)
            .Where(r => r.ServiceType == ServiceTypes.Insurance.Id && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new ServiceRequestSummaryDto
            {
                Id = r.Id,
                Title = "Sigorta Talebi #" + r.Id,
                RequestorName = r.Order != null && r.Order.BuyerVendor != null ? r.Order.BuyerVendor.CompanyName : "",
                StatusId = r.Status,
                StatusName = ServiceRequestStatuses.GetById(r.Status) != null ? ServiceRequestStatuses.GetById(r.Status)!.Description : "",
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new InsuranceDashboardDto
        {
            OpenRequests = openRequests,
            ActivePolicies = activePolicies,
            PendingQuotes = pendingQuotes,
            PendingClaims = 0, // TODO: Claims tablosu eklendiginde
            MonthlyPremium = monthlyPremium,
            RecentRequests = recentRequests
        };
    }

    // ============================================
    // GUMRUK DASHBOARD (ID: 6)
    // ============================================
    public async Task<CustomsDashboardDto> GetCustomsDashboardAsync(int vendorId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Open customs requests
        var openRequests = await _context.OrderServiceRequests
            .CountAsync(r => r.ServiceType == ServiceTypes.Customs.Id &&
                !r.IsDeleted && r.Status == ServiceRequestStatuses.Open.Id);

        // Active declarations (accepted quotes, in progress)
        var activeDeclarations = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .CountAsync(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Customs.Id &&
                q.Status == ServiceQuoteStatuses.Accepted.Id &&
                q.ServiceRequest.Status == ServiceRequestStatuses.QuoteSelected.Id);

        // Pending quotes
        var pendingQuotes = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .CountAsync(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Customs.Id &&
                q.Status == ServiceQuoteStatuses.Pending.Id);

        // Completed this month (quotes accepted this month)
        var completedThisMonth = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .CountAsync(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Customs.Id &&
                q.Status == ServiceQuoteStatuses.Accepted.Id &&
                q.UpdatedAt >= monthStart);

        // Monthly earnings
        var monthlyEarnings = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .Where(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Customs.Id &&
                q.Status == ServiceQuoteStatuses.Accepted.Id &&
                q.UpdatedAt >= monthStart)
            .SumAsync(q => (decimal?)q.QuoteAmount) ?? 0;

        // Recent requests
        var recentRequests = await _context.OrderServiceRequests
            .Include(r => r.Order).ThenInclude(o => o!.BuyerVendor)
            .Where(r => r.ServiceType == ServiceTypes.Customs.Id && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new ServiceRequestSummaryDto
            {
                Id = r.Id,
                Title = "Gumruk Talebi #" + r.Id,
                RequestorName = r.Order != null && r.Order.BuyerVendor != null ? r.Order.BuyerVendor.CompanyName : "",
                StatusId = r.Status,
                StatusName = ServiceRequestStatuses.GetById(r.Status) != null ? ServiceRequestStatuses.GetById(r.Status)!.Description : "",
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new CustomsDashboardDto
        {
            OpenRequests = openRequests,
            ActiveDeclarations = activeDeclarations,
            PendingQuotes = pendingQuotes,
            CompletedThisMonth = completedThisMonth,
            MonthlyEarnings = monthlyEarnings,
            RecentRequests = recentRequests
        };
    }

    // ============================================
    // GOZETIM DASHBOARD (ID: 7)
    // ============================================
    public async Task<SurveyDashboardDto> GetSurveyDashboardAsync(int vendorId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Open survey requests
        var openRequests = await _context.OrderServiceRequests
            .CountAsync(r => r.ServiceType == ServiceTypes.Survey.Id &&
                !r.IsDeleted && r.Status == ServiceRequestStatuses.Open.Id);

        // Scheduled inspections (accepted quotes, in progress)
        var scheduledInspections = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .CountAsync(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Survey.Id &&
                q.Status == ServiceQuoteStatuses.Accepted.Id &&
                q.ServiceRequest.Status == ServiceRequestStatuses.QuoteSelected.Id);

        // Pending reports (accepted but quote selected - active work)
        var pendingReports = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .CountAsync(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Survey.Id &&
                q.Status == ServiceQuoteStatuses.Accepted.Id &&
                q.ServiceRequest.Status == ServiceRequestStatuses.QuoteSelected.Id);

        // Completed this month (quotes accepted this month)
        var completedThisMonth = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .CountAsync(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Survey.Id &&
                q.Status == ServiceQuoteStatuses.Accepted.Id &&
                q.UpdatedAt >= monthStart);

        // Monthly earnings
        var monthlyEarnings = await _context.OrderServiceQuotes
            .Include(q => q.ServiceRequest)
            .Where(q => q.ProviderVendorId == vendorId && !q.IsDeleted &&
                q.ServiceRequest != null &&
                q.ServiceRequest.ServiceType == ServiceTypes.Survey.Id &&
                q.Status == ServiceQuoteStatuses.Accepted.Id &&
                q.UpdatedAt >= monthStart)
            .SumAsync(q => (decimal?)q.QuoteAmount) ?? 0;

        // Recent requests
        var recentRequests = await _context.OrderServiceRequests
            .Include(r => r.Order).ThenInclude(o => o!.BuyerVendor)
            .Where(r => r.ServiceType == ServiceTypes.Survey.Id && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new ServiceRequestSummaryDto
            {
                Id = r.Id,
                Title = "Gozetim Talebi #" + r.Id,
                RequestorName = r.Order != null && r.Order.BuyerVendor != null ? r.Order.BuyerVendor.CompanyName : "",
                StatusId = r.Status,
                StatusName = ServiceRequestStatuses.GetById(r.Status) != null ? ServiceRequestStatuses.GetById(r.Status)!.Description : "",
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new SurveyDashboardDto
        {
            OpenRequests = openRequests,
            ScheduledInspections = scheduledInspections,
            PendingReports = pendingReports,
            CompletedThisMonth = completedThisMonth,
            MonthlyEarnings = monthlyEarnings,
            RecentRequests = recentRequests
        };
    }

    // ============================================
    // YATIRIMCI DASHBOARD (ID: 8)
    // ============================================
    public async Task<InvestorDashboardDto> GetInvestorDashboardAsync(int vendorId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Available opportunities (open financing requests)
        var availableOpportunities = await _context.FinancingRequests
            .CountAsync(f => !f.IsDeleted && f.Status == FinancingRequestStatuses.Open.Id);

        // Active investments (my accepted offers)
        var activeInvestments = await _context.InvestmentOffers
            .CountAsync(o => o.InvestorVendorId == vendorId && !o.IsDeleted &&
                o.Status == InvestmentOfferStatuses.Accepted.Id);

        // Pending offers
        var pendingOffers = await _context.InvestmentOffers
            .CountAsync(o => o.InvestorVendorId == vendorId && !o.IsDeleted &&
                o.Status == InvestmentOfferStatuses.Pending.Id);

        // Total portfolio value
        var totalPortfolioValue = await _context.InvestmentOffers
            .Where(o => o.InvestorVendorId == vendorId && !o.IsDeleted &&
                o.Status == InvestmentOfferStatuses.Accepted.Id)
            .SumAsync(o => (decimal?)o.OfferedAmount) ?? 0;

        // Expected monthly return (simplified calculation based on daily rate * 30)
        var expectedMonthlyReturn = await _context.InvestmentOffers
            .Include(o => o.FinancingRequest)
            .Where(o => o.InvestorVendorId == vendorId && !o.IsDeleted &&
                o.Status == InvestmentOfferStatuses.Accepted.Id)
            .SumAsync(o => (decimal?)(o.OfferedAmount * o.InterestRate / 100 / 12)) ?? 0;

        // Recent opportunities
        var recentOpportunities = await _context.FinancingRequests
            .Include(f => f.RequesterVendor)
            .Where(f => !f.IsDeleted && f.Status == FinancingRequestStatuses.Open.Id)
            .OrderByDescending(f => f.CreatedAt)
            .Take(5)
            .Select(f => new InvestmentOpportunitySummaryDto
            {
                Id = f.Id,
                Title = f.Title,
                VendorName = f.RequesterVendor != null ? f.RequesterVendor.CompanyName : "",
                RequestedAmount = f.RequestedAmount,
                Currency = f.Currency,
                MinInterestRate = 0, // Financing doesn't have min, only max
                MaxInterestRate = f.MaxInterestRate ?? 0,
                DurationMonths = f.DurationDays / 30, // Convert days to months
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        // Active investments list
        var activeInvestmentsList = await _context.InvestmentOffers
            .Include(o => o.FinancingRequest).ThenInclude(f => f!.RequesterVendor)
            .Where(o => o.InvestorVendorId == vendorId && !o.IsDeleted &&
                o.Status == InvestmentOfferStatuses.Accepted.Id)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new ActiveInvestmentDto
            {
                Id = o.Id,
                Title = o.FinancingRequest != null ? o.FinancingRequest.Title : "",
                VendorName = o.FinancingRequest != null && o.FinancingRequest.RequesterVendor != null
                    ? o.FinancingRequest.RequesterVendor.CompanyName : "",
                InvestedAmount = o.OfferedAmount,
                Currency = o.FinancingRequest != null ? o.FinancingRequest.Currency : "TRY",
                InterestRate = o.InterestRate,
                ExpectedReturn = o.TotalRepaymentAmount - o.OfferedAmount,
                MaturityDate = o.FinancingRequest != null && o.FinancingRequest.RepaymentDate.HasValue
                    ? o.FinancingRequest.RepaymentDate.Value
                    : o.CreatedAt.AddDays(o.FinancingRequest != null ? o.FinancingRequest.DurationDays : 30),
                StatusId = o.Status,
                StatusName = InvestmentOfferStatuses.GetById(o.Status) != null
                    ? InvestmentOfferStatuses.GetById(o.Status)!.Description : ""
            })
            .ToListAsync();

        return new InvestorDashboardDto
        {
            AvailableOpportunities = availableOpportunities,
            ActiveInvestments = activeInvestments,
            PendingOffers = pendingOffers,
            TotalPortfolioValue = totalPortfolioValue,
            ExpectedMonthlyReturn = expectedMonthlyReturn,
            RecentOpportunities = recentOpportunities,
            ActiveInvestmentsList = activeInvestmentsList
        };
    }

    // ============================================
    // FIRMA GENEL BILGILERI DASHBOARD
    // ============================================
    public async Task<CompanyDashboardDto> GetCompanyDashboardAsync(int vendorId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Vendor bilgisi
        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Id == vendorId && !v.IsDeleted);

        if (vendor == null)
            return new CompanyDashboardDto();

        var result = new CompanyDashboardDto
        {
            VendorId = vendor.Id,
            CompanyName = vendor.CompanyName,
            LogoUrl = vendor.LogoUrl,
            Email = vendor.Email,
            Phone = vendor.Phone,
            Website = vendor.Website,
            TaxNumber = vendor.TaxNumber,
            TradeRegistryNo = vendor.TradeRegistryNo,
            IsVerified = vendor.IsVerified,
            IsProfileComplete = vendor.IsProfileComplete,
            CreatedAt = vendor.CreatedAt
        };

        // Capability'ler - TypeDefinitions'dan static olarak alinir
        var capabilityMappings = await _context.VendorCapabilityMappings
            .Where(m => m.VendorId == vendorId && m.IsActive)
            .ToListAsync();

        result.Capabilities = capabilityMappings
            .Select(m =>
            {
                var capability = Capabilities.GetById(m.CapabilityId);
                if (capability == null) return null;
                return new CompanyCapabilityInfoDto
                {
                    Id = capability.Id,
                    Name = capability.Description ?? capability.SystemName,
                    Code = capability.SystemName.ToLower(),
                    Icon = capability.Icon,
                    AddedAt = m.CreatedAt
                };
            })
            .Where(c => c != null)
            .Cast<CompanyCapabilityInfoDto>()
            .ToList();

        // Ekip bilgileri
        result.TeamMemberCount = await _context.VendorTeamMembers
            .CountAsync(t => t.VendorId == vendorId && !t.IsDeleted);

        result.ActiveUserCount = await _context.Users
            .CountAsync(u => u.VendorId == vendorId && u.IsActive);

        result.PendingInviteCount = await _context.VendorTeamMembers
            .CountAsync(t => t.VendorId == vendorId && !t.IsDeleted &&
                t.TeamMemberStatusId == TeamMemberStatuses.Pending.Id);

        // Merkez adres
        var headquartersAddress = await _context.Addresses
            .Include(a => a.CountryEntity)
            .Include(a => a.StateEntity)
            .Where(a => a.VendorId == vendorId && !a.IsDeleted && a.IsDefault)
            .Select(a => new CompanyAddressDto
            {
                Id = a.Id,
                Title = a.Title,
                AddressLine = a.AddressLine,
                City = a.City,
                StateName = a.StateEntity != null ? a.StateEntity.Name : null,
                CountryName = a.CountryEntity != null ? a.CountryEntity.Name : null,
                PostalCode = a.PostalCode
            })
            .FirstOrDefaultAsync();

        result.HeadquartersAddress = headquartersAddress;
        result.TotalAddressCount = await _context.Addresses
            .CountAsync(a => a.VendorId == vendorId && !a.IsDeleted);

        result.WarehouseCount = await _context.Warehouses
            .CountAsync(w => w.VendorId == vendorId && !w.IsDeleted);

        // Urun bilgileri (Satici capability varsa)
        var hasSellerCapability = await _context.VendorCapabilityMappings
            .AnyAsync(m => m.VendorId == vendorId && m.CapabilityId == 2); // Satici = 2

        if (hasSellerCapability)
        {
            result.TotalProductCount = await _context.Products
                .CountAsync(p => p.VendorId == vendorId && !p.IsDeleted);

            result.ActiveProductCount = await _context.Products
                .CountAsync(p => p.VendorId == vendorId && !p.IsDeleted &&
                    p.ProductStatusId == ProductStatuses.Active.Id);

            result.CategoryCount = await _context.Products
                .Where(p => p.VendorId == vendorId && !p.IsDeleted && p.CategoryId.HasValue)
                .Select(p => p.CategoryId)
                .Distinct()
                .CountAsync();
        }

        // Siparis ozeti
        // Satis siparisleri (Satici olarak)
        var salesOrders = await _context.Orders
            .Where(o => o.SellerVendorId == vendorId && !o.IsDeleted)
            .ToListAsync();

        // Alis siparisleri (Alici olarak)
        var purchaseOrders = await _context.Orders
            .Where(o => o.BuyerVendorId == vendorId && !o.IsDeleted)
            .ToListAsync();

        result.TotalOrderCount = salesOrders.Count + purchaseOrders.Count;

        var activeStatusIds = new[] {
            OrderStatuses.PendingPayment.Id, OrderStatuses.PendingConfirm.Id,
            OrderStatuses.Confirmed.Id, OrderStatuses.Processing.Id,
            OrderStatuses.Shipped.Id, OrderStatuses.InTransit.Id
        };
        result.ActiveOrderCount = salesOrders.Count(o => activeStatusIds.Contains(o.Status)) +
            purchaseOrders.Count(o => activeStatusIds.Contains(o.Status));

        var confirmedStatusIds = new[] {
            OrderStatuses.Confirmed.Id, OrderStatuses.Processing.Id, OrderStatuses.Shipped.Id,
            OrderStatuses.InTransit.Id, OrderStatuses.Delivered.Id, OrderStatuses.Completed.Id
        };
        result.TotalRevenue = salesOrders.Where(o => confirmedStatusIds.Contains(o.Status))
            .Sum(o => o.TotalAmount);
        result.TotalSpending = purchaseOrders.Where(o => confirmedStatusIds.Contains(o.Status))
            .Sum(o => o.TotalAmount);

        // Son aktiviteler (son 10)
        var activities = new List<RecentActivityDto>();

        // Son siparisler (satici)
        var recentSalesOrders = salesOrders.OrderByDescending(o => o.CreatedAt).Take(3);
        foreach (var order in recentSalesOrders)
        {
            activities.Add(new RecentActivityDto
            {
                Type = "SalesOrder",
                Title = $"Siparis #{order.OrderNumber}",
                Description = $"{order.TotalAmount:N2} {order.Currency}",
                Icon = "bi-cart-check",
                IconColor = "text-success",
                CreatedAt = order.CreatedAt,
                ActionUrl = $"/Orders/Detail/{order.Id}"
            });
        }

        // Son siparisler (alici)
        var recentPurchaseOrders = purchaseOrders.OrderByDescending(o => o.CreatedAt).Take(3);
        foreach (var order in recentPurchaseOrders)
        {
            activities.Add(new RecentActivityDto
            {
                Type = "PurchaseOrder",
                Title = $"Alis #{order.OrderNumber}",
                Description = $"{order.TotalAmount:N2} {order.Currency}",
                Icon = "bi-bag-check",
                IconColor = "text-primary",
                CreatedAt = order.CreatedAt,
                ActionUrl = $"/Orders/MyOrders/{order.Id}"
            });
        }

        // Son talepler
        var recentDemands = await _context.PublicDemands
            .Where(d => d.VendorId == vendorId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Take(3)
            .ToListAsync();

        foreach (var demand in recentDemands)
        {
            activities.Add(new RecentActivityDto
            {
                Type = "Demand",
                Title = demand.Title,
                Description = GetDemandStatusName(demand.Status),
                Icon = "bi-megaphone",
                IconColor = "text-warning",
                CreatedAt = demand.CreatedAt,
                ActionUrl = $"/Demands/Detail/{demand.Slug}"
            });
        }

        // Son urun sorgulari
        var recentInquiries = await _context.ProductInquiries
            .Include(i => i.Product)
            .Where(i => i.SellerVendorId == vendorId && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .Take(3)
            .ToListAsync();

        foreach (var inquiry in recentInquiries)
        {
            activities.Add(new RecentActivityDto
            {
                Type = "Inquiry",
                Title = inquiry.Product?.Name ?? "Urun Sorgusu",
                Description = $"Miktar: {inquiry.Quantity}",
                Icon = "bi-chat-dots",
                IconColor = "text-info",
                CreatedAt = inquiry.CreatedAt,
                ActionUrl = $"/Inquiries/Incoming"
            });
        }

        result.RecentActivities = activities
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .ToList();

        return result;
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private static string GetDemandStatusName(int status)
    {
        return status switch
        {
            1 => "Taslak",
            2 => "Aktif",
            3 => "Kapandi",
            4 => "Tamamlandi",
            5 => "Suresi Doldu",
            6 => "Iptal",
            _ => "Bilinmiyor"
        };
    }

    private static string GetDemandStatusCssClass(int status)
    {
        return status switch
        {
            1 => "bg-secondary",
            2 => "bg-success",
            3 => "bg-info",
            4 => "bg-primary",
            5 => "bg-warning",
            6 => "bg-danger",
            _ => "bg-secondary"
        };
    }
}
