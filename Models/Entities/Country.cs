using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Ülke bilgileri
/// </summary>
public class Country : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? OfficialName { get; set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 (TR, US, DE)
    /// </summary>
    [Required]
    [MaxLength(2)]
    public string Iso2Code { get; set; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 alpha-3 (TUR, USA, DEU)
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Iso3Code { get; set; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 numeric (792, 840, 276)
    /// </summary>
    public int? NumericCode { get; set; }

    /// <summary>
    /// Telefon kodu (+90, +1, +49)
    /// </summary>
    [MaxLength(10)]
    public string? PhoneCode { get; set; }

    /// <summary>
    /// Para birimi kodu (TRY, USD, EUR)
    /// </summary>
    [MaxLength(3)]
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Para birimi adı (Turkish Lira, US Dollar)
    /// </summary>
    [MaxLength(50)]
    public string? CurrencyName { get; set; }

    /// <summary>
    /// Para birimi sembolü (₺, $, €, Sh.so.)
    /// </summary>
    [MaxLength(20)]
    public string? CurrencySymbol { get; set; }

    /// <summary>
    /// Bayrak emoji (🇹🇷)
    /// </summary>
    [MaxLength(10)]
    public string? FlagEmoji { get; set; }

    /// <summary>
    /// Bölge (Europe, Asia, Americas)
    /// </summary>
    [MaxLength(50)]
    public string? Region { get; set; }

    /// <summary>
    /// Alt bölge (Western Europe, Southern Asia)
    /// </summary>
    [MaxLength(50)]
    public string? SubRegion { get; set; }

    /// <summary>
    /// Başkent
    /// </summary>
    [MaxLength(100)]
    public string? Capital { get; set; }

    /// <summary>
    /// Enlem
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Boylam
    /// </summary>
    public double? Longitude { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<CountryTranslation> Translations { get; set; } = new List<CountryTranslation>();
    public virtual ICollection<State> States { get; set; } = new List<State>();
}

/// <summary>
/// Ülke adı çevirileri
/// </summary>
public class CountryTranslation : BaseEntity
{
    public int CountryId { get; set; }

    /// <summary>
    /// Dil kodu (tr, en, de, fr, ar, zh)
    /// </summary>
    [Required]
    [MaxLength(5)]
    public string LanguageCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? OfficialName { get; set; }

    // Navigation
    public virtual Country Country { get; set; } = null!;
}
