using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bridgo.DTOs.Vendor;
using Bridgo.Models.Entities;
using Bridgo.Models.Identity;
using Bridgo.Repositories;
using Bridgo.Services.Interfaces;
using Bridgo.Models.Enums;

namespace Bridgo.Services;

/// <summary>
/// Vendor islemleri servisi implementation
/// NOT: Join request/invitation islemleri TeamService'e tasinmistir
/// </summary>
public class VendorService : IVendorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public VendorService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    // ========================================
    // VENDOR CRUD
    // ========================================

    public async Task<VendorSetupResultDto> SetupVendorAsync(VendorSetupDto dto, int userId, string? createdBy = null)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new VendorSetupResultDto { Success = false, Message = "Kullanici bulunamadi" };
            }

            if (user.VendorId.HasValue)
            {
                return new VendorSetupResultDto { Success = false, Message = "Bu kullanici zaten bir firmaya kayitli" };
            }

            if (await EmailExistsAsync(dto.Email))
            {
                return new VendorSetupResultDto { Success = false, Message = "Bu e-posta adresi baska bir firma tarafindan kullaniliyor" };
            }

            // Ulke ve eyalet kontrolu
            var country = await _unitOfWork.Countries.GetByIdAsync(dto.CountryId);
            if (country == null)
            {
                return new VendorSetupResultDto { Success = false, Message = "Secilen ulke bulunamadi" };
            }

            if (dto.StateId.HasValue)
            {
                var state = await _unitOfWork.States.GetByIdAsync(dto.StateId.Value);
                if (state == null)
                {
                    return new VendorSetupResultDto { Success = false, Message = "Secilen il/eyalet bulunamadi" };
                }
            }

            // Vendor olustur
            var vendor = new Bridgo.Models.Entities.Vendor
            {
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                Phone = dto.Phone,
                VendorStatusId = VendorStatuses.PendingProfile.Id,
                IsProfileComplete = false,
                IsVerified = false,
                CreatedBy = createdBy
            };

            await _unitOfWork.Vendors.AddAsync(vendor);
            await _unitOfWork.SaveChangesAsync();

            // Address olustur
            var address = new Address
            {
                VendorId = vendor.Id,
                AddressTypeId = dto.AddressTypeId,
                Title = dto.AddressTitle,
                CountryId = dto.CountryId,
                StateId = dto.StateId,
                City = dto.City,
                District = dto.District,
                AddressLine = dto.AddressLine,
                PostalCode = dto.PostalCode,
                IsDefault = true,
                IsActive = true,
                CreatedBy = createdBy
            };

            await _unitOfWork.Addresses.AddAsync(address);
            await _unitOfWork.SaveChangesAsync();

            // Kullaniciyi Vendor'a bagla
            user.VendorId = vendor.Id;
            await _userManager.UpdateAsync(user);

            await _unitOfWork.CommitAsync();

            return new VendorSetupResultDto
            {
                Success = true,
                VendorId = vendor.Id,
                AddressId = address.Id,
                CompanyName = vendor.CompanyName,
                Message = "Firma kaydi basariyla olusturuldu"
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return new VendorSetupResultDto { Success = false, Message = $"Bir hata olustu: {ex.Message}" };
        }
    }

    public async Task<VendorDto?> GetByIdAsync(int id)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(id);
        return vendor == null ? null : MapToDto(vendor);
    }

    public async Task<VendorDto?> GetByUserIdAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user?.VendorId == null) return null;
        return await GetByIdAsync(user.VendorId.Value);
    }

    public async Task<bool> UserHasVendorAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.VendorId.HasValue ?? false;
    }

    public async Task<IEnumerable<AddressDto>> GetAddressesByVendorIdAsync(int vendorId)
    {
        var addresses = await _unitOfWork.Addresses
            .QueryNoTracking()
            .Include(a => a.CountryEntity)
            .Include(a => a.StateEntity)
            .Where(a => a.VendorId == vendorId && !a.IsDeleted)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Title)
            .ToListAsync();

        return addresses.Select(MapToAddressDto);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeVendorId = null)
    {
        return await _unitOfWork.Vendors.AnyAsync(
            v => v.Email == email && (!excludeVendorId.HasValue || v.Id != excludeVendorId.Value));
    }

    public async Task<bool> UpdateVendorAsync(int id, VendorDto dto, string? updatedBy = null)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(id);
        if (vendor == null) return false;

        vendor.CompanyName = dto.CompanyName;
        vendor.Email = dto.Email;
        vendor.Phone = dto.Phone;
        vendor.TaxNumber = dto.TaxNumber;
        vendor.TaxOffice = dto.TaxOffice;
        vendor.Website = dto.Website;
        vendor.LogoUrl = dto.LogoUrl;
        vendor.UpdatedBy = updatedBy;

        _unitOfWork.Vendors.Update(vendor);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // ========================================
    // SEARCH
    // ========================================

    public async Task<IEnumerable<VendorSearchResultDto>> SearchVendorsAsync(string searchTerm, int maxResults = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            return Enumerable.Empty<VendorSearchResultDto>();

        var searchLower = searchTerm.ToLower();

        var vendors = await _unitOfWork.Vendors
            .QueryNoTracking()
            .Where(v => !v.IsDeleted && v.VendorStatusId == VendorStatuses.Active.Id)
            .Where(v => v.CompanyName.ToLower().Contains(searchLower))
            .OrderBy(v => v.CompanyName)
            .Take(maxResults)
            .Select(v => new VendorSearchResultDto
            {
                Id = v.Id,
                CompanyName = v.CompanyName,
                Website = v.Website,
                LogoUrl = v.LogoUrl,
                IsVerified = v.IsVerified
            })
            .ToListAsync();

        // Sehir bilgilerini ekle
        foreach (var vendor in vendors)
        {
            var city = await _unitOfWork.Addresses
                .QueryNoTracking()
                .Where(a => a.VendorId == vendor.Id && a.IsDefault)
                .Select(a => a.City)
                .FirstOrDefaultAsync();

            vendor.City = city;
        }

        return vendors;
    }

    // ========================================
    // HELPER METHODS
    // ========================================

    private static VendorDto MapToDto(Bridgo.Models.Entities.Vendor vendor)
    {
        return new VendorDto
        {
            Id = vendor.Id,
            CompanyName = vendor.CompanyName,
            Email = vendor.Email,
            Phone = vendor.Phone,
            TaxNumber = vendor.TaxNumber,
            TaxOffice = vendor.TaxOffice,
            Website = vendor.Website,
            LogoUrl = vendor.LogoUrl,
            VendorStatusId = vendor.VendorStatusId,
            IsProfileComplete = vendor.IsProfileComplete,
            IsVerified = vendor.IsVerified,
            CreatedAt = vendor.CreatedAt
        };
    }

    private static AddressDto MapToAddressDto(Address address)
    {
        return new AddressDto
        {
            Id = address.Id,
            VendorId = address.VendorId,
            AddressTypeId = address.AddressTypeId,
            Title = address.Title,
            CountryId = address.CountryId,
            Country = address.CountryEntity?.Name ?? string.Empty,
            StateId = address.StateId,
            State = address.StateEntity?.Name,
            City = address.City,
            District = address.District,
            AddressLine = address.AddressLine,
            PostalCode = address.PostalCode,
            AddressDescription = address.AddressDescription,
            ContactName = address.ContactName,
            ContactPhone = address.ContactPhone,
            IsDefault = address.IsDefault,
            IsActive = address.IsActive,
            FullAddress = address.FullAddress
        };
    }
}
