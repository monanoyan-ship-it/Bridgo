using Bridgo.DTOs.Report;

namespace Bridgo.Services.Interfaces;

public interface IReportService
{
    // Seller Reports
    Task<SellerReportDto> GetSellerReportAsync(int vendorId, ReportFilterDto filter);
    Task<SalesOverviewDto> GetSellerOverviewAsync(int vendorId);
    Task<List<SalesByPeriodDto>> GetSalesByPeriodAsync(int vendorId, ReportFilterDto filter);
    Task<List<TopProductDto>> GetTopProductsAsync(int vendorId, int count = 10);
    Task<List<TopCustomerDto>> GetTopCustomersAsync(int vendorId, int count = 10);

    // Buyer Reports
    Task<BuyerReportDto> GetBuyerReportAsync(int vendorId, ReportFilterDto filter);
    Task<PurchaseOverviewDto> GetBuyerOverviewAsync(int vendorId);
    Task<List<PurchaseByPeriodDto>> GetPurchasesByPeriodAsync(int vendorId, ReportFilterDto filter);
    Task<List<TopSupplierDto>> GetTopSuppliersAsync(int vendorId, int count = 10);
    Task<List<TopPurchasedProductDto>> GetTopPurchasedProductsAsync(int vendorId, int count = 10);
}
