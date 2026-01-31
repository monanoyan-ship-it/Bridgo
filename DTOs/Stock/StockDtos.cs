namespace Bridgo.DTOs.Stock;

/// <summary>
/// Stok hareketi listesi DTO
/// </summary>
public class StockMovementListDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSKU { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int MovementType { get; set; }
    public string MovementTypeName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public int? ReferenceType { get; set; }
    public string? ReferenceTypeName { get; set; }
    public int? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }
    public int? TargetWarehouseId { get; set; }
    public string? TargetWarehouseName { get; set; }
    public string? Notes { get; set; }
    public DateTime MovementDate { get; set; }
    public string? CreatedByName { get; set; }
}

/// <summary>
/// Stok hareketi olusturma DTO
/// </summary>
public class CreateStockMovementDto
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int MovementType { get; set; }
    public decimal Quantity { get; set; }
    public int? TargetWarehouseId { get; set; }  // Transfer icin
    public string? Notes { get; set; }
}

/// <summary>
/// Stok duzeltme (adjustment) DTO
/// </summary>
public class StockAdjustmentDto
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public decimal NewQuantity { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Stok transfer DTO
/// </summary>
public class StockTransferDto
{
    public int ProductId { get; set; }
    public int SourceWarehouseId { get; set; }
    public int TargetWarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Stok hareketi filtre DTO
/// </summary>
public class StockMovementFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? ProductId { get; set; }
    public int? WarehouseId { get; set; }
    public int? MovementType { get; set; }
    public int? ReferenceType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
}

/// <summary>
/// Stok hareketi arama sonucu
/// </summary>
public class StockMovementSearchResultDto
{
    public List<StockMovementListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

/// <summary>
/// Stok ozet bilgisi (urun bazli)
/// </summary>
public class ProductStockSummaryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSKU { get; set; }
    public string? ProductImageUrl { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalReserved { get; set; }
    public decimal TotalAvailable { get; set; }
    public decimal? MinStockLevel { get; set; }
    public bool IsLowStock { get; set; }
    public List<WarehouseStockDto> WarehouseStocks { get; set; } = new();
}

/// <summary>
/// Depo bazli stok bilgisi
/// </summary>
public class WarehouseStockDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public string? BinLocation { get; set; }
    public string? Zone { get; set; }
    public decimal? MinStockLevel { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime? LastMovementAt { get; set; }
}

/// <summary>
/// Stok istatistikleri
/// </summary>
public class StockStatsDto
{
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int TotalWarehouses { get; set; }
    public int TodayMovements { get; set; }
    public int WeekMovements { get; set; }
    public decimal TotalStockValue { get; set; }
}

/// <summary>
/// Dusuk stok alarm bilgisi
/// </summary>
public class LowStockAlertDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSKU { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal CurrentQuantity { get; set; }
    public decimal MinStockLevel { get; set; }
    public decimal Deficit { get; set; }
}
