using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly ILocalizationService _localizationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AdminService(
        ApplicationDbContext context,
        ILocalizationService localizationService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _localizationService = localizationService;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    #region Dashboard

    public async Task<AdminDashboardStats> GetDashboardStatsAsync()
    {
        return new AdminDashboardStats
        {
            VendorCount = await _context.Vendors.CountAsync(),
            UserCount = await _context.Users.CountAsync(),
            CapabilityCount = Capabilities.All.Count(),
            RoleCount = await _context.CompanyRoles.CountAsync()
        };
    }

    public async Task<List<RecentVendorDto>> GetRecentVendorsAsync(int count = 5)
    {
        return await _context.Vendors
            .OrderByDescending(v => v.CreatedAt)
            .Take(count)
            .Select(v => new RecentVendorDto
            {
                Id = v.Id,
                CompanyName = v.CompanyName,
                VendorStatusId = v.VendorStatusId,
                StatusText = GetStatusText(v.VendorStatusId),
                StatusClass = GetStatusClass(v.VendorStatusId),
                CreatedAt = v.CreatedAt.ToString("dd.MM.yyyy")
            })
            .ToListAsync();
    }

    public async Task<List<CapabilityStatDto>> GetCapabilityStatsAsync()
    {
        // Capability vendor sayilarini DB'den al
        var vendorCounts = await _context.VendorCapabilityMappings
            .Where(m => m.IsActive)
            .GroupBy(m => m.CapabilityId)
            .Select(g => new { CapabilityId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CapabilityId, x => x.Count);

        var stats = Capabilities.All.Select(c => new
        {
            c.Id,
            Name = c.Description ?? c.SystemName,
            c.Icon,
            VendorCount = vendorCounts.GetValueOrDefault(c.Id, 0)
        }).ToList();

        var total = stats.Sum(s => s.VendorCount);

        return stats.Select(s => new CapabilityStatDto
        {
            Id = s.Id,
            Name = s.Name,
            Icon = s.Icon,
            VendorCount = s.VendorCount,
            Percentage = total > 0 ? (s.VendorCount * 100 / total) : 0
        }).ToList();
    }

    #endregion

    #region Capabilities

    public async Task<List<AdminCapabilityDto>> GetCapabilitiesAsync()
    {
        // Capability istatistiklerini DB'den al
        var vendorCounts = await _context.VendorCapabilityMappings
            .Where(m => m.IsActive)
            .GroupBy(m => m.CapabilityId)
            .Select(g => new { CapabilityId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CapabilityId, x => x.Count);

        var moduleCounts = await _context.CapabilityModuleMappings
            .GroupBy(m => m.CapabilityId)
            .Select(g => new { CapabilityId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CapabilityId, x => x.Count);

        var roleCounts = await _context.CompanyRoles
            .GroupBy(r => r.CapabilityId)
            .Select(g => new { CapabilityId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CapabilityId, x => x.Count);

        return Capabilities.All
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new AdminCapabilityDto
            {
                Id = c.Id,
                Name = c.Description ?? c.SystemName,
                Description = c.Description,
                Icon = c.Icon,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                VendorCount = vendorCounts.GetValueOrDefault(c.Id, 0),
                ModuleCount = moduleCounts.GetValueOrDefault(c.Id, 0),
                RoleCount = roleCounts.GetValueOrDefault(c.Id, 0)
            })
            .ToList();
    }

    // Create/Update/Delete kaldirildi - Capabilities artik TypeDefinitions.cs'de static tanimli

    #endregion

    #region Modules (PlatformModules)

    public async Task<List<AdminModuleDto>> GetAllModulesAsync()
    {
        // Tum aktif modulleri getir
        var modules = await _context.PlatformModules
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.Name)
            .Select(m => new AdminModuleDto
            {
                Id = m.Id,
                ParentId = m.ParentId,
                Name = m.Name,
                DisplayName = m.DisplayName,
                DisplayNameResourceKey = m.DisplayNameResourceKey,
                Description = m.Description,
                Icon = m.Icon,
                Route = m.Route,
                IsMenuSection = m.IsMenuSection,
                DisplayOrder = m.DisplayOrder,
                IsMenuItem = m.IsMenuItem,
                IsActive = m.IsActive,
                CapabilityIds = m.CapabilityMappings
                    .Select(cm => cm.CapabilityId)
                    .ToList()
            })
            .ToListAsync();

        // Tree olarak don (children'li)
        return BuildModuleTree(modules, null);
    }

    public async Task<List<AdminModuleDto>> GetDeletedModulesAsync()
    {
        var modules = await _context.PlatformModules
            .IgnoreQueryFilters()
            .Where(m => m.IsDeleted)
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.Name)
            .Select(m => new AdminModuleDto
            {
                Id = m.Id,
                ParentId = m.ParentId,
                Name = m.Name,
                DisplayName = m.DisplayName,
                DisplayNameResourceKey = m.DisplayNameResourceKey,
                Description = m.Description,
                Icon = m.Icon,
                Route = m.Route,
                IsMenuSection = m.IsMenuSection,
                DisplayOrder = m.DisplayOrder,
                IsMenuItem = m.IsMenuItem,
                IsActive = m.IsActive,
                CapabilityIds = new List<int>()
            })
            .ToListAsync();

        return BuildModuleTree(modules, null);
    }

    public async Task<ServiceResult<int>> CreateModuleAsync(AdminModuleCreateDto dto)
    {
        // DisplayNameResourceKey benzersiz olmali
        if (string.IsNullOrWhiteSpace(dto.DisplayNameResourceKey))
            return ServiceResult<int>.Fail("Resource Key zorunludur");

        if (await _context.PlatformModules.AnyAsync(m => m.DisplayNameResourceKey == dto.DisplayNameResourceKey))
            return ServiceResult<int>.Fail("Bu Resource Key zaten kullaniliyor");

        // Name benzersiz mi kontrol et
        if (await _context.PlatformModules.AnyAsync(m => m.Name == dto.Name))
            return ServiceResult<int>.Fail("Bu modul adi zaten kullaniliyor");

        // ParentId gecerliyse kontrol et
        if (dto.ParentId.HasValue)
        {
            var parent = await _context.PlatformModules.FindAsync(dto.ParentId.Value);
            if (parent == null)
                return ServiceResult<int>.Fail("Ust modul bulunamadi");
        }

        var module = new PlatformModule
        {
            ParentId = dto.ParentId,
            Name = dto.Name,
            DisplayName = dto.DisplayName,
            DisplayNameResourceKey = dto.DisplayNameResourceKey,
            Description = dto.Description,
            Icon = dto.Icon,
            Route = dto.Route,
            IsMenuSection = dto.IsMenuSection,
            DisplayOrder = dto.DisplayOrder,
            IsMenuItem = dto.IsMenuItem,
            IsActive = dto.IsActive
        };

        _context.PlatformModules.Add(module);
        await _context.SaveChangesAsync();

        // Capability mapping'leri ekle
        if (dto.CapabilityIds != null && dto.CapabilityIds.Any())
        {
            foreach (var capId in dto.CapabilityIds)
            {
                _context.CapabilityModuleMappings.Add(new CapabilityModuleMapping
                {
                    PlatformModuleId = module.Id,
                    CapabilityId = capId
                });
            }
            await _context.SaveChangesAsync();
        }

        return ServiceResult<int>.Ok(module.Id, "Modul olusturuldu");
    }

    public async Task<ServiceResult> UpdateModuleAsync(int id, AdminModuleUpdateDto dto)
    {
        var module = await _context.PlatformModules.FindAsync(id);
        if (module == null)
            return ServiceResult.Fail("Modul bulunamadi");

        // ParentId gecerliyse kontrol et (kendisini parent olarak secemez)
        if (dto.ParentId.HasValue)
        {
            if (dto.ParentId == id)
                return ServiceResult.Fail("Modul kendisinin alt modulu olamaz");

            var parent = await _context.PlatformModules.FindAsync(dto.ParentId.Value);
            if (parent == null)
                return ServiceResult.Fail("Ust modul bulunamadi");

            // Parent bu modulun alt modulu olamaz (dongu engelleme)
            if (await IsDescendantOfModule(dto.ParentId.Value, id))
                return ServiceResult.Fail("Dongusel iliski olusturulamaz");
        }

        // Name degistiyse benzersiz mi kontrol et
        if (module.Name != dto.Name && await _context.PlatformModules.AnyAsync(m => m.Name == dto.Name && m.Id != id))
            return ServiceResult.Fail("Bu modul adi zaten kullaniliyor");

        // ResourceKey degistiyse benzersiz mi kontrol et
        if (module.DisplayNameResourceKey != dto.DisplayNameResourceKey &&
            await _context.PlatformModules.AnyAsync(m => m.DisplayNameResourceKey == dto.DisplayNameResourceKey && m.Id != id))
            return ServiceResult.Fail("Bu Resource Key zaten kullaniliyor");

        module.ParentId = dto.ParentId;
        module.Name = dto.Name;
        module.DisplayName = dto.DisplayName;
        module.DisplayNameResourceKey = dto.DisplayNameResourceKey;
        module.Description = dto.Description;
        module.Icon = dto.Icon;
        module.Route = dto.Route;
        module.IsMenuSection = dto.IsMenuSection;
        module.DisplayOrder = dto.DisplayOrder;
        module.IsMenuItem = dto.IsMenuItem;
        module.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Modul guncellendi");
    }

    public async Task<ServiceResult> DeleteModuleAsync(int id)
    {
        var module = await _context.PlatformModules.FindAsync(id);
        if (module == null)
            return ServiceResult.Fail("Modul bulunamadi");

        // Alt modulleri bul
        var children = await _context.PlatformModules
            .Where(m => m.ParentId == id)
            .ToListAsync();

        // Silinecek tum modul ID'lerini topla
        var moduleIdsToDelete = new List<int> { id };
        await CollectChildModuleIds(id, moduleIdsToDelete);

        // Alt modulleri de soft delete yap
        foreach (var child in children)
        {
            child.IsDeleted = true;
        }
        module.IsDeleted = true;

        // Capability mapping'leri sil
        var mappings = await _context.CapabilityModuleMappings
            .Where(m => moduleIdsToDelete.Contains(m.PlatformModuleId))
            .ToListAsync();
        _context.CapabilityModuleMappings.RemoveRange(mappings);

        // Izinleri sil
        var permissions = await _context.CompanyRoleModulePermissions
            .Where(p => moduleIdsToDelete.Contains(p.PlatformModuleId))
            .ToListAsync();
        _context.CompanyRoleModulePermissions.RemoveRange(permissions);

        await _context.SaveChangesAsync();

        var deletedCount = moduleIdsToDelete.Count;
        return ServiceResult.Ok(deletedCount > 1
            ? $"{deletedCount} modul silindi"
            : "Modul silindi");
    }

    public async Task<ServiceResult> RestoreModuleAsync(int id)
    {
        var module = await _context.PlatformModules
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == id && m.IsDeleted);

        if (module == null)
            return ServiceResult.Fail("Silinmis modul bulunamadi");

        // Parent silinmis mi kontrol et
        if (module.ParentId.HasValue)
        {
            var parent = await _context.PlatformModules.FindAsync(module.ParentId.Value);
            if (parent == null)
            {
                // Parent hala silinmis, parent'i da geri yukle veya orphan yap
                module.ParentId = null;
            }
        }

        // Alt modulleri de restore et
        var children = await _context.PlatformModules
            .IgnoreQueryFilters()
            .Where(m => m.ParentId == id && m.IsDeleted)
            .ToListAsync();

        foreach (var child in children)
        {
            child.IsDeleted = false;
        }

        module.IsDeleted = false;
        await _context.SaveChangesAsync();

        var restoredCount = children.Count + 1;
        return ServiceResult.Ok(restoredCount > 1
            ? $"{restoredCount} modul geri yuklendi"
            : "Modul geri yuklendi");
    }

    public async Task<ServiceResult> HardDeleteModuleAsync(int id)
    {
        var module = await _context.PlatformModules
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == id && m.IsDeleted);

        if (module == null)
            return ServiceResult.Fail("Silinmis modul bulunamadi");

        // Alt modulleri bul (recursive)
        var allModuleIds = new List<int> { id };
        await CollectChildModuleIdsIgnoreFilter(id, allModuleIds);

        // Mapping'leri sil
        var mappings = await _context.CapabilityModuleMappings
            .Where(m => allModuleIds.Contains(m.PlatformModuleId))
            .ToListAsync();
        _context.CapabilityModuleMappings.RemoveRange(mappings);

        // Izinleri sil
        var permissions = await _context.CompanyRoleModulePermissions
            .Where(p => allModuleIds.Contains(p.PlatformModuleId))
            .ToListAsync();
        _context.CompanyRoleModulePermissions.RemoveRange(permissions);

        // Modulleri sil (alt moduller dahil)
        var modulesToDelete = await _context.PlatformModules
            .IgnoreQueryFilters()
            .Where(m => allModuleIds.Contains(m.Id))
            .ToListAsync();
        _context.PlatformModules.RemoveRange(modulesToDelete);

        await _context.SaveChangesAsync();

        return ServiceResult.Ok(allModuleIds.Count > 1
            ? $"{allModuleIds.Count} modul kalici olarak silindi"
            : "Modul kalici olarak silindi");
    }

    public async Task<List<ModuleCapabilityMappingDto>> GetModuleCapabilitiesAsync(int moduleId)
    {
        // Bu modulun mevcut mapping'lerini getir
        var existingMappings = await _context.CapabilityModuleMappings
            .Where(m => m.PlatformModuleId == moduleId)
            .Select(m => m.CapabilityId)
            .ToListAsync();

        // Tum capability'leri getir (TypeDefinitions'dan)
        return Capabilities.All
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new ModuleCapabilityMappingDto
            {
                CapabilityId = c.Id,
                CapabilityName = c.Description ?? c.SystemName,
                CapabilityIcon = c.Icon,
                IsMapped = existingMappings.Contains(c.Id)
            }).ToList();
    }

    public async Task<ServiceResult> SaveModuleCapabilitiesAsync(int moduleId, List<int> capabilityIds)
    {
        var module = await _context.PlatformModules.FindAsync(moduleId);
        if (module == null)
            return ServiceResult.Fail("Modul bulunamadi");

        // Mevcut mapping'leri sil
        var existingMappings = await _context.CapabilityModuleMappings
            .Where(m => m.PlatformModuleId == moduleId)
            .ToListAsync();
        _context.CapabilityModuleMappings.RemoveRange(existingMappings);

        // Yeni mapping'leri ekle
        foreach (var capId in capabilityIds)
        {
            _context.CapabilityModuleMappings.Add(new CapabilityModuleMapping
            {
                PlatformModuleId = moduleId,
                CapabilityId = capId
            });
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Capability atamasi kaydedildi");
    }

    public async Task<List<CapabilityModuleSelectionDto>> GetCapabilityModulesAsync(int capabilityId)
    {
        // Tum platform modullerini getir
        var allModules = await _context.PlatformModules
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.Name)
            .Select(m => new CapabilityModuleSelectionDto
            {
                Id = m.Id,
                ParentId = m.ParentId,
                Name = m.Name,
                DisplayName = m.DisplayName,
                DisplayNameResourceKey = m.DisplayNameResourceKey,
                Icon = m.Icon,
                Route = m.Route,
                IsMenuSection = m.IsMenuSection,
                IsMenuItem = m.IsMenuItem,
                IsActive = m.IsActive,
                DisplayOrder = m.DisplayOrder,
                IsSelected = false
            })
            .ToListAsync();

        // Bu capability'ye atanmis modulleri bul
        var selectedModuleIds = await _context.CapabilityModuleMappings
            .Where(m => m.CapabilityId == capabilityId)
            .Select(m => m.PlatformModuleId)
            .ToListAsync();

        // IsSelected flag'ini set et
        foreach (var module in allModules)
        {
            module.IsSelected = selectedModuleIds.Contains(module.Id);
        }

        // Tree yapisi olustur
        return BuildCapabilityModuleTree(allModules, null);
    }

    public async Task<ServiceResult> SaveCapabilityModulesAsync(int capabilityId, List<int> moduleIds)
    {
        var capability = Capabilities.GetById(capabilityId);
        if (capability == null)
            return ServiceResult.Fail("Capability bulunamadi");

        // Mevcut mapping'leri sil
        var existingMappings = await _context.CapabilityModuleMappings
            .Where(m => m.CapabilityId == capabilityId)
            .ToListAsync();
        _context.CapabilityModuleMappings.RemoveRange(existingMappings);

        // Yeni mapping'leri ekle
        foreach (var moduleId in moduleIds)
        {
            _context.CapabilityModuleMappings.Add(new CapabilityModuleMapping
            {
                CapabilityId = capabilityId,
                PlatformModuleId = moduleId
            });
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"{moduleIds.Count} modul atamasi kaydedildi");
    }

    public async Task<ServiceResult> AddModuleToCapabilityAsync(int capabilityId, int moduleId)
    {
        var capability = Capabilities.GetById(capabilityId);
        if (capability == null)
            return ServiceResult.Fail("Capability bulunamadi");

        var module = await _context.PlatformModules.FindAsync(moduleId);
        if (module == null)
            return ServiceResult.Fail("Modul bulunamadi");

        // Zaten ekli mi kontrol et
        var existing = await _context.CapabilityModuleMappings
            .AnyAsync(m => m.CapabilityId == capabilityId && m.PlatformModuleId == moduleId);

        if (existing)
            return ServiceResult.Fail("Bu modul zaten bu capability'ye ekli");

        // Yeni mapping olustur
        _context.CapabilityModuleMappings.Add(new CapabilityModuleMapping
        {
            CapabilityId = capabilityId,
            PlatformModuleId = moduleId
        });

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Modul capability'ye eklendi");
    }

    public async Task<ServiceResult> AddMultipleModulesToCapabilityAsync(int capabilityId, List<int> moduleIds)
    {
        var capability = Capabilities.GetById(capabilityId);
        if (capability == null)
            return ServiceResult.Fail("Capability bulunamadi");

        if (moduleIds == null || !moduleIds.Any())
            return ServiceResult.Fail("En az bir modul secilmelidir");

        // Mevcut mapping'leri kontrol et
        var existingModuleIds = await _context.CapabilityModuleMappings
            .Where(m => m.CapabilityId == capabilityId && moduleIds.Contains(m.PlatformModuleId))
            .Select(m => m.PlatformModuleId)
            .ToListAsync();

        var addedCount = 0;

        foreach (var moduleId in moduleIds)
        {
            // Zaten ekli ise skip
            if (existingModuleIds.Contains(moduleId))
                continue;

            // Modul var mi kontrol et
            var moduleExists = await _context.PlatformModules.AnyAsync(m => m.Id == moduleId);
            if (moduleExists)
            {
                _context.CapabilityModuleMappings.Add(new CapabilityModuleMapping
                {
                    CapabilityId = capabilityId,
                    PlatformModuleId = moduleId
                });
                addedCount++;
            }
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok($"{addedCount} modul capability'ye eklendi");
    }

    public async Task<ServiceResult> RemoveModuleFromCapabilityAsync(int capabilityId, int moduleId)
    {
        var mapping = await _context.CapabilityModuleMappings
            .FirstOrDefaultAsync(m => m.CapabilityId == capabilityId && m.PlatformModuleId == moduleId);

        if (mapping == null)
            return ServiceResult.Fail("Modul bu capability'de bulunamadi");

        _context.CapabilityModuleMappings.Remove(mapping);
        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Modul capability'den cikarildi");
    }

    #endregion

    #region Roles

    public async Task<List<AdminRoleDto>> GetRolesAsync()
    {
        var dbRoles = await _context.CompanyRoles
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.NameResourceKey,
                r.Description,
                r.CapabilityId,
                r.IsDefault,
                r.IsActive,
                UserCount = _context.CompanyRoleUserMappings.Count(ur => ur.CompanyRoleId == r.Id && ur.IsActive)
            })
            .ToListAsync();

        var roles = dbRoles.Select(r =>
        {
            var capability = Capabilities.GetById(r.CapabilityId);
            return new AdminRoleDto
            {
                Id = r.Id,
                Name = r.Name,
                NameResourceKey = r.NameResourceKey,
                Description = r.Description,
                CapabilityId = r.CapabilityId,
                CapabilityName = capability?.Description ?? capability?.SystemName ?? "",
                CapabilityIcon = capability?.Icon,
                IsDefault = r.IsDefault,
                IsActive = r.IsActive,
                UserCount = r.UserCount
            };
        })
        .OrderBy(r => Capabilities.GetById(r.CapabilityId)?.DisplayOrder ?? 0)
        .ThenBy(r => r.Name)
        .ToList();

        // Localize role names
        foreach (var role in roles)
        {
            role.LocalizedName = !string.IsNullOrEmpty(role.NameResourceKey)
                ? _localizationService.T(role.NameResourceKey, role.Name)
                : role.Name;
        }

        return roles;
    }

    public async Task<ServiceResult<int>> CreateRoleAsync(AdminRoleCreateDto dto)
    {
        var role = new CompanyRole
        {
            Name = dto.Name,
            NameResourceKey = dto.NameResourceKey,
            Description = dto.Description,
            CapabilityId = dto.CapabilityId,
            IsDefault = dto.IsDefault,
            IsActive = dto.IsActive
        };

        _context.CompanyRoles.Add(role);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Ok(role.Id, "Rol olusturuldu");
    }

    public async Task<ServiceResult> UpdateRoleAsync(int id, AdminRoleCreateDto dto)
    {
        var role = await _context.CompanyRoles.FindAsync(id);
        if (role == null)
            return ServiceResult.Fail("Rol bulunamadi");

        role.Name = dto.Name;
        role.NameResourceKey = dto.NameResourceKey;
        role.Description = dto.Description;
        role.IsDefault = dto.IsDefault;
        role.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Rol guncellendi");
    }

    public async Task<ServiceResult> DeleteRoleAsync(int id, int? targetRoleId = null)
    {
        var role = await _context.CompanyRoles.FindAsync(id);
        if (role == null)
            return ServiceResult.Fail("Rol bulunamadi");

        // Kullanicilar var mi?
        var userMappings = await _context.CompanyRoleUserMappings
            .Where(ur => ur.CompanyRoleId == id && ur.IsActive)
            .ToListAsync();

        if (userMappings.Any())
        {
            // Hedef rol belirtilmemisse hata
            if (!targetRoleId.HasValue)
                return ServiceResult.Fail("Bu rolde kullanicilar var. Tasinacak rol belirtilmeli.");

            // Hedef rol ayni capability'de olmali
            var targetRole = await _context.CompanyRoles.FindAsync(targetRoleId.Value);
            if (targetRole == null || targetRole.CapabilityId != role.CapabilityId)
                return ServiceResult.Fail("Hedef rol ayni yetenek altinda olmali");

            // Kullanicilari tasi
            foreach (var mapping in userMappings)
            {
                mapping.CompanyRoleId = targetRoleId.Value;
            }
        }

        role.IsDeleted = true;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Rol silindi");
    }

    #endregion

    #region Role Permissions

    public async Task<List<ModulePermissionViewDto>> GetRolePermissionsAsync(int roleId)
    {
        var role = await _context.CompanyRoles
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
            return new List<ModulePermissionViewDto>();

        // Bu capability'nin gorebilecegi modulleri getir (CapabilityModuleMappings uzerinden)
        var moduleIds = await _context.CapabilityModuleMappings
            .Where(cm => cm.CapabilityId == role.CapabilityId)
            .Select(cm => cm.PlatformModuleId)
            .ToListAsync();

        var modules = await _context.PlatformModules
            .Where(m => moduleIds.Contains(m.Id))
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();

        // Bu rol icin mevcut izinleri getir
        var permissions = await _context.CompanyRoleModulePermissions
            .Where(p => p.CompanyRoleId == roleId)
            .ToListAsync();

        return modules.Select(m =>
        {
            var perm = permissions.FirstOrDefault(p => p.PlatformModuleId == m.Id);
            return new ModulePermissionViewDto
            {
                Id = m.Id,
                Name = m.Name,
                DisplayNameResourceKey = m.DisplayNameResourceKey,
                Icon = m.Icon,
                ParentId = m.ParentId,
                Level = GetPlatformModuleLevel(modules, m),
                IsParent = modules.Any(c => c.ParentId == m.Id),
                IsSection = m.IsMenuSection,
                CanView = perm?.CanView ?? false,
                CanCreate = perm?.CanCreate ?? false,
                CanEdit = perm?.CanEdit ?? false,
                CanDelete = perm?.CanDelete ?? false
            };
        })
        .OrderBy(m => m.ParentId ?? 0)
        .ThenBy(m => m.Level)
        .ToList();
    }

    public async Task<List<CapabilityModulesGroupDto>> GetAllModulesGroupedByCapabilityAsync(int roleId)
    {
        // Rol bilgisini al
        var role = await _context.CompanyRoles
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
            return new List<CapabilityModulesGroupDto>();

        // Sadece rolun ait oldugu capability'yi al (tek capability)
        var capability = Capabilities.GetById(role.CapabilityId);
        if (capability == null || !capability.IsActive)
            return new List<CapabilityModulesGroupDto>();

        var capabilityList = new List<TypeItem> { capability };

        // Bu rol icin mevcut izinleri getir
        var permissions = await _context.CompanyRoleModulePermissions
            .Where(p => p.CompanyRoleId == roleId)
            .ToListAsync();

        var result = new List<CapabilityModulesGroupDto>();

        foreach (var cap in capabilityList)
        {
            // Bu capability'nin modüllerini al
            var moduleIds = await _context.CapabilityModuleMappings
                .Where(cm => cm.CapabilityId == cap.Id)
                .Select(cm => cm.PlatformModuleId)
                .ToListAsync();

            var modules = await _context.PlatformModules
                .Where(m => moduleIds.Contains(m.Id))
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();

            if (!modules.Any()) continue;

            var moduleList = modules.Select(m =>
            {
                var perm = permissions.FirstOrDefault(p => p.PlatformModuleId == m.Id);
                return new ModulePermissionViewDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    DisplayNameResourceKey = m.DisplayNameResourceKey,
                    Icon = m.Icon,
                    ParentId = m.ParentId,
                    Level = GetPlatformModuleLevel(modules, m),
                    IsParent = modules.Any(c => c.ParentId == m.Id),
                    IsSection = m.IsMenuSection,
                    CanView = perm?.CanView ?? false,
                    CanCreate = perm?.CanCreate ?? false,
                    CanEdit = perm?.CanEdit ?? false,
                    CanDelete = perm?.CanDelete ?? false
                };
            })
            .OrderBy(m => m.ParentId ?? 0)
            .ThenBy(m => m.Level)
            .ToList();

            result.Add(new CapabilityModulesGroupDto
            {
                CapabilityId = cap.Id,
                CapabilityName = cap.Description ?? cap.SystemName,
                CapabilityIcon = cap.Icon,
                Modules = moduleList
            });
        }

        return result;
    }

    public async Task<ServiceResult> SaveRolePermissionsAsync(int roleId, List<PermissionSaveDto> permissions)
    {
        var role = await _context.CompanyRoles.FindAsync(roleId);
        if (role == null)
            return ServiceResult.Fail("Rol bulunamadi");

        // Mevcut izinleri sil
        var existing = await _context.CompanyRoleModulePermissions
            .Where(p => p.CompanyRoleId == roleId)
            .ToListAsync();
        _context.CompanyRoleModulePermissions.RemoveRange(existing);

        // Yeni izinleri ekle
        foreach (var perm in permissions)
        {
            if (perm.CanView || perm.CanCreate || perm.CanEdit || perm.CanDelete)
            {
                _context.CompanyRoleModulePermissions.Add(new CompanyRoleModulePermission
                {
                    CompanyRoleId = roleId,
                    PlatformModuleId = perm.ModuleId,
                    CanView = perm.CanView,
                    CanCreate = perm.CanCreate,
                    CanEdit = perm.CanEdit,
                    CanDelete = perm.CanDelete
                });
            }
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Izinler kaydedildi");
    }

    #endregion

    #region Users

    public async Task<List<AdminUserDto>> GetUsersAsync(string? search = null, string? roleFilter = null)
    {
        var query = _context.Users
            .Include(u => u.Vendor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search) ||
                (u.Vendor != null && u.Vendor.CompanyName.ToLower().Contains(search)));
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Take(100)
            .ToListAsync();

        // Owner olan kullanicilarin ID'lerini al (VendorTeamMember.Source = OwnerCreated)
        var ownerUserIds = await _context.VendorTeamMembers
            .Where(m => m.Source == TeamMemberSource.OwnerCreated && !m.IsDeleted && m.UserId.HasValue)
            .Select(m => m.UserId!.Value)
            .ToListAsync();
        var ownerSet = new HashSet<int>(ownerUserIds);

        var result = new List<AdminUserDto>();
        foreach (var u in users)
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == u.Id)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();

            // Role filter
            if (!string.IsNullOrEmpty(roleFilter) && !roles.Contains(roleFilter))
                continue;

            result.Add(new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email ?? "",
                FirstName = u.FirstName,
                LastName = u.LastName,
                VendorId = u.VendorId,
                CompanyName = u.Vendor?.CompanyName,
                VendorRole = ownerSet.Contains(u.Id) ? "Owner" : "Member",
                SystemRoles = roles!,
                IsActive = u.IsActive,
                IsSystemAdmin = u.IsSystemAdmin,
                LastLoginAt = u.LastLoginAt?.ToString("dd.MM.yyyy HH:mm"),
                CreatedAt = u.CreatedAt.ToString("dd.MM.yyyy")
            });
        }

        return result;
    }

    public async Task<AdminUserDetailDto?> GetUserByIdAsync(int id)
    {
        var u = await _context.Users
            .Include(u => u.Vendor)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (u == null) return null;

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == u.Id)
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .ToListAsync();

        // Owner kontrolu (VendorTeamMember.Source = OwnerCreated)
        var isOwner = await _context.VendorTeamMembers
            .AnyAsync(m => m.UserId == u.Id && m.Source == TeamMemberSource.OwnerCreated && !m.IsDeleted);

        return new AdminUserDetailDto
        {
            Id = u.Id,
            Email = u.Email ?? "",
            FirstName = u.FirstName,
            LastName = u.LastName,
            Phone = u.PhoneNumber,
            VendorId = u.VendorId,
            CompanyName = u.Vendor?.CompanyName,
            VendorRole = isOwner ? "Owner" : "Member",
            SystemRoles = roles!,
            IsActive = u.IsActive,
            IsSystemAdmin = u.IsSystemAdmin,
            EmailConfirmed = u.EmailConfirmed,
            PhoneConfirmed = u.PhoneNumberConfirmed,
            AccessFailedCount = u.AccessFailedCount,
            LockoutEnabled = u.LockoutEnabled,
            LockoutEnd = u.LockoutEnd?.ToString("dd.MM.yyyy HH:mm"),
            LastLoginAt = u.LastLoginAt?.ToString("dd.MM.yyyy HH:mm"),
            CreatedAt = u.CreatedAt.ToString("dd.MM.yyyy")
        };
    }

    public async Task<ServiceResult<int>> CreateUserAsync(AdminUserCreateDto dto)
    {
        // Email kontrolu
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return ServiceResult<int>.Fail("Bu e-posta adresi zaten kullanilmakta");

        // Owner/Member kontrolu
        bool isOwner = dto.VendorRole?.ToLower() == "owner";

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.Phone,
            VendorId = dto.VendorId,
            IsSystemAdmin = dto.IsSystemAdmin,
            EmailConfirmed = true,  // Admin eklediginde dogrulanmis say
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ServiceResult<int>.Fail("Kullanici olusturulamadi: " + errors);
        }

        // User rolunu ata (tum kayitli kullanicilar icin)
        await _userManager.AddToRoleAsync(user, "User");

        // Ek sistem rolu ata (Admin vb.)
        if (!string.IsNullOrEmpty(dto.SystemRole) && dto.SystemRole != "User")
        {
            await _userManager.AddToRoleAsync(user, dto.SystemRole);
        }

        // VendorTeamMember kaydı olustur (VendorId varsa)
        if (dto.VendorId.HasValue)
        {
            var teamMember = new VendorTeamMember
            {
                VendorId = dto.VendorId.Value,
                UserId = user.Id,
                Email = user.Email!,
                Name = user.FullName,
                Source = isOwner ? TeamMemberSource.OwnerCreated : TeamMemberSource.Invitation,
                TeamMemberStatusId = 2, // Active
                ProcessedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.VendorTeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();
        }

        return ServiceResult<int>.Ok(user.Id, "Kullanici olusturuldu");
    }

    public async Task<ServiceResult> ToggleUserStatusAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return ServiceResult.Fail("Kullanici bulunamadi");

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok(user.IsActive ? "Kullanici aktif edildi" : "Kullanici pasif edildi");
    }

    #endregion

    #region Vendors

    public async Task<List<AdminVendorDto>> GetVendorsAsync(string? search = null, int? statusFilter = null)
    {
        var query = _context.Vendors.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(v =>
                v.CompanyName.ToLower().Contains(search) ||
                v.Email.ToLower().Contains(search) ||
                (v.Phone != null && v.Phone.Contains(search)));
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(v => v.VendorStatusId == statusFilter.Value);
        }

        var vendors = await query
            .OrderByDescending(v => v.CreatedAt)
            .Take(100)
            .ToListAsync();

        var result = new List<AdminVendorDto>();
        foreach (var v in vendors)
        {
            // Capability name - ilk capability'yi al
            var capabilityId = await _context.VendorCapabilityMappings
                .Where(vc => vc.VendorId == v.Id && vc.IsActive)
                .Select(vc => vc.CapabilityId)
                .FirstOrDefaultAsync();

            var capabilityName = capabilityId > 0
                ? Capabilities.GetById(capabilityId)?.Description ?? Capabilities.GetById(capabilityId)?.SystemName
                : null;

            // User count
            var userCount = await _context.Users.CountAsync(u => u.VendorId == v.Id);

            result.Add(new AdminVendorDto
            {
                Id = v.Id,
                CompanyName = v.CompanyName,
                Email = v.Email,
                Phone = v.Phone,
                VendorStatusId = v.VendorStatusId,
                StatusText = GetStatusText(v.VendorStatusId),
                StatusClass = GetStatusClass(v.VendorStatusId),
                CapabilityName = capabilityName,
                UserCount = userCount,
                IsVerified = v.IsVerified,
                CreatedAt = v.CreatedAt.ToString("dd.MM.yyyy")
            });
        }

        return result;
    }

    public async Task<AdminVendorDetailDto?> GetVendorByIdAsync(int id)
    {
        var v = await _context.Vendors.FindAsync(id);
        if (v == null) return null;

        // Capabilities
        var capabilityIds = await _context.VendorCapabilityMappings
            .Where(vc => vc.VendorId == v.Id && vc.IsActive)
            .Select(vc => vc.CapabilityId)
            .ToListAsync();

        var capabilities = capabilityIds
            .Select(cid => Capabilities.GetById(cid))
            .Where(c => c != null)
            .Select(c => c!.Description ?? c.SystemName)
            .ToList();

        // Owner olan kullanicilarin ID'lerini al (bu vendor icin)
        var ownerUserIds = await _context.VendorTeamMembers
            .Where(m => m.VendorId == v.Id && m.Source == TeamMemberSource.OwnerCreated && !m.IsDeleted && m.UserId.HasValue)
            .Select(m => m.UserId!.Value)
            .ToListAsync();
        var ownerSet = new HashSet<int>(ownerUserIds);

        // Users
        var userList = await _context.Users
            .Where(u => u.VendorId == v.Id)
            .ToListAsync();

        var users = userList.Select(u => new AdminUserDto
        {
            Id = u.Id,
            Email = u.Email ?? "",
            FirstName = u.FirstName,
            LastName = u.LastName,
            VendorRole = ownerSet.Contains(u.Id) ? "Owner" : "Member",
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt.ToString("dd.MM.yyyy")
        }).ToList();

        return new AdminVendorDetailDto
        {
            Id = v.Id,
            CompanyName = v.CompanyName,
            Email = v.Email,
            Phone = v.Phone,
            VendorStatusId = v.VendorStatusId,
            StatusText = GetStatusText(v.VendorStatusId),
            StatusClass = GetStatusClass(v.VendorStatusId),
            CapabilityName = capabilities.FirstOrDefault(),
            UserCount = users.Count,
            IsVerified = v.IsVerified,
            TaxNumber = v.TaxNumber,
            TaxOffice = v.TaxOffice,
            Website = v.Website,
            LogoUrl = v.LogoUrl,
            IsProfileComplete = v.IsProfileComplete,
            Users = users,
            Capabilities = capabilities!,
            CreatedAt = v.CreatedAt.ToString("dd.MM.yyyy")
        };
    }

    public async Task<ServiceResult> ApproveVendorAsync(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return ServiceResult.Fail("Firma bulunamadi");

        if (vendor.VendorStatusId != 2) // PendingVerification
            return ServiceResult.Fail("Bu firma onay bekliyor durumunda degil");

        vendor.VendorStatusId = 3; // Active
        vendor.IsVerified = true;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Firma onaylandi");
    }

    public async Task<ServiceResult> SuspendVendorAsync(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return ServiceResult.Fail("Firma bulunamadi");

        vendor.VendorStatusId = 4; // Suspended
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Firma askiya alindi");
    }

    public async Task<ServiceResult> ReactivateVendorAsync(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return ServiceResult.Fail("Firma bulunamadi");

        vendor.VendorStatusId = 3; // Active
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Firma yeniden aktif edildi");
    }

    private static string GetStatusText(int vendorStatusId)
    {
        return vendorStatusId switch
        {
            1 => "Profil Bekliyor",      // PendingProfile
            2 => "Dogrulama Bekliyor",   // PendingVerification
            3 => "Aktif",                 // Active
            4 => "Askida",                // Suspended
            5 => "Reddedildi",            // Rejected
            _ => "Bilinmiyor"
        };
    }

    private static string GetStatusClass(int vendorStatusId)
    {
        return vendorStatusId switch
        {
            1 => "bg-warning",   // PendingProfile
            2 => "bg-info",      // PendingVerification
            3 => "bg-success",   // Active
            4 => "bg-secondary", // Suspended
            5 => "bg-danger",    // Rejected
            _ => "bg-secondary"
        };
    }

    #endregion

    #region Languages

    public async Task<List<AdminLanguageDto>> GetLanguagesAsync(bool onlyActive = false)
    {
        var query = _context.Languages.AsQueryable();

        if (onlyActive)
            query = query.Where(l => l.IsActive);

        return await query
            .OrderBy(l => l.DisplayOrder)
            .Select(l => new AdminLanguageDto
            {
                Id = l.Id,
                Name = l.Name,
                NativeName = l.NativeName,
                LanguageCulture = l.LanguageCulture,
                UniqueSeoCode = l.UniqueSeoCode,
                Iso3Code = l.Iso3Code,
                FlagEmoji = l.FlagEmoji,
                FlagImageFileName = l.FlagImageFileName,
                Rtl = l.Rtl,
                IsDefault = l.IsDefault,
                IsGeoTranslationEnabled = l.IsGeoTranslationEnabled,
                IsActive = l.IsActive,
                DisplayOrder = l.DisplayOrder,
                ResourceCount = l.LocaleStringResources.Count
            })
            .ToListAsync();
    }

    public async Task<AdminLanguageDto?> GetLanguageByIdAsync(int id)
    {
        return await _context.Languages
            .Where(l => l.Id == id)
            .Select(l => new AdminLanguageDto
            {
                Id = l.Id,
                Name = l.Name,
                NativeName = l.NativeName,
                LanguageCulture = l.LanguageCulture,
                UniqueSeoCode = l.UniqueSeoCode,
                Iso3Code = l.Iso3Code,
                FlagEmoji = l.FlagEmoji,
                FlagImageFileName = l.FlagImageFileName,
                Rtl = l.Rtl,
                IsDefault = l.IsDefault,
                IsGeoTranslationEnabled = l.IsGeoTranslationEnabled,
                IsActive = l.IsActive,
                DisplayOrder = l.DisplayOrder,
                ResourceCount = l.LocaleStringResources.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<int>> CreateLanguageAsync(AdminLanguageCreateDto dto)
    {
        if (await _context.Languages.AnyAsync(l => l.LanguageCulture == dto.LanguageCulture))
            return ServiceResult<int>.Fail("Bu kultur kodu zaten kullaniliyor");

        if (await _context.Languages.AnyAsync(l => l.UniqueSeoCode == dto.UniqueSeoCode))
            return ServiceResult<int>.Fail("Bu SEO kodu zaten kullaniliyor");

        var language = new Language
        {
            Name = dto.Name,
            NativeName = dto.NativeName,
            LanguageCulture = dto.LanguageCulture,
            UniqueSeoCode = dto.UniqueSeoCode,
            Iso3Code = dto.Iso3Code,
            FlagEmoji = dto.FlagEmoji,
            FlagImageFileName = dto.FlagImageFileName,
            Rtl = dto.Rtl,
            IsGeoTranslationEnabled = dto.IsGeoTranslationEnabled,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder,
            IsDefault = false
        };

        _context.Languages.Add(language);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Ok(language.Id, "Dil olusturuldu");
    }

    public async Task<ServiceResult> UpdateLanguageAsync(int id, AdminLanguageCreateDto dto)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null)
            return ServiceResult.Fail("Dil bulunamadi");

        // Kultur kodu baska bir dilde kullaniliyor mu?
        if (await _context.Languages.AnyAsync(l => l.Id != id && l.LanguageCulture == dto.LanguageCulture))
            return ServiceResult.Fail("Bu kultur kodu baska bir dil tarafindan kullaniliyor");

        if (await _context.Languages.AnyAsync(l => l.Id != id && l.UniqueSeoCode == dto.UniqueSeoCode))
            return ServiceResult.Fail("Bu SEO kodu baska bir dil tarafindan kullaniliyor");

        language.Name = dto.Name;
        language.NativeName = dto.NativeName;
        language.LanguageCulture = dto.LanguageCulture;
        language.UniqueSeoCode = dto.UniqueSeoCode;
        language.Iso3Code = dto.Iso3Code;
        language.FlagEmoji = dto.FlagEmoji;
        language.FlagImageFileName = dto.FlagImageFileName;
        language.Rtl = dto.Rtl;
        language.IsGeoTranslationEnabled = dto.IsGeoTranslationEnabled;
        language.IsActive = dto.IsActive;
        language.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Dil guncellendi");
    }

    public async Task<ServiceResult> DeleteLanguageAsync(int id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null)
            return ServiceResult.Fail("Dil bulunamadi");

        if (language.IsDefault)
            return ServiceResult.Fail("Varsayilan dil silinemez");

        // Iliskili kaynaklari da sil
        var resources = await _context.LocaleStringResources
            .Where(r => r.LanguageId == id)
            .ToListAsync();

        _context.LocaleStringResources.RemoveRange(resources);
        _context.Languages.Remove(language);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok($"Dil ve {resources.Count} kaynak silindi");
    }

    public async Task<ServiceResult> SetDefaultLanguageAsync(int id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null)
            return ServiceResult.Fail("Dil bulunamadi");

        if (!language.IsActive)
            return ServiceResult.Fail("Pasif bir dil varsayilan yapilamaz");

        // Mevcut varsayilani kaldir
        var currentDefault = await _context.Languages.FirstOrDefaultAsync(l => l.IsDefault);
        if (currentDefault != null)
        {
            currentDefault.IsDefault = false;
        }

        language.IsDefault = true;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Varsayilan dil degistirildi");
    }

    public async Task<ServiceResult> ToggleLanguageActiveAsync(int id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null)
            return ServiceResult.Fail("Dil bulunamadi");

        if (language.IsDefault && language.IsActive)
            return ServiceResult.Fail("Varsayilan dil pasif yapilamaz");

        language.IsActive = !language.IsActive;
        await _context.SaveChangesAsync();

        var status = language.IsActive ? "aktif" : "pasif";
        return ServiceResult.Ok($"Dil {status} yapildi");
    }

    #endregion

    #region Localization Resources

    public async Task<LocalizationResourceListDto> GetLocalizationResourcesAsync(int languageId, string? search = null, int page = 1, int pageSize = 50)
    {
        var query = _context.LocaleStringResources
            .Where(r => r.LanguageId == languageId);

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(r =>
                r.ResourceName.ToLower().Contains(search) ||
                r.ResourceValue.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(r => r.ResourceName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AdminLocalizationResourceDto
            {
                Id = r.Id,
                LanguageId = r.LanguageId,
                ResourceName = r.ResourceName,
                ResourceValue = r.ResourceValue
            })
            .ToListAsync();

        return new LocalizationResourceListDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ServiceResult<int>> CreateLocalizationResourceAsync(AdminLocalizationResourceCreateDto dto)
    {
        // Dil mevcut mu?
        var languageExists = await _context.Languages.AnyAsync(l => l.Id == dto.LanguageId);
        if (!languageExists)
            return ServiceResult<int>.Fail("Dil bulunamadi");

        // Bu key zaten var mi?
        if (await _context.LocaleStringResources.AnyAsync(r => r.LanguageId == dto.LanguageId && r.ResourceName == dto.ResourceName))
            return ServiceResult<int>.Fail("Bu kaynak adi zaten mevcut");

        var resource = new LocaleStringResource
        {
            LanguageId = dto.LanguageId,
            ResourceName = dto.ResourceName,
            ResourceValue = dto.ResourceValue
        };

        _context.LocaleStringResources.Add(resource);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Ok(resource.Id, "Kaynak olusturuldu");
    }

    public async Task<ServiceResult> UpdateLocalizationResourceAsync(int id, AdminLocalizationResourceUpdateDto dto)
    {
        var resource = await _context.LocaleStringResources.FindAsync(id);
        if (resource == null)
            return ServiceResult.Fail("Kaynak bulunamadi");

        resource.ResourceValue = dto.ResourceValue;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Kaynak guncellendi");
    }

    public async Task<ServiceResult> DeleteLocalizationResourceAsync(int id)
    {
        var resource = await _context.LocaleStringResources.FindAsync(id);
        if (resource == null)
            return ServiceResult.Fail("Kaynak bulunamadi");

        _context.LocaleStringResources.Remove(resource);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Kaynak silindi");
    }

    public async Task<ServiceResult<int>> ImportLocalizationResourcesAsync(int languageId, List<LocalizationResourceImportDto> resources)
    {
        var languageExists = await _context.Languages.AnyAsync(l => l.Id == languageId);
        if (!languageExists)
            return ServiceResult<int>.Fail("Dil bulunamadi");

        var existingKeys = await _context.LocaleStringResources
            .Where(r => r.LanguageId == languageId)
            .Select(r => r.ResourceName)
            .ToListAsync();

        var addedCount = 0;
        var updatedCount = 0;

        foreach (var item in resources)
        {
            if (string.IsNullOrEmpty(item.ResourceName))
                continue;

            var existing = await _context.LocaleStringResources
                .FirstOrDefaultAsync(r => r.LanguageId == languageId && r.ResourceName == item.ResourceName);

            if (existing != null)
            {
                existing.ResourceValue = item.ResourceValue;
                updatedCount++;
            }
            else
            {
                _context.LocaleStringResources.Add(new LocaleStringResource
                {
                    LanguageId = languageId,
                    ResourceName = item.ResourceName,
                    ResourceValue = item.ResourceValue
                });
                addedCount++;
            }
        }

        await _context.SaveChangesAsync();

        return ServiceResult<int>.Ok(addedCount, $"{addedCount} kaynak eklendi, {updatedCount} kaynak guncellendi");
    }

    public async Task<List<LocalizationResourceExportDto>> ExportLocalizationResourcesAsync(int languageId)
    {
        return await _context.LocaleStringResources
            .Where(r => r.LanguageId == languageId)
            .OrderBy(r => r.ResourceName)
            .Select(r => new LocalizationResourceExportDto
            {
                ResourceName = r.ResourceName,
                ResourceValue = r.ResourceValue
            })
            .ToListAsync();
    }

    public async Task<ServiceResult<int>> ImportLocalizationResourcesFromFileAsync(int languageId)
    {
        var language = await _context.Languages.FindAsync(languageId);
        if (language == null)
            return ServiceResult<int>.Fail("Dil bulunamadi");

        // wwwroot/data/localization/resources.{seoCode}.xml dosyasini bul
        var fileName = $"resources.{language.UniqueSeoCode}.xml";
        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "localization", fileName);

        if (!File.Exists(filePath))
            return ServiceResult<int>.Fail($"Dosya bulunamadi: {fileName}");

        try
        {
            var xmlContent = await File.ReadAllTextAsync(filePath);
            var xDoc = XDocument.Parse(xmlContent);

            var resources = xDoc.Descendants("resource")
                .Select(x => new LocalizationResourceImportDto
                {
                    ResourceName = x.Attribute("name")?.Value ?? "",
                    ResourceValue = x.Value
                })
                .Where(r => !string.IsNullOrEmpty(r.ResourceName))
                .ToList();

            if (resources.Count == 0)
                return ServiceResult<int>.Fail("XML dosyasinda kaynak bulunamadi");

            // Import islemi
            return await ImportLocalizationResourcesAsync(languageId, resources);
        }
        catch (Exception ex)
        {
            return ServiceResult<int>.Fail($"XML okuma hatasi: {ex.Message}");
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Module tree yapisi olusturur (recursive)
    /// </summary>
    private List<AdminModuleDto> BuildModuleTree(List<AdminModuleDto> modules, int? parentId)
    {
        return modules
            .Where(m => m.ParentId == parentId)
            .Select(m =>
            {
                m.Children = BuildModuleTree(modules, m.Id);
                return m;
            })
            .ToList();
    }

    /// <summary>
    /// PlatformModule icin level hesaplar
    /// </summary>
    private int GetPlatformModuleLevel(List<PlatformModule> modules, PlatformModule module)
    {
        int level = 0;
        var current = module;
        while (current.ParentId != null)
        {
            level++;
            current = modules.FirstOrDefault(m => m.Id == current.ParentId);
            if (current == null) break;
        }
        return level;
    }

    /// <summary>
    /// Alt modul ID'lerini recursive olarak toplar
    /// </summary>
    private async Task CollectChildModuleIds(int parentId, List<int> ids)
    {
        var children = await _context.PlatformModules
            .Where(m => m.ParentId == parentId)
            .Select(m => m.Id)
            .ToListAsync();

        foreach (var childId in children)
        {
            ids.Add(childId);
            await CollectChildModuleIds(childId, ids);
        }
    }

    /// <summary>
    /// Alt modul ID'lerini recursive olarak toplar (IgnoreQueryFilters ile)
    /// </summary>
    private async Task CollectChildModuleIdsIgnoreFilter(int parentId, List<int> ids)
    {
        var children = await _context.PlatformModules
            .IgnoreQueryFilters()
            .Where(m => m.ParentId == parentId)
            .Select(m => m.Id)
            .ToListAsync();

        foreach (var childId in children)
        {
            ids.Add(childId);
            await CollectChildModuleIdsIgnoreFilter(childId, ids);
        }
    }

    /// <summary>
    /// Bir modulun baska bir modulun alt modulu olup olmadigini kontrol eder
    /// </summary>
    private async Task<bool> IsDescendantOfModule(int moduleId, int potentialParentId)
    {
        var descendants = new List<int>();
        await CollectChildModuleIds(potentialParentId, descendants);
        return descendants.Contains(moduleId);
    }

    /// <summary>
    /// Capability modul secimi icin tree yapisi olusturur
    /// </summary>
    private List<CapabilityModuleSelectionDto> BuildCapabilityModuleTree(List<CapabilityModuleSelectionDto> modules, int? parentId)
    {
        return modules
            .Where(m => m.ParentId == parentId)
            .Select(m =>
            {
                m.Children = BuildCapabilityModuleTree(modules, m.Id);
                return m;
            })
            .ToList();
    }

    #endregion

    #region Product Categories (Global - Admin yonetimli)

    public async Task<List<AdminCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _context.ProductCategories
            .Where(c => !c.IsDeleted)
            .Include(c => c.Parent)
            .Include(c => c.Children.Where(ch => !ch.IsDeleted))
            .Include(c => c.Products.Where(p => !p.IsDeleted))
            .OrderBy(c => c.Level)
            .ThenBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new AdminCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                DisplayName = c.DisplayName,
                NameResourceKey = c.NameResourceKey,
                Description = c.Description,
                DescriptionResourceKey = c.DescriptionResourceKey,
                Icon = c.Icon,
                ImageUrl = c.ImageUrl,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                DisplayOrder = c.DisplayOrder,
                Level = c.Level,
                IsActive = c.IsActive,
                Slug = c.Slug,
                MetaTitle = c.MetaTitle,
                MetaDescription = c.MetaDescription,
                ProductCount = c.Products.Count(p => !p.IsDeleted),
                ChildCount = c.Children.Count(ch => !ch.IsDeleted),
                // LocalizedName daha sonra set edilecek
                LocalizedName = c.DisplayName ?? c.Name
            })
            .ToListAsync();

        // Hiyerarsik yapi olustur (root kategoriler)
        return BuildCategoryTree(categories, null);
    }

    public async Task<AdminCategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _context.ProductCategories
            .Where(c => c.Id == id && !c.IsDeleted)
            .Include(c => c.Parent)
            .Include(c => c.Children.Where(ch => !ch.IsDeleted))
            .Include(c => c.Products.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync();

        if (category == null) return null;

        return new AdminCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            DisplayName = category.DisplayName,
            NameResourceKey = category.NameResourceKey,
            Description = category.Description,
            DescriptionResourceKey = category.DescriptionResourceKey,
            Icon = category.Icon,
            ImageUrl = category.ImageUrl,
            ParentId = category.ParentId,
            ParentName = category.Parent?.Name,
            DisplayOrder = category.DisplayOrder,
            Level = category.Level,
            IsActive = category.IsActive,
            Slug = category.Slug,
            MetaTitle = category.MetaTitle,
            MetaDescription = category.MetaDescription,
            ProductCount = category.Products.Count(p => !p.IsDeleted),
            ChildCount = category.Children.Count(ch => !ch.IsDeleted),
            LocalizedName = category.DisplayName ?? category.Name
        };
    }

    public async Task<ServiceResult<int>> CreateCategoryAsync(AdminCategoryCreateUpdateDto dto)
    {
        // Isim kontrolu
        var exists = await _context.ProductCategories
            .AnyAsync(c => c.Name == dto.Name && !c.IsDeleted);
        if (exists)
            return ServiceResult<int>.Fail("Bu isimde bir kategori zaten var");

        // Slug kontrolu
        var slug = string.IsNullOrEmpty(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug;
        var slugExists = await _context.ProductCategories
            .AnyAsync(c => c.Slug == slug && !c.IsDeleted);
        if (slugExists)
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";

        // Parent level kontrolu
        int level = 0;
        if (dto.ParentId.HasValue)
        {
            var parent = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == dto.ParentId && !c.IsDeleted);
            if (parent == null)
                return ServiceResult<int>.Fail("Ust kategori bulunamadi");
            level = parent.Level + 1;
        }

        var category = new ProductCategory
        {
            Name = dto.Name,
            DisplayName = dto.DisplayName,
            NameResourceKey = dto.NameResourceKey,
            Description = dto.Description,
            DescriptionResourceKey = dto.DescriptionResourceKey,
            Icon = dto.Icon,
            ImageUrl = dto.ImageUrl,
            ParentId = dto.ParentId,
            DisplayOrder = dto.DisplayOrder,
            Level = level,
            IsActive = dto.IsActive,
            Slug = slug,
            MetaTitle = dto.MetaTitle,
            MetaDescription = dto.MetaDescription
        };

        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Ok(category.Id, "Kategori basariyla olusturuldu");
    }

    public async Task<ServiceResult> UpdateCategoryAsync(int id, AdminCategoryCreateUpdateDto dto)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (category == null)
            return ServiceResult.Fail("Kategori bulunamadi");

        // Isim kontrolu (kendisi haric)
        var nameExists = await _context.ProductCategories
            .AnyAsync(c => c.Name == dto.Name && c.Id != id && !c.IsDeleted);
        if (nameExists)
            return ServiceResult.Fail("Bu isimde bir kategori zaten var");

        // Slug kontrolu
        var slug = string.IsNullOrEmpty(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug;
        var slugExists = await _context.ProductCategories
            .AnyAsync(c => c.Slug == slug && c.Id != id && !c.IsDeleted);
        if (slugExists)
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";

        // Parent degisiyorsa level'i guncelle
        int level = 0;
        if (dto.ParentId.HasValue)
        {
            // Kendisini parent olarak secemez
            if (dto.ParentId == id)
                return ServiceResult.Fail("Kategori kendisinin alt kategorisi olamaz");

            var parent = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == dto.ParentId && !c.IsDeleted);
            if (parent == null)
                return ServiceResult.Fail("Ust kategori bulunamadi");

            // Parent, kendi alt kategorisi olamaz
            if (await IsDescendantOf(dto.ParentId.Value, id))
                return ServiceResult.Fail("Alt kategori ust kategori olarak secilemez");

            level = parent.Level + 1;
        }

        category.Name = dto.Name;
        category.DisplayName = dto.DisplayName;
        category.NameResourceKey = dto.NameResourceKey;
        category.Description = dto.Description;
        category.DescriptionResourceKey = dto.DescriptionResourceKey;
        category.Icon = dto.Icon;
        category.ImageUrl = dto.ImageUrl;
        category.ParentId = dto.ParentId;
        category.DisplayOrder = dto.DisplayOrder;
        category.Level = level;
        category.IsActive = dto.IsActive;
        category.Slug = slug;
        category.MetaTitle = dto.MetaTitle;
        category.MetaDescription = dto.MetaDescription;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Alt kategorilerin level'ini guncelle
        await UpdateChildrenLevelsAsync(id, level + 1);

        return ServiceResult.Ok("Kategori basariyla guncellendi");
    }

    public async Task<ServiceResult> DeleteCategoryAsync(int id, int? targetCategoryId = null, string? deletedBy = null)
    {
        var category = await _context.ProductCategories
            .Include(c => c.Children)
            .Include(c => c.Products.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (category == null)
            return ServiceResult.Fail("Kategori bulunamadi");

        // Alt kategori kontrolu
        var hasActiveChildren = category.Children.Any(c => !c.IsDeleted);
        if (hasActiveChildren)
            return ServiceResult.Fail("Bu kategorinin alt kategorileri var, once onlari silin");

        // Urun kontrolu
        var activeProductCount = category.Products.Count;
        if (activeProductCount > 0)
        {
            // Eger hedef kategori belirtilmemisse, urun sayisini dondurup kullanicidan sor
            if (!targetCategoryId.HasValue)
            {
                return ServiceResult.Fail($"PRODUCTS_EXIST:{activeProductCount}");
            }

            // Hedef kategori kontrolu
            if (targetCategoryId.Value == id)
                return ServiceResult.Fail("Urunler ayni kategoriye tasinamaz");

            var targetCategory = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == targetCategoryId.Value && !c.IsDeleted);

            if (targetCategory == null)
                return ServiceResult.Fail("Hedef kategori bulunamadi");

            // Tasinan urun ID'lerini kaydet (geri alma icin)
            var productIds = category.Products.Select(p => p.Id).ToList();
            category.MigratedProductIds = System.Text.Json.JsonSerializer.Serialize(productIds);
            category.MigratedProductCount = productIds.Count;
            category.MigratedToCategoryId = targetCategoryId.Value;

            // Urunleri hedef kategoriye tasi
            foreach (var product in category.Products)
            {
                product.CategoryId = targetCategoryId.Value;
                product.UpdatedAt = DateTime.UtcNow;
            }
        }

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        category.DeletedBy = deletedBy;
        category.DeletionStatus = CategoryDeletionStatuses.Pending.Id;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var message = activeProductCount > 0
            ? $"Kategori silindi, {activeProductCount} urun yeni kategoriye tasindi. Onay bekleniyor."
            : "Kategori basariyla silindi. Onay bekleniyor.";

        return ServiceResult.Ok(message);
    }

    public async Task<ServiceResult> SeedCategoriesAsync()
    {
        // Mevcut kategorileri kontrol et
        var existingCount = await _context.ProductCategories
            .CountAsync(c => !c.IsDeleted);

        if (existingCount > 0)
            return ServiceResult.Fail($"Zaten {existingCount} kategori mevcut. Once mevcut kategorileri silin.");

        // JSON dosyasini oku
        var jsonPath = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data", "Category", "categories-seed.json");

        if (!File.Exists(jsonPath))
            return ServiceResult.Fail("Seed dosyasi bulunamadi: categories-seed.json");

        try
        {
            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var seedData = System.Text.Json.JsonSerializer.Deserialize<CategorySeedRoot>(jsonContent, options);
            if (seedData?.Categories == null || !seedData.Categories.Any())
                return ServiceResult.Fail("JSON dosyasi bos veya hatali");

            var createdCount = 0;
            var categoryMap = new Dictionary<string, int>(); // name -> id mapping

            foreach (var rootCategory in seedData.Categories)
            {
                // Root kategoriyi olustur
                var rootEntity = new ProductCategory
                {
                    Name = rootCategory.Name,
                    Icon = rootCategory.Icon,
                    DisplayOrder = rootCategory.DisplayOrder,
                    IsActive = true,
                    Slug = GenerateSlug(rootCategory.Name)
                };

                _context.ProductCategories.Add(rootEntity);
                await _context.SaveChangesAsync();
                categoryMap[rootCategory.Name] = rootEntity.Id;
                createdCount++;

                // Child kategorileri isle
                if (rootCategory.Children != null && rootCategory.Children.Any())
                {
                    // Once parent'i olmayan child'lari isle
                    var directChildren = rootCategory.Children.Where(c => string.IsNullOrEmpty(c.Parent)).ToList();
                    foreach (var child in directChildren)
                    {
                        var childEntity = new ProductCategory
                        {
                            Name = child.Name,
                            Icon = child.Icon,
                            DisplayOrder = child.DisplayOrder,
                            ParentId = rootEntity.Id,
                            Level = 1,
                            IsActive = true,
                            Slug = GenerateSlug(child.Name)
                        };

                        _context.ProductCategories.Add(childEntity);
                        await _context.SaveChangesAsync();
                        categoryMap[child.Name] = childEntity.Id;
                        createdCount++;
                    }

                    // Sonra parent'i olan child'lari isle (nested children)
                    var nestedChildren = rootCategory.Children.Where(c => !string.IsNullOrEmpty(c.Parent)).ToList();
                    foreach (var child in nestedChildren)
                    {
                        if (categoryMap.TryGetValue(child.Parent!, out var parentId))
                        {
                            var childEntity = new ProductCategory
                            {
                                Name = child.Name,
                                Icon = child.Icon,
                                DisplayOrder = child.DisplayOrder,
                                ParentId = parentId,
                                Level = 2,
                                IsActive = true,
                                Slug = GenerateSlug(child.Name)
                            };

                            _context.ProductCategories.Add(childEntity);
                            await _context.SaveChangesAsync();
                            categoryMap[child.Name] = childEntity.Id;
                            createdCount++;
                        }
                    }
                }
            }

            return ServiceResult.Ok($"{createdCount} kategori basariyla yuklendi");
        }
        catch (System.Text.Json.JsonException ex)
        {
            return ServiceResult.Fail($"JSON parse hatasi: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Beklenmeyen hata: {ex.Message}");
        }
    }

    // === Category Trash Bin ===

    public async Task<List<AdminCategoryDto>> GetDeletedCategoriesAsync()
    {
        var deletedCategories = await _context.ProductCategories
            .IgnoreQueryFilters()
            .Include(c => c.Parent)
            .Where(c => c.IsDeleted && (c.DeletionStatus == CategoryDeletionStatuses.Approved.Id || c.DeletionStatus == null))
            .OrderByDescending(c => c.DeletedAt ?? c.UpdatedAt)
            .Select(c => new AdminCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                DisplayName = c.DisplayName,
                NameResourceKey = c.NameResourceKey,
                Description = c.Description,
                DescriptionResourceKey = c.DescriptionResourceKey,
                Icon = c.Icon,
                ImageUrl = c.ImageUrl,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                DisplayOrder = c.DisplayOrder,
                Level = c.Level,
                IsActive = c.IsActive,
                Slug = c.Slug,
                MetaTitle = c.MetaTitle,
                MetaDescription = c.MetaDescription,
                ProductCount = c.Products.Count(p => !p.IsDeleted),
                DeletedAt = c.DeletedAt,
                DeletedBy = c.DeletedBy,
                MigratedProductCount = c.MigratedProductCount,
                MigratedToCategoryId = c.MigratedToCategoryId,
                DeletionStatus = c.DeletionStatus,
                ReviewedBy = c.ReviewedBy,
                ReviewedAt = c.ReviewedAt,
                ReviewNote = c.ReviewNote,
                LocalizedName = c.DisplayName ?? c.Name
            })
            .ToListAsync();

        // Hedef kategori isimlerini getir
        var targetCategoryIds = deletedCategories
            .Where(c => c.MigratedToCategoryId.HasValue)
            .Select(c => c.MigratedToCategoryId!.Value)
            .Distinct()
            .ToList();

        if (targetCategoryIds.Any())
        {
            var targetCategories = await _context.ProductCategories
                .Where(c => targetCategoryIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Name, c.DisplayName })
                .ToListAsync();

            foreach (var cat in deletedCategories.Where(c => c.MigratedToCategoryId.HasValue))
            {
                var target = targetCategories.FirstOrDefault(t => t.Id == cat.MigratedToCategoryId);
                cat.MigratedToCategoryName = target?.DisplayName ?? target?.Name;
            }
        }

        return deletedCategories;
    }

    public async Task<List<AdminCategoryDto>> GetPendingDeletionCategoriesAsync()
    {
        var pendingCategories = await _context.ProductCategories
            .IgnoreQueryFilters()
            .Include(c => c.Parent)
            .Where(c => c.IsDeleted && c.DeletionStatus == CategoryDeletionStatuses.Pending.Id)
            .OrderByDescending(c => c.DeletedAt ?? c.UpdatedAt)
            .Select(c => new AdminCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                DisplayName = c.DisplayName,
                NameResourceKey = c.NameResourceKey,
                Description = c.Description,
                DescriptionResourceKey = c.DescriptionResourceKey,
                Icon = c.Icon,
                ImageUrl = c.ImageUrl,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                DisplayOrder = c.DisplayOrder,
                Level = c.Level,
                IsActive = c.IsActive,
                Slug = c.Slug,
                MetaTitle = c.MetaTitle,
                MetaDescription = c.MetaDescription,
                ProductCount = c.Products.Count(p => !p.IsDeleted),
                DeletedAt = c.DeletedAt,
                DeletedBy = c.DeletedBy,
                MigratedProductCount = c.MigratedProductCount,
                MigratedToCategoryId = c.MigratedToCategoryId,
                DeletionStatus = c.DeletionStatus,
                LocalizedName = c.DisplayName ?? c.Name
            })
            .ToListAsync();

        // Hedef kategori isimlerini getir
        var targetCategoryIds = pendingCategories
            .Where(c => c.MigratedToCategoryId.HasValue)
            .Select(c => c.MigratedToCategoryId!.Value)
            .Distinct()
            .ToList();

        if (targetCategoryIds.Any())
        {
            var targetCategories = await _context.ProductCategories
                .Where(c => targetCategoryIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Name, c.DisplayName })
                .ToListAsync();

            foreach (var cat in pendingCategories.Where(c => c.MigratedToCategoryId.HasValue))
            {
                var target = targetCategories.FirstOrDefault(t => t.Id == cat.MigratedToCategoryId);
                cat.MigratedToCategoryName = target?.DisplayName ?? target?.Name;
            }
        }

        return pendingCategories;
    }

    public async Task<ServiceResult> RestoreCategoryAsync(int id)
    {
        var category = await _context.ProductCategories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);

        if (category == null)
            return ServiceResult.Fail("Kategori bulunamadi");

        // Eger parent varsa ve parent silinmisse, restore edilemez
        if (category.ParentId.HasValue)
        {
            var parent = await _context.ProductCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == category.ParentId.Value);

            if (parent != null && parent.IsDeleted)
                return ServiceResult.Fail("Ust kategori silinmis durumda. Once ust kategoriyi geri yukleyin.");
        }

        category.IsDeleted = false;
        category.DeletedAt = null;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Kategori basariyla geri yuklendi");
    }

    public async Task<ServiceResult> PermanentDeleteCategoryAsync(int id)
    {
        var category = await _context.ProductCategories
            .IgnoreQueryFilters()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);

        if (category == null)
            return ServiceResult.Fail("Kategori bulunamadi");

        // Urunu varsa kalici silinemez
        if (category.Products.Any())
            return ServiceResult.Fail("Bu kategoride urunler var, kalici silinemez");

        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Kategori kalici olarak silindi");
    }

    public async Task<ServiceResult> ApproveDeletionAsync(int id, string reviewedBy, string? reviewNote = null)
    {
        var category = await _context.ProductCategories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted && c.DeletionStatus == CategoryDeletionStatuses.Pending.Id);

        if (category == null)
            return ServiceResult.Fail("Onay bekleyen kategori bulunamadi");

        // Silen kisi onaylayamaz
        if (category.DeletedBy == reviewedBy)
            return ServiceResult.Fail("Silme islemini yapan kisi onay veremez");

        category.DeletionStatus = CategoryDeletionStatuses.Approved.Id;
        category.ReviewedBy = reviewedBy;
        category.ReviewedAt = DateTime.UtcNow;
        category.ReviewNote = reviewNote;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Silme islemi onaylandi");
    }

    public async Task<ServiceResult> RejectDeletionAsync(int id, string reviewedBy, string? reviewNote = null)
    {
        var category = await _context.ProductCategories
            .IgnoreQueryFilters()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted && c.DeletionStatus == CategoryDeletionStatuses.Pending.Id);

        if (category == null)
            return ServiceResult.Fail("Onay bekleyen kategori bulunamadi");

        // Silen kisi reddedemez
        if (category.DeletedBy == reviewedBy)
            return ServiceResult.Fail("Silme islemini yapan kisi red veremez");

        // Eger urunler tasinmissa, geri al
        if (!string.IsNullOrEmpty(category.MigratedProductIds) && category.MigratedToCategoryId.HasValue)
        {
            var productIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(category.MigratedProductIds);
            if (productIds != null && productIds.Any())
            {
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                foreach (var product in products)
                {
                    product.CategoryId = category.Id;
                    product.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        // Kategoriyi geri yukle
        category.IsDeleted = false;
        category.DeletedAt = null;
        category.DeletedBy = null;
        category.DeletionStatus = CategoryDeletionStatuses.Rejected.Id;
        category.ReviewedBy = reviewedBy;
        category.ReviewedAt = DateTime.UtcNow;
        category.ReviewNote = reviewNote;
        category.MigratedProductIds = null;
        category.MigratedProductCount = null;
        category.MigratedToCategoryId = null;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Silme islemi reddedildi, kategori geri yuklendi");
    }

    // === Helper Methods ===

    private List<AdminCategoryDto> BuildCategoryTree(List<AdminCategoryDto> categories, int? parentId)
    {
        return categories
            .Where(c => c.ParentId == parentId)
            .Select(c =>
            {
                c.Children = BuildCategoryTree(categories, c.Id);
                return c;
            })
            .ToList();
    }

    private async Task<bool> IsDescendantOf(int categoryId, int potentialParentId)
    {
        var descendants = await GetAllDescendantIds(potentialParentId);
        return descendants.Contains(categoryId);
    }

    private async Task<List<int>> GetAllDescendantIds(int categoryId)
    {
        var descendants = new List<int>();
        var children = await _context.ProductCategories
            .Where(c => c.ParentId == categoryId && !c.IsDeleted)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var childId in children)
        {
            descendants.Add(childId);
            descendants.AddRange(await GetAllDescendantIds(childId));
        }

        return descendants;
    }

    private async Task UpdateChildrenLevelsAsync(int parentId, int newLevel)
    {
        var children = await _context.ProductCategories
            .Where(c => c.ParentId == parentId && !c.IsDeleted)
            .ToListAsync();

        foreach (var child in children)
        {
            child.Level = newLevel;
            await UpdateChildrenLevelsAsync(child.Id, newLevel + 1);
        }

        await _context.SaveChangesAsync();
    }

    private string GenerateSlug(string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("\"", "");

        // Turkce karakterleri cevir
        slug = slug
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c");

        // Sadece alfanumerik ve tire birak
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Ardisik tireleri tek tireye indir
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

        return slug.Trim('-');
    }

    #endregion

    #region Geography (Countries & States)

    public async Task<List<AdminCountryDto>> GetCountriesAsync(string? search = null)
    {
        var query = _context.Countries.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Iso2Code.ToLower().Contains(search) ||
                (c.Iso3Code != null && c.Iso3Code.ToLower().Contains(search)) ||
                (c.PhoneCode != null && c.PhoneCode.Contains(search)));
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new AdminCountryDto
            {
                Id = c.Id,
                Name = c.Name,
                Iso2Code = c.Iso2Code,
                Iso3Code = c.Iso3Code,
                PhoneCode = c.PhoneCode,
                CurrencyCode = c.CurrencyCode,
                FlagEmoji = c.FlagEmoji,
                IsActive = c.IsActive,
                StateCount = c.States.Count
            })
            .ToListAsync();
    }

    public async Task<AdminCountryDto?> GetCountryByIdAsync(int id)
    {
        return await _context.Countries
            .Where(c => c.Id == id)
            .Select(c => new AdminCountryDto
            {
                Id = c.Id,
                Name = c.Name,
                Iso2Code = c.Iso2Code,
                Iso3Code = c.Iso3Code,
                PhoneCode = c.PhoneCode,
                CurrencyCode = c.CurrencyCode,
                FlagEmoji = c.FlagEmoji,
                IsActive = c.IsActive,
                StateCount = c.States.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<int>> CreateCountryAsync(AdminCountryCreateUpdateDto dto)
    {
        // Iso2Code benzersiz olmali
        if (await _context.Countries.AnyAsync(c => c.Iso2Code == dto.Iso2Code))
            return ServiceResult<int>.Fail("Bu ISO2 kodu zaten kullaniliyor");

        // Isim benzersiz olmali
        if (await _context.Countries.AnyAsync(c => c.Name == dto.Name))
            return ServiceResult<int>.Fail("Bu ulke adi zaten kullaniliyor");

        var country = new Country
        {
            Name = dto.Name,
            Iso2Code = dto.Iso2Code.ToUpperInvariant(),
            Iso3Code = dto.Iso3Code.ToUpperInvariant(),
            PhoneCode = dto.PhoneCode,
            CurrencyCode = dto.CurrencyCode?.ToUpperInvariant(),
            FlagEmoji = dto.FlagEmoji,
            IsActive = dto.IsActive
        };

        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Ok(country.Id, "Ulke olusturuldu");
    }

    public async Task<ServiceResult> UpdateCountryAsync(int id, AdminCountryCreateUpdateDto dto)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country == null)
            return ServiceResult.Fail("Ulke bulunamadi");

        // Iso2Code degistiyse benzersiz mi kontrol et
        if (country.Iso2Code != dto.Iso2Code && await _context.Countries.AnyAsync(c => c.Iso2Code == dto.Iso2Code && c.Id != id))
            return ServiceResult.Fail("Bu ISO2 kodu zaten kullaniliyor");

        // Isim degistiyse benzersiz mi kontrol et
        if (country.Name != dto.Name && await _context.Countries.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            return ServiceResult.Fail("Bu ulke adi zaten kullaniliyor");

        country.Name = dto.Name;
        country.Iso2Code = dto.Iso2Code.ToUpperInvariant();
        country.Iso3Code = dto.Iso3Code.ToUpperInvariant();
        country.PhoneCode = dto.PhoneCode;
        country.CurrencyCode = dto.CurrencyCode?.ToUpperInvariant();
        country.FlagEmoji = dto.FlagEmoji;
        country.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Ulke guncellendi");
    }

    public async Task<ServiceResult> DeleteCountryAsync(int id)
    {
        var country = await _context.Countries
            .Include(c => c.States)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (country == null)
            return ServiceResult.Fail("Ulke bulunamadi");

        // Eyalet/il varsa silinemesin
        if (country.States.Any())
            return ServiceResult.Fail($"Bu ulkenin {country.States.Count} eyaleti/ili var, once onlari silin");

        // Adres kullanimini kontrol et
        var addressUsed = await _context.Addresses.AnyAsync(a => a.CountryId == id);
        if (addressUsed)
            return ServiceResult.Fail("Bu ulke adreslerde kullaniliyor, silinemez");

        _context.Countries.Remove(country);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Ulke silindi");
    }

    public async Task<List<AdminStateDto>> GetStatesAsync(int? countryId = null, string? search = null)
    {
        var query = _context.States
            .Include(s => s.Country)
            .AsQueryable();

        if (countryId.HasValue)
        {
            query = query.Where(s => s.CountryId == countryId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(search) ||
                (s.Code != null && s.Code.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(s => s.Country.Name)
            .ThenBy(s => s.Name)
            .Select(s => new AdminStateDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                CountryId = s.CountryId,
                CountryName = s.Country.Name,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<AdminStateDto?> GetStateByIdAsync(int id)
    {
        return await _context.States
            .Include(s => s.Country)
            .Where(s => s.Id == id)
            .Select(s => new AdminStateDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                CountryId = s.CountryId,
                CountryName = s.Country.Name,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<int>> CreateStateAsync(AdminStateCreateUpdateDto dto)
    {
        // Ulke var mi kontrol et
        var countryExists = await _context.Countries.AnyAsync(c => c.Id == dto.CountryId);
        if (!countryExists)
            return ServiceResult<int>.Fail("Ulke bulunamadi");

        // Ayni ulkede ayni isim var mi
        if (await _context.States.AnyAsync(s => s.CountryId == dto.CountryId && s.Name == dto.Name))
            return ServiceResult<int>.Fail("Bu ulkede bu isimde bir eyalet/il zaten var");

        var state = new State
        {
            Name = dto.Name,
            Code = dto.Code?.ToUpperInvariant(),
            CountryId = dto.CountryId,
            IsActive = dto.IsActive
        };

        _context.States.Add(state);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Ok(state.Id, "Eyalet/il olusturuldu");
    }

    public async Task<ServiceResult> UpdateStateAsync(int id, AdminStateCreateUpdateDto dto)
    {
        var state = await _context.States.FindAsync(id);
        if (state == null)
            return ServiceResult.Fail("Eyalet/il bulunamadi");

        // Ulke var mi kontrol et
        var countryExists = await _context.Countries.AnyAsync(c => c.Id == dto.CountryId);
        if (!countryExists)
            return ServiceResult.Fail("Ulke bulunamadi");

        // Ayni ulkede ayni isim var mi (kendisi haric)
        if (await _context.States.AnyAsync(s => s.CountryId == dto.CountryId && s.Name == dto.Name && s.Id != id))
            return ServiceResult.Fail("Bu ulkede bu isimde bir eyalet/il zaten var");

        state.Name = dto.Name;
        state.Code = dto.Code?.ToUpperInvariant();
        state.CountryId = dto.CountryId;
        state.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Eyalet/il guncellendi");
    }

    public async Task<ServiceResult> DeleteStateAsync(int id)
    {
        var state = await _context.States.FindAsync(id);
        if (state == null)
            return ServiceResult.Fail("Eyalet/il bulunamadi");

        // Adres kullanimini kontrol et
        var addressUsed = await _context.Addresses.AnyAsync(a => a.StateId == id);
        if (addressUsed)
            return ServiceResult.Fail("Bu eyalet/il adreslerde kullaniliyor, silinemez");

        _context.States.Remove(state);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Eyalet/il silindi");
    }

    // === Country Translations ===

    public async Task<AdminCountryDetailDto?> GetCountryDetailAsync(int id)
    {
        var country = await _context.Countries
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (country == null) return null;

        // Aktif dilleri al
        var languages = await _context.Languages
            .Where(l => l.IsActive)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();

        var translations = new List<AdminCountryTranslationDto>();

        foreach (var lang in languages)
        {
            var existing = country.Translations.FirstOrDefault(t => t.LanguageCode == lang.LanguageCulture);
            translations.Add(new AdminCountryTranslationDto
            {
                Id = existing?.Id ?? 0,
                LanguageCode = lang.LanguageCulture,
                LanguageName = lang.Name,
                Name = existing?.Name ?? country.Name,
                OfficialName = existing?.OfficialName ?? country.OfficialName
            });
        }

        return new AdminCountryDetailDto
        {
            Id = country.Id,
            Name = country.Name,
            OfficialName = country.OfficialName,
            Iso2Code = country.Iso2Code,
            Iso3Code = country.Iso3Code,
            PhoneCode = country.PhoneCode,
            CurrencyCode = country.CurrencyCode,
            FlagEmoji = country.FlagEmoji,
            Translations = translations
        };
    }

    public async Task<ServiceResult> UpdateCountryTranslationsAsync(int countryId, List<AdminCountryTranslationUpdateDto> translations)
    {
        var country = await _context.Countries
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == countryId);

        if (country == null)
            return ServiceResult.Fail("Ulke bulunamadi");

        foreach (var dto in translations)
        {
            var existing = country.Translations.FirstOrDefault(t => t.LanguageCode == dto.LanguageCode);

            if (existing != null)
            {
                existing.Name = dto.Name;
                existing.OfficialName = dto.OfficialName;
            }
            else
            {
                country.Translations.Add(new CountryTranslation
                {
                    CountryId = countryId,
                    LanguageCode = dto.LanguageCode,
                    Name = dto.Name,
                    OfficialName = dto.OfficialName
                });
            }
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Ceviriler guncellendi");
    }

    public async Task<ServiceResult> ToggleCountryActiveAsync(int id)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country == null)
            return ServiceResult.Fail("Ulke bulunamadi");

        country.IsActive = !country.IsActive;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok(country.IsActive ? "Ulke aktif edildi" : "Ulke pasif edildi");
    }

    // === State Translations & Status ===

    public async Task<AdminStateDetailDto?> GetStateDetailAsync(int id)
    {
        var state = await _context.States
            .Include(s => s.Country)
            .Include(s => s.Translations)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (state == null) return null;

        var languages = await _context.Languages
            .Where(l => l.IsActive)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();

        var translations = new List<AdminStateTranslationDto>();

        foreach (var lang in languages)
        {
            var existing = state.Translations.FirstOrDefault(t => t.LanguageCode == lang.LanguageCulture);
            translations.Add(new AdminStateTranslationDto
            {
                Id = existing?.Id ?? 0,
                LanguageCode = lang.LanguageCulture,
                LanguageName = lang.Name,
                Name = existing?.Name ?? state.Name
            });
        }

        return new AdminStateDetailDto
        {
            Id = state.Id,
            CountryId = state.CountryId,
            CountryName = state.Country.Name,
            Name = state.Name,
            Code = state.Code,
            IsActive = state.IsActive,
            Translations = translations
        };
    }

    public async Task<ServiceResult> UpdateStateTranslationsAsync(int stateId, List<AdminStateTranslationUpdateDto> translations)
    {
        var state = await _context.States
            .Include(s => s.Translations)
            .FirstOrDefaultAsync(s => s.Id == stateId);

        if (state == null)
            return ServiceResult.Fail("Eyalet/il bulunamadi");

        foreach (var dto in translations)
        {
            var existing = state.Translations.FirstOrDefault(t => t.LanguageCode == dto.LanguageCode);

            if (existing != null)
            {
                existing.Name = dto.Name;
            }
            else
            {
                state.Translations.Add(new StateTranslation
                {
                    StateId = stateId,
                    LanguageCode = dto.LanguageCode,
                    Name = dto.Name
                });
            }
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Ceviriler guncellendi");
    }

    public async Task<ServiceResult> ToggleStateActiveAsync(int id)
    {
        var state = await _context.States.FindAsync(id);
        if (state == null)
            return ServiceResult.Fail("Eyalet/il bulunamadi");

        state.IsActive = !state.IsActive;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok(state.IsActive ? "Eyalet/il aktif edildi" : "Eyalet/il pasif edildi");
    }

    #endregion

    #region Profile Approvals

    public async Task<List<AdminProfileApprovalDto>> GetPendingProfileApprovalsAsync()
    {
        var profiles = await _context.CapabilityProfiles
            .Include(p => p.Vendor)
            .Where(p => p.IsPubliclyVisible && !p.IsVerified && p.RejectionReason == null)
            .OrderByDescending(p => p.PublicationRequestedAt ?? p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.VendorId,
                CompanyName = p.Vendor.CompanyName,
                LogoUrl = p.Vendor.LogoUrl,
                p.CapabilityId,
                p.DisplayName,
                p.Slug,
                p.ShortDescription,
                p.PublicationRequestedAt,
                p.CreatedAt
            })
            .ToListAsync();

        return profiles.Select(p =>
        {
            var cap = Capabilities.GetById(p.CapabilityId);
            return new AdminProfileApprovalDto
            {
                Id = p.Id,
                VendorId = p.VendorId,
                CompanyName = p.CompanyName,
                LogoUrl = p.LogoUrl,
                CapabilityId = p.CapabilityId,
                CapabilityName = cap?.Description ?? "",
                CapabilityIcon = cap?.Icon,
                DisplayName = p.DisplayName,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                PublicationRequestedAt = p.PublicationRequestedAt,
                CreatedAt = p.CreatedAt
            };
        }).ToList();
    }

    public async Task<AdminProfileDetailDto?> GetProfileByIdAsync(int id)
    {
        var profile = await _context.CapabilityProfiles
            .Include(p => p.Vendor)
            .Include(p => p.Country)
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.VendorId,
                CompanyName = p.Vendor.CompanyName,
                LogoUrl = p.Vendor.LogoUrl,
                p.CapabilityId,
                p.Slug,
                p.DisplayName,
                p.Description,
                p.ShortDescription,
                p.Tagline,
                p.Services,
                p.Certifications,
                p.ServiceRegions,
                p.PublicEmail,
                p.PublicPhone,
                p.PublicWebsite,
                CountryName = p.Country != null ? p.Country.Name : null,
                p.City,
                p.Address,
                p.CoverImageUrl,
                p.GalleryImages,
                p.IsPubliclyVisible,
                p.IsVerified,
                p.PublicationRequestedAt,
                p.VerifiedAt,
                p.RejectionReason,
                p.ViewCount,
                p.ContactRequestCount,
                p.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (profile == null) return null;

        var cap = Capabilities.GetById(profile.CapabilityId);
        return new AdminProfileDetailDto
        {
            Id = profile.Id,
            VendorId = profile.VendorId,
            CompanyName = profile.CompanyName,
            LogoUrl = profile.LogoUrl,
            CapabilityId = profile.CapabilityId,
            CapabilityName = cap?.Description ?? "",
            CapabilityIcon = cap?.Icon,
            Slug = profile.Slug,
            DisplayName = profile.DisplayName,
            Description = profile.Description,
            ShortDescription = profile.ShortDescription,
            Tagline = profile.Tagline,
            Services = profile.Services,
            Certifications = profile.Certifications,
            ServiceRegions = profile.ServiceRegions,
            PublicEmail = profile.PublicEmail,
            PublicPhone = profile.PublicPhone,
            PublicWebsite = profile.PublicWebsite,
            CountryName = profile.CountryName,
            City = profile.City,
            Address = profile.Address,
            CoverImageUrl = profile.CoverImageUrl,
            GalleryImages = profile.GalleryImages,
            IsPubliclyVisible = profile.IsPubliclyVisible,
            IsVerified = profile.IsVerified,
            PublicationRequestedAt = profile.PublicationRequestedAt,
            VerifiedAt = profile.VerifiedAt,
            RejectionReason = profile.RejectionReason,
            ViewCount = profile.ViewCount,
            ContactRequestCount = profile.ContactRequestCount,
            CreatedAt = profile.CreatedAt
        };
    }

    public async Task<bool> ApproveProfileAsync(int profileId, int approvedByUserId)
    {
        var profile = await _context.CapabilityProfiles.FindAsync(profileId);
        if (profile == null) return false;

        profile.IsVerified = true;
        profile.VerifiedAt = DateTime.UtcNow;
        profile.VerifiedByUserId = approvedByUserId;
        profile.RejectionReason = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectProfileAsync(int profileId, string reason, int rejectedByUserId)
    {
        var profile = await _context.CapabilityProfiles.FindAsync(profileId);
        if (profile == null) return false;

        profile.IsVerified = false;
        profile.RejectionReason = reason;
        profile.VerifiedByUserId = rejectedByUserId;

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion
}
