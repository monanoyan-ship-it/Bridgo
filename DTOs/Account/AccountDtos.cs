using System.ComponentModel.DataAnnotations;

namespace Bridgo.DTOs.Account;

// ============================================
// PERSONAL INFO
// ============================================

/// <summary>
/// Kisisel bilgiler gosterimi
/// </summary>
public class PersonalInfoDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int? LanguageId { get; set; }
    public string? LanguageName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Kisisel bilgiler guncelleme
/// </summary>
public class PersonalInfoUpdateDto
{
    [Required(ErrorMessage = "Ad gereklidir")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad 2-100 karakter olmalidir")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad gereklidir")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Soyad 2-100 karakter olmalidir")]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Gecerli bir telefon numarasi giriniz")]
    public string? PhoneNumber { get; set; }

    public int? LanguageId { get; set; }
}

/// <summary>
/// Profil resmi guncelleme
/// </summary>
public class ProfileImageUpdateDto
{
    public string? ProfileImageUrl { get; set; }
}

// ============================================
// PASSWORD & SECURITY
// ============================================

/// <summary>
/// Sifre degistirme
/// </summary>
public class ChangePasswordDto
{
    [Required(ErrorMessage = "Mevcut sifre gereklidir")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni sifre gereklidir")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Sifre en az 6 karakter olmalidir")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre tekrari gereklidir")]
    [Compare("NewPassword", ErrorMessage = "Sifreler eslesmedi")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Guvenlik bilgileri
/// </summary>
public class SecurityInfoDto
{
    public bool HasPassword { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public int ActiveSessionsCount { get; set; }
}

// ============================================
// NOTIFICATION SETTINGS
// ============================================

/// <summary>
/// Bildirim tercihleri
/// </summary>
public class NotificationSettingsDto
{
    // Email bildirimleri - Siparisler
    public bool EmailOnNewOrder { get; set; } = true;
    public bool EmailOnOrderStatusChange { get; set; } = true;

    // Email bildirimleri - Talepler/Teklifler
    public bool EmailOnNewInquiry { get; set; } = true;
    public bool EmailOnNewDemand { get; set; } = true;
    public bool EmailOnNewOffer { get; set; } = true;

    // Email bildirimleri - Mesajlar
    public bool EmailOnNewMessage { get; set; } = true;

    // Email bildirimleri - Sistem
    public bool EmailMarketing { get; set; } = false;
    public bool EmailWeeklyDigest { get; set; } = true;

    // In-App bildirimleri
    public bool InAppOrders { get; set; } = true;
    public bool InAppMessages { get; set; } = true;
    public bool InAppSystem { get; set; } = true;
}

/// <summary>
/// Bildirim tercihleri guncelleme
/// </summary>
public class NotificationSettingsUpdateDto
{
    // Email bildirimleri - Siparisler
    public bool EmailOnNewOrder { get; set; }
    public bool EmailOnOrderStatusChange { get; set; }

    // Email bildirimleri - Talepler/Teklifler
    public bool EmailOnNewInquiry { get; set; }
    public bool EmailOnNewDemand { get; set; }
    public bool EmailOnNewOffer { get; set; }

    // Email bildirimleri - Mesajlar
    public bool EmailOnNewMessage { get; set; }

    // Email bildirimleri - Sistem
    public bool EmailMarketing { get; set; }
    public bool EmailWeeklyDigest { get; set; }

    // In-App bildirimleri
    public bool InAppOrders { get; set; }
    public bool InAppMessages { get; set; }
    public bool InAppSystem { get; set; }
}

// ============================================
// VERIFICATIONS
// ============================================

/// <summary>
/// Dogrulama durumlari
/// </summary>
public class VerificationStatusDto
{
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public bool IdentityVerified { get; set; }
    public bool CompanyVerified { get; set; }
}

// ============================================
// LANGUAGE
// ============================================

/// <summary>
/// Dil secenegi
/// </summary>
public class LanguageOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LanguageCulture { get; set; } = string.Empty;
    public string FlagIcon { get; set; } = string.Empty;
}

public class UpdateLanguagePreferenceDto
{
    public int LanguageId { get; set; }
}
