using Bridgo.DTOs.Branch;

namespace Bridgo.Services.Interfaces;

public interface IBranchService
{
    Task<IEnumerable<BranchDto>> GetAllAsync(bool? isActive = null);
    Task<BranchDto?> GetByIdAsync(int id);
    Task<BranchDto> CreateAsync(CreateBranchDto dto, string? createdBy = null);
    Task<bool> UpdateAsync(int id, UpdateBranchDto dto, string? updatedBy = null);
    Task<bool> DeleteAsync(int id, string? deletedBy = null);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
}
