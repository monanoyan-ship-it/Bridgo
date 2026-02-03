using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Bridgo.DTOs.ProductReview;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Urun degerlendirme API endpoint'leri
/// </summary>
[ApiController]
[Route("api/product-reviews")]
[Authorize]
public class ProductReviewsApiController : ControllerBase
{
    private readonly IProductReviewService _reviewService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductReviewsApiController(
        IProductReviewService reviewService,
        UserManager<ApplicationUser> userManager)
    {
        _reviewService = reviewService;
        _userManager = userManager;
    }

    // ===== BUYER ENDPOINTS =====

    /// <summary>
    /// Buyer - Benim degerlendirmelerim
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReviews()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        var reviews = await _reviewService.GetMyReviewsAsync(user.VendorId.Value);
        return Ok(reviews);
    }

    /// <summary>
    /// Buyer - Yeni degerlendirme olustur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductReviewCreateDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        if (dto.OverallRating < 1 || dto.OverallRating > 5)
            return BadRequest(new { message = "Puan 1-5 arasinda olmalidir." });

        try
        {
            var review = await _reviewService.CreateAsync(dto, user.VendorId.Value, user.Id);
            return Ok(review);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Buyer - Degerlendirme guncelle
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductReviewUpdateDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        try
        {
            var result = await _reviewService.UpdateAsync(id, dto, user.VendorId.Value);
            if (!result)
                return NotFound(new { message = "Degerlendirme bulunamadi." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Buyer - Degerlendirme sil
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        var result = await _reviewService.DeleteAsync(id, user.VendorId.Value);
        if (!result)
            return NotFound(new { message = "Degerlendirme bulunamadi." });

        return NoContent();
    }

    /// <summary>
    /// Buyer - Bu urunu degerlendirme yapabilir mi?
    /// </summary>
    [HttpGet("can-review/{productId:int}")]
    public async Task<IActionResult> CanReview(int productId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        var canReview = await _reviewService.CanReviewProductAsync(productId, user.VendorId.Value);
        return Ok(new { canReview });
    }

    // ===== SELLER ENDPOINTS =====

    /// <summary>
    /// Seller - Gelen degerlendirmeler
    /// </summary>
    [HttpGet("incoming")]
    public async Task<IActionResult> GetIncomingReviews()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        var reviews = await _reviewService.GetIncomingReviewsAsync(user.VendorId.Value);
        return Ok(reviews);
    }

    /// <summary>
    /// Seller - Degerlendirme istatistikleri
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        var stats = await _reviewService.GetStatsAsync(user.VendorId.Value);
        return Ok(stats);
    }

    /// <summary>
    /// Seller - Degerlendirmeye yanit ver
    /// </summary>
    [HttpPost("{id:int}/respond")]
    public async Task<IActionResult> Respond(int id, [FromBody] SellerReviewResponseDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.VendorId == null)
            return Forbid();

        try
        {
            var result = await _reviewService.RespondToReviewAsync(id, dto, user.VendorId.Value);
            if (!result)
                return NotFound(new { message = "Degerlendirme bulunamadi." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Degerlendirme durumlari listesi
    /// </summary>
    [HttpGet("statuses")]
    public IActionResult GetStatuses()
    {
        var statuses = Models.Enums.ProductReviewStatuses.All.Select(s => new
        {
            id = s.Id,
            systemName = s.SystemName,
            nameResourceKey = s.NameResourceKey,
            cssClass = s.CssClass,
            icon = s.Icon
        });

        return Ok(statuses);
    }
}
