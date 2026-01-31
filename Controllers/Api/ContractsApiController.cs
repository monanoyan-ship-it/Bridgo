using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Models.Enums;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Sozlesmeler ve Faturalar API - Order bazli
/// </summary>
[Authorize]
[ApiController]
[Route("api/contracts")]
public class ContractsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ContractsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var userId = GetUserId();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user?.VendorId;
    }

    /// <summary>
    /// Kullanicinin sozlesmelerini getir (onaylanmis siparisler)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetContracts()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Ok(new List<object>());

        // Onaylanmis ve sonraki durumlardaki siparisler = sozlesme
        var confirmedStatuses = new[] {
            OrderStatuses.Confirmed.Id,
            OrderStatuses.Processing.Id,
            OrderStatuses.Shipped.Id,
            OrderStatuses.InTransit.Id,
            OrderStatuses.Delivered.Id,
            OrderStatuses.Completed.Id
        };

        var orders = await _context.Orders
            .Include(o => o.BuyerVendor)
            .Include(o => o.SellerVendor)
            .Where(o => (o.BuyerVendorId == vendorId || o.SellerVendorId == vendorId)
                        && confirmedStatuses.Contains(o.Status)
                        && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                id = o.Id,
                contractNumber = "SZL-" + o.Id.ToString("D6"),
                orderNumber = o.OrderNumber,
                orderId = o.Id,
                counterparty = o.BuyerVendorId == vendorId ? o.SellerVendor.CompanyName : o.BuyerVendor.CompanyName,
                vendorName = o.SellerVendor.CompanyName,
                buyerName = o.BuyerVendor.CompanyName,
                amount = o.TotalAmount,
                currency = o.Currency,
                status = o.Status >= OrderStatuses.Completed.Id ? "completed"
                       : o.Status >= OrderStatuses.Confirmed.Id ? "signed"
                       : "pending",
                summary = "Siparis sozlesmesi - " + o.OrderNumber,
                createdAt = o.CreatedAt
            })
            .ToListAsync();

        return Ok(orders);
    }

    /// <summary>
    /// Kullanicinin faturalarini getir (tamamlanmis siparisler)
    /// </summary>
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Ok(new List<object>());

        // Tamamlanmis siparisler = fatura kesilmis
        var completedStatuses = new[] {
            OrderStatuses.Delivered.Id,
            OrderStatuses.Completed.Id
        };

        var orders = await _context.Orders
            .Include(o => o.BuyerVendor)
            .Include(o => o.SellerVendor)
            .Where(o => (o.BuyerVendorId == vendorId || o.SellerVendorId == vendorId)
                        && completedStatuses.Contains(o.Status)
                        && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                id = o.Id,
                invoiceNumber = "FTR-" + o.Id.ToString("D6"),
                orderNumber = o.OrderNumber,
                orderId = o.Id,
                // Alici icin alis faturasi, satici icin satis faturasi
                type = o.BuyerVendorId == vendorId ? "purchase" : "sales",
                amount = o.TotalAmount,
                currency = o.Currency,
                status = o.Status == OrderStatuses.Completed.Id ? "paid" : "issued",
                createdAt = o.CreatedAt
            })
            .ToListAsync();

        return Ok(orders);
    }

    /// <summary>
    /// Sozlesme imzala
    /// </summary>
    [HttpPost("{id}/sign")]
    public async Task<IActionResult> SignContract(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized(new { message = "Vendor bulunamadi" });

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id
                && (o.BuyerVendorId == vendorId || o.SellerVendorId == vendorId)
                && !o.IsDeleted);

        if (order == null)
            return NotFound(new { message = "Sozlesme bulunamadi" });

        // Zaten onaylanmis mi?
        if (order.Status >= OrderStatuses.Confirmed.Id)
            return Ok(new { message = "Sozlesme zaten imzali" });

        return Ok(new { message = "Sozlesme imzalandi" });
    }

    /// <summary>
    /// Sozlesme indir (placeholder)
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadContract(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized();

        var order = await _context.Orders
            .Include(o => o.BuyerVendor)
            .Include(o => o.SellerVendor)
            .FirstOrDefaultAsync(o => o.Id == id
                && (o.BuyerVendorId == vendorId || o.SellerVendorId == vendorId)
                && !o.IsDeleted);

        if (order == null)
            return NotFound();

        // TODO: PDF olustur
        // Simdilik placeholder
        return Ok(new { message = "PDF indirme henuz implemente edilmedi", orderId = id });
    }

    /// <summary>
    /// Fatura indir (placeholder)
    /// </summary>
    [HttpGet("invoices/{id}/download")]
    public async Task<IActionResult> DownloadInvoice(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null)
            return Unauthorized();

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id
                && (o.BuyerVendorId == vendorId || o.SellerVendorId == vendorId)
                && !o.IsDeleted);

        if (order == null)
            return NotFound();

        // TODO: PDF olustur
        return Ok(new { message = "PDF indirme henuz implemente edilmedi", orderId = id });
    }
}
