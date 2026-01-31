namespace Bridgo.Models.Entities;

/// <summary>
/// Siparis - Her satici icin ayri siparis olusur
/// </summary>
public class Order : BaseEntity
{
    /// <summary>
    /// Benzersiz siparis numarasi (ORD-2024-00001)
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Alici firma
    /// </summary>
    public int BuyerVendorId { get; set; }

    /// <summary>
    /// Satici firma
    /// </summary>
    public int SellerVendorId { get; set; }

    // Source tracking
    /// <summary>
    /// Siparis kaynagi (Direct, FromInquiry, FromDemand)
    /// </summary>
    public int SourceType { get; set; }

    /// <summary>
    /// Kaynak ProductInquiry ID (FromInquiry ise)
    /// </summary>
    public int? SourceInquiryId { get; set; }

    /// <summary>
    /// Kaynak PublicDemand ID (FromDemand ise)
    /// </summary>
    public int? SourceDemandId { get; set; }

    /// <summary>
    /// Kaynak DemandResponse ID (FromDemand ise)
    /// </summary>
    public int? SourceDemandResponseId { get; set; }

    // Amounts
    /// <summary>
    /// Ara toplam (KDV haric)
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Kargo ucreti
    /// </summary>
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Vergi tutari
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Toplam tutar
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Para birimi (TRY, USD, EUR)
    /// </summary>
    public string Currency { get; set; } = "TRY";

    // Addresses
    /// <summary>
    /// Teslimat adresi
    /// </summary>
    public int? ShippingAddressId { get; set; }

    /// <summary>
    /// Fatura adresi
    /// </summary>
    public int? BillingAddressId { get; set; }

    // Status
    /// <summary>
    /// Siparis durumu (OrderStatuses)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Siparis notu
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Iptal nedeni
    /// </summary>
    public string? CancellationReason { get; set; }

    // Incoterm
    /// <summary>
    /// Incoterm ID (EXW, FOB, CIF, DDP vb.)
    /// </summary>
    public int? IncotermId { get; set; }

    /// <summary>
    /// Incoterm lokasyonu (örn: "Istanbul Port", "Buyer's Warehouse")
    /// </summary>
    public string? IncotermLocation { get; set; }

    // === DIŞ TİCARET / GÜMRÜK BİLGİLERİ ===

    /// <summary>
    /// Navlun tutari (Freight) - Tasima ucreti
    /// CIF hesaplamasinda kullanilir
    /// </summary>
    public decimal? FreightAmount { get; set; }

    /// <summary>
    /// Sigorta tutari (Insurance)
    /// CIF hesaplamasinda kullanilir
    /// </summary>
    public decimal? InsuranceAmount { get; set; }

    /// <summary>
    /// Doviz kuru (siparis tarihindeki kur)
    /// TCMB efektif satis kuru kullanilir
    /// </summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// Doviz kuru tarihi
    /// </summary>
    public DateTime? ExchangeRateDate { get; set; }

    /// <summary>
    /// FOB Degeri (Free On Board) - Urun degeri
    /// Ihracatta: SubTotal
    /// </summary>
    public decimal? FobValue { get; set; }

    /// <summary>
    /// CIF Degeri (Cost, Insurance, Freight)
    /// CIF = FOB + Navlun + Sigorta
    /// Ithalatta gumruk vergisi matrahidir
    /// </summary>
    public decimal? CifValue { get; set; }

    /// <summary>
    /// Istatistiki kiymeti (USD cinsinden)
    /// Beyanname kutu 46 - FOB/USD (ihracat) veya CIF/USD (ithalat)
    /// </summary>
    public decimal? StatisticalValue { get; set; }

    /// <summary>
    /// Gumruk idaresi kodu (4 haneli)
    /// Ornek: 3400 (Istanbul Havalimani)
    /// </summary>
    public string? CustomsOfficeCode { get; set; }

    /// <summary>
    /// Beyanname tescil numarasi
    /// BILGE sisteminden alinan numara
    /// </summary>
    public string? DeclarationNumber { get; set; }

    /// <summary>
    /// Beyanname tescil tarihi
    /// </summary>
    public DateTime? DeclarationDate { get; set; }

    /// <summary>
    /// Gumruk rejim kodu (4 haneli)
    /// Ornek: 1000 (Serbest Dolasima Giris), 1040 (Dahilde Isleme), 3151 (Gecici Ihracat)
    /// Beyanname kutu 37
    /// </summary>
    public string? CustomsRegimeCode { get; set; }

    /// <summary>
    /// Ihracat tipi
    /// 1: Normal Ihracat (Gumruk Beyannamesi)
    /// 2: Mikro Ihracat (ETGB/BGB - 30.000 EUR altı)
    /// 3: Hizmet Ihracati
    /// </summary>
    public int? ExportType { get; set; }

    /// <summary>
    /// Tasima sekli kodu
    /// 1: Deniz, 2: Demiryolu, 3: Karayolu, 4: Havayolu, 5: Posta, 7: Boru hatti, 9: Diger
    /// Beyanname kutu 25
    /// </summary>
    public int? TransportModeCode { get; set; }

    /// <summary>
    /// Tasima araci kimlik bilgisi (Plaka, IMO, Ucus no)
    /// Beyanname kutu 18
    /// </summary>
    public string? TransportIdentity { get; set; }

    // Payment
    /// <summary>
    /// Odeme durumu (PaymentStatuses)
    /// </summary>
    public int PaymentStatus { get; set; }

    /// <summary>
    /// Stripe PaymentIntent ID
    /// </summary>
    public string? StripePaymentIntentId { get; set; }

    /// <summary>
    /// Stripe Charge ID
    /// </summary>
    public string? StripeChargeId { get; set; }

    /// <summary>
    /// Odeme tarihi
    /// </summary>
    public DateTime? PaidAt { get; set; }

    // === SOZLESME DURUMU ===

    /// <summary>
    /// Sozlesme onaylandi mi (tum teklifler secildikten sonra)
    /// </summary>
    public bool IsContractAccepted { get; set; }

    /// <summary>
    /// Sozlesme onay tarihi
    /// </summary>
    public DateTime? ContractAcceptedAt { get; set; }

    // === SERVIS AKISI YONETIMI ===

    /// <summary>
    /// Checkout adimi (CheckoutSteps)
    /// 1: ServiceRequested - Hizmet talepleri gonderildi
    /// 2: WaitingForQuotes - Teklifler bekleniyor
    /// 3: QuotesReceived - Teklifler geldi
    /// 4: QuotesSelected - Teklifler secildi
    /// 5: FinancingPending - Finansman bekleniyor
    /// 6: PaymentPending - Odeme bekleniyor
    /// 7: Completed - Siparis onaylandi
    /// </summary>
    public int CheckoutStep { get; set; } = 1;

    /// <summary>
    /// Finansman gerekli mi? (Checkout'ta secilir)
    /// </summary>
    public bool RequiresFinancing { get; set; }

    /// <summary>
    /// Gozetim tetikleme durumu (SurveyTriggerStatuses)
    /// 0: NotRequired - Gozetim istenmedi
    /// 1: WaitingForLogistics - Lojistik secimi bekleniyor
    /// 2: Ready - Survey request olusturulabilir
    /// 3: Created - Survey request olusturuldu
    /// </summary>
    public int SurveyTriggerStatus { get; set; }

    /// <summary>
    /// Tum servisler secildi mi kontrolu icin tarih
    /// </summary>
    public DateTime? AllServicesSelectedAt { get; set; }

    /// <summary>
    /// Otomatik olusturulan FinancingRequest ID
    /// </summary>
    public int? FinancingRequestId { get; set; }

    // Navigation properties
    public virtual Vendor? BuyerVendor { get; set; }
    public virtual Vendor? SellerVendor { get; set; }
    public virtual Address? ShippingAddress { get; set; }
    public virtual Address? BillingAddress { get; set; }
    public virtual ProductInquiry? SourceInquiry { get; set; }
    public virtual PublicDemand? SourceDemand { get; set; }
    public virtual DemandResponse? SourceDemandResponse { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderShipment> Shipments { get; set; } = new List<OrderShipment>();
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<StripePayment> StripePayments { get; set; } = new List<StripePayment>();
    public virtual ICollection<OrderServiceRequest> ServiceRequests { get; set; } = new List<OrderServiceRequest>();
    public virtual ICollection<OrderParticipant> Participants { get; set; } = new List<OrderParticipant>();
    public virtual ICollection<OrderTask> Tasks { get; set; } = new List<OrderTask>();
    public virtual ICollection<OrderInvestment> Investments { get; set; } = new List<OrderInvestment>();
    public virtual FinancingRequest? FinancingRequest { get; set; }
}
