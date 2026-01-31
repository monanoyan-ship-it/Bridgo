using Bridgo.DTOs.Cart;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Sepet yonetimi servisi
/// Onemli: Bir sepet tek satici + tek depodan urunler icerir
/// Farkli satici/depodan urunler farkli sepetlere eklenir
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Kullanicinin tum aktif sepetlerini getir
    /// </summary>
    Task<CartsDto> GetActiveCartsAsync(int vendorId, int userId);

    /// <summary>
    /// Belirli bir sepeti getir
    /// </summary>
    Task<CartDto?> GetCartByIdAsync(int vendorId, int userId, int cartId);

    /// <summary>
    /// Sepete urun ekle
    /// Ayni satici + depo + adres icin mevcut sepete ekler
    /// Farkli ise yeni sepet olusturur
    /// </summary>
    Task<AddToCartResultDto> AddToCartAsync(int vendorId, int userId, AddToCartDto dto);

    /// <summary>
    /// Sepet kalemini guncelle
    /// </summary>
    Task<ServiceResult> UpdateCartItemAsync(int vendorId, int userId, int cartItemId, UpdateCartItemDto dto);

    /// <summary>
    /// Sepetten urun kaldir
    /// </summary>
    Task<ServiceResult> RemoveFromCartAsync(int vendorId, int userId, int cartItemId);

    /// <summary>
    /// Belirli bir sepeti temizle
    /// </summary>
    Task<ServiceResult> ClearCartAsync(int vendorId, int userId, int cartId);

    /// <summary>
    /// Tum sepetlerdeki toplam urun sayisi
    /// </summary>
    Task<int> GetCartItemCountAsync(int vendorId, int userId);

    /// <summary>
    /// Urun icin uygun fiyati hesapla (kademe fiyatlandirmasi)
    /// </summary>
    Task<decimal> CalculatePriceForQuantityAsync(int productId, int quantity);

    /// <summary>
    /// Urun icin minimum siparis miktarini getir
    /// </summary>
    Task<int> GetMinimumOrderQuantityAsync(int productId);

    /// <summary>
    /// Ayni satici + depodan diger urunleri getir (cross-sell)
    /// </summary>
    Task<List<SameWarehouseProductDto>> GetSameWarehouseProductsAsync(int productId, int warehouseId, int limit = 6);

    /// <summary>
    /// Urunun varsayilan deposunu getir
    /// </summary>
    Task<int?> GetProductDefaultWarehouseIdAsync(int productId);
}
