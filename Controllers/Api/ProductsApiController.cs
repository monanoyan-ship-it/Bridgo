using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Authorization;
using Bridgo.DTOs.Product;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Urun yonetimi API Controller
/// </summary>
[Authorize]
[ApiController]
[Route("api/products")]
[RequireCapability(VendorCapabilities.Seller)]
public class ProductsApiController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IAdminService _adminService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public ProductsApiController(
        IProductService productService,
        IAdminService adminService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment)
    {
        _productService = productService;
        _adminService = adminService;
        _userManager = userManager;
        _environment = environment;
    }

    #region Helper Methods

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = GetUserId();
        return userId == 0 ? null : await _userManager.FindByIdAsync(userId.ToString());
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.VendorId;
    }

    #endregion

    #region Public Search (Teklif yonetimi icin urun referansi)

    /// <summary>
    /// Platform genelinde aktif urunleri ara (Teklif referansi icin)
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<ProductSearchResultDto>>> SearchProducts(
        [FromQuery] string? search,
        [FromQuery] int pageSize = 10)
    {
        var products = await _productService.SearchProductsAsync(search, pageSize);
        return Ok(products);
    }

    #endregion

    #region Products

    /// <summary>
    /// Urunleri listele
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ProductListDto>>> GetProducts([FromQuery] ProductFilterDto? filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var products = await _productService.GetProductsAsync(vendorId.Value, filter);
        return Ok(products);
    }

    /// <summary>
    /// Urun detayi getir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var product = await _productService.GetProductByIdAsync(id, vendorId.Value);
        if (product == null)
            return NotFound(new { message = "Urun bulunamadi" });

        return Ok(product);
    }

    /// <summary>
    /// Yeni urun olustur
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateProduct([FromBody] ProductCreateUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.CreateProductAsync(dto, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { id = result.Data, message = result.Message });
    }

    /// <summary>
    /// Urunu guncelle
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProduct(int id, [FromBody] ProductCreateUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.UpdateProductAsync(id, dto, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Urunu sil
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.DeleteProductAsync(id, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Urun durumunu degistir
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateProductStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.UpdateProductStatusAsync(id, dto.ProductStatusId, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Toplu durum guncelleme
    /// </summary>
    [HttpPatch("bulk-status")]
    public async Task<ActionResult> BulkUpdateStatus([FromBody] BulkStatusDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.BulkUpdateStatusAsync(dto.ProductIds, dto.ProductStatusId, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Toplu silme
    /// </summary>
    [HttpDelete("bulk")]
    public async Task<ActionResult> BulkDelete([FromBody] BulkDeleteDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.BulkDeleteAsync(dto.ProductIds, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    #endregion

    #region Product Images

    /// <summary>
    /// Urune gorsel ekle (URL ile)
    /// </summary>
    [HttpPost("{productId}/images")]
    public async Task<ActionResult> AddImage(int productId, [FromBody] ProductImageCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.AddProductImageAsync(productId, dto, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { id = result.Data, message = result.Message });
    }

    /// <summary>
    /// Urune gorsel yukle (dosya ile)
    /// </summary>
    [HttpPost("{productId}/images/upload")]
    public async Task<ActionResult> UploadImage(int productId, IFormFile file)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya secilmedi" });

        // Dosya tipi kontrolu
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Gecersiz dosya tipi. Sadece JPG, PNG, GIF ve WebP desteklenir." });

        // Dosya boyutu kontrolu (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Dosya boyutu 5MB'dan buyuk olamaz" });

        try
        {
            // Klasor olustur
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products", vendorId.Value.ToString());
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Benzersiz dosya adi
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var uniqueFileName = $"{Guid.NewGuid():N}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Dosyayi kaydet
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // URL olustur
            var imageUrl = $"/uploads/products/{vendorId.Value}/{uniqueFileName}";

            // Veritabanina ekle
            var dto = new ProductImageCreateDto
            {
                Url = imageUrl,
                AltText = Path.GetFileNameWithoutExtension(file.FileName),
                IsMain = false
            };

            var result = await _productService.AddProductImageAsync(productId, dto, vendorId.Value);
            if (!result.Success)
            {
                // Basarisizsa dosyayi sil
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { id = result.Data, url = imageUrl, message = "Gorsel yuklendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Dosya yuklenirken hata olustu: " + ex.Message });
        }
    }

    /// <summary>
    /// Gorsel sil
    /// </summary>
    [HttpDelete("images/{imageId}")]
    public async Task<ActionResult> DeleteImage(int imageId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.DeleteProductImageAsync(imageId, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Ana gorsel sec
    /// </summary>
    [HttpPatch("images/{imageId}/main")]
    public async Task<ActionResult> SetMainImage(int imageId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.SetMainImageAsync(imageId, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Gorsel sirasini guncelle
    /// </summary>
    [HttpPatch("{productId}/images/order")]
    public async Task<ActionResult> UpdateImageOrder(int productId, [FromBody] UpdateImageOrderDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.UpdateImageOrderAsync(productId, dto.ImageIds, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    #endregion

    #region Price Tiers

    /// <summary>
    /// Urunun fiyat esiklerini getir
    /// </summary>
    [HttpGet("{productId}/price-tiers")]
    public async Task<ActionResult<List<ProductPriceTierDto>>> GetPriceTiers(int productId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var tiers = await _productService.GetPriceTiersAsync(productId, vendorId.Value);
        return Ok(tiers);
    }

    /// <summary>
    /// Fiyat esigi ekle
    /// </summary>
    [HttpPost("{productId}/price-tiers")]
    public async Task<ActionResult> AddPriceTier(int productId, [FromBody] ProductPriceTierCreateUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.AddPriceTierAsync(productId, dto, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { id = result.Data, message = result.Message });
    }

    /// <summary>
    /// Fiyat esigini guncelle
    /// </summary>
    [HttpPut("price-tiers/{tierId}")]
    public async Task<ActionResult> UpdatePriceTier(int tierId, [FromBody] ProductPriceTierCreateUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.UpdatePriceTierAsync(tierId, dto, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Fiyat esigini sil
    /// </summary>
    [HttpDelete("price-tiers/{tierId}")]
    public async Task<ActionResult> DeletePriceTier(int tierId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.DeletePriceTierAsync(tierId, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Urunun tum fiyat esiklerini kaydet (toplu guncelleme)
    /// </summary>
    [HttpPut("{productId}/price-tiers")]
    public async Task<ActionResult> SavePriceTiers(int productId, [FromBody] SavePriceTiersDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var result = await _productService.SavePriceTiersAsync(productId, dto.Tiers, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    #endregion

    #region Stock (Depo Bazli Stok Yonetimi)

    /// <summary>
    /// Urunun depo bazli stok bilgilerini getir
    /// </summary>
    [HttpGet("{productId}/stock")]
    public async Task<IActionResult> GetProductStock(int productId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized();

        var stocks = await _productService.GetProductStockAsync(productId, vendorId.Value);
        return Ok(stocks);
    }

    /// <summary>
    /// Urunun depo bazli stok bilgilerini kaydet
    /// </summary>
    [HttpPost("{productId}/stock")]
    public async Task<IActionResult> SaveProductStock(int productId, [FromBody] SaveProductStockDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized();

        var result = await _productService.SaveProductStockAsync(productId, dto.Stocks, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    #endregion

    #region Categories (Read-only - Global kategoriler Admin'de yonetiliyor)

    /// <summary>
    /// Tree gorunumu icin kategori listesi (Global - Admin tarafindan yonetiliyor)
    /// </summary>
    [HttpGet("categories/tree")]
    public async Task<IActionResult> GetCategoriesTree()
    {
        var categories = await _adminService.GetCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Dropdown icin kategori listesi (Global - Admin tarafindan yonetiliyor)
    /// </summary>
    [HttpGet("categories/select")]
    public async Task<ActionResult<List<ProductCategorySelectDto>>> GetCategoriesForSelect()
    {
        var categories = await _productService.GetCategoriesForSelectAsync();
        return Ok(categories);
    }

    #endregion

    #region Types (Dropdown icin tip listeleri)

    /// <summary>
    /// Urun durumlari listesi (lokalize edilmis)
    /// </summary>
    [HttpGet("types/statuses")]
    public ActionResult<List<TypeSelectDto>> GetProductStatuses()
    {
        var statuses = _productService.GetProductStatuses();
        return Ok(statuses);
    }

    #endregion

    #region Stock Overview (Stok Ozet Sayfasi)

    /// <summary>
    /// Stok ozet bilgilerini getir
    /// </summary>
    [HttpGet("stock/overview")]
    public async Task<IActionResult> GetStockOverview()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized();

        var overview = await _productService.GetStockOverviewAsync(vendorId.Value);
        return Ok(overview);
    }

    /// <summary>
    /// Dusuk stoklu urunleri getir
    /// </summary>
    [HttpGet("stock/low-stock")]
    public async Task<IActionResult> GetLowStockProducts()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized();

        var products = await _productService.GetLowStockProductsAsync(vendorId.Value);
        return Ok(products);
    }

    /// <summary>
    /// Depo bazli stok ozeti
    /// </summary>
    [HttpGet("stock/by-warehouse")]
    public async Task<IActionResult> GetStockByWarehouse()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized();

        var warehouseStocks = await _productService.GetStockByWarehouseAsync(vendorId.Value);
        return Ok(warehouseStocks);
    }

    #endregion

    #region Packaging (Paketleme Bilgileri)

    /// <summary>
    /// Urunun paketleme bilgilerini getir
    /// </summary>
    [HttpGet("{productId:int}/packaging")]
    public async Task<IActionResult> GetProductPackaging(int productId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var packagings = await _productService.GetProductPackagingAsync(productId, vendorId.Value);
        return Ok(packagings);
    }

    /// <summary>
    /// Urunun paketleme bilgilerini kaydet
    /// </summary>
    [HttpPost("{productId:int}/packaging")]
    public async Task<IActionResult> SaveProductPackaging(int productId, [FromBody] SaveProductPackagingDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var result = await _productService.SaveProductPackagingAsync(productId, dto.Packagings, vendorId.Value);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Paketleme bilgileri kaydedildi." });
    }

    #endregion
}

#region Request DTOs

public class UpdateStatusDto
{
    public int ProductStatusId { get; set; }
}

public class BulkStatusDto
{
    public List<int> ProductIds { get; set; } = new();
    public int ProductStatusId { get; set; }
}

public class BulkDeleteDto
{
    public List<int> ProductIds { get; set; } = new();
}

public class UpdateImageOrderDto
{
    public List<int> ImageIds { get; set; } = new();
}

public class SavePriceTiersDto
{
    public List<ProductPriceTierCreateUpdateDto> Tiers { get; set; } = new();
}

public class SaveProductStockDto
{
    public List<ProductWarehouseStockUpdateDto> Stocks { get; set; } = new();
}

public class SaveProductPackagingDto
{
    public List<ProductPackagingCreateUpdateDto> Packagings { get; set; } = new();
}

#endregion
