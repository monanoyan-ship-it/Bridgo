using Bridgo.Models.Entities;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Evrak olusturma ve imza yonetimi servisi
/// </summary>
public interface IDocumentGenerationService
{
    // =======================================
    // Evrak Olusturma
    // =======================================

    /// <summary>
    /// Islem tipine gore gerekli evraklari olustur
    /// </summary>
    Task<List<GeneratedDocument>> GenerateDocumentsForTransactionAsync(
        int transactionTypeId,
        int orderId,
        int vendorId,
        int? serviceRequestId = null);

    /// <summary>
    /// Tek evrak olustur
    /// </summary>
    Task<GeneratedDocument> GenerateDocumentAsync(
        int documentTypeId,
        int transactionTypeId,
        int vendorId,
        int? orderId = null,
        int? serviceRequestId = null,
        int? financingRequestId = null,
        int? counterpartyVendorId = null,
        Dictionary<string, object>? metadata = null);

    // =======================================
    // Evrak Sorgulama
    // =======================================

    /// <summary>
    /// Siparise ait evraklari getir
    /// </summary>
    Task<List<GeneratedDocument>> GetOrderDocumentsAsync(int orderId);

    /// <summary>
    /// Servis talebine ait evraklari getir
    /// </summary>
    Task<List<GeneratedDocument>> GetServiceRequestDocumentsAsync(int serviceRequestId);

    /// <summary>
    /// Finansman talebine ait evraklari getir
    /// </summary>
    Task<List<GeneratedDocument>> GetFinancingDocumentsAsync(int financingRequestId);

    /// <summary>
    /// Evrak detayi getir
    /// </summary>
    Task<GeneratedDocument?> GetDocumentByIdAsync(int documentId);

    /// <summary>
    /// Evrak numarasina gore evrak getir
    /// </summary>
    Task<GeneratedDocument?> GetDocumentByNumberAsync(string documentNumber);

    /// <summary>
    /// Vendor'in evraklarini getir
    /// </summary>
    Task<List<GeneratedDocument>> GetVendorDocumentsAsync(int vendorId, int? documentTypeId = null, int? status = null);

    // =======================================
    // Evrak Numaralandirma
    // =======================================

    /// <summary>
    /// Yeni evrak numarasi uret
    /// </summary>
    Task<string> GenerateDocumentNumberAsync(int documentTypeId);

    // =======================================
    // Imza Islemleri
    // =======================================

    /// <summary>
    /// Evragi imzala (elektronik onay)
    /// </summary>
    Task<DocumentSignature> SignDocumentAsync(
        int documentId,
        int vendorId,
        int userId,
        int signatureType,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Imza dogrula
    /// </summary>
    Task<bool> VerifySignatureAsync(string verificationCode);

    /// <summary>
    /// Evrakin imza durumunu kontrol et
    /// </summary>
    Task<DocumentSignatureStatusDto> GetDocumentSignatureStatusAsync(int documentId);

    /// <summary>
    /// Kullanicinin imzalamasi gereken evraklari getir
    /// </summary>
    Task<List<GeneratedDocument>> GetPendingSignatureDocumentsAsync(int vendorId);

    // =======================================
    // Sablon Yonetimi
    // =======================================

    /// <summary>
    /// Evrak tipi icin aktif sablonu getir
    /// </summary>
    Task<DocumentTemplate?> GetActiveTemplateAsync(int documentTypeId);

    /// <summary>
    /// Tum aktif sablonlari getir
    /// </summary>
    Task<List<DocumentTemplate>> GetActiveTemplatesAsync();

    /// <summary>
    /// Islem tipine gore gerekli evrak tiplerini getir
    /// </summary>
    Task<List<TransactionDocumentMapping>> GetRequiredDocumentsForTransactionAsync(int transactionTypeId, int? capabilityId = null);

    // =======================================
    // Kilitleme Islemleri
    // =======================================

    /// <summary>
    /// Evrak imzalandiktan sonra ilgili servis talebini kilitle
    /// </summary>
    Task LockServiceRequestAfterSignatureAsync(int documentId);

    /// <summary>
    /// Servis talebinin kilit durumunu kontrol et
    /// </summary>
    Task<bool> IsServiceRequestLockedAsync(int serviceRequestId);
}

/// <summary>
/// Evrak imza durumu DTO
/// </summary>
public class DocumentSignatureStatusDto
{
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public int RequiredSignatures { get; set; }
    public int CompletedSignatures { get; set; }
    public bool IsFullySigned { get; set; }
    public List<SignatureInfoDto> Signatures { get; set; } = new();
}

/// <summary>
/// Imza bilgisi DTO
/// </summary>
public class SignatureInfoDto
{
    public int SignatureId { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int SignatureType { get; set; }
    public string SignatureTypeText { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
    public bool IsVerified { get; set; }
    public string? VerificationCode { get; set; }
}
