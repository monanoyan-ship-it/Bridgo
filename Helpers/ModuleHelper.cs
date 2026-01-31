using Bridgo.Services.Interfaces;

namespace Bridgo.Helpers;

/// <summary>
/// Module islemleri icin yardimci metodlar
/// </summary>
public static class ModuleHelper
{
    /// <summary>
    /// Module listesindeki tum ID'leri ve DisplayNameResourceKey'leri (children dahil) toplar
    /// </summary>
    public static (HashSet<int> Ids, HashSet<string> ResourceKeys) GetAllModuleIdentifiers(List<CapabilityModuleDto> modules)
    {
        var ids = new HashSet<int>();
        var resourceKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in modules)
        {
            CollectModuleIdentifiers(module, ids, resourceKeys);
        }
        return (ids, resourceKeys);
    }

    private static void CollectModuleIdentifiers(CapabilityModuleDto module, HashSet<int> ids, HashSet<string> resourceKeys)
    {
        ids.Add(module.Id);
        if (!string.IsNullOrEmpty(module.DisplayNameResourceKey))
            resourceKeys.Add(module.DisplayNameResourceKey);
        foreach (var child in module.Children)
        {
            CollectModuleIdentifiers(child, ids, resourceKeys);
        }
    }

    /// <summary>
    /// Modulun zaten mevcut olup olmadigini kontrol eder (ID veya DisplayNameResourceKey ile)
    /// Yeni mimaride moduller globaldir (PlatformModules), ID benzersizdir
    /// </summary>
    public static bool IsModuleAlreadyExists(CapabilityModuleDto module, HashSet<int> existingIds, HashSet<string> existingResourceKeys)
    {
        // ID kontrolu (moduller artik global, ID benzersiz)
        if (existingIds.Contains(module.Id))
            return true;

        // DisplayNameResourceKey kontrolu (ayni key'li modul varsa)
        if (!string.IsNullOrEmpty(module.DisplayNameResourceKey) && existingResourceKeys.Contains(module.DisplayNameResourceKey))
            return true;

        return false;
    }

    /// <summary>
    /// Capability modullerini platform modullerine ekler (duplicate kontrolu ile)
    /// </summary>
    public static void MergeModules(List<CapabilityModuleDto> targetModules, List<CapabilityModuleDto> sourceModules)
    {
        var (existingIds, existingResourceKeys) = GetAllModuleIdentifiers(targetModules);
        foreach (var module in sourceModules.Where(m => !IsModuleAlreadyExists(m, existingIds, existingResourceKeys)))
        {
            targetModules.Add(module);
        }
    }
}
