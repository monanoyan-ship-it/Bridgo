namespace Bridgo.DTOs.Evrim;

/// <summary>
/// Evrim belge yukleme istegi
/// </summary>
public class EvrimDocumentUploadRequest
{
    /// <summary>
    /// Evrim dosya numarasi
    /// </summary>
    public string DosyaNo { get; set; } = string.Empty;

    /// <summary>
    /// Dosya tipi (IHR, ITH, ETGB)
    /// </summary>
    public string DosyaTipi { get; set; } = "IHR";

    /// <summary>
    /// Belge tipi kodu
    /// Ornek: INV (Fatura), PKL (Paket Listesi), CMR, BL (Konşimento), vb.
    /// </summary>
    public string BelgeTipiKodu { get; set; } = string.Empty;

    /// <summary>
    /// Belge numarasi
    /// </summary>
    public string? BelgeNo { get; set; }

    /// <summary>
    /// Belge tarihi
    /// </summary>
    public DateTime? BelgeTarihi { get; set; }

    /// <summary>
    /// Belge dosya adi
    /// </summary>
    public string DosyaAdi { get; set; } = string.Empty;

    /// <summary>
    /// Belge icerigi (Base64)
    /// </summary>
    public string DosyaIcerik { get; set; } = string.Empty;

    /// <summary>
    /// MIME tipi
    /// </summary>
    public string MimeType { get; set; } = "application/pdf";

    /// <summary>
    /// Aciklama
    /// </summary>
    public string? Aciklama { get; set; }
}

/// <summary>
/// Evrim belge yukleme yaniti
/// </summary>
public class EvrimDocumentUploadResponse
{
    /// <summary>
    /// Islem basarili mi
    /// </summary>
    public bool Basarili { get; set; }

    /// <summary>
    /// Evrim belge ID (GUID)
    /// </summary>
    public string? BelgeId { get; set; }

    /// <summary>
    /// Hata mesaji
    /// </summary>
    public string? HataMesaji { get; set; }
}

/// <summary>
/// Evrim belge listesi istegi
/// </summary>
public class EvrimDocumentListRequest
{
    /// <summary>
    /// Evrim dosya numarasi
    /// </summary>
    public string DosyaNo { get; set; } = string.Empty;

    /// <summary>
    /// Dosya tipi (IHR, ITH, ETGB)
    /// </summary>
    public string DosyaTipi { get; set; } = "IHR";
}

/// <summary>
/// Evrim belge listesi item
/// </summary>
public class EvrimDocumentListItem
{
    /// <summary>
    /// Belge ID (GUID)
    /// </summary>
    public string BelgeId { get; set; } = string.Empty;

    /// <summary>
    /// Belge tipi kodu
    /// </summary>
    public string BelgeTipiKodu { get; set; } = string.Empty;

    /// <summary>
    /// Belge tipi adi
    /// </summary>
    public string? BelgeTipiAdi { get; set; }

    /// <summary>
    /// Belge numarasi
    /// </summary>
    public string? BelgeNo { get; set; }

    /// <summary>
    /// Belge tarihi
    /// </summary>
    public DateTime? BelgeTarihi { get; set; }

    /// <summary>
    /// Dosya adi
    /// </summary>
    public string? DosyaAdi { get; set; }

    /// <summary>
    /// Dosya boyutu (byte)
    /// </summary>
    public long? DosyaBoyut { get; set; }

    /// <summary>
    /// Yukleme tarihi
    /// </summary>
    public DateTime? YuklemeTarihi { get; set; }

    /// <summary>
    /// Yukleyen kullanici
    /// </summary>
    public string? YukleyenKullanici { get; set; }
}

/// <summary>
/// Evrim belge indirme yaniti
/// </summary>
public class EvrimDocumentDownloadResponse
{
    /// <summary>
    /// Islem basarili mi
    /// </summary>
    public bool Basarili { get; set; }

    /// <summary>
    /// Dosya adi
    /// </summary>
    public string? DosyaAdi { get; set; }

    /// <summary>
    /// MIME tipi
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Dosya icerigi (byte array)
    /// </summary>
    public byte[]? DosyaIcerik { get; set; }

    /// <summary>
    /// Hata mesaji
    /// </summary>
    public string? HataMesaji { get; set; }
}

/// <summary>
/// Belge tipi tanimlari
/// </summary>
public static class EvrimDocumentTypes
{
    public const string Invoice = "INV";           // Ticari Fatura
    public const string PackingList = "PKL";       // Paket Listesi
    public const string BillOfLading = "BL";       // Konşimento (Deniz)
    public const string AirWaybill = "AWB";        // Havayolu Konşimentosu
    public const string CMR = "CMR";               // Karayolu Taşıma Belgesi
    public const string CertificateOfOrigin = "COO"; // Menşe Şahadetnamesi
    public const string EUR1 = "EUR1";             // EUR.1 Dolaşım Belgesi
    public const string ATR = "ATR";               // A.TR Dolaşım Belgesi
    public const string InsurancePolicy = "INS";   // Sigorta Poliçesi
    public const string ProformaInvoice = "PRF";   // Proforma Fatura
    public const string CustomsDeclaration = "DEC"; // Gümrük Beyannamesi
    public const string HealthCertificate = "HLT"; // Sağlık Sertifikası
    public const string QualityCertificate = "QC"; // Kalite Sertifikası
}
