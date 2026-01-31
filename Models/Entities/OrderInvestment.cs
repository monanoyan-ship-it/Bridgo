namespace Bridgo.Models.Entities;

/// <summary>
/// Siparis yatirimi - Finansmana yardimci olmak isteyenler icin
/// Alici siparisi karsilayamiyorsa yatirimci ortaklar destek olabilir
/// </summary>
public class OrderInvestment : BaseEntity
{
    /// <summary>
    /// Siparis ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Yatirimci firma ID
    /// </summary>
    public int InvestorVendorId { get; set; }

    /// <summary>
    /// Yatirim tutari
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Para birimi
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Toplam siparis tutarinin yuzdesi
    /// </summary>
    public decimal PercentageOfTotal { get; set; }

    /// <summary>
    /// Yatirim tipi (InvestmentTypes)
    /// 1: Equity (Kar ortakligi)
    /// 2: Loan (Kredi - faizli)
    /// 3: PrePayment (On odeme - indirimli aliyor)
    /// </summary>
    public int InvestmentType { get; set; }

    /// <summary>
    /// Kar/Faiz orani (%)
    /// </summary>
    public decimal? ReturnRate { get; set; }

    /// <summary>
    /// Beklenen geri odeme tutari
    /// </summary>
    public decimal? ExpectedReturn { get; set; }

    /// <summary>
    /// Geri odeme tarihi
    /// </summary>
    public DateTime? RepaymentDueDate { get; set; }

    // === DURUM ===

    /// <summary>
    /// Yatirim durumu (InvestmentStatuses)
    /// 1: Pending (Onay bekliyor)
    /// 2: Active (Aktif)
    /// 3: Repaid (Geri odendi)
    /// 4: Defaulted (Temerrut)
    /// 5: Cancelled (Iptal)
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// Yatirim yatirildı mi
    /// </summary>
    public bool IsFunded { get; set; }

    /// <summary>
    /// Yatirim tarihi
    /// </summary>
    public DateTime? FundedAt { get; set; }

    /// <summary>
    /// Geri odeme yapildi mi
    /// </summary>
    public bool IsRepaid { get; set; }

    /// <summary>
    /// Geri odeme tarihi
    /// </summary>
    public DateTime? RepaidAt { get; set; }

    /// <summary>
    /// Geri odenen tutar
    /// </summary>
    public decimal? RepaidAmount { get; set; }

    /// <summary>
    /// Sartlar ve kosullar
    /// </summary>
    public string? TermsAndConditions { get; set; }

    /// <summary>
    /// Notlar
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual Vendor? InvestorVendor { get; set; }
}
