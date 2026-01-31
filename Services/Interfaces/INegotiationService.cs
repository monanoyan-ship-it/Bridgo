using Bridgo.DTOs.Demand;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Pazarlik (Negotiation) servisi
/// DemandResponse uzerinde cift yonlu pazarlik islemleri
/// </summary>
public interface INegotiationService
{
    // ============================================
    // PAZARLIK BASLATMA
    // ============================================

    /// <summary>
    /// Buyer pazarlik baslatir (ilk karsi teklif)
    /// </summary>
    /// <param name="demandResponseId">Teklif ID</param>
    /// <param name="buyerVendorId">Buyer Vendor ID</param>
    /// <param name="dto">Karsi teklif detaylari</param>
    /// <param name="createdBy">Olusturan kullanici</param>
    /// <returns>Olusturulan tur</returns>
    Task<NegotiationRoundDetailDto> StartNegotiationAsync(int demandResponseId, int buyerVendorId, CounterOfferCreateDto dto, string? createdBy);

    // ============================================
    // KARSI TEKLIF
    // ============================================

    /// <summary>
    /// Karsi teklif ver (her iki taraf icin)
    /// </summary>
    /// <param name="vendorId">Teklif veren Vendor ID</param>
    /// <param name="dto">Karsi teklif detaylari</param>
    /// <param name="createdBy">Olusturan kullanici</param>
    /// <returns>Olusturulan tur</returns>
    Task<NegotiationRoundDetailDto> CreateCounterOfferAsync(int vendorId, CounterOfferCreateDto dto, string? createdBy);

    // ============================================
    // YANIT ISLEMLERI
    // ============================================

    /// <summary>
    /// Karsi teklifi kabul et
    /// </summary>
    /// <param name="roundId">Kabul edilecek tur ID</param>
    /// <param name="vendorId">Kabul eden Vendor ID</param>
    /// <param name="updatedBy">Guncelleyen kullanici</param>
    /// <returns>Basarili mi</returns>
    Task<bool> AcceptCounterOfferAsync(int roundId, int vendorId, string? updatedBy);

    /// <summary>
    /// Karsi teklifi reddet
    /// </summary>
    /// <param name="roundId">Reddedilecek tur ID</param>
    /// <param name="vendorId">Reddeden Vendor ID</param>
    /// <param name="reason">Red nedeni</param>
    /// <param name="updatedBy">Guncelleyen kullanici</param>
    /// <returns>Basarili mi</returns>
    Task<bool> RejectCounterOfferAsync(int roundId, int vendorId, string? reason, string? updatedBy);

    /// <summary>
    /// Pazarliktan geri cekil
    /// </summary>
    /// <param name="demandResponseId">Teklif ID</param>
    /// <param name="vendorId">Geri cekilen Vendor ID</param>
    /// <param name="updatedBy">Guncelleyen kullanici</param>
    /// <returns>Basarili mi</returns>
    Task<bool> WithdrawFromNegotiationAsync(int demandResponseId, int vendorId, string? updatedBy);

    // ============================================
    // SORGULAMA
    // ============================================

    /// <summary>
    /// Pazarlik gecmisini getir
    /// </summary>
    /// <param name="demandResponseId">Teklif ID</param>
    /// <param name="vendorId">Sorgulayan Vendor ID (yetki kontrolu)</param>
    /// <returns>Pazarlik gecmisi</returns>
    Task<NegotiationHistoryDto?> GetNegotiationHistoryAsync(int demandResponseId, int vendorId);

    /// <summary>
    /// Pazarlik ozetini getir
    /// </summary>
    /// <param name="demandResponseId">Teklif ID</param>
    /// <param name="vendorId">Sorgulayan Vendor ID</param>
    /// <returns>Ozet bilgisi</returns>
    Task<NegotiationSummaryDto?> GetNegotiationSummaryAsync(int demandResponseId, int vendorId);

    /// <summary>
    /// Aktif pazarliklarimi listele (sira bende olanlar)
    /// </summary>
    /// <param name="vendorId">Vendor ID</param>
    /// <returns>Aktif pazarlik listesi</returns>
    Task<List<ActiveNegotiationDto>> GetMyActiveNegotiationsAsync(int vendorId);

    /// <summary>
    /// Bekleyen pazarliklarimi listele (sira karsi tarafta)
    /// </summary>
    /// <param name="vendorId">Vendor ID</param>
    /// <returns>Bekleyen pazarlik listesi</returns>
    Task<List<ActiveNegotiationDto>> GetPendingNegotiationsAsync(int vendorId);

    // ============================================
    // TIMEOUT ISLEMLERI
    // ============================================

    /// <summary>
    /// Suresi dolmus pazarliklari isle
    /// </summary>
    /// <returns>Islenen pazarlik sayisi</returns>
    Task<int> ProcessExpiredNegotiationsAsync();

    /// <summary>
    /// Belirli bir pazarligin suresini kontrol et
    /// </summary>
    /// <param name="demandResponseId">Teklif ID</param>
    /// <returns>Suresi dolmus mu</returns>
    Task<bool> CheckAndExpireNegotiationAsync(int demandResponseId);
}
