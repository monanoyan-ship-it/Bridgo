using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Alici talepleri/arayislari
/// Public olarak yayinlanabilir (SEO, Google indexleme)
/// Urun referansli veya serbest metin olabilir
/// </summary>
public class PublicDemand : BaseEntity
{
    // === TEMEL BILGILER ===
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;  // SEO-friendly URL

    [MaxLength(4000)]
    public string? Description { get; set; }

    // === MIKTAR VE BIRIM ===
    public int? Quantity { get; set; }

    [MaxLength(20)]
    public string? Unit { get; set; }  // Adet, Kg, Metre, Kutu, vb.

    // === KATEGORI ===
    public int? CategoryId { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }  // Virgul ile ayrilmis etiketler

    // === REFERANS URUN (Opsiyonel) ===
    /// <summary>
    /// Sistemdeki bir urune referans verilebilir
    /// "Buna benzer ama su ozellikleri farkli olsun" senaryosu
    /// </summary>
    public int? ReferenceProductId { get; set; }

    [MaxLength(2000)]
    public string? ModificationNotes { get; set; }  // Urun modifikasyon aciklamasi

    // === KONUM ===
    public int? CountryId { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    // === TESLIMAT ===
    public int? DesiredLeadTimeDays { get; set; }  // Istenen teslim suresi (gun)

    public DateTime? DesiredDeliveryDate { get; set; }  // Istenen teslim tarihi

    // === BUTCE ===
    public decimal? BudgetMin { get; set; }

    public decimal? BudgetMax { get; set; }

    [MaxLength(10)]
    public string? BudgetCurrency { get; set; } = "TRY";

    // === GORUNURLUK VE DURUM ===
    /// <summary>
    /// 1: Public (Tum dunya gorebilir, Google indexler)
    /// 2: Platform (Sadece kayitli ureticiler)
    /// 3: Invited (Sadece davet edilenler)
    /// </summary>
    public int Visibility { get; set; } = 1;  // Default: Public

    /// <summary>
    /// 1: Draft (Taslak)
    /// 2: Active (Aktif - teklif alinabilir)
    /// 3: Closed (Kapandi - teklif alinamaz)
    /// 4: Awarded (Birisi kazandi)
    /// 5: Expired (Suresi doldu)
    /// 6: Cancelled (Iptal edildi)
    /// </summary>
    public int Status { get; set; } = 1;  // Default: Draft

    public DateTime? ExpiresAt { get; set; }  // Son teklif tarihi

    public DateTime? PublishedAt { get; set; }  // Yayinlanma tarihi

    // === SEO ===
    public bool IsIndexable { get; set; } = true;  // Google indexlesin mi

    [MaxLength(200)]
    public string? MetaTitle { get; set; }

    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    // === ISTATISTIKLER ===
    public int ViewCount { get; set; } = 0;

    public int ResponseCount { get; set; } = 0;

    // === KISMI KARSILAMA (Multi-supplier) ===
    /// <summary>
    /// Kalan miktar (Kismi karsilama icin)
    /// Ilk deger = Quantity, her siparis kabul edildiginde azalir
    /// </summary>
    public int? RemainingQuantity { get; set; }

    /// <summary>
    /// Karsilama durumu (DemandFulfillmentStatuses)
    /// 0: Open, 1: PartiallyFulfilled, 2: FullyFulfilled
    /// </summary>
    public int FulfillmentStatus { get; set; } = 0;

    // === MULTI-TENANT ===
    public int VendorId { get; set; }

    // === NAVIGATION PROPERTIES ===
    public Vendor Vendor { get; set; } = null!;
    public ProductCategory? Category { get; set; }
    public Product? ReferenceProduct { get; set; }
    public Country? Country { get; set; }
    public ICollection<DemandResponse> Responses { get; set; } = new List<DemandResponse>();
    public ICollection<DemandAttachment> Attachments { get; set; } = new List<DemandAttachment>();
    public ICollection<DemandModification> Modifications { get; set; } = new List<DemandModification>();
}
