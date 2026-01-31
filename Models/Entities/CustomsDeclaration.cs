namespace Bridgo.Models.Entities;

/// <summary>
/// Gumruk beyannamesi - Evrim API entegrasyonu icin
/// </summary>
public class CustomsDeclaration : BaseEntity
{
    /// <summary>
    /// Ilgili siparis
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Beyanname tipi (CustomsDeclarationTypes)
    /// 1: Export (Ihracat), 2: Import (Ithalat), 3: ETGB
    /// </summary>
    public int DeclarationTypeId { get; set; }

    /// <summary>
    /// Evrim dosya numarasi (Evrim'in verdigi internal numara)
    /// </summary>
    public string? EvrimDosyaNo { get; set; }

    /// <summary>
    /// Evrim dosya tipi (IHR, ITH, ETGB)
    /// </summary>
    public string? EvrimDosyaTipi { get; set; }

    /// <summary>
    /// Tescil edilmis beyanname numarasi (BILGE'den gelen)
    /// </summary>
    public string? DeclarationNumber { get; set; }

    /// <summary>
    /// Beyanname durumu (CustomsDeclarationStatuses)
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Tescil tarihi
    /// </summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>
    /// Kapanis tarihi
    /// </summary>
    public DateTime? ClosingDate { get; set; }

    /// <summary>
    /// Gumruk idaresi kodu (4 haneli)
    /// </summary>
    public string? CustomsOfficeCode { get; set; }

    /// <summary>
    /// Gumruk idaresi adi
    /// </summary>
    public string? CustomsOfficeName { get; set; }

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
    /// Odenmesi gereken vergi tutari
    /// </summary>
    public decimal? TaxDue { get; set; }

    /// <summary>
    /// Odenen vergi tutari
    /// </summary>
    public decimal? TaxPaid { get; set; }

    /// <summary>
    /// Evrim'den gelen son JSON yanit (loglama icin)
    /// </summary>
    public string? EvrimResponse { get; set; }

    /// <summary>
    /// Son senkronizasyon tarihi
    /// </summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>
    /// Son hata mesaji
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Notlar
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Olusturan kullanici
    /// </summary>
    public int? CreatedByUserId { get; set; }

    // Navigation properties
    public virtual Order? Order { get; set; }
}
