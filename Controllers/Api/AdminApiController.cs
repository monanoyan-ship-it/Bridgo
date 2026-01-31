using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Services;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Admin Panel API - RBAC Yonetimi
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public class AdminApiController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly GeographySeedService _geographySeedService;

    public AdminApiController(
        IAdminService adminService,
        GeographySeedService geographySeedService)
    {
        _adminService = adminService;
        _geographySeedService = geographySeedService;
    }

    #region Dashboard

    /// <summary>
    /// Dashboard istatistikleri
    /// </summary>
    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Son kayit olan firmalar
    /// </summary>
    [HttpGet("dashboard/recent-vendors")]
    public async Task<IActionResult> GetRecentVendors()
    {
        var vendors = await _adminService.GetRecentVendorsAsync();
        return Ok(vendors);
    }

    /// <summary>
    /// Capability dagilimi
    /// </summary>
    [HttpGet("dashboard/capability-stats")]
    public async Task<IActionResult> GetCapabilityStats()
    {
        var stats = await _adminService.GetCapabilityStatsAsync();
        return Ok(stats);
    }

    #endregion

    #region Capabilities

    /// <summary>
    /// Tum capability'leri listele
    /// </summary>
    [HttpGet("capabilities")]
    public async Task<IActionResult> GetCapabilities()
    {
        var capabilities = await _adminService.GetCapabilitiesAsync();
        return Ok(capabilities);
    }

    // Capability Create/Update/Delete kaldirildi - TypeDefinitions.cs'de static tanimli

    #endregion

    #region Modules (PlatformModules)

    /// <summary>
    /// Tum platform modullerini listele (hiyerarsik)
    /// </summary>
    [HttpGet("modules")]
    public async Task<IActionResult> GetAllModules()
    {
        var modules = await _adminService.GetAllModulesAsync();
        return Ok(modules);
    }

    /// <summary>
    /// Yeni modul olustur
    /// </summary>
    [HttpPost("modules")]
    public async Task<IActionResult> CreateModule([FromBody] AdminModuleCreateDto dto)
    {
        var result = await _adminService.CreateModuleAsync(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message, id = result.Data });
    }

    /// <summary>
    /// Modul guncelle
    /// </summary>
    [HttpPut("modules/{id}")]
    public async Task<IActionResult> UpdateModule(int id, [FromBody] AdminModuleUpdateDto dto)
    {
        var result = await _adminService.UpdateModuleAsync(id, dto);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Modul sil
    /// </summary>
    [HttpDelete("modules/{id}")]
    public async Task<IActionResult> DeleteModule(int id)
    {
        var result = await _adminService.DeleteModuleAsync(id);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Silinmis modulleri listele
    /// </summary>
    [HttpGet("modules/deleted")]
    public async Task<IActionResult> GetDeletedModules()
    {
        var modules = await _adminService.GetDeletedModulesAsync();
        return Ok(modules);
    }

    /// <summary>
    /// Modulu geri yukle (restore)
    /// </summary>
    [HttpPost("modules/{id}/restore")]
    public async Task<IActionResult> RestoreModule(int id)
    {
        var result = await _adminService.RestoreModuleAsync(id);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Modulu kalici olarak sil (hard delete)
    /// </summary>
    [HttpDelete("modules/{id}/hard")]
    public async Task<IActionResult> HardDeleteModule(int id)
    {
        var result = await _adminService.HardDeleteModuleAsync(id);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Modulun capability mapping'lerini getir
    /// </summary>
    [HttpGet("modules/{id}/capabilities")]
    public async Task<IActionResult> GetModuleCapabilities(int id)
    {
        var mappings = await _adminService.GetModuleCapabilitiesAsync(id);
        return Ok(mappings);
    }

    /// <summary>
    /// Modulun capability mapping'lerini kaydet
    /// </summary>
    [HttpPost("modules/{id}/capabilities")]
    public async Task<IActionResult> SaveModuleCapabilities(int id, [FromBody] List<int> capabilityIds)
    {
        var result = await _adminService.SaveModuleCapabilitiesAsync(id, capabilityIds);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Capability'nin modul secim listesini getir (coklu secim icin)
    /// </summary>
    [HttpGet("capabilities/{id}/modules")]
    public async Task<IActionResult> GetCapabilityModules(int id)
    {
        var modules = await _adminService.GetCapabilityModulesAsync(id);
        return Ok(modules);
    }

    /// <summary>
    /// Capability'ye modulleri toplu olarak ata
    /// </summary>
    [HttpPost("capabilities/{id}/modules")]
    public async Task<IActionResult> SaveCapabilityModules(int id, [FromBody] List<int> moduleIds)
    {
        var result = await _adminService.SaveCapabilityModulesAsync(id, moduleIds);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Capability'ye tek modul ekle
    /// </summary>
    [HttpPost("capabilities/{id}/modules/add")]
    public async Task<IActionResult> AddModuleToCapability(int id, [FromBody] AddModuleToCapabilityDto dto)
    {
        var result = await _adminService.AddModuleToCapabilityAsync(id, dto.ModuleId);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Capability'ye birden fazla modul ekle
    /// </summary>
    [HttpPost("capabilities/{id}/modules/add-multiple")]
    public async Task<IActionResult> AddMultipleModulesToCapability(int id, [FromBody] AddMultipleModulesToCapabilityDto dto)
    {
        var result = await _adminService.AddMultipleModulesToCapabilityAsync(id, dto.ModuleIds);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Capability'den modul cikar
    /// </summary>
    [HttpPost("capabilities/{id}/modules/remove")]
    public async Task<IActionResult> RemoveModuleFromCapability(int id, [FromBody] AddModuleToCapabilityDto dto)
    {
        var result = await _adminService.RemoveModuleFromCapabilityAsync(id, dto.ModuleId);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    #endregion

    #region Roles

    /// <summary>
    /// Tum rolleri listele
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _adminService.GetRolesAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Yeni rol olustur
    /// </summary>
    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] AdminRoleCreateDto dto)
    {
        var result = await _adminService.CreateRoleAsync(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message, id = result.Data });
    }

    /// <summary>
    /// Rol guncelle
    /// </summary>
    [HttpPut("roles/{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] AdminRoleCreateDto dto)
    {
        var result = await _adminService.UpdateRoleAsync(id, dto);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Rol sil
    /// </summary>
    [HttpDelete("roles/{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var result = await _adminService.DeleteRoleAsync(id);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    #endregion

    #region Geography (Countries & States CRUD)

    /// <summary>
    /// Tum ulkeleri listele
    /// </summary>
    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries([FromQuery] string? search = null)
    {
        var countries = await _adminService.GetCountriesAsync(search);
        return Ok(countries);
    }

    /// <summary>
    /// Ulke detayi
    /// </summary>
    [HttpGet("countries/{id}")]
    public async Task<IActionResult> GetCountry(int id)
    {
        var country = await _adminService.GetCountryByIdAsync(id);
        if (country == null)
            return NotFound(new { message = "Ulke bulunamadi" });
        return Ok(country);
    }

    /// <summary>
    /// Yeni ulke olustur
    /// </summary>
    [HttpPost("countries")]
    public async Task<IActionResult> CreateCountry([FromBody] AdminCountryCreateUpdateDto dto)
    {
        var result = await _adminService.CreateCountryAsync(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message, id = result.Data });
    }

    /// <summary>
    /// Ulke guncelle
    /// </summary>
    [HttpPut("countries/{id}")]
    public async Task<IActionResult> UpdateCountry(int id, [FromBody] AdminCountryCreateUpdateDto dto)
    {
        var result = await _adminService.UpdateCountryAsync(id, dto);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Ulke sil
    /// </summary>
    [HttpDelete("countries/{id}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var result = await _adminService.DeleteCountryAsync(id);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Tum eyaletleri/illeri listele
    /// </summary>
    [HttpGet("states")]
    public async Task<IActionResult> GetStates([FromQuery] int? countryId = null, [FromQuery] string? search = null)
    {
        var states = await _adminService.GetStatesAsync(countryId, search);
        return Ok(states);
    }

    /// <summary>
    /// Eyalet/il detayi
    /// </summary>
    [HttpGet("states/{id}")]
    public async Task<IActionResult> GetState(int id)
    {
        var state = await _adminService.GetStateByIdAsync(id);
        if (state == null)
            return NotFound(new { message = "Eyalet/il bulunamadi" });
        return Ok(state);
    }

    /// <summary>
    /// Yeni eyalet/il olustur
    /// </summary>
    [HttpPost("states")]
    public async Task<IActionResult> CreateState([FromBody] AdminStateCreateUpdateDto dto)
    {
        var result = await _adminService.CreateStateAsync(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message, id = result.Data });
    }

    /// <summary>
    /// Eyalet/il guncelle
    /// </summary>
    [HttpPut("states/{id}")]
    public async Task<IActionResult> UpdateState(int id, [FromBody] AdminStateCreateUpdateDto dto)
    {
        var result = await _adminService.UpdateStateAsync(id, dto);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Eyalet/il sil
    /// </summary>
    [HttpDelete("states/{id}")]
    public async Task<IActionResult> DeleteState(int id)
    {
        var result = await _adminService.DeleteStateAsync(id);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Ulke detayi ve cevirileri getir
    /// </summary>
    [HttpGet("countries/{id}/detail")]
    public async Task<IActionResult> GetCountryDetail(int id)
    {
        var country = await _adminService.GetCountryDetailAsync(id);
        if (country == null)
            return NotFound(new { message = "Ulke bulunamadi" });
        return Ok(country);
    }

    /// <summary>
    /// Ulke cevirilerini guncelle
    /// </summary>
    [HttpPut("countries/{id}/translations")]
    public async Task<IActionResult> UpdateCountryTranslations(int id, [FromBody] List<AdminCountryTranslationUpdateDto> translations)
    {
        var result = await _adminService.UpdateCountryTranslationsAsync(id, translations);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Ulke aktif/pasif toggle
    /// </summary>
    [HttpPost("countries/{id}/toggle-active")]
    public async Task<IActionResult> ToggleCountryActive(int id)
    {
        var result = await _adminService.ToggleCountryActiveAsync(id);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Eyalet detayi ve cevirileri getir
    /// </summary>
    [HttpGet("states/{id}/detail")]
    public async Task<IActionResult> GetStateDetail(int id)
    {
        var state = await _adminService.GetStateDetailAsync(id);
        if (state == null)
            return NotFound(new { message = "Eyalet/il bulunamadi" });
        return Ok(state);
    }

    /// <summary>
    /// Eyalet cevirilerini guncelle
    /// </summary>
    [HttpPut("states/{id}/translations")]
    public async Task<IActionResult> UpdateStateTranslations(int id, [FromBody] List<AdminStateTranslationUpdateDto> translations)
    {
        var result = await _adminService.UpdateStateTranslationsAsync(id, translations);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Eyalet aktif/pasif toggle
    /// </summary>
    [HttpPost("states/{id}/toggle-active")]
    public async Task<IActionResult> ToggleStateActive(int id)
    {
        var result = await _adminService.ToggleStateActiveAsync(id);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    #endregion

    #region Geography Seed

    /// <summary>
    /// Tum cografi verileri seed et (ulkeler, eyaletler, sehirler)
    /// </summary>
    [HttpPost("geography/seed")]
    public async Task<IActionResult> SeedAllGeography([FromQuery] bool forceRefresh = false)
    {
        var result = await _geographySeedService.SeedAllAsync(forceRefresh);
        return Ok(new
        {
            success = true,
            message = "Cografi veriler basariyla yuklendi",
            data = new
            {
                countriesAdded = result.CountriesAdded,
                countriesUpdated = result.CountriesUpdated,
                statesAdded = result.StatesAdded,
                translationsAdded = result.TranslationsAdded
            }
        });
    }

    /// <summary>
    /// Sadece ulkeleri seed et
    /// </summary>
    [HttpPost("geography/seed/countries")]
    public async Task<IActionResult> SeedCountries([FromQuery] bool forceRefresh = false)
    {
        var (added, updated) = await _geographySeedService.SeedCountriesAsync(forceRefresh);
        return Ok(new
        {
            success = true,
            message = $"{added} ulke eklendi, {updated} ulke guncellendi",
            data = new { added, updated }
        });
    }

    /// <summary>
    /// Sadece eyaletleri seed et
    /// </summary>
    [HttpPost("geography/seed/states")]
    public async Task<IActionResult> SeedStates([FromQuery] bool forceRefresh = false)
    {
        var (added, updated) = await _geographySeedService.SeedStatesAsync(forceRefresh);
        return Ok(new
        {
            success = true,
            message = $"{added} eyalet eklendi, {updated} eyalet guncellendi",
            data = new { added, updated }
        });
    }

    /// <summary>
    /// Belirli bir dil icin cevirileri guncelle
    /// </summary>
    [HttpPost("geography/translations/{languageCode}")]
    public async Task<IActionResult> UpdateTranslationsForLanguage(string languageCode)
    {
        var added = await _geographySeedService.UpdateTranslationsForLanguageAsync(languageCode);
        return Ok(new
        {
            success = true,
            message = $"{languageCode} dili icin {added} ceviri eklendi",
            data = new { languageCode, added }
        });
    }

    /// <summary>
    /// Cografi verilerin istatistiklerini getir
    /// </summary>
    [HttpGet("geography/stats")]
    public async Task<IActionResult> GetGeographyStats()
    {
        var stats = await _geographySeedService.GetStatsAsync();
        return Ok(stats);
    }

    #endregion

    #region Role Permissions

    /// <summary>
    /// Rol izinlerini getir
    /// </summary>
    [HttpGet("roles/{roleId}/permissions")]
    public async Task<IActionResult> GetRolePermissions(int roleId)
    {
        var permissions = await _adminService.GetRolePermissionsAsync(roleId);
        return Ok(permissions);
    }

    /// <summary>
    /// Tum capability modullerini gruplu getir (rol izin atama icin)
    /// </summary>
    [HttpGet("roles/{roleId}/permissions/grouped")]
    public async Task<IActionResult> GetRolePermissionsGrouped(int roleId)
    {
        var groups = await _adminService.GetAllModulesGroupedByCapabilityAsync(roleId);
        return Ok(groups);
    }

    /// <summary>
    /// Rol izinlerini kaydet
    /// </summary>
    [HttpPost("roles/{roleId}/permissions")]
    public async Task<IActionResult> SaveRolePermissions(int roleId, [FromBody] List<PermissionSaveDto> permissions)
    {
        var result = await _adminService.SaveRolePermissionsAsync(roleId, permissions);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    #endregion

    #region Users

    /// <summary>
    /// Tum kullanicilari listele
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search = null, [FromQuery] string? roleFilter = null)
    {
        var users = await _adminService.GetUsersAsync(search, roleFilter);
        return Ok(users);
    }

    /// <summary>
    /// Kullanici detayi
    /// </summary>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "Kullanici bulunamadi" });
        return Ok(user);
    }

    /// <summary>
    /// Yeni kullanici olustur
    /// </summary>
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] AdminUserCreateDto dto)
    {
        var result = await _adminService.CreateUserAsync(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message, id = result.Data });
    }

    /// <summary>
    /// Kullanici durumunu degistir (aktif/pasif)
    /// </summary>
    [HttpPost("users/{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var result = await _adminService.ToggleUserStatusAsync(id);
        if (!result.Success)
            return NotFound(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Firma listesi (dropdown icin)
    /// </summary>
    [HttpGet("vendors/dropdown")]
    public async Task<IActionResult> GetVendorsForDropdown()
    {
        var vendors = await _adminService.GetVendorsAsync();
        return Ok(vendors.Select(v => new { v.Id, v.CompanyName }));
    }

    #endregion

    #region Vendors

    /// <summary>
    /// Tum firmalari listele
    /// </summary>
    [HttpGet("vendors")]
    public async Task<IActionResult> GetVendors([FromQuery] string? search = null, [FromQuery] int? status = null)
    {
        var vendors = await _adminService.GetVendorsAsync(search, status);
        return Ok(vendors);
    }

    /// <summary>
    /// Firma detayi
    /// </summary>
    [HttpGet("vendors/{id}")]
    public async Task<IActionResult> GetVendor(int id)
    {
        var vendor = await _adminService.GetVendorByIdAsync(id);
        if (vendor == null)
            return NotFound(new { message = "Firma bulunamadi" });
        return Ok(vendor);
    }

    /// <summary>
    /// Firma onayla
    /// </summary>
    [HttpPost("vendors/{id}/approve")]
    public async Task<IActionResult> ApproveVendor(int id)
    {
        var result = await _adminService.ApproveVendorAsync(id);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Firma askiya al
    /// </summary>
    [HttpPost("vendors/{id}/suspend")]
    public async Task<IActionResult> SuspendVendor(int id)
    {
        var result = await _adminService.SuspendVendorAsync(id);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Firma yeniden aktif et
    /// </summary>
    [HttpPost("vendors/{id}/reactivate")]
    public async Task<IActionResult> ReactivateVendor(int id)
    {
        var result = await _adminService.ReactivateVendorAsync(id);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    #endregion

    #region Languages

    /// <summary>
    /// Tum dilleri listele
    /// </summary>
    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages([FromQuery] bool onlyActive = false)
    {
        var languages = await _adminService.GetLanguagesAsync(onlyActive);
        return Ok(languages);
    }

    /// <summary>
    /// Dil detayi
    /// </summary>
    [HttpGet("languages/{id}")]
    public async Task<IActionResult> GetLanguage(int id)
    {
        var language = await _adminService.GetLanguageByIdAsync(id);
        if (language == null)
            return NotFound(new { message = "Dil bulunamadi" });
        return Ok(language);
    }

    /// <summary>
    /// Yeni dil olustur
    /// </summary>
    [HttpPost("languages")]
    public async Task<IActionResult> CreateLanguage([FromBody] AdminLanguageCreateDto dto)
    {
        var result = await _adminService.CreateLanguageAsync(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message, id = result.Data });
    }

    /// <summary>
    /// Dil guncelle
    /// </summary>
    [HttpPut("languages/{id}")]
    public async Task<IActionResult> UpdateLanguage(int id, [FromBody] AdminLanguageCreateDto dto)
    {
        var result = await _adminService.UpdateLanguageAsync(id, dto);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Dil sil
    /// </summary>
    [HttpDelete("languages/{id}")]
    public async Task<IActionResult> DeleteLanguage(int id)
    {
        var result = await _adminService.DeleteLanguageAsync(id);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Varsayilan dil yap
    /// </summary>
    [HttpPost("languages/{id}/set-default")]
    public async Task<IActionResult> SetDefaultLanguage(int id)
    {
        var result = await _adminService.SetDefaultLanguageAsync(id);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Dil aktif/pasif toggle
    /// </summary>
    [HttpPost("languages/{id}/toggle-active")]
    public async Task<IActionResult> ToggleLanguageActive(int id)
    {
        var result = await _adminService.ToggleLanguageActiveAsync(id);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    #endregion

    #region Localization Resources

    /// <summary>
    /// Dile ait kaynaklari listele (sayfalamali)
    /// </summary>
    [HttpGet("languages/{languageId}/resources")]
    public async Task<IActionResult> GetLocalizationResources(
        int languageId,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var resources = await _adminService.GetLocalizationResourcesAsync(languageId, search, page, pageSize);
        return Ok(resources);
    }

    /// <summary>
    /// Yeni kaynak olustur
    /// </summary>
    [HttpPost("localization/resources")]
    public async Task<IActionResult> CreateLocalizationResource([FromBody] AdminLocalizationResourceCreateDto dto)
    {
        var result = await _adminService.CreateLocalizationResourceAsync(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message, id = result.Data });
    }

    /// <summary>
    /// Kaynak guncelle
    /// </summary>
    [HttpPut("localization/resources/{id}")]
    public async Task<IActionResult> UpdateLocalizationResource(int id, [FromBody] AdminLocalizationResourceUpdateDto dto)
    {
        var result = await _adminService.UpdateLocalizationResourceAsync(id, dto);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Kaynak sil
    /// </summary>
    [HttpDelete("localization/resources/{id}")]
    public async Task<IActionResult> DeleteLocalizationResource(int id)
    {
        var result = await _adminService.DeleteLocalizationResourceAsync(id);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Kaynaklari toplu ice aktar
    /// </summary>
    [HttpPost("languages/{languageId}/resources/import")]
    public async Task<IActionResult> ImportLocalizationResources(int languageId, [FromBody] List<LocalizationResourceImportDto> resources)
    {
        var result = await _adminService.ImportLocalizationResourcesAsync(languageId, resources);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message, addedCount = result.Data });
    }

    /// <summary>
    /// Kaynaklari dosyadan ice aktar (wwwroot/data/localization/resources.{seoCode}.xml)
    /// </summary>
    [HttpPost("languages/{languageId}/resources/import-from-file")]
    public async Task<IActionResult> ImportLocalizationResourcesFromFile(int languageId)
    {
        var result = await _adminService.ImportLocalizationResourcesFromFileAsync(languageId);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message, addedCount = result.Data });
    }

    /// <summary>
    /// Kaynaklari disa aktar (JSON)
    /// </summary>
    [HttpGet("languages/{languageId}/resources/export")]
    public async Task<IActionResult> ExportLocalizationResources(int languageId)
    {
        var resources = await _adminService.ExportLocalizationResourcesAsync(languageId);
        return Ok(resources);
    }

    #endregion

    #region Product Categories (Global - Admin yonetimli)

    /// <summary>
    /// Tum kategorileri listele (hiyerarsik)
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _adminService.GetCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Kategori detayi
    /// </summary>
    [HttpGet("categories/{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _adminService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound(new { message = "Kategori bulunamadi" });
        return Ok(category);
    }

    /// <summary>
    /// Yeni kategori olustur
    /// </summary>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] AdminCategoryCreateUpdateDto dto)
    {
        var result = await _adminService.CreateCategoryAsync(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message, id = result.Data });
    }

    /// <summary>
    /// Kategoriyi guncelle
    /// </summary>
    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] AdminCategoryCreateUpdateDto dto)
    {
        var result = await _adminService.UpdateCategoryAsync(id, dto);
        if (!result.Success)
            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Kategoriyi sil
    /// </summary>
    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id, [FromQuery] int? targetCategoryId = null)
    {
        var deletedBy = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "Unknown";
        var result = await _adminService.DeleteCategoryAsync(id, targetCategoryId, deletedBy);
        if (!result.Success)
        {
            // PRODUCTS_EXIST:count formatinda donus - urunlerin tasinmasi gerekiyor
            if (result.Message?.StartsWith("PRODUCTS_EXIST:") == true)
            {
                var count = int.Parse(result.Message.Split(':')[1]);
                return BadRequest(new {
                    message = $"Bu kategoride {count} urun var. Urunleri tasimak icin hedef kategori secin.",
                    requiresProductMigration = true,
                    productCount = count
                });
            }

            return result.Message?.Contains("bulunamadi") == true
                ? NotFound(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        }
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Ornek kategorileri yukle (seed)
    /// </summary>
    [HttpPost("categories/seed")]
    public async Task<IActionResult> SeedCategories()
    {
        var result = await _adminService.SeedCategoriesAsync();
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Silinen kategorileri listele (cop kutusu)
    /// </summary>
    [HttpGet("categories/deleted")]
    public async Task<IActionResult> GetDeletedCategories()
    {
        var categories = await _adminService.GetDeletedCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Kategoriyi geri yukle
    /// </summary>
    [HttpPost("categories/{id}/restore")]
    public async Task<IActionResult> RestoreCategory(int id)
    {
        var result = await _adminService.RestoreCategoryAsync(id);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Kategoriyi kalici olarak sil
    /// </summary>
    [HttpDelete("categories/{id}/permanent")]
    public async Task<IActionResult> PermanentDeleteCategory(int id)
    {
        var result = await _adminService.PermanentDeleteCategoryAsync(id);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Onay bekleyen silme islemlerini listele
    /// </summary>
    [HttpGet("categories/pending")]
    public async Task<IActionResult> GetPendingDeletionCategories()
    {
        var categories = await _adminService.GetPendingDeletionCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Silme islemini onayla
    /// </summary>
    [HttpPost("categories/{id}/approve")]
    public async Task<IActionResult> ApproveDeletion(int id, [FromBody] ReviewRequest? request)
    {
        var reviewedBy = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "Unknown";
        var result = await _adminService.ApproveDeletionAsync(id, reviewedBy, request?.ReviewNote);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Silme islemini reddet
    /// </summary>
    [HttpPost("categories/{id}/reject")]
    public async Task<IActionResult> RejectDeletion(int id, [FromBody] ReviewRequest? request)
    {
        var reviewedBy = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "Unknown";
        var result = await _adminService.RejectDeletionAsync(id, reviewedBy, request?.ReviewNote);
        if (!result.Success)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    #endregion

    #region Profile Approvals

    /// <summary>
    /// Onay bekleyen profilleri listele
    /// </summary>
    [HttpGet("profiles/pending")]
    public async Task<IActionResult> GetPendingProfiles()
    {
        var profiles = await _adminService.GetPendingProfileApprovalsAsync();
        return Ok(profiles);
    }

    /// <summary>
    /// Profil detayi getir
    /// </summary>
    [HttpGet("profiles/{id}")]
    public async Task<IActionResult> GetProfileById(int id)
    {
        var profile = await _adminService.GetProfileByIdAsync(id);
        if (profile == null)
            return NotFound(new { message = "Profil bulunamadi" });
        return Ok(profile);
    }

    /// <summary>
    /// Profili onayla
    /// </summary>
    [HttpPost("profiles/{id}/approve")]
    public async Task<IActionResult> ApproveProfile(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { message = "Kullanici bulunamadi" });

        var result = await _adminService.ApproveProfileAsync(id, userId);
        if (!result)
            return BadRequest(new { message = "Profil onaylanamadi" });
        return Ok(new { message = "Profil onaylandi" });
    }

    /// <summary>
    /// Profili reddet
    /// </summary>
    [HttpPost("profiles/{id}/reject")]
    public async Task<IActionResult> RejectProfile(int id, [FromBody] ProfileRejectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Reason))
            return BadRequest(new { message = "Red sebebi zorunludur" });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { message = "Kullanici bulunamadi" });

        var result = await _adminService.RejectProfileAsync(id, request.Reason, userId);
        if (!result)
            return BadRequest(new { message = "Profil reddedilemedi" });
        return Ok(new { message = "Profil reddedildi" });
    }

    #endregion
}

public class ReviewRequest
{
    public string? ReviewNote { get; set; }
}

public class ProfileRejectRequest
{
    public string? Reason { get; set; }
}
