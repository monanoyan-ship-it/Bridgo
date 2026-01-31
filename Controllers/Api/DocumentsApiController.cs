using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.Authorization;
using Bridgo.Data;
using Bridgo.Models.Entities;
using Bridgo.Models.Identity;

namespace Bridgo.Controllers.Api;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public DocumentsApiController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
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

    /// <summary>
    /// Tum belgeleri getir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDocuments()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var documents = await _context.VendorDocuments
            .Where(d => d.VendorId == vendorId.Value)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new
            {
                id = d.Id,
                fileName = d.FileName,
                category = d.Category,
                type = d.DocumentType,
                uploadedAt = d.CreatedAt,
                status = d.Status
            })
            .ToListAsync();

        return Ok(documents);
    }

    /// <summary>
    /// Belge yukle
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument([FromForm] IFormFile file, [FromForm] string category, [FromForm] string type)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya secilmedi." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Dosya boyutu 5MB'dan buyuk olamaz." });

        var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Gecersiz dosya tipi. PDF, JPG, PNG veya GIF yukleyebilirsiniz." });

        var validCategories = new[] { "identity", "company", "bank", "auth" };
        if (!validCategories.Contains(category))
            return BadRequest(new { message = "Gecersiz kategori." });

        // Create directory if not exists
        var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "documents", vendorId.Value.ToString());
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        // Generate unique filename
        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Save to database
        var document = new VendorDocument
        {
            VendorId = vendorId.Value,
            UploadedByUserId = GetUserId(),
            Category = category,
            DocumentType = type,
            FileName = file.FileName,
            FilePath = $"/uploads/documents/{vendorId.Value}/{uniqueFileName}",
            MimeType = file.ContentType,
            FileSize = file.Length,
            Status = "pending"
        };

        _context.VendorDocuments.Add(document);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Belge yuklendi.", id = document.Id });
    }

    /// <summary>
    /// Belge sil
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var document = await _context.VendorDocuments
            .FirstOrDefaultAsync(d => d.Id == id && d.VendorId == vendorId.Value);

        if (document == null)
            return NotFound(new { message = "Belge bulunamadi." });

        // Delete file from disk
        var fullPath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        // Delete from database
        _context.VendorDocuments.Remove(document);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Belge silindi." });
    }

    /// <summary>
    /// Belge indir/goruntule
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (vendorId == null) return Unauthorized(new { message = "Vendor bulunamadi." });

        var document = await _context.VendorDocuments
            .FirstOrDefaultAsync(d => d.Id == id && d.VendorId == vendorId.Value);

        if (document == null)
            return NotFound(new { message = "Belge bulunamadi." });

        var fullPath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
        if (!System.IO.File.Exists(fullPath))
            return NotFound(new { message = "Dosya bulunamadi." });

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        return File(bytes, document.MimeType ?? "application/octet-stream", document.FileName);
    }
}
