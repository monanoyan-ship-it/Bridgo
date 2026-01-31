using Bridgo.DTOs.Demand;

namespace Bridgo.Services.Interfaces;

public interface IDemandService
{
    // ============================================
    // PUBLIC DEMAND - Talep CRUD
    // ============================================

    /// <summary>
    /// Public talepleri listele (SEO sayfasi icin)
    /// </summary>
    Task<DemandSearchResultDto> GetPublicDemandsAsync(DemandFilterDto filter);

    /// <summary>
    /// Talep detayi getir (slug ile - SEO)
    /// </summary>
    Task<DemandDetailDto?> GetDemandBySlugAsync(string slug, bool incrementViewCount = false);

    /// <summary>
    /// Talep detayi getir (ID ile)
    /// </summary>
    Task<DemandDetailDto?> GetDemandByIdAsync(int id);

    /// <summary>
    /// Vendor'in kendi taleplerini listele
    /// </summary>
    Task<DemandSearchResultDto> GetVendorDemandsAsync(int vendorId, DemandFilterDto filter);

    /// <summary>
    /// Takip edilen kategorilerdeki talepleri listele (supplier icin)
    /// </summary>
    Task<DemandSearchResultDto> GetSubscribedDemandsAsync(int vendorId, DemandFilterDto filter);

    /// <summary>
    /// Yeni talep olustur
    /// </summary>
    Task<DemandDetailDto> CreateDemandAsync(int vendorId, DemandCreateDto dto, string? createdBy);

    /// <summary>
    /// Talep guncelle
    /// </summary>
    Task<bool> UpdateDemandAsync(int demandId, int vendorId, DemandUpdateDto dto, string? updatedBy);

    /// <summary>
    /// Talep sil (soft delete)
    /// </summary>
    Task<bool> DeleteDemandAsync(int demandId, int vendorId, string? deletedBy);

    /// <summary>
    /// Talep durumunu guncelle
    /// </summary>
    Task<bool> UpdateDemandStatusAsync(int demandId, int vendorId, int status, string? updatedBy);

    /// <summary>
    /// Talep yayinla (Draft -> Active)
    /// </summary>
    Task<bool> PublishDemandAsync(int demandId, int vendorId, string? updatedBy);

    /// <summary>
    /// Talep kapat
    /// </summary>
    Task<bool> CloseDemandAsync(int demandId, int vendorId, string? updatedBy);

    // ============================================
    // DEMAND MODIFICATIONS - Modifikasyonlar
    // ============================================

    /// <summary>
    /// Talebe modifikasyon ekle
    /// </summary>
    Task<DemandModificationDto> AddModificationAsync(int demandId, int vendorId, DemandModificationCreateDto dto, string? createdBy);

    /// <summary>
    /// Modifikasyon guncelle
    /// </summary>
    Task<bool> UpdateModificationAsync(int modificationId, int vendorId, DemandModificationCreateDto dto, string? updatedBy);

    /// <summary>
    /// Modifikasyon sil
    /// </summary>
    Task<bool> DeleteModificationAsync(int modificationId, int vendorId, string? deletedBy);

    // ============================================
    // DEMAND ATTACHMENTS - Ekler
    // ============================================

    /// <summary>
    /// Talebe ek yukle
    /// </summary>
    Task<DemandAttachmentDto> AddAttachmentAsync(int demandId, int vendorId, string fileName, string filePath, string? mimeType, long? fileSize, string? title, string? description, string? createdBy);

    /// <summary>
    /// Ek sil
    /// </summary>
    Task<bool> DeleteAttachmentAsync(int attachmentId, int vendorId, string? deletedBy);

    // ============================================
    // DEMAND RESPONSES - Teklif Yanitlari
    // ============================================

    /// <summary>
    /// Talebe gelen yanitlari listele (talep sahibi icin)
    /// </summary>
    Task<List<DemandResponseListDto>> GetDemandResponsesAsync(int demandId, int vendorId);

    /// <summary>
    /// Vendor'in tum taleplerine gelen yanitlari listele
    /// </summary>
    Task<List<DemandResponseListDto>> GetAllResponsesForVendorAsync(int vendorId);

    /// <summary>
    /// Yanit detayi getir
    /// </summary>
    Task<DemandResponseDetailDto?> GetResponseByIdAsync(int responseId, int vendorId);

    /// <summary>
    /// Kayitli uretici olarak teklif ver
    /// </summary>
    Task<DemandResponseDetailDto> CreateResponseAsync(int supplierVendorId, DemandResponseCreateDto dto, string? createdBy);

    /// <summary>
    /// Dis uretici (kayitsiz) olarak teklif ver
    /// </summary>
    Task<DemandResponseDetailDto> CreateExternalResponseAsync(ExternalDemandResponseCreateDto dto);

    /// <summary>
    /// Yanit durumunu guncelle (talep sahibi)
    /// </summary>
    Task<bool> UpdateResponseStatusAsync(int responseId, int demandOwnerVendorId, DemandResponseStatusUpdateDto dto, string? updatedBy);

    /// <summary>
    /// Yanit goruldu olarak isaretle
    /// </summary>
    Task<bool> MarkResponseAsViewedAsync(int responseId, int demandOwnerVendorId);

    /// <summary>
    /// Uretici kendi verdigi yanitlari listele
    /// </summary>
    Task<List<DemandResponseListDto>> GetMyResponsesAsync(int supplierVendorId);

    // ============================================
    // RESPONSE ATTACHMENTS - Yanit Ekleri
    // ============================================

    /// <summary>
    /// Yanita ek yukle
    /// </summary>
    Task<DemandResponseAttachmentDto> AddResponseAttachmentAsync(int responseId, int supplierVendorId, string fileName, string filePath, string? mimeType, long? fileSize, string? title, string? createdBy);

    /// <summary>
    /// Yanit ekini sil
    /// </summary>
    Task<bool> DeleteResponseAttachmentAsync(int attachmentId, int supplierVendorId, string? deletedBy);

    // ============================================
    // ISTATISTIKLER
    // ============================================

    /// <summary>
    /// Vendor talep istatistikleri
    /// </summary>
    Task<DemandStatsDto> GetVendorDemandStatsAsync(int vendorId);

    /// <summary>
    /// Supplier yanit istatistikleri
    /// </summary>
    Task<ResponseStatsDto> GetSupplierResponseStatsAsync(int vendorId);

    // ============================================
    // SLUG URETIMI
    // ============================================

    /// <summary>
    /// Unique slug uret
    /// </summary>
    Task<string> GenerateUniqueSlugAsync(string title);
}

// ============================================
// ISTATISTIK DTO'lari
// ============================================

public class DemandStatsDto
{
    public int TotalDemands { get; set; }
    public int ActiveDemands { get; set; }
    public int ClosedDemands { get; set; }
    public int TotalResponses { get; set; }
    public int PendingResponses { get; set; }
    public int AcceptedResponses { get; set; }
}

public class ResponseStatsDto
{
    public int TotalResponses { get; set; }
    public int PendingResponses { get; set; }
    public int ViewedResponses { get; set; }
    public int AcceptedResponses { get; set; }
    public int RejectedResponses { get; set; }
}
