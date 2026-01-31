using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

/// <summary>
/// Yatirimci teklifi
/// Yatirimci bir finansman talebine teklif verir
/// </summary>
public class InvestmentOffer : BaseEntity
{
    /// <summary>
    /// Iliskili finansman talebi
    /// </summary>
    public int FinancingRequestId { get; set; }

    /// <summary>
    /// Teklif veren yatirimci firma ID
    /// </summary>
    public int InvestorVendorId { get; set; }

    /// <summary>
    /// Teklif veren kullanici ID
    /// </summary>
    public int InvestorUserId { get; set; }

    // === TEKLIF DETAYLARI ===

    /// <summary>
    /// Teklif edilen tutar
    /// </summary>
    public decimal OfferedAmount { get; set; }

    /// <summary>
    /// Teklif edilen faiz orani (aylik %)
    /// </summary>
    public decimal InterestRate { get; set; }

    /// <summary>
    /// Toplam geri odeme tutari (anapara + faiz)
    /// </summary>
    public decimal TotalRepaymentAmount { get; set; }

    /// <summary>
    /// Teklif aciklamasi/kosullari
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Teklif gecerlilik tarihi
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    // === DURUM ===

    /// <summary>
    /// Teklif durumu (InvestmentOfferStatuses)
    /// 1: Pending (Beklemede)
    /// 2: Accepted (Kabul edildi)
    /// 3: Rejected (Reddedildi)
    /// 4: Withdrawn (Geri cekildi)
    /// 5: Expired (Suresi doldu)
    /// 6: Funded (Finanse edildi - para transferi tamamlandi)
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// Kabul/Red tarihi
    /// </summary>
    public DateTime? ResponseDate { get; set; }

    /// <summary>
    /// Kabul/Red yapan kullanici
    /// </summary>
    public int? ResponseByUserId { get; set; }

    /// <summary>
    /// Red/Geri cekilme nedeni
    /// </summary>
    public string? RejectionReason { get; set; }

    // === ODEME TAKIBI ===

    /// <summary>
    /// Transfer edildi mi? (Yatirimci -> Talep eden)
    /// </summary>
    public bool IsFundTransferred { get; set; }

    /// <summary>
    /// Transfer tarihi
    /// </summary>
    public DateTime? FundTransferDate { get; set; }

    /// <summary>
    /// Transfer referans numarasi
    /// </summary>
    public string? TransferReference { get; set; }

    /// <summary>
    /// Geri odeme yapildi mi? (Talep eden -> Yatirimci)
    /// </summary>
    public bool IsRepaid { get; set; }

    /// <summary>
    /// Geri odeme tarihi
    /// </summary>
    public DateTime? RepaymentDate { get; set; }

    /// <summary>
    /// Geri odeme referans numarasi
    /// </summary>
    public string? RepaymentReference { get; set; }

    // Navigation properties
    public virtual FinancingRequest? FinancingRequest { get; set; }
    public virtual Vendor? InvestorVendor { get; set; }
    public virtual ApplicationUser? InvestorUser { get; set; }
    public virtual ApplicationUser? ResponseByUser { get; set; }
}
