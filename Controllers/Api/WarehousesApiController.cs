using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Authorization;
using Bridgo.DTOs.Warehouse;
using Bridgo.Models.Identity;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Depo yönetimi API endpoint'leri
/// </summary>
[ApiController]
[Route("api/warehouses")]
[Authorize]
[RequireCapability(VendorCapabilities.Seller)]
public class WarehousesApiController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly UserManager<ApplicationUser> _userManager;

    public WarehousesApiController(
        IWarehouseService warehouseService,
        UserManager<ApplicationUser> userManager)
    {
        _warehouseService = warehouseService;
        _userManager = userManager;
    }

    /// <summary>
    /// Tüm depoları listele
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        var warehouses = await _warehouseService.GetAllAsync(user.VendorId.Value, isActive);
        return Ok(warehouses);
    }

    /// <summary>
    /// Depo detayı
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        var warehouse = await _warehouseService.GetByIdAsync(id, user.VendorId.Value);
        if (warehouse == null)
            return NotFound(new { message = "Depo bulunamadı." });

        return Ok(warehouse);
    }

    /// <summary>
    /// Yeni depo oluştur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WarehouseCreateDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "Depo kodu zorunludur." });

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Depo adı zorunludur." });

        try
        {
            var warehouse = await _warehouseService.CreateAsync(dto, user.VendorId.Value, user.Id.ToString());
            return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Depo güncelle
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] WarehouseUpdateDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "Depo kodu zorunludur." });

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Depo adı zorunludur." });

        try
        {
            var result = await _warehouseService.UpdateAsync(id, dto, user.VendorId.Value, user.Id.ToString());
            if (!result)
                return NotFound(new { message = "Depo bulunamadı." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Depo sil
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        try
        {
            var result = await _warehouseService.DeleteAsync(id, user.VendorId.Value, user.Id.ToString());
            if (!result)
                return NotFound(new { message = "Depo bulunamadı." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Depoyu varsayılan yap
    /// </summary>
    [HttpPost("{id:int}/set-default")]
    public async Task<IActionResult> SetAsDefault(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        var result = await _warehouseService.SetAsDefaultAsync(id, user.VendorId.Value);
        if (!result)
            return NotFound(new { message = "Depo bulunamadı." });

        return NoContent();
    }

    /// <summary>
    /// Dropdown için depo listesi
    /// </summary>
    [HttpGet("select")]
    public async Task<IActionResult> GetForSelect()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        var warehouses = await _warehouseService.GetForSelectAsync(user.VendorId.Value);
        return Ok(warehouses);
    }

    /// <summary>
    /// Depo tipleri listesi
    /// </summary>
    [HttpGet("types")]
    public IActionResult GetWarehouseTypes()
    {
        var types = WarehouseTypes.All.Select(t => new
        {
            id = t.Id,
            systemName = t.SystemName,
            nameResourceKey = t.NameResourceKey,
            icon = t.Icon
        });

        return Ok(types);
    }

    /// <summary>
    /// Kod benzersizlik kontrolü
    /// </summary>
    [HttpGet("check-code")]
    public async Task<IActionResult> CheckCode([FromQuery] string code, [FromQuery] int? excludeId = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        var isUnique = await _warehouseService.IsCodeUniqueAsync(code, user.VendorId.Value, excludeId);
        return Ok(new { isUnique });
    }
}
