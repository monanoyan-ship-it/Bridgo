using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

/// <summary>
/// Kullanici bildirimi
/// </summary>
public class Notification
{
    public int Id { get; set; }

    /// <summary>
    /// Bildirimin gonderildigi vendor
    /// </summary>
    public int VendorId { get; set; }
    public virtual Vendor? Vendor { get; set; }

    /// <summary>
    /// Bildirimin gonderildigi kullanici (opsiyonel - null ise tum vendor kullanicilarina)
    /// </summary>
    public int? UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }

    /// <summary>
    /// Bildirim tipi
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Bildirim basligi
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Bildirim mesaji
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Ilgili entity tipi (Demand, Response, Order vb.)
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Ilgili entity ID
    /// </summary>
    public int? EntityId { get; set; }

    /// <summary>
    /// Yonlendirme URL'i
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Bildirim ikonu (Bootstrap Icons)
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Okundu mu?
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Okundugu tarih
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// E-posta gonderildi mi?
    /// </summary>
    public bool EmailSent { get; set; }

    /// <summary>
    /// E-posta gonderim tarihi
    /// </summary>
    public DateTime? EmailSentAt { get; set; }

    /// <summary>
    /// Olusturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Silinmis mi?
    /// </summary>
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Bildirim tipleri
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Genel bilgi
    /// </summary>
    Info = 1,

    /// <summary>
    /// Yeni talep (kategori aboneliginden)
    /// </summary>
    NewDemand = 2,

    /// <summary>
    /// Talebinize teklif geldi
    /// </summary>
    NewResponse = 3,

    /// <summary>
    /// Teklifiniz kabul edildi
    /// </summary>
    ResponseAccepted = 4,

    /// <summary>
    /// Teklifiniz reddedildi
    /// </summary>
    ResponseRejected = 5,

    /// <summary>
    /// Talep durumu degisti
    /// </summary>
    DemandStatusChanged = 6,

    /// <summary>
    /// Profil mesaji geldi
    /// </summary>
    ContactMessage = 7,

    /// <summary>
    /// Yeni urun istegi (saticiya)
    /// </summary>
    NewProductInquiry = 8,

    /// <summary>
    /// Urun istegine yanit (aliciya)
    /// </summary>
    ProductInquiryResponse = 9,

    /// <summary>
    /// Sistem bildirimi
    /// </summary>
    System = 10,

    // ============================================
    // ORDER NOTIFICATIONS
    // ============================================

    /// <summary>
    /// Yeni siparis (saticiya)
    /// </summary>
    NewOrder = 11,

    /// <summary>
    /// Siparis onaylandi (aliciya)
    /// </summary>
    OrderConfirmed = 12,

    /// <summary>
    /// Siparis reddedildi (aliciya)
    /// </summary>
    OrderRejected = 13,

    /// <summary>
    /// Siparis durumu degisti
    /// </summary>
    OrderStatusChanged = 14,

    /// <summary>
    /// Kargo bilgisi eklendi
    /// </summary>
    ShipmentCreated = 15,

    /// <summary>
    /// Kargo durumu guncellendi
    /// </summary>
    ShipmentUpdated = 16,

    /// <summary>
    /// Siparis teslim edildi
    /// </summary>
    OrderDelivered = 17,

    /// <summary>
    /// Siparis iptal edildi
    /// </summary>
    OrderCancelled = 18,

    // ============================================
    // SERVICE REQUEST NOTIFICATIONS
    // ============================================

    /// <summary>
    /// Yeni servis talebi (lojistik/gumruk/sigorta/gozetim firmalarina)
    /// </summary>
    NewServiceRequest = 20,

    /// <summary>
    /// Servis teklifi geldi (aliciya)
    /// </summary>
    NewServiceQuote = 21,

    /// <summary>
    /// Servis teklifi kabul edildi (servis saglayiciya)
    /// </summary>
    ServiceQuoteAccepted = 22,

    /// <summary>
    /// Servis teklifi reddedildi (servis saglayiciya)
    /// </summary>
    ServiceQuoteRejected = 23,

    // ============================================
    // TASK NOTIFICATIONS
    // ============================================

    /// <summary>
    /// Yeni gorev atandi
    /// </summary>
    TaskAssigned = 30,

    /// <summary>
    /// Gorev tamamlandi
    /// </summary>
    TaskCompleted = 31,

    // ============================================
    // INVESTMENT NOTIFICATIONS
    // ============================================

    /// <summary>
    /// Yeni finansman talebi (yatirimcilara)
    /// </summary>
    NewFinancingRequest = 40,

    /// <summary>
    /// Yeni yatirim teklifi (talep sahibine)
    /// </summary>
    NewInvestmentOffer = 41,

    /// <summary>
    /// Yatirim teklifi kabul edildi (yatirimciya)
    /// </summary>
    InvestmentAccepted = 42,

    /// <summary>
    /// Yatirim teklifi reddedildi (yatirimciya)
    /// </summary>
    InvestmentRejected = 43,

    /// <summary>
    /// Finansman transferi tamamlandi (talep sahibine)
    /// </summary>
    InvestmentFunded = 44,

    /// <summary>
    /// Geri odeme alindi (yatirimciya)
    /// </summary>
    InvestmentRepaid = 45,

    // ============================================
    // NEGOTIATION NOTIFICATIONS
    // ============================================

    /// <summary>
    /// Pazarlik basladi (satici/aliciya)
    /// </summary>
    NegotiationStarted = 50,

    /// <summary>
    /// Yeni karsi teklif geldi
    /// </summary>
    NegotiationCounterOffer = 51,

    /// <summary>
    /// Pazarlik kabul edildi
    /// </summary>
    NegotiationAccepted = 52,

    /// <summary>
    /// Pazarlik reddedildi
    /// </summary>
    NegotiationRejected = 53,

    /// <summary>
    /// Pazarlik suresi doldu
    /// </summary>
    NegotiationExpired = 54,

    // ============================================
    // SOCIAL FEED NOTIFICATIONS
    // ============================================

    /// <summary>
    /// Yeni sosyal paylasim (takip edilen firmadan)
    /// </summary>
    NewSocialPost = 60,

    /// <summary>
    /// Paylasim begeni aldı
    /// </summary>
    SocialPostLiked = 61,

    /// <summary>
    /// Paylasima yorum yapildi
    /// </summary>
    SocialPostCommented = 62,

    /// <summary>
    /// Yeni takipci
    /// </summary>
    NewFollower = 63
}
