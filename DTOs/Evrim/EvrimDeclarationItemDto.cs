namespace Bridgo.DTOs.Evrim;

/// <summary>
/// Evrim beyanname kalemi (GTIP satiri)
/// </summary>
public class EvrimDeclarationItemDto
{
    /// <summary>
    /// Kalem sira numarasi
    /// </summary>
    public int SiraNo { get; set; }

    /// <summary>
    /// GTIP kodu (12 haneli)
    /// </summary>
    public string GtipKodu { get; set; } = string.Empty;

    /// <summary>
    /// Ticari tanim
    /// </summary>
    public string TicariTanim { get; set; } = string.Empty;

    /// <summary>
    /// Mense ulke kodu (2 haneli ISO)
    /// </summary>
    public string MenseUlkeKodu { get; set; } = string.Empty;

    /// <summary>
    /// Sevk ulkesi kodu
    /// </summary>
    public string? SevkUlkeKodu { get; set; }

    /// <summary>
    /// Miktar
    /// </summary>
    public decimal Miktar { get; set; }

    /// <summary>
    /// Olcu birimi (KG, AD, M, vb.)
    /// </summary>
    public string OlcuBirimi { get; set; } = "KG";

    /// <summary>
    /// Istatistiki miktar
    /// </summary>
    public decimal? IstatistikiMiktar { get; set; }

    /// <summary>
    /// Istatistiki birim
    /// </summary>
    public string? IstatistikiBirim { get; set; }

    /// <summary>
    /// Brut agirlik (kg)
    /// </summary>
    public decimal BrutAgirlik { get; set; }

    /// <summary>
    /// Net agirlik (kg)
    /// </summary>
    public decimal NetAgirlik { get; set; }

    /// <summary>
    /// Fatura tutari
    /// </summary>
    public decimal FaturaTutari { get; set; }

    /// <summary>
    /// Fatura dovizi (USD, EUR, TRY, vb.)
    /// </summary>
    public string FaturaDovizi { get; set; } = "USD";

    /// <summary>
    /// Istatistiki kiymeti (USD)
    /// </summary>
    public decimal? IstatistikiKiymet { get; set; }

    /// <summary>
    /// FOB degeri (USD)
    /// </summary>
    public decimal? FobDeger { get; set; }

    /// <summary>
    /// CIF degeri (USD)
    /// </summary>
    public decimal? CifDeger { get; set; }

    /// <summary>
    /// Navlun tutari
    /// </summary>
    public decimal? NavlunTutar { get; set; }

    /// <summary>
    /// Sigorta tutari
    /// </summary>
    public decimal? SigortaTutar { get; set; }

    /// <summary>
    /// Kap adedi
    /// </summary>
    public int? KapAdedi { get; set; }

    /// <summary>
    /// Kap cinsi kodu
    /// </summary>
    public string? KapCinsiKodu { get; set; }

    /// <summary>
    /// Marka
    /// </summary>
    public string? Marka { get; set; }

    /// <summary>
    /// Model
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Muafiyet kodu
    /// </summary>
    public string? MuafiyetKodu { get; set; }

    /// <summary>
    /// Tercihli mense kodu
    /// </summary>
    public string? TercihliMenseKodu { get; set; }

    /// <summary>
    /// Belge bilgileri (liste)
    /// </summary>
    public List<EvrimItemDocumentDto>? Belgeler { get; set; }
}

/// <summary>
/// Kalem bazli belge bilgisi
/// </summary>
public class EvrimItemDocumentDto
{
    /// <summary>
    /// Belge tipi kodu
    /// </summary>
    public string BelgeTipiKodu { get; set; } = string.Empty;

    /// <summary>
    /// Belge numarasi
    /// </summary>
    public string BelgeNo { get; set; } = string.Empty;

    /// <summary>
    /// Belge tarihi
    /// </summary>
    public DateTime? BelgeTarihi { get; set; }
}
