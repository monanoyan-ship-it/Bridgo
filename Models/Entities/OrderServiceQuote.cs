namespace Bridgo.Models.Entities;

/// <summary>
/// Servis saglayici teklifi (Lojistik, Gumruk, Sigorta firmalari)
/// </summary>
public class OrderServiceQuote : BaseEntity
{
    /// <summary>
    /// Servis talebi ID
    /// </summary>
    public int ServiceRequestId { get; set; }

    /// <summary>
    /// Teklif veren firma ID
    /// </summary>
    public int ProviderVendorId { get; set; }

    // === FIYATLANDIRMA ===

    /// <summary>
    /// Teklif fiyati
    /// </summary>
    public decimal QuoteAmount { get; set; }

    /// <summary>
    /// Para birimi
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Fiyata dahil hizmetler
    /// </summary>
    public string? IncludedServices { get; set; }

    /// <summary>
    /// Ek masraflar aciklamasi
    /// </summary>
    public string? AdditionalCosts { get; set; }

    // === SURE ===

    /// <summary>
    /// Tahmini sure (gun)
    /// </summary>
    public int? EstimatedDays { get; set; }

    // === DETAYLAR ===

    /// <summary>
    /// Teklif notlari
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Sartlar ve kosullar
    /// </summary>
    public string? TermsAndConditions { get; set; }

    /// <summary>
    /// Teklif gecerlilik tarihi
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    // === LOJISTIK OZEL ===

    /// <summary>
    /// Tasima modlari (Road, Sea, Air vb.) - virgul ile ayrilmis ID'ler
    /// Ornek: "1,2,3" = Karayolu, Denizyolu, Havayolu
    /// </summary>
    public string? TransportModes { get; set; }

    /// <summary>
    /// Kargo firmasi/hat
    /// </summary>
    public string? CarrierName { get; set; }

    /// <summary>
    /// Aktarma sayisi
    /// </summary>
    public int? TransitStops { get; set; }

    // === SIGORTA OZEL ===

    /// <summary>
    /// Teminat tutari
    /// </summary>
    public decimal? CoverageAmount { get; set; }

    /// <summary>
    /// Sigorta kapsamı
    /// </summary>
    public string? CoverageDetails { get; set; }

    /// <summary>
    /// Muafiyet yuzdesi (%)
    /// </summary>
    public decimal? DeductiblePercent { get; set; }

    /// <summary>
    /// Muafiyet tutari (eski alan, artik DeductiblePercent kullaniliyor)
    /// </summary>
    [Obsolete("DeductiblePercent kullanin")]
    public decimal? Deductible { get; set; }

    // === GOZETIM OZEL ===

    /// <summary>
    /// Gozetim tipleri - virgul ile ayrilmis ID'ler
    /// 1: PreLoading, 2: Loading, 3: Unloading, 4: DamageAssessment
    /// </summary>
    public string? SurveyTypes { get; set; }

    // === DURUM ===

    /// <summary>
    /// Teklif durumu (ServiceQuoteStatuses)
    /// 1: Pending (Degerlendirilmeyi bekliyor)
    /// 2: Accepted (Kabul edildi)
    /// 3: Rejected (Reddedildi)
    /// 4: Expired (Suresi doldu)
    /// 5: Withdrawn (Geri cekildi)
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// Red nedeni
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Alici tarafindan okundu mu
    /// </summary>
    public bool IsReadByBuyer { get; set; }

    /// <summary>
    /// Okunma tarihi
    /// </summary>
    public DateTime? ReadByBuyerAt { get; set; }

    // Navigation properties
    public virtual OrderServiceRequest? ServiceRequest { get; set; }
    public virtual Vendor? ProviderVendor { get; set; }
}
