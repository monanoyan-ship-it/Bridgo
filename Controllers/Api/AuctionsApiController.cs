using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bridgo.DTOs.Auction;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/auctions")]
[Authorize]
public class AuctionsApiController : ControllerBase
{
    private readonly IAuctionService _auctionService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuctionsApiController(
        IAuctionService auctionService,
        UserManager<ApplicationUser> userManager)
    {
        _auctionService = auctionService;
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

    // ============================================
    // PUBLIC
    // ============================================

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicAuctions([FromQuery] AuctionFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        var result = await _auctionService.GetPublicAuctionsAsync(filter, vendorId);
        return Ok(result);
    }

    // ============================================
    // MY AUCTIONS (Seller)
    // ============================================

    [HttpGet]
    public async Task<IActionResult> GetMyAuctions([FromQuery] AuctionFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _auctionService.GetAuctionsAsync(vendorId.Value, filter);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAuction(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        var result = await _auctionService.GetAuctionByIdAsync(id, vendorId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAuction([FromBody] AuctionCreateUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _auctionService.CreateAuctionAsync(dto, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { id = result.Data, message = result.Message });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAuction(int id, [FromBody] AuctionCreateUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _auctionService.UpdateAuctionAsync(id, dto, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> ActivateAuction(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _auctionService.ActivateAuctionAsync(id, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelAuction(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _auctionService.CancelAuctionAsync(id, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // ============================================
    // BIDS (Buyer)
    // ============================================

    [HttpPost("{id:int}/bid")]
    public async Task<IActionResult> PlaceBid(int id, [FromBody] PlaceBidDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _auctionService.PlaceBidAsync(id, dto, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpGet("{id:int}/bids")]
    public async Task<IActionResult> GetBids(int id)
    {
        var result = await _auctionService.GetBidsAsync(id);
        return Ok(result);
    }

    // ============================================
    // WATCHLIST
    // ============================================

    [HttpPost("{id:int}/watch")]
    public async Task<IActionResult> ToggleWatch(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _auctionService.ToggleWatchAsync(id, vendorId.Value);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpGet("watchlist")]
    public async Task<IActionResult> GetWatchlist([FromQuery] AuctionFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized();

        var result = await _auctionService.GetWatchedAuctionsAsync(vendorId.Value, filter);
        return Ok(result);
    }
}
