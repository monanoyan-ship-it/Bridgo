namespace Bridgo.Services.Interfaces;

/// <summary>
/// Servis teklifi kabul sonrasi sonuc
/// </summary>
public class QuoteAcceptedResult
{
    public int? CreatedSurveyRequestId { get; set; }
    public bool AllServicesSelected { get; set; }
    public bool FinancingAvailable { get; set; }
    public decimal? TotalAmount { get; set; }
    public int? FinancingRequestId { get; set; }
}

/// <summary>
/// Finansman durumu bilgisi
/// </summary>
public class FinancingStatusDto
{
    public bool RequiresFinancing { get; set; }
    public bool AllServicesSelected { get; set; }
    public bool CanTriggerFinancing { get; set; }
    public int? ExistingFinancingRequestId { get; set; }
    public decimal ItemsTotal { get; set; }
    public decimal ServicesTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Currency { get; set; }
    public int PendingServiceRequests { get; set; }
    public int SelectedServiceRequests { get; set; }
}

/// <summary>
/// Checkout ilerleme durumu
/// </summary>
public class CheckoutProgressDto
{
    /// <summary>Mevcut checkout adimi (1-7)</summary>
    public int CurrentStep { get; set; }

    /// <summary>Adim adi</summary>
    public string StepName { get; set; } = string.Empty;

    /// <summary>Adim aciklamasi</summary>
    public string StepDescription { get; set; } = string.Empty;

    /// <summary>Adim CSS class</summary>
    public string StepCssClass { get; set; } = string.Empty;

    /// <summary>Tum adimlar ve durumlari</summary>
    public List<CheckoutStepInfo> Steps { get; set; } = new();

    /// <summary>Siparis toplami</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Para birimi</summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>Toplam servis sayisi</summary>
    public int TotalServiceRequests { get; set; }

    /// <summary>Teklif gelen servis sayisi</summary>
    public int ServiceRequestsWithQuotes { get; set; }

    /// <summary>Secilen servis sayisi</summary>
    public int SelectedServiceRequests { get; set; }
}

/// <summary>
/// Checkout adim bilgisi
/// </summary>
public class CheckoutStepInfo
{
    public int StepNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsPending { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Siparis akisi orkestrasyon servisi
/// Servisler arasi bagimliliklari ve tetiklemeleri yonetir
/// </summary>
public interface IOrderOrchestrationService
{
    /// <summary>
    /// Servis teklifi kabul edildiginde cagrilir
    /// Lojistik teklifi ise ve gozetim bekliyorsa, Survey request olusturur
    /// </summary>
    /// <param name="orderId">Siparis ID</param>
    /// <param name="serviceRequestId">Kabul edilen servis request ID</param>
    /// <param name="quoteId">Kabul edilen teklif ID</param>
    /// <returns>Islem sonucu</returns>
    Task<QuoteAcceptedResult> OnServiceQuoteAcceptedAsync(int orderId, int serviceRequestId, int quoteId);

    /// <summary>
    /// Lojistik bilgilerinden Survey request olusturur
    /// </summary>
    /// <param name="orderId">Siparis ID</param>
    /// <param name="logisticsRequestId">Lojistik request ID (bilgi kaynagi)</param>
    /// <param name="logisticsQuoteId">Secilen lojistik teklif ID</param>
    /// <returns>Olusturulan Survey request ID</returns>
    Task<int> CreateSurveyRequestFromLogisticsAsync(int orderId, int logisticsRequestId, int logisticsQuoteId);

    /// <summary>
    /// Tum servisler secildi mi kontrol eder
    /// Secildiyse AllServicesSelectedAt tarihini gunceller
    /// </summary>
    /// <param name="orderId">Siparis ID</param>
    /// <returns>Tum servisler secildiyse true</returns>
    Task<bool> CheckAllServicesSelectedAsync(int orderId);

    /// <summary>
    /// Siparis icin otomatik FinancingRequest olusturur
    /// </summary>
    /// <param name="orderId">Siparis ID</param>
    /// <returns>Olusturulan FinancingRequest ID</returns>
    Task<int?> TriggerFinancingIfNeededAsync(int orderId);

    /// <summary>
    /// Siparise finansman talebini manuel olarak baglar
    /// </summary>
    /// <param name="orderId">Siparis ID</param>
    /// <param name="buyerVendorId">Alici firma ID (yetki kontrolu)</param>
    /// <returns>Olusturulan FinancingRequest ID</returns>
    Task<int?> RequestFinancingAsync(int orderId, int buyerVendorId);

    /// <summary>
    /// Siparisin finansman durumunu getirir
    /// </summary>
    /// <param name="orderId">Siparis ID</param>
    /// <returns>Finansman durumu</returns>
    Task<FinancingStatusDto> GetFinancingStatusAsync(int orderId);

    /// <summary>
    /// Siparisin checkout ilerleme durumunu getirir
    /// </summary>
    /// <param name="orderId">Siparis ID</param>
    /// <returns>Checkout ilerleme durumu</returns>
    Task<CheckoutProgressDto> GetCheckoutProgressAsync(int orderId);

    /// <summary>
    /// Teklif geldiginde checkout adimini gunceller
    /// </summary>
    /// <param name="orderId">Siparis ID</param>
    Task UpdateCheckoutStepOnQuoteReceivedAsync(int orderId);

    /// <summary>
    /// Checkout adimini gunceller
    /// </summary>
    /// <param name="orderId">Siparis ID</param>
    /// <param name="step">Yeni adim</param>
    Task UpdateCheckoutStepAsync(int orderId, int step);
}
