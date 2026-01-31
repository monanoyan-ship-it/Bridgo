namespace Bridgo.Services.Interfaces;

/// <summary>
/// Risk skoru hesaplama sonucu
/// </summary>
public class RiskScoreResult
{
    /// <summary>
    /// Toplam risk skoru (0-100, dusuk = dusuk risk)
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Risk seviyesi (Low, Medium, High, VeryHigh)
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Risk seviyesi CSS class'i
    /// </summary>
    public string LevelCssClass { get; set; } = string.Empty;

    /// <summary>
    /// Risk notlari (faktörlerin aciklamalari)
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Risk faktorleri detayi
    /// </summary>
    public List<RiskFactorDto> Factors { get; set; } = new();
}

/// <summary>
/// Risk faktoru detayi
/// </summary>
public class RiskFactorDto
{
    /// <summary>
    /// Faktor adi
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Faktor kategorisi
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Faktor skoru (0-100)
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Faktor agirligi (toplam skordaki yuzde)
    /// </summary>
    public int Weight { get; set; }

    /// <summary>
    /// Agirlikli skor (Score * Weight / 100)
    /// </summary>
    public decimal WeightedScore { get; set; }

    /// <summary>
    /// Faktor aciklamasi
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Pozitif mi negatif mi
    /// </summary>
    public bool IsPositive { get; set; }
}

/// <summary>
/// Risk skorlama servisi
/// Finansman talepleri icin risk hesaplar
/// </summary>
public interface IRiskScoringService
{
    /// <summary>
    /// Finansman talebi icin risk skoru hesaplar
    /// </summary>
    /// <param name="financingRequestId">Finansman talebi ID</param>
    /// <returns>Risk skoru sonucu</returns>
    Task<RiskScoreResult> CalculateRiskScoreAsync(int financingRequestId);

    /// <summary>
    /// Siparis icin risk skoru hesaplar (finansman talebi olmadan)
    /// </summary>
    /// <param name="orderId">Siparis ID</param>
    /// <param name="requestedAmount">Talep edilen tutar</param>
    /// <param name="durationDays">Vade (gun)</param>
    /// <returns>Risk skoru sonucu</returns>
    Task<RiskScoreResult> CalculateOrderRiskScoreAsync(int orderId, decimal requestedAmount, int durationDays);

    /// <summary>
    /// Firma icin temel risk skoru hesaplar
    /// </summary>
    /// <param name="vendorId">Firma ID</param>
    /// <returns>Risk skoru sonucu</returns>
    Task<RiskScoreResult> CalculateVendorRiskScoreAsync(int vendorId);

    /// <summary>
    /// Risk skorunu gunceller ve kaydeder
    /// </summary>
    /// <param name="financingRequestId">Finansman talebi ID</param>
    /// <returns>Guncellenen risk skoru</returns>
    Task<int> UpdateRiskScoreAsync(int financingRequestId);
}
