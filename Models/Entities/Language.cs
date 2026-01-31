using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

public class Language : BaseEntity
{
    /// <summary>
    /// Dil adı (English, Türkçe, Deutsch)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Native dil adı (English, Türkçe, Deutsch)
    /// </summary>
    [MaxLength(100)]
    public string? NativeName { get; set; }

    /// <summary>
    /// Kültür kodu (tr-TR, en-US, de-DE)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string LanguageCulture { get; set; } = string.Empty;

    /// <summary>
    /// SEO code (tr, en, de, zh-Hans, zh-Hant)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string UniqueSeoCode { get; set; } = string.Empty;

    /// <summary>
    /// ISO 639-2 3-letter code (tur, eng, deu)
    /// </summary>
    [MaxLength(5)]
    public string? Iso3Code { get; set; }

    /// <summary>
    /// Bayrak emoji (🇹🇷, 🇺🇸, 🇩🇪)
    /// </summary>
    [MaxLength(10)]
    public string? FlagEmoji { get; set; }

    /// <summary>
    /// Bayrak görsel dosyası (tr.png, en.png)
    /// </summary>
    [MaxLength(100)]
    public string? FlagImageFileName { get; set; }

    /// <summary>
    /// Sağdan sola yazım (Arapça, İbranice vb.)
    /// </summary>
    public bool Rtl { get; set; } = false;

    /// <summary>
    /// Varsayılan dil mi?
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Coğrafi çeviriler için aktif mi? (Country, State, City çevirileri)
    /// </summary>
    public bool IsGeoTranslationEnabled { get; set; } = false;

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public ICollection<LocaleStringResource> LocaleStringResources { get; set; } = new List<LocaleStringResource>();
}
