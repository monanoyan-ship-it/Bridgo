namespace Bridgo.DTOs.Evrim;

/// <summary>
/// Evrim API login istegi
/// </summary>
public class EvrimLoginRequest
{
    public string KullaniciAdi { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
    public string FirmaKodu { get; set; } = string.Empty;
}

/// <summary>
/// Evrim API login yaniti
/// </summary>
public class EvrimLoginResponse
{
    public bool Basarili { get; set; }
    public string? Token { get; set; }
    public string? HataMesaji { get; set; }
    public DateTime? TokenGecerlilikTarihi { get; set; }
}

/// <summary>
/// Evrim API genel yanit wrapper
/// </summary>
public class EvrimApiResponse<T>
{
    public bool Basarili { get; set; }
    public string? HataMesaji { get; set; }
    public string? HataKodu { get; set; }
    public T? Data { get; set; }
}
