using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Authorization;
using Bridgo.DTOs.Proposal;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[Route("api/proposals")]
[ApiController]
[Authorize]
[RequireCapability(VendorCapabilities.Seller)]
public class ProposalsApiController : ControllerBase
{
    private readonly IProposalService _proposalService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProposalsApiController(
        IProposalService proposalService,
        UserManager<ApplicationUser> userManager)
    {
        _proposalService = proposalService;
        _userManager = userManager;
    }

    // ============================================
    // SELLER PROPOSALS
    // ============================================

    /// <summary>
    /// Satıcının tüm tekliflerini listele (birleşik)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<UnifiedProposalSearchResultDto>> GetProposals([FromQuery] UnifiedProposalFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { message = "Vendor bilgisi bulunamadı" });

        var result = await _proposalService.GetSellerProposalsAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Sadece ürün tekliflerini listele (ProductInquiryResponse)
    /// </summary>
    [HttpGet("inquiry")]
    public async Task<ActionResult<UnifiedProposalSearchResultDto>> GetInquiryProposals([FromQuery] UnifiedProposalFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { message = "Vendor bilgisi bulunamadı" });

        var result = await _proposalService.GetInquiryProposalsAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Sadece talep tekliflerini listele (DemandResponse)
    /// </summary>
    [HttpGet("demand")]
    public async Task<ActionResult<UnifiedProposalSearchResultDto>> GetDemandProposals([FromQuery] UnifiedProposalFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { message = "Vendor bilgisi bulunamadı" });

        var result = await _proposalService.GetDemandProposalsAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Teklif istatistiklerini getir
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<UnifiedProposalStatsDto>> GetStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized(new { message = "Vendor bilgisi bulunamadı" });

        var stats = await _proposalService.GetSellerProposalStatsAsync(vendorId.Value);
        return Ok(stats);
    }

    // ============================================
    // HELPERS
    // ============================================

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;
        return await _userManager.FindByIdAsync(userId);
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.VendorId;
    }
}
