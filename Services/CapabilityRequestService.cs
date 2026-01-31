using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class CapabilityRequestService : ICapabilityRequestService
{
    private readonly ApplicationDbContext _context;
    private readonly ICompanyService _companyService;
    private readonly ILogger<CapabilityRequestService> _logger;

    public CapabilityRequestService(
        ApplicationDbContext context,
        ICompanyService companyService,
        ILogger<CapabilityRequestService> logger)
    {
        _context = context;
        _companyService = companyService;
        _logger = logger;
    }

    public async Task<CapabilityRequestResultDto> CreateRequestAsync(int vendorId, int userId, CreateCapabilityRequestDto dto)
    {
        // Capability var mi?
        var capability = Capabilities.GetById(dto.CapabilityId);

        if (capability == null || !capability.IsActive)
        {
            return new CapabilityRequestResultDto
            {
                Success = false,
                Message = "Gecersiz hizmet"
            };
        }

        // Vendor zaten bu capability'ye sahip mi?
        var hasCapability = await _context.VendorCapabilityMappings
            .AnyAsync(m => m.VendorId == vendorId && m.CapabilityId == dto.CapabilityId && m.IsActive);

        if (hasCapability)
        {
            return new CapabilityRequestResultDto
            {
                Success = false,
                Message = "Bu hizmete zaten sahipsiniz"
            };
        }

        // Bekleyen talep var mi?
        var pendingRequest = await _context.CapabilityRequests
            .AnyAsync(r => r.VendorId == vendorId
                && r.CapabilityId == dto.CapabilityId
                && r.Status == CapabilityRequestStatuses.Pending.Id);

        if (pendingRequest)
        {
            return new CapabilityRequestResultDto
            {
                Success = false,
                Message = "Bu hizmet icin bekleyen bir talebiniz var"
            };
        }

        // Talep olustur
        var request = new CapabilityRequest
        {
            VendorId = vendorId,
            CapabilityId = dto.CapabilityId,
            RequestedByUserId = userId,
            Status = CapabilityRequestStatuses.Pending.Id,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.CapabilityRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Capability request created: VendorId={VendorId}, CapabilityId={CapabilityId}, RequestId={RequestId}",
            vendorId, dto.CapabilityId, request.Id);

        return new CapabilityRequestResultDto
        {
            Success = true,
            RequestId = request.Id,
            Message = "Talebiniz basariyla olusturuldu"
        };
    }

    public async Task<List<CapabilityRequestDto>> GetMyRequestsAsync(int vendorId)
    {
        var requests = await _context.CapabilityRequests
            .Where(r => r.VendorId == vendorId)
            .Include(r => r.RequestedByUser)
            .Include(r => r.ProcessedByUser)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto).ToList();
    }

    public async Task<List<AvailableCapabilityDto>> GetAvailableCapabilitiesAsync(int vendorId)
    {
        // Vendor'in sahip oldugu capability'ler
        var ownedCapabilityIds = await _context.VendorCapabilityMappings
            .Where(m => m.VendorId == vendorId && m.IsActive)
            .Select(m => m.CapabilityId)
            .ToListAsync();

        // Bekleyen talep olan capability'ler
        var pendingCapabilityIds = await _context.CapabilityRequests
            .Where(r => r.VendorId == vendorId && r.Status == CapabilityRequestStatuses.Pending.Id)
            .Select(r => r.CapabilityId)
            .ToListAsync();

        // Sahip olunmayan aktif capability'ler (TypeDefinitions'dan)
        return Capabilities.All
            .Where(c => c.IsActive && !ownedCapabilityIds.Contains(c.Id))
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new AvailableCapabilityDto
            {
                Id = c.Id,
                Name = c.Description ?? c.SystemName,
                NameResourceKey = c.NameResourceKey,
                Description = c.Description,
                Icon = c.Icon,
                HasPendingRequest = pendingCapabilityIds.Contains(c.Id)
            }).ToList();
    }

    public async Task<List<CapabilityRequestDto>> GetPendingRequestsAsync()
    {
        var requests = await _context.CapabilityRequests
            .Where(r => r.Status == CapabilityRequestStatuses.Pending.Id)
            .Include(r => r.Vendor)
            .Include(r => r.RequestedByUser)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto).ToList();
    }

    public async Task<List<CapabilityRequestDto>> GetAllRequestsAsync(int? statusId = null)
    {
        var query = _context.CapabilityRequests
            .Include(r => r.Vendor)
            .Include(r => r.RequestedByUser)
            .Include(r => r.ProcessedByUser)
            .AsQueryable();

        if (statusId.HasValue)
        {
            query = query.Where(r => r.Status == statusId.Value);
        }

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto).ToList();
    }

    public async Task<bool> ApproveRequestAsync(int requestId, int adminUserId, string? adminNotes = null)
    {
        var request = await _context.CapabilityRequests
            .Include(r => r.Vendor)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
        {
            _logger.LogWarning("Capability request not found: {RequestId}", requestId);
            return false;
        }

        if (request.Status != CapabilityRequestStatuses.Pending.Id)
        {
            _logger.LogWarning("Capability request already processed: {RequestId}", requestId);
            return false;
        }

        // Capability'yi ekle (ID ile)
        var added = await _companyService.EnsureVendorCapabilityWithDefaultRoleAsync(
            request.VendorId, request.CapabilityId);

        // Talep durumunu guncelle
        request.Status = CapabilityRequestStatuses.Approved.Id;
        request.AdminNotes = adminNotes;
        request.ProcessedByUserId = adminUserId;
        request.ProcessedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Capability request approved: RequestId={RequestId}, VendorId={VendorId}, CapabilityId={CapabilityId}",
            requestId, request.VendorId, request.CapabilityId);

        return true;
    }

    public async Task<bool> RejectRequestAsync(int requestId, int adminUserId, string? adminNotes = null)
    {
        var request = await _context.CapabilityRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
        {
            _logger.LogWarning("Capability request not found: {RequestId}", requestId);
            return false;
        }

        if (request.Status != CapabilityRequestStatuses.Pending.Id)
        {
            _logger.LogWarning("Capability request already processed: {RequestId}", requestId);
            return false;
        }

        // Talep durumunu guncelle
        request.Status = CapabilityRequestStatuses.Rejected.Id;
        request.AdminNotes = adminNotes;
        request.ProcessedByUserId = adminUserId;
        request.ProcessedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Capability request rejected: RequestId={RequestId}, VendorId={VendorId}",
            requestId, request.VendorId);

        return true;
    }

    private CapabilityRequestDto MapToDto(CapabilityRequest r)
    {
        var status = CapabilityRequestStatuses.GetById(r.Status);
        var capability = Capabilities.GetById(r.CapabilityId);
        return new CapabilityRequestDto
        {
            Id = r.Id,
            VendorId = r.VendorId,
            VendorName = r.Vendor?.CompanyName ?? "",
            CapabilityId = r.CapabilityId,
            CapabilityName = capability?.Description ?? capability?.SystemName ?? "",
            CapabilityIcon = capability?.Icon,
            RequestedByUserId = r.RequestedByUserId,
            RequestedByUserName = r.RequestedByUser != null ? $"{r.RequestedByUser.FirstName} {r.RequestedByUser.LastName}" : "",
            Status = r.Status,
            StatusName = status?.SystemName ?? "",
            StatusCssClass = status?.CssClass,
            Notes = r.Notes,
            AdminNotes = r.AdminNotes,
            ProcessedByUserId = r.ProcessedByUserId,
            ProcessedByUserName = r.ProcessedByUser != null ? $"{r.ProcessedByUser.FirstName} {r.ProcessedByUser.LastName}" : null,
            ProcessedAt = r.ProcessedAt,
            CreatedAt = r.CreatedAt
        };
    }
}
