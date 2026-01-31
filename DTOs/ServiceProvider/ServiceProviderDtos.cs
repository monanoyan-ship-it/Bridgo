namespace Bridgo.DTOs.ServiceProvider;

/// <summary>
/// Servis saglayiciya gelen talep listesi
/// </summary>
public class ServiceRequestListDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int ServiceType { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    public string ServiceTypeIcon { get; set; } = string.Empty;

    // Alici bilgisi
    public int BuyerVendorId { get; set; }
    public string BuyerName { get; set; } = string.Empty;

    // Kargo detaylari
    public string? OriginCity { get; set; }
    public string? OriginCountry { get; set; }
    public string? DestinationCity { get; set; }
    public string? DestinationCountry { get; set; }

    public decimal? WeightKg { get; set; }
    public decimal? VolumeM3 { get; set; }
    public decimal? CargoValue { get; set; }
    public string? Currency { get; set; }
    public string? HsCode { get; set; }

    public int? TransportMode { get; set; }
    public string? TransportModeName { get; set; }
    public string? Incoterms { get; set; }

    public DateTime? DesiredPickupDate { get; set; }
    public DateTime? DesiredDeliveryDate { get; set; }

    // Durum
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int QuoteCount { get; set; }
    public bool HasMyQuote { get; set; }
    public int? MyQuoteId { get; set; }
    public decimal? MyQuoteAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Servis talebi detayi
/// </summary>
public class ServiceRequestDetailDto : ServiceRequestListDto
{
    // Siparis kalemleri
    public List<ServiceRequestItemDto> OrderItems { get; set; } = new();

    // Adres detaylari
    public string? OriginFullAddress { get; set; }
    public string? DestinationFullAddress { get; set; }

    // Ek bilgiler
    public string? Notes { get; set; }
    public int? CustomsOperationType { get; set; }
    public string? CustomsOperationTypeName { get; set; }
    public int? InsuranceType { get; set; }
    public string? InsuranceTypeName { get; set; }
}

/// <summary>
/// Siparis kalemi (teklif icin)
/// </summary>
public class ServiceRequestItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "TRY";
}

/// <summary>
/// Teklif olusturma DTO
/// </summary>
public class CreateQuoteDto
{
    public int ServiceRequestId { get; set; }
    public decimal QuoteAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public int? EstimatedDays { get; set; }
    public string? IncludedServices { get; set; }
    public string? Notes { get; set; }
    public DateTime? ValidUntil { get; set; }
}

/// <summary>
/// Teklif guncelleme DTO
/// </summary>
public class UpdateQuoteDto
{
    public decimal QuoteAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public int? EstimatedDays { get; set; }
    public string? IncludedServices { get; set; }
    public string? Notes { get; set; }
    public DateTime? ValidUntil { get; set; }
}

/// <summary>
/// Verdigi teklifler listesi
/// </summary>
public class MyQuoteListDto
{
    public int Id { get; set; }
    public int ServiceRequestId { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int ServiceType { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;

    public string BuyerName { get; set; } = string.Empty;

    public decimal QuoteAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public int? EstimatedDays { get; set; }

    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsSelected { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ValidUntil { get; set; }
}

// ============================================================
// PROVIDER JOB DTOs - Aktif isler
// ============================================================

/// <summary>
/// Provider is listesi (Aktif ve tamamlanmis)
/// </summary>
public class ProviderJobListDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int Role { get; set; }
    public string RoleName { get; set; } = string.Empty;

    // Alici/Satici
    public int BuyerVendorId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;

    // Hedef
    public string? DestinationCity { get; set; }
    public string? DestinationCountry { get; set; }

    // Tutar
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal NetAmount { get; set; }

    // Durum
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCssClass { get; set; } = "bg-secondary";
    public bool IsTaskCompleted { get; set; }
    public DateTime? TaskCompletedAt { get; set; }

    // Servis bilgisi
    public int? ServiceRequestId { get; set; }
    public int? ServiceType { get; set; }
    public string? ServiceTypeName { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Provider is detayi
/// </summary>
public class ProviderJobDetailDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int Role { get; set; }
    public string RoleName { get; set; } = string.Empty;

    // Alici firma
    public int BuyerVendorId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }
    public string? BuyerPhone { get; set; }

    // Satici firma
    public int SellerVendorId { get; set; }
    public string SellerName { get; set; } = string.Empty;

    // Adres
    public string? DestinationAddress { get; set; }

    // Tutar
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal CommissionAmount { get; set; }
    public decimal NetAmount { get; set; }

    // Durum
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsTaskCompleted { get; set; }
    public DateTime? TaskCompletedAt { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }

    // Servis detaylari
    public int? ServiceRequestId { get; set; }
    public int? ServiceType { get; set; }
    public string? ServiceTypeName { get; set; }

    // Gumruk ozel
    public int? CustomsOperationType { get; set; }
    public string? CustomsOperationTypeName { get; set; }

    // Sigorta ozel
    public int? InsuranceType { get; set; }
    public string? InsuranceTypeName { get; set; }
    public decimal? CoverageAmount { get; set; }

    // Kargo bilgileri
    public decimal? CargoValue { get; set; }
    public decimal? WeightKg { get; set; }
    public string? HsCode { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Siparis kalemleri
    public List<ProviderJobOrderItemDto> OrderItems { get; set; } = new();

    // Gorevler
    public List<ProviderJobTaskDto> Tasks { get; set; } = new();
}

/// <summary>
/// Provider is - siparis kalemi
/// </summary>
public class ProviderJobOrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

/// <summary>
/// Provider is - gorev
/// </summary>
public class ProviderJobTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
}
