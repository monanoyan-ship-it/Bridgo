using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.DTOs.Team;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Models.Identity;
using Bridgo.Repositories;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Vendor ekip yonetimi API Controller
/// Davet gondermesi, istek onaylama, ekip listeleme
/// </summary>
[Authorize]
[ApiController]
[Route("api/team")]
public class TeamApiController : ControllerBase
{
    private readonly ITeamService _teamService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public TeamApiController(
        ITeamService teamService,
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork)
    {
        _teamService = teamService;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = GetUserId();
        return userId == 0 ? null : await _userManager.FindByIdAsync(userId.ToString());
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.VendorId;
    }

    private async Task<bool> IsOwnerOrAdminAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user?.VendorId == null) return false;

        // VendorTeamMember'dan Source kontrolu
        var member = await _unitOfWork.VendorTeamMembers
            .QueryNoTracking()
            .FirstOrDefaultAsync(m => m.VendorId == user.VendorId && m.UserId == user.Id && !m.IsDeleted);

        return member?.Source == TeamMemberSource.OwnerCreated;
    }

    // ========================================
    // EKIP LISTELEMELERI
    // ========================================

    /// <summary>
    /// Vendor'in tum ekip uyelerini listele (yeni /members endpoint'i)
    /// </summary>
    [HttpGet("members")]
    public async Task<ActionResult> GetMembers()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var members = await _unitOfWork.VendorTeamMembers
            .QueryNoTracking()
            .Where(m => m.VendorId == vendorId.Value && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new
            {
                id = m.Id,
                userId = m.UserId,
                memberTypeId = m.MemberTypeId,
                email = m.Email,
                name = m.Name,
                phone = m.Phone,
                title = m.Title,
                identityNumber = m.IdentityNumber,
                sharePercentage = m.SharePercentage,
                isAuthorizedSignatory = m.IsAuthorizedSignatory,
                isLegalRepresentative = m.IsLegalRepresentative,
                source = m.Source.ToString(),
                teamMemberStatusId = m.TeamMemberStatusId,
                verificationStatusId = m.VerificationStatusId,
                isExpired = m.ExpiresAt.HasValue && DateTime.UtcNow > m.ExpiresAt.Value,
                createdAt = m.CreatedAt
            })
            .ToListAsync();

        return Ok(members);
    }

    /// <summary>
    /// Vendor'in tum ekip uyelerini listele (eski endpoint)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetAll()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var members = await _teamService.GetTeamMembersAsync(vendorId.Value);
        return Ok(members);
    }

    /// <summary>
    /// Vendor'in aktif ekip uyelerini listele
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetActive()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var members = await _teamService.GetActiveTeamMembersAsync(vendorId.Value);
        return Ok(members);
    }

    /// <summary>
    /// Bekleyen davetleri listele
    /// </summary>
    [HttpGet("invitations/pending")]
    public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetPendingInvitations()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var invitations = await _teamService.GetPendingInvitationsAsync(vendorId.Value);
        return Ok(invitations);
    }

    /// <summary>
    /// Bekleyen katilma isteklerini listele
    /// </summary>
    [HttpGet("requests/pending")]
    public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetPendingRequests()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        var requests = await _teamService.GetPendingJoinRequestsAsync(vendorId.Value);
        return Ok(requests);
    }

    // ========================================
    // DAVET ISLEMLERI
    // ========================================

    /// <summary>
    /// Yeni davet olustur
    /// </summary>
    [HttpPost("invitations")]
    public async Task<ActionResult<TeamOperationResultDto>> CreateInvitation([FromBody] CreateInvitationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        var vendorId = await GetUserVendorIdAsync();

        if (!vendorId.HasValue || userId == 0)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        var result = await _teamService.CreateInvitationAsync(vendorId.Value, dto, userId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Daveti iptal et
    /// </summary>
    [HttpDelete("invitations/{memberId}")]
    public async Task<ActionResult> CancelInvitation(int memberId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        var success = await _teamService.CancelInvitationAsync(memberId, vendorId.Value);
        if (!success)
            return NotFound(new { message = "Davet bulunamadi" });

        return Ok(new { success = true, message = "Davet iptal edildi" });
    }

    /// <summary>
    /// Daveti yeniden gonder
    /// </summary>
    [HttpPost("invitations/{memberId}/resend")]
    public async Task<ActionResult<TeamMemberDto>> ResendInvitation(int memberId)
    {
        var userId = GetUserId();
        var vendorId = await GetUserVendorIdAsync();

        if (!vendorId.HasValue || userId == 0)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        var result = await _teamService.ResendInvitationAsync(memberId, vendorId.Value, userId);
        if (result == null)
            return BadRequest(new { message = "Davet yenilenemedi" });

        return Ok(result);
    }

    // ========================================
    // ISTEK ISLEMLERI
    // ========================================

    /// <summary>
    /// Katilma istegini onayla/reddet
    /// </summary>
    [HttpPost("requests/process")]
    public async Task<ActionResult<TeamOperationResultDto>> ProcessRequest([FromBody] ProcessRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        var vendorId = await GetUserVendorIdAsync();

        if (!vendorId.HasValue || userId == 0)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        var result = await _teamService.ProcessJoinRequestAsync(dto, userId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Vendor'in capability'lerine bagli CompanyRole'leri listele
    /// Katilma istegi onaylarken rol secimi icin kullanilir
    /// </summary>
    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<CompanyRoleDto>>> GetCompanyRoles()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        // Vendor'in capability ID'lerini al
        var vendorCapabilityIds = await _unitOfWork.VendorCapabilityMappings
            .QueryNoTracking()
            .Where(m => m.VendorId == vendorId.Value && m.IsActive)
            .Select(m => m.CapabilityId)
            .Distinct()
            .ToListAsync();

        // Bu capability'lere ait aktif rolleri getir
        var rolesQuery = await _unitOfWork.CompanyRoles
            .QueryNoTracking()
            .Where(r => r.IsActive && vendorCapabilityIds.Contains(r.CapabilityId))
            .OrderBy(r => r.CapabilityId)
            .ThenBy(r => r.Name)
            .ToListAsync();

        // Static Capabilities class'indan bilgileri al
        var roles = rolesQuery.Select(r => {
            var cap = Capabilities.GetById(r.CapabilityId);
            return new CompanyRoleDto
            {
                Id = r.Id,
                Name = r.Name,
                NameResourceKey = r.NameResourceKey,
                Description = r.Description,
                CapabilityId = r.CapabilityId,
                CapabilityName = cap?.SystemName ?? string.Empty,
                CapabilityNameResourceKey = cap?.NameResourceKey,
                IsDefault = r.IsDefault
            };
        }).OrderBy(r => Capabilities.GetById(r.CapabilityId)?.DisplayOrder ?? 0)
          .ThenBy(r => r.Name)
          .ToList();

        return Ok(roles);
    }

    // ========================================
    // MEMBER CRUD (Manuel ekleme/duzenleme)
    // ========================================

    /// <summary>
    /// Yeni uye ekle (manuel - kullanici olmayan)
    /// </summary>
    [HttpPost("members")]
    public async Task<ActionResult> AddMember([FromBody] AddTeamMemberDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "Ad ve E-posta zorunludur" });

        // Ayni e-posta var mi?
        var exists = await _unitOfWork.VendorTeamMembers
            .AnyAsync(m => m.VendorId == vendorId.Value && m.Email.ToLower() == dto.Email.ToLower() && !m.IsDeleted);

        if (exists)
            return BadRequest(new { message = "Bu e-posta adresi zaten kayitli" });

        var member = new VendorTeamMember
        {
            VendorId = vendorId.Value,
            MemberTypeId = dto.MemberTypeId,
            Email = dto.Email.ToLower(),
            Name = dto.Name,
            Phone = dto.Phone,
            Title = dto.Title,
            IdentityNumber = dto.IdentityNumber,
            SharePercentage = dto.SharePercentage,
            IsAuthorizedSignatory = dto.IsAuthorizedSignatory,
            IsLegalRepresentative = dto.IsLegalRepresentative,
            Source = TeamMemberSource.ManualEntry,
            TeamMemberStatusId = 2, // Active
            VerificationStatusId = 1, // NotVerified
            CreatedBy = User.Identity?.Name
        };

        await _unitOfWork.VendorTeamMembers.AddAsync(member);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { success = true, id = member.Id });
    }

    /// <summary>
    /// Uye guncelle
    /// </summary>
    [HttpPut("members/{id}")]
    public async Task<ActionResult> UpdateMember(int id, [FromBody] AddTeamMemberDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        var member = await _unitOfWork.VendorTeamMembers
            .FirstOrDefaultAsync(m => m.Id == id && m.VendorId == vendorId.Value && !m.IsDeleted);

        if (member == null)
            return NotFound(new { message = "Uye bulunamadi" });

        member.MemberTypeId = dto.MemberTypeId;
        member.Name = dto.Name;
        member.Phone = dto.Phone;
        member.Title = dto.Title;
        member.IdentityNumber = dto.IdentityNumber;
        member.SharePercentage = dto.SharePercentage;
        member.IsAuthorizedSignatory = dto.IsAuthorizedSignatory;
        member.IsLegalRepresentative = dto.IsLegalRepresentative;
        member.UpdatedBy = User.Identity?.Name;

        // E-posta degistiyse kontrol et
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email.ToLower() != member.Email.ToLower())
        {
            var exists = await _unitOfWork.VendorTeamMembers
                .AnyAsync(m => m.VendorId == vendorId.Value && m.Email.ToLower() == dto.Email.ToLower() && m.Id != id && !m.IsDeleted);

            if (exists)
                return BadRequest(new { message = "Bu e-posta adresi baska bir uye tarafindan kullaniliyor" });

            member.Email = dto.Email.ToLower();
        }

        _unitOfWork.VendorTeamMembers.Update(member);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { success = true });
    }

    /// <summary>
    /// Uye sil/cikar
    /// </summary>
    [HttpDelete("members/{id}")]
    public async Task<ActionResult> RemoveMember(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        var member = await _unitOfWork.VendorTeamMembers
            .FirstOrDefaultAsync(m => m.Id == id && m.VendorId == vendorId.Value && !m.IsDeleted);

        if (member == null)
            return NotFound(new { message = "Uye bulunamadi" });

        // Owner olan kullanici silinemez (Source = OwnerCreated)
        if (member.Source == TeamMemberSource.OwnerCreated)
            return BadRequest(new { message = "Firma sahibi silinemez" });

        member.IsDeleted = true;
        member.TeamMemberStatusId = 6; // Left
        member.UpdatedBy = User.Identity?.Name;

        _unitOfWork.VendorTeamMembers.Update(member);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { success = true });
    }

    /// <summary>
    /// Uye dogrula
    /// </summary>
    [HttpPost("members/{id}/verify")]
    public async Task<ActionResult> VerifyMember(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        var member = await _unitOfWork.VendorTeamMembers
            .FirstOrDefaultAsync(m => m.Id == id && m.VendorId == vendorId.Value && !m.IsDeleted);

        if (member == null)
            return NotFound(new { message = "Uye bulunamadi" });

        member.VerificationStatusId = 3; // Verified
        member.VerifiedAt = DateTime.UtcNow;
        member.VerifiedBy = User.Identity?.Name;
        member.UpdatedBy = User.Identity?.Name;

        _unitOfWork.VendorTeamMembers.Update(member);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { success = true });
    }

    /// <summary>
    /// Davet gonder (yeni endpoint)
    /// </summary>
    [HttpPost("invite")]
    public async Task<ActionResult> SendInvite([FromBody] CreateInvitationDto dto)
    {
        var userId = GetUserId();
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue || userId == 0)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        var result = await _teamService.CreateInvitationAsync(vendorId.Value, dto, userId);
        return Ok(result);
    }

    /// <summary>
    /// Daveti tekrar gonder (yeni endpoint)
    /// </summary>
    [HttpPost("resend/{id}")]
    public async Task<ActionResult> ResendInvite(int id)
    {
        var userId = GetUserId();
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue || userId == 0)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        var result = await _teamService.ResendInvitationAsync(id, vendorId.Value, userId);
        return result == null
            ? BadRequest(new { message = "Davet yenilenemedi" })
            : Ok(new { success = true });
    }

    /// <summary>
    /// Daveti iptal et (yeni endpoint)
    /// </summary>
    [HttpDelete("cancel/{id}")]
    public async Task<ActionResult> CancelInvite(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        var success = await _teamService.CancelInvitationAsync(id, vendorId.Value);
        return success
            ? Ok(new { success = true })
            : NotFound(new { message = "Davet bulunamadi" });
    }

    /// <summary>
    /// Istegi isle (yeni endpoint)
    /// </summary>
    [HttpPost("process/{id}")]
    public async Task<ActionResult> ProcessMemberRequest(int id, [FromBody] ProcessRequestDto dto)
    {
        var userId = GetUserId();
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue || userId == 0)
            return Forbid();

        if (!await IsOwnerOrAdminAsync())
            return Forbid();

        dto.MemberId = id;
        var result = await _teamService.ProcessJoinRequestAsync(dto, userId);
        return Ok(result);
    }

    // ========================================
    // PUBLIC ENDPOINTS (Davet kabul)
    // ========================================

    /// <summary>
    /// Davet token'ini dogrula (public)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("invitations/validate/{token}")]
    public async Task<ActionResult<ValidateInvitationResultDto>> ValidateToken(string token)
    {
        var result = await _teamService.ValidateInvitationTokenAsync(token);
        return Ok(result);
    }

    /// <summary>
    /// Davet ile kayit ol (public)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("invitations/accept")]
    public async Task<ActionResult<TeamOperationResultDto>> AcceptInvitation([FromBody] AcceptInvitationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _teamService.AcceptInvitationAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
