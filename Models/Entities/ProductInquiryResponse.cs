using Bridgo.Models.Enums;

namespace Bridgo.Models.Entities;

/// <summary>
/// Urun istegine saticinin verdigi yanit/teklif
/// </summary>
public class ProductInquiryResponse : BaseEntity
{
    // Hangi istege yanit veriliyor
    public int InquiryId { get; set; }
    public ProductInquiry? Inquiry { get; set; }

    // Fiyat teklifi
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string Currency { get; set; } = "TRY";

    // Miktar (farkli miktar teklif edebilir)
    public int? OfferedQuantity { get; set; }
    public string? OfferedUnit { get; set; }

    // Teslim suresi
    public int? LeadTimeDays { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }

    // Gecerlilik
    public DateTime? ValidUntil { get; set; }

    // Satici notu
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }

    // Durum (ProductInquiryResponseStatuses type class)
    public int Status { get; set; } = ProductInquiryResponseStatuses.Pending.Id;

    // Alici tarafindan gorulme
    public bool IsReadByBuyer { get; set; } = false;
    public DateTime? ReadByBuyerAt { get; set; }
}
