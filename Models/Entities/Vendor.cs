using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

public class Vendor : BaseEntity
{
    // === TEMEL BILGILER (Kayitta alinacak) ===
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // === SIRKET BILGILERI (Sonra doldurulacak) ===
    public string? TaxNumber { get; set; }           // Vergi No (VKN/TCKN)
    public string? TaxOffice { get; set; }           // Vergi Dairesi
    public int? BillingAddressId { get; set; }       // Fatura adresi ID
    public string? TradeRegistryNo { get; set; }    // Ticaret Sicil No
    public string? MersisNo { get; set; }           // MERSIS No
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }

    // === DIŞ TİCARET BİLGİLERİ ===
    /// <summary>
    /// EORI Numarasi - Economic Operators Registration and Identification
    /// AB ile ticaret icin zorunlu. Format: TR + VKN (ornek: TR1234567890)
    /// </summary>
    public string? EoriNumber { get; set; }

    /// <summary>
    /// e-Fatura Posta Kutusu (PK) veya Ozel Entegrator Etiket (GB)
    /// e-Fatura gonderimi icin gerekli
    /// </summary>
    public string? EInvoiceId { get; set; }

    /// <summary>
    /// KEP (Kayitli Elektronik Posta) Adresi
    /// Resmi elektronik tebligat icin kullanilir
    /// </summary>
    public string? KepAddress { get; set; }

    /// <summary>
    /// Gumruk Musaviri Firma Kodu
    /// Gumruk islemlerinde kullanilir
    /// </summary>
    public string? CustomsBrokerCode { get; set; }

    // === YETKILI KISI BILGILERI ===
    public string? AuthorizedPersonName { get; set; }
    public string? AuthorizedPersonTitle { get; set; }  // Unvan (Genel Mudur, vb.)
    public string? AuthorizedPersonPhone { get; set; }
    public string? AuthorizedPersonEmail { get; set; }

    /// <summary>
    /// Yetkili kisi TC Kimlik No veya Vergi No
    /// Gumruk beyannamesinde mali sorumlu VKN olarak kullanilir
    /// </summary>
    public string? AuthorizedPersonTaxNo { get; set; }

    // === BANKA BILGILERI ===
    public string? BankName { get; set; }
    public string? BankBranch { get; set; }
    public string? Iban { get; set; }
    public string? AccountHolder { get; set; }

    // === OPERASYON BILGILERI ===
    public string? ActiveCountries { get; set; }  // Virgul ile ayrilmis ulke kodlari: TR,DE,GB

    // === DURUM BILGILERI ===
    /// <summary>
    /// Firma durumu (VendorStatuses static class'tan ID)
    /// </summary>
    public int VendorStatusId { get; set; } = 1; // Default: PendingProfile

    public bool IsProfileComplete { get; set; } = false;
    public bool IsVerified { get; set; } = false;        // Admin tarafindan dogrulandi mi
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    public string? RejectionReason { get; set; }

    // === NAVIGATION PROPERTIES ===
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<VendorCapabilityMapping> Capabilities { get; set; } = new List<VendorCapabilityMapping>();
    public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
