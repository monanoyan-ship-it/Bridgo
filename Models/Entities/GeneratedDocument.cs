namespace Bridgo.Models.Entities;

/// <summary>
/// Olusturulan evraklar - PDF olarak uretilen dokuman kayitlari
/// </summary>
public class GeneratedDocument : BaseEntity
{
    // =======================================
    // Iliskiler
    // =======================================

    /// <summary>
    /// Siparis ID (optional)
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>
    /// Servis talep ID (optional)
    /// </summary>
    public int? OrderServiceRequestId { get; set; }

    /// <summary>
    /// Finansman talep ID (optional)
    /// </summary>
    public int? FinancingRequestId { get; set; }

    // =======================================
    // Evrak Bilgileri
    // =======================================

    /// <summary>
    /// Evrak tipi (DocumentTypes'dan)
    /// </summary>
    public int DocumentTypeId { get; set; }

    /// <summary>
    /// Islem tipi (TransactionTypes'dan)
    /// </summary>
    public int TransactionTypeId { get; set; }

    /// <summary>
    /// Evrak numarasi (SZL-2024-000001)
    /// </summary>
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Evrak basligi
    /// </summary>
    public string? Title { get; set; }

    // =======================================
    // Dosya Bilgileri
    // =======================================

    /// <summary>
    /// PDF dosya yolu (sunucuda)
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Dosya URL'i (erisim icin)
    /// </summary>
    public string? FileUrl { get; set; }

    /// <summary>
    /// Dosya boyutu (byte)
    /// </summary>
    public long? FileSize { get; set; }

    // =======================================
    // Durum
    // =======================================

    /// <summary>
    /// Evrak durumu (DocumentStatuses'dan)
    /// </summary>
    public int Status { get; set; }

    // =======================================
    // Taraflar
    // =======================================

    /// <summary>
    /// Evraki olusturan firma
    /// </summary>
    public int CreatedByVendorId { get; set; }

    /// <summary>
    /// Karsi taraf firma (imza icin)
    /// </summary>
    public int? CounterpartyVendorId { get; set; }

    // =======================================
    // Imza Bilgileri
    // =======================================

    /// <summary>
    /// Gereken imza sayisi
    /// </summary>
    public int RequiredSignatures { get; set; } = 1;

    /// <summary>
    /// Tamamlanan imza sayisi
    /// </summary>
    public int CompletedSignatures { get; set; }

    // =======================================
    // Metadata
    // =======================================

    /// <summary>
    /// Evrak icerigi icin dinamik data (JSON)
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Evragin son gecerlilik tarihi
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    // =======================================
    // Navigation Properties
    // =======================================
    public Order? Order { get; set; }
    public OrderServiceRequest? ServiceRequest { get; set; }
    public FinancingRequest? FinancingRequest { get; set; }
    public Vendor? CreatedByVendor { get; set; }
    public Vendor? CounterpartyVendor { get; set; }
    public ICollection<DocumentSignature> Signatures { get; set; } = new List<DocumentSignature>();
}
