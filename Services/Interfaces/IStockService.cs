using Bridgo.DTOs.Stock;
using Bridgo.Helpers;

namespace Bridgo.Services.Interfaces;

public interface IStockService
{
    // ============================================
    // STOK HAREKETLERI
    // ============================================

    /// <summary>
    /// Stok hareketlerini listele
    /// </summary>
    Task<StockMovementSearchResultDto> GetMovementsAsync(int vendorId, StockMovementFilterDto filter);

    /// <summary>
    /// Urunun stok hareketlerini getir
    /// </summary>
    Task<StockMovementSearchResultDto> GetProductMovementsAsync(int vendorId, int productId, StockMovementFilterDto filter);

    /// <summary>
    /// Stok girisi yap (In)
    /// </summary>
    Task<ServiceResult<StockMovementListDto>> AddStockAsync(int vendorId, int userId, CreateStockMovementDto dto);

    /// <summary>
    /// Stok cikisi yap (Out)
    /// </summary>
    Task<ServiceResult<StockMovementListDto>> RemoveStockAsync(int vendorId, int userId, CreateStockMovementDto dto);

    /// <summary>
    /// Stok transferi yap
    /// </summary>
    Task<ServiceResult> TransferStockAsync(int vendorId, int userId, StockTransferDto dto);

    /// <summary>
    /// Stok duzeltmesi yap (Adjustment)
    /// </summary>
    Task<ServiceResult<StockMovementListDto>> AdjustStockAsync(int vendorId, int userId, StockAdjustmentDto dto);

    // ============================================
    // STOK SORGULARI
    // ============================================

    /// <summary>
    /// Urun stok ozetini getir
    /// </summary>
    Task<ProductStockSummaryDto?> GetProductStockSummaryAsync(int vendorId, int productId);

    /// <summary>
    /// Tum urunlerin stok ozetini getir
    /// </summary>
    Task<List<ProductStockSummaryDto>> GetAllProductStockSummariesAsync(int vendorId);

    /// <summary>
    /// Dusuk stok alarmlarini getir
    /// </summary>
    Task<List<LowStockAlertDto>> GetLowStockAlertsAsync(int vendorId);

    /// <summary>
    /// Stok istatistiklerini getir
    /// </summary>
    Task<StockStatsDto> GetStockStatsAsync(int vendorId);

    // ============================================
    // SIPARIS ILE ENTEGRASYON
    // ============================================

    /// <summary>
    /// Siparis icin stok rezerve et
    /// </summary>
    Task<ServiceResult> ReserveStockForOrderAsync(int vendorId, int orderId, int productId, int warehouseId, decimal quantity);

    /// <summary>
    /// Siparis rezervasyonunu iptal et
    /// </summary>
    Task<ServiceResult> UnreserveStockForOrderAsync(int vendorId, int orderId, int productId, int warehouseId, decimal quantity);

    /// <summary>
    /// Siparis onaylandiginda stok dus
    /// </summary>
    Task<ServiceResult> ConfirmStockForOrderAsync(int vendorId, int orderId, int productId, int warehouseId, decimal quantity, string orderNumber);

    /// <summary>
    /// Iade icin stok ekle
    /// </summary>
    Task<ServiceResult> AddStockForReturnAsync(int vendorId, int orderId, int productId, int warehouseId, decimal quantity, string orderNumber);
}
