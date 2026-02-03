using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bridgo.DTOs.Auth;
using Bridgo.Services;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthApiController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// JWT token ile giris yap
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] JwtLoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { error = "Gecersiz istek", details = ModelState });

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (response, error) = await _authService.LoginAsync(request, ipAddress);

        if (response == null)
            return Unauthorized(new { error });

        return Ok(response);
    }

    /// <summary>
    /// Refresh token ile yeni access token al
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { error = "Gecersiz istek", details = ModelState });

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (response, error) = await _authService.RefreshAsync(request, ipAddress);

        if (response == null)
            return Unauthorized(new { error });

        return Ok(response);
    }

    /// <summary>
    /// Tek bir refresh token'i iptal et (logout)
    /// </summary>
    [HttpPost("revoke")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { error = "Gecersiz istek", details = ModelState });

        await _authService.RevokeAsync(request.RefreshToken);
        return Ok(new { message = "Token basariyla iptal edildi" });
    }

    /// <summary>
    /// Tum refresh token'lari iptal et (tum cihazlardan cikis)
    /// </summary>
    [HttpPost("revoke-all")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> RevokeAll()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { error = "Gecersiz kullanici" });

        await _authService.RevokeAllAsync(userId);
        return Ok(new { message = "Tum token'lar basariyla iptal edildi" });
    }

    /// <summary>
    /// Mevcut kullanici bilgilerini getir
    /// </summary>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { error = "Gecersiz kullanici" });

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
            return NotFound(new { error = "Kullanici bulunamadi" });

        return Ok(user);
    }
}
