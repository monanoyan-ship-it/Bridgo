using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Pazarlik turu - Her karsi teklif bir tur olusturur
/// Buyer ve Seller arasinda cift yonlu pazarlik
/// </summary>
public class NegotiationRound : BaseEntity
{
    // === ILISKILER ===

    /// <summary>
    /// Hangi DemandResponse icin pazarlik yapiliyor
    /// </summary>
    public int DemandResponseId { get; set; }
    public DemandResponse DemandResponse { get; set; } = null!;

    /// <summary>
    /// Bu turu baslatan (karsi teklif veren) firma
    /// Buyer veya Supplier olabilir
    /// </summary>
    public int InitiatorVendorId { get; set; }
    public Vendor Initiator { get; set; } = null!;

    /// <summary>
    /// Tur numarasi (1, 2, 3, ...)
    /// </summary>
    public int RoundNumber { get; set; }

    // === TEKLIF DETAYLARI ===

    /// <summary>
    /// Teklif edilen birim fiyat
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// Teklif edilen toplam fiyat
    /// </summary>
    public decimal? TotalPrice { get; set; }

    /// <summary>
    /// Para birimi
    /// </summary>
    [MaxLength(10)]
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Teklif edilen miktar
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// Birim
    /// </summary>
    [MaxLength(20)]
    public string? Unit { get; set; }

    /// <summary>
    /// Teslim suresi (gun)
    /// </summary>
    public int? LeadTimeDays { get; set; }

    /// <summary>
    /// Teklif gecerlilik tarihi
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Bu tur icin not/aciklama
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Kosullar
    /// </summary>
    [MaxLength(1000)]
    public string? TermsAndConditions { get; set; }

    // === DURUM ===

    /// <summary>
    /// Tur durumu (NegotiationRoundStatuses)
    /// 1: Active (Yanit bekleniyor)
    /// 2: Countered (Karsi teklif verildi - yeni tur basladi)
    /// 3: Accepted (Kabul edildi)
    /// 4: Rejected (Reddedildi)
    /// 5: Expired (48 saat gecti)
    /// 6: Withdrawn (Geri cekildi)
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// Yanit verilme zamani
    /// </summary>
    public DateTime? RespondedAt { get; set; }

    /// <summary>
    /// Bu tur icin son yanit tarihi (CreatedAt + 48 saat)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Red nedeni (reddedilmisse)
    /// </summary>
    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}
