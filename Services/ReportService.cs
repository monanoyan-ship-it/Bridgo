using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Report;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================
    // SELLER REPORTS
    // ============================================

    public async Task<SellerReportDto> GetSellerReportAsync(int vendorId, ReportFilterDto filter)
    {
        return new SellerReportDto
        {
            Overview = await GetSellerOverviewAsync(vendorId),
            SalesByPeriod = await GetSalesByPeriodAsync(vendorId, filter),
            TopProducts = await GetTopProductsAsync(vendorId, 10),
            TopCustomers = await GetTopCustomersAsync(vendorId, 10)
        };
    }

    public async Task<SalesOverviewDto> GetSellerOverviewAsync(int vendorId)
    {
        var orders = await _context.Orders
            .Where(o => o.SellerVendorId == vendorId)
            .ToListAsync();

        var products = await _context.Products
            .Where(p => p.VendorId == vendorId)
            .ToListAsync();

        var lowStockCount = products.Count(p => p.StockQuantity <= (p.MinStockLevel ?? 10));

        return new SalesOverviewDto
        {
            TotalRevenue = orders.Where(o => o.Status == OrderStatuses.Delivered.Id || o.Status == OrderStatuses.Completed.Id)
                                 .Sum(o => o.TotalAmount),
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(o => o.Status == OrderStatuses.PendingPayment.Id || o.Status == OrderStatuses.PendingConfirm.Id || o.Status == OrderStatuses.Processing.Id),
            CompletedOrders = orders.Count(o => o.Status == OrderStatuses.Delivered.Id || o.Status == OrderStatuses.Completed.Id),
            CancelledOrders = orders.Count(o => o.Status == OrderStatuses.Cancelled.Id),
            AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
            TotalProducts = products.Count,
            LowStockProducts = lowStockCount
        };
    }

    public async Task<List<SalesByPeriodDto>> GetSalesByPeriodAsync(int vendorId, ReportFilterDto filter)
    {
        var fromDate = filter.FromDate ?? DateTime.UtcNow.AddMonths(-12);
        var toDate = filter.ToDate ?? DateTime.UtcNow;

        var orders = await _context.Orders
            .Where(o => o.SellerVendorId == vendorId &&
                        o.CreatedAt >= fromDate &&
                        o.CreatedAt <= toDate &&
                        (o.Status == OrderStatuses.Delivered.Id || o.Status == OrderStatuses.Completed.Id))
            .ToListAsync();

        var result = new List<SalesByPeriodDto>();

        switch (filter.Period?.ToLower())
        {
            case "day":
                result = orders.GroupBy(o => o.CreatedAt.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new SalesByPeriodDto
                    {
                        Period = g.Key.ToString("dd MMM"),
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    }).ToList();
                break;

            case "week":
                result = orders.GroupBy(o => new { Year = o.CreatedAt.Year, Week = GetWeekOfYear(o.CreatedAt) })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Week)
                    .Select(g => new SalesByPeriodDto
                    {
                        Period = $"Hafta {g.Key.Week}, {g.Key.Year}",
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    }).ToList();
                break;

            case "year":
                result = orders.GroupBy(o => o.CreatedAt.Year)
                    .OrderBy(g => g.Key)
                    .Select(g => new SalesByPeriodDto
                    {
                        Period = g.Key.ToString(),
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    }).ToList();
                break;

            default: // month
                result = orders.GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new SalesByPeriodDto
                    {
                        Period = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    }).ToList();
                break;
        }

        return result;
    }

    public async Task<List<TopProductDto>> GetTopProductsAsync(int vendorId, int count = 10)
    {
        var result = await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .Where(oi => oi.Order != null &&
                         oi.Order.SellerVendorId == vendorId &&
                         (oi.Order.Status == OrderStatuses.Delivered.Id || oi.Order.Status == OrderStatuses.Completed.Id))
            .GroupBy(oi => new { oi.ProductId, ProductName = oi.Product != null ? oi.Product.Name : "", SKU = oi.Product != null ? oi.Product.SKU : null })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                SKU = g.Key.SKU,
                TotalQuantity = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.TotalPrice),
                OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
            })
            .OrderByDescending(p => p.TotalRevenue)
            .Take(count)
            .ToListAsync();

        return result;
    }

    public async Task<List<TopCustomerDto>> GetTopCustomersAsync(int vendorId, int count = 10)
    {
        var result = await _context.Orders
            .Include(o => o.BuyerVendor)
            .Where(o => o.SellerVendorId == vendorId &&
                        (o.Status == OrderStatuses.Delivered.Id || o.Status == OrderStatuses.Completed.Id))
            .GroupBy(o => new { o.BuyerVendorId, VendorName = o.BuyerVendor != null ? o.BuyerVendor.CompanyName : "" })
            .Select(g => new TopCustomerDto
            {
                VendorId = g.Key.BuyerVendorId,
                VendorName = g.Key.VendorName,
                OrderCount = g.Count(),
                TotalSpent = g.Sum(o => o.TotalAmount),
                LastOrderDate = g.Max(o => o.CreatedAt)
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(count)
            .ToListAsync();

        return result;
    }

    // ============================================
    // BUYER REPORTS
    // ============================================

    public async Task<BuyerReportDto> GetBuyerReportAsync(int vendorId, ReportFilterDto filter)
    {
        return new BuyerReportDto
        {
            Overview = await GetBuyerOverviewAsync(vendorId),
            PurchasesByPeriod = await GetPurchasesByPeriodAsync(vendorId, filter),
            TopSuppliers = await GetTopSuppliersAsync(vendorId, 10),
            TopProducts = await GetTopPurchasedProductsAsync(vendorId, 10)
        };
    }

    public async Task<PurchaseOverviewDto> GetBuyerOverviewAsync(int vendorId)
    {
        var orders = await _context.Orders
            .Where(o => o.BuyerVendorId == vendorId)
            .ToListAsync();

        var demands = await _context.PublicDemands
            .Where(d => d.VendorId == vendorId)
            .ToListAsync();

        var demandResponses = await _context.DemandResponses
            .Include(r => r.Demand)
            .Where(r => r.Demand != null && r.Demand.VendorId == vendorId)
            .CountAsync();

        return new PurchaseOverviewDto
        {
            TotalSpent = orders.Where(o => o.Status == OrderStatuses.Delivered.Id || o.Status == OrderStatuses.Completed.Id)
                               .Sum(o => o.TotalAmount),
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(o => o.Status == OrderStatuses.PendingPayment.Id || o.Status == OrderStatuses.PendingConfirm.Id || o.Status == OrderStatuses.Processing.Id),
            CompletedOrders = orders.Count(o => o.Status == OrderStatuses.Delivered.Id || o.Status == OrderStatuses.Completed.Id),
            ActiveDemands = demands.Count(d => d.Status == 2),  // 2 = Active
            ReceivedProposals = demandResponses,
            AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0
        };
    }

    public async Task<List<PurchaseByPeriodDto>> GetPurchasesByPeriodAsync(int vendorId, ReportFilterDto filter)
    {
        var fromDate = filter.FromDate ?? DateTime.UtcNow.AddMonths(-12);
        var toDate = filter.ToDate ?? DateTime.UtcNow;

        var orders = await _context.Orders
            .Where(o => o.BuyerVendorId == vendorId &&
                        o.CreatedAt >= fromDate &&
                        o.CreatedAt <= toDate &&
                        (o.Status == OrderStatuses.Delivered.Id || o.Status == OrderStatuses.Completed.Id))
            .ToListAsync();

        var result = new List<PurchaseByPeriodDto>();

        switch (filter.Period?.ToLower())
        {
            case "day":
                result = orders.GroupBy(o => o.CreatedAt.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new PurchaseByPeriodDto
                    {
                        Period = g.Key.ToString("dd MMM"),
                        Spent = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    }).ToList();
                break;

            case "week":
                result = orders.GroupBy(o => new { Year = o.CreatedAt.Year, Week = GetWeekOfYear(o.CreatedAt) })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Week)
                    .Select(g => new PurchaseByPeriodDto
                    {
                        Period = $"Hafta {g.Key.Week}, {g.Key.Year}",
                        Spent = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    }).ToList();
                break;

            case "year":
                result = orders.GroupBy(o => o.CreatedAt.Year)
                    .OrderBy(g => g.Key)
                    .Select(g => new PurchaseByPeriodDto
                    {
                        Period = g.Key.ToString(),
                        Spent = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    }).ToList();
                break;

            default: // month
                result = orders.GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new PurchaseByPeriodDto
                    {
                        Period = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        Spent = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    }).ToList();
                break;
        }

        return result;
    }

    public async Task<List<TopSupplierDto>> GetTopSuppliersAsync(int vendorId, int count = 10)
    {
        var result = await _context.Orders
            .Include(o => o.SellerVendor)
            .Where(o => o.BuyerVendorId == vendorId &&
                        (o.Status == OrderStatuses.Delivered.Id || o.Status == OrderStatuses.Completed.Id))
            .GroupBy(o => new { o.SellerVendorId, VendorName = o.SellerVendor != null ? o.SellerVendor.CompanyName : "" })
            .Select(g => new TopSupplierDto
            {
                VendorId = g.Key.SellerVendorId,
                VendorName = g.Key.VendorName,
                OrderCount = g.Count(),
                TotalSpent = g.Sum(o => o.TotalAmount),
                LastOrderDate = g.Max(o => o.CreatedAt)
            })
            .OrderByDescending(s => s.TotalSpent)
            .Take(count)
            .ToListAsync();

        return result;
    }

    public async Task<List<TopPurchasedProductDto>> GetTopPurchasedProductsAsync(int vendorId, int count = 10)
    {
        var result = await _context.OrderItems
            .Include(oi => oi.Order)
            .ThenInclude(o => o!.SellerVendor)
            .Include(oi => oi.Product)
            .Where(oi => oi.Order != null &&
                         oi.Order.BuyerVendorId == vendorId &&
                         (oi.Order.Status == OrderStatuses.Delivered.Id || oi.Order.Status == OrderStatuses.Completed.Id))
            .GroupBy(oi => new {
                oi.ProductId,
                ProductName = oi.Product != null ? oi.Product.Name : "",
                SupplierName = oi.Order != null && oi.Order.SellerVendor != null ? oi.Order.SellerVendor.CompanyName : ""
            })
            .Select(g => new TopPurchasedProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                SupplierName = g.Key.SupplierName,
                TotalQuantity = g.Sum(oi => oi.Quantity),
                TotalSpent = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(p => p.TotalSpent)
            .Take(count)
            .ToListAsync();

        return result;
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private static int GetWeekOfYear(DateTime date)
    {
        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }
}
