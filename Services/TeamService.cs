using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bridgo.DTOs.Team;
using Bridgo.Models.Entities;
using Bridgo.Models.Identity;
using Bridgo.Repositories;
using Bridgo.Services.Interfaces;
using Bridgo.Models.Enums;

namespace Bridgo.Services;

/// <summary>
/// Vendor ekip yonetimi servisi implementation
/// </summary>
public class TeamService : ITeamService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private const int InvitationExpirationDays = 7;

    // Kisisel e-posta domain'leri
    private static readonly HashSet<string> PersonalEmailDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "gmail.com", "googlemail.com",
        "hotmail.com", "outlook.com", "live.com", "msn.com",
        "yahoo.com", "yahoo.com.tr", "ymail.com",
        "icloud.com", "me.com", "mac.com",
        "mail.com", "email.com", "aol.com",
        "protonmail.com", "proton.me",
        "yandex.com", "yandex.ru",
        "gmx.com", "gmx.de", "zoho.com", "mail.ru"
    };

    public TeamService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    // ========================================
    // DAVET ISLEMLERI
    // ========================================

    public async Task<TeamOperationResultDto> CreateInvitationAsync(int vendorId, CreateInvitationDto dto, int invitedByUserId)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(vendorId);
        if (vendor == null)
            return new TeamOperationResultDto { Success = false, Message = "Firma bulunamadi" };

        // Ayni e-posta icin bekleyen kayit var mi?
        if (await HasPendingMemberAsync(dto.Email, vendorId))
            return new TeamOperationResultDto { Success = false, Message = "Bu e-posta icin zaten bekleyen bir davet/istek var" };

        // E-posta zaten kayitli mi?
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return new TeamOperationResultDto { Success = false, Message = "Bu e-posta adresi zaten kayitli" };

        var member = new VendorTeamMember
        {
            VendorId = vendorId,
            Email = dto.Email.ToLower(),
            Name = dto.Name,
            Source = TeamMemberSource.Invitation,
            TeamMemberStatusId = TeamMemberStatuses.Pending.Id,
            InvitationToken = GenerateToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(InvitationExpirationDays),
            ProcessedByUserId = invitedByUserId
        };

        await _unitOfWork.VendorTeamMembers.AddAsync(member);
        await _unitOfWork.SaveChangesAsync();

        return new TeamOperationResultDto
        {
            Success = true,
            Message = "Davet gonderildi",
            MemberId = member.Id,
            VendorId = vendorId,
            VendorName = vendor.CompanyName
        };
    }

    public async Task<ValidateInvitationResultDto> ValidateInvitationTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new ValidateInvitationResultDto { IsValid = false, Message = "Davet kodu eksik" };

        var member = await _unitOfWork.VendorTeamMembers
            .QueryNoTracking()
            .Include(m => m.Vendor)
            .FirstOrDefaultAsync(m => m.InvitationToken == token);

        if (member == null)
            return new ValidateInvitationResultDto { IsValid = false, Message = "Gecersiz davet kodu" };

        if (member.TeamMemberStatusId != TeamMemberStatuses.Pending.Id)
        {
            string msg;
            if (member.TeamMemberStatusId == TeamMemberStatuses.Active.Id)
                msg = "Bu davet zaten kullanilmis";
            else if (member.TeamMemberStatusId == TeamMemberStatuses.Expired.Id)
                msg = "Bu davetin suresi dolmus";
            else if (member.TeamMemberStatusId == TeamMemberStatuses.Cancelled.Id)
                msg = "Bu davet iptal edilmis";
            else
                msg = "Bu davet gecersiz";

            return new ValidateInvitationResultDto { IsValid = false, Message = msg };
        }

        if (member.IsExpired)
        {
            var m = await _unitOfWork.VendorTeamMembers.GetByIdAsync(member.Id);
            if (m != null)
            {
                m.TeamMemberStatusId = TeamMemberStatuses.Expired.Id;
                _unitOfWork.VendorTeamMembers.Update(m);
                await _unitOfWork.SaveChangesAsync();
            }
            return new ValidateInvitationResultDto { IsValid = false, Message = "Bu davetin suresi dolmus" };
        }

        return new ValidateInvitationResultDto
        {
            IsValid = true,
            Member = await MapToDtoAsync(member)
        };
    }

    public async Task<TeamOperationResultDto> AcceptInvitationAsync(AcceptInvitationDto dto)
    {
        var validation = await ValidateInvitationTokenAsync(dto.Token);
        if (!validation.IsValid || validation.Member == null)
            return new TeamOperationResultDto { Success = false, Message = validation.Message };

        var member = await _unitOfWork.VendorTeamMembers
            .FirstOrDefaultAsync(m => m.InvitationToken == dto.Token);

        if (member == null)
            return new TeamOperationResultDto { Success = false, Message = "Davet bulunamadi" };

        var existingUser = await _userManager.FindByEmailAsync(member.Email);
        if (existingUser != null)
            return new TeamOperationResultDto { Success = false, Message = "Bu e-posta adresi zaten kayitli" };

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Kullanici olustur
            var user = new ApplicationUser
            {
                UserName = member.Email,
                Email = member.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                EmailConfirmed = true,
                IsActive = true,
                VendorId = member.VendorId
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                await _unitOfWork.RollbackAsync();
                return new TeamOperationResultDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            // Team member'i guncelle
            member.UserId = user.Id;
            member.Name = $"{dto.FirstName} {dto.LastName}";
            member.TeamMemberStatusId = 2; // Active
            member.ProcessedAt = DateTime.UtcNow;
            _unitOfWork.VendorTeamMembers.Update(member);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitAsync();

            var vendor = await _unitOfWork.Vendors.GetByIdAsync(member.VendorId);

            return new TeamOperationResultDto
            {
                Success = true,
                Message = "Kayit basarili",
                MemberId = member.Id,
                UserId = user.Id,
                VendorId = member.VendorId,
                VendorName = vendor?.CompanyName
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return new TeamOperationResultDto { Success = false, Message = ex.Message };
        }
    }

    public async Task<bool> CancelInvitationAsync(int memberId, int vendorId)
    {
        var member = await _unitOfWork.VendorTeamMembers
            .FirstOrDefaultAsync(m => m.Id == memberId &&
                                      m.VendorId == vendorId &&
                                      m.Source == TeamMemberSource.Invitation &&
                                      m.TeamMemberStatusId == 1); // Pending

        if (member == null) return false;

        member.TeamMemberStatusId = 4; // Cancelled
        _unitOfWork.VendorTeamMembers.Update(member);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<TeamMemberDto?> ResendInvitationAsync(int memberId, int vendorId, int resendByUserId)
    {
        var member = await _unitOfWork.VendorTeamMembers
            .FirstOrDefaultAsync(m => m.Id == memberId &&
                                      m.VendorId == vendorId &&
                                      m.Source == TeamMemberSource.Invitation &&
                                      (m.TeamMemberStatusId == 1 || m.TeamMemberStatusId == 5)); // Pending or Expired

        if (member == null) return null;

        member.InvitationToken = GenerateToken();
        member.TeamMemberStatusId = 1; // Pending
        member.ExpiresAt = DateTime.UtcNow.AddDays(InvitationExpirationDays);
        member.ProcessedByUserId = resendByUserId;

        _unitOfWork.VendorTeamMembers.Update(member);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(member);
    }

    // ========================================
    // JOIN REQUEST ISLEMLERI
    // ========================================

    public async Task<TeamOperationResultDto> CreateJoinRequestAsync(int userId, CreateJoinRequestDto dto, TeamMemberSource source)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new TeamOperationResultDto { Success = false, Message = "Kullanici bulunamadi" };

        if (user.VendorId.HasValue)
            return new TeamOperationResultDto { Success = false, Message = "Zaten bir firmaya bagli" };

        var vendor = await _unitOfWork.Vendors.GetByIdAsync(dto.VendorId);
        if (vendor == null)
            return new TeamOperationResultDto { Success = false, Message = "Firma bulunamadi" };

        // Bekleyen istek var mi?
        var existing = await _unitOfWork.VendorTeamMembers
            .FirstOrDefaultAsync(m => m.UserId == userId && m.TeamMemberStatusId == 1); // Pending

        if (existing != null)
            return new TeamOperationResultDto { Success = false, Message = "Zaten bekleyen bir isteginiz var" };

        var member = new VendorTeamMember
        {
            VendorId = dto.VendorId,
            UserId = userId,
            Email = user.Email ?? string.Empty,
            Name = $"{user.FirstName} {user.LastName}",
            Source = source,
            TeamMemberStatusId = 1, // Pending
            Message = dto.Message
        };

        await _unitOfWork.VendorTeamMembers.AddAsync(member);
        await _unitOfWork.SaveChangesAsync();

        return new TeamOperationResultDto
        {
            Success = true,
            Message = "Katilma isteginiz gonderildi",
            MemberId = member.Id,
            VendorId = dto.VendorId,
            VendorName = vendor.CompanyName
        };
    }

    public async Task<TeamOperationResultDto> ProcessJoinRequestAsync(ProcessRequestDto dto, int processedByUserId)
    {
        var member = await _unitOfWork.VendorTeamMembers
            .FirstOrDefaultAsync(m => m.Id == dto.MemberId &&
                                      m.Source != TeamMemberSource.Invitation &&
                                      m.TeamMemberStatusId == 1); // Pending

        if (member == null)
            return new TeamOperationResultDto { Success = false, Message = "Istek bulunamadi" };

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            member.ProcessedByUserId = processedByUserId;
            member.ProcessedAt = DateTime.UtcNow;

            if (dto.Approve)
            {
                member.TeamMemberStatusId = 2; // Active

                // Kullaniciyi vendor'a bagla
                if (member.UserId.HasValue)
                {
                    var user = await _userManager.FindByIdAsync(member.UserId.Value.ToString());
                    if (user != null)
                    {
                        user.VendorId = member.VendorId;
                        await _userManager.UpdateAsync(user);

                        // CompanyRole'leri ata (RBAC)
                        if (dto.CompanyRoleIds.Any())
                        {
                            foreach (var roleId in dto.CompanyRoleIds)
                            {
                                var roleMapping = new CompanyRoleUserMapping
                                {
                                    UserId = user.Id,
                                    VendorId = member.VendorId,
                                    CompanyRoleId = roleId,
                                    IsActive = true,
                                    AssignedByUserId = processedByUserId,
                                    AssignedAt = DateTime.UtcNow
                                };
                                await _unitOfWork.CompanyRoleUserMappings.AddAsync(roleMapping);
                            }
                        }
                    }
                }
            }
            else
            {
                member.TeamMemberStatusId = 3; // Rejected
                member.RejectionReason = dto.RejectionReason;
            }

            _unitOfWork.VendorTeamMembers.Update(member);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return new TeamOperationResultDto
            {
                Success = true,
                Message = dto.Approve ? "Istek onaylandi" : "Istek reddedildi",
                MemberId = member.Id
            };
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            return new TeamOperationResultDto { Success = false, Message = "Bir hata olustu" };
        }
    }

    public async Task<bool> CancelJoinRequestAsync(int userId, int memberId)
    {
        var member = await _unitOfWork.VendorTeamMembers
            .FirstOrDefaultAsync(m => m.Id == memberId &&
                                      m.UserId == userId &&
                                      m.TeamMemberStatusId == 1); // Pending

        if (member == null) return false;

        member.TeamMemberStatusId = 4; // Cancelled
        _unitOfWork.VendorTeamMembers.Update(member);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // ========================================
    // SORGULAR
    // ========================================

    public async Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(int vendorId)
    {
        var members = await _unitOfWork.VendorTeamMembers
            .QueryNoTracking()
            .Where(m => m.VendorId == vendorId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        var result = new List<TeamMemberDto>();
        foreach (var m in members)
            result.Add(await MapToDtoAsync(m));

        return result;
    }

    public async Task<IEnumerable<TeamMemberDto>> GetActiveTeamMembersAsync(int vendorId)
    {
        var members = await _unitOfWork.VendorTeamMembers
            .QueryNoTracking()
            .Where(m => m.VendorId == vendorId && m.TeamMemberStatusId == 2) // Active
            .OrderBy(m => m.Name)
            .ToListAsync();

        var result = new List<TeamMemberDto>();
        foreach (var m in members)
            result.Add(await MapToDtoAsync(m));

        return result;
    }

    public async Task<IEnumerable<TeamMemberDto>> GetPendingInvitationsAsync(int vendorId)
    {
        var members = await _unitOfWork.VendorTeamMembers
            .QueryNoTracking()
            .Where(m => m.VendorId == vendorId &&
                       m.Source == TeamMemberSource.Invitation &&
                       m.TeamMemberStatusId == 1) // Pending
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        var result = new List<TeamMemberDto>();
        foreach (var m in members)
            result.Add(await MapToDtoAsync(m));

        return result;
    }

    public async Task<IEnumerable<TeamMemberDto>> GetPendingJoinRequestsAsync(int vendorId)
    {
        var members = await _unitOfWork.VendorTeamMembers
            .QueryNoTracking()
            .Where(m => m.VendorId == vendorId &&
                       m.Source != TeamMemberSource.Invitation &&
                       m.TeamMemberStatusId == 1) // Pending
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        var result = new List<TeamMemberDto>();
        foreach (var m in members)
            result.Add(await MapToDtoAsync(m));

        return result;
    }

    public async Task<TeamMemberDto?> GetUserPendingRequestAsync(int userId)
    {
        var member = await _unitOfWork.VendorTeamMembers
            .QueryNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId && m.TeamMemberStatusId == 1); // Pending

        return member == null ? null : await MapToDtoAsync(member);
    }

    public async Task<bool> HasPendingMemberAsync(string email, int vendorId)
    {
        return await _unitOfWork.VendorTeamMembers
            .AnyAsync(m => m.Email.ToLower() == email.ToLower() &&
                          m.VendorId == vendorId &&
                          m.TeamMemberStatusId == 1); // Pending
    }

    // ========================================
    // DOMAIN MATCHING
    // ========================================

    public async Task<TeamMemberDto?> CheckDomainMatchAsync(int userId, string email)
    {
        var domain = ExtractDomain(email);
        if (string.IsNullOrEmpty(domain) || PersonalEmailDomains.Contains(domain))
            return null;

        var vendor = await _unitOfWork.Vendors
            .QueryNoTracking()
            .Where(v => !v.IsDeleted && v.VendorStatusId == 3) // Active
            .Where(v => v.Website != null && v.Website.Contains(domain))
            .FirstOrDefaultAsync();

        if (vendor == null) return null;

        // Eslesen vendor icin sahte bir TeamMemberDto don (henuz kayit yok)
        return new TeamMemberDto
        {
            VendorId = vendor.Id,
            VendorName = vendor.CompanyName,
            Email = email,
            Source = TeamMemberSource.DomainMatch,
            TeamMemberStatusId = 1 // Pending
        };
    }

    public async Task<bool> UserHasVendorAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.VendorId.HasValue ?? false;
    }

    // ========================================
    // HELPERS
    // ========================================

    private static string GenerateToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private static string? ExtractDomain(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        var parts = email.Split('@');
        return parts.Length == 2 ? parts[1].ToLower() : null;
    }

    private async Task<TeamMemberDto> MapToDtoAsync(VendorTeamMember member)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(member.VendorId);
        ApplicationUser? processedBy = null;

        if (member.ProcessedByUserId.HasValue)
            processedBy = await _userManager.FindByIdAsync(member.ProcessedByUserId.Value.ToString());

        return new TeamMemberDto
        {
            Id = member.Id,
            VendorId = member.VendorId,
            VendorName = vendor?.CompanyName ?? "Bilinmiyor",
            UserId = member.UserId,
            Email = member.Email,
            Name = member.Name,
            Source = member.Source,
            TeamMemberStatusId = member.TeamMemberStatusId,
            CreatedAt = member.CreatedAt,
            ExpiresAt = member.ExpiresAt,
            IsExpired = member.IsExpired,
            IsValidInvitation = member.IsValidInvitation,
            IsActiveMember = member.IsActiveMember,
            ProcessedByUserName = processedBy != null ? $"{processedBy.FirstName} {processedBy.LastName}" : null,
            ProcessedAt = member.ProcessedAt,
            Message = member.Message,
            RejectionReason = member.RejectionReason
        };
    }
}
