using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.DTOs.Category;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/category-requests")]
[Authorize]
public class CategoryRequestApiController : ControllerBase
{
    private readonly ICategoryRequestService _categoryRequestService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CategoryRequestApiController(
        ICategoryRequestService categoryRequestService,
        UserManager<ApplicationUser> userManager)
    {
        _categoryRequestService = categoryRequestService;
        _userManager = userManager;
    }

    private async Task<(ApplicationUser user, int? vendorId)> GetCurrentUserAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return (user!, user?.VendorId);
    }

    // ========== Kullanici endpoint'leri ==========

    /// <summary>
    /// Vendor'in kategori taleplerini listele
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyRequests([FromQuery] int? status = null)
    {
        var (user, vendorId) = await GetCurrentUserAsync();
        if (!vendorId.HasValue)
            return BadRequest(new { message = "Vendor bulunamadi" });

        var requests = await _categoryRequestService.GetVendorRequestsAsync(vendorId.Value, status);
        return Ok(requests);
    }

    /// <summary>
    /// Talep detayi
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var (user, vendorId) = await GetCurrentUserAsync();
        if (!vendorId.HasValue)
            return BadRequest(new { message = "Vendor bulunamadi" });

        var request = await _categoryRequestService.GetByIdAsync(id, vendorId.Value);
        if (request == null)
            return NotFound(new { message = "Talep bulunamadi" });

        return Ok(request);
    }

    /// <summary>
    /// Yeni kategori talebi olustur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryRequestCreateDto dto)
    {
        var (user, vendorId) = await GetCurrentUserAsync();
        if (!vendorId.HasValue)
            return BadRequest(new { message = "Vendor bulunamadi" });

        if (string.IsNullOrWhiteSpace(dto.RequestedName))
            return BadRequest(new { message = "Kategori adi zorunludur" });

        try
        {
            var result = await _categoryRequestService.CreateAsync(dto, vendorId.Value, user.Id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Talep iptal et
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var (user, vendorId) = await GetCurrentUserAsync();
        if (!vendorId.HasValue)
            return BadRequest(new { message = "Vendor bulunamadi" });

        try
        {
            var result = await _categoryRequestService.CancelAsync(id, vendorId.Value);
            if (!result)
                return NotFound(new { message = "Talep bulunamadi" });

            return Ok(new { message = "Talep iptal edildi" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kategori arama
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchCategories([FromQuery] string q, [FromQuery] int? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new List<CategorySearchResultDto>());

        var results = await _categoryRequestService.SearchCategoriesAsync(q, parentId);
        return Ok(results);
    }

    /// <summary>
    /// Benzer kategori kontrolu
    /// </summary>
    [HttpGet("check-similar")]
    public async Task<IActionResult> CheckSimilar([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Ok(new List<CategorySearchResultDto>());

        var results = await _categoryRequestService.FindSimilarCategoriesAsync(name);
        return Ok(results);
    }

    // ========== Admin endpoint'leri ==========

    /// <summary>
    /// Tum talepleri listele (Admin)
    /// </summary>
    [HttpGet("admin/all")]
    [Authorize(Policy = "SystemAdmin")]
    public async Task<IActionResult> GetAllRequests([FromQuery] int? status = null)
    {
        var requests = await _categoryRequestService.GetAllRequestsAsync(status);
        return Ok(requests);
    }

    /// <summary>
    /// Bekleyen talep sayisi (Admin)
    /// </summary>
    [HttpGet("admin/pending-count")]
    [Authorize(Policy = "SystemAdmin")]
    public async Task<IActionResult> GetPendingCount()
    {
        var count = await _categoryRequestService.GetPendingCountAsync();
        return Ok(new { count });
    }

    /// <summary>
    /// Talep incele - onay/red (Admin)
    /// </summary>
    [HttpPost("admin/review")]
    [Authorize(Policy = "SystemAdmin")]
    public async Task<IActionResult> ReviewRequest([FromBody] CategoryRequestReviewDto dto)
    {
        var (user, _) = await GetCurrentUserAsync();

        var result = await _categoryRequestService.ReviewRequestAsync(dto, user.Id);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    /// <summary>
    /// Talep detayi (Admin - vendorId kontrolu yok)
    /// </summary>
    [HttpGet("admin/{id}")]
    [Authorize(Policy = "SystemAdmin")]
    public async Task<IActionResult> GetByIdAdmin(int id)
    {
        var request = await _categoryRequestService.GetByIdAsync(id);
        if (request == null)
            return NotFound(new { message = "Talep bulunamadi" });

        return Ok(request);
    }
}
