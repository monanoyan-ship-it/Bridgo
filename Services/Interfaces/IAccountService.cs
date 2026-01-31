using Bridgo.DTOs.Account;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Kullanici hesap yonetimi servisi
/// </summary>
public interface IAccountService
{
    // ============================================
    // PERSONAL INFO
    // ============================================

    /// <summary>
    /// Kullanici kisisel bilgilerini getir
    /// </summary>
    Task<PersonalInfoDto?> GetPersonalInfoAsync(int userId);

    /// <summary>
    /// Kisisel bilgileri guncelle
    /// </summary>
    Task<(bool Success, string? Error)> UpdatePersonalInfoAsync(int userId, PersonalInfoUpdateDto dto);

    /// <summary>
    /// Profil resmini guncelle
    /// </summary>
    Task<(bool Success, string? Error)> UpdateProfileImageAsync(int userId, string imageUrl);

    /// <summary>
    /// Profil resmini sil
    /// </summary>
    Task<(bool Success, string? Error)> DeleteProfileImageAsync(int userId);

    // ============================================
    // PASSWORD & SECURITY
    // ============================================

    /// <summary>
    /// Guvenlik bilgilerini getir
    /// </summary>
    Task<SecurityInfoDto> GetSecurityInfoAsync(int userId);

    /// <summary>
    /// Sifre degistir
    /// </summary>
    Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, ChangePasswordDto dto);

    // ============================================
    // NOTIFICATION SETTINGS
    // ============================================

    /// <summary>
    /// Bildirim tercihlerini getir
    /// </summary>
    Task<NotificationSettingsDto> GetNotificationSettingsAsync(int userId);

    /// <summary>
    /// Bildirim tercihlerini guncelle
    /// </summary>
    Task<(bool Success, string? Error)> UpdateNotificationSettingsAsync(int userId, NotificationSettingsUpdateDto dto);

    // ============================================
    // LANGUAGES
    // ============================================

    /// <summary>
    /// Aktif dilleri getir
    /// </summary>
    Task<List<LanguageOptionDto>> GetLanguagesAsync();

    // ============================================
    // VERIFICATIONS
    // ============================================

    /// <summary>
    /// Dogrulama durumlarini getir
    /// </summary>
    Task<VerificationStatusDto> GetVerificationStatusAsync(int userId);

    /// <summary>
    /// E-posta dogrulama e-postasi gonder
    /// </summary>
    Task<(bool Success, string? Error)> SendEmailVerificationAsync(int userId);
}
