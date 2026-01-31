using Bridgo.DTOs.Evrim;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Evrim Gumruk API servisi interface
/// </summary>
public interface IEvrimApiService
{
    // ================================
    // AUTHENTICATION
    // ================================

    /// <summary>
    /// Evrim API'dan JWT token alir
    /// Token cache'lenir, expire'a yakin yenilenir
    /// </summary>
    Task<string?> GetTokenAsync();

    /// <summary>
    /// API baglantisinini test eder
    /// </summary>
    Task<ConnectionTestResultDto> TestConnectionAsync();

    // ================================
    // BEYANNAME ISLEMLERI
    // ================================

    /// <summary>
    /// Ihracat beyannamesi olusturur
    /// </summary>
    Task<EvrimDeclarationCreateResponse> CreateExportDeclarationAsync(int orderId, int vendorId, int userId);

    /// <summary>
    /// Ithalat beyannamesi olusturur
    /// </summary>
    Task<EvrimDeclarationCreateResponse> CreateImportDeclarationAsync(int orderId, int vendorId, int userId);

    /// <summary>
    /// ETGB (Mikro Ihracat) beyannamesi olusturur
    /// </summary>
    Task<EvrimDeclarationCreateResponse> CreateETGBDeclarationAsync(int orderId, int vendorId, int userId);

    /// <summary>
    /// Beyanname durumunu Evrim'den sorgular
    /// </summary>
    Task<EvrimDeclarationStatusResponse> GetDeclarationStatusAsync(string dosyaNo, string dosyaTipi);

    /// <summary>
    /// Belirli bir beyannamenin durumunu senkronize eder
    /// </summary>
    Task<bool> SyncDeclarationStatusAsync(int declarationId);

    /// <summary>
    /// Aktif durumdaki tum beyannamelerin durumunu senkronize eder
    /// Background job tarafindan cagirilir
    /// </summary>
    Task SyncAllPendingDeclarationsAsync();

    // ================================
    // INTERNAL CRUD (UI icin)
    // ================================

    /// <summary>
    /// Siparise ait beyannameleri listeler
    /// </summary>
    Task<List<CustomsDeclarationListDto>> GetDeclarationsByOrderAsync(int orderId, int vendorId);

    /// <summary>
    /// Beyanname detayini getirir
    /// </summary>
    Task<CustomsDeclarationDetailDto?> GetDeclarationDetailAsync(int declarationId, int vendorId);

    // ================================
    // BELGE ISLEMLERI
    // ================================

    /// <summary>
    /// Beyanname icin belge yukler
    /// </summary>
    Task<EvrimDocumentUploadResponse> UploadDocumentAsync(UploadCustomsDocumentDto request, int vendorId);

    /// <summary>
    /// Beyanname belgelerini listeler
    /// </summary>
    Task<List<EvrimDocumentListItem>> GetDocumentListAsync(int declarationId, int vendorId);

    /// <summary>
    /// Belge indirir
    /// </summary>
    Task<EvrimDocumentDownloadResponse> DownloadDocumentAsync(string guid, int vendorId);
}
