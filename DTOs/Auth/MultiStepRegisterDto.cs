using System.ComponentModel.DataAnnotations;

namespace Bridgo.DTOs.Auth;

/// <summary>
/// Multi-step register DTO
/// Adim 1: Hesap, Adim 2: Firma, Adim 3: Adres, Adim 4: Kullanim Amaci, Adim 5: Ulkeler
/// </summary>
public class MultiStepRegisterDto
{
    // ===== ADIM 1: HESAP BILGILERI =====
    [Required(ErrorMessage = "Ad gereklidir")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad gereklidir")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre gereklidir")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Sifre en az 6 karakter olmalidir")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre tekrari gereklidir")]
    [Compare("Password", ErrorMessage = "Sifreler eslesmedi")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    // ===== ADIM 2: FIRMA BILGILERI =====
    [Required(ErrorMessage = "Firma adi gereklidir")]
    [StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Url]
    [StringLength(200)]
    public string? Website { get; set; }

    // ===== ADIM 3: ADRES BILGILERI =====
    [Required(ErrorMessage = "Ulke gereklidir")]
    public int CountryId { get; set; }

    public int? StateId { get; set; }

    [Required(ErrorMessage = "Il gereklidir")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ilce gereklidir")]
    [StringLength(100)]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adres gereklidir")]
    [StringLength(500)]
    public string AddressLine { get; set; } = string.Empty;

    [StringLength(10)]
    public string? PostalCode { get; set; }

    // ===== ADIM 4: KULLANIM AMACI =====
    /// <summary>
    /// Secilen capability ID'leri (Capabilities.Ids'den)
    /// </summary>
    [Required(ErrorMessage = "En az bir kullanim amaci secmelisiniz")]
    public List<int> SelectedCapabilities { get; set; } = new();

    // ===== ADIM 5: AKTIF ULKELER =====
    /// <summary>
    /// Secilen ulke kodlari: TR, DE, GB, FR, NL, vb.
    /// </summary>
    [Required(ErrorMessage = "En az bir ulke secmelisiniz")]
    public List<string> SelectedCountries { get; set; } = new();
}

/// <summary>
/// Ulke listesi
/// </summary>
public static class CountryList
{
    public static readonly List<CountryItem> Countries = new()
    {
        new("TR", "Turkiye", "bi-flag"),
        new("DE", "Almanya", "bi-flag"),
        new("GB", "Ingiltere", "bi-flag"),
        new("FR", "Fransa", "bi-flag"),
        new("NL", "Hollanda", "bi-flag"),
        new("IT", "Italya", "bi-flag"),
        new("ES", "Ispanya", "bi-flag"),
        new("BE", "Belcika", "bi-flag"),
        new("AT", "Avusturya", "bi-flag"),
        new("PL", "Polonya", "bi-flag"),
        new("CZ", "Cek Cumhuriyeti", "bi-flag"),
        new("RO", "Romanya", "bi-flag"),
        new("BG", "Bulgaristan", "bi-flag"),
        new("GR", "Yunanistan", "bi-flag"),
        new("HU", "Macaristan", "bi-flag"),
        new("RU", "Rusya", "bi-flag"),
        new("UA", "Ukrayna", "bi-flag"),
        new("US", "Amerika Birlesik Devletleri", "bi-flag"),
        new("CN", "Cin", "bi-flag"),
        new("AE", "Birlesik Arap Emirlikleri", "bi-flag"),
        new("SA", "Suudi Arabistan", "bi-flag"),
        new("OTHER", "Diger", "bi-globe")
    };
}

public record CountryItem(string Code, string Name, string Icon);

/// <summary>
/// Servis saglayici alt turleri
/// </summary>
public static class ServiceProviderTypes
{
    public const string Carrier = "carrier";      // Tasimaci/Lojistik
    public const string Insurance = "insurance";  // Sigorta
    public const string Customs = "customs";      // Gumruk Musavirligi
}

/// <summary>
/// Mevcut firmaya katilma istegi ile kayit
/// </summary>
public class RegisterWithJoinDto
{
    [Required(ErrorMessage = "Ad gereklidir")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad gereklidir")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre gereklidir")]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Firma secimi gereklidir")]
    public int VendorId { get; set; }

    /// <summary>
    /// Firma yoneticisine mesaj
    /// </summary>
    [StringLength(500)]
    public string? Message { get; set; }
}
