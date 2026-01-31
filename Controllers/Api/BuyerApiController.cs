using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.Authorization;
using Bridgo.DTOs.Buyer;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/buyer")]
[Authorize]
[RequireCapability(VendorCapabilities.Buyer)]
public class BuyerApiController : ControllerBase
{
    private readonly IBuyerService _buyerService;
    private readonly UserManager<ApplicationUser> _userManager;

    public BuyerApiController(
        IBuyerService buyerService,
        UserManager<ApplicationUser> userManager)
    {
        _buyerService = buyerService;
        _userManager = userManager;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        return await _userManager.GetUserAsync(User);
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.VendorId;
    }

    // ============================================
    // TEDARİKÇİ KEŞFİ
    // ============================================

    /// <summary>
    /// Tedarikçileri ara ve listele
    /// </summary>
    [HttpGet("suppliers")]
    public async Task<IActionResult> SearchSuppliers([FromQuery] SupplierFilterDto filter)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var result = await _buyerService.SearchSuppliersAsync(vendorId.Value, filter);
        return Ok(result);
    }

    /// <summary>
    /// Tedarikçi detayını getir
    /// </summary>
    [HttpGet("suppliers/{vendorId:int}")]
    public async Task<IActionResult> GetSupplierDetail(int vendorId)
    {
        var buyerVendorId = await GetUserVendorIdAsync();
        if (!buyerVendorId.HasValue)
            return Unauthorized();

        var result = await _buyerService.GetSupplierDetailAsync(buyerVendorId.Value, vendorId);
        if (result == null)
            return NotFound(new { message = "Tedarikçi bulunamadı." });

        return Ok(result);
    }

    /// <summary>
    /// Önerilen tedarikçileri getir
    /// </summary>
    [HttpGet("suppliers/recommended")]
    public async Task<IActionResult> GetRecommendedSuppliers([FromQuery] int count = 10)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var result = await _buyerService.GetRecommendedSuppliersAsync(vendorId.Value, count);
        return Ok(result);
    }

    // ============================================
    // SİPARİŞ GEÇMİŞİNDEN TEDARİKÇİLER
    // ============================================

    /// <summary>
    /// Sipariş geçmişinden tedarikçileri getir
    /// </summary>
    [HttpGet("order-suppliers")]
    public async Task<IActionResult> GetOrderSuppliers()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var result = await _buyerService.GetOrderSuppliersAsync(vendorId.Value);
        return Ok(result);
    }

    // ============================================
    // FAVORİ TEDARİKÇİLER
    // ============================================

    /// <summary>
    /// Favori tedarikçileri listele
    /// </summary>
    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavorites()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var result = await _buyerService.GetFavoriteVendorsAsync(vendorId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Favori tedarikçi ekle
    /// </summary>
    [HttpPost("favorites")]
    public async Task<IActionResult> AddFavorite([FromBody] FavoriteVendorCreateDto dto)
    {
        var user = await GetCurrentUserAsync();
        if (user?.VendorId == null)
            return Unauthorized();

        var result = await _buyerService.AddFavoriteVendorAsync(user.VendorId.Value, user.Id, dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    /// <summary>
    /// Favori tedarikçiyi güncelle
    /// </summary>
    [HttpPut("favorites/{id:int}")]
    public async Task<IActionResult> UpdateFavorite(int id, [FromBody] FavoriteVendorUpdateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var result = await _buyerService.UpdateFavoriteVendorAsync(vendorId.Value, id, dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Favori tedarikçiyi kaldır
    /// </summary>
    [HttpDelete("favorites/{id:int}")]
    public async Task<IActionResult> RemoveFavorite(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var result = await _buyerService.RemoveFavoriteVendorAsync(vendorId.Value, id);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Tedarikçinin favori olup olmadığını kontrol et
    /// </summary>
    [HttpGet("favorites/check/{sellerVendorId:int}")]
    public async Task<IActionResult> CheckFavorite(int sellerVendorId)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var isFavorite = await _buyerService.IsFavoriteAsync(vendorId.Value, sellerVendorId);
        return Ok(new { isFavorite });
    }

    // ============================================
    // TEDARİKÇİ DEĞERLENDİRMELERİ
    // ============================================

    /// <summary>
    /// Tedarikçinin değerlendirmelerini getir
    /// </summary>
    [HttpGet("suppliers/{vendorId:int}/reviews")]
    public async Task<IActionResult> GetSupplierReviews(int vendorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var reviews = await _buyerService.GetVendorReviewsAsync(vendorId, page, pageSize);
        return Ok(reviews);
    }

    /// <summary>
    /// Tedarikçinin rating istatistiklerini getir
    /// </summary>
    [HttpGet("suppliers/{vendorId:int}/rating-stats")]
    public async Task<IActionResult> GetSupplierRatingStats(int vendorId)
    {
        var stats = await _buyerService.GetVendorRatingStatsAsync(vendorId);
        return Ok(stats);
    }

    /// <summary>
    /// Yeni değerlendirme oluştur
    /// </summary>
    [HttpPost("reviews")]
    public async Task<IActionResult> CreateReview([FromBody] VendorReviewCreateDto dto)
    {
        var user = await GetCurrentUserAsync();
        if (user?.VendorId == null)
            return Unauthorized();

        var result = await _buyerService.CreateReviewAsync(user.VendorId.Value, user.Id, dto);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result.Data);
    }

    /// <summary>
    /// Kullanıcının yaptığı değerlendirmeleri getir
    /// </summary>
    [HttpGet("reviews/my")]
    public async Task<IActionResult> GetMyReviews()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var reviews = await _buyerService.GetMyReviewsAsync(vendorId.Value);
        return Ok(reviews);
    }

    /// <summary>
    /// Sipariş için değerlendirme var mı kontrol et
    /// </summary>
    [HttpGet("reviews/order/{orderId:int}")]
    public async Task<IActionResult> GetReviewByOrder(int orderId)
    {
        var review = await _buyerService.GetReviewByOrderAsync(orderId);
        if (review == null)
            return Ok(new { exists = false });

        return Ok(new { exists = true, review });
    }

    // ============================================
    // İSTATİSTİKLER
    // ============================================

    /// <summary>
    /// Alıcı istatistiklerini getir
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetBuyerStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var stats = await _buyerService.GetBuyerStatsAsync(vendorId.Value);
        return Ok(stats);
    }

    // ============================================
    // TEKLİF KARŞILAŞTIRMA
    // ============================================

    /// <summary>
    /// Teklifli talepleri getir
    /// </summary>
    [HttpGet("proposals/demands")]
    public async Task<IActionResult> GetDemandsWithProposals()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var result = await _buyerService.GetDemandsWithProposalsAsync(vendorId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Teklifli ürün isteklerini getir
    /// </summary>
    [HttpGet("proposals/inquiries")]
    public async Task<IActionResult> GetInquiriesWithProposals()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var result = await _buyerService.GetInquiriesWithProposalsAsync(vendorId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Teklif karşılaştırma istatistikleri
    /// </summary>
    [HttpGet("proposals/stats")]
    public async Task<IActionResult> GetProposalCompareStats()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        var result = await _buyerService.GetProposalCompareStatsAsync(vendorId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Talep teklifinin durumunu güncelle
    /// </summary>
    [HttpPost("proposals/demands/{proposalId:int}/action")]
    public async Task<IActionResult> UpdateDemandProposalStatus(int proposalId, [FromBody] ProposalActionDto action)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        action.ProposalId = proposalId;
        var result = await _buyerService.UpdateProposalStatusAsync(vendorId.Value, proposalId, action);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Ürün isteği teklifinin durumunu güncelle
    /// </summary>
    [HttpPost("proposals/inquiries/{proposalId:int}/action")]
    public async Task<IActionResult> UpdateInquiryProposalStatus(int proposalId, [FromBody] ProposalActionDto action)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Unauthorized();

        action.ProposalId = proposalId;
        var result = await _buyerService.UpdateInquiryProposalStatusAsync(vendorId.Value, proposalId, action);
        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }
}
