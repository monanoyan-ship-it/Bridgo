using Microsoft.EntityFrameworkCore;
using Bridgo.DTOs.Warehouse;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Repositories;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Depo yönetimi servisi implementation
/// </summary>
public class WarehouseService : IWarehouseService
{
    private readonly IUnitOfWork _unitOfWork;

    public WarehouseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<WarehouseDto>> GetAllAsync(int vendorId, bool? isActive = null)
    {
        var query = _unitOfWork.Warehouses.Query()
            .Include(w => w.Address)
            .Include(w => w.Manager)
            .Include(w => w.ProductStocks)
            .Where(w => w.VendorId == vendorId && !w.IsDeleted);

        if (isActive.HasValue)
        {
            query = query.Where(w => w.IsActive == isActive.Value);
        }

        var warehouses = await query
            .OrderBy(w => w.Priority)
            .ThenBy(w => w.Name)
            .ToListAsync();

        return warehouses.Select(MapToDto).ToList();
    }

    public async Task<WarehouseDto?> GetByIdAsync(int id, int vendorId)
    {
        var warehouse = await _unitOfWork.Warehouses.Query()
            .Include(w => w.Address)
            .Include(w => w.Manager)
            .Include(w => w.ProductStocks)
            .FirstOrDefaultAsync(w => w.Id == id && w.VendorId == vendorId && !w.IsDeleted);

        return warehouse == null ? null : MapToDto(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(WarehouseCreateDto dto, int vendorId, string? createdBy)
    {
        // Kod benzersizlik kontrolü
        if (!await IsCodeUniqueAsync(dto.Code, vendorId))
        {
            throw new InvalidOperationException("Bu kod zaten kullanılıyor.");
        }

        var warehouse = new Warehouse
        {
            Code = dto.Code.ToUpper().Trim(),
            Name = dto.Name.Trim(),
            WarehouseTypeId = dto.WarehouseTypeId,
            Description = dto.Description?.Trim(),
            AddressId = dto.AddressId,
            TotalCapacity = dto.TotalCapacity,
            CapacityUnit = dto.CapacityUnit?.Trim(),
            ManagerUserId = dto.ManagerUserId,
            ContactPhone = dto.ContactPhone?.Trim(),
            ContactEmail = dto.ContactEmail?.Trim(),
            OperatingHours = dto.OperatingHours,
            Priority = dto.Priority,
            IsDefault = dto.IsDefault,
            IsActive = dto.IsActive,
            VendorId = vendorId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        // Eğer varsayılan olarak işaretlendiyse, diğerlerini kaldır
        if (dto.IsDefault)
        {
            await ClearDefaultAsync(vendorId);
        }

        await _unitOfWork.Warehouses.AddAsync(warehouse);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(warehouse);
    }

    public async Task<bool> UpdateAsync(int id, WarehouseUpdateDto dto, int vendorId, string? updatedBy)
    {
        var warehouse = await _unitOfWork.Warehouses.Query()
            .FirstOrDefaultAsync(w => w.Id == id && w.VendorId == vendorId && !w.IsDeleted);

        if (warehouse == null)
            return false;

        // Kod benzersizlik kontrolü
        if (!await IsCodeUniqueAsync(dto.Code, vendorId, id))
        {
            throw new InvalidOperationException("Bu kod zaten kullanılıyor.");
        }

        warehouse.Code = dto.Code.ToUpper().Trim();
        warehouse.Name = dto.Name.Trim();
        warehouse.WarehouseTypeId = dto.WarehouseTypeId;
        warehouse.Description = dto.Description?.Trim();
        warehouse.AddressId = dto.AddressId;
        warehouse.TotalCapacity = dto.TotalCapacity;
        warehouse.CapacityUnit = dto.CapacityUnit?.Trim();
        warehouse.ManagerUserId = dto.ManagerUserId;
        warehouse.ContactPhone = dto.ContactPhone?.Trim();
        warehouse.ContactEmail = dto.ContactEmail?.Trim();
        warehouse.OperatingHours = dto.OperatingHours;
        warehouse.Priority = dto.Priority;
        warehouse.IsActive = dto.IsActive;
        warehouse.UpdatedAt = DateTime.UtcNow;
        warehouse.UpdatedBy = updatedBy;

        // Varsayılan değişikliği
        if (dto.IsDefault && !warehouse.IsDefault)
        {
            await ClearDefaultAsync(vendorId);
            warehouse.IsDefault = true;
        }
        else if (!dto.IsDefault && warehouse.IsDefault)
        {
            // Varsayılanlık kaldırılmaya çalışılıyor - izin verme (en az bir varsayılan olmalı)
            // Veya izin ver ve uyarı dön
            warehouse.IsDefault = false;
        }

        _unitOfWork.Warehouses.Update(warehouse);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id, int vendorId, string? deletedBy)
    {
        var warehouse = await _unitOfWork.Warehouses.Query()
            .Include(w => w.ProductStocks)
            .FirstOrDefaultAsync(w => w.Id == id && w.VendorId == vendorId && !w.IsDeleted);

        if (warehouse == null)
            return false;

        // Varsayılan depo silinemez
        if (warehouse.IsDefault)
        {
            throw new InvalidOperationException("Varsayılan depo silinemez. Önce başka bir depoyu varsayılan yapın.");
        }

        // Stoklu depo silinemez
        if (warehouse.ProductStocks.Any(ps => ps.Quantity > 0))
        {
            throw new InvalidOperationException("Bu depoda stok bulunuyor. Önce stokları başka depoya aktarın.");
        }

        warehouse.IsDeleted = true;
        warehouse.UpdatedAt = DateTime.UtcNow;
        warehouse.UpdatedBy = deletedBy;

        _unitOfWork.Warehouses.Update(warehouse);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetAsDefaultAsync(int id, int vendorId)
    {
        var warehouse = await _unitOfWork.Warehouses.Query()
            .FirstOrDefaultAsync(w => w.Id == id && w.VendorId == vendorId && !w.IsDeleted);

        if (warehouse == null)
            return false;

        await ClearDefaultAsync(vendorId);

        warehouse.IsDefault = true;
        warehouse.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Warehouses.Update(warehouse);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<WarehouseSelectDto>> GetForSelectAsync(int vendorId)
    {
        return await _unitOfWork.Warehouses.Query()
            .Where(w => w.VendorId == vendorId && w.IsActive && !w.IsDeleted)
            .OrderBy(w => w.Priority)
            .ThenBy(w => w.Name)
            .Select(w => new WarehouseSelectDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                IsDefault = w.IsDefault
            })
            .ToListAsync();
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int vendorId, int? excludeId = null)
    {
        var normalizedCode = code.ToUpper().Trim();
        var query = _unitOfWork.Warehouses.Query()
            .Where(w => w.VendorId == vendorId && w.Code == normalizedCode && !w.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(w => w.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    private async Task ClearDefaultAsync(int vendorId)
    {
        var defaultWarehouses = await _unitOfWork.Warehouses.Query()
            .Where(w => w.VendorId == vendorId && w.IsDefault && !w.IsDeleted)
            .ToListAsync();

        foreach (var w in defaultWarehouses)
        {
            w.IsDefault = false;
            _unitOfWork.Warehouses.Update(w);
        }
    }

    private WarehouseDto MapToDto(Warehouse warehouse)
    {
        var warehouseType = WarehouseTypes.GetById(warehouse.WarehouseTypeId);

        return new WarehouseDto
        {
            Id = warehouse.Id,
            Code = warehouse.Code,
            Name = warehouse.Name,
            WarehouseTypeId = warehouse.WarehouseTypeId,
            WarehouseTypeName = warehouseType?.NameResourceKey,
            Description = warehouse.Description,
            AddressId = warehouse.AddressId,
            AddressLine = warehouse.Address?.AddressLine,
            City = warehouse.Address?.City,
            Country = null, // Country name için ayrı sorgu gerekir
            TotalCapacity = warehouse.TotalCapacity,
            CapacityUnit = warehouse.CapacityUnit,
            ManagerUserId = warehouse.ManagerUserId,
            ManagerName = warehouse.Manager != null
                ? $"{warehouse.Manager.FirstName} {warehouse.Manager.LastName}"
                : null,
            ContactPhone = warehouse.ContactPhone,
            ContactEmail = warehouse.ContactEmail,
            OperatingHours = warehouse.OperatingHours,
            Priority = warehouse.Priority,
            IsDefault = warehouse.IsDefault,
            IsActive = warehouse.IsActive,
            ProductCount = warehouse.ProductStocks?.Select(ps => ps.ProductId).Distinct().Count() ?? 0,
            TotalStockQuantity = (int)(warehouse.ProductStocks?.Sum(ps => ps.Quantity) ?? 0),
            CreatedAt = warehouse.CreatedAt
        };
    }
}
