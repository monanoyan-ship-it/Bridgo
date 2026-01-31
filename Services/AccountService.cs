using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Account;
using Bridgo.Models.Entities;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class AccountService : IAccountService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ============================================
    // PERSONAL INFO
    // ============================================

    public async Task<PersonalInfoDto?> GetPersonalInfoAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Language)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        return new PersonalInfoDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            LanguageId = user.LanguageId,
            LanguageName = user.Language?.Name,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public async Task<(bool Success, string? Error)> UpdatePersonalInfoAsync(int userId, PersonalInfoUpdateDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "Kullanici bulunamadi");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.LanguageId = dto.LanguageId;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateProfileImageAsync(int userId, string imageUrl)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "Kullanici bulunamadi");

        user.ProfileImageUrl = imageUrl;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteProfileImageAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "Kullanici bulunamadi");

        user.ProfileImageUrl = null;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    // ============================================
    // PASSWORD & SECURITY
    // ============================================

    public async Task<SecurityInfoDto> GetSecurityInfoAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new SecurityInfoDto();

        return new SecurityInfoDto
        {
            HasPassword = await _userManager.HasPasswordAsync(user),
            TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
            PasswordChangedAt = null, // Tracked separately if needed
            ActiveSessionsCount = 1 // Placeholder - implement session tracking if needed
        };
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return (false, "Kullanici bulunamadi");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? "Sifre degistirilemedi";
            return (false, error);
        }

        return (true, null);
    }

    // ============================================
    // NOTIFICATION SETTINGS
    // ============================================

    public async Task<NotificationSettingsDto> GetNotificationSettingsAsync(int userId)
    {
        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            // Return defaults
            return new NotificationSettingsDto();
        }

        return new NotificationSettingsDto
        {
            // Email - Siparisler
            EmailOnNewOrder = settings.EmailOnNewOrder,
            EmailOnOrderStatusChange = settings.EmailOnOrderStatusChange,

            // Email - Talepler/Teklifler
            EmailOnNewInquiry = settings.EmailOnNewInquiry,
            EmailOnNewDemand = settings.EmailOnNewDemand,
            EmailOnNewOffer = settings.EmailOnNewOffer,

            // Email - Mesajlar
            EmailOnNewMessage = settings.EmailOnNewMessage,

            // Email - Sistem
            EmailMarketing = settings.EmailMarketing,
            EmailWeeklyDigest = settings.EmailWeeklyDigest,

            // In-App
            InAppOrders = settings.InAppOrders,
            InAppMessages = settings.InAppMessages,
            InAppSystem = settings.InAppSystem
        };
    }

    public async Task<(bool Success, string? Error)> UpdateNotificationSettingsAsync(int userId, NotificationSettingsUpdateDto dto)
    {
        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new UserSettings { UserId = userId };
            _context.UserSettings.Add(settings);
        }

        // Email - Siparisler
        settings.EmailOnNewOrder = dto.EmailOnNewOrder;
        settings.EmailOnOrderStatusChange = dto.EmailOnOrderStatusChange;

        // Email - Talepler/Teklifler
        settings.EmailOnNewInquiry = dto.EmailOnNewInquiry;
        settings.EmailOnNewDemand = dto.EmailOnNewDemand;
        settings.EmailOnNewOffer = dto.EmailOnNewOffer;

        // Email - Mesajlar
        settings.EmailOnNewMessage = dto.EmailOnNewMessage;

        // Email - Sistem
        settings.EmailMarketing = dto.EmailMarketing;
        settings.EmailWeeklyDigest = dto.EmailWeeklyDigest;

        // In-App
        settings.InAppOrders = dto.InAppOrders;
        settings.InAppMessages = dto.InAppMessages;
        settings.InAppSystem = dto.InAppSystem;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    // ============================================
    // LANGUAGES
    // ============================================

    public async Task<List<LanguageOptionDto>> GetLanguagesAsync()
    {
        return await _context.Languages
            .Where(l => l.IsActive)
            .OrderBy(l => l.DisplayOrder)
            .Select(l => new LanguageOptionDto
            {
                Id = l.Id,
                Name = l.Name,
                LanguageCulture = l.LanguageCulture,
                FlagIcon = l.UniqueSeoCode == "tr" ? "🇹🇷" : l.UniqueSeoCode == "en" ? "🇬🇧" : "🌐"
            })
            .ToListAsync();
    }

    // ============================================
    // VERIFICATIONS
    // ============================================

    public async Task<VerificationStatusDto> GetVerificationStatusAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new VerificationStatusDto();

        // Vendor verified status
        var vendor = user.VendorId.HasValue
            ? await _context.Vendors.FindAsync(user.VendorId.Value)
            : null;

        return new VerificationStatusDto
        {
            EmailVerified = user.EmailConfirmed,
            PhoneVerified = user.PhoneNumberConfirmed,
            IdentityVerified = false, // Not implemented yet
            CompanyVerified = vendor?.IsVerified ?? false
        };
    }

    public async Task<(bool Success, string? Error)> SendEmailVerificationAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return (false, "Kullanici bulunamadi");

        if (user.EmailConfirmed)
            return (false, "E-posta zaten dogrulanmis");

        // In a real implementation, you would:
        // 1. Generate a verification token
        // 2. Send an email with the verification link
        // For now, we just return success as a placeholder

        // var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        // await _emailService.SendVerificationEmailAsync(user.Email, token);

        return (true, null);
    }
}
