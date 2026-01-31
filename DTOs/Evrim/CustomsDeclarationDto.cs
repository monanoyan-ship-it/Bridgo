namespace Bridgo.DTOs.Evrim;

/// <summary>
/// Gumruk beyannamesi olusturma istegi (Internal - UI'dan gelen)
/// </summary>
public class CreateCustomsDeclarationDto
{
    /// <summary>
    /// Siparis ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Beyanname tipi: 1=Export, 2=Import, 3=ETGB
    /// </summary>
    public int DeclarationTypeId { get; set; }

    /// <summary>
    /// Gumruk idaresi kodu (4 haneli)
    /// </summary>
    public string? CustomsOfficeCode { get; set; }

    /// <summary>
    /// Rejim kodu (4 haneli)
    /// </summary>
    public string? RegimeCode { get; set; }

    /// <summary>
    /// Teslim sekli (Incoterm)
    /// </summary>
    public string? DeliveryTerms { get; set; }

    /// <summary>
    /// Teslim yeri
    /// </summary>
    public string? DeliveryPlace { get; set; }

    /// <summary>
    /// Ek notlar
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Gumruk beyannamesi liste item (Internal - UI icin)
/// </summary>
public class CustomsDeclarationListDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? OrderNumber { get; set; }

    /// <summary>
    /// Beyanname tipi: Export, Import, ETGB
    /// </summary>
    public int DeclarationTypeId { get; set; }
    public string? DeclarationTypeText { get; set; }
    public string? DeclarationTypeIcon { get; set; }

    /// <summary>
    /// Evrim dosya numarasi
    /// </summary>
    public string? EvrimDosyaNo { get; set; }

    /// <summary>
    /// Tescil edilmis beyanname numarasi
    /// </summary>
    public string? DeclarationNumber { get; set; }

    /// <summary>
    /// Durum
    /// </summary>
    public int StatusId { get; set; }
    public string? StatusText { get; set; }
    public string? StatusCssClass { get; set; }
    public string? StatusIcon { get; set; }

    /// <summary>
    /// Gumruk idaresi
    /// </summary>
    public string? CustomsOffice { get; set; }

    /// <summary>
    /// Rejim kodu
    /// </summary>
    public string? RegimeCode { get; set; }

    /// <summary>
    /// Tescil tarihi
    /// </summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>
    /// Kapanis tarihi
    /// </summary>
    public DateTime? ClosingDate { get; set; }

    /// <summary>
    /// Son senkronizasyon
    /// </summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>
    /// Son hata mesaji
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Belge sayisi
    /// </summary>
    public int DocumentCount { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Gumruk beyannamesi detay (Internal - UI icin)
/// </summary>
public class CustomsDeclarationDetailDto : CustomsDeclarationListDto
{
    /// <summary>
    /// Evrim'den gelen ham JSON yanit
    /// </summary>
    public string? EvrimResponse { get; set; }

    /// <summary>
    /// Teslim sekli
    /// </summary>
    public string? DeliveryTerms { get; set; }

    /// <summary>
    /// Teslim yeri
    /// </summary>
    public string? DeliveryPlace { get; set; }

    /// <summary>
    /// Odenmesi gereken vergi
    /// </summary>
    public decimal? TaxDue { get; set; }

    /// <summary>
    /// Odenen vergi
    /// </summary>
    public decimal? TaxPaid { get; set; }

    /// <summary>
    /// Notlar
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Belgeler
    /// </summary>
    public List<CustomsDocumentDto> Documents { get; set; } = new();
}

/// <summary>
/// Gumruk belgesi (Internal - UI icin)
/// </summary>
public class CustomsDocumentDto
{
    /// <summary>
    /// Evrim belge GUID
    /// </summary>
    public string DocumentGuid { get; set; } = string.Empty;

    /// <summary>
    /// Belge tipi kodu
    /// </summary>
    public string DocumentTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Belge tipi adi
    /// </summary>
    public string? DocumentTypeName { get; set; }

    /// <summary>
    /// Belge numarasi
    /// </summary>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Belge tarihi
    /// </summary>
    public DateTime? DocumentDate { get; set; }

    /// <summary>
    /// Dosya adi
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Dosya boyutu (byte)
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// Yukleme tarihi
    /// </summary>
    public DateTime? UploadedAt { get; set; }
}

/// <summary>
/// Belge yukleme istegi (Internal - UI'dan gelen)
/// </summary>
public class UploadCustomsDocumentDto
{
    /// <summary>
    /// Beyanname ID
    /// </summary>
    public int DeclarationId { get; set; }

    /// <summary>
    /// Belge tipi kodu (INV, PKL, BL, vb.)
    /// </summary>
    public string DocumentTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Belge numarasi
    /// </summary>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Belge tarihi
    /// </summary>
    public DateTime? DocumentDate { get; set; }

    /// <summary>
    /// Dosya adi
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Base64 encoded dosya icerigi
    /// </summary>
    public string FileContent { get; set; } = string.Empty;

    /// <summary>
    /// MIME tipi
    /// </summary>
    public string MimeType { get; set; } = "application/pdf";

    /// <summary>
    /// Aciklama
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Baglanti testi yaniti
/// </summary>
public class ConnectionTestResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ApiVersion { get; set; }
    public DateTime? TestTime { get; set; }
}
