using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.DTOs.Cart;
using Bridgo.Services.Interfaces;
using Bridgo.Data;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Sepet API Controller - Tum giris yapmis kullanicilar erisebilir
/// Onemli: Bir sepet tek satici + tek depodan urunler icerir
/// </summary>
[Authorize]
[ApiController]
[Route("api/cart")]
public class CartApiController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ApplicationDbContext _context;

    public CartApiController(ICartService cartService, ApplicationDbContext context)
    {
        _cartService = cartService;
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var userId = GetUserId();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user?.VendorId;
    }

    /// <summary>
    /// Tum aktif sepetleri getir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCarts()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized(new { message = "Vendor bulunamadi" });

        var carts = await _cartService.GetActiveCartsAsync(vendorId.Value, GetUserId());
        return Ok(carts);
    }

    /// <summary>
    /// Belirli bir sepeti getir
    /// </summary>
    [HttpGet("{cartId}")]
    public async Task<IActionResult> GetCart(int cartId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized(new { message = "Vendor bulunamadi" });

        var cart = await _cartService.GetCartByIdAsync(vendorId.Value, GetUserId(), cartId);
        if (cart == null)
            return NotFound(new { message = "Sepet bulunamadi" });

        return Ok(cart);
    }

    /// <summary>
    /// Sepete urun ekle
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized(new { message = "Vendor bulunamadi" });

        var result = await _cartService.AddToCartAsync(vendorId.Value, GetUserId(), dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Sepet kalemini guncelle
    /// </summary>
    [HttpPut("items/{id}")]
    public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized(new { message = "Vendor bulunamadi" });

        var result = await _cartService.UpdateCartItemAsync(vendorId.Value, GetUserId(), id, dto);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Sepet guncellendi" });
    }

    /// <summary>
    /// Sepetten urun kaldir
    /// </summary>
    [HttpDelete("items/{id}")]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized(new { message = "Vendor bulunamadi" });

        var result = await _cartService.RemoveFromCartAsync(vendorId.Value, GetUserId(), id);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Urun sepetten cikarildi" });
    }

    /// <summary>
    /// Belirli bir sepeti temizle
    /// </summary>
    [HttpDelete("{cartId}")]
    public async Task<IActionResult> ClearCart(int cartId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized(new { message = "Vendor bulunamadi" });

        var result = await _cartService.ClearCartAsync(vendorId.Value, GetUserId(), cartId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = "Sepet temizlendi" });
    }

    /// <summary>
    /// Tum sepetlerdeki toplam kalem sayisi
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetCartCount()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Ok(new { count = 0 });

        var count = await _cartService.GetCartItemCountAsync(vendorId.Value, GetUserId());
        return Ok(new { count });
    }

    /// <summary>
    /// Kullanicinin adreslerini getir (dropdown icin)
    /// </summary>
    [HttpGet("addresses")]
    public async Task<IActionResult> GetAddresses()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized(new { message = "Vendor bulunamadi" });

        var addresses = await _context.Addresses
            .Where(a => a.VendorId == vendorId && !a.IsDeleted && a.IsActive)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Title)
            .Select(a => new AddressSelectDto
            {
                Id = a.Id,
                Title = a.Title,
                FullAddress = a.AddressLine + ", " + a.City,
                IsDefault = a.IsDefault
            })
            .ToListAsync();

        return Ok(addresses);
    }

    /// <summary>
    /// Urun icin minimum siparis miktari ve fiyat hesapla
    /// </summary>
    [HttpGet("product-info/{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductCartInfo(int productId, [FromQuery] int quantity = 1)
    {
        var minQuantity = await _cartService.GetMinimumOrderQuantityAsync(productId);
        var effectiveQuantity = Math.Max(quantity, minQuantity);
        var unitPrice = await _cartService.CalculatePriceForQuantityAsync(productId, effectiveQuantity);
        var warehouseId = await _cartService.GetProductDefaultWarehouseIdAsync(productId);

        return Ok(new
        {
            minQuantity,
            unitPrice,
            totalPrice = unitPrice * effectiveQuantity,
            warehouseId
        });
    }

    /// <summary>
    /// Ayni depodan diger urunleri getir (cross-sell)
    /// </summary>
    [HttpGet("same-warehouse-products/{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSameWarehouseProducts(int productId, [FromQuery] int limit = 6)
    {
        var warehouseId = await _cartService.GetProductDefaultWarehouseIdAsync(productId);
        if (warehouseId == null)
            return Ok(new List<SameWarehouseProductDto>());

        var products = await _cartService.GetSameWarehouseProductsAsync(productId, warehouseId.Value, limit);
        return Ok(products);
    }
}
