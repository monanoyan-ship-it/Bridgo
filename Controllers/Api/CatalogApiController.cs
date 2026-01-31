using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bridgo.DTOs.Catalog;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Public Catalog API (login gerektirmez)
/// </summary>
[ApiController]
[Route("api/catalog")]
[AllowAnonymous]
public class CatalogApiController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CatalogApiController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    // ============================================
    // PRODUCTS
    // ============================================

    /// <summary>
    /// Urunleri listele (filtreleme, sayfalama)
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] CatalogFilterDto filter)
    {
        var result = await _catalogService.GetProductsAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Urun detayi getir (slug ile)
    /// </summary>
    [HttpGet("products/{slug}")]
    public async Task<IActionResult> GetProductBySlug(string slug)
    {
        var product = await _catalogService.GetProductBySlugAsync(slug);
        if (product == null)
            return NotFound(new { message = "Urun bulunamadi." });
        return Ok(product);
    }

    /// <summary>
    /// Urun detayi getir (ID ile)
    /// </summary>
    [HttpGet("products/by-id/{id:int}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _catalogService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Urun bulunamadi." });
        return Ok(product);
    }

    /// <summary>
    /// Kategoriye gore urunler
    /// </summary>
    [HttpGet("products/category/{categorySlug}")]
    public async Task<IActionResult> GetProductsByCategory(string categorySlug, [FromQuery] CatalogFilterDto filter)
    {
        var result = await _catalogService.GetProductsByCategoryAsync(categorySlug, filter);
        return Ok(result);
    }

    /// <summary>
    /// Saticiya gore urunler
    /// </summary>
    [HttpGet("products/vendor/{vendorSlug}")]
    public async Task<IActionResult> GetProductsByVendor(string vendorSlug, [FromQuery] CatalogFilterDto filter)
    {
        var result = await _catalogService.GetProductsByVendorAsync(vendorSlug, filter);
        return Ok(result);
    }

    /// <summary>
    /// Benzer urunler
    /// </summary>
    [HttpGet("products/{id:int}/related")]
    public async Task<IActionResult> GetRelatedProducts(int id, [FromQuery] int count = 8)
    {
        var products = await _catalogService.GetRelatedProductsAsync(id, count);
        return Ok(products);
    }

    /// <summary>
    /// One cikan ve yeni urunler (anasayfa icin)
    /// </summary>
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured()
    {
        var result = await _catalogService.GetFeaturedAsync();
        return Ok(result);
    }

    // ============================================
    // CATEGORIES
    // ============================================

    /// <summary>
    /// Tum kategoriler (tree yapisi)
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _catalogService.GetCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Kategori detayi (slug ile)
    /// </summary>
    [HttpGet("categories/{slug}")]
    public async Task<IActionResult> GetCategoryBySlug(string slug)
    {
        var category = await _catalogService.GetCategoryBySlugAsync(slug);
        if (category == null)
            return NotFound(new { message = "Kategori bulunamadi." });
        return Ok(category);
    }

    /// <summary>
    /// Kategori breadcrumb
    /// </summary>
    [HttpGet("categories/{id:int}/breadcrumb")]
    public async Task<IActionResult> GetCategoryBreadcrumb(int id)
    {
        var breadcrumb = await _catalogService.GetCategoryBreadcrumbAsync(id);
        return Ok(breadcrumb);
    }

    // ============================================
    // STATS & SEARCH
    // ============================================

    /// <summary>
    /// Katalog istatistikleri
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _catalogService.GetStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Arama onerileri (autocomplete)
    /// </summary>
    [HttpGet("search/suggestions")]
    public async Task<IActionResult> GetSearchSuggestions([FromQuery] string q, [FromQuery] int count = 10)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new List<string>());

        var suggestions = await _catalogService.GetSearchSuggestionsAsync(q, count);
        return Ok(suggestions);
    }
}
