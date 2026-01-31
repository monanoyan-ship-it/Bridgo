using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers;

/// <summary>
/// Public Catalog sayfalari (SEO-optimized, login gerektirmez)
/// </summary>
public class CatalogController : Controller
{
    private readonly ICatalogService _catalogService;
    private readonly ApplicationDbContext _context;

    public CatalogController(ICatalogService catalogService, ApplicationDbContext context)
    {
        _catalogService = catalogService;
        _context = context;
    }

    private async Task<int?> GetCurrentUserVendorIdAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true) return null;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return null;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user?.VendorId;
    }

    /// <summary>
    /// Ana katalog sayfasi - Tum urunler
    /// /Catalog
    /// </summary>
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? search, int? categoryId, string? sort)
    {
        ViewData["Title"] = "Urun Katalogu | Bridgo B2B";
        ViewData["MetaDescription"] = "B2B urun katalogu. Binlerce urun arasinda arayin, tedarikcilerle iletisime gecin.";

        // Kategori bilgisini ViewBag'e ekle (sidebar icin)
        var categories = await _catalogService.GetCategoriesAsync();
        ViewBag.Categories = categories;

        // URL parametrelerini ViewBag'e ekle
        ViewBag.InitialSearch = search;
        ViewBag.InitialCategoryId = categoryId;
        ViewBag.InitialSort = sort;

        return View();
    }

    /// <summary>
    /// Urun detay sayfasi
    /// /Catalog/{slug}
    /// </summary>
    [AllowAnonymous]
    [Route("Catalog/{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var product = await _catalogService.GetProductBySlugAsync(slug, incrementViewCount: true);

        if (product == null)
            return NotFound();

        ViewData["Title"] = $"{product.MetaTitle ?? product.Name} | Bridgo B2B";
        ViewData["MetaDescription"] = product.MetaDescription ?? product.ShortDescription ?? product.Name;

        // Kategori bilgisini ViewBag'e ekle (sidebar icin)
        var categories = await _catalogService.GetCategoriesAsync();
        ViewBag.Categories = categories;

        // Kullanicinin VendorId'si (kendi urunu mu kontrolu icin)
        var currentUserVendorId = await GetCurrentUserVendorIdAsync();
        ViewBag.CurrentUserVendorId = currentUserVendorId;
        ViewBag.IsOwnProduct = currentUserVendorId.HasValue && currentUserVendorId.Value == product.VendorId;

        return View(product);
    }

    /// <summary>
    /// Kategori sayfasi - Kategoriye gore urunler
    /// /Catalog/Category/{slug}
    /// </summary>
    [AllowAnonymous]
    [Route("Catalog/Category/{slug}")]
    public async Task<IActionResult> Category(string slug)
    {
        var category = await _catalogService.GetCategoryBySlugAsync(slug);

        if (category == null)
            return NotFound();

        ViewData["Title"] = $"{category.Name} | Bridgo B2B";
        ViewData["MetaDescription"] = $"{category.Name} kategorisindeki B2B urunleri. Tedarikci bul, teklif al.";

        // Kategori bilgisini ViewBag'e ekle
        var categories = await _catalogService.GetCategoriesAsync();
        ViewBag.Categories = categories;
        ViewBag.CurrentCategory = category;

        // Breadcrumb
        var breadcrumb = await _catalogService.GetCategoryBreadcrumbAsync(category.Id);
        ViewBag.Breadcrumb = breadcrumb;

        return View();
    }

    /// <summary>
    /// Satici urunleri sayfasi
    /// /Catalog/Vendor/{slug}
    /// </summary>
    [AllowAnonymous]
    [Route("Catalog/Vendor/{slug}")]
    public async Task<IActionResult> Vendor(string slug)
    {
        ViewData["Title"] = "Satici Urunleri | Bridgo B2B";

        var categories = await _catalogService.GetCategoriesAsync();
        ViewBag.Categories = categories;
        ViewBag.VendorSlug = slug;

        return View("Index"); // Ayni view, farkli filtre
    }
}
