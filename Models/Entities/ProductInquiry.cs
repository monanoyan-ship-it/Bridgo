using Bridgo.Models.Enums;

namespace Bridgo.Models.Entities;

/// <summary>
/// Urun fiyat/teklif istegi - Alici bir urun icin satıcıdan teklif ister
/// </summary>
public class ProductInquiry : BaseEntity
{
    // Hangi urun icin istek yapiliyor
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    // Isteği yapan alici (Buyer)
    public int BuyerVendorId { get; set; }
    public Vendor? BuyerVendor { get; set; }

    // Urunun sahibi satici (Seller) - Product'tan alinir ama denormalize ediyoruz
    public int SellerVendorId { get; set; }
    public Vendor? SellerVendor { get; set; }

    // Istek detaylari
    public int Quantity { get; set; }
    public string? Unit { get; set; } // Adet, Kg, vb.
    public string? Message { get; set; } // Alicinin notu/mesaji
    public string? SpecialRequirements { get; set; } // Ozel gereksinimler

    // Teslimat adresi (Addresses tablosuna referans)
    public int? DeliveryAddressId { get; set; }
    public Address? DeliveryAddress { get; set; }
    public DateTime? DesiredDeliveryDate { get; set; }

    // Satıcıya özel teklif bilgileri (satıcı hızlı cevap verebilir)
    public decimal? OfferedPrice { get; set; }
    public string? OfferedCurrency { get; set; }
    public DateTime? OfferValidUntil { get; set; }

    // Durum (ProductInquiryStatuses type class)
    public int Status { get; set; } = ProductInquiryStatuses.Pending.Id;

    // Gorulme/Okunma
    public bool IsReadBySeller { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Yanitlar
    public ICollection<ProductInquiryResponse> Responses { get; set; } = new List<ProductInquiryResponse>();
}
