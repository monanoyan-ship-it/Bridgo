namespace Bridgo.DTOs.Evrim;

/// <summary>
/// Evrim beyannamesi firma bilgileri (Ihracatci/Ithalatci)
/// </summary>
public class EvrimDeclarationCompanyDto
{
    /// <summary>
    /// Vergi numarasi
    /// </summary>
    public string VergiNo { get; set; } = string.Empty;

    /// <summary>
    /// Firma unvani
    /// </summary>
    public string Unvan { get; set; } = string.Empty;

    /// <summary>
    /// Adres
    /// </summary>
    public string? Adres { get; set; }

    /// <summary>
    /// Posta kodu
    /// </summary>
    public string? PostaKodu { get; set; }

    /// <summary>
    /// Sehir
    /// </summary>
    public string? Sehir { get; set; }

    /// <summary>
    /// Ulke kodu (2 haneli ISO)
    /// </summary>
    public string UlkeKodu { get; set; } = "TR";

    /// <summary>
    /// Telefon
    /// </summary>
    public string? Telefon { get; set; }

    /// <summary>
    /// E-posta
    /// </summary>
    public string? Eposta { get; set; }
}

/// <summary>
/// Gumruk musaviri bilgileri
/// </summary>
public class EvrimDeclarationBrokerDto
{
    /// <summary>
    /// Musavir vergi numarasi
    /// </summary>
    public string VergiNo { get; set; } = string.Empty;

    /// <summary>
    /// Musavir unvani
    /// </summary>
    public string Unvan { get; set; } = string.Empty;

    /// <summary>
    /// Musavirlik belge numarasi
    /// </summary>
    public string? BelgeNo { get; set; }
}
