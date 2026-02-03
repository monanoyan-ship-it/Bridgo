using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Bridgo.Authorization;
using Bridgo.Data;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Models.Identity;

namespace Bridgo.Services;

public class TokenPairResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
}

public interface IJwtService
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
    ClaimsPrincipal? ValidateToken(string token);
    Task<TokenPairResult> GenerateTokenPairAsync(ApplicationUser user, IList<string> roles, string? deviceInfo = null, string? ipAddress = null);
    Task<TokenPairResult?> RefreshTokenPairAsync(string refreshToken, string? deviceInfo = null, string? ipAddress = null);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task RevokeAllRefreshTokensAsync(int userId);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtService(IConfiguration configuration, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _configuration = configuration;
        _context = context;
        _userManager = userManager;
    }

    public string GenerateToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = BuildClaims(user, roles);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<TokenPairResult> GenerateTokenPairAsync(ApplicationUser user, IList<string> roles, string? deviceInfo = null, string? ipAddress = null)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var accessMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");
        var refreshDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "30");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jti = Guid.NewGuid().ToString();
        var claims = await BuildClaimsWithCapabilitiesAsync(user, roles, jti);

        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(accessMinutes);
        var accessToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: accessTokenExpiry,
            signingCredentials: credentials
        );

        var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

        // Generate refresh token
        var refreshTokenString = GenerateRefreshTokenString();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(refreshDays);

        // Hash and store refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshTokenString),
            JwtId = jti,
            ExpiresAt = refreshTokenExpiry,
            IsRevoked = false,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new TokenPairResult
        {
            AccessToken = accessTokenString,
            RefreshToken = refreshTokenString,
            AccessTokenExpiresAt = accessTokenExpiry,
            RefreshTokenExpiresAt = refreshTokenExpiry
        };
    }

    public async Task<TokenPairResult?> RefreshTokenPairAsync(string refreshToken, string? deviceInfo = null, string? ipAddress = null)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && !r.IsDeleted);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt <= DateTime.UtcNow)
            return null;

        var user = storedToken.User;
        if (user == null || !user.IsActive)
            return null;

        // Revoke the old refresh token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        _context.RefreshTokens.Update(storedToken);
        await _context.SaveChangesAsync();

        // Get roles for user
        var roles = await _userManager.GetRolesAsync(user);

        // Generate new token pair
        return await GenerateTokenPairAsync(user, roles, deviceInfo, ipAddress);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && !r.IsDeleted);

        if (storedToken != null && !storedToken.IsRevoked)
        {
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllRefreshTokensAsync(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked && !r.IsDeleted)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        if (tokens.Count > 0)
        {
            _context.RefreshTokens.UpdateRange(tokens);
            await _context.SaveChangesAsync();
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private List<Claim> BuildClaims(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new("firstName", user.FirstName),
            new("lastName", user.LastName),
            new("vendorId", user.VendorId?.ToString() ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    private async Task<List<Claim>> BuildClaimsWithCapabilitiesAsync(ApplicationUser user, IList<string> roles, string jti)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new("firstName", user.FirstName),
            new("lastName", user.LastName),
            new("IsSystemAdmin", user.IsSystemAdmin.ToString().ToLower()),
            new(JwtRegisteredClaimNames.Jti, jti)
        };

        // Roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // VendorId + VendorCapability claims (mirrors CustomUserClaimsPrincipalFactory)
        if (user.VendorId.HasValue)
        {
            claims.Add(new Claim("VendorId", user.VendorId.Value.ToString()));

            var capabilityIds = await _context.VendorCapabilityMappings
                .Where(m => m.VendorId == user.VendorId.Value && m.IsActive)
                .Select(m => m.CapabilityId)
                .ToListAsync();

            foreach (var capabilityId in capabilityIds)
            {
                var capability = Capabilities.GetById(capabilityId);
                if (capability != null)
                {
                    claims.Add(new Claim(CapabilityAuthorizationHandler.CapabilityClaimType, capability.SystemName));
                }
            }
        }

        return claims;
    }

    private static string GenerateRefreshTokenString()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }
}
