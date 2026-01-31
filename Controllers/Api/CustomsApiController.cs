using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bridgo.DTOs.Evrim;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Gumruk beyanname islemleri API controller
/// Evrim API entegrasyonu
/// </summary>
[Route("api/customs")]
[ApiController]
[Authorize]
public class CustomsApiController : ControllerBase
{
    private readonly IEvrimApiService _evrimApiService;
    private readonly ILogger<CustomsApiController> _logger;

    public CustomsApiController(
        IEvrimApiService evrimApiService,
        ILogger<CustomsApiController> logger)
    {
        _evrimApiService = evrimApiService;
        _logger = logger;
    }

    private int GetCurrentVendorId() =>
        int.TryParse(User.FindFirst("VendorId")?.Value, out var id) ? id : 0;

    private int GetCurrentUserId() =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    // ================================
    // BAGLANTI TESTI
    // ================================

    /// <summary>
    /// Evrim API baglantisini test eder
    /// GET /api/customs/test-connection
    /// </summary>
    [HttpGet("test-connection")]
    public async Task<IActionResult> TestConnection()
    {
        var result = await _evrimApiService.TestConnectionAsync();
        return Ok(result);
    }

    // ================================
    // BEYANNAME LISTELEME
    // ================================

    /// <summary>
    /// Siparise ait beyannameleri listeler
    /// GET /api/customs/order/{orderId}
    /// </summary>
    [HttpGet("order/{orderId:int}")]
    public async Task<IActionResult> GetByOrder(int orderId)
    {
        var vendorId = GetCurrentVendorId();
        if (vendorId == 0)
            return Unauthorized();

        var declarations = await _evrimApiService.GetDeclarationsByOrderAsync(orderId, vendorId);
        return Ok(declarations);
    }

    /// <summary>
    /// Beyanname detayini getirir
    /// GET /api/customs/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vendorId = GetCurrentVendorId();
        if (vendorId == 0)
            return Unauthorized();

        var declaration = await _evrimApiService.GetDeclarationDetailAsync(id, vendorId);
        if (declaration == null)
            return NotFound(new { message = "Beyanname bulunamadi" });

        return Ok(declaration);
    }

    // ================================
    // BEYANNAME OLUSTURMA
    // ================================

    /// <summary>
    /// Ihracat beyannamesi olusturur
    /// POST /api/customs/export
    /// Body: { orderId: int }
    /// </summary>
    [HttpPost("export")]
    public async Task<IActionResult> CreateExport([FromBody] CreateDeclarationRequest request)
    {
        var vendorId = GetCurrentVendorId();
        var userId = GetCurrentUserId();
        if (vendorId == 0 || userId == 0)
            return Unauthorized();

        var result = await _evrimApiService.CreateExportDeclarationAsync(request.OrderId, vendorId, userId);

        if (result.Basarili)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Ithalat beyannamesi olusturur
    /// POST /api/customs/import
    /// Body: { orderId: int }
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> CreateImport([FromBody] CreateDeclarationRequest request)
    {
        var vendorId = GetCurrentVendorId();
        var userId = GetCurrentUserId();
        if (vendorId == 0 || userId == 0)
            return Unauthorized();

        var result = await _evrimApiService.CreateImportDeclarationAsync(request.OrderId, vendorId, userId);

        if (result.Basarili)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// ETGB (Mikro Ihracat) beyannamesi olusturur
    /// POST /api/customs/etgb
    /// Body: { orderId: int }
    /// </summary>
    [HttpPost("etgb")]
    public async Task<IActionResult> CreateETGB([FromBody] CreateDeclarationRequest request)
    {
        var vendorId = GetCurrentVendorId();
        var userId = GetCurrentUserId();
        if (vendorId == 0 || userId == 0)
            return Unauthorized();

        var result = await _evrimApiService.CreateETGBDeclarationAsync(request.OrderId, vendorId, userId);

        if (result.Basarili)
            return Ok(result);

        return BadRequest(result);
    }

    // ================================
    // DURUM SENKRONIZASYONU
    // ================================

    /// <summary>
    /// Beyanname durumunu Evrim'den senkronize eder
    /// POST /api/customs/{id}/sync
    /// </summary>
    [HttpPost("{id:int}/sync")]
    public async Task<IActionResult> SyncStatus(int id)
    {
        var vendorId = GetCurrentVendorId();
        if (vendorId == 0)
            return Unauthorized();

        // Oncelikle beyanname vendor yetkisini kontrol et
        var declaration = await _evrimApiService.GetDeclarationDetailAsync(id, vendorId);
        if (declaration == null)
            return NotFound(new { message = "Beyanname bulunamadi" });

        var success = await _evrimApiService.SyncDeclarationStatusAsync(id);

        if (success)
        {
            // Guncellenmis halini don
            var updated = await _evrimApiService.GetDeclarationDetailAsync(id, vendorId);
            return Ok(updated);
        }

        return BadRequest(new { message = "Senkronizasyon basarisiz" });
    }

    // ================================
    // BELGE ISLEMLERI
    // ================================

    /// <summary>
    /// Beyanname belgelerini listeler
    /// GET /api/customs/{id}/documents
    /// </summary>
    [HttpGet("{id:int}/documents")]
    public async Task<IActionResult> GetDocuments(int id)
    {
        var vendorId = GetCurrentVendorId();
        if (vendorId == 0)
            return Unauthorized();

        var documents = await _evrimApiService.GetDocumentListAsync(id, vendorId);
        return Ok(documents);
    }

    /// <summary>
    /// Beyanname icin belge yukler
    /// POST /api/customs/{id}/documents
    /// </summary>
    [HttpPost("{id:int}/documents")]
    public async Task<IActionResult> UploadDocument(int id, [FromBody] UploadDocumentRequest request)
    {
        var vendorId = GetCurrentVendorId();
        if (vendorId == 0)
            return Unauthorized();

        var dto = new UploadCustomsDocumentDto
        {
            DeclarationId = id,
            DocumentTypeCode = request.DocumentTypeCode,
            DocumentNumber = request.DocumentNumber,
            DocumentDate = request.DocumentDate,
            FileName = request.FileName,
            FileContent = request.FileContent,
            MimeType = request.MimeType ?? "application/pdf",
            Description = request.Description
        };

        var result = await _evrimApiService.UploadDocumentAsync(dto, vendorId);

        if (result.Basarili)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Belge indirir
    /// GET /api/customs/documents/{guid}/download
    /// </summary>
    [HttpGet("documents/{guid}/download")]
    public async Task<IActionResult> DownloadDocument(string guid)
    {
        var vendorId = GetCurrentVendorId();
        if (vendorId == 0)
            return Unauthorized();

        var result = await _evrimApiService.DownloadDocumentAsync(guid, vendorId);

        if (result.Basarili && result.DosyaIcerik != null)
        {
            return File(result.DosyaIcerik, result.MimeType ?? "application/octet-stream", result.DosyaAdi);
        }

        return NotFound(new { message = result.HataMesaji ?? "Belge bulunamadi" });
    }

    // ================================
    // TYPE ENDPOINTS
    // ================================

    /// <summary>
    /// Beyanname tiplerini listeler
    /// GET /api/customs/types
    /// </summary>
    [HttpGet("types")]
    [AllowAnonymous]
    public IActionResult GetDeclarationTypes()
    {
        var types = CustomsDeclarationTypes.All.Select(t => new
        {
            id = t.Id,
            name = t.SystemName,
            description = t.Description,
            icon = t.Icon,
            cssClass = t.CssClass
        });
        return Ok(types);
    }

    /// <summary>
    /// Beyanname durumlarini listeler
    /// GET /api/customs/statuses
    /// </summary>
    [HttpGet("statuses")]
    [AllowAnonymous]
    public IActionResult GetDeclarationStatuses()
    {
        var statuses = CustomsDeclarationStatuses.All.Select(s => new
        {
            id = s.Id,
            name = s.SystemName,
            description = s.Description,
            icon = s.Icon,
            cssClass = s.CssClass
        });
        return Ok(statuses);
    }

    /// <summary>
    /// Belge tiplerini listeler
    /// GET /api/customs/document-types
    /// </summary>
    [HttpGet("document-types")]
    [AllowAnonymous]
    public IActionResult GetDocumentTypes()
    {
        var documentTypes = new[]
        {
            new { code = EvrimDocumentTypes.Invoice, name = "Ticari Fatura" },
            new { code = EvrimDocumentTypes.PackingList, name = "Paket Listesi" },
            new { code = EvrimDocumentTypes.BillOfLading, name = "Konşimento (Deniz)" },
            new { code = EvrimDocumentTypes.AirWaybill, name = "Havayolu Konşimentosu" },
            new { code = EvrimDocumentTypes.CMR, name = "CMR (Karayolu)" },
            new { code = EvrimDocumentTypes.CertificateOfOrigin, name = "Menşe Şahadetnamesi" },
            new { code = EvrimDocumentTypes.EUR1, name = "EUR.1 Dolaşım Belgesi" },
            new { code = EvrimDocumentTypes.ATR, name = "A.TR Dolaşım Belgesi" },
            new { code = EvrimDocumentTypes.InsurancePolicy, name = "Sigorta Poliçesi" },
            new { code = EvrimDocumentTypes.ProformaInvoice, name = "Proforma Fatura" },
            new { code = EvrimDocumentTypes.HealthCertificate, name = "Sağlık Sertifikası" },
            new { code = EvrimDocumentTypes.QualityCertificate, name = "Kalite Sertifikası" }
        };
        return Ok(documentTypes);
    }
}

// ================================
// REQUEST DTOs
// ================================

public class CreateDeclarationRequest
{
    public int OrderId { get; set; }
}

public class UploadDocumentRequest
{
    public string DocumentTypeCode { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public DateTime? DocumentDate { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileContent { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public string? Description { get; set; }
}
