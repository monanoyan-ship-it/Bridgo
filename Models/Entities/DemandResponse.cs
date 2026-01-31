using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Talebe verilen teklif/yanit
/// Kayitli ureticiler veya dis ureticiler (kayit olmadan) teklif verebilir
/// </summary>
public class DemandResponse : BaseEntity
{
    // === ILISKILER ===
    public int DemandId { get; set; }

    /// <summary>
    /// Teklif veren firma (sistemde kayitli ise)
    /// Null ise disaridan gelen teklif (ExternalSupplier bilgileri kullanilir)
    /// </summary>
    public int? SupplierVendorId { get; set; }

    // === DIS TEDARIKCI BILGILERI (Kayitli degilse) ===
    [MaxLength(200)]
    public string? ExternalCompanyName { get; set; }

    [MaxLength(200)]
    public string? ExternalContactName { get; set; }

    [MaxLength(200)]
    public string? ExternalEmail { get; set; }

    [MaxLength(50)]
    public string? ExternalPhone { get; set; }

    [MaxLength(500)]
    public string? ExternalWebsite { get; set; }

    // === TEKLIF DETAYLARI ===
    public decimal? UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    [MaxLength(10)]
    public string? Currency { get; set; } = "TRY";

    public int? Quantity { get; set; }  // Teklif edilen miktar

    [MaxLength(20)]
    public string? Unit { get; set; }

    public int? LeadTimeDays { get; set; }  // Teslim suresi (gun)

    public DateTime? ValidUntil { get; set; }  // Teklif gecerlilik tarihi

    // === ACIKLAMA VE NOTLAR ===
    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(1000)]
    public string? TermsAndConditions { get; set; }

    // === DURUM ===
    /// <summary>
    /// 1: Pending (Beklemede - henuz incelenmedi)
    /// 2: Viewed (Goruldu)
    /// 3: Shortlisted (Kisa listeye alindi)
    /// 4: Accepted (Kabul edildi)
    /// 5: Rejected (Reddedildi)
    /// </summary>
    public int Status { get; set; } = 1;  // Default: Pending

    public DateTime? ViewedAt { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    // === PAZARLIK ALANLARI ===

    /// <summary>
    /// Pazarlik aktif mi?
    /// </summary>
    public bool IsNegotiationActive { get; set; } = false;

    /// <summary>
    /// Simdi kimin sirasi? (Null ise pazarlik yok/bitti)
    /// Buyer veya Supplier VendorId
    /// </summary>
    public int? CurrentTurnVendorId { get; set; }

    /// <summary>
    /// Aktif tur numarasi (0 = pazarlik baslamadi)
    /// </summary>
    public int CurrentRoundNumber { get; set; } = 0;

    /// <summary>
    /// Son pazarlik turu bitis zamani
    /// </summary>
    public DateTime? NegotiationExpiresAt { get; set; }

    // === NAVIGATION PROPERTIES ===
    public PublicDemand Demand { get; set; } = null!;
    public Vendor? SupplierVendor { get; set; }
    public Vendor? CurrentTurnVendor { get; set; }
    public ICollection<DemandResponseAttachment> Attachments { get; set; } = new List<DemandResponseAttachment>();
    public ICollection<NegotiationRound> NegotiationRounds { get; set; } = new List<NegotiationRound>();
}
