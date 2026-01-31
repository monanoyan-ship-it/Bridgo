using Bridgo.DTOs.Proposal;

namespace Bridgo.Services.Interfaces;

/// <summary>
/// Satıcının verdiği tüm teklifleri yöneten birleşik servis
/// ProductInquiryResponse + DemandResponse
/// </summary>
public interface IProposalService
{
    /// <summary>
    /// Satıcının tüm tekliflerini birleşik listele
    /// </summary>
    Task<UnifiedProposalSearchResultDto> GetSellerProposalsAsync(int sellerVendorId, UnifiedProposalFilterDto filter);

    /// <summary>
    /// Satıcının teklif istatistiklerini getir
    /// </summary>
    Task<UnifiedProposalStatsDto> GetSellerProposalStatsAsync(int sellerVendorId);

    /// <summary>
    /// Satıcının ürün tekliflerini (ProductInquiryResponse) listele
    /// </summary>
    Task<UnifiedProposalSearchResultDto> GetInquiryProposalsAsync(int sellerVendorId, UnifiedProposalFilterDto filter);

    /// <summary>
    /// Satıcının talep tekliflerini (DemandResponse) listele
    /// </summary>
    Task<UnifiedProposalSearchResultDto> GetDemandProposalsAsync(int sellerVendorId, UnifiedProposalFilterDto filter);
}
