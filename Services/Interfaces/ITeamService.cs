using Bridgo.DTOs.Team;
using Bridgo.Models.Entities;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Vendor ekip yonetimi servisi (davet + join request)
/// </summary>
public interface ITeamService
{
    // ========================================
    // DAVET ISLEMLERI
    // ========================================

    /// <summary>
    /// E-posta ile davet gonder
    /// </summary>
    Task<TeamOperationResultDto> CreateInvitationAsync(int vendorId, CreateInvitationDto dto, int invitedByUserId);

    /// <summary>
    /// Davet token'ini dogrula
    /// </summary>
    Task<ValidateInvitationResultDto> ValidateInvitationTokenAsync(string token);

    /// <summary>
    /// Davet kabul et (kullanici kayit olur)
    /// </summary>
    Task<TeamOperationResultDto> AcceptInvitationAsync(AcceptInvitationDto dto);

    /// <summary>
    /// Daveti iptal et
    /// </summary>
    Task<bool> CancelInvitationAsync(int memberId, int vendorId);

    /// <summary>
    /// Daveti yeniden gonder
    /// </summary>
    Task<TeamMemberDto?> ResendInvitationAsync(int memberId, int vendorId, int resendByUserId);

    // ========================================
    // JOIN REQUEST ISLEMLERI
    // ========================================

    /// <summary>
    /// Firmaya katilma istegi olustur
    /// </summary>
    Task<TeamOperationResultDto> CreateJoinRequestAsync(int userId, CreateJoinRequestDto dto, TeamMemberSource source);

    /// <summary>
    /// Katilma istegini onayla/reddet
    /// </summary>
    Task<TeamOperationResultDto> ProcessJoinRequestAsync(ProcessRequestDto dto, int processedByUserId);

    /// <summary>
    /// Kullanicinin bekleyen istegini iptal et
    /// </summary>
    Task<bool> CancelJoinRequestAsync(int userId, int memberId);

    // ========================================
    // SORGULAR
    // ========================================

    /// <summary>
    /// Vendor'in tum ekip uyelerini listele
    /// </summary>
    Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(int vendorId);

    /// <summary>
    /// Vendor'in aktif ekip uyelerini listele
    /// </summary>
    Task<IEnumerable<TeamMemberDto>> GetActiveTeamMembersAsync(int vendorId);

    /// <summary>
    /// Vendor'in bekleyen davetlerini listele
    /// </summary>
    Task<IEnumerable<TeamMemberDto>> GetPendingInvitationsAsync(int vendorId);

    /// <summary>
    /// Vendor'a gelen bekleyen katilma isteklerini listele
    /// </summary>
    Task<IEnumerable<TeamMemberDto>> GetPendingJoinRequestsAsync(int vendorId);

    /// <summary>
    /// Kullanicinin bekleyen istegini getir
    /// </summary>
    Task<TeamMemberDto?> GetUserPendingRequestAsync(int userId);

    /// <summary>
    /// E-posta icin bekleyen davet/istek var mi?
    /// </summary>
    Task<bool> HasPendingMemberAsync(string email, int vendorId);

    // ========================================
    // DOMAIN MATCHING
    // ========================================

    /// <summary>
    /// E-posta domain'ine gore eslesen vendor arar
    /// </summary>
    Task<TeamMemberDto?> CheckDomainMatchAsync(int userId, string email);

    /// <summary>
    /// Kullanicinin Vendor'i var mi kontrol et
    /// </summary>
    Task<bool> UserHasVendorAsync(int userId);
}
