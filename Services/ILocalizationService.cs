using Bridgo.Models.Entities;

namespace Bridgo.Services;

public interface ILocalizationService
{
    // ========================================
    // Dil Islemleri
    // ========================================
    Task<IEnumerable<Language>> GetAllLanguagesAsync(bool onlyActive = true);
    Task<Language?> GetLanguageByIdAsync(int id);
    Task<Language?> GetLanguageByCodeAsync(string code);
    Task<Language?> GetDefaultLanguageAsync();
    Task<Language> CreateLanguageAsync(Language language);
    Task<Language> UpdateLanguageAsync(Language language);
    Task DeleteLanguageAsync(int id);

    // ========================================
    // Ceviri Islemleri (Sync - View icin)
    // ========================================
    string T(string resourceKey);
    string T(string resourceKey, params object[] args);
    string GetResource(string resourceKey, string? defaultValue = null);
    string GetResource(string resourceKey, int languageId, string? defaultValue = null);

    // ========================================
    // Ceviri Islemleri (Async - API icin)
    // ========================================
    Task<string> GetResourceAsync(string resourceKey, int? languageId = null, string? defaultValue = null);
    Task<Dictionary<string, string>> GetAllResourcesAsync(int languageId);
    Task<Dictionary<string, string>> GetResourcesByPrefixAsync(string prefix, int? languageId = null);

    // ========================================
    // Kaynak Yonetimi
    // ========================================
    Task<LocaleStringResource?> GetResourceByNameAsync(string resourceName, int languageId);
    Task<IEnumerable<LocaleStringResource>> GetResourcesByLanguageAsync(int languageId);
    Task<LocaleStringResource> SetResourceAsync(int languageId, string resourceName, string resourceValue);
    Task DeleteResourceAsync(int resourceId);
    Task DeleteResourceByNameAsync(string resourceName, int languageId);

    // ========================================
    // Toplu Islemler
    // ========================================
    Task ImportResourcesAsync(int languageId, Dictionary<string, string> resources);
    Task<Dictionary<string, string>> ExportResourcesAsync(int languageId);

    // ========================================
    // XML Import/Export
    // ========================================
    Task<int> ImportFromXmlAsync(int languageId, string xmlContent);
    Task<int> ImportFromXmlFileAsync(int languageId, string filePath);
    Task<int> ImportFromDefaultXmlAsync(int languageId, string basePath);
    Task<string> ExportToXmlAsync(int languageId);

    // ========================================
    // Mevcut Dil
    // ========================================
    int GetCurrentLanguageId();
    string GetCurrentLanguageCode();
    string GetCurrentLanguageCulture();
    void SetCurrentLanguage(int languageId);
    void SetCurrentLanguage(string languageCode);

    // ========================================
    // Cache
    // ========================================
    void ClearCache();
    void ClearLanguageCache(int languageId);
}
