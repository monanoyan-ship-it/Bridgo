using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Cart;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Sepet yonetimi servisi implementation
/// Onemli: Bir sepet tek satici + tek depo + tek teslimat adresi icerir
/// </summary>
public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartsDto> GetActiveCartsAsync(int vendorId, int userId)
    {
        var carts = await _context.Carts
            .Include(c => c.SellerVendor)
            .Include(c => c.SourceWarehouse)
                .ThenInclude(w => w!.Address)
            .Include(c => c.DeliveryAddress)
            .Include(c => c.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images.Where(img => img.IsMain))
            .Where(c => c.VendorId == vendorId && c.UserId == userId && c.Status == CartStatus.Active && !c.IsDeleted)
            .Where(c => c.Items.Any(i => !i.IsDeleted)) // Bos sepetleri gosterme
            .OrderByDescending(c => c.LastUpdatedAt)
            .ToListAsync();

        var cartDtos = new List<CartDto>();

        foreach (var cart in carts)
        {
            var items = cart.Items.Where(i => !i.IsDeleted).Select(i => new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                ProductSlug = i.Product.Slug,
                ProductSku = i.Product.SKU,
                ProductImageUrl = i.Product.Images.FirstOrDefault()?.Url,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                Currency = i.Currency,
                Note = i.Note,
                InStock = i.Product.StockQuantity > 0 || i.Product.AllowBackorder,
                AvailableStock = i.Product.StockQuantity,
                MinQuantity = GetMinQuantitySync(i.Product)
            }).ToList();

            cartDtos.Add(new CartDto
            {
                Id = cart.Id,
                SellerVendorId = cart.SellerVendorId ?? 0,
                SellerVendorName = cart.SellerVendor?.CompanyName ?? "",
                SellerIsVerified = cart.SellerVendor?.IsVerified ?? false,
                SourceWarehouseId = cart.SourceWarehouseId ?? 0,
                SourceWarehouseName = cart.SourceWarehouse?.Name ?? "",
                SourceWarehouseCity = cart.SourceWarehouse?.Address?.City,
                DeliveryAddressId = cart.DeliveryAddressId ?? 0,
                DeliveryAddressTitle = cart.DeliveryAddress?.Title ?? "",
                DeliveryAddressFull = cart.DeliveryAddress?.FullAddress ?? "",
                Items = items,
                TotalItemCount = items.Sum(i => i.Quantity),
                TotalAmount = items.Sum(i => i.TotalPrice),
                Currency = items.FirstOrDefault()?.Currency ?? "TRY",
                LastUpdatedAt = cart.LastUpdatedAt
            });
        }

        return new CartsDto
        {
            Carts = cartDtos,
            TotalItemCount = cartDtos.Sum(c => c.TotalItemCount),
            TotalAmount = cartDtos.Sum(c => c.TotalAmount)
        };
    }

    public async Task<CartDto?> GetCartByIdAsync(int vendorId, int userId, int cartId)
    {
        var cart = await _context.Carts
            .Include(c => c.SellerVendor)
            .Include(c => c.SourceWarehouse)
                .ThenInclude(w => w!.Address)
            .Include(c => c.DeliveryAddress)
            .Include(c => c.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images.Where(img => img.IsMain))
            .FirstOrDefaultAsync(c => c.Id == cartId && c.VendorId == vendorId && c.UserId == userId && !c.IsDeleted);

        if (cart == null) return null;

        var items = cart.Items.Where(i => !i.IsDeleted).Select(i => new CartItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product.Name,
            ProductSlug = i.Product.Slug,
            ProductSku = i.Product.SKU,
            ProductImageUrl = i.Product.Images.FirstOrDefault()?.Url,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TotalPrice = i.TotalPrice,
            Currency = i.Currency,
            Note = i.Note,
            InStock = i.Product.StockQuantity > 0 || i.Product.AllowBackorder,
            AvailableStock = i.Product.StockQuantity,
            MinQuantity = GetMinQuantitySync(i.Product)
        }).ToList();

        return new CartDto
        {
            Id = cart.Id,
            SellerVendorId = cart.SellerVendorId ?? 0,
            SellerVendorName = cart.SellerVendor?.CompanyName ?? "",
            SellerIsVerified = cart.SellerVendor?.IsVerified ?? false,
            SourceWarehouseId = cart.SourceWarehouseId ?? 0,
            SourceWarehouseName = cart.SourceWarehouse?.Name ?? "",
            SourceWarehouseCity = cart.SourceWarehouse?.Address?.City,
            DeliveryAddressId = cart.DeliveryAddressId ?? 0,
            DeliveryAddressTitle = cart.DeliveryAddress?.Title ?? "",
            DeliveryAddressFull = cart.DeliveryAddress?.FullAddress ?? "",
            Items = items,
            TotalItemCount = items.Sum(i => i.Quantity),
            TotalAmount = items.Sum(i => i.TotalPrice),
            Currency = items.FirstOrDefault()?.Currency ?? "TRY",
            LastUpdatedAt = cart.LastUpdatedAt
        };
    }

    public async Task<AddToCartResultDto> AddToCartAsync(int vendorId, int userId, AddToCartDto dto)
    {
        // Urun kontrolu
        var product = await _context.Products
            .Include(p => p.PriceTiers)
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId && !p.IsDeleted && p.ProductStatusId == ProductStatuses.Active.Id);

        if (product == null)
            return new AddToCartResultDto { Success = false, Message = "Urun bulunamadi veya aktif degil." };

        // Adres kontrolu
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == dto.DeliveryAddressId && a.VendorId == vendorId && !a.IsDeleted);

        if (address == null)
            return new AddToCartResultDto { Success = false, Message = "Lutfen gecerli bir teslimat adresi secin." };

        // Minimum miktar kontrolu
        var minQuantity = await GetMinimumOrderQuantityAsync(dto.ProductId);
        if (dto.Quantity < minQuantity)
            return new AddToCartResultDto { Success = false, Message = $"Bu urun icin minimum siparis miktari {minQuantity} adettir." };

        // Urunun deposunu bul (varsayilan veya stoklu depo)
        var warehouseId = await GetProductDefaultWarehouseIdAsync(dto.ProductId);
        if (warehouseId == null)
        {
            // Depo yoksa satici vendor'un varsayilan deposunu kullan
            var sellerDefaultWarehouse = await _context.Warehouses
                .Where(w => w.VendorId == product.VendorId && w.IsDefault && !w.IsDeleted)
                .Select(w => w.Id)
                .FirstOrDefaultAsync();

            warehouseId = sellerDefaultWarehouse > 0 ? sellerDefaultWarehouse : null;
        }

        // Ayni satici + depo + adres icin mevcut sepeti bul
        var existingCart = await _context.Carts
            .Include(c => c.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(c =>
                c.VendorId == vendorId &&
                c.UserId == userId &&
                c.Status == CartStatus.Active &&
                c.SellerVendorId == product.VendorId &&
                c.SourceWarehouseId == warehouseId &&
                c.DeliveryAddressId == dto.DeliveryAddressId &&
                !c.IsDeleted);

        Cart cart;
        if (existingCart != null)
        {
            cart = existingCart;
        }
        else
        {
            // Yeni sepet olustur
            cart = new Cart
            {
                VendorId = vendorId,
                UserId = userId,
                Status = CartStatus.Active,
                SellerVendorId = product.VendorId,
                SourceWarehouseId = warehouseId,
                DeliveryAddressId = dto.DeliveryAddressId
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        // Ayni urun zaten sepette mi kontrol et
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId && !i.IsDeleted);

        // Fiyat hesapla
        var unitPrice = await CalculatePriceForQuantityAsync(dto.ProductId, dto.Quantity);

        if (existingItem != null)
        {
            // Miktari guncelle
            existingItem.Quantity = dto.Quantity;
            existingItem.UnitPrice = unitPrice;
            existingItem.Note = dto.Note;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Yeni kalem ekle
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                SellerVendorId = product.VendorId,
                SourceWarehouseId = warehouseId,
                Quantity = dto.Quantity,
                UnitPrice = unitPrice,
                Currency = product.Currency,
                Note = dto.Note
            };
            _context.CartItems.Add(cartItem);
        }

        cart.LastUpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Cross-sell: Ayni depodan diger urunleri getir
        var sameWarehouseProducts = warehouseId.HasValue
            ? await GetSameWarehouseProductsAsync(dto.ProductId, warehouseId.Value, 4)
            : new List<SameWarehouseProductDto>();

        return new AddToCartResultDto
        {
            Success = true,
            Message = "Urun sepete eklendi",
            CartId = cart.Id,
            HasSameWarehouseProducts = sameWarehouseProducts.Any(),
            SameWarehouseProducts = sameWarehouseProducts
        };
    }

    public async Task<ServiceResult> UpdateCartItemAsync(int vendorId, int userId, int cartItemId, UpdateCartItemDto dto)
    {
        var cartItem = await _context.CartItems
            .Include(i => i.Cart)
            .Include(i => i.Product)
                .ThenInclude(p => p.PriceTiers)
            .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.VendorId == vendorId && i.Cart.UserId == userId && !i.IsDeleted);

        if (cartItem == null)
            return ServiceResult.Fail("Sepet kalemi bulunamadi.");

        if (dto.Quantity.HasValue)
        {
            var minQuantity = await GetMinimumOrderQuantityAsync(cartItem.ProductId);
            if (dto.Quantity.Value < minQuantity)
                return ServiceResult.Fail($"Bu urun icin minimum siparis miktari {minQuantity} adettir.");

            cartItem.Quantity = dto.Quantity.Value;
            cartItem.UnitPrice = await CalculatePriceForQuantityAsync(cartItem.ProductId, dto.Quantity.Value);
        }

        if (dto.Note != null)
            cartItem.Note = dto.Note;

        cartItem.UpdatedAt = DateTime.UtcNow;
        cartItem.Cart.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RemoveFromCartAsync(int vendorId, int userId, int cartItemId)
    {
        var cartItem = await _context.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.VendorId == vendorId && i.Cart.UserId == userId && !i.IsDeleted);

        if (cartItem == null)
            return ServiceResult.Fail("Sepet kalemi bulunamadi.");

        cartItem.IsDeleted = true;
        cartItem.DeletedAt = DateTime.UtcNow;
        cartItem.Cart.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ClearCartAsync(int vendorId, int userId, int cartId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == cartId && c.VendorId == vendorId && c.UserId == userId && !c.IsDeleted);

        if (cart == null)
            return ServiceResult.Ok();

        foreach (var item in cart.Items)
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
        }

        cart.LastUpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<int> GetCartItemCountAsync(int vendorId, int userId)
    {
        return await _context.CartItems
            .Include(i => i.Cart)
            .Where(i => i.Cart.VendorId == vendorId && i.Cart.UserId == userId && i.Cart.Status == CartStatus.Active && !i.Cart.IsDeleted && !i.IsDeleted)
            .SumAsync(i => i.Quantity);
    }

    public async Task<decimal> CalculatePriceForQuantityAsync(int productId, int quantity)
    {
        var product = await _context.Products
            .Include(p => p.PriceTiers.OrderBy(pt => pt.MinQuantity))
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return 0;

        // Miktar bazli fiyat esigi bul
        var applicableTier = product.PriceTiers
            .Where(pt => quantity >= pt.MinQuantity && (pt.MaxQuantity == null || quantity <= pt.MaxQuantity))
            .OrderByDescending(pt => pt.MinQuantity)
            .FirstOrDefault();

        return applicableTier?.Price ?? product.Price;
    }

    public async Task<int> GetMinimumOrderQuantityAsync(int productId)
    {
        var product = await _context.Products
            .Include(p => p.PriceTiers.OrderBy(pt => pt.MinQuantity))
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return 1;

        // Eger fiyat esikleri varsa, en dusuk MinQuantity minimum siparis miktaridir
        var firstTier = product.PriceTiers.OrderBy(pt => pt.MinQuantity).FirstOrDefault();

        return firstTier?.MinQuantity ?? 1;
    }

    public async Task<List<SameWarehouseProductDto>> GetSameWarehouseProductsAsync(int productId, int warehouseId, int limit = 6)
    {
        // Deponun sahibi vendor'u bul
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == warehouseId && !w.IsDeleted);

        if (warehouse == null) return new List<SameWarehouseProductDto>();

        // Bu depodan stoklu diger urunleri getir
        var products = await _context.ProductWarehouseStocks
            .Include(pws => pws.Product)
                .ThenInclude(p => p.Images.Where(i => i.IsMain))
            .Where(pws =>
                pws.WarehouseId == warehouseId &&
                pws.ProductId != productId &&
                pws.Quantity > pws.ReservedQuantity &&
                !pws.Product.IsDeleted &&
                pws.Product.ProductStatusId == ProductStatuses.Active.Id)
            .OrderByDescending(pws => pws.Quantity - pws.ReservedQuantity)
            .Take(limit)
            .Select(pws => new SameWarehouseProductDto
            {
                Id = pws.Product.Id,
                Name = pws.Product.Name,
                Slug = pws.Product.Slug,
                ImageUrl = pws.Product.Images.FirstOrDefault()!.Url,
                Price = pws.Product.Price,
                Currency = pws.Product.Currency,
                InStock = true
            })
            .ToListAsync();

        return products;
    }

    public async Task<int?> GetProductDefaultWarehouseIdAsync(int productId)
    {
        // Urunun stoklu oldugu varsayilan depoyu bul
        var warehouseStock = await _context.ProductWarehouseStocks
            .Include(pws => pws.Warehouse)
            .Where(pws =>
                pws.ProductId == productId &&
                pws.Quantity > pws.ReservedQuantity &&
                !pws.Warehouse.IsDeleted)
            .OrderByDescending(pws => pws.Warehouse.IsDefault)
            .ThenByDescending(pws => pws.Quantity - pws.ReservedQuantity)
            .FirstOrDefaultAsync();

        return warehouseStock?.WarehouseId;
    }

    private int GetMinQuantitySync(Product product)
    {
        var firstTier = product.PriceTiers?.OrderBy(pt => pt.MinQuantity).FirstOrDefault();
        return firstTier?.MinQuantity ?? 1;
    }
}
