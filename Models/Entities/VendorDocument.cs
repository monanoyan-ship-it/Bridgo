namespace Bridgo.Models.Entities;

/// <summary>
/// Vendor KYC belgeleri
/// </summary>
public class VendorDocument : BaseEntity
{
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public int UploadedByUserId { get; set; }

    /// <summary>
    /// Belge kategorisi: identity, company, bank, auth
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Belge tipi: tc_id, passport, tax_plate, iban_document, vb.
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public long? FileSize { get; set; }

    /// <summary>
    /// pending, approved, rejected
    /// </summary>
    public string Status { get; set; } = "pending";

    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }
}
