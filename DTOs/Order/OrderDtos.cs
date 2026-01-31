namespace Bridgo.DTOs.Order;

// ============================================
// CREATE/UPDATE DTOs
// ============================================

/// <summary>
/// Yeni siparis olusturma (dogrudan satin alma)
/// </summary>
public class CreateOrderDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public int? WarehouseId { get; set; }

    // Adresler
    public int? ShippingAddressId { get; set; }
    public int? BillingAddressId { get; set; }

    // Notlar
    public string? Notes { get; set; }
}

/// <summary>
/// Inquiry'den siparis olusturma
/// </summary>
public class CreateOrderFromInquiryDto
{
    public int InquiryId { get; set; }
    public int ResponseId { get; set; }
    public int? ShippingAddressId { get; set; }
    public int? BillingAddressId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Demand'dan siparis olusturma
/// </summary>
public class CreateOrderFromDemandDto
{
    public int DemandId { get; set; }
    public int ResponseId { get; set; }
    public int? ShippingAddressId { get; set; }
    public int? BillingAddressId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Siparis durumu guncelleme (satici)
/// </summary>
public class UpdateOrderStatusDto
{
    public int Status { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Siparis iptal (alici veya satici)
/// </summary>
public class CancelOrderDto
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Kargo ekleme (satici tarafindan)
/// </summary>
public class CreateShipmentDto
{
    public string? CarrierCode { get; set; }
    public string? CarrierName { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? Notes { get; set; }

    // Yük bilgileri (Seller doldurur)
    public decimal? TotalGrossWeight { get; set; }
    public decimal? TotalNetWeight { get; set; }
    public decimal? TotalVolume { get; set; }
    public int? TotalPackageCount { get; set; }
    public string? PackageType { get; set; }

    // Hangi urunler bu kargoda
    public List<ShipmentItemDto> Items { get; set; } = new();
}

/// <summary>
/// Sevkiyat taşıma detayları güncelleme (Carrier tarafından)
/// </summary>
public class UpdateShipmentTransportDto
{
    // Taşıma modu ve araç
    public int? TransportModeCode { get; set; }
    public int? VehicleTypeCode { get; set; }
    public string? VehicleIdentity { get; set; }
    public string? VehicleCountryCode { get; set; }
    public string? BorderVehicleIdentity { get; set; }
    public string? BorderVehicleCountryCode { get; set; }

    // Konteyner / TIR bilgileri
    public bool HasContainer { get; set; }
    public string? ContainerNumbers { get; set; }
    public string? ContainerType { get; set; }
    public int? ContainerCount { get; set; }
    public string? SealNumbers { get; set; }

    // Liman / Terminal bilgileri
    public string? LoadingPortCode { get; set; }
    public string? LoadingPortName { get; set; }
    public string? DischargePortCode { get; set; }
    public string? DischargePortName { get; set; }
    public string? WarehouseCode { get; set; }

    // Taşıma belgesi
    public string? TransportDocumentNumber { get; set; }
    public int? TransportDocumentType { get; set; }
    public DateTime? TransportDocumentDate { get; set; }

    // Tarihler
    public DateTime? LoadingDate { get; set; }
    public DateTime? BorderCrossDate { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// Kargo kalemi
/// </summary>
public class ShipmentItemDto
{
    public int OrderItemId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Kargo durumu guncelleme
/// </summary>
public class UpdateShipmentDto
{
    public int Status { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? Notes { get; set; }
}

// ============================================
// LIST DTOs
// ============================================

/// <summary>
/// Siparis listesi icin
/// </summary>
public class OrderListDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    // Alici/Satici
    public int BuyerVendorId { get; set; }
    public string? BuyerCompanyName { get; set; }
    public int SellerVendorId { get; set; }
    public string? SellerCompanyName { get; set; }

    // Kaynak
    public int SourceType { get; set; }
    public string? SourceTypeText { get; set; }

    // Tutarlar
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";

    // Durum
    public int Status { get; set; }
    public string? StatusText { get; set; }
    public string? StatusCssClass { get; set; }
    public string? StatusIcon { get; set; }

    public int PaymentStatus { get; set; }
    public string? PaymentStatusText { get; set; }

    // Ozet bilgiler
    public int ItemCount { get; set; }
    public int ShipmentCount { get; set; }
    public bool HasActiveShipment { get; set; }

    // Hizmet talepleri
    public int ServiceRequestCount { get; set; }
    public int PendingQuotesCount { get; set; }
    public bool HasPendingQuotes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

/// <summary>
/// Siparis detayi
/// </summary>
public class OrderDetailDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    // Alici
    public int BuyerVendorId { get; set; }
    public string? BuyerCompanyName { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerPhone { get; set; }

    // Satici
    public int SellerVendorId { get; set; }
    public string? SellerCompanyName { get; set; }
    public string? SellerEmail { get; set; }
    public string? SellerPhone { get; set; }

    // Kaynak
    public int SourceType { get; set; }
    public string? SourceTypeText { get; set; }
    public int? SourceInquiryId { get; set; }
    public int? SourceDemandId { get; set; }
    public int? SourceDemandResponseId { get; set; }

    // Tutarlar
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";

    // Incoterm
    public int? IncotermId { get; set; }
    public string? IncotermCode { get; set; }
    public string? IncotermName { get; set; }
    public string? IncotermLocation { get; set; }

    // Dış Ticaret / Gümrük Bilgileri
    public decimal? FreightAmount { get; set; }      // Navlun
    public decimal? InsuranceAmount { get; set; }    // Sigorta
    public decimal? ExchangeRate { get; set; }       // Döviz kuru
    public DateTime? ExchangeRateDate { get; set; }  // Kur tarihi
    public decimal? FobValue { get; set; }           // FOB değer
    public decimal? CifValue { get; set; }           // CIF değer
    public decimal? StatisticalValue { get; set; }   // İstatistiki kıymet (USD)

    // Gümrük Beyanname Bilgileri
    public string? CustomsOfficeCode { get; set; }   // Gümrük idaresi kodu
    public string? DeclarationNumber { get; set; }   // Beyanname no
    public DateTime? DeclarationDate { get; set; }   // Beyanname tarihi
    public string? CustomsRegimeCode { get; set; }   // Rejim kodu
    public int? ExportType { get; set; }             // 1: Normal, 2: ETGB, 3: Hizmet
    public string? ExportTypeText { get; set; }
    public int? TransportModeCode { get; set; }      // Taşıma şekli
    public string? TransportModeText { get; set; }

    // Adresler
    public OrderAddressDto? ShippingAddress { get; set; }
    public OrderAddressDto? BillingAddress { get; set; }

    // Durum
    public int Status { get; set; }
    public string? StatusText { get; set; }
    public string? StatusCssClass { get; set; }
    public string? StatusIcon { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }

    // Odeme
    public int PaymentStatus { get; set; }
    public string? PaymentStatusText { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public DateTime? PaidAt { get; set; }

    // Kalemler
    public List<OrderItemDto> Items { get; set; } = new();

    // Kargolar
    public List<OrderShipmentDto> Shipments { get; set; } = new();

    // Durum gecmisi
    public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Siparis kalemi
/// </summary>
public class OrderItemDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    // Urun
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductImageUrl { get; set; }

    // Depo
    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

    // Miktar ve fiyat
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Teslimat durumu
    public int ShippedQuantity { get; set; }
    public int DeliveredQuantity { get; set; }
    public int RemainingToShip { get; set; }

    // Gümrük Beyannamesi Alanları (Kalem Bazlı)
    public string? HSCode { get; set; }              // GTİP kodu
    public string? CountryOfOrigin { get; set; }     // Menşe ülke
    public string? ProductDescription { get; set; }  // Ticari tanım
    public string? Brand { get; set; }               // Marka
    public string? Model { get; set; }               // Model
    public decimal? GrossWeight { get; set; }        // Brüt ağırlık (kg)
    public decimal? NetWeight { get; set; }          // Net ağırlık (kg)
    public decimal? FobUnitPrice { get; set; }       // FOB birim fiyat
    public decimal? FobTotalPrice { get; set; }      // FOB toplam
    public decimal? CifTotalPrice { get; set; }      // CIF toplam
    public decimal? StatisticalValue { get; set; }   // İstatistiki kıymet
    public string? ExemptionCode { get; set; }       // Muafiyet kodu
}

/// <summary>
/// Siparis adresi
/// </summary>
public class OrderAddressDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? AddressLine { get; set; }
    public string? PostalCode { get; set; }
}

/// <summary>
/// Kargo bilgisi
/// </summary>
public class OrderShipmentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ShipmentNumber { get; set; } = string.Empty;

    // Taşıyıcı firma
    public int? CarrierVendorId { get; set; }
    public string? CarrierVendorName { get; set; }
    public string? CarrierCode { get; set; }
    public string? CarrierName { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }

    // Taşıma modu ve araç (Beyanname kutu 18, 25, 26)
    public int? TransportModeCode { get; set; }
    public string? TransportModeText { get; set; }
    public int? VehicleTypeCode { get; set; }
    public string? VehicleTypeText { get; set; }
    public string? VehicleIdentity { get; set; }         // Plaka/IMO/Uçuş no
    public string? VehicleCountryCode { get; set; }      // Araç ülkesi

    // Konteyner / TIR bilgileri
    public bool HasContainer { get; set; }
    public string? ContainerNumbers { get; set; }
    public string? ContainerType { get; set; }
    public int? ContainerCount { get; set; }
    public string? SealNumbers { get; set; }

    // Liman / Terminal bilgileri
    public string? LoadingPortCode { get; set; }
    public string? LoadingPortName { get; set; }
    public string? DischargePortCode { get; set; }
    public string? DischargePortName { get; set; }
    public string? WarehouseCode { get; set; }

    // Yük bilgileri
    public decimal? TotalGrossWeight { get; set; }
    public decimal? TotalNetWeight { get; set; }
    public decimal? TotalVolume { get; set; }
    public int? TotalPackageCount { get; set; }
    public string? PackageType { get; set; }

    // Taşıma belgesi
    public string? TransportDocumentNumber { get; set; }  // Konşimento/CMR/AWB
    public int? TransportDocumentType { get; set; }
    public string? TransportDocumentTypeText { get; set; }
    public DateTime? TransportDocumentDate { get; set; }

    // Durum
    public int Status { get; set; }
    public string? StatusText { get; set; }
    public string? StatusCssClass { get; set; }
    public string? StatusIcon { get; set; }

    // Tarihler
    public DateTime? LoadingDate { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? BorderCrossDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }

    public string? Notes { get; set; }

    // Icerik
    public List<OrderShipmentItemDto> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Kargo icerigi
/// </summary>
public class OrderShipmentItemDto
{
    public int Id { get; set; }
    public int ShipmentId { get; set; }
    public int OrderItemId { get; set; }

    // Urun bilgisi
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }

    public int Quantity { get; set; }
}

/// <summary>
/// Durum gecmisi
/// </summary>
public class OrderStatusHistoryDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    public int OldStatus { get; set; }
    public string? OldStatusText { get; set; }
    public int NewStatus { get; set; }
    public string? NewStatusText { get; set; }

    public string? Notes { get; set; }
    public int? ChangedByUserId { get; set; }
    public string? ChangedByUserName { get; set; }

    public DateTime CreatedAt { get; set; }
}

// ============================================
// SEARCH/FILTER DTOs
// ============================================

/// <summary>
/// Siparis filtresi
/// </summary>
public class OrderFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public int? Status { get; set; }
    public int? PaymentStatus { get; set; }
    public int? SourceType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Siparis arama sonucu
/// </summary>
public class OrderSearchResultDto
{
    public List<OrderListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

// ============================================
// STATISTICS DTOs
// ============================================

/// <summary>
/// Alici siparis istatistikleri
/// </summary>
public class BuyerOrderStatsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ActiveOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public string Currency { get; set; } = "TRY";
}

/// <summary>
/// Satici siparis istatistikleri
/// </summary>
public class SellerOrderStatsDto
{
    public int TotalOrders { get; set; }
    public int PendingConfirmation { get; set; }
    public int ActiveOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public string Currency { get; set; } = "TRY";
}

// ============================================
// PAYMENT DTOs
// ============================================

/// <summary>
/// Stripe PaymentIntent olusturma icin
/// </summary>
public class CreatePaymentIntentDto
{
    public int OrderId { get; set; }
}

/// <summary>
/// PaymentIntent sonucu
/// </summary>
public class PaymentIntentResultDto
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// Odeme durumu
/// </summary>
public class PaymentStatusDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int PaymentStatus { get; set; }
    public string? PaymentStatusText { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string? ReceiptUrl { get; set; }
}

/// <summary>
/// Iade istegi
/// </summary>
public class RefundRequestDto
{
    public decimal? Amount { get; set; }  // null = tam iade
    public string? Reason { get; set; }
}

/// <summary>
/// Iade sonucu
/// </summary>
public class RefundResultDto
{
    public bool Success { get; set; }
    public string? RefundId { get; set; }
    public decimal RefundedAmount { get; set; }
    public string? ErrorMessage { get; set; }
}

// ============================================
// SERVICE REQUEST DTOs
// ============================================

/// <summary>
/// Servis talebi olusturma
/// </summary>
public class CreateServiceRequestDto
{
    public int ServiceType { get; set; }  // 1: Logistics, 2: Customs, 3: Insurance, 4: Survey
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Yuk bilgileri
    public decimal? WeightKg { get; set; }
    public decimal? VolumeM3 { get; set; }
    public int? PackageCount { get; set; }
    public decimal? CargoValue { get; set; }
    public string? Currency { get; set; }

    // Lokasyonlar
    public int? OriginCountryId { get; set; }
    public string? OriginCity { get; set; }
    public string? OriginAddress { get; set; }
    public int? DestinationCountryId { get; set; }
    public string? DestinationCity { get; set; }
    public string? DestinationAddress { get; set; }

    // Lojistik ozel
    public int? TransportMode { get; set; }
    public string? Incoterms { get; set; }

    // Gumruk ozel
    public int? CustomsOperationType { get; set; }
    public string? HsCode { get; set; }

    // Sigorta ozel
    public int? InsuranceType { get; set; }

    // Tarihler
    public DateTime? DesiredPickupDate { get; set; }
    public DateTime? DesiredDeliveryDate { get; set; }
    public DateTime? QuoteDeadline { get; set; }
}

/// <summary>
/// Servis talebi listesi
/// </summary>
public class ServiceRequestListDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int ServiceType { get; set; }
    public string? ServiceTypeText { get; set; }
    public string Title { get; set; } = string.Empty;

    // Buyer bilgisi
    public int BuyerVendorId { get; set; }
    public string? BuyerCompanyName { get; set; }

    // Ozet
    public decimal? CargoValue { get; set; }
    public string? Currency { get; set; }
    public string? OriginCountry { get; set; }
    public string? DestinationCountry { get; set; }

    // Durum
    public int Status { get; set; }
    public string? StatusText { get; set; }
    public int QuoteCount { get; set; }
    public DateTime? QuoteDeadline { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Servis talebi detayi
/// </summary>
public class ServiceRequestDetailDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int ServiceType { get; set; }
    public string? ServiceTypeText { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Buyer bilgisi
    public int BuyerVendorId { get; set; }
    public string? BuyerCompanyName { get; set; }

    // Yuk bilgileri
    public decimal? WeightKg { get; set; }
    public decimal? VolumeM3 { get; set; }
    public int? PackageCount { get; set; }
    public decimal? CargoValue { get; set; }
    public string? Currency { get; set; }

    // Lokasyonlar
    public int? OriginCountryId { get; set; }
    public string? OriginCountryName { get; set; }
    public string? OriginCity { get; set; }
    public string? OriginAddress { get; set; }
    public int? DestinationCountryId { get; set; }
    public string? DestinationCountryName { get; set; }
    public string? DestinationCity { get; set; }
    public string? DestinationAddress { get; set; }

    // Lojistik ozel
    public int? TransportMode { get; set; }
    public string? TransportModeText { get; set; }
    public string? Incoterms { get; set; }

    // Gumruk ozel
    public int? CustomsOperationType { get; set; }
    public string? CustomsOperationTypeText { get; set; }
    public string? HsCode { get; set; }

    // Sigorta ozel
    public int? InsuranceType { get; set; }
    public string? InsuranceTypeText { get; set; }

    // Tarihler
    public DateTime? DesiredPickupDate { get; set; }
    public DateTime? DesiredDeliveryDate { get; set; }
    public DateTime? QuoteDeadline { get; set; }

    // Durum
    public int Status { get; set; }
    public string? StatusText { get; set; }
    public int? SelectedQuoteId { get; set; }

    // Teklifler
    public List<ServiceQuoteDto> Quotes { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Servis teklifi olusturma
/// </summary>
public class CreateServiceQuoteDto
{
    public int ServiceRequestId { get; set; }
    public decimal QuoteAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string? IncludedServices { get; set; }
    public string? AdditionalCosts { get; set; }
    public int? EstimatedDays { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }

    // Lojistik ozel
    public List<int>? TransportModes { get; set; }
    public string? CarrierName { get; set; }
    public int? TransitStops { get; set; }

    // Sigorta ozel
    public decimal? CoverageAmount { get; set; }
    public decimal? DeductiblePercent { get; set; }
    public string? CoverageDetails { get; set; }
}

/// <summary>
/// Servis teklifi guncelleme
/// </summary>
public class UpdateServiceQuoteDto
{
    public decimal? QuoteAmount { get; set; }
    public int? EstimatedDays { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Servis teklifi
/// </summary>
public class ServiceQuoteDto
{
    public int Id { get; set; }
    public int ServiceRequestId { get; set; }

    // Provider bilgisi
    public int ProviderVendorId { get; set; }
    public string? ProviderCompanyName { get; set; }

    // Fiyat
    public decimal QuoteAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string? IncludedServices { get; set; }
    public string? AdditionalCosts { get; set; }

    // Sure
    public int? EstimatedDays { get; set; }
    public DateTime? ValidUntil { get; set; }

    // Detaylar
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }

    // Lojistik ozel
    public string? TransportModes { get; set; }
    public string? TransportModesText { get; set; }
    public string? CarrierName { get; set; }
    public int? TransitStops { get; set; }

    // Sigorta ozel
    public decimal? CoverageAmount { get; set; }
    public decimal? DeductiblePercent { get; set; }
    public string? CoverageDetails { get; set; }

    // Durum
    public int Status { get; set; }
    public string? StatusText { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsReadByBuyer { get; set; }

    public DateTime CreatedAt { get; set; }
}

// ============================================
// TASK DTOs
// ============================================

/// <summary>
/// Siparis gorevi
/// </summary>
public class OrderTaskDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int ParticipantId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TaskType { get; set; }
    public string? TaskTypeText { get; set; }
    public int SortOrder { get; set; }

    public int? DependsOnTaskId { get; set; }
    public bool CanStart { get; set; }  // Bagli gorev tamamlanmis mi

    public DateTime? DueDate { get; set; }

    // Durum
    public int Status { get; set; }
    public string? StatusText { get; set; }
    public string? StatusCssClass { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletionNotes { get; set; }

    public bool IsRequired { get; set; }

    public DateTime CreatedAt { get; set; }
}

// ============================================
// INVESTMENT DTOs
// ============================================

/// <summary>
/// Yatirim firsati
/// </summary>
public class InvestmentOpportunityDto
{
    public int OrderId { get; set; }
    public string? OrderNumber { get; set; }

    // Buyer bilgisi
    public int BuyerVendorId { get; set; }
    public string? BuyerCompanyName { get; set; }

    // Siparis tutari
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";

    // Yatirim ihtiyaci
    public decimal RequiredInvestment { get; set; }
    public decimal CurrentInvestment { get; set; }
    public decimal RemainingInvestment { get; set; }

    // Detaylar
    public string? Description { get; set; }
    public decimal? SuggestedReturnRate { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Yatirim olusturma
/// </summary>
public class CreateInvestmentDto
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public int InvestmentType { get; set; }  // 1: Equity, 2: Loan, 3: PrePayment
    public decimal? ReturnRate { get; set; }
    public DateTime? RepaymentDueDate { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Yatirim bilgisi
/// </summary>
public class OrderInvestmentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? OrderNumber { get; set; }

    // Yatirimci
    public int InvestorVendorId { get; set; }
    public string? InvestorCompanyName { get; set; }

    // Tutar
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal PercentageOfTotal { get; set; }

    // Tip ve getiri
    public int InvestmentType { get; set; }
    public string? InvestmentTypeText { get; set; }
    public decimal? ReturnRate { get; set; }
    public decimal? ExpectedReturn { get; set; }
    public DateTime? RepaymentDueDate { get; set; }

    // Durum
    public int Status { get; set; }
    public string? StatusText { get; set; }
    public bool IsFunded { get; set; }
    public DateTime? FundedAt { get; set; }
    public bool IsRepaid { get; set; }
    public DateTime? RepaidAt { get; set; }
    public decimal? RepaidAmount { get; set; }

    public string? TermsAndConditions { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
