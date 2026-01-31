using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Models.Entities;
using Bridgo.Services;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Localization API - Dil ve ceviri yonetimi
/// </summary>
[ApiController]
[Route("api/localization")]
public class LocalizationApiController : ControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IWebHostEnvironment _environment;
    private readonly ApplicationDbContext _context;

    public LocalizationApiController(
        ILocalizationService localizationService,
        IWebHostEnvironment environment,
        ApplicationDbContext context)
    {
        _localizationService = localizationService;
        _environment = environment;
        _context = context;
    }

    #region Languages

    /// <summary>
    /// Tum dilleri getirir
    /// GET /api/localization/languages
    /// </summary>
    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages([FromQuery] bool onlyActive = true)
    {
        var languages = await _localizationService.GetAllLanguagesAsync(onlyActive);
        return Ok(languages.Select(l => new
        {
            l.Id,
            l.Name,
            l.LanguageCulture,
            l.UniqueSeoCode,
            l.FlagImageFileName,
            l.Rtl,
            l.IsDefault,
            l.IsActive,
            l.DisplayOrder
        }));
    }

    /// <summary>
    /// Belirli bir dili getirir
    /// GET /api/localization/languages/{id}
    /// </summary>
    [HttpGet("languages/{id}")]
    public async Task<IActionResult> GetLanguage(int id)
    {
        var language = await _localizationService.GetLanguageByIdAsync(id);
        if (language == null)
            return NotFound(new { message = "Dil bulunamadi" });

        return Ok(new
        {
            language.Id,
            language.Name,
            language.LanguageCulture,
            language.UniqueSeoCode,
            language.FlagImageFileName,
            language.Rtl,
            language.IsDefault,
            language.IsActive,
            language.DisplayOrder
        });
    }

    /// <summary>
    /// Yeni dil olusturur (Admin)
    /// POST /api/localization/languages
    /// </summary>
    [HttpPost("languages")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateLanguage([FromBody] CreateLanguageDto dto)
    {
        var language = new Language
        {
            Name = dto.Name,
            LanguageCulture = dto.LanguageCulture,
            UniqueSeoCode = dto.UniqueSeoCode,
            FlagImageFileName = dto.FlagImageFileName,
            Rtl = dto.Rtl,
            IsDefault = dto.IsDefault,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };

        var result = await _localizationService.CreateLanguageAsync(language);
        return Ok(new { message = "Dil olusturuldu", id = result.Id });
    }

    /// <summary>
    /// Dili gunceller (Admin)
    /// PUT /api/localization/languages/{id}
    /// </summary>
    [HttpPut("languages/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateLanguage(int id, [FromBody] UpdateLanguageDto dto)
    {
        var language = new Language
        {
            Id = id,
            Name = dto.Name,
            LanguageCulture = dto.LanguageCulture,
            UniqueSeoCode = dto.UniqueSeoCode,
            FlagImageFileName = dto.FlagImageFileName,
            Rtl = dto.Rtl,
            IsDefault = dto.IsDefault,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };

        await _localizationService.UpdateLanguageAsync(language);
        return Ok(new { message = "Dil guncellendi" });
    }

    /// <summary>
    /// Dili siler (Admin)
    /// DELETE /api/localization/languages/{id}
    /// </summary>
    [HttpDelete("languages/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteLanguage(int id)
    {
        try
        {
            await _localizationService.DeleteLanguageAsync(id);
            return Ok(new { message = "Dil silindi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Resources

    /// <summary>
    /// Tum ceviri kaynaklarini getirir (JS icin)
    /// GET /api/localization/resources
    /// </summary>
    [HttpGet("resources")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> GetResources()
    {
        var languageId = _localizationService.GetCurrentLanguageId();
        var resources = await _localizationService.GetAllResourcesAsync(languageId);

        return Ok(new
        {
            languageId,
            culture = _localizationService.GetCurrentLanguageCulture(),
            resources
        });
    }

    /// <summary>
    /// Belirli bir dil icin tum kaynaklari getirir
    /// GET /api/localization/resources/{languageId}
    /// </summary>
    [HttpGet("resources/{languageId}")]
    public async Task<IActionResult> GetResourcesByLanguage(int languageId)
    {
        var resources = await _localizationService.GetResourcesByLanguageAsync(languageId);
        return Ok(resources.Select(r => new
        {
            r.Id,
            r.ResourceName,
            r.ResourceValue
        }));
    }

    /// <summary>
    /// Prefix ile kaynaklari getirir
    /// GET /api/localization/resources/{languageId}/prefix/{prefix}
    /// </summary>
    [HttpGet("resources/{languageId}/prefix/{prefix}")]
    public async Task<IActionResult> GetResourcesByPrefix(int languageId, string prefix)
    {
        var resources = await _localizationService.GetResourcesByPrefixAsync(prefix, languageId);
        return Ok(resources);
    }

    /// <summary>
    /// Belirli bir key'in cevirisini getirir
    /// GET /api/localization/resource?key=Common.Save
    /// </summary>
    [HttpGet("resource")]
    public IActionResult GetResource([FromQuery] string key)
    {
        if (string.IsNullOrEmpty(key))
            return BadRequest(new { message = "Key gerekli" });

        var value = _localizationService.T(key);
        return Ok(new { key, value });
    }

    /// <summary>
    /// Ceviri ekler/gunceller (Admin)
    /// POST /api/localization/resources/{languageId}
    /// </summary>
    [HttpPost("resources/{languageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetResource(int languageId, [FromBody] SetResourceDto dto)
    {
        var result = await _localizationService.SetResourceAsync(languageId, dto.ResourceName, dto.ResourceValue);
        return Ok(new { message = "Kaynak kaydedildi", id = result.Id });
    }

    /// <summary>
    /// Ceviriyi siler (Admin)
    /// DELETE /api/localization/resources/{resourceId}
    /// </summary>
    [HttpDelete("resources/{resourceId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteResource(int resourceId)
    {
        await _localizationService.DeleteResourceAsync(resourceId);
        return Ok(new { message = "Kaynak silindi" });
    }

    #endregion

    #region Import/Export

    /// <summary>
    /// XML'den ceviri import eder (Admin)
    /// POST /api/localization/import/{languageId}/xml
    /// </summary>
    [HttpPost("import/{languageId}/xml")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ImportFromXml(int languageId, [FromBody] ImportXmlDto dto)
    {
        try
        {
            var count = await _localizationService.ImportFromXmlAsync(languageId, dto.XmlContent);
            return Ok(new { message = $"{count} kaynak import edildi", count });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Varsayilan XML dosyasindan import eder (Admin)
    /// POST /api/localization/import/{languageId}/default
    /// </summary>
    [HttpPost("import/{languageId}/default")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ImportFromDefaultXml(int languageId)
    {
        try
        {
            var basePath = _environment.ContentRootPath;
            var count = await _localizationService.ImportFromDefaultXmlAsync(languageId, basePath);
            return Ok(new { message = $"{count} kaynak import edildi", count });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cevirileri XML olarak export eder (Admin)
    /// GET /api/localization/export/{languageId}/xml
    /// </summary>
    [HttpGet("export/{languageId}/xml")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportToXml(int languageId)
    {
        try
        {
            var xml = await _localizationService.ExportToXmlAsync(languageId);
            var language = await _localizationService.GetLanguageByIdAsync(languageId);
            var fileName = $"resources.{language?.UniqueSeoCode ?? "unknown"}.xml";

            return File(
                System.Text.Encoding.UTF8.GetBytes(xml),
                "application/xml",
                fileName
            );
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mevcut XML dosyalarini listeler
    /// GET /api/localization/available-xml-files
    /// </summary>
    [HttpGet("available-xml-files")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAvailableXmlFiles()
    {
        var localizationPath = Path.Combine(_environment.ContentRootPath, "App_Data", "Localization");

        if (!Directory.Exists(localizationPath))
            return Ok(new List<object>());

        var files = Directory.GetFiles(localizationPath, "resources.*.xml")
            .Select(f => new
            {
                FileName = Path.GetFileName(f),
                LanguageCode = Path.GetFileNameWithoutExtension(f).Replace("resources.", "")
            })
            .ToList();

        return Ok(files);
    }

    #endregion

    #region Current Language

    /// <summary>
    /// Mevcut dil bilgisini getirir
    /// GET /api/localization/current
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentLanguage()
    {
        var langId = _localizationService.GetCurrentLanguageId();
        var language = await _localizationService.GetLanguageByIdAsync(langId);

        if (language == null)
            language = await _localizationService.GetDefaultLanguageAsync();

        return Ok(new
        {
            language?.Id,
            language?.Name,
            language?.LanguageCulture,
            language?.UniqueSeoCode,
            language?.Rtl
        });
    }

    /// <summary>
    /// Mevcut dili degistirir
    /// POST /api/localization/current/{languageId}
    /// </summary>
    [HttpPost("current/{languageId}")]
    public IActionResult SetCurrentLanguage(int languageId)
    {
        _localizationService.SetCurrentLanguage(languageId);
        return Ok(new { message = "Dil degistirildi" });
    }

    /// <summary>
    /// Dil kodu ile mevcut dili degistirir
    /// POST /api/localization/current/code/{languageCode}
    /// </summary>
    [HttpPost("current/code/{languageCode}")]
    public IActionResult SetCurrentLanguageByCode(string languageCode)
    {
        _localizationService.SetCurrentLanguage(languageCode);
        return Ok(new { message = "Dil degistirildi" });
    }

    #endregion

    #region Countries

    /// <summary>
    /// Aktif ulkeleri getirir
    /// GET /api/localization/countries
    /// </summary>
    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries([FromQuery] string? lang = null)
    {
        var languageCode = lang ?? _localizationService.GetCurrentLanguageCulture()?.Split('-')[0] ?? "en";

        var countries = await _context.Countries
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                Code = c.Iso2Code,
                Name = c.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? c.Name,
                c.FlagEmoji,
                c.PhoneCode,
                c.Region
            })
            .ToListAsync();

        return Ok(countries);
    }

    #endregion
}

#region DTOs

public class CreateLanguageDto
{
    public string Name { get; set; } = string.Empty;
    public string LanguageCulture { get; set; } = string.Empty;
    public string UniqueSeoCode { get; set; } = string.Empty;
    public string? FlagImageFileName { get; set; }
    public bool Rtl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

public class UpdateLanguageDto
{
    public string Name { get; set; } = string.Empty;
    public string LanguageCulture { get; set; } = string.Empty;
    public string UniqueSeoCode { get; set; } = string.Empty;
    public string? FlagImageFileName { get; set; }
    public bool Rtl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public class SetResourceDto
{
    public string ResourceName { get; set; } = string.Empty;
    public string ResourceValue { get; set; } = string.Empty;
}

public class ImportXmlDto
{
    public string XmlContent { get; set; } = string.Empty;
}

#endregion
