using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Evrak olusturma ve imza yonetimi servisi implementasyonu
/// </summary>
public class DocumentGenerationService : IDocumentGenerationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DocumentGenerationService> _logger;

    public DocumentGenerationService(
        ApplicationDbContext context,
        ILogger<DocumentGenerationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =======================================
    // Evrak Olusturma
    // =======================================

    public async Task<List<GeneratedDocument>> GenerateDocumentsForTransactionAsync(
        int transactionTypeId,
        int orderId,
        int vendorId,
        int? serviceRequestId = null)
    {
        // Islem tipi icin gerekli evrak eslestirmelerini al
        var mappings = await GetRequiredDocumentsForTransactionAsync(transactionTypeId, null);
        var documents = new List<GeneratedDocument>();

        foreach (var mapping in mappings.OrderBy(m => m.GenerationOrder))
        {
            var doc = await GenerateDocumentAsync(
                mapping.DocumentTypeId,
                transactionTypeId,
                vendorId,
                orderId,
                serviceRequestId);

            documents.Add(doc);
        }

        return documents;
    }

    public async Task<GeneratedDocument> GenerateDocumentAsync(
        int documentTypeId,
        int transactionTypeId,
        int vendorId,
        int? orderId = null,
        int? serviceRequestId = null,
        int? financingRequestId = null,
        int? counterpartyVendorId = null,
        Dictionary<string, object>? metadata = null)
    {
        // Evrak numarasi olustur
        var documentNumber = await GenerateDocumentNumberAsync(documentTypeId);

        // Sablon bilgilerini al
        var template = await GetActiveTemplateAsync(documentTypeId);
        var requiredSignatures = template?.SignatureCount ?? 1;

        var document = new GeneratedDocument
        {
            DocumentTypeId = documentTypeId,
            TransactionTypeId = transactionTypeId,
            DocumentNumber = documentNumber,
            Title = GetDocumentTitle(documentTypeId),
            Status = DocumentStatuses.Draft.Id,
            OrderId = orderId,
            OrderServiceRequestId = serviceRequestId,
            FinancingRequestId = financingRequestId,
            CreatedByVendorId = vendorId,
            CounterpartyVendorId = counterpartyVendorId,
            RequiredSignatures = requiredSignatures,
            CompletedSignatures = 0,
            MetadataJson = metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : null
        };

        _context.GeneratedDocuments.Add(document);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Evrak olusturuldu: {DocumentNumber}, Tip: {DocumentType}",
            documentNumber, documentTypeId);

        return document;
    }

    private string GetDocumentTitle(int documentTypeId)
    {
        var docType = DocumentTypes.GetById(documentTypeId);
        return docType?.NameResourceKey ?? "Evrak";
    }

    // =======================================
    // Evrak Sorgulama
    // =======================================

    public async Task<List<GeneratedDocument>> GetOrderDocumentsAsync(int orderId)
    {
        return await _context.GeneratedDocuments
            .Include(d => d.CreatedByVendor)
            .Include(d => d.CounterpartyVendor)
            .Include(d => d.Signatures)
            .Where(d => d.OrderId == orderId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<GeneratedDocument>> GetServiceRequestDocumentsAsync(int serviceRequestId)
    {
        return await _context.GeneratedDocuments
            .Include(d => d.CreatedByVendor)
            .Include(d => d.CounterpartyVendor)
            .Include(d => d.Signatures)
            .Where(d => d.OrderServiceRequestId == serviceRequestId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<GeneratedDocument>> GetFinancingDocumentsAsync(int financingRequestId)
    {
        return await _context.GeneratedDocuments
            .Include(d => d.CreatedByVendor)
            .Include(d => d.CounterpartyVendor)
            .Include(d => d.Signatures)
            .Where(d => d.FinancingRequestId == financingRequestId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<GeneratedDocument?> GetDocumentByIdAsync(int documentId)
    {
        return await _context.GeneratedDocuments
            .Include(d => d.Order)
            .Include(d => d.ServiceRequest)
            .Include(d => d.FinancingRequest)
            .Include(d => d.CreatedByVendor)
            .Include(d => d.CounterpartyVendor)
            .Include(d => d.Signatures)
                .ThenInclude(s => s.SignerVendor)
            .Include(d => d.Signatures)
                .ThenInclude(s => s.SignerUser)
            .FirstOrDefaultAsync(d => d.Id == documentId);
    }

    public async Task<GeneratedDocument?> GetDocumentByNumberAsync(string documentNumber)
    {
        return await _context.GeneratedDocuments
            .Include(d => d.CreatedByVendor)
            .Include(d => d.CounterpartyVendor)
            .Include(d => d.Signatures)
            .FirstOrDefaultAsync(d => d.DocumentNumber == documentNumber);
    }

    public async Task<List<GeneratedDocument>> GetVendorDocumentsAsync(int vendorId, int? documentTypeId = null, int? status = null)
    {
        var query = _context.GeneratedDocuments
            .Include(d => d.Order)
            .Include(d => d.CreatedByVendor)
            .Include(d => d.CounterpartyVendor)
            .Include(d => d.Signatures)
            .Where(d => d.CreatedByVendorId == vendorId || d.CounterpartyVendorId == vendorId);

        if (documentTypeId.HasValue)
            query = query.Where(d => d.DocumentTypeId == documentTypeId.Value);

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    // =======================================
    // Evrak Numaralandirma
    // =======================================

    public async Task<string> GenerateDocumentNumberAsync(int documentTypeId)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = DocumentTypes.GetPrefix(documentTypeId);

        // Sequence kaydini al veya olustur
        var sequence = await _context.DocumentNumberSequences
            .FirstOrDefaultAsync(s => s.DocumentTypeId == documentTypeId && s.Year == year);

        if (sequence == null)
        {
            sequence = new DocumentNumberSequence
            {
                DocumentTypeId = documentTypeId,
                Year = year,
                LastNumber = 0,
                Prefix = prefix
            };
            _context.DocumentNumberSequences.Add(sequence);
        }

        // Numarayi artir
        sequence.LastNumber++;
        await _context.SaveChangesAsync();

        // Format: SZL-2024-000001
        return $"{prefix}-{year}-{sequence.LastNumber:D6}";
    }

    // =======================================
    // Imza Islemleri
    // =======================================

    public async Task<DocumentSignature> SignDocumentAsync(
        int documentId,
        int vendorId,
        int userId,
        int signatureType,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var document = await _context.GeneratedDocuments
            .Include(d => d.Signatures)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null)
            throw new InvalidOperationException("Evrak bulunamadi.");

        // Bu vendor zaten imzalamis mi kontrol et
        if (document.Signatures.Any(s => s.SignerVendorId == vendorId && !s.IsDeleted))
            throw new InvalidOperationException("Bu evrak zaten imzalanmis.");

        // Benzersiz dogrulama kodu olustur
        var verificationCode = Guid.NewGuid().ToString("N").ToUpperInvariant();

        var signature = new DocumentSignature
        {
            GeneratedDocumentId = documentId,
            SignerVendorId = vendorId,
            SignerUserId = userId,
            SignatureType = signatureType,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            VerificationCode = verificationCode,
            IsVerified = true, // Elektronik onay icin otomatik dogrula
            VerifiedAt = DateTime.UtcNow,
            SignedAt = DateTime.UtcNow
        };

        _context.DocumentSignatures.Add(signature);

        // Evrak durumunu guncelle
        document.CompletedSignatures++;

        var isFullySigned = document.CompletedSignatures >= document.RequiredSignatures;

        if (isFullySigned)
        {
            document.Status = DocumentStatuses.Signed.Id;
        }
        else if (document.CompletedSignatures > 0)
        {
            document.Status = DocumentStatuses.PartiallySigned.Id;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Evrak imzalandi: {DocumentNumber}, Vendor: {VendorId}, User: {UserId}",
            document.DocumentNumber, vendorId, userId);

        // Evrak tamamen imzalandiysa ilgili servis talebini kilitle
        if (isFullySigned)
        {
            await LockServiceRequestAfterSignatureAsync(documentId);
        }

        return signature;
    }

    public async Task<bool> VerifySignatureAsync(string verificationCode)
    {
        var signature = await _context.DocumentSignatures
            .FirstOrDefaultAsync(s => s.VerificationCode == verificationCode);

        return signature != null && signature.IsVerified;
    }

    public async Task<DocumentSignatureStatusDto> GetDocumentSignatureStatusAsync(int documentId)
    {
        var document = await _context.GeneratedDocuments
            .Include(d => d.Signatures)
                .ThenInclude(s => s.SignerVendor)
            .Include(d => d.Signatures)
                .ThenInclude(s => s.SignerUser)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null)
            throw new InvalidOperationException("Evrak bulunamadi.");

        var statusType = DocumentStatuses.GetById(document.Status);

        return new DocumentSignatureStatusDto
        {
            DocumentId = document.Id,
            DocumentNumber = document.DocumentNumber,
            Status = document.Status,
            StatusText = statusType?.NameResourceKey ?? "Bilinmiyor",
            RequiredSignatures = document.RequiredSignatures,
            CompletedSignatures = document.CompletedSignatures,
            IsFullySigned = document.CompletedSignatures >= document.RequiredSignatures,
            Signatures = document.Signatures
                .Where(s => !s.IsDeleted)
                .Select(s => new SignatureInfoDto
                {
                    SignatureId = s.Id,
                    VendorId = s.SignerVendorId,
                    VendorName = s.SignerVendor?.CompanyName ?? "",
                    UserId = s.SignerUserId,
                    UserName = s.SignerUser != null ? $"{s.SignerUser.FirstName} {s.SignerUser.LastName}" : "",
                    SignatureType = s.SignatureType,
                    SignatureTypeText = SignatureTypes.GetById(s.SignatureType)?.NameResourceKey ?? "",
                    SignedAt = s.SignedAt,
                    IsVerified = s.IsVerified,
                    VerificationCode = s.VerificationCode
                })
                .OrderBy(s => s.SignedAt)
                .ToList()
        };
    }

    public async Task<List<GeneratedDocument>> GetPendingSignatureDocumentsAsync(int vendorId)
    {
        // Vendor'in imzalamasi gereken ama henuz imzalamadigi evraklar
        var pendingStatusId = DocumentStatuses.NeedsSignature.Id;

        return await _context.GeneratedDocuments
            .Include(d => d.Order)
            .Include(d => d.CreatedByVendor)
            .Include(d => d.CounterpartyVendor)
            .Include(d => d.Signatures)
            .Where(d => d.Status == pendingStatusId)
            .Where(d => d.CreatedByVendorId == vendorId || d.CounterpartyVendorId == vendorId)
            .Where(d => !d.Signatures.Any(s => s.SignerVendorId == vendorId && !s.IsDeleted))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    // =======================================
    // Sablon Yonetimi
    // =======================================

    public async Task<DocumentTemplate?> GetActiveTemplateAsync(int documentTypeId)
    {
        return await _context.DocumentTemplates
            .Where(t => t.DocumentTypeId == documentTypeId && t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .FirstOrDefaultAsync();
    }

    public async Task<List<DocumentTemplate>> GetActiveTemplatesAsync()
    {
        return await _context.DocumentTemplates
            .Where(t => t.IsActive)
            .OrderBy(t => t.DocumentTypeId)
            .ThenBy(t => t.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<TransactionDocumentMapping>> GetRequiredDocumentsForTransactionAsync(int transactionTypeId, int? capabilityId = null)
    {
        var query = _context.TransactionDocumentMappings
            .Where(m => m.TransactionTypeId == transactionTypeId && m.IsActive);

        if (capabilityId.HasValue)
        {
            query = query.Where(m => m.CapabilityId == null || m.CapabilityId == capabilityId.Value);
        }

        return await query
            .OrderBy(m => m.GenerationOrder)
            .ToListAsync();
    }

    // =======================================
    // Kilitleme Islemleri
    // =======================================

    public async Task LockServiceRequestAfterSignatureAsync(int documentId)
    {
        var document = await _context.GeneratedDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null) return;

        // Evrak bir servis talebine aitse onu kilitle
        if (document.OrderServiceRequestId.HasValue)
        {
            var serviceRequest = await _context.OrderServiceRequests
                .FirstOrDefaultAsync(r => r.Id == document.OrderServiceRequestId.Value);

            if (serviceRequest != null && !serviceRequest.IsLocked)
            {
                serviceRequest.IsLocked = true;
                serviceRequest.LockedByDocumentId = documentId;
                serviceRequest.LockedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Servis talebi kilitlendi: {ServiceRequestId}, Evrak: {DocumentId}",
                    serviceRequest.Id, documentId);
            }
        }
    }

    public async Task<bool> IsServiceRequestLockedAsync(int serviceRequestId)
    {
        var serviceRequest = await _context.OrderServiceRequests
            .FirstOrDefaultAsync(r => r.Id == serviceRequestId);

        return serviceRequest?.IsLocked ?? false;
    }
}
