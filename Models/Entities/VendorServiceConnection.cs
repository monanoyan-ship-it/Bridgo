namespace Bridgo.Models.Entities;

/// <summary>
/// Vendor harici servis baglantilari
/// Stripe, Lojistik, Muhasebe, E-Fatura vb.
/// </summary>
public class VendorServiceConnection : BaseEntity
{
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    /// <summary>
    /// Servis kodu: stripe, iyzico, parasut, logo, aras, yurtici, ups, dhl, efatura, vb.
    /// </summary>
    public string ServiceCode { get; set; } = string.Empty;

    /// <summary>
    /// Servis kategorisi: payment, logistics, accounting, invoice, erp, marketplace
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Baglanti durumu: pending, connected, disconnected, error, expired
    /// </summary>
    public string Status { get; set; } = "disconnected";

    // Kimlik bilgileri (sifrelenmis olarak tutulmali)
    public string? ExternalAccountId { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }

    // Stripe-specific (diger servisler icin de genisletilebilir)
    public bool ChargesEnabled { get; set; }
    public bool PayoutsEnabled { get; set; }
    public bool DetailsSubmitted { get; set; }

    // Genel durum bilgileri
    public string? LastError { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }

    // Ek yapilandirma (JSON)
    public string? ConfigJson { get; set; }
    public string? MetadataJson { get; set; }
}
