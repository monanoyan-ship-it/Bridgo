using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

/// <summary>
/// Kullanici ayarlari ve tercihleri
/// </summary>
public class UserSettings : BaseEntity
{
    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }

    // ============================================
    // EMAIL BILDIRIMLERI - Siparisler
    // ============================================
    public bool EmailOnNewOrder { get; set; } = true;
    public bool EmailOnOrderStatusChange { get; set; } = true;

    // ============================================
    // EMAIL BILDIRIMLERI - Talepler/Teklifler
    // ============================================
    public bool EmailOnNewInquiry { get; set; } = true;
    public bool EmailOnNewDemand { get; set; } = true;
    public bool EmailOnNewOffer { get; set; } = true;

    // ============================================
    // EMAIL BILDIRIMLERI - Mesajlar
    // ============================================
    public bool EmailOnNewMessage { get; set; } = true;

    // ============================================
    // EMAIL BILDIRIMLERI - Sistem
    // ============================================
    public bool EmailMarketing { get; set; } = false;
    public bool EmailWeeklyDigest { get; set; } = true;

    // ============================================
    // IN-APP BILDIRIMLERI
    // ============================================
    public bool InAppOrders { get; set; } = true;
    public bool InAppMessages { get; set; } = true;
    public bool InAppSystem { get; set; } = true;

    // ============================================
    // DIGER TERCIHLER
    // ============================================
    public string? TimeZone { get; set; }
    public string? DateFormat { get; set; }
    public string? Currency { get; set; }
}
