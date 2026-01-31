using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Bridgo.Data;
using Bridgo.Models.Entities;

namespace Bridgo.Services;

public class LocalizationService : ILocalizationService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CacheKeyPrefix = "locale_";
    private const string LanguagesCacheKey = "all_languages";
    private const int CacheMinutes = 60;

    public LocalizationService(
        ApplicationDbContext context,
        IMemoryCache cache,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
    }

    #region Dil Islemleri

    public async Task<IEnumerable<Language>> GetAllLanguagesAsync(bool onlyActive = true)
    {
        var cacheKey = $"{LanguagesCacheKey}_{onlyActive}";

        if (!_cache.TryGetValue(cacheKey, out IEnumerable<Language>? languages))
        {
            var query = _context.Languages.AsQueryable();
            if (onlyActive)
                query = query.Where(l => l.IsActive);

            languages = await query.OrderBy(l => l.DisplayOrder).ToListAsync();
            _cache.Set(cacheKey, languages, TimeSpan.FromHours(1));
        }

        return languages!;
    }

    public async Task<Language?> GetLanguageByIdAsync(int id)
    {
        return await _context.Languages.FindAsync(id);
    }

    public async Task<Language?> GetLanguageByCodeAsync(string code)
    {
        return await _context.Languages
            .FirstOrDefaultAsync(l => l.UniqueSeoCode == code || l.LanguageCulture == code);
    }

    public async Task<Language?> GetDefaultLanguageAsync()
    {
        return await _context.Languages.FirstOrDefaultAsync(l => l.IsDefault && l.IsActive)
            ?? await _context.Languages.FirstOrDefaultAsync(l => l.IsActive);
    }

    public async Task<Language> CreateLanguageAsync(Language language)
    {
        if (language.IsDefault)
        {
            var others = await _context.Languages.Where(l => l.IsDefault).ToListAsync();
            foreach (var other in others)
                other.IsDefault = false;
        }

        _context.Languages.Add(language);
        await _context.SaveChangesAsync();
        ClearCache();

        return language;
    }

    public async Task<Language> UpdateLanguageAsync(Language language)
    {
        var existing = await _context.Languages.FindAsync(language.Id);
        if (existing == null)
            throw new Exception("Dil bulunamadi.");

        if (language.IsDefault && !existing.IsDefault)
        {
            var others = await _context.Languages.Where(l => l.IsDefault && l.Id != language.Id).ToListAsync();
            foreach (var other in others)
                other.IsDefault = false;
        }

        existing.Name = language.Name;
        existing.LanguageCulture = language.LanguageCulture;
        existing.UniqueSeoCode = language.UniqueSeoCode;
        existing.FlagImageFileName = language.FlagImageFileName;
        existing.Rtl = language.Rtl;
        existing.IsDefault = language.IsDefault;
        existing.IsActive = language.IsActive;
        existing.DisplayOrder = language.DisplayOrder;

        await _context.SaveChangesAsync();
        ClearCache();

        return existing;
    }

    public async Task DeleteLanguageAsync(int id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null)
            return;

        if (language.IsDefault)
            throw new Exception("Varsayilan dil silinemez.");

        var resources = await _context.LocaleStringResources
            .Where(r => r.LanguageId == id)
            .ToListAsync();
        _context.LocaleStringResources.RemoveRange(resources);

        _context.Languages.Remove(language);
        await _context.SaveChangesAsync();
        ClearCache();
    }

    #endregion

    #region Ceviri Islemleri (Sync)

    public string T(string resourceKey)
    {
        return GetResource(resourceKey, null);
    }

    public string T(string resourceKey, params object[] args)
    {
        var value = GetResource(resourceKey, null);
        try
        {
            return string.Format(value, args);
        }
        catch
        {
            return value;
        }
    }

    public string GetResource(string resourceKey, string? defaultValue = null)
    {
        var languageId = GetCurrentLanguageId();
        return GetResource(resourceKey, languageId, defaultValue);
    }

    public string GetResource(string resourceKey, int languageId, string? defaultValue = null)
    {
        if (languageId == 0)
            return defaultValue ?? resourceKey;

        var cacheKey = $"{CacheKeyPrefix}{languageId}_{resourceKey}";

        if (_cache.TryGetValue(cacheKey, out string? cachedValue) && cachedValue != null)
            return cachedValue;

        var resource = _context.LocaleStringResources
            .AsNoTracking()
            .FirstOrDefault(r => r.LanguageId == languageId && r.ResourceName == resourceKey);

        var value = resource?.ResourceValue ?? defaultValue ?? resourceKey;
        _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CacheMinutes));

        return value;
    }

    #endregion

    #region Ceviri Islemleri (Async)

    public async Task<string> GetResourceAsync(string resourceKey, int? languageId = null, string? defaultValue = null)
    {
        var langId = languageId ?? GetCurrentLanguageId();
        if (langId == 0)
            return defaultValue ?? resourceKey;

        var cacheKey = $"{CacheKeyPrefix}{langId}_{resourceKey}";

        if (_cache.TryGetValue(cacheKey, out string? cachedValue) && cachedValue != null)
            return cachedValue;

        var resource = await _context.LocaleStringResources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.LanguageId == langId && r.ResourceName == resourceKey);

        var value = resource?.ResourceValue ?? defaultValue ?? resourceKey;
        _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CacheMinutes));

        return value;
    }

    public async Task<Dictionary<string, string>> GetAllResourcesAsync(int languageId)
    {
        var cacheKey = $"{CacheKeyPrefix}{languageId}_all";

        if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? cachedResources) && cachedResources != null)
            return cachedResources;

        var resources = await _context.LocaleStringResources
            .AsNoTracking()
            .Where(r => r.LanguageId == languageId)
            .ToDictionaryAsync(r => r.ResourceName, r => r.ResourceValue);

        _cache.Set(cacheKey, resources, TimeSpan.FromMinutes(CacheMinutes));

        return resources;
    }

    public async Task<Dictionary<string, string>> GetResourcesByPrefixAsync(string prefix, int? languageId = null)
    {
        var langId = languageId ?? GetCurrentLanguageId();

        return await _context.LocaleStringResources
            .AsNoTracking()
            .Where(r => r.LanguageId == langId && r.ResourceName.StartsWith(prefix))
            .ToDictionaryAsync(r => r.ResourceName, r => r.ResourceValue);
    }

    #endregion

    #region Kaynak Yonetimi

    public async Task<LocaleStringResource?> GetResourceByNameAsync(string resourceName, int languageId)
    {
        return await _context.LocaleStringResources
            .FirstOrDefaultAsync(r => r.LanguageId == languageId && r.ResourceName == resourceName);
    }

    public async Task<IEnumerable<LocaleStringResource>> GetResourcesByLanguageAsync(int languageId)
    {
        return await _context.LocaleStringResources
            .Where(r => r.LanguageId == languageId)
            .OrderBy(r => r.ResourceName)
            .ToListAsync();
    }

    public async Task<LocaleStringResource> SetResourceAsync(int languageId, string resourceName, string resourceValue)
    {
        var existing = await GetResourceByNameAsync(resourceName, languageId);

        if (existing != null)
        {
            existing.ResourceValue = resourceValue;
        }
        else
        {
            existing = new LocaleStringResource
            {
                LanguageId = languageId,
                ResourceName = resourceName,
                ResourceValue = resourceValue
            };
            _context.LocaleStringResources.Add(existing);
        }

        await _context.SaveChangesAsync();
        ClearLanguageCache(languageId);

        return existing;
    }

    public async Task DeleteResourceAsync(int resourceId)
    {
        var resource = await _context.LocaleStringResources.FindAsync(resourceId);
        if (resource != null)
        {
            var langId = resource.LanguageId;
            _context.LocaleStringResources.Remove(resource);
            await _context.SaveChangesAsync();
            ClearLanguageCache(langId);
        }
    }

    public async Task DeleteResourceByNameAsync(string resourceName, int languageId)
    {
        var resource = await GetResourceByNameAsync(resourceName, languageId);
        if (resource != null)
        {
            _context.LocaleStringResources.Remove(resource);
            await _context.SaveChangesAsync();
            ClearLanguageCache(languageId);
        }
    }

    #endregion

    #region Toplu Islemler

    public async Task ImportResourcesAsync(int languageId, Dictionary<string, string> resources)
    {
        foreach (var kvp in resources)
        {
            await SetResourceAsync(languageId, kvp.Key, kvp.Value);
        }
    }

    public async Task<Dictionary<string, string>> ExportResourcesAsync(int languageId)
    {
        return await GetAllResourcesAsync(languageId);
    }

    #endregion

    #region XML Import/Export

    public async Task<int> ImportFromXmlAsync(int languageId, string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
        var resources = doc.Descendants("LocaleResource");
        int count = 0;

        foreach (var resource in resources)
        {
            var name = resource.Attribute("Name")?.Value;
            var value = resource.Element("Value")?.Value;

            if (!string.IsNullOrEmpty(name) && value != null)
            {
                await SetResourceAsync(languageId, name, value);
                count++;
            }
        }

        ClearLanguageCache(languageId);
        return count;
    }

    public async Task<int> ImportFromXmlFileAsync(int languageId, string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"XML dosyasi bulunamadi: {filePath}");

        var xmlContent = await File.ReadAllTextAsync(filePath);
        return await ImportFromXmlAsync(languageId, xmlContent);
    }

    public async Task<int> ImportFromDefaultXmlAsync(int languageId, string basePath)
    {
        var language = await GetLanguageByIdAsync(languageId);
        if (language == null)
            throw new Exception("Dil bulunamadi.");

        var fileName = $"resources.{language.UniqueSeoCode}.xml";
        var filePath = Path.Combine(basePath, "App_Data", "Localization", fileName);

        return await ImportFromXmlFileAsync(languageId, filePath);
    }

    public async Task<string> ExportToXmlAsync(int languageId)
    {
        var language = await GetLanguageByIdAsync(languageId);
        if (language == null)
            throw new Exception("Dil bulunamadi.");

        var resources = await GetResourcesByLanguageAsync(languageId);

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("Language",
                new XAttribute("Name", language.Name),
                new XAttribute("LanguageCulture", language.LanguageCulture),
                new XAttribute("UniqueSeoCode", language.UniqueSeoCode),
                resources.Select(r =>
                    new XElement("LocaleResource",
                        new XAttribute("Name", r.ResourceName),
                        new XElement("Value", r.ResourceValue)
                    )
                )
            )
        );

        return doc.ToString();
    }

    #endregion

    #region Mevcut Dil

    public int GetCurrentLanguageId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext != null)
        {
            if (httpContext.Request.Cookies.TryGetValue("LanguageId", out var langIdStr)
                && int.TryParse(langIdStr, out var langId))
            {
                return langId;
            }
        }

        var defaultLanguage = _context.Languages
            .AsNoTracking()
            .FirstOrDefault(l => l.IsDefault && l.IsActive);

        return defaultLanguage?.Id ?? 0;
    }

    public string GetCurrentLanguageCode()
    {
        var languageId = GetCurrentLanguageId();
        if (languageId == 0)
            return "tr";

        var language = _context.Languages
            .AsNoTracking()
            .FirstOrDefault(l => l.Id == languageId);

        return language?.UniqueSeoCode ?? "tr";
    }

    public string GetCurrentLanguageCulture()
    {
        var languageId = GetCurrentLanguageId();
        if (languageId == 0)
            return "tr-TR";

        var language = _context.Languages
            .AsNoTracking()
            .FirstOrDefault(l => l.Id == languageId);

        return language?.LanguageCulture ?? "tr-TR";
    }

    public void SetCurrentLanguage(int languageId)
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Append("LanguageId", languageId.ToString(),
            new CookieOptions
            {
                Expires = DateTime.UtcNow.AddYears(1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });
    }

    public void SetCurrentLanguage(string languageCode)
    {
        var lang = _context.Languages
            .FirstOrDefault(l => l.UniqueSeoCode == languageCode || l.LanguageCulture == languageCode);

        if (lang != null)
            SetCurrentLanguage(lang.Id);
    }

    #endregion

    #region Cache

    public void ClearCache()
    {
        // Language cache'leri temizle
        _cache.Remove($"{LanguagesCacheKey}_true");
        _cache.Remove($"{LanguagesCacheKey}_false");
    }

    public void ClearLanguageCache(int languageId)
    {
        _cache.Remove($"{CacheKeyPrefix}{languageId}_all");
        // Not: Bireysel key'leri temizlemek icin IMemoryCache'de dogrudan yol yok
        // Uygulama yeniden baslatilana kadar eski degerler cache'de kalabilir
    }

    #endregion
}
