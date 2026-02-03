using System.ComponentModel.DataAnnotations;

namespace Bridgo.DTOs.Auth;

public class JwtLoginRequest
{
    [Required(ErrorMessage = "E-posta adresi zorunludur")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre zorunludur")]
    public string Password { get; set; } = string.Empty;

    public string? DeviceInfo { get; set; }
}

public class JwtLoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public JwtUserDto User { get; set; } = null!;
}

public class JwtUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int? VendorId { get; set; }
    public string? CompanyName { get; set; }
    public bool IsSystemAdmin { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Capabilities { get; set; } = new();
}

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token zorunludur")]
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
}

public class RevokeTokenRequest
{
    [Required(ErrorMessage = "Refresh token zorunludur")]
    public string RefreshToken { get; set; } = string.Empty;
}
