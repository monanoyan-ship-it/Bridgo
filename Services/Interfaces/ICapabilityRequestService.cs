namespace Bridgo.Services.Interfaces;

public interface ICapabilityRequestService
{
    /// <summary>
    /// Yeni capability talebi olustur
    /// </summary>
    Task<CapabilityRequestResultDto> CreateRequestAsync(int vendorId, int userId, CreateCapabilityRequestDto dto);

    /// <summary>
    /// Kullanicinin taleplerini getir
    /// </summary>
    Task<List<CapabilityRequestDto>> GetMyRequestsAsync(int vendorId);

    /// <summary>
    /// Vendor'in sahip olmadigi capability'leri getir (talep edilebilir)
    /// </summary>
    Task<List<AvailableCapabilityDto>> GetAvailableCapabilitiesAsync(int vendorId);

    /// <summary>
    /// Bekleyen talepleri getir (admin)
    /// </summary>
    Task<List<CapabilityRequestDto>> GetPendingRequestsAsync();

    /// <summary>
    /// Tum talepleri getir (admin)
    /// </summary>
    Task<List<CapabilityRequestDto>> GetAllRequestsAsync(int? statusId = null);

    /// <summary>
    /// Talebi onayla (admin)
    /// </summary>
    Task<bool> ApproveRequestAsync(int requestId, int adminUserId, string? adminNotes = null);

    /// <summary>
    /// Talebi reddet (admin)
    /// </summary>
    Task<bool> RejectRequestAsync(int requestId, int adminUserId, string? adminNotes = null);
}

// DTOs
public class CapabilityRequestDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int CapabilityId { get; set; }
    public string CapabilityName { get; set; } = string.Empty;
    public string? CapabilityIcon { get; set; }
    public int RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? StatusCssClass { get; set; }
    public string? Notes { get; set; }
    public string? AdminNotes { get; set; }
    public int? ProcessedByUserId { get; set; }
    public string? ProcessedByUserName { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCapabilityRequestDto
{
    public int CapabilityId { get; set; }
    public string? Notes { get; set; }
}

public class CapabilityRequestResultDto
{
    public bool Success { get; set; }
    public int? RequestId { get; set; }
    public string? Message { get; set; }
}

public class AvailableCapabilityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? NameResourceKey { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool HasPendingRequest { get; set; }
}
