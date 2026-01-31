using Bridgo.DTOs.Dashboard;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Capability bazli dashboard verileri servisi
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Firma genel bilgileri dashboard'u (capability parametresi olmadan)
    /// </summary>
    Task<CompanyDashboardDto> GetCompanyDashboardAsync(int vendorId);

    /// <summary>
    /// Satici dashboard verisi (Capability ID: 2)
    /// </summary>
    Task<SellerDashboardDto> GetSellerDashboardAsync(int vendorId);

    /// <summary>
    /// Alici dashboard verisi (Capability ID: 3)
    /// </summary>
    Task<BuyerDashboardDto> GetBuyerDashboardAsync(int vendorId);

    /// <summary>
    /// Tasimaci dashboard verisi (Capability ID: 4)
    /// </summary>
    Task<CarrierDashboardDto> GetCarrierDashboardAsync(int vendorId);

    /// <summary>
    /// Sigorta dashboard verisi (Capability ID: 5)
    /// </summary>
    Task<InsuranceDashboardDto> GetInsuranceDashboardAsync(int vendorId);

    /// <summary>
    /// Gumruk dashboard verisi (Capability ID: 6)
    /// </summary>
    Task<CustomsDashboardDto> GetCustomsDashboardAsync(int vendorId);

    /// <summary>
    /// Gozetim dashboard verisi (Capability ID: 7)
    /// </summary>
    Task<SurveyDashboardDto> GetSurveyDashboardAsync(int vendorId);

    /// <summary>
    /// Yatirimci dashboard verisi (Capability ID: 8)
    /// </summary>
    Task<InvestorDashboardDto> GetInvestorDashboardAsync(int vendorId);
}
