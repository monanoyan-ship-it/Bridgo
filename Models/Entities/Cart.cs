using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Kullanici sepeti - VendorId bazli (alici firma)
/// Onemli: Bir sepetteki tum urunler ayni satici ve ayni depodan gelmek zorundadir
/// </summary>
public class Cart : BaseEntity
{
    /// <summary>
    /// Sepet sahibi firma (alici)
    /// </summary>
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;

    /// <summary>
    /// Sepeti olusturan kullanici
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Sepet durumu
    /// </summary>
    public CartStatus Status { get; set; } = CartStatus.Active;

    /// <summary>
    /// Satici firma (tum urunler bu saticinan) - ilk urun eklendiginde set edilir
    /// </summary>
    public int? SellerVendorId { get; set; }
    public Vendor? SellerVendor { get; set; }

    /// <summary>
    /// Kaynak depo (tum urunler bu depodan) - ilk urun eklendiginde set edilir
    /// </summary>
    public int? SourceWarehouseId { get; set; }
    public Warehouse? SourceWarehouse { get; set; }

    /// <summary>
    /// Teslimat adresi (tum urunler icin ortak)
    /// </summary>
    public int? DeliveryAddressId { get; set; }
    public Address? DeliveryAddress { get; set; }

    /// <summary>
    /// Sepet kalemleri
    /// </summary>
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    /// <summary>
    /// Son guncelleme tarihi
    /// </summary>
    public DateTime? LastUpdatedAt { get; set; }
}

/// <summary>
/// Sepet kalemi
/// </summary>
public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    /// <summary>
    /// Urun
    /// </summary>
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Satici firma (Cart seviyesinde de var, denormalize)
    /// </summary>
    public int SellerVendorId { get; set; }
    public Vendor SellerVendor { get; set; } = null!;

    /// <summary>
    /// Kaynak depo - urunun geldigi depo
    /// </summary>
    public int? SourceWarehouseId { get; set; }
    public Warehouse? SourceWarehouse { get; set; }

    /// <summary>
    /// Miktar
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Eklenme anindaki birim fiyat (kademe fiyati uygulanmis)
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Para birimi
    /// </summary>
    [MaxLength(3)]
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Teslimat adresi (legacy - artik Cart seviyesinde)
    /// </summary>
    public int? DeliveryAddressId { get; set; }
    public Address? DeliveryAddress { get; set; }

    /// <summary>
    /// Kullanici notu
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Toplam tutar
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;
}

/// <summary>
/// Sepet durumu
/// </summary>
public enum CartStatus
{
    Active = 1,      // Aktif sepet
    Abandoned = 2,   // Terk edilmis
    Converted = 3    // Siparise donusturulmus
}
