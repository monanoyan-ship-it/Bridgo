using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Eyalet/İl/Bölge bilgileri
/// </summary>
public class State : BaseEntity
{
    public int CountryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Eyalet/İl kodu (CA, NY, 34, IST)
    /// </summary>
    [MaxLength(10)]
    public string? Code { get; set; }

    /// <summary>
    /// Eyalet tipi (State, Province, Region, City vb.)
    /// </summary>
    [MaxLength(50)]
    public string? Type { get; set; }

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
    public virtual Country Country { get; set; } = null!;
    public virtual ICollection<StateTranslation> Translations { get; set; } = new List<StateTranslation>();
}

/// <summary>
/// Eyalet/İl adı çevirileri
/// </summary>
public class StateTranslation : BaseEntity
{
    public int StateId { get; set; }

    /// <summary>
    /// Dil kodu (tr, en, de, fr, ar, zh)
    /// </summary>
    [Required]
    [MaxLength(5)]
    public string LanguageCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Navigation
    public virtual State State { get; set; } = null!;
}
