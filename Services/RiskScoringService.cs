using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Risk skorlama servisi implementasyonu
/// Finansman talepleri icin risk hesaplar (0-100, dusuk = dusuk risk)
/// </summary>
public class RiskScoringService : IRiskScoringService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RiskScoringService> _logger;

    // Risk agirliklari (toplam 100)
    private const int WEIGHT_VENDOR_VERIFICATION = 20;    // Firma dogrulama durumu
    private const int WEIGHT_VENDOR_AGE = 10;             // Firma yasi
    private const int WEIGHT_VENDOR_PROFILE = 10;         // Profil tamamlanma
    private const int WEIGHT_ORDER_HISTORY = 20;          // Siparis gecmisi
    private const int WEIGHT_PAYMENT_HISTORY = 15;        // Odeme gecmisi
    private const int WEIGHT_COLLATERAL = 15;             // Teminat tipi
    private const int WEIGHT_AMOUNT_RATIO = 10;           // Tutar/Gecmis orani

    public RiskScoringService(
        ApplicationDbContext context,
        ILogger<RiskScoringService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RiskScoreResult> CalculateRiskScoreAsync(int financingRequestId)
    {
        var request = await _context.FinancingRequests
            .Include(r => r.RequesterVendor)
            .Include(r => r.RelatedOrder)
            .FirstOrDefaultAsync(r => r.Id == financingRequestId);

        if (request == null)
        {
            _logger.LogWarning("FinancingRequest {Id} bulunamadi", financingRequestId);
            return CreateDefaultHighRiskResult("Finansman talebi bulunamadi");
        }

        return await CalculateRiskScoreInternalAsync(
            request.RequesterVendorId,
            request.RequestedAmount,
            request.DurationDays,
            request.CollateralType,
            request.RelatedOrderId);
    }

    /// <inheritdoc />
    public async Task<RiskScoreResult> CalculateOrderRiskScoreAsync(int orderId, decimal requestedAmount, int durationDays)
    {
        var order = await _context.Orders
            .Include(o => o.BuyerVendor)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            _logger.LogWarning("Order {Id} bulunamadi", orderId);
            return CreateDefaultHighRiskResult("Siparis bulunamadi");
        }

        return await CalculateRiskScoreInternalAsync(
            order.BuyerVendorId,
            requestedAmount,
            durationDays,
            CollateralTypes.Order.Id, // Siparis teminati
            orderId);
    }

    /// <inheritdoc />
    public async Task<RiskScoreResult> CalculateVendorRiskScoreAsync(int vendorId)
    {
        var vendor = await _context.Vendors.FindAsync(vendorId);
        if (vendor == null)
        {
            return CreateDefaultHighRiskResult("Firma bulunamadi");
        }

        // Sadece firma faktorlerini hesapla
        var factors = new List<RiskFactorDto>();

        // 1. Firma dogrulama
        factors.Add(CalculateVerificationFactor(vendor.IsVerified));

        // 2. Firma yasi
        factors.Add(CalculateAgeFactor(vendor.CreatedAt));

        // 3. Profil tamamlanma
        factors.Add(CalculateProfileFactor(vendor.IsProfileComplete, vendor));

        // 4. Siparis gecmisi
        var orderStats = await GetOrderStatisticsAsync(vendorId);
        factors.Add(CalculateOrderHistoryFactor(orderStats.completedCount, orderStats.totalCount));

        // 5. Odeme gecmisi
        var paymentStats = await GetPaymentStatisticsAsync(vendorId);
        factors.Add(CalculatePaymentHistoryFactor(paymentStats.onTimeCount, paymentStats.totalCount, paymentStats.defaultCount));

        return BuildRiskScoreResult(factors);
    }

    /// <inheritdoc />
    public async Task<int> UpdateRiskScoreAsync(int financingRequestId)
    {
        var result = await CalculateRiskScoreAsync(financingRequestId);

        var request = await _context.FinancingRequests.FindAsync(financingRequestId);
        if (request != null)
        {
            request.RiskScore = result.Score;
            request.RiskNotes = result.Notes;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Risk skoru guncellendi. FinancingRequest: {Id}, Score: {Score}, Level: {Level}",
                financingRequestId, result.Score, result.Level);
        }

        return result.Score;
    }

    /// <summary>
    /// Ana risk hesaplama metodu
    /// </summary>
    private async Task<RiskScoreResult> CalculateRiskScoreInternalAsync(
        int vendorId,
        decimal requestedAmount,
        int durationDays,
        int? collateralType,
        int? orderId)
    {
        var vendor = await _context.Vendors.FindAsync(vendorId);
        if (vendor == null)
        {
            return CreateDefaultHighRiskResult("Firma bulunamadi");
        }

        var factors = new List<RiskFactorDto>();

        // 1. Firma dogrulama durumu (20%)
        factors.Add(CalculateVerificationFactor(vendor.IsVerified));

        // 2. Firma yasi (10%)
        factors.Add(CalculateAgeFactor(vendor.CreatedAt));

        // 3. Profil tamamlanma (10%)
        factors.Add(CalculateProfileFactor(vendor.IsProfileComplete, vendor));

        // 4. Siparis gecmisi (20%)
        var orderStats = await GetOrderStatisticsAsync(vendorId);
        factors.Add(CalculateOrderHistoryFactor(orderStats.completedCount, orderStats.totalCount));

        // 5. Odeme/Finansman gecmisi (15%)
        var paymentStats = await GetPaymentStatisticsAsync(vendorId);
        factors.Add(CalculatePaymentHistoryFactor(paymentStats.onTimeCount, paymentStats.totalCount, paymentStats.defaultCount));

        // 6. Teminat tipi (15%)
        factors.Add(CalculateCollateralFactor(collateralType));

        // 7. Tutar/Gecmis orani (10%)
        factors.Add(CalculateAmountRatioFactor(requestedAmount, orderStats.totalAmount));

        return BuildRiskScoreResult(factors);
    }

    /// <summary>
    /// Dogrulama faktoru hesapla
    /// </summary>
    private RiskFactorDto CalculateVerificationFactor(bool isVerified)
    {
        var score = isVerified ? 0 : 80; // Dogrulanmis = dusuk risk
        return new RiskFactorDto
        {
            Name = "Firma Dogrulama",
            Category = "Firma",
            Score = score,
            Weight = WEIGHT_VENDOR_VERIFICATION,
            WeightedScore = score * WEIGHT_VENDOR_VERIFICATION / 100m,
            Description = isVerified
                ? "Firma platform tarafindan dogrulanmis"
                : "Firma henuz dogrulanmamis - yuksek risk",
            IsPositive = isVerified
        };
    }

    /// <summary>
    /// Firma yasi faktoru hesapla
    /// </summary>
    private RiskFactorDto CalculateAgeFactor(DateTime createdAt)
    {
        var ageMonths = (DateTime.UtcNow - createdAt).TotalDays / 30;
        int score;
        string description;
        bool isPositive;

        if (ageMonths >= 24) // 2+ yil
        {
            score = 0;
            description = "Firma 2+ yildir platformda - cok dusuk risk";
            isPositive = true;
        }
        else if (ageMonths >= 12) // 1-2 yil
        {
            score = 20;
            description = "Firma 1-2 yildir platformda - dusuk risk";
            isPositive = true;
        }
        else if (ageMonths >= 6) // 6-12 ay
        {
            score = 40;
            description = "Firma 6-12 aydir platformda - orta risk";
            isPositive = false;
        }
        else if (ageMonths >= 3) // 3-6 ay
        {
            score = 60;
            description = "Firma 3-6 aydir platformda - orta-yuksek risk";
            isPositive = false;
        }
        else // 0-3 ay
        {
            score = 80;
            description = "Firma yeni kayitli (<3 ay) - yuksek risk";
            isPositive = false;
        }

        return new RiskFactorDto
        {
            Name = "Firma Yasi",
            Category = "Firma",
            Score = score,
            Weight = WEIGHT_VENDOR_AGE,
            WeightedScore = score * WEIGHT_VENDOR_AGE / 100m,
            Description = description,
            IsPositive = isPositive
        };
    }

    /// <summary>
    /// Profil tamamlanma faktoru hesapla
    /// </summary>
    private RiskFactorDto CalculateProfileFactor(bool isProfileComplete, Models.Entities.Vendor vendor)
    {
        // Profil tamamlanma + kritik alanlarin kontrolu
        var criticalFieldsComplete = !string.IsNullOrEmpty(vendor.TaxNumber) &&
                                     !string.IsNullOrEmpty(vendor.Iban) &&
                                     !string.IsNullOrEmpty(vendor.AuthorizedPersonName);

        int score;
        string description;
        bool isPositive;

        if (isProfileComplete && criticalFieldsComplete)
        {
            score = 0;
            description = "Profil tam ve kritik alanlar dolu";
            isPositive = true;
        }
        else if (isProfileComplete)
        {
            score = 30;
            description = "Profil tamamlanmis ama bazi kritik alanlar eksik";
            isPositive = false;
        }
        else if (criticalFieldsComplete)
        {
            score = 40;
            description = "Kritik alanlar dolu ama profil tamamlanmamis";
            isPositive = false;
        }
        else
        {
            score = 70;
            description = "Profil eksik - yuksek risk";
            isPositive = false;
        }

        return new RiskFactorDto
        {
            Name = "Profil Durumu",
            Category = "Firma",
            Score = score,
            Weight = WEIGHT_VENDOR_PROFILE,
            WeightedScore = score * WEIGHT_VENDOR_PROFILE / 100m,
            Description = description,
            IsPositive = isPositive
        };
    }

    /// <summary>
    /// Siparis gecmisi faktoru hesapla
    /// </summary>
    private RiskFactorDto CalculateOrderHistoryFactor(int completedCount, int totalCount)
    {
        int score;
        string description;
        bool isPositive;

        if (totalCount == 0)
        {
            score = 70;
            description = "Hic siparis gecmisi yok - yuksek risk";
            isPositive = false;
        }
        else if (completedCount >= 10)
        {
            var completionRate = (decimal)completedCount / totalCount * 100;
            if (completionRate >= 90)
            {
                score = 0;
                description = $"10+ tamamlanan siparis, %{completionRate:F0} basari orani - cok dusuk risk";
                isPositive = true;
            }
            else
            {
                score = 20;
                description = $"10+ tamamlanan siparis, %{completionRate:F0} basari orani - dusuk risk";
                isPositive = true;
            }
        }
        else if (completedCount >= 5)
        {
            score = 30;
            description = $"{completedCount} tamamlanan siparis - orta-dusuk risk";
            isPositive = true;
        }
        else if (completedCount >= 1)
        {
            score = 50;
            description = $"{completedCount} tamamlanan siparis - orta risk";
            isPositive = false;
        }
        else
        {
            score = 70;
            description = "Tamamlanan siparis yok - yuksek risk";
            isPositive = false;
        }

        return new RiskFactorDto
        {
            Name = "Siparis Gecmisi",
            Category = "Islem Gecmisi",
            Score = score,
            Weight = WEIGHT_ORDER_HISTORY,
            WeightedScore = score * WEIGHT_ORDER_HISTORY / 100m,
            Description = description,
            IsPositive = isPositive
        };
    }

    /// <summary>
    /// Odeme gecmisi faktoru hesapla
    /// </summary>
    private RiskFactorDto CalculatePaymentHistoryFactor(int onTimeCount, int totalCount, int defaultCount)
    {
        int score;
        string description;
        bool isPositive;

        if (defaultCount > 0)
        {
            score = 100; // Temerut varsa maksimum risk
            description = $"{defaultCount} temerut kaydi - cok yuksek risk";
            isPositive = false;
        }
        else if (totalCount == 0)
        {
            score = 50; // Gecmis yok - orta risk
            description = "Finansman gecmisi yok - orta risk";
            isPositive = false;
        }
        else
        {
            var onTimeRate = (decimal)onTimeCount / totalCount * 100;
            if (onTimeRate >= 95)
            {
                score = 0;
                description = $"%{onTimeRate:F0} zamaninda odeme - cok dusuk risk";
                isPositive = true;
            }
            else if (onTimeRate >= 80)
            {
                score = 20;
                description = $"%{onTimeRate:F0} zamaninda odeme - dusuk risk";
                isPositive = true;
            }
            else if (onTimeRate >= 60)
            {
                score = 50;
                description = $"%{onTimeRate:F0} zamaninda odeme - orta risk";
                isPositive = false;
            }
            else
            {
                score = 80;
                description = $"%{onTimeRate:F0} zamaninda odeme - yuksek risk";
                isPositive = false;
            }
        }

        return new RiskFactorDto
        {
            Name = "Odeme Gecmisi",
            Category = "Islem Gecmisi",
            Score = score,
            Weight = WEIGHT_PAYMENT_HISTORY,
            WeightedScore = score * WEIGHT_PAYMENT_HISTORY / 100m,
            Description = description,
            IsPositive = isPositive
        };
    }

    /// <summary>
    /// Teminat tipi faktoru hesapla
    /// </summary>
    private RiskFactorDto CalculateCollateralFactor(int? collateralType)
    {
        int score;
        string description;
        bool isPositive;

        if (!collateralType.HasValue || collateralType == CollateralTypes.None.Id)
        {
            score = 90;
            description = "Teminat yok - cok yuksek risk";
            isPositive = false;
        }
        else if (collateralType == CollateralTypes.Order.Id)
        {
            score = 20;
            description = "Siparis teminati - dusuk risk";
            isPositive = true;
        }
        else if (collateralType == CollateralTypes.Invoice.Id)
        {
            score = 30;
            description = "Fatura teminati - dusuk-orta risk";
            isPositive = true;
        }
        else if (collateralType == CollateralTypes.Inventory.Id)
        {
            score = 40;
            description = "Stok teminati - orta risk";
            isPositive = false;
        }
        else if (collateralType == CollateralTypes.Receivables.Id)
        {
            score = 50;
            description = "Alacak teminati - orta risk";
            isPositive = false;
        }
        else
        {
            score = 60;
            description = "Diger teminat tipi - orta-yuksek risk";
            isPositive = false;
        }

        return new RiskFactorDto
        {
            Name = "Teminat Tipi",
            Category = "Finansman",
            Score = score,
            Weight = WEIGHT_COLLATERAL,
            WeightedScore = score * WEIGHT_COLLATERAL / 100m,
            Description = description,
            IsPositive = isPositive
        };
    }

    /// <summary>
    /// Tutar/Gecmis orani faktoru hesapla
    /// </summary>
    private RiskFactorDto CalculateAmountRatioFactor(decimal requestedAmount, decimal historicalTotal)
    {
        int score;
        string description;
        bool isPositive;

        if (historicalTotal == 0)
        {
            score = 70;
            description = "Gecmis islem tutari yok - yuksek risk";
            isPositive = false;
        }
        else
        {
            var ratio = requestedAmount / historicalTotal;
            if (ratio <= 0.1m) // %10'dan az
            {
                score = 0;
                description = $"Talep tutari gecmis islemlerin %{ratio * 100:F0}'i - cok dusuk risk";
                isPositive = true;
            }
            else if (ratio <= 0.25m) // %25'ten az
            {
                score = 20;
                description = $"Talep tutari gecmis islemlerin %{ratio * 100:F0}'i - dusuk risk";
                isPositive = true;
            }
            else if (ratio <= 0.5m) // %50'den az
            {
                score = 40;
                description = $"Talep tutari gecmis islemlerin %{ratio * 100:F0}'i - orta risk";
                isPositive = false;
            }
            else if (ratio <= 1m) // %100'den az
            {
                score = 60;
                description = $"Talep tutari gecmis islemlerin %{ratio * 100:F0}'i - orta-yuksek risk";
                isPositive = false;
            }
            else
            {
                score = 80;
                description = $"Talep tutari gecmis islemlerden buyuk - yuksek risk";
                isPositive = false;
            }
        }

        return new RiskFactorDto
        {
            Name = "Tutar Orani",
            Category = "Finansman",
            Score = score,
            Weight = WEIGHT_AMOUNT_RATIO,
            WeightedScore = score * WEIGHT_AMOUNT_RATIO / 100m,
            Description = description,
            IsPositive = isPositive
        };
    }

    /// <summary>
    /// Siparis istatistiklerini getir
    /// </summary>
    private async Task<(int totalCount, int completedCount, decimal totalAmount)> GetOrderStatisticsAsync(int vendorId)
    {
        var orders = await _context.Orders
            .Where(o => o.BuyerVendorId == vendorId && !o.IsDeleted)
            .Select(o => new { o.Status, o.TotalAmount })
            .ToListAsync();

        var totalCount = orders.Count;
        var completedCount = orders.Count(o => o.Status == OrderStatuses.Completed.Id);
        var totalAmount = orders.Sum(o => o.TotalAmount);

        return (totalCount, completedCount, totalAmount);
    }

    /// <summary>
    /// Odeme/Finansman istatistiklerini getir
    /// </summary>
    private async Task<(int totalCount, int onTimeCount, int defaultCount)> GetPaymentStatisticsAsync(int vendorId)
    {
        var financingRequests = await _context.FinancingRequests
            .Where(r => r.RequesterVendorId == vendorId && !r.IsDeleted)
            .Where(r => r.Status == FinancingRequestStatuses.Repaid.Id ||
                       r.Status == FinancingRequestStatuses.Defaulted.Id)
            .Select(r => new { r.Status })
            .ToListAsync();

        var totalCount = financingRequests.Count;
        var onTimeCount = financingRequests.Count(r => r.Status == FinancingRequestStatuses.Repaid.Id);
        var defaultCount = financingRequests.Count(r => r.Status == FinancingRequestStatuses.Defaulted.Id);

        return (totalCount, onTimeCount, defaultCount);
    }

    /// <summary>
    /// Risk skoru sonucunu olustur
    /// </summary>
    private RiskScoreResult BuildRiskScoreResult(List<RiskFactorDto> factors)
    {
        var totalScore = (int)Math.Round(factors.Sum(f => f.WeightedScore));
        totalScore = Math.Clamp(totalScore, 0, 100);

        var (level, cssClass) = GetRiskLevel(totalScore);

        var notes = string.Join("\n", factors
            .OrderByDescending(f => f.WeightedScore)
            .Take(3)
            .Select(f => $"- {f.Name}: {f.Description}"));

        return new RiskScoreResult
        {
            Score = totalScore,
            Level = level,
            LevelCssClass = cssClass,
            Notes = notes,
            Factors = factors
        };
    }

    /// <summary>
    /// Risk seviyesini belirle
    /// </summary>
    private (string level, string cssClass) GetRiskLevel(int score)
    {
        return score switch
        {
            <= 25 => ("Low", "success"),
            <= 50 => ("Medium", "warning"),
            <= 75 => ("High", "danger"),
            _ => ("VeryHigh", "dark")
        };
    }

    /// <summary>
    /// Varsayilan yuksek risk sonucu olustur
    /// </summary>
    private RiskScoreResult CreateDefaultHighRiskResult(string reason)
    {
        return new RiskScoreResult
        {
            Score = 100,
            Level = "VeryHigh",
            LevelCssClass = "dark",
            Notes = reason,
            Factors = new List<RiskFactorDto>
            {
                new RiskFactorDto
                {
                    Name = "Hata",
                    Category = "Sistem",
                    Score = 100,
                    Weight = 100,
                    WeightedScore = 100,
                    Description = reason,
                    IsPositive = false
                }
            }
        };
    }
}
