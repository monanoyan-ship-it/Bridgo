using System.ComponentModel.DataAnnotations;
using Bridgo.DTOs.Team;
using Bridgo.Models.Identity;

namespace Bridgo.DTOs.Vendor;

/// <summary>
/// VendorSetup icin DTO - Yeni kullanicinin ilk Vendor + Address olusturmasi
/// </summary>
public class VendorSetupDto
{
    // === VENDOR BILGILERI ===
    [Required(ErrorMessage = "Sirket adi zorunludur")]
    [StringLength(200, ErrorMessage = "Sirket adi en fazla 200 karakter olabilir")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon zorunludur")]
    [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir")]
    public string Phone { get; set; } = string.Empty;

    // === ADRES BILGILERI ===
    [Required(ErrorMessage = "Adres basligi zorunludur")]
    [StringLength(100, ErrorMessage = "Adres basligi en fazla 100 karakter olabilir")]
    public string AddressTitle { get; set; } = "Merkez";

    [Required(ErrorMessage = "Ulke zorunludur")]
    public int CountryId { get; set; }

    public int? StateId { get; set; }

    [Required(ErrorMessage = "Il zorunludur")]
    [StringLength(100, ErrorMessage = "Il en fazla 100 karakter olabilir")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ilce zorunludur")]
    [StringLength(100, ErrorMessage = "Ilce en fazla 100 karakter olabilir")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adres zorunludur")]
    [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
    public string AddressLine { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "Posta kodu en fazla 10 karakter olabilir")]
    public string? PostalCode { get; set; }

    public int AddressTypeId { get; set; } = 3; // Default: Headquarters
}

/// <summary>
/// VendorSetup sonuc DTO
/// </summary>
public class VendorSetupResultDto
{
    public int VendorId { get; set; }
    public int AddressId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Vendor bilgi DTO (read-only)
/// </summary>
public class VendorDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? TradeRegistryNo { get; set; }
    public string? MersisNo { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }

    // Dış Ticaret Bilgileri
    public string? EoriNumber { get; set; }         // AB için EORI
    public string? EInvoiceId { get; set; }         // e-Fatura PK/GB
    public string? KepAddress { get; set; }         // KEP adresi
    public string? CustomsBrokerCode { get; set; }  // Gümrük müşaviri kodu

    // Yetkili Kişi
    public string? AuthorizedPersonName { get; set; }
    public string? AuthorizedPersonTitle { get; set; }
    public string? AuthorizedPersonPhone { get; set; }
    public string? AuthorizedPersonEmail { get; set; }
    public string? AuthorizedPersonTaxNo { get; set; }

    // Banka
    public string? BankName { get; set; }
    public string? BankBranch { get; set; }
    public string? Iban { get; set; }

    // Durum
    public int VendorStatusId { get; set; }
    public string? VendorStatusCode { get; set; }
    public string? VendorStatusName { get; set; }
    public bool IsProfileComplete { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Address bilgi DTO (read-only)
/// </summary>
public class AddressDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public int AddressTypeId { get; set; }
    public string? AddressTypeCode { get; set; }
    public string? AddressTypeName { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? CountryId { get; set; }
    public string Country { get; set; } = string.Empty;
    public int? StateId { get; set; }
    public string? State { get; set; }  // Eyalet (ABD, Almanya vb. icin)
    public string City { get; set; } = string.Empty;  // Il
    public string District { get; set; } = string.Empty;  // Ilce
    public string AddressLine { get; set; } = string.Empty;
    public string? AddressDescription { get; set; }
    public string? PostalCode { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string FullAddress { get; set; } = string.Empty;
}

// ========================================
// VENDOR SEARCH DTOs
// ========================================

/// <summary>
/// Vendor arama sonucu (listede gosterilecek)
/// </summary>
public class VendorSearchResultDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsVerified { get; set; }
}

/// <summary>
/// Domain esleme sonucu - VendorSetup sayfasinda kullanilir
/// NOT: Join request islemleri icin Bridgo.DTOs.Team kullaniniz
/// </summary>
public class DomainMatchResultDto
{
    /// <summary>
    /// E-posta domain'i kurumsal mi? (gmail, hotmail vb. degil)
    /// </summary>
    public bool IsCorporateEmail { get; set; }

    /// <summary>
    /// Eslesen vendor bulundu mu?
    /// </summary>
    public bool HasMatch { get; set; }

    /// <summary>
    /// Eslesen vendor (varsa)
    /// </summary>
    public VendorSearchResultDto? MatchedVendor { get; set; }

    /// <summary>
    /// Kullanicinin mevcut bekleyen istegi var mi?
    /// </summary>
    public bool HasPendingRequest { get; set; }

    /// <summary>
    /// Bekleyen istek (varsa) - TeamMemberDto kullanir
    /// </summary>
    public TeamMemberDto? PendingRequest { get; set; }
}
