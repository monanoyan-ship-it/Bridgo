namespace Bridgo.DTOs.Report;

// ============================================
// SELLER REPORTS
// ============================================

public class SalesOverviewDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
}

public class SalesByPeriodDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopCustomerDto
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }
}

public class SellerReportDto
{
    public SalesOverviewDto Overview { get; set; } = new();
    public List<SalesByPeriodDto> SalesByPeriod { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
}

// ============================================
// BUYER REPORTS
// ============================================

public class PurchaseOverviewDto
{
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int ActiveDemands { get; set; }
    public int ReceivedProposals { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class PurchaseByPeriodDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Spent { get; set; }
    public int OrderCount { get; set; }
}

public class TopSupplierDto
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }
}

public class TopPurchasedProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalSpent { get; set; }
}

public class BuyerReportDto
{
    public PurchaseOverviewDto Overview { get; set; } = new();
    public List<PurchaseByPeriodDto> PurchasesByPeriod { get; set; } = new();
    public List<TopSupplierDto> TopSuppliers { get; set; } = new();
    public List<TopPurchasedProductDto> TopProducts { get; set; } = new();
}

// ============================================
// FILTER DTOs
// ============================================

public class ReportFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string Period { get; set; } = "month"; // day, week, month, year
    public int? CategoryId { get; set; }
    public int? ProductId { get; set; }
}
