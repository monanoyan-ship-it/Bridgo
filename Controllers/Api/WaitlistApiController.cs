using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Bridgo.DTOs.Waitlist;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[AllowAnonymous]
[EnableCors("LandingPage")]
[ApiController]
[Route("api/waitlist")]
public class WaitlistApiController : ControllerBase
{
    private readonly IWaitlistService _waitlistService;

    public WaitlistApiController(IWaitlistService waitlistService)
    {
        _waitlistService = waitlistService;
    }

    [HttpPost]
    public async Task<IActionResult> Join([FromBody] WaitlistRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { success = false, message = "Email is required." });

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var (success, message) = await _waitlistService.JoinAsync(
            request.Email, request.Source, ipAddress, userAgent);

        return Ok(new { success, message });
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        var count = await _waitlistService.GetCountAsync();
        return Ok(new { count });
    }
}
