namespace Bridgo.Models.Entities;

/// <summary>
/// Siparis kargosu - Kismi teslimat icin birden fazla olabilir
/// Seller olusturur, Carrier tasima detaylarini girer
/// </summary>
public class OrderShipment : BaseEntity
{
    /// <summary>
    /// Siparis ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Kargo numarasi (SHP-001)
    /// </summary>
    public string ShipmentNumber { get; set; } = string.Empty;

    // === TASIYICI BILGILERI ===

    /// <summary>
    /// Tasiyici firma (Carrier capability)
    /// Lojistik teklifi secildiginde atanir
    /// </summary>
    public int? CarrierVendorId { get; set; }

    /// <summary>
    /// Secilen lojistik teklifi
    /// </summary>
    public int? ServiceQuoteId { get; set; }

    /// <summary>
    /// Kargo firmasi kodu (DHL, UPS, FEDEX, OTHER veya ozel)
    /// </summary>
    public string? CarrierCode { get; set; }

    /// <summary>
    /// Kargo firmasi adi
    /// </summary>
    public string? CarrierName { get; set; }

    /// <summary>
    /// Takip numarasi
    /// </summary>
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Takip URL'i
    /// </summary>
    public string? TrackingUrl { get; set; }

    // === TASIMA MODU VE ARAC BILGILERI (Beyanname Kutu 18, 25, 26) ===

    /// <summary>
    /// Tasima sekli kodu
    /// 1: Deniz, 2: Demiryolu, 3: Karayolu, 4: Havayolu, 5: Posta, 9: Diger
    /// </summary>
    public int? TransportModeCode { get; set; }

    /// <summary>
    /// Cikistaki arac tipi (Beyanname kutu 26)
    /// 1: Gemi, 2: Vagon, 3: Tir/Kamyon, 4: Ucak
    /// </summary>
    public int? VehicleTypeCode { get; set; }

    /// <summary>
    /// Arac kimlik bilgisi - Plaka, IMO, Ucus no (Beyanname kutu 18)
    /// </summary>
    public string? VehicleIdentity { get; set; }

    /// <summary>
    /// Arac plaka ulkesi - ISO 2 hane (Beyanname kutu 18)
    /// </summary>
    public string? VehicleCountryCode { get; set; }

    /// <summary>
    /// Sinirdaki arac kimlik bilgisi (aktarma varsa)
    /// </summary>
    public string? BorderVehicleIdentity { get; set; }

    /// <summary>
    /// Sinirdaki arac ulkesi
    /// </summary>
    public string? BorderVehicleCountryCode { get; set; }

    // === KONTEYNER / TIR BILGILERI ===

    /// <summary>
    /// Konteyner var mi? (1: Evet, 0: Hayir)
    /// </summary>
    public bool HasContainer { get; set; } = false;

    /// <summary>
    /// Konteyner numarasi (birden fazla ise virgul ile)
    /// </summary>
    public string? ContainerNumbers { get; set; }

    /// <summary>
    /// Konteyner tipi (20DC, 40HC, vb.)
    /// </summary>
    public string? ContainerType { get; set; }

    /// <summary>
    /// Konteyner / TIR sayisi (Beyanname icin)
    /// </summary>
    public int? ContainerCount { get; set; }

    /// <summary>
    /// Muhur numaralari
    /// </summary>
    public string? SealNumbers { get; set; }

    // === LIMAN / TERMINAL BILGILERI ===

    /// <summary>
    /// Yukleme limani/terminali kodu (UN/LOCODE)
    /// Ornek: TRIST (Istanbul), TRIZM (Izmir)
    /// </summary>
    public string? LoadingPortCode { get; set; }

    /// <summary>
    /// Yukleme limani/terminali adi
    /// </summary>
    public string? LoadingPortName { get; set; }

    /// <summary>
    /// Bosaltma limani/terminali kodu (UN/LOCODE)
    /// </summary>
    public string? DischargePortCode { get; set; }

    /// <summary>
    /// Bosaltma limani/terminali adi
    /// </summary>
    public string? DischargePortName { get; set; }

    /// <summary>
    /// Antrepo kodu (varsa)
    /// </summary>
    public string? WarehouseCode { get; set; }

    // === YUK BILGILERI ===

    /// <summary>
    /// Toplam brut agirlik (kg)
    /// </summary>
    public decimal? TotalGrossWeight { get; set; }

    /// <summary>
    /// Toplam net agirlik (kg)
    /// </summary>
    public decimal? TotalNetWeight { get; set; }

    /// <summary>
    /// Toplam hacim (m3)
    /// </summary>
    public decimal? TotalVolume { get; set; }

    /// <summary>
    /// Toplam kap/paket sayisi
    /// </summary>
    public int? TotalPackageCount { get; set; }

    /// <summary>
    /// Kap cinsi (Beyanname kutu 31)
    /// Ornek: Karton kutu, Palet, Kasa, Cuval
    /// </summary>
    public string? PackageType { get; set; }

    // === TASIMA BELGELERI ===

    /// <summary>
    /// Tasima senedi numarasi (Konşimento, CMR, AWB)
    /// </summary>
    public string? TransportDocumentNumber { get; set; }

    /// <summary>
    /// Tasima senedi tipi
    /// 1: Bill of Lading (Konşimento), 2: CMR (Karayolu), 3: AWB (Havayolu), 4: CIM (Demiryolu)
    /// </summary>
    public int? TransportDocumentType { get; set; }

    /// <summary>
    /// Tasima senedi tarihi
    /// </summary>
    public DateTime? TransportDocumentDate { get; set; }

    // === TARIHLER VE DURUM ===

    /// <summary>
    /// Kargo durumu (ShipmentStatuses)
    /// 1: Preparing, 2: ReadyForPickup, 3: PickedUp, 4: InTransit, 5: Customs, 6: Delivered, 7: Returned
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Yukleme tarihi
    /// </summary>
    public DateTime? LoadingDate { get; set; }

    /// <summary>
    /// Gonderim tarihi
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// Sinir gecis tarihi
    /// </summary>
    public DateTime? BorderCrossDate { get; set; }

    /// <summary>
    /// Tahmini teslim tarihi
    /// </summary>
    public DateTime? EstimatedDeliveryDate { get; set; }

    /// <summary>
    /// Gercek teslim tarihi
    /// </summary>
    public DateTime? ActualDeliveryDate { get; set; }

    /// <summary>
    /// Notlar
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual Vendor? CarrierVendor { get; set; }
    public virtual OrderServiceQuote? ServiceQuote { get; set; }
    public virtual ICollection<OrderShipmentItem> Items { get; set; } = new List<OrderShipmentItem>();
}
