namespace Bridgo.Configuration;

/// <summary>
/// Evrim Gumruk API yapilandirma ayarlari
/// </summary>
public class EvrimApiSettings
{
    /// <summary>
    /// API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.evrim.com/EvrimEntRestWS.dll";

    /// <summary>
    /// API kullanici adi
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// API sifresi
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Evrim firma kodu
    /// </summary>
    public string FirmaKodu { get; set; } = string.Empty;

    /// <summary>
    /// API istekleri arasi minimum bekleme suresi (ms)
    /// Evrim API rate limit: 1 istek/saniye
    /// </summary>
    public int RateLimitMs { get; set; } = 1000;

    /// <summary>
    /// API istek timeout suresi (saniye)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Otomatik durum senkronizasyonu aktif mi
    /// </summary>
    public bool EnableStatusPolling { get; set; } = true;

    /// <summary>
    /// Durum senkronizasyon araligi (dakika)
    /// </summary>
    public int StatusPollingIntervalMinutes { get; set; } = 15;

    /// <summary>
    /// JWT token gecerlilik suresi (dakika) - cache icin
    /// Evrim token'lari genelde 60 dakika gecerli
    /// </summary>
    public int TokenExpirationMinutes { get; set; } = 55;
}
