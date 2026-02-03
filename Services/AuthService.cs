using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bridgo.Authorization;
using Bridgo.Data;
using Bridgo.DTOs.Auth;
using Bridgo.Models.Enums;
using Bridgo.Models.Identity;

namespace Bridgo.Services;

public interface IAuthService
{
    Task<(JwtLoginResponse? Response, string? Error)> LoginAsync(JwtLoginRequest request, string? ipAddress);
    Task<(RefreshTokenResponse? Response, string? Error)> RefreshAsync(RefreshTokenRequest request, string? ipAddress);
    Task RevokeAsync(string refreshToken);
    Task RevokeAllAsync(int userId);
    Task<JwtUserDto?> GetCurrentUserAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ApplicationDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _context = context;
    }

    public async Task<(JwtLoginResponse? Response, string? Error)> LoginAsync(JwtLoginRequest request, string? ipAddress)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return (null, "Gecersiz e-posta veya sifre");

        if (!user.IsActive)
            return (null, "Hesabiniz devre disi birakilmis");

        var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (passwordCheck.IsLockedOut)
            return (null, "Hesabiniz gecici olarak kilitlenmis. Lutfen daha sonra tekrar deneyin");
        if (!passwordCheck.Succeeded)
            return (null, "Gecersiz e-posta veya sifre");

        var roles = await _userManager.GetRolesAsync(user);
        var tokenPair = await _jwtService.GenerateTokenPairAsync(user, roles, request.DeviceInfo, ipAddress);

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var userDto = await BuildUserDtoAsync(user, roles);

        return (new JwtLoginResponse
        {
            AccessToken = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            AccessTokenExpiresAt = tokenPair.AccessTokenExpiresAt,
            RefreshTokenExpiresAt = tokenPair.RefreshTokenExpiresAt,
            User = userDto
        }, null);
    }

    public async Task<(RefreshTokenResponse? Response, string? Error)> RefreshAsync(RefreshTokenRequest request, string? ipAddress)
    {
        var tokenPair = await _jwtService.RefreshTokenPairAsync(request.RefreshToken, ipAddress: ipAddress);
        if (tokenPair == null)
            return (null, "Gecersiz veya suresi dolmus refresh token");

        return (new RefreshTokenResponse
        {
            AccessToken = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            AccessTokenExpiresAt = tokenPair.AccessTokenExpiresAt,
            RefreshTokenExpiresAt = tokenPair.RefreshTokenExpiresAt
        }, null);
    }

    public async Task RevokeAsync(string refreshToken)
    {
        await _jwtService.RevokeRefreshTokenAsync(refreshToken);
    }

    public async Task RevokeAllAsync(int userId)
    {
        await _jwtService.RevokeAllRefreshTokensAsync(userId);
    }

    public async Task<JwtUserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return await BuildUserDtoAsync(user, roles);
    }

    private async Task<JwtUserDto> BuildUserDtoAsync(ApplicationUser user, IList<string> roles)
    {
        var capabilities = new List<string>();
        string? companyName = null;

        if (user.VendorId.HasValue)
        {
            // Vendor company name
            companyName = await _context.Vendors
                .Where(v => v.Id == user.VendorId.Value)
                .Select(v => v.CompanyName)
                .FirstOrDefaultAsync();

            // Capabilities
            var capabilityIds = await _context.VendorCapabilityMappings
                .Where(m => m.VendorId == user.VendorId.Value && m.IsActive)
                .Select(m => m.CapabilityId)
                .ToListAsync();

            foreach (var capabilityId in capabilityIds)
            {
                var capability = Capabilities.GetById(capabilityId);
                if (capability != null)
                    capabilities.Add(capability.SystemName);
            }
        }

        return new JwtUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            VendorId = user.VendorId,
            CompanyName = companyName,
            IsSystemAdmin = user.IsSystemAdmin,
            Roles = roles.ToList(),
            Capabilities = capabilities
        };
    }
}
