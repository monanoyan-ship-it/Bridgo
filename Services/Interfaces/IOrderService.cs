using Bridgo.DTOs.Order;

namespace Bridgo.Services.Interfaces;

public interface IOrderService
{
    // ============================================
    // BUYER (ALICI) OPERATIONS
    // ============================================

    /// <summary>
    /// Alicinin siparislerini listele
    /// </summary>
    Task<OrderSearchResultDto> GetBuyerOrdersAsync(int buyerVendorId, OrderFilterDto filter);

    /// <summary>
    /// Siparis detayi getir (alici icin)
    /// </summary>
    Task<OrderDetailDto?> GetOrderForBuyerAsync(int orderId, int buyerVendorId);

    /// <summary>
    /// Dogrudan satin alma ile siparis olustur
    /// </summary>
    Task<OrderDetailDto> CreateDirectOrderAsync(int buyerVendorId, CreateOrderDto dto, string? createdBy);

    /// <summary>
    /// Inquiry'den siparis olustur (teklif kabul edildiginde)
    /// </summary>
    Task<OrderDetailDto> CreateOrderFromInquiryAsync(int buyerVendorId, CreateOrderFromInquiryDto dto, string? createdBy);

    /// <summary>
    /// Demand'dan siparis olustur (talep yaniti kabul edildiginde)
    /// </summary>
    Task<OrderDetailDto> CreateOrderFromDemandAsync(int buyerVendorId, CreateOrderFromDemandDto dto, string? createdBy);

    /// <summary>
    /// Siparisi iptal et (alici)
    /// </summary>
    Task<bool> CancelOrderByBuyerAsync(int orderId, int buyerVendorId, CancelOrderDto dto, string? updatedBy);

    /// <summary>
    /// Teslim alindi onayla
    /// </summary>
    Task<bool> ConfirmDeliveryAsync(int orderId, int buyerVendorId, string? updatedBy);

    /// <summary>
    /// Sozlesmeyi onayla (tum teklifler secildikten sonra)
    /// </summary>
    Task<bool> AcceptContractAsync(int orderId, int buyerVendorId, string? updatedBy);

    /// <summary>
    /// Alici istatistikleri
    /// </summary>
    Task<BuyerOrderStatsDto> GetBuyerStatsAsync(int buyerVendorId);

    // ============================================
    // SELLER (SATICI) OPERATIONS
    // ============================================

    /// <summary>
    /// Saticinin siparislerini listele
    /// </summary>
    Task<OrderSearchResultDto> GetSellerOrdersAsync(int sellerVendorId, OrderFilterDto filter);

    /// <summary>
    /// Siparis detayi getir (satici icin)
    /// </summary>
    Task<OrderDetailDto?> GetOrderForSellerAsync(int orderId, int sellerVendorId);

    /// <summary>
    /// Siparisi onayla (satici)
    /// </summary>
    Task<bool> ConfirmOrderAsync(int orderId, int sellerVendorId, string? notes, string? updatedBy);

    /// <summary>
    /// Siparisi reddet (satici)
    /// </summary>
    Task<bool> RejectOrderAsync(int orderId, int sellerVendorId, string? reason, string? updatedBy);

    /// <summary>
    /// Siparis durumunu guncelle (satici)
    /// </summary>
    Task<bool> UpdateOrderStatusAsync(int orderId, int sellerVendorId, UpdateOrderStatusDto dto, string? updatedBy);

    /// <summary>
    /// Kargo ekle
    /// </summary>
    Task<OrderShipmentDto> CreateShipmentAsync(int orderId, int sellerVendorId, CreateShipmentDto dto, string? createdBy);

    /// <summary>
    /// Kargo guncelle
    /// </summary>
    Task<bool> UpdateShipmentAsync(int orderId, int shipmentId, int sellerVendorId, UpdateShipmentDto dto, string? updatedBy);

    /// <summary>
    /// Satici istatistikleri
    /// </summary>
    Task<SellerOrderStatsDto> GetSellerStatsAsync(int sellerVendorId);

    // ============================================
    // SERVICE PROVIDER OPERATIONS
    // ============================================

    /// <summary>
    /// Servis saglayicinin acik taleplerini listele (Lojistik/Gumruk/Sigorta/Gozetim)
    /// </summary>
    Task<List<ServiceRequestListDto>> GetOpenServiceRequestsAsync(int providerVendorId, int serviceType);

    /// <summary>
    /// Servis talebi detayi
    /// </summary>
    Task<ServiceRequestDetailDto?> GetServiceRequestDetailAsync(int requestId, int providerVendorId);

    /// <summary>
    /// Servis teklifi ver
    /// </summary>
    Task<ServiceQuoteDto> CreateServiceQuoteAsync(int providerVendorId, CreateServiceQuoteDto dto, string? createdBy);

    /// <summary>
    /// Servis teklifi guncelle
    /// </summary>
    Task<bool> UpdateServiceQuoteAsync(int quoteId, int providerVendorId, UpdateServiceQuoteDto dto, string? updatedBy);

    /// <summary>
    /// Servis teklifi geri cek
    /// </summary>
    Task<bool> WithdrawServiceQuoteAsync(int quoteId, int providerVendorId, string? updatedBy);

    // ============================================
    // BUYER - SERVICE REQUEST OPERATIONS
    // ============================================

    /// <summary>
    /// Servis talebi olustur (Buyer siparis icin lojistik/gumruk/sigorta/gozetim talep eder)
    /// </summary>
    Task<ServiceRequestDetailDto> CreateServiceRequestAsync(int orderId, int buyerVendorId, CreateServiceRequestDto dto, string? createdBy);

    /// <summary>
    /// Servis teklifini kabul et
    /// </summary>
    Task<bool> AcceptServiceQuoteAsync(int quoteId, int buyerVendorId, string? updatedBy);

    /// <summary>
    /// Servis teklifini reddet
    /// </summary>
    Task<bool> RejectServiceQuoteAsync(int quoteId, int buyerVendorId, string? reason, string? updatedBy);

    // ============================================
    // TASK OPERATIONS
    // ============================================

    /// <summary>
    /// Katilimcinin gorevlerini listele
    /// </summary>
    Task<List<OrderTaskDto>> GetParticipantTasksAsync(int orderId, int vendorId);

    /// <summary>
    /// Gorevi baslat
    /// </summary>
    Task<bool> StartTaskAsync(int taskId, int vendorId, string? updatedBy);

    /// <summary>
    /// Gorevi tamamla
    /// </summary>
    Task<bool> CompleteTaskAsync(int taskId, int vendorId, string? notes, string? updatedBy);

    // ============================================
    // INVESTMENT OPERATIONS
    // ============================================

    /// <summary>
    /// Yatirim firsatlarini listele
    /// </summary>
    Task<List<InvestmentOpportunityDto>> GetInvestmentOpportunitiesAsync(int investorVendorId);

    /// <summary>
    /// Yatirim teklifi ver
    /// </summary>
    Task<OrderInvestmentDto> CreateInvestmentAsync(int investorVendorId, CreateInvestmentDto dto, string? createdBy);

    /// <summary>
    /// Yatirim teklifi kabul et (Buyer)
    /// </summary>
    Task<bool> AcceptInvestmentAsync(int investmentId, int buyerVendorId, string? updatedBy);

    /// <summary>
    /// Yatirim teklifi reddet (Buyer)
    /// </summary>
    Task<bool> RejectInvestmentAsync(int investmentId, int buyerVendorId, string? reason, string? updatedBy);

    // ============================================
    // COMMON OPERATIONS
    // ============================================

    /// <summary>
    /// Siparis numarasi olustur
    /// </summary>
    Task<string> GenerateOrderNumberAsync();

    /// <summary>
    /// Kargo numarasi olustur
    /// </summary>
    Task<string> GenerateShipmentNumberAsync(int orderId);

    /// <summary>
    /// Siparis durum gecmisi
    /// </summary>
    Task<List<OrderStatusHistoryDto>> GetOrderStatusHistoryAsync(int orderId);

    /// <summary>
    /// Kargo takip
    /// </summary>
    Task<OrderShipmentDto?> GetShipmentTrackingAsync(int shipmentId);
}
