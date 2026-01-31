using System.ComponentModel.DataAnnotations;
using Bridgo.Models.Entities;

namespace Bridgo.DTOs.Team;

/// <summary>
/// Ekip uyesi bilgisi
/// </summary>
public class TeamMemberDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public TeamMemberSource Source { get; set; }
    public int TeamMemberStatusId { get; set; }
    public string? TeamMemberStatusCode { get; set; }
    public string? TeamMemberStatusName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
    public bool IsValidInvitation { get; set; }
    public bool IsActiveMember { get; set; }

    // Davet/Onay yapan
    public string? ProcessedByUserName { get; set; }
    public DateTime? ProcessedAt { get; set; }

    // Istek mesaji veya red nedeni
    public string? Message { get; set; }
    public string? RejectionReason { get; set; }
}

/// <summary>
/// Yeni davet olusturma
/// </summary>
public class CreateInvitationDto
{
    [Required(ErrorMessage = "E-posta zorunludur")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz")]
    public string Email { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Isim en fazla 200 karakter olabilir")]
    public string? Name { get; set; }
}

/// <summary>
/// Katilma istegi olusturma
/// </summary>
public class CreateJoinRequestDto
{
    [Required(ErrorMessage = "Firma secimi zorunludur")]
    public int VendorId { get; set; }

    [StringLength(1000, ErrorMessage = "Mesaj en fazla 1000 karakter olabilir")]
    public string? Message { get; set; }
}

/// <summary>
/// Davet token ile kayit
/// </summary>
public class AcceptInvitationDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ad zorunludur")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre zorunludur")]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "Sifreler eslesmeli")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Istek onaylama/reddetme
/// </summary>
public class ProcessRequestDto
{
    [Required]
    public int MemberId { get; set; }

    [Required]
    public bool Approve { get; set; }

    /// <summary>
    /// Atanacak CompanyRole ID'leri (RBAC rolleri)
    /// </summary>
    public List<int> CompanyRoleIds { get; set; } = new();

    [StringLength(500)]
    public string? RejectionReason { get; set; }
}

/// <summary>
/// Davet dogrulama sonucu
/// </summary>
public class ValidateInvitationResultDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public TeamMemberDto? Member { get; set; }
}

/// <summary>
/// Islem sonucu
/// </summary>
public class TeamOperationResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? MemberId { get; set; }
    public int? UserId { get; set; }
    public int? VendorId { get; set; }
    public string? VendorName { get; set; }
}

/// <summary>
/// CompanyRole bilgisi - Katilma istegi onayinda rol secimi icin
/// </summary>
public class CompanyRoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameResourceKey { get; set; }
    public string? Description { get; set; }
    public int CapabilityId { get; set; }
    public string CapabilityName { get; set; } = string.Empty;
    public string? CapabilityNameResourceKey { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// Manuel uye ekleme/guncelleme icin DTO
/// </summary>
public class AddTeamMemberDto
{
    public int MemberTypeId { get; set; }
    public int Role { get; set; } = 4; // Employee

    [Required(ErrorMessage = "Ad zorunludur")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Title { get; set; }

    [Required(ErrorMessage = "E-posta zorunludur")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(20)]
    public string? IdentityNumber { get; set; }

    public decimal? SharePercentage { get; set; }
    public bool IsAuthorizedSignatory { get; set; }
    public bool IsLegalRepresentative { get; set; }
}
