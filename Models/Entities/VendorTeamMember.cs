using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

/// <summary>
/// Vendor ekip uyesi - hem davetler hem de katilim istekleri icin tek model
/// UserId null ise: Bekleyen davet (henuz kayit olmamis) veya kullanici olmayan uye (temsilci, ortak vb.)
/// UserId dolu ise: Ekibe katilmis kullanici
/// </summary>
public class VendorTeamMember : BaseEntity
{
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;

    /// <summary>
    /// Kullanici ID - null ise henuz kayit olmamis veya kullanici olmayan uye
    /// </summary>
    public int? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Uye tipi (TeamMemberTypes static class'tan ID)
    /// </summary>
    public int MemberTypeId { get; set; } = 0; // Default: Employee

    /// <summary>
    /// E-posta adresi (davet veya istek icin)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Kisinin adi (davet edilirken veya istekte)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Telefon numarasi
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Unvan/Pozisyon (Genel Mudur, Mali Isler Direktoru, vb.)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// TC Kimlik No veya Pasaport No
    /// </summary>
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Hisse orani (%) - Hissedar ve UBO icin
    /// </summary>
    public decimal? SharePercentage { get; set; }

    /// <summary>
    /// Imza yetkisi var mi?
    /// </summary>
    public bool IsAuthorizedSignatory { get; set; }

    /// <summary>
    /// Yasal temsilci mi?
    /// </summary>
    public bool IsLegalRepresentative { get; set; }

    /// <summary>
    /// Dogrulama durumu (MemberVerificationStatuses)
    /// </summary>
    public int VerificationStatusId { get; set; } = 1; // NotVerified

    /// <summary>
    /// Dogrulama tarihi
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// Dogrulayan kullanici
    /// </summary>
    public string? VerifiedBy { get; set; }

    /// <summary>
    /// Dogrulama notu/red nedeni
    /// </summary>
    public string? VerificationNote { get; set; }

    /// <summary>
    /// Kaynak: Davet mi, Katilma istegi mi?
    /// </summary>
    public TeamMemberSource Source { get; set; }

    /// <summary>
    /// Durum (TeamMemberStatuses static class'tan ID)
    /// </summary>
    public int TeamMemberStatusId { get; set; } = 1; // Default: Pending

    /// <summary>
    /// Davet token'i (sadece Invitation source icin)
    /// </summary>
    public string? InvitationToken { get; set; }

    /// <summary>
    /// Davet son kullanim tarihi
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Davet eden / Onaylayan kullanici
    /// </summary>
    public int? ProcessedByUserId { get; set; }
    public ApplicationUser? ProcessedByUser { get; set; }

    /// <summary>
    /// Islem tarihi (onay/red/kabul)
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Istek mesaji (JoinRequest icin)
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Red nedeni
    /// </summary>
    public string? RejectionReason { get; set; }

    // === HELPER PROPERTIES ===

    /// <summary>
    /// Davet suresi dolmus mu?
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    /// <summary>
    /// Davet gecerli mi? (Pending + suresi dolmamis)
    /// </summary>
    public bool IsValidInvitation =>
        Source == TeamMemberSource.Invitation &&
        TeamMemberStatusId == 1 && // Pending
        !IsExpired;

    /// <summary>
    /// Kullanici aktif ekip uyesi mi?
    /// </summary>
    public bool IsActiveMember =>
        UserId.HasValue && TeamMemberStatusId == 2; // Active
}

/// <summary>
/// Ekip uyesi kaynak turu (sistem icin - localization gerekmez)
/// </summary>
public enum TeamMemberSource
{
    Invitation = 0,         // E-posta ile davet edildi
    JoinRequest = 1,        // Kendisi istekte bulundu
    DomainMatch = 2,        // E-posta domain'i eslesti
    OwnerCreated = 3,       // Vendor olusturulurken otomatik eklendi
    ManualEntry = 4         // Manuel olarak eklendi (kullanici olmayan)
}

