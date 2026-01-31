using Bridgo.DTOs.Checkout;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Checkout islemleri - Sepetten Siparise donusum
/// </summary>
public interface ICheckoutService
{
    /// <summary>
    /// Checkout ozeti getir (sepet + hizmet secenekleri)
    /// </summary>
    Task<CheckoutSummaryDto?> GetCheckoutSummaryAsync(int vendorId, int userId);

    /// <summary>
    /// Checkout'u baslat - Order ve ServiceRequest'ler olustur
    /// </summary>
    Task<CheckoutResultDto> InitiateCheckoutAsync(int vendorId, int userId, InitiateCheckoutDto dto);

    /// <summary>
    /// Siparis icin gelen teklifleri getir
    /// </summary>
    Task<OrderQuotesDto?> GetOrderQuotesAsync(int orderId, int vendorId);

    /// <summary>
    /// Teklif sec
    /// </summary>
    Task<ServiceResult> SelectQuoteAsync(int orderId, int quoteId, int vendorId);

    /// <summary>
    /// Tum hizmetler secildi mi kontrol et
    /// </summary>
    Task<bool> AreAllServicesSelectedAsync(int orderId);

    /// <summary>
    /// Checkout'u tamamla - Odeme adimina gec
    /// </summary>
    Task<CheckoutCompleteDto> CompleteCheckoutAsync(int orderId, int vendorId);

    /// <summary>
    /// Servis saglayicilara bildirim gonder
    /// </summary>
    Task NotifyServiceProvidersAsync(int orderId, int serviceType);
}
