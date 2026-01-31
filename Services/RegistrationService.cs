using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Auth;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class RegistrationService : IRegistrationService
{
    private readonly ApplicationDbContext _context;

    public RegistrationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RegistrationResult> RegisterVendorAsync(MultiStepRegisterDto model, int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // AddressLine validasyonu
            if (string.IsNullOrWhiteSpace(model.AddressLine))
            {
                return new RegistrationResult
                {
                    Success = false,
                    Message = "Adres satiri zorunludur"
                };
            }

            // Ulke kontrolu
            var countryExists = await _context.Countries.AnyAsync(c => c.Id == model.CountryId);
            if (!countryExists)
            {
                return new RegistrationResult
                {
                    Success = false,
                    Message = "Secilen ulke bulunamadi"
                };
            }

            // 1. Vendor olustur
            var vendor = new Vendor
            {
                CompanyName = model.CompanyName,
                Email = model.Email,
                Phone = model.Phone,
                Website = model.Website,
                ActiveCountries = string.Join(",", model.SelectedCountries),
                VendorStatusId = 3, // Active
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            // 2. Merkez adres olustur
            var address = new Address
            {
                VendorId = vendor.Id,
                Title = "Merkez",
                AddressTypeId = 3, // Headquarters
                CountryId = model.CountryId,
                StateId = model.StateId,
                City = model.City,
                District = model.District,
                AddressLine = model.AddressLine,
                PostalCode = model.PostalCode,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Addresses.Add(address);

            // 3. Capability mapping'leri olustur
            foreach (var capabilityId in model.SelectedCapabilities)
            {
                // Gecerli capability mi kontrol et
                var capability = Capabilities.GetById(capabilityId);
                if (capability == null || !capability.IsActive)
                    continue;

                _context.VendorCapabilityMappings.Add(new VendorCapabilityMapping
                {
                    VendorId = vendor.Id,
                    CapabilityId = capabilityId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            // 4. User'i vendor'a bagla
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.VendorId = vendor.Id;
            }

            // 5. VendorTeamMember olustur (Owner olarak)
            _context.VendorTeamMembers.Add(new VendorTeamMember
            {
                VendorId = vendor.Id,
                UserId = userId,
                Email = model.Email,
                Name = $"{model.FirstName} {model.LastName}",
                TeamMemberStatusId = 2, // Active
                Source = TeamMemberSource.OwnerCreated,
                ProcessedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            // 6. Owner'a varsayilan CompanyRole'leri ata (secilen capability'ler icin)
            var defaultRoles = await _context.CompanyRoles
                .Where(r => r.IsDefault && r.IsActive && model.SelectedCapabilities.Contains(r.CapabilityId))
                .ToListAsync();

            foreach (var role in defaultRoles)
            {
                _context.CompanyRoleUserMappings.Add(new CompanyRoleUserMapping
                {
                    UserId = userId,
                    VendorId = vendor.Id,
                    CompanyRoleId = role.Id,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new RegistrationResult
            {
                Success = true,
                VendorId = vendor.Id
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return new RegistrationResult
            {
                Success = false,
                Message = "Kayit sirasinda bir hata olustu. Lutfen tekrar deneyin."
            };
        }
    }

    public async Task<RegistrationResult> RegisterWithJoinRequestAsync(RegisterWithJoinDto model, int userId)
    {
        // Firma var mi kontrol et
        var vendor = await _context.Vendors.FindAsync(model.VendorId);
        if (vendor == null || vendor.IsDeleted)
        {
            return new RegistrationResult
            {
                Success = false,
                Message = "Firma bulunamadi"
            };
        }

        // Join request olustur (VendorTeamMember kullanarak)
        var joinRequest = new VendorTeamMember
        {
            VendorId = model.VendorId,
            UserId = userId,
            Email = model.Email,
            Name = $"{model.FirstName} {model.LastName}",
            Source = TeamMemberSource.JoinRequest,
            TeamMemberStatusId = 1, // Pending
            Message = model.Message,
            CreatedAt = DateTime.UtcNow
        };

        _context.VendorTeamMembers.Add(joinRequest);
        await _context.SaveChangesAsync();

        return new RegistrationResult
        {
            Success = true,
            Message = "Katilma istegi gonderildi. Firma yoneticisi onayladiktan sonra sisteme erisebilirsiniz.",
            VendorId = model.VendorId
        };
    }

    public async Task<VendorSearchResult?> CheckDomainMatchAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return null;
        }

        var domain = email.Split('@')[1].ToLower();

        // www. ile baslayanlar ve www. olmadan ara
        var vendor = await _context.Vendors
            .Where(v => !v.IsDeleted &&
                        v.VendorStatusId != 5 && v.VendorStatusId != 4 && // Not Rejected, Not Suspended
                        v.Website != null &&
                       (v.Website.ToLower().Contains(domain) ||
                        v.Website.ToLower().Replace("www.", "").Contains(domain)))
            .Select(v => new VendorSearchResult
            {
                Id = v.Id,
                CompanyName = v.CompanyName,
                City = v.Addresses.Where(a => a.AddressTypeId == 3 && !a.IsDeleted) // Headquarters
                                  .Select(a => a.City)
                                  .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        return vendor;
    }

    public async Task<List<VendorSearchResult>> SearchCompaniesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return new List<VendorSearchResult>();
        }

        return await _context.Vendors
            .Where(v => !v.IsDeleted &&
                        v.VendorStatusId != 5 && v.VendorStatusId != 4 && // Not Rejected, Not Suspended
                        v.CompanyName.ToLower().Contains(query.ToLower()))
            .Take(10)
            .Select(v => new VendorSearchResult
            {
                Id = v.Id,
                CompanyName = v.CompanyName,
                City = v.Addresses.Where(a => a.AddressTypeId == 3 && !a.IsDeleted) // Headquarters
                                  .Select(a => a.City)
                                  .FirstOrDefault()
            })
            .ToListAsync();
    }
}
