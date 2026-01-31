namespace Bridgo.DTOs.Dashboard;

// ============================================
// FIRMA GENEL BILGILERI DASHBOARD
// ============================================

/// <summary>
/// Firma genel bilgileri dashboard'u
/// Capability parametresi olmadan /Dashboard'a gelindiginde gosterilir
/// </summary>
public class CompanyDashboardDto
{
    // Firma Bilgileri
    public int VendorId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? TaxNumber { get; set; }
    public string? TradeRegistryNo { get; set; }
    public bool IsVerified { get; set; }
    public bool IsProfileComplete { get; set; }
    public DateTime CreatedAt { get; set; }

    // Capability Bilgileri
    public List<CompanyCapabilityInfoDto> Capabilities { get; set; } = new();

    // Ekip Bilgileri
    public int TeamMemberCount { get; set; }
    public int ActiveUserCount { get; set; }
    public int PendingInviteCount { get; set; }

    // Adres Bilgileri
    public CompanyAddressDto? HeadquartersAddress { get; set; }
    public int TotalAddressCount { get; set; }
    public int WarehouseCount { get; set; }

    // Urun/Katalog Bilgileri (Satici capability varsa)
    public int? TotalProductCount { get; set; }
    public int? ActiveProductCount { get; set; }
    public int? CategoryCount { get; set; }

    // Siparis Ozeti
    public int TotalOrderCount { get; set; }
    public int ActiveOrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalSpending { get; set; }
    public string Currency { get; set; } = "TRY";

    // Son Aktiviteler
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

/// <summary>
/// Firma capability bilgisi
/// </summary>
public class CompanyCapabilityInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public DateTime AddedAt { get; set; }
}

/// <summary>
/// Firma adres bilgisi
/// </summary>
public class CompanyAddressDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? StateName { get; set; }
    public string? CountryName { get; set; }
    public string? PostalCode { get; set; }
}

/// <summary>
/// Son aktivite
/// </summary>
public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty; // Order, Demand, Product, Inquiry, etc.
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? IconColor { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ActionUrl { get; set; }
}

// ============================================
// ORTAK DTO'LAR
// ============================================

/// <summary>
/// Son siparis ozeti
/// </summary>
public class RecentOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCssClass { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// En cok satan urun
/// </summary>
public class TopProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SoldQuantity { get; set; }
    public decimal Revenue { get; set; }
    public string Currency { get; set; } = "TRY";
}

/// <summary>
/// Son talep ozeti
/// </summary>
public class RecentDemandDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int Quantity { get; set; }
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCssClass { get; set; } = string.Empty;
    public int ResponseCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Son urun sorgusu
/// </summary>
public class RecentInquiryDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Bekleyen teklif ozeti
/// </summary>
public class PendingProposalDto
{
    public int Id { get; set; }
    public string DemandTitle { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Servis talebi ozeti (Lojistik, Sigorta, Gumruk, Gozetim)
/// </summary>
public class ServiceRequestSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RequestorName { get; set; } = string.Empty;
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Yatirim firsati ozeti
/// </summary>
public class InvestmentOpportunitySummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal MinInterestRate { get; set; }
    public decimal MaxInterestRate { get; set; }
    public int DurationMonths { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Aktif yatirim ozeti
/// </summary>
public class ActiveInvestmentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public decimal InvestedAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal InterestRate { get; set; }
    public decimal ExpectedReturn { get; set; }
    public DateTime MaturityDate { get; set; }
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
}

// ============================================
// SATICI DASHBOARD (ID: 2)
// ============================================

public class SellerDashboardDto
{
    // Stats
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public string RevenueCurrency { get; set; } = "TRY";
    public int ActiveProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int NewInquiries { get; set; }
    public int PendingDemandResponses { get; set; }

    // Recent items
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
    public List<RecentInquiryDto> RecentInquiries { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
}

// ============================================
// ALICI DASHBOARD (ID: 3)
// ============================================

public class BuyerDashboardDto
{
    // Stats
    public int ActiveOrders { get; set; }
    public int PendingDeliveries { get; set; }
    public int ActiveDemands { get; set; }
    public int PendingProposals { get; set; }
    public decimal MonthlySpending { get; set; }
    public string SpendingCurrency { get; set; } = "TRY";

    // Recent items
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
    public List<RecentDemandDto> RecentDemands { get; set; } = new();
    public List<PendingProposalDto> PendingProposalsList { get; set; } = new();
}

// ============================================
// TASIMACI DASHBOARD (ID: 4)
// ============================================

public class CarrierDashboardDto
{
    // Stats
    public int OpenRequests { get; set; }
    public int ActiveShipments { get; set; }
    public int PendingQuotes { get; set; }
    public int CompletedThisMonth { get; set; }
    public decimal MonthlyEarnings { get; set; }
    public string EarningsCurrency { get; set; } = "TRY";

    // Recent items
    public List<ServiceRequestSummaryDto> RecentRequests { get; set; } = new();
}

// ============================================
// SIGORTA DASHBOARD (ID: 5)
// ============================================

public class InsuranceDashboardDto
{
    // Stats
    public int OpenRequests { get; set; }
    public int ActivePolicies { get; set; }
    public int PendingQuotes { get; set; }
    public int PendingClaims { get; set; }
    public decimal MonthlyPremium { get; set; }
    public string PremiumCurrency { get; set; } = "TRY";

    // Recent items
    public List<ServiceRequestSummaryDto> RecentRequests { get; set; } = new();
}

// ============================================
// GUMRUK DASHBOARD (ID: 6)
// ============================================

public class CustomsDashboardDto
{
    // Stats
    public int OpenRequests { get; set; }
    public int ActiveDeclarations { get; set; }
    public int PendingQuotes { get; set; }
    public int CompletedThisMonth { get; set; }
    public decimal MonthlyEarnings { get; set; }
    public string EarningsCurrency { get; set; } = "TRY";

    // Recent items
    public List<ServiceRequestSummaryDto> RecentRequests { get; set; } = new();
}

// ============================================
// GOZETIM DASHBOARD (ID: 7)
// ============================================

public class SurveyDashboardDto
{
    // Stats
    public int OpenRequests { get; set; }
    public int ScheduledInspections { get; set; }
    public int PendingReports { get; set; }
    public int CompletedThisMonth { get; set; }
    public decimal MonthlyEarnings { get; set; }
    public string EarningsCurrency { get; set; } = "TRY";

    // Recent items
    public List<ServiceRequestSummaryDto> RecentRequests { get; set; } = new();
}

// ============================================
// YATIRIMCI DASHBOARD (ID: 8)
// ============================================

public class InvestorDashboardDto
{
    // Stats
    public int AvailableOpportunities { get; set; }
    public int ActiveInvestments { get; set; }
    public int PendingOffers { get; set; }
    public decimal TotalPortfolioValue { get; set; }
    public decimal ExpectedMonthlyReturn { get; set; }
    public string Currency { get; set; } = "TRY";

    // Recent items
    public List<InvestmentOpportunitySummaryDto> RecentOpportunities { get; set; } = new();
    public List<ActiveInvestmentDto> ActiveInvestmentsList { get; set; } = new();
}
