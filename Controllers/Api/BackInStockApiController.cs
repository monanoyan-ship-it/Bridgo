using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.DTOs.BackInStock;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/back-in-stock")]
[Authorize]
public class BackInStockApiController : ControllerBase
{
    private readonly IBackInStockService _backInStockService;
    private readonly UserManager<ApplicationUser> _userManager;

    public BackInStockApiController(
        IBackInStockService backInStockService,
        UserManager<ApplicationUser> userManager)
    {
        _backInStockService = backInStockService;
        _userManager = userManager;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var userId = GetUserId();
        if (userId == 0) return null;
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.VendorId;
    }

    [HttpGet]
    public async Task<IActionResult> GetSubscriptions()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _backInStockService.GetSubscriptionsAsync(vendorId.Value);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] BackInStockSubscribeDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _backInStockService.SubscribeAsync(vendorId.Value, dto.ProductId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Unsubscribe(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _backInStockService.UnsubscribeAsync(vendorId.Value, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("check/{productId}")]
    public async Task<IActionResult> CheckSubscription(int productId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var isSubscribed = await _backInStockService.IsSubscribedAsync(vendorId.Value, productId);
        return Ok(new { isSubscribed });
    }
}
