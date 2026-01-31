using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Stock;
using Bridgo.Helpers;
using Bridgo.Models.Entities;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class StockService : IStockService
{
    private readonly ApplicationDbContext _context;

    public StockService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================
    // STOK HAREKETLERI
    // ============================================

    public async Task<StockMovementSearchResultDto> GetMovementsAsync(int vendorId, StockMovementFilterDto filter)
    {
        var query = _context.StockMovements
            .Where(m => m.VendorId == vendorId)
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Include(m => m.TargetWarehouse)
            .AsQueryable();

        // Filters
        if (filter.ProductId.HasValue)
            query = query.Where(m => m.ProductId == filter.ProductId.Value);

        if (filter.WarehouseId.HasValue)
            query = query.Where(m => m.WarehouseId == filter.WarehouseId.Value);

        if (filter.MovementType.HasValue)
            query = query.Where(m => m.MovementType == filter.MovementType.Value);

        if (filter.ReferenceType.HasValue)
            query = query.Where(m => m.ReferenceType == filter.ReferenceType.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(m => m.MovementDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(m => m.MovementDate <= filter.ToDate.Value.AddDays(1));

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(m =>
                (m.Product != null && m.Product.Name.ToLower().Contains(search)) ||
                (m.Product != null && m.Product.SKU != null && m.Product.SKU.ToLower().Contains(search)) ||
                (m.ReferenceNumber != null && m.ReferenceNumber.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        var items = await query
            .OrderByDescending(m => m.MovementDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(m => MapToListDto(m))
            .ToListAsync();

        return new StockMovementSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<StockMovementSearchResultDto> GetProductMovementsAsync(int vendorId, int productId, StockMovementFilterDto filter)
    {
        filter.ProductId = productId;
        return await GetMovementsAsync(vendorId, filter);
    }

    public async Task<ServiceResult<StockMovementListDto>> AddStockAsync(int vendorId, int userId, CreateStockMovementDto dto)
    {
        // Validate
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.VendorId == vendorId);
        if (product == null)
            return ServiceResult<StockMovementListDto>.Fail("Urun bulunamadi.");

        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == dto.WarehouseId && w.VendorId == vendorId);
        if (warehouse == null)
            return ServiceResult<StockMovementListDto>.Fail("Depo bulunamadi.");

        if (dto.Quantity <= 0)
            return ServiceResult<StockMovementListDto>.Fail("Miktar 0'dan buyuk olmalidir.");

        // Get or create warehouse stock
        var stock = await _context.ProductWarehouseStocks
            .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.WarehouseId);

        decimal previousQuantity = 0;
        if (stock == null)
        {
            stock = new ProductWarehouseStock
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                Quantity = 0
            };
            _context.ProductWarehouseStocks.Add(stock);
        }
        else
        {
            previousQuantity = stock.Quantity;
        }

        // Update stock
        stock.Quantity += dto.Quantity;
        stock.LastMovementAt = DateTime.UtcNow;

        // Create movement record
        var movement = new StockMovement
        {
            VendorId = vendorId,
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            MovementType = StockMovementTypes.In.Id,
            Quantity = dto.Quantity,
            PreviousQuantity = previousQuantity,
            NewQuantity = stock.Quantity,
            ReferenceType = StockReferenceTypes.Manual.Id,
            Notes = dto.Notes,
            UserId = userId,
            MovementDate = DateTime.UtcNow
        };
        _context.StockMovements.Add(movement);

        // Update product total stock
        product.StockQuantity = (int)(await CalculateProductTotalStock(dto.ProductId) + dto.Quantity);

        await _context.SaveChangesAsync();

        return ServiceResult<StockMovementListDto>.Ok(await GetMovementDto(movement.Id), "Stok girisi basariyla yapildi.");
    }

    public async Task<ServiceResult<StockMovementListDto>> RemoveStockAsync(int vendorId, int userId, CreateStockMovementDto dto)
    {
        // Validate
        var stock = await _context.ProductWarehouseStocks
            .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.WarehouseId);

        if (stock == null || stock.Quantity < dto.Quantity)
            return ServiceResult<StockMovementListDto>.Fail("Yeterli stok bulunmuyor.");

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.VendorId == vendorId);
        if (product == null)
            return ServiceResult<StockMovementListDto>.Fail("Urun bulunamadi.");

        decimal previousQuantity = stock.Quantity;

        // Update stock
        stock.Quantity -= dto.Quantity;
        stock.LastMovementAt = DateTime.UtcNow;

        // Create movement record
        var movement = new StockMovement
        {
            VendorId = vendorId,
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            MovementType = StockMovementTypes.Out.Id,
            Quantity = -dto.Quantity,
            PreviousQuantity = previousQuantity,
            NewQuantity = stock.Quantity,
            ReferenceType = StockReferenceTypes.Manual.Id,
            Notes = dto.Notes,
            UserId = userId,
            MovementDate = DateTime.UtcNow
        };
        _context.StockMovements.Add(movement);

        // Update product total stock
        product.StockQuantity = (int)(await CalculateProductTotalStock(dto.ProductId) - dto.Quantity);

        await _context.SaveChangesAsync();

        return ServiceResult<StockMovementListDto>.Ok(await GetMovementDto(movement.Id), "Stok cikisi basariyla yapildi.");
    }

    public async Task<ServiceResult> TransferStockAsync(int vendorId, int userId, StockTransferDto dto)
    {
        if (dto.SourceWarehouseId == dto.TargetWarehouseId)
            return ServiceResult.Fail("Kaynak ve hedef depo ayni olamaz.");

        // Validate source stock
        var sourceStock = await _context.ProductWarehouseStocks
            .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.SourceWarehouseId);

        if (sourceStock == null || sourceStock.AvailableQuantity < dto.Quantity)
            return ServiceResult.Fail("Kaynak depoda yeterli stok bulunmuyor.");

        var targetWarehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == dto.TargetWarehouseId && w.VendorId == vendorId);
        if (targetWarehouse == null)
            return ServiceResult.Fail("Hedef depo bulunamadi.");

        // Get or create target stock
        var targetStock = await _context.ProductWarehouseStocks
            .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.TargetWarehouseId);

        decimal sourcePrevious = sourceStock.Quantity;
        decimal targetPrevious = 0;

        if (targetStock == null)
        {
            targetStock = new ProductWarehouseStock
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.TargetWarehouseId,
                Quantity = 0
            };
            _context.ProductWarehouseStocks.Add(targetStock);
        }
        else
        {
            targetPrevious = targetStock.Quantity;
        }

        // Update stocks
        sourceStock.Quantity -= dto.Quantity;
        sourceStock.LastMovementAt = DateTime.UtcNow;

        targetStock.Quantity += dto.Quantity;
        targetStock.LastMovementAt = DateTime.UtcNow;

        // Create movement records
        var sourceMovement = new StockMovement
        {
            VendorId = vendorId,
            ProductId = dto.ProductId,
            WarehouseId = dto.SourceWarehouseId,
            MovementType = StockMovementTypes.Transfer.Id,
            Quantity = -dto.Quantity,
            PreviousQuantity = sourcePrevious,
            NewQuantity = sourceStock.Quantity,
            ReferenceType = StockReferenceTypes.Transfer.Id,
            TargetWarehouseId = dto.TargetWarehouseId,
            Notes = dto.Notes,
            UserId = userId,
            MovementDate = DateTime.UtcNow
        };
        _context.StockMovements.Add(sourceMovement);

        var targetMovement = new StockMovement
        {
            VendorId = vendorId,
            ProductId = dto.ProductId,
            WarehouseId = dto.TargetWarehouseId,
            MovementType = StockMovementTypes.Transfer.Id,
            Quantity = dto.Quantity,
            PreviousQuantity = targetPrevious,
            NewQuantity = targetStock.Quantity,
            ReferenceType = StockReferenceTypes.Transfer.Id,
            Notes = dto.Notes,
            UserId = userId,
            MovementDate = DateTime.UtcNow
        };
        _context.StockMovements.Add(targetMovement);

        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Transfer basariyla tamamlandi.");
    }

    public async Task<ServiceResult<StockMovementListDto>> AdjustStockAsync(int vendorId, int userId, StockAdjustmentDto dto)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.VendorId == vendorId);
        if (product == null)
            return ServiceResult<StockMovementListDto>.Fail("Urun bulunamadi.");

        var stock = await _context.ProductWarehouseStocks
            .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.WarehouseId);

        decimal previousQuantity = 0;
        if (stock == null)
        {
            stock = new ProductWarehouseStock
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                Quantity = 0
            };
            _context.ProductWarehouseStocks.Add(stock);
        }
        else
        {
            previousQuantity = stock.Quantity;
        }

        decimal difference = dto.NewQuantity - previousQuantity;

        // Update stock
        stock.Quantity = dto.NewQuantity;
        stock.LastMovementAt = DateTime.UtcNow;
        stock.LastStockCheckAt = DateTime.UtcNow;

        // Create movement record
        var movement = new StockMovement
        {
            VendorId = vendorId,
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            MovementType = StockMovementTypes.Adjustment.Id,
            Quantity = difference,
            PreviousQuantity = previousQuantity,
            NewQuantity = dto.NewQuantity,
            ReferenceType = StockReferenceTypes.Adjustment.Id,
            Notes = dto.Reason,
            UserId = userId,
            MovementDate = DateTime.UtcNow
        };
        _context.StockMovements.Add(movement);

        // Update product total stock
        product.StockQuantity = (int)(await CalculateProductTotalStock(dto.ProductId) + difference);

        await _context.SaveChangesAsync();

        return ServiceResult<StockMovementListDto>.Ok(await GetMovementDto(movement.Id), "Stok duzeltmesi yapildi.");
    }

    // ============================================
    // STOK SORGULARI
    // ============================================

    public async Task<ProductStockSummaryDto?> GetProductStockSummaryAsync(int vendorId, int productId)
    {
        var product = await _context.Products
            .Where(p => p.Id == productId && p.VendorId == vendorId)
            .Include(p => p.Images)
            .FirstOrDefaultAsync();

        if (product == null) return null;

        var warehouseStocks = await _context.ProductWarehouseStocks
            .Where(s => s.ProductId == productId)
            .Include(s => s.Warehouse)
            .ToListAsync();

        var dto = new ProductStockSummaryDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            ProductSKU = product.SKU,
            ProductImageUrl = product.Images?.FirstOrDefault(i => i.IsMain)?.Url,
            TotalQuantity = warehouseStocks.Sum(s => s.Quantity),
            TotalReserved = warehouseStocks.Sum(s => s.ReservedQuantity),
            TotalAvailable = warehouseStocks.Sum(s => s.AvailableQuantity),
            MinStockLevel = product.MinStockLevel,
            IsLowStock = product.MinStockLevel.HasValue && warehouseStocks.Sum(s => s.AvailableQuantity) < product.MinStockLevel.Value,
            WarehouseStocks = warehouseStocks.Select(s => new WarehouseStockDto
            {
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse?.Name ?? "",
                Quantity = s.Quantity,
                ReservedQuantity = s.ReservedQuantity,
                AvailableQuantity = s.AvailableQuantity,
                BinLocation = s.BinLocation,
                Zone = s.Zone,
                MinStockLevel = s.MinStockLevel,
                IsLowStock = s.MinStockLevel.HasValue && s.AvailableQuantity < s.MinStockLevel.Value,
                LastMovementAt = s.LastMovementAt
            }).ToList()
        };

        return dto;
    }

    public async Task<List<ProductStockSummaryDto>> GetAllProductStockSummariesAsync(int vendorId)
    {
        var products = await _context.Products
            .Where(p => p.VendorId == vendorId)
            .Include(p => p.Images)
            .Include(p => p.WarehouseStocks)
                .ThenInclude(s => s.Warehouse)
            .ToListAsync();

        return products.Select(p => new ProductStockSummaryDto
        {
            ProductId = p.Id,
            ProductName = p.Name,
            ProductSKU = p.SKU,
            ProductImageUrl = p.Images?.FirstOrDefault(i => i.IsMain)?.Url,
            TotalQuantity = p.WarehouseStocks?.Sum(s => s.Quantity) ?? 0,
            TotalReserved = p.WarehouseStocks?.Sum(s => s.ReservedQuantity) ?? 0,
            TotalAvailable = p.WarehouseStocks?.Sum(s => s.AvailableQuantity) ?? 0,
            MinStockLevel = p.MinStockLevel,
            IsLowStock = p.MinStockLevel.HasValue && (p.WarehouseStocks?.Sum(s => s.AvailableQuantity) ?? 0) < p.MinStockLevel.Value,
            WarehouseStocks = p.WarehouseStocks?.Select(s => new WarehouseStockDto
            {
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse?.Name ?? "",
                Quantity = s.Quantity,
                ReservedQuantity = s.ReservedQuantity,
                AvailableQuantity = s.AvailableQuantity,
                BinLocation = s.BinLocation,
                Zone = s.Zone,
                MinStockLevel = s.MinStockLevel,
                IsLowStock = s.MinStockLevel.HasValue && s.AvailableQuantity < s.MinStockLevel.Value,
                LastMovementAt = s.LastMovementAt
            }).ToList() ?? new()
        }).ToList();
    }

    public async Task<List<LowStockAlertDto>> GetLowStockAlertsAsync(int vendorId)
    {
        // AvailableQuantity computed property oldugu icin SQL'e cevrilemiyor
        // Bu yuzden (Quantity - ReservedQuantity) kullaniyoruz
        var alerts = await _context.ProductWarehouseStocks
            .Where(s => s.Product!.VendorId == vendorId &&
                       s.MinStockLevel.HasValue &&
                       (s.Quantity - s.ReservedQuantity) < s.MinStockLevel.Value)
            .Select(s => new LowStockAlertDto
            {
                ProductId = s.ProductId,
                ProductName = s.Product!.Name,
                ProductSKU = s.Product!.SKU,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse!.Name,
                CurrentQuantity = s.Quantity - s.ReservedQuantity,
                MinStockLevel = s.MinStockLevel!.Value,
                Deficit = s.MinStockLevel!.Value - (s.Quantity - s.ReservedQuantity)
            })
            .ToListAsync();

        return alerts;
    }

    public async Task<StockStatsDto> GetStockStatsAsync(int vendorId)
    {
        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);

        var products = await _context.Products.Where(p => p.VendorId == vendorId).ToListAsync();
        var warehouses = await _context.Warehouses.Where(w => w.VendorId == vendorId).CountAsync();

        return new StockStatsDto
        {
            TotalProducts = products.Count,
            LowStockProducts = products.Count(p => p.MinStockLevel.HasValue && p.StockQuantity < p.MinStockLevel.Value),
            OutOfStockProducts = products.Count(p => p.StockQuantity <= 0),
            TotalWarehouses = warehouses,
            TodayMovements = await _context.StockMovements
                .CountAsync(m => m.VendorId == vendorId && m.MovementDate >= today),
            WeekMovements = await _context.StockMovements
                .CountAsync(m => m.VendorId == vendorId && m.MovementDate >= weekAgo),
            TotalStockValue = products.Sum(p => p.StockQuantity * p.Price)
        };
    }

    // ============================================
    // SIPARIS ILE ENTEGRASYON
    // ============================================

    public async Task<ServiceResult> ReserveStockForOrderAsync(int vendorId, int orderId, int productId, int warehouseId, decimal quantity)
    {
        var stock = await _context.ProductWarehouseStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);

        if (stock == null || stock.AvailableQuantity < quantity)
            return ServiceResult.Fail("Yeterli stok bulunmuyor.");

        stock.ReservedQuantity += quantity;
        stock.LastMovementAt = DateTime.UtcNow;

        var movement = new StockMovement
        {
            VendorId = vendorId,
            ProductId = productId,
            WarehouseId = warehouseId,
            MovementType = StockMovementTypes.Reserve.Id,
            Quantity = quantity,
            PreviousQuantity = stock.Quantity,
            NewQuantity = stock.Quantity,
            ReferenceType = StockReferenceTypes.Order.Id,
            ReferenceId = orderId,
            MovementDate = DateTime.UtcNow
        };
        _context.StockMovements.Add(movement);

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Stok rezerve edildi.");
    }

    public async Task<ServiceResult> UnreserveStockForOrderAsync(int vendorId, int orderId, int productId, int warehouseId, decimal quantity)
    {
        var stock = await _context.ProductWarehouseStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);

        if (stock == null) return ServiceResult.Fail("Stok kaydi bulunamadi.");

        stock.ReservedQuantity = Math.Max(0, stock.ReservedQuantity - quantity);
        stock.LastMovementAt = DateTime.UtcNow;

        var movement = new StockMovement
        {
            VendorId = vendorId,
            ProductId = productId,
            WarehouseId = warehouseId,
            MovementType = StockMovementTypes.Unreserve.Id,
            Quantity = -quantity,
            PreviousQuantity = stock.Quantity,
            NewQuantity = stock.Quantity,
            ReferenceType = StockReferenceTypes.Order.Id,
            ReferenceId = orderId,
            MovementDate = DateTime.UtcNow
        };
        _context.StockMovements.Add(movement);

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Rezervasyon kaldirildi.");
    }

    public async Task<ServiceResult> ConfirmStockForOrderAsync(int vendorId, int orderId, int productId, int warehouseId, decimal quantity, string orderNumber)
    {
        var stock = await _context.ProductWarehouseStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);

        if (stock == null) return ServiceResult.Fail("Stok kaydi bulunamadi.");

        decimal previousQuantity = stock.Quantity;

        // Stok dus ve rezervasyonu kaldir
        stock.Quantity -= quantity;
        stock.ReservedQuantity = Math.Max(0, stock.ReservedQuantity - quantity);
        stock.LastMovementAt = DateTime.UtcNow;

        var movement = new StockMovement
        {
            VendorId = vendorId,
            ProductId = productId,
            WarehouseId = warehouseId,
            MovementType = StockMovementTypes.Out.Id,
            Quantity = -quantity,
            PreviousQuantity = previousQuantity,
            NewQuantity = stock.Quantity,
            ReferenceType = StockReferenceTypes.Order.Id,
            ReferenceId = orderId,
            ReferenceNumber = orderNumber,
            MovementDate = DateTime.UtcNow
        };
        _context.StockMovements.Add(movement);

        // Update product total stock
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            product.StockQuantity = (int)(await CalculateProductTotalStock(productId) - quantity);
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Stok onaylandi.");
    }

    public async Task<ServiceResult> AddStockForReturnAsync(int vendorId, int orderId, int productId, int warehouseId, decimal quantity, string orderNumber)
    {
        var stock = await _context.ProductWarehouseStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);

        decimal previousQuantity = 0;
        if (stock == null)
        {
            stock = new ProductWarehouseStock
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = 0
            };
            _context.ProductWarehouseStocks.Add(stock);
        }
        else
        {
            previousQuantity = stock.Quantity;
        }

        stock.Quantity += quantity;
        stock.LastMovementAt = DateTime.UtcNow;

        var movement = new StockMovement
        {
            VendorId = vendorId,
            ProductId = productId,
            WarehouseId = warehouseId,
            MovementType = StockMovementTypes.In.Id,
            Quantity = quantity,
            PreviousQuantity = previousQuantity,
            NewQuantity = stock.Quantity,
            ReferenceType = StockReferenceTypes.Return.Id,
            ReferenceId = orderId,
            ReferenceNumber = orderNumber,
            Notes = "Iade stok girisi",
            MovementDate = DateTime.UtcNow
        };
        _context.StockMovements.Add(movement);

        // Update product total stock
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            product.StockQuantity = (int)(await CalculateProductTotalStock(productId) + quantity);
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Iade stok eklendi.");
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private async Task<decimal> CalculateProductTotalStock(int productId)
    {
        return await _context.ProductWarehouseStocks
            .Where(s => s.ProductId == productId)
            .SumAsync(s => s.Quantity);
    }

    private async Task<StockMovementListDto> GetMovementDto(int movementId)
    {
        var movement = await _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Include(m => m.TargetWarehouse)
            .FirstAsync(m => m.Id == movementId);

        return MapToListDto(movement);
    }

    private static StockMovementListDto MapToListDto(StockMovement m)
    {
        return new StockMovementListDto
        {
            Id = m.Id,
            ProductId = m.ProductId,
            ProductName = m.Product?.Name ?? "",
            ProductSKU = m.Product?.SKU,
            WarehouseId = m.WarehouseId,
            WarehouseName = m.Warehouse?.Name ?? "",
            MovementType = m.MovementType,
            MovementTypeName = StockMovementTypes.GetById(m.MovementType)?.SystemName ?? "",
            Quantity = m.Quantity,
            PreviousQuantity = m.PreviousQuantity,
            NewQuantity = m.NewQuantity,
            ReferenceType = m.ReferenceType,
            ReferenceTypeName = m.ReferenceType.HasValue ? StockReferenceTypes.GetById(m.ReferenceType.Value)?.SystemName : null,
            ReferenceId = m.ReferenceId,
            ReferenceNumber = m.ReferenceNumber,
            TargetWarehouseId = m.TargetWarehouseId,
            TargetWarehouseName = m.TargetWarehouse?.Name,
            Notes = m.Notes,
            MovementDate = m.MovementDate
        };
    }
}
