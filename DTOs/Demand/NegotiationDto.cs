namespace Bridgo.DTOs.Demand;

// ============================================
// NEGOTIATION ROUND DTO'lari
// ============================================

/// <summary>
/// Pazarlik turu liste gorunumu
/// </summary>
public class NegotiationRoundListDto
{
    public int Id { get; set; }
    public int DemandResponseId { get; set; }
    public int RoundNumber { get; set; }
    public int InitiatorVendorId { get; set; }
    public string? InitiatorName { get; set; }
    public bool IsInitiatorBuyer { get; set; }

    // Teklif detaylari
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public int? LeadTimeDays { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Notes { get; set; }

    // Durum
    public int Status { get; set; }
    public string StatusName { get; set; } = "";
    public string StatusCssClass { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt && Status == 1;
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

/// <summary>
/// Pazarlik turu detay gorunumu
/// </summary>
public class NegotiationRoundDetailDto : NegotiationRoundListDto
{
    public string? TermsAndConditions { get; set; }
    public string? RejectionReason { get; set; }
}

/// <summary>
/// Karsi teklif olusturma DTO
/// </summary>
public class CounterOfferCreateDto
{
    public int DemandResponseId { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Currency { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public int? LeadTimeDays { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
}

/// <summary>
/// Pazarlik durumu ozet DTO
/// </summary>
public class NegotiationSummaryDto
{
    public int DemandResponseId { get; set; }
    public bool IsNegotiationActive { get; set; }
    public int CurrentRoundNumber { get; set; }
    public int? CurrentTurnVendorId { get; set; }
    public string? CurrentTurnVendorName { get; set; }
    public bool IsMyTurn { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
    public string? TimeRemaining { get; set; }

    // Son teklif
    public NegotiationRoundListDto? LastRound { get; set; }
    public int? LastRoundId => LastRound?.Id;

    // Istatistikler
    public int TotalRounds { get; set; }
    public decimal? InitialPrice { get; set; }
    public decimal? CurrentPrice { get; set; }
    public decimal? PriceChangePercent { get; set; }
    public string Currency { get; set; } = "TRY";
    public int? Quantity { get; set; }
    public int? LeadTimeDays { get; set; }

    // JS uyumluluk alias
    public int CurrentRound => CurrentRoundNumber;
    public decimal? OriginalPrice => InitialPrice;
}

/// <summary>
/// Pazarlik gecmisi DTO
/// </summary>
public class NegotiationHistoryDto
{
    public int DemandResponseId { get; set; }
    public string DemandTitle { get; set; } = "";
    public int BuyerVendorId { get; set; }
    public string BuyerName { get; set; } = "";
    public int? SupplierVendorId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime CreatedAt { get; set; }

    // Baslangic teklifi (DemandResponse'dan)
    public decimal? InitialUnitPrice { get; set; }
    public decimal? InitialTotalPrice { get; set; }
    public string? InitialCurrency { get; set; }
    public int? InitialQuantity { get; set; }
    public string? InitialUnit { get; set; }
    public int? InitialLeadTimeDays { get; set; }

    // JS uyumluluk aliaslar
    public string? SellerCompanyName => SupplierName;
    public decimal? OriginalUnitPrice => InitialUnitPrice;
    public decimal? OriginalTotalPrice => InitialTotalPrice;
    public string? Currency => InitialCurrency;
    public int? OriginalQuantity => InitialQuantity;
    public string? Unit => InitialUnit;
    public int? OriginalLeadTimeDays => InitialLeadTimeDays;

    // Tum turlar
    public List<NegotiationRoundListDto> Rounds { get; set; } = new();

    // Ozet
    public NegotiationSummaryDto Summary { get; set; } = new();
}

/// <summary>
/// Karsi teklif yanitlama DTO
/// </summary>
public class CounterOfferResponseDto
{
    public int RoundId { get; set; }

    /// <summary>
    /// "accept", "reject", "counter"
    /// </summary>
    public string Action { get; set; } = "";
    public string? RejectionReason { get; set; }

    // Counter icin
    public CounterOfferCreateDto? CounterOffer { get; set; }
}

/// <summary>
/// Pazarlik red DTO
/// </summary>
public class NegotiationRejectDto
{
    public string? Reason { get; set; }
}

/// <summary>
/// Aktif pazarlik ozeti (liste icin)
/// </summary>
public class ActiveNegotiationDto
{
    public int DemandResponseId { get; set; }
    public int DemandId { get; set; }
    public string DemandTitle { get; set; } = "";
    public string? CounterpartyName { get; set; }
    public int CurrentRoundNumber { get; set; }
    public decimal? CurrentPrice { get; set; }
    public string? Currency { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? TimeRemaining { get; set; }
    public bool IsMyTurn { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = "";
    public string StatusCssClass { get; set; } = "";
}
