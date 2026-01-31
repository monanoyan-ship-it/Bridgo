namespace Bridgo.DTOs.Checkout;

/// <summary>
/// Checkout ozet bilgisi
/// </summary>
public class CheckoutSummaryDto
{
    public int CartId { get; set; }
    public List<CheckoutVendorGroupDto> VendorGroups { get; set; } = new();
    public decimal TotalProductAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public int TotalItemCount { get; set; }

    // Teslimat bilgileri
    public CheckoutAddressDto? ShippingAddress { get; set; }
    public CheckoutAddressDto? BillingAddress { get; set; }
    public List<CheckoutAddressDto> AvailableAddresses { get; set; } = new();

    // Kaynak adresler (satici depolari)
    public List<CheckoutOriginDto> Origins { get; set; } = new();

    // Mevcut hizmet secenekleri
    public List<ServiceOptionDto> ServiceOptions { get; set; } = new();
}

/// <summary>
/// Satici bazli urun grubu
/// </summary>
public class CheckoutVendorGroupDto
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public List<CheckoutItemDto> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public CheckoutOriginDto? Origin { get; set; }
}

/// <summary>
/// Checkout kalemi
/// </summary>
public class CheckoutItemDto
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "TRY";

    // Urun bilgileri (hizmet hesaplamasi icin)
    public decimal? WeightKg { get; set; }
    public decimal? VolumeM3 { get; set; }
    public string? HsCode { get; set; }
}

/// <summary>
/// Adres bilgisi
/// </summary>
public class CheckoutAddressDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// Kaynak adres (satici deposu)
/// </summary>
public class CheckoutOriginDto
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
}

/// <summary>
/// Hizmet secenegi
/// </summary>
public class ServiceOptionDto
{
    public int ServiceType { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    public string ServiceTypeIcon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsSelected { get; set; }
}

/// <summary>
/// Checkout baslatma istegi
/// </summary>
public class InitiateCheckoutDto
{
    public int ShippingAddressId { get; set; }
    public int? BillingAddressId { get; set; }
    public string? Notes { get; set; }

    // Incoterm secimi
    public int? IncotermId { get; set; }
    public string? IncotermLocation { get; set; }

    // Secilen hizmetler
    public List<ServiceSelectionDto> Services { get; set; } = new();

    // Finansman istekleri (birden fazla kalem icin)
    public bool RequestFinancing { get; set; }
    public List<FinancingRequestDto>? FinancingRequests { get; set; }

    // Eski uyumluluk icin (tek finansman)
    public FinancingRequestDto? FinancingDetails { get; set; }
}

/// <summary>
/// Hizmet secimi detayi
/// </summary>
public class ServiceSelectionDto
{
    public int ServiceType { get; set; }

    // Lojistik icin
    public int? TransportMode { get; set; }
    public string? Incoterms { get; set; }
    public DateTime? DesiredPickupDate { get; set; }
    public DateTime? DesiredDeliveryDate { get; set; }

    // Gumruk icin
    public int? CustomsOperationType { get; set; }

    // Sigorta icin
    public int? InsuranceType { get; set; }

    // Genel
    public string? Notes { get; set; }
}

/// <summary>
/// Finansman istegi detayi
/// </summary>
public class FinancingRequestDto
{
    /// <summary>
    /// Finansman konusu (FinancingSubjects)
    /// 1: ProductCost (Mal Bedeli)
    /// 2: Logistics (Lojistik/Navlun)
    /// 3: Insurance (Sigorta)
    /// 4: ExportCustoms (Ihracat Gumruk)
    /// 5: ImportCustoms (Ithalat Gumruk)
    /// </summary>
    public int? FinancingSubject { get; set; }

    public decimal? RequestedAmount { get; set; }
    public int DurationDays { get; set; } = 30;
    public decimal? MaxInterestRate { get; set; }
    public DateTime? OfferDeadline { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Checkout baslatma sonucu
/// </summary>
public class CheckoutResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public List<int> ServiceRequestIds { get; set; } = new();
    public int? FinancingRequestId { get; set; }

    // Bir sonraki adim
    public string NextStep { get; set; } = string.Empty; // "AwaitingQuotes", "Payment"
}

/// <summary>
/// Siparis icin gelen teklifler
/// </summary>
public class OrderQuotesDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int OrderStatus { get; set; }
    public string OrderStatusName { get; set; } = string.Empty;

    public decimal ProductTotal { get; set; }
    public string Currency { get; set; } = "TRY";

    public List<ServiceRequestQuotesDto> ServiceRequests { get; set; } = new();
    public List<InvestmentOfferDto> InvestmentOffers { get; set; } = new();

    public decimal TotalWithServices { get; set; }
    public bool AllServicesSelected { get; set; }
    public bool CanProceedToPayment { get; set; }
}

/// <summary>
/// Hizmet talebi ve teklifleri
/// </summary>
public class ServiceRequestQuotesDto
{
    public int Id { get; set; }
    public int ServiceType { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    public string ServiceTypeIcon { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;

    public int? SelectedQuoteId { get; set; }
    public decimal? SelectedQuoteAmount { get; set; }

    public List<ServiceQuoteItemDto> Quotes { get; set; } = new();
}

/// <summary>
/// Servis teklifi
/// </summary>
public class ServiceQuoteItemDto
{
    public int Id { get; set; }
    public int ProviderVendorId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string? ProviderLogo { get; set; }
    public decimal? ProviderRating { get; set; }

    public decimal QuoteAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public int? EstimatedDays { get; set; }

    public string? IncludedServices { get; set; }
    public string? Notes { get; set; }
    public DateTime? ValidUntil { get; set; }

    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

/// <summary>
/// Yatirim teklifi (finansman icin)
/// </summary>
public class InvestmentOfferDto
{
    public int Id { get; set; }
    public int InvestorVendorId { get; set; }
    public string InvestorName { get; set; } = string.Empty;

    public decimal OfferedAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal TotalRepaymentAmount { get; set; }
    public string Currency { get; set; } = "TRY";

    public string? Notes { get; set; }
    public DateTime? ValidUntil { get; set; }

    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

/// <summary>
/// Checkout tamamlama sonucu
/// </summary>
public class CheckoutCompleteDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";

    // Stripe odeme bilgileri
    public string? PaymentIntentClientSecret { get; set; }
    public string? PaymentUrl { get; set; }
}
