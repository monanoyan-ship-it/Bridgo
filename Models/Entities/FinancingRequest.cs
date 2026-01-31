namespace Bridgo.Models.Entities;

/// <summary>
/// Finansman talebi (Fatura, Siparis, Proje)
/// Satici/Alici yatirimcilardan finansman talep eder
/// </summary>
public class FinancingRequest : BaseEntity
{
    /// <summary>
    /// Talep eden firma ID
    /// </summary>
    public int RequesterVendorId { get; set; }

    /// <summary>
    /// Finansman tipi (FinancingTypes)
    /// 1: Invoice (Fatura Finansmani)
    /// 2: Order (Siparis Finansmani)
    /// 3: Project (Proje Finansmani)
    /// </summary>
    public int FinancingType { get; set; }

    /// <summary>
    /// Finansman konusu (FinancingSubjects)
    /// 1: ProductCost (Mal Bedeli)
    /// 2: Logistics (Lojistik/Navlun)
    /// 3: Insurance (Sigorta)
    /// 4: ExportCustoms (Ihracat Gumruk)
    /// 5: ImportCustoms (Ithalat Gumruk)
    /// </summary>
    public int? FinancingSubject { get; set; }

    /// <summary>
    /// Talep basligi
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detayli aciklama
    /// </summary>
    public string? Description { get; set; }

    // === TUTAR BILGILERI ===

    /// <summary>
    /// Talep edilen tutar
    /// </summary>
    public decimal RequestedAmount { get; set; }

    /// <summary>
    /// Para birimi
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Toplam fatura/siparis degeri (teminat icin)
    /// </summary>
    public decimal? TotalValue { get; set; }

    /// <summary>
    /// Kabul edilen maksimum faiz orani (aylik %)
    /// </summary>
    public decimal? MaxInterestRate { get; set; }

    // === VADE ===

    /// <summary>
    /// Finansman suresi (gun)
    /// </summary>
    public int DurationDays { get; set; }

    /// <summary>
    /// Geri odeme tarihi
    /// </summary>
    public DateTime? RepaymentDate { get; set; }

    // === TEMINAT ===

    /// <summary>
    /// Teminat tipi (CollateralTypes)
    /// 1: Invoice (Fatura)
    /// 2: Order (Siparis)
    /// 3: Inventory (Stok)
    /// 4: Receivables (Alacaklar)
    /// 5: None (Teminatsiz)
    /// </summary>
    public int? CollateralType { get; set; }

    /// <summary>
    /// Teminat aciklamasi
    /// </summary>
    public string? CollateralDescription { get; set; }

    /// <summary>
    /// Iliskili siparis ID (siparis finansmani icin)
    /// </summary>
    public int? RelatedOrderId { get; set; }

    /// <summary>
    /// Fatura numarasi (fatura finansmani icin)
    /// </summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Fatura tarihi
    /// </summary>
    public DateTime? InvoiceDate { get; set; }

    /// <summary>
    /// Fatura borcluru (alici firma)
    /// </summary>
    public string? DebtorName { get; set; }

    /// <summary>
    /// Borclu vergi no
    /// </summary>
    public string? DebtorTaxNumber { get; set; }

    // === RISK ===

    /// <summary>
    /// Platform tarafindan hesaplanan risk skoru (0-100)
    /// </summary>
    public int? RiskScore { get; set; }

    /// <summary>
    /// Risk notu
    /// </summary>
    public string? RiskNotes { get; set; }

    // === TARIHLER ===

    /// <summary>
    /// Teklif son tarihi
    /// </summary>
    public DateTime? OfferDeadline { get; set; }

    // === DURUM ===

    /// <summary>
    /// Talep durumu (FinancingRequestStatuses)
    /// 1: Draft (Taslak)
    /// 2: Open (Teklif bekleniyor)
    /// 3: OffersReceived (Teklifler geldi)
    /// 4: PartiallyFunded (Kismi finanse edildi)
    /// 5: FullyFunded (Tam finanse edildi)
    /// 6: Repaid (Geri odendi)
    /// 7: Defaulted (Temerrut)
    /// 8: Cancelled (Iptal)
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// Finanse edilen toplam tutar
    /// </summary>
    public decimal FundedAmount { get; set; }

    // Navigation properties
    public virtual Vendor? RequesterVendor { get; set; }
    public virtual Order? RelatedOrder { get; set; }
    public virtual ICollection<InvestmentOffer> Offers { get; set; } = new List<InvestmentOffer>();
}
