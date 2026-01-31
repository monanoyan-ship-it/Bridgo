namespace Bridgo.Models.Entities;

/// <summary>
/// Siparis servis talebi (Lojistik, Gumruk, Sigorta)
/// Buyer servis saglayicilardan teklif toplar
/// </summary>
public class OrderServiceRequest : BaseEntity
{
    /// <summary>
    /// Siparis ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Servis tipi (ServiceTypes enum)
    /// 1: Logistics (Lojistik/Tasima)
    /// 2: Customs (Gumruk)
    /// 3: Insurance (Sigorta)
    /// </summary>
    public int ServiceType { get; set; }

    /// <summary>
    /// Talep basligi
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detayli aciklama/gereksinimler
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Yukun agirligi (kg)
    /// </summary>
    public decimal? WeightKg { get; set; }

    /// <summary>
    /// Yukun hacmi (m3)
    /// </summary>
    public decimal? VolumeM3 { get; set; }

    /// <summary>
    /// Paket sayisi
    /// </summary>
    public int? PackageCount { get; set; }

    /// <summary>
    /// Yukun degeri (sigorta icin)
    /// </summary>
    public decimal? CargoValue { get; set; }

    /// <summary>
    /// Para birimi
    /// </summary>
    public string? Currency { get; set; } = "TRY";

    // === LOKASYONLAR ===

    /// <summary>
    /// Cikis ulkesi
    /// </summary>
    public int? OriginCountryId { get; set; }

    /// <summary>
    /// Cikis sehri
    /// </summary>
    public string? OriginCity { get; set; }

    /// <summary>
    /// Cikis adresi
    /// </summary>
    public string? OriginAddress { get; set; }

    /// <summary>
    /// Varis ulkesi
    /// </summary>
    public int? DestinationCountryId { get; set; }

    /// <summary>
    /// Varis sehri
    /// </summary>
    public string? DestinationCity { get; set; }

    /// <summary>
    /// Varis adresi
    /// </summary>
    public string? DestinationAddress { get; set; }

    // === LOJISTIK OZEL ===

    /// <summary>
    /// Tasima modu (TransportModes)
    /// 1: Road (Kara), 2: Sea (Deniz), 3: Air (Hava), 4: Rail (Demir), 5: Multimodal
    /// </summary>
    public int? TransportMode { get; set; }

    /// <summary>
    /// Incoterms (EXW, FOB, CIF, DDP vb.)
    /// </summary>
    public string? Incoterms { get; set; }

    // === GUMRUK OZEL ===

    /// <summary>
    /// Gumruk islem tipi (CustomsOperationTypes)
    /// 1: Export (Ihracat), 2: Import (Ithalat), 3: Transit
    /// </summary>
    public int? CustomsOperationType { get; set; }

    /// <summary>
    /// GTiP/HS kodu
    /// </summary>
    public string? HsCode { get; set; }

    // === SIGORTA OZEL ===

    /// <summary>
    /// Sigorta tipi (InsuranceTypes)
    /// 1: Cargo (Yuk), 2: Liability (Sorumluluk), 3: AllRisk (Tam Koruma)
    /// </summary>
    public int? InsuranceType { get; set; }

    // === GOZETIM OZEL ===

    /// <summary>
    /// Gozetim tipleri (SurveyTypes) - virgul ile ayrilmis ID'ler
    /// 1: PreLoading, 2: Loading, 3: Unloading, 4: DamageAssessment
    /// </summary>
    public string? SurveyTypes { get; set; }

    /// <summary>
    /// Gozetim lokasyonu
    /// </summary>
    public string? SurveyLocation { get; set; }

    /// <summary>
    /// Tercih edilen gozetim tarihi
    /// </summary>
    public DateTime? PreferredSurveyDate { get; set; }

    // === TARIHLER ===

    /// <summary>
    /// Istenen yukleme tarihi
    /// </summary>
    public DateTime? DesiredPickupDate { get; set; }

    /// <summary>
    /// Istenen teslim tarihi
    /// </summary>
    public DateTime? DesiredDeliveryDate { get; set; }

    /// <summary>
    /// Teklif son tarihi
    /// </summary>
    public DateTime? QuoteDeadline { get; set; }

    // === DURUM ===

    /// <summary>
    /// Talep durumu (ServiceRequestStatuses)
    /// 1: Open (Teklif bekleniyor)
    /// 2: QuotesReceived (Teklifler geldi)
    /// 3: QuoteSelected (Teklif secildi)
    /// 4: Cancelled (Iptal)
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// Talep kilitli mi? (Belge islemi devam ederken kilit)
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Kilitleyen belge ID
    /// </summary>
    public int? LockedByDocumentId { get; set; }

    /// <summary>
    /// Kilit tarihi
    /// </summary>
    public DateTime? LockedAt { get; set; }

    /// <summary>
    /// Secilen teklif ID
    /// </summary>
    public int? SelectedQuoteId { get; set; }

    // === BAGIMLILIK YONETIMI ===

    /// <summary>
    /// Bu request hangi request'e bagli? (Survey -> Logistics gibi)
    /// </summary>
    public int? DependsOnServiceRequestId { get; set; }

    /// <summary>
    /// Otomatik olusturuldu mu? (Lojistik secilince Survey otomatik olusur)
    /// </summary>
    public bool AutoCreated { get; set; }

    /// <summary>
    /// Tetikleme kaynagi (ornek: "LogisticsQuoteAccepted", "AllServicesSelected")
    /// </summary>
    public string? TriggerSource { get; set; }

    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual Country? OriginCountry { get; set; }
    public virtual Country? DestinationCountry { get; set; }
    public virtual OrderServiceQuote? SelectedQuote { get; set; }
    public virtual OrderServiceRequest? DependsOnServiceRequest { get; set; }
    public virtual ICollection<OrderServiceQuote> Quotes { get; set; } = new List<OrderServiceQuote>();
    public virtual ICollection<OrderServiceRequest> DependentRequests { get; set; } = new List<OrderServiceRequest>();
}
