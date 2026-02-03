using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Models.Entities;

namespace Bridgo.Services;

/// <summary>
/// Coğrafi verileri (ülke, eyalet, şehir) harici API'lerden çekip database'e yazan servis
/// </summary>
public class GeographySeedService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<GeographySeedService> _logger;

    // Lokal dosya yollari (wwwroot/data/geography/)
    private const string COUNTRIES_FILE = "data/geography/countries.json";
    private const string STATES_FILE = "data/geography/states.json";

    public GeographySeedService(
        ApplicationDbContext context,
        IWebHostEnvironment env,
        ILogger<GeographySeedService> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Lokal JSON dosyasini okur
    /// </summary>
    private async Task<string> ReadLocalFileAsync(string relativePath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, relativePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"Dosya bulunamadi: {relativePath}. " +
                $"Lutfen https://github.com/dr5hn/countries-states-cities-database/tree/master/json adresinden indirin.",
                fullPath);
        }
        return await File.ReadAllTextAsync(fullPath);
    }

    /// <summary>
    /// Tüm coğrafi verileri seed eder
    /// </summary>
    public async Task<SeedResult> SeedAllAsync(bool forceRefresh = false)
    {
        var result = new SeedResult();

        try
        {
            // 1. Ülkeleri seed et
            _logger.LogInformation("Seeding countries...");
            var countryResult = await SeedCountriesAsync(forceRefresh);
            result.CountriesAdded = countryResult.Added;
            result.CountriesUpdated = countryResult.Updated;

            // 2. Eyaletleri seed et
            _logger.LogInformation("Seeding states...");
            var stateResult = await SeedStatesAsync(forceRefresh);
            result.StatesAdded = stateResult.Added;
            result.StatesUpdated = stateResult.Updated;

            result.Success = true;
            result.Message = $"Seed tamamlandı. Ülkeler: {result.CountriesAdded}/{result.CountriesUpdated}, " +
                           $"Eyaletler: {result.StatesAdded}/{result.StatesUpdated}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Seed işlemi sırasında hata oluştu");
            result.Success = false;
            result.Message = $"Hata: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Ülkeleri GitHub'daki countries.json'dan çeker ve database'e yazar
    /// </summary>
    public async Task<(int Added, int Updated)> SeedCountriesAsync(bool forceRefresh = false)
    {
        int added = 0, updated = 0;

        // Mevcut ülke sayısını kontrol et
        var existingCount = await _context.Countries.CountAsync();
        if (existingCount > 0 && !forceRefresh)
        {
            _logger.LogInformation("Ülkeler zaten mevcut ({Count}), atlanıyor...", existingCount);
            return (0, 0);
        }

        // Lokal dosyadan ülkeleri oku
        _logger.LogInformation("Ülkeler okunuyor: {File}", COUNTRIES_FILE);
        var response = await ReadLocalFileAsync(COUNTRIES_FILE);
        var countriesJson = JsonDocument.Parse(response);

        // Aktif dilleri al
        var activeLanguages = await _context.Languages
            .Where(l => l.IsGeoTranslationEnabled && l.IsActive)
            .Select(l => l.UniqueSeoCode)
            .ToListAsync();

        foreach (var countryElement in countriesJson.RootElement.EnumerateArray())
        {
            try
            {
                var iso2 = countryElement.GetProperty("iso2").GetString() ?? "";
                var iso3 = countryElement.GetProperty("iso3").GetString() ?? "";

                // Mevcut ülkeyi bul veya yeni oluştur
                var country = await _context.Countries
                    .Include(c => c.Translations)
                    .FirstOrDefaultAsync(c => c.Iso2Code == iso2);

                if (country == null)
                {
                    country = new Country { Iso2Code = iso2, Iso3Code = iso3 };
                    _context.Countries.Add(country);
                    added++;
                }
                else
                {
                    updated++;
                }

                // Name
                country.Name = countryElement.GetProperty("name").GetString() ?? "";
                country.Iso3Code = iso3;

                // Numeric code
                if (countryElement.TryGetProperty("numeric_code", out var numCode) && numCode.ValueKind == JsonValueKind.String)
                {
                    if (int.TryParse(numCode.GetString(), out var numericCode))
                        country.NumericCode = numericCode;
                }

                // Phone code (dr5hn format: "phonecode" not "phone_code")
                if (countryElement.TryGetProperty("phonecode", out var phoneCode))
                    country.PhoneCode = phoneCode.GetString();

                // Currency
                if (countryElement.TryGetProperty("currency", out var currency))
                    country.CurrencyCode = currency.GetString();
                if (countryElement.TryGetProperty("currency_name", out var currencyName))
                    country.CurrencyName = currencyName.GetString();
                if (countryElement.TryGetProperty("currency_symbol", out var currencySymbol))
                    country.CurrencySymbol = currencySymbol.GetString();

                // Flag emoji
                if (countryElement.TryGetProperty("emoji", out var emoji))
                    country.FlagEmoji = emoji.GetString();

                // Region
                if (countryElement.TryGetProperty("region", out var region))
                    country.Region = region.GetString();
                if (countryElement.TryGetProperty("subregion", out var subregion))
                    country.SubRegion = subregion.GetString();

                // Capital (string olarak geliyor)
                if (countryElement.TryGetProperty("capital", out var capital))
                    country.Capital = capital.GetString();

                // Coordinates (ayrı latitude/longitude fields)
                if (countryElement.TryGetProperty("latitude", out var lat) && lat.ValueKind == JsonValueKind.String)
                    if (double.TryParse(lat.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var latVal))
                        country.Latitude = latVal;

                if (countryElement.TryGetProperty("longitude", out var lng) && lng.ValueKind == JsonValueKind.String)
                    if (double.TryParse(lng.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lngVal))
                        country.Longitude = lngVal;

                country.IsActive = true;

                // Çevirileri ekle (dr5hn formatı: "tr": "Türkiye", "de": "Deutschland" şeklinde)
                if (countryElement.TryGetProperty("translations", out var translations))
                {
                    foreach (var translation in translations.EnumerateObject())
                    {
                        var langCode = translation.Name; // "tr", "de", "fr" etc. (2 letter)

                        // Sadece aktif dilleri ekle
                        if (!activeLanguages.Contains(langCode)) continue;

                        var translatedName = translation.Value.GetString();
                        if (string.IsNullOrEmpty(translatedName)) continue;

                        var existingTranslation = country.Translations
                            .FirstOrDefault(t => t.LanguageCode == langCode);

                        if (existingTranslation == null)
                        {
                            country.Translations.Add(new CountryTranslation
                            {
                                LanguageCode = langCode,
                                Name = translatedName
                            });
                        }
                        else
                        {
                            existingTranslation.Name = translatedName;
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ülke işlenirken hata: {Country}", countryElement.ToString());
            }
        }

        return (added, updated);
    }

    /// <summary>
    /// Eyaletleri seed eder
    /// </summary>
    public async Task<(int Added, int Updated)> SeedStatesAsync(bool forceRefresh = false)
    {
        int added = 0, updated = 0;

        var existingCount = await _context.States.CountAsync();
        if (existingCount > 0 && !forceRefresh)
        {
            _logger.LogInformation("Eyaletler zaten mevcut ({Count}), atlanıyor...", existingCount);
            return (0, 0);
        }

        // Lokal dosyadan eyaletleri oku
        _logger.LogInformation("Eyaletler okunuyor: {File}", STATES_FILE);
        var response = await ReadLocalFileAsync(STATES_FILE);
        var statesJson = JsonDocument.Parse(response);

        // Ülke mapping (ISO2 -> Id)
        var countryMapping = await _context.Countries
            .ToDictionaryAsync(c => c.Iso2Code, c => c.Id);

        foreach (var stateElement in statesJson.RootElement.EnumerateArray())
        {
            try
            {
                var countryCode = stateElement.GetProperty("country_code").GetString() ?? "";
                if (!countryMapping.TryGetValue(countryCode, out var countryId))
                    continue;

                var stateName = stateElement.GetProperty("name").GetString() ?? "";
                // dr5hn format uses "iso2" for state code (e.g. "01", "CA", "NY")
                var stateCode = stateElement.TryGetProperty("iso2", out var sc) ? sc.GetString() : null;

                var state = await _context.States
                    .FirstOrDefaultAsync(s => s.CountryId == countryId && s.Name == stateName);

                if (state == null)
                {
                    state = new State
                    {
                        CountryId = countryId,
                        Name = stateName,
                        Code = stateCode,
                        IsActive = true
                    };

                    if (stateElement.TryGetProperty("latitude", out var lat) && lat.ValueKind == JsonValueKind.String)
                        if (double.TryParse(lat.GetString(), out var latVal))
                            state.Latitude = latVal;

                    if (stateElement.TryGetProperty("longitude", out var lng) && lng.ValueKind == JsonValueKind.String)
                        if (double.TryParse(lng.GetString(), out var lngVal))
                            state.Longitude = lngVal;

                    if (stateElement.TryGetProperty("type", out var type))
                        state.Type = type.GetString();

                    _context.States.Add(state);
                    added++;
                }
                else
                {
                    updated++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Eyalet işlenirken hata");
            }
        }

        await _context.SaveChangesAsync();
        return (added, updated);
    }

    /// <summary>
    /// Belirli bir dil için çevirileri günceller
    /// </summary>
    public async Task<int> UpdateTranslationsForLanguageAsync(string languageCode)
    {
        int updated = 0;

        // Dili aktif olarak işaretle (IsGeoTranslationEnabled)
        var language = await _context.Languages.FirstOrDefaultAsync(l => l.UniqueSeoCode == languageCode);
        if (language != null && !language.IsGeoTranslationEnabled)
        {
            language.IsGeoTranslationEnabled = true;
            await _context.SaveChangesAsync();
        }

        // Lokal dosyadan ülkeleri oku
        _logger.LogInformation("Çeviriler için ülkeler okunuyor: {Lang}", languageCode);
        var response = await ReadLocalFileAsync(COUNTRIES_FILE);
        var countriesJson = JsonDocument.Parse(response);

        var countries = await _context.Countries
            .Include(c => c.Translations)
            .ToListAsync();

        foreach (var countryElement in countriesJson.RootElement.EnumerateArray())
        {
            var iso2 = countryElement.GetProperty("iso2").GetString() ?? "";
            var country = countries.FirstOrDefault(c => c.Iso2Code == iso2);
            if (country == null) continue;

            if (countryElement.TryGetProperty("translations", out var translations))
            {
                // dr5hn formatında: "tr": "Türkiye" şeklinde 2-letter code ile
                if (translations.TryGetProperty(languageCode, out var translatedName))
                {
                    var name = translatedName.GetString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        var existingTranslation = country.Translations
                            .FirstOrDefault(t => t.LanguageCode == languageCode);

                        if (existingTranslation == null)
                        {
                            country.Translations.Add(new CountryTranslation
                            {
                                LanguageCode = languageCode,
                                Name = name
                            });
                            updated++;
                        }
                        else if (existingTranslation.Name != name)
                        {
                            existingTranslation.Name = name;
                            updated++;
                        }
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        return updated;
    }

    /// <summary>
    /// Coğrafi verilerin istatistiklerini döndürür
    /// </summary>
    public async Task<GeographyStats> GetStatsAsync()
    {
        return new GeographyStats
        {
            CountryCount = await _context.Countries.CountAsync(),
            StateCount = await _context.States.CountAsync(),
            CountryTranslationCount = await _context.CountryTranslations.CountAsync(),
            StateTranslationCount = await _context.StateTranslations.CountAsync(),
            ActiveLanguageCount = await _context.Languages.CountAsync(l => l.IsGeoTranslationEnabled && l.IsActive)
        };
    }

    /// <summary>
    /// ISO 639-2 (3 letter) -> ISO 639-1 (2 letter) mapping
    /// </summary>
    private static string? MapIso639_2To1(string iso3)
    {
        return iso3 switch
        {
            "ara" => "ar",
            "bre" => "br",
            "ces" => "cs",
            "cym" => "cy",
            "deu" => "de",
            "est" => "et",
            "fin" => "fi",
            "fra" => "fr",
            "hrv" => "hr",
            "hun" => "hu",
            "ita" => "it",
            "jpn" => "ja",
            "kor" => "ko",
            "nld" => "nl",
            "per" or "fas" => "fa",
            "pol" => "pl",
            "por" => "pt",
            "rus" => "ru",
            "slk" => "sk",
            "spa" => "es",
            "srp" => "sr",
            "swe" => "sv",
            "tur" => "tr",
            "ukr" => "uk",
            "urd" => "ur",
            "zho" => "zh",
            _ => null
        };
    }

    /// <summary>
    /// ISO 639-1 (2 letter) -> ISO 639-2 (3 letter) mapping
    /// </summary>
    private static string? MapIso639_1To2(string iso1)
    {
        return iso1 switch
        {
            "ar" => "ara",
            "br" => "bre",
            "cs" => "ces",
            "cy" => "cym",
            "de" => "deu",
            "et" => "est",
            "fi" => "fin",
            "fr" => "fra",
            "hr" => "hrv",
            "hu" => "hun",
            "it" => "ita",
            "ja" => "jpn",
            "ko" => "kor",
            "nl" => "nld",
            "fa" => "per",
            "pl" => "pol",
            "pt" => "por",
            "ru" => "rus",
            "sk" => "slk",
            "es" => "spa",
            "sr" => "srp",
            "sv" => "swe",
            "tr" => "tur",
            "uk" => "ukr",
            "ur" => "urd",
            "zh" => "zho",
            _ => null
        };
    }
}

/// <summary>
/// Seed işlemi sonucu
/// </summary>
public class SeedResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CountriesAdded { get; set; }
    public int CountriesUpdated { get; set; }
    public int StatesAdded { get; set; }
    public int StatesUpdated { get; set; }
    public int TranslationsAdded { get; set; }
}

/// <summary>
/// Coğrafi veri istatistikleri
/// </summary>
public class GeographyStats
{
    public int CountryCount { get; set; }
    public int StateCount { get; set; }
    public int CountryTranslationCount { get; set; }
    public int StateTranslationCount { get; set; }
    public int ActiveLanguageCount { get; set; }
}
