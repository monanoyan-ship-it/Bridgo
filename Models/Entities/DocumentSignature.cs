using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

/// <summary>
/// Evrak imzalari - Elektronik onay veya dijital imza kayitlari
/// </summary>
public class DocumentSignature : BaseEntity
{
    /// <summary>
    /// Imzalanan evrak ID
    /// </summary>
    public int GeneratedDocumentId { get; set; }

    /// <summary>
    /// Imzalayan firma
    /// </summary>
    public int SignerVendorId { get; set; }

    /// <summary>
    /// Imzalayan kullanici
    /// </summary>
    public int SignerUserId { get; set; }

    // =======================================
    // Imza Detaylari
    // =======================================

    /// <summary>
    /// Imza tipi (SignatureTypes'dan)
    /// </summary>
    public int SignatureType { get; set; }

    /// <summary>
    /// Imza verisi (Base64 encoded signature veya hash)
    /// </summary>
    public string? SignatureData { get; set; }

    /// <summary>
    /// E-imza sertifika bilgisi (nitelikli imza icin)
    /// </summary>
    public string? CertificateInfo { get; set; }

    /// <summary>
    /// Imzalayanin IP adresi
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Imzalayanin tarayici bilgisi
    /// </summary>
    public string? UserAgent { get; set; }

    // =======================================
    // Dogrulama
    // =======================================

    /// <summary>
    /// Benzersiz dogrulama kodu (UUID)
    /// </summary>
    public string? VerificationCode { get; set; }

    /// <summary>
    /// Imza dogrulandi mi?
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Dogrulama tarihi
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    // =======================================
    // Zaman
    // =======================================

    /// <summary>
    /// Imza tarihi/saati
    /// </summary>
    public DateTime SignedAt { get; set; }

    // =======================================
    // Navigation Properties
    // =======================================
    public GeneratedDocument? Document { get; set; }
    public Vendor? SignerVendor { get; set; }
    public ApplicationUser? SignerUser { get; set; }
}
