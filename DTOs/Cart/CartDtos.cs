namespace Bridgo.DTOs.Cart;

/// <summary>
/// Tum sepetleri iceren DTO (coklu sepet destegi)
/// </summary>
public class CartsDto
{
    public List<CartDto> Carts { get; set; } = new();
    public int TotalItemCount { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Sepet DTO - tek satici + tek depo
/// </summary>
public class CartDto
{
    public int Id { get; set; }

    /// <summary>
    /// Satici firma
    /// </summary>
    public int SellerVendorId { get; set; }
    public string SellerVendorName { get; set; } = string.Empty;
    public bool SellerIsVerified { get; set; }

    /// <summary>
    /// Kaynak depo
    /// </summary>
    public int SourceWarehouseId { get; set; }
    public string SourceWarehouseName { get; set; } = string.Empty;
    public string? SourceWarehouseCity { get; set; }

    /// <summary>
    /// Teslimat adresi
    /// </summary>
    public int DeliveryAddressId { get; set; }
    public string DeliveryAddressTitle { get; set; } = string.Empty;
    public string DeliveryAddressFull { get; set; } = string.Empty;

    public List<CartItemDto> Items { get; set; } = new();
    public int TotalItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime? LastUpdatedAt { get; set; }
}

/// <summary>
/// Sepet kalemi DTO
/// </summary>
public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSlug { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public string? Note { get; set; }
    public int MinQuantity { get; set; } = 1;
    public bool InStock { get; set; }
    public int AvailableStock { get; set; }
}

/// <summary>
/// Sepete ekleme DTO
/// </summary>
public class AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int DeliveryAddressId { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// Sepete ekleme sonucu DTO
/// </summary>
public class AddToCartResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int CartId { get; set; }

    /// <summary>
    /// Ayni depodan baska urunler var mi? (cross-sell icin)
    /// </summary>
    public bool HasSameWarehouseProducts { get; set; }

    /// <summary>
    /// Ayni depodan diger urunler (cross-sell)
    /// </summary>
    public List<SameWarehouseProductDto> SameWarehouseProducts { get; set; } = new();
}

/// <summary>
/// Ayni depodan urun onerisi (cross-sell)
/// </summary>
public class SameWarehouseProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public bool InStock { get; set; }
}

/// <summary>
/// Sepet kalemi guncelleme DTO
/// </summary>
public class UpdateCartItemDto
{
    public int? Quantity { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// Adres secim listesi icin DTO
/// </summary>
public class AddressSelectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

/// <summary>
/// Sepet uyumsuzluk durumu - farkli satici/depodan urun ekleme girisimi
/// </summary>
public class CartConflictDto
{
    public bool HasConflict { get; set; }
    public string ConflictType { get; set; } = string.Empty; // "different_seller", "different_warehouse", "different_address"
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Mevcut sepet bilgisi
    /// </summary>
    public int ExistingCartId { get; set; }
    public string ExistingSellerName { get; set; } = string.Empty;
    public string ExistingWarehouseName { get; set; } = string.Empty;

    /// <summary>
    /// Yeni urunun bilgisi
    /// </summary>
    public string NewSellerName { get; set; } = string.Empty;
    public string NewWarehouseName { get; set; } = string.Empty;
}
