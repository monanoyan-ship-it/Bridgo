using Bridgo.DTOs.Warehouse;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Depo yönetimi servisi interface
/// </summary>
public interface IWarehouseService
{
    /// <summary>
    /// Vendor'a ait tüm depoları getir
    /// </summary>
    Task<List<WarehouseDto>> GetAllAsync(int vendorId, bool? isActive = null);

    /// <summary>
    /// Depo detayını getir
    /// </summary>
    Task<WarehouseDto?> GetByIdAsync(int id, int vendorId);

    /// <summary>
    /// Yeni depo oluştur
    /// </summary>
    Task<WarehouseDto> CreateAsync(WarehouseCreateDto dto, int vendorId, string? createdBy);

    /// <summary>
    /// Depo güncelle
    /// </summary>
    Task<bool> UpdateAsync(int id, WarehouseUpdateDto dto, int vendorId, string? updatedBy);

    /// <summary>
    /// Depo sil (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(int id, int vendorId, string? deletedBy);

    /// <summary>
    /// Depoyu varsayılan olarak işaretle
    /// </summary>
    Task<bool> SetAsDefaultAsync(int id, int vendorId);

    /// <summary>
    /// Dropdown için depo listesi
    /// </summary>
    Task<List<WarehouseSelectDto>> GetForSelectAsync(int vendorId);

    /// <summary>
    /// Kod benzersizlik kontrolü
    /// </summary>
    Task<bool> IsCodeUniqueAsync(string code, int vendorId, int? excludeId = null);
}
