namespace Bridgo.DTOs.Proposal;

// ============================================
// UNIFIED PROPOSAL DTOs
// Satıcının verdiği tüm teklifleri birleşik gösterim
// ============================================

/// <summary>
/// Birleşik teklif listesi (ProductInquiryResponse + DemandResponse)
/// </summary>
public class UnifiedProposalListDto
{
    public int Id { get; set; }

    /// <summary>
    /// Kaynak tipi: "inquiry" veya "demand"
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// Kaynak ID (InquiryId veya DemandId)
    /// </summary>
    public int SourceId { get; set; }

    /// <summary>
    /// Kaynak başlığı (Ürün adı veya Talep başlığı)
    /// </summary>
    public string SourceTitle { get; set; } = string.Empty;

    /// <summary>
    /// Kaynak görseli (Ürün resmi veya null)
    /// </summary>
    public string? SourceImageUrl { get; set; }

    // Alıcı bilgisi
    public int BuyerVendorId { get; set; }
    public string? BuyerCompanyName { get; set; }

    // Fiyat bilgisi
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string Currency { get; set; } = "TRY";

    // Miktar
    public int? OfferedQuantity { get; set; }
    public string? OfferedUnit { get; set; }

    // Teslim
    public int? LeadTimeDays { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ValidUntil { get; set; }

    // Durum
    public int Status { get; set; }
    public string? StatusText { get; set; }

    // Okunma
    public bool IsReadByBuyer { get; set; }
    public DateTime? ReadByBuyerAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Birleşik teklif arama sonucu
/// </summary>
public class UnifiedProposalSearchResultDto
{
    public List<UnifiedProposalListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

/// <summary>
/// Birleşik teklif filtresi
/// </summary>
public class UnifiedProposalFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public int? Status { get; set; }

    /// <summary>
    /// null = hepsi, "inquiry" = sadece ürün teklifleri, "demand" = sadece talep teklifleri
    /// </summary>
    public string? SourceType { get; set; }

    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Birleşik teklif istatistikleri
/// </summary>
public class UnifiedProposalStatsDto
{
    // Toplam
    public int TotalProposals { get; set; }
    public int PendingProposals { get; set; }
    public int AcceptedProposals { get; set; }
    public int RejectedProposals { get; set; }

    // Kaynak bazlı
    public int InquiryProposals { get; set; }
    public int DemandProposals { get; set; }

    // Bu ay
    public int ThisMonthProposals { get; set; }
    public decimal ThisMonthValue { get; set; }
}
