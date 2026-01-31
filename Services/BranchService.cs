using Microsoft.EntityFrameworkCore;
using Bridgo.DTOs.Branch;
using Bridgo.Models.Entities;
using Bridgo.Repositories;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class BranchService : IBranchService
{
    private readonly IUnitOfWork _unitOfWork;

    public BranchService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<BranchDto>> GetAllAsync(bool? isActive = null)
    {
        var query = _unitOfWork.Branches.QueryNoTracking();

        if (isActive.HasValue)
        {
            query = query.Where(b => b.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(b => b.SortOrder)
            .ThenBy(b => b.Name)
            .Select(b => new BranchDto
            {
                Id = b.Id,
                Code = b.Code,
                Name = b.Name,
                Address = b.Address,
                Phone = b.Phone,
                Email = b.Email,
                City = b.City,
                District = b.District,
                IsActive = b.IsActive,
                SortOrder = b.SortOrder,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<BranchDto?> GetByIdAsync(int id)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(id);

        if (branch == null)
            return null;

        return new BranchDto
        {
            Id = branch.Id,
            Code = branch.Code,
            Name = branch.Name,
            Address = branch.Address,
            Phone = branch.Phone,
            Email = branch.Email,
            City = branch.City,
            District = branch.District,
            IsActive = branch.IsActive,
            SortOrder = branch.SortOrder,
            CreatedAt = branch.CreatedAt
        };
    }

    public async Task<BranchDto> CreateAsync(CreateBranchDto dto, string? createdBy = null)
    {
        var branch = new Branch
        {
            Code = dto.Code,
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            Email = dto.Email,
            City = dto.City,
            District = dto.District,
            IsActive = dto.IsActive,
            SortOrder = dto.SortOrder,
            CreatedBy = createdBy
        };

        await _unitOfWork.Branches.AddAsync(branch);
        await _unitOfWork.SaveChangesAsync();

        return new BranchDto
        {
            Id = branch.Id,
            Code = branch.Code,
            Name = branch.Name,
            Address = branch.Address,
            Phone = branch.Phone,
            Email = branch.Email,
            City = branch.City,
            District = branch.District,
            IsActive = branch.IsActive,
            SortOrder = branch.SortOrder,
            CreatedAt = branch.CreatedAt
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateBranchDto dto, string? updatedBy = null)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(id);

        if (branch == null)
            return false;

        branch.Code = dto.Code;
        branch.Name = dto.Name;
        branch.Address = dto.Address;
        branch.Phone = dto.Phone;
        branch.Email = dto.Email;
        branch.City = dto.City;
        branch.District = dto.District;
        branch.IsActive = dto.IsActive;
        branch.SortOrder = dto.SortOrder;
        branch.UpdatedBy = updatedBy;

        _unitOfWork.Branches.Update(branch);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id, string? deletedBy = null)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(id);

        if (branch == null)
            return false;

        branch.IsDeleted = true;
        branch.UpdatedBy = deletedBy;

        _unitOfWork.Branches.Update(branch);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        return await _unitOfWork.Branches
            .AnyAsync(b => b.Code == code && (!excludeId.HasValue || b.Id != excludeId.Value));
    }
}
