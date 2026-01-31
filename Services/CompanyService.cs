using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class CompanyService : ICompanyService
{
    private readonly ApplicationDbContext _context;

    public CompanyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VendorCapabilityDto>> GetVendorCapabilitiesAsync(int vendorId)
    {
        var capabilityIds = await _context.VendorCapabilityMappings
            .Where(m => m.VendorId == vendorId && m.IsActive)
            .Select(m => m.CapabilityId)
            .ToListAsync();

        return capabilityIds
            .Select(id => Capabilities.GetById(id))
            .Where(c => c != null && c.IsActive)
            .OrderBy(c => c!.DisplayOrder)
            .Select(c => new VendorCapabilityDto
            {
                Id = c!.Id,
                Name = c.Description ?? c.SystemName,
                Code = c.SystemName.ToLowerInvariant(), // seller, buyer, carrier, etc.
                NameResourceKey = c.NameResourceKey,
                Description = c.Description,
                Icon = c.Icon,
                CssClass = c.CssClass,
                DisplayOrder = c.DisplayOrder
            })
            .ToList();
    }

    public async Task<List<CapabilityModuleDto>> GetCapabilityModulesAsync(int capabilityId)
    {
        // Bu capability'ye map edilmis modul ID'lerini al
        var moduleIds = await _context.CapabilityModuleMappings
            .Where(m => m.CapabilityId == capabilityId)
            .Select(m => m.PlatformModuleId)
            .ToListAsync();

        // Platform modullerini getir
        var allModules = await _context.PlatformModules
            .Where(m => moduleIds.Contains(m.Id) && m.IsActive && m.IsMenuItem)
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new CapabilityModuleDto
            {
                Id = m.Id,
                ParentId = m.ParentId,
                Name = m.Name,
                DisplayName = m.DisplayName,
                DisplayNameResourceKey = m.DisplayNameResourceKey,
                Description = m.Description,
                Icon = m.Icon,
                Route = m.Route,
                DisplayOrder = m.DisplayOrder,
                IsMenuItem = m.IsMenuItem,
                IsMenuSection = m.IsMenuSection
            })
            .ToListAsync();

        // Hiyerarsi olustur
        return BuildModuleTree(allModules);
    }

    public async Task<List<UserCompanyRoleDto>> GetUserRolesAsync(int userId, int vendorId, int? capabilityId = null)
    {
        var query = _context.CompanyRoleUserMappings
            .Where(r => r.UserId == userId && r.VendorId == vendorId && r.IsActive)
            .Include(r => r.CompanyRole)
            .AsQueryable();

        if (capabilityId.HasValue)
        {
            query = query.Where(r => r.CompanyRole.CapabilityId == capabilityId.Value);
        }

        var roles = await query
            .Select(r => new
            {
                r.Id,
                r.CompanyRoleId,
                r.CompanyRole.Name,
                r.CompanyRole.NameResourceKey,
                r.CompanyRole.CapabilityId
            })
            .ToListAsync();

        return roles.Select(r =>
        {
            var capability = Capabilities.GetById(r.CapabilityId);
            return new UserCompanyRoleDto
            {
                Id = r.Id,
                CompanyRoleId = r.CompanyRoleId,
                RoleName = r.Name,
                RoleNameResourceKey = r.NameResourceKey,
                CapabilityId = r.CapabilityId,
                CapabilityName = capability?.Description ?? capability?.SystemName ?? "",
                CapabilityNameResourceKey = capability?.NameResourceKey
            };
        }).ToList();
    }

    public async Task<ModulePermissionDto?> GetUserModulePermissionAsync(int userId, int vendorId, int moduleId)
    {
        // User'in bu vendor'daki tum rollerini al
        var userRoleIds = await _context.CompanyRoleUserMappings
            .Where(r => r.UserId == userId && r.VendorId == vendorId && r.IsActive)
            .Select(r => r.CompanyRoleId)
            .ToListAsync();

        if (!userRoleIds.Any())
            return null;

        // Bu moduldeki izinleri al (en yuksek yetki)
        var permissions = await _context.CompanyRoleModulePermissions
            .Where(p => userRoleIds.Contains(p.CompanyRoleId) && p.PlatformModuleId == moduleId)
            .Include(p => p.PlatformModule)
            .ToListAsync();

        if (!permissions.Any())
            return null;

        // En yuksek izinleri birleştir (herhangi bir rolde varsa true)
        return new ModulePermissionDto
        {
            ModuleId = moduleId,
            ModuleName = permissions.First().PlatformModule.Name,
            CanView = permissions.Any(p => p.CanView),
            CanCreate = permissions.Any(p => p.CanCreate),
            CanEdit = permissions.Any(p => p.CanEdit),
            CanDelete = permissions.Any(p => p.CanDelete)
        };
    }

    public async Task<List<CapabilityModuleDto>> GetUserAccessibleModulesAsync(int userId, int vendorId, int capabilityId)
    {
        // User'in bu vendor ve capability'deki rollerini al
        var userRoleIds = await _context.CompanyRoleUserMappings
            .Where(r => r.UserId == userId && r.VendorId == vendorId && r.IsActive)
            .Include(r => r.CompanyRole)
            .Where(r => r.CompanyRole.CapabilityId == capabilityId)
            .Select(r => r.CompanyRoleId)
            .ToListAsync();

        if (!userRoleIds.Any())
            return new List<CapabilityModuleDto>();

        // Bu rollerin gorebilecegi modul ID'lerini al
        var accessibleModuleIds = await _context.CompanyRoleModulePermissions
            .Where(p => userRoleIds.Contains(p.CompanyRoleId) && p.CanView)
            .Select(p => p.PlatformModuleId)
            .Distinct()
            .ToListAsync();

        // Bu capability'ye map edilmis modul ID'lerini al
        var capabilityModuleIds = await _context.CapabilityModuleMappings
            .Where(m => m.CapabilityId == capabilityId)
            .Select(m => m.PlatformModuleId)
            .ToListAsync();

        // Her iki listede de olan modulleri getir
        var finalModuleIds = accessibleModuleIds.Intersect(capabilityModuleIds).ToList();

        var allModules = await _context.PlatformModules
            .Where(m => finalModuleIds.Contains(m.Id) && m.IsActive && m.IsMenuItem)
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new CapabilityModuleDto
            {
                Id = m.Id,
                ParentId = m.ParentId,
                Name = m.Name,
                DisplayName = m.DisplayName,
                DisplayNameResourceKey = m.DisplayNameResourceKey,
                Description = m.Description,
                Icon = m.Icon,
                Route = m.Route,
                DisplayOrder = m.DisplayOrder,
                IsMenuItem = m.IsMenuItem,
                IsMenuSection = m.IsMenuSection
            })
            .ToListAsync();

        return BuildModuleTree(allModules);
    }

    public async Task<List<CapabilityModuleDto>> GetPlatformModulesAsync()
    {
        // Tum platform modullerini getir
        var allModules = await _context.PlatformModules
            .Where(m => m.IsActive && m.IsMenuItem)
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new CapabilityModuleDto
            {
                Id = m.Id,
                ParentId = m.ParentId,
                Name = m.Name,
                DisplayName = m.DisplayName,
                DisplayNameResourceKey = m.DisplayNameResourceKey,
                Description = m.Description,
                Icon = m.Icon,
                Route = m.Route,
                DisplayOrder = m.DisplayOrder,
                IsMenuItem = m.IsMenuItem,
                IsMenuSection = m.IsMenuSection
            })
            .ToListAsync();

        return BuildModuleTree(allModules);
    }

    public Task<VendorCapabilityDto?> GetPlatformCapabilityAsync()
    {
        // Platform capability artik TypeDefinitions'da tanimli degil
        // Bu metod geriye donuk uyumluluk icin null donduruyor
        return Task.FromResult<VendorCapabilityDto?>(null);
    }

    public async Task<bool> AddVendorCapabilityAsync(int vendorId, int capabilityId)
    {
        var exists = await _context.VendorCapabilityMappings
            .AnyAsync(m => m.VendorId == vendorId && m.CapabilityId == capabilityId);

        if (exists)
            return false;

        var mapping = new VendorCapabilityMapping
        {
            VendorId = vendorId,
            CapabilityId = capabilityId,
            IsActive = true
        };

        _context.VendorCapabilityMappings.Add(mapping);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> AddVendorCapabilitiesAsync(int vendorId, IEnumerable<int> capabilityIds)
    {
        var ids = capabilityIds.ToList();
        int added = 0;

        foreach (var capabilityId in ids)
        {
            // Gecerli capability mi kontrol et
            var capability = Capabilities.GetById(capabilityId);
            if (capability == null || !capability.IsActive)
                continue;

            var exists = await _context.VendorCapabilityMappings
                .AnyAsync(m => m.VendorId == vendorId && m.CapabilityId == capabilityId);

            if (!exists)
            {
                var mapping = new VendorCapabilityMapping
                {
                    VendorId = vendorId,
                    CapabilityId = capabilityId,
                    IsActive = true
                };
                _context.VendorCapabilityMappings.Add(mapping);
                added++;
            }
        }

        if (added > 0)
            await _context.SaveChangesAsync();

        return added;
    }

    public async Task<bool> RemoveVendorCapabilityAsync(int vendorId, int capabilityId)
    {
        var mapping = await _context.VendorCapabilityMappings
            .FirstOrDefaultAsync(m => m.VendorId == vendorId && m.CapabilityId == capabilityId);

        if (mapping == null)
            return false;

        mapping.IsActive = false;
        mapping.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignUserRoleAsync(int userId, int vendorId, int companyRoleId, int assignedByUserId)
    {
        var exists = await _context.CompanyRoleUserMappings
            .AnyAsync(r => r.UserId == userId && r.VendorId == vendorId && r.CompanyRoleId == companyRoleId);

        if (exists)
            return false;

        var roleMapping = new CompanyRoleUserMapping
        {
            UserId = userId,
            VendorId = vendorId,
            CompanyRoleId = companyRoleId,
            IsActive = true,
            AssignedByUserId = assignedByUserId,
            AssignedAt = DateTime.UtcNow
        };

        _context.CompanyRoleUserMappings.Add(roleMapping);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveUserRoleAsync(int userId, int vendorId, int companyRoleId)
    {
        var roleMapping = await _context.CompanyRoleUserMappings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.VendorId == vendorId && r.CompanyRoleId == companyRoleId);

        if (roleMapping == null)
            return false;

        roleMapping.IsActive = false;
        roleMapping.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnsureVendorCapabilityWithDefaultRoleAsync(int vendorId, int capabilityId)
    {
        // Gecerli capability mi kontrol et
        var capability = Capabilities.GetById(capabilityId);
        if (capability == null || !capability.IsActive)
            return false;

        // Vendor zaten bu capability'ye sahip mi kontrol et
        var existingMapping = await _context.VendorCapabilityMappings
            .FirstOrDefaultAsync(m => m.VendorId == vendorId && m.CapabilityId == capabilityId);

        if (existingMapping != null)
        {
            // Zaten var, deaktifse aktifle
            if (!existingMapping.IsActive)
            {
                existingMapping.IsActive = true;
                existingMapping.IsDeleted = false;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Zaten aktif, bir sey yapma
                return false;
            }
        }
        else
        {
            // Yeni capability mapping olustur
            var mapping = new VendorCapabilityMapping
            {
                VendorId = vendorId,
                CapabilityId = capabilityId,
                IsActive = true
            };
            _context.VendorCapabilityMappings.Add(mapping);
            await _context.SaveChangesAsync();
        }

        // Vendor'in owner'ini bul (VendorTeamMember.Source = OwnerCreated)
        var ownerMember = await _context.VendorTeamMembers
            .FirstOrDefaultAsync(m => m.VendorId == vendorId && m.Source == TeamMemberSource.OwnerCreated && !m.IsDeleted);
        var ownerUser = ownerMember?.UserId != null
            ? await _context.Users.FindAsync(ownerMember.UserId)
            : null;

        if (ownerUser == null)
            return true; // Capability eklendi ama owner bulunamadi

        // Bu capability icin default role'leri bul
        var defaultRoles = await _context.CompanyRoles
            .Where(r => r.IsDefault && r.IsActive && r.CapabilityId == capabilityId)
            .ToListAsync();

        // Her default role'u owner'a ata (yoksa)
        foreach (var role in defaultRoles)
        {
            var hasRole = await _context.CompanyRoleUserMappings
                .AnyAsync(r => r.UserId == ownerUser.Id && r.VendorId == vendorId && r.CompanyRoleId == role.Id && r.IsActive);

            if (!hasRole)
            {
                _context.CompanyRoleUserMappings.Add(new CompanyRoleUserMapping
                {
                    UserId = ownerUser.Id,
                    VendorId = vendorId,
                    CompanyRoleId = role.Id,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private List<CapabilityModuleDto> BuildModuleTree(List<CapabilityModuleDto> allModules)
    {
        var lookup = allModules.ToLookup(m => m.ParentId);
        var rootModules = lookup[null].ToList();

        foreach (var module in allModules)
        {
            module.Children = lookup[module.Id].OrderBy(m => m.DisplayOrder).ToList();
        }

        return rootModules.OrderBy(m => m.DisplayOrder).ToList();
    }
}
