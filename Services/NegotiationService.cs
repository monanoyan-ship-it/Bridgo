using Bridgo.Data;
using Bridgo.DTOs.Demand;
using Bridgo.DTOs.Notification;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bridgo.Services;

/// <summary>
/// Pazarlik (Negotiation) servisi implementasyonu
/// DemandResponse uzerinde cift yonlu pazarlik islemleri
/// </summary>
public class NegotiationService : INegotiationService
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notificationService;
    private const int NEGOTIATION_TIMEOUT_HOURS = 48;

    public NegotiationService(ApplicationDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    // ============================================
    // PAZARLIK BASLATMA
    // ============================================

    public async Task<NegotiationRoundDetailDto> StartNegotiationAsync(int demandResponseId, int buyerVendorId, CounterOfferCreateDto dto, string? createdBy)
    {
        var response = await _db.DemandResponses
            .Include(r => r.Demand)
            .Include(r => r.SupplierVendor)
            .FirstOrDefaultAsync(r => r.Id == demandResponseId && !r.IsDeleted);

        if (response == null)
            throw new InvalidOperationException("Teklif bulunamadi.");

        // Buyer kontrolu: Demand sahibi mi?
        if (response.Demand.VendorId != buyerVendorId)
            throw new InvalidOperationException("Bu teklif uzerinde pazarlik baslatamazsiniz.");

        // Zaten pazarlik var mi?
        if (response.IsNegotiationActive)
            throw new InvalidOperationException("Bu teklif icin zaten aktif bir pazarlik var.");

        // Status kontrolu: Accepted veya Rejected degilse pazarlik baslatilabilir
        if (response.Status == DemandResponseStatuses.Ids.Accepted || response.Status == DemandResponseStatuses.Ids.Rejected)
            throw new InvalidOperationException("Bu teklif uzerinde pazarlik baslatilamaz.");

        // Supplier vendor gerekli
        if (response.SupplierVendorId == null)
            throw new InvalidOperationException("Dis tedarikci teklifleri uzerinde pazarlik yapilamaz.");

        var now = DateTime.UtcNow;
        var expiresAt = now.AddHours(NEGOTIATION_TIMEOUT_HOURS);

        // Ilk pazarlik turu olustur
        var round = new NegotiationRound
        {
            DemandResponseId = demandResponseId,
            InitiatorVendorId = buyerVendorId,
            RoundNumber = 1,
            UnitPrice = dto.UnitPrice,
            TotalPrice = dto.TotalPrice,
            Currency = dto.Currency ?? response.Currency ?? "TRY",
            Quantity = dto.Quantity,
            Unit = dto.Unit ?? response.Unit,
            LeadTimeDays = dto.LeadTimeDays,
            ValidUntil = dto.ValidUntil,
            Notes = dto.Notes,
            TermsAndConditions = dto.TermsAndConditions,
            Status = NegotiationRoundStatuses.Ids.Active,
            ExpiresAt = expiresAt,
            CreatedBy = createdBy
        };

        _db.NegotiationRounds.Add(round);

        // DemandResponse guncelle
        response.IsNegotiationActive = true;
        response.CurrentTurnVendorId = response.SupplierVendorId; // Sira supplier'da
        response.CurrentRoundNumber = 1;
        response.NegotiationExpiresAt = expiresAt;
        response.Status = DemandResponseStatuses.Ids.Negotiating;
        response.UpdatedBy = createdBy;

        await _db.SaveChangesAsync();

        // Bildirim gonder
        await _notificationService.CreateAsync(new NotificationCreateDto
        {
            VendorId = response.SupplierVendorId!.Value,
            UserId = null,
            Type = NotificationType.NegotiationStarted,
            Title = "Pazarlik Basladi",
            Message = $"{response.Demand.Title} teklifi icin karsi teklif geldi.",
            ActionUrl = $"/Demands/SupplierOffers?responseId={demandResponseId}",
            Icon = "bi-arrow-left-right"
        });

        return MapToDetailDto(round, response.Demand.VendorId);
    }

    // ============================================
    // KARSI TEKLIF
    // ============================================

    public async Task<NegotiationRoundDetailDto> CreateCounterOfferAsync(int vendorId, CounterOfferCreateDto dto, string? createdBy)
    {
        var response = await _db.DemandResponses
            .Include(r => r.Demand)
            .Include(r => r.SupplierVendor)
            .Include(r => r.NegotiationRounds.OrderByDescending(n => n.RoundNumber).Take(1))
            .FirstOrDefaultAsync(r => r.Id == dto.DemandResponseId && !r.IsDeleted);

        if (response == null)
            throw new InvalidOperationException("Teklif bulunamadi.");

        // Pazarlik aktif mi?
        if (!response.IsNegotiationActive)
            throw new InvalidOperationException("Bu teklif icin aktif pazarlik yok.");

        // Sira bu vendor'da mi?
        if (response.CurrentTurnVendorId != vendorId)
            throw new InvalidOperationException("Karsi teklif verme sirasi sizde degil.");

        // Timeout kontrolu
        if (response.NegotiationExpiresAt.HasValue && response.NegotiationExpiresAt.Value < DateTime.UtcNow)
        {
            await ExpireNegotiationAsync(response, createdBy);
            throw new InvalidOperationException("Pazarlik suresi dolmus.");
        }

        var lastRound = response.NegotiationRounds.FirstOrDefault();
        if (lastRound != null)
        {
            lastRound.Status = NegotiationRoundStatuses.Ids.Countered;
            lastRound.RespondedAt = DateTime.UtcNow;
            lastRound.UpdatedBy = createdBy;
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddHours(NEGOTIATION_TIMEOUT_HOURS);
        var newRoundNumber = response.CurrentRoundNumber + 1;

        // Yeni tur olustur
        var round = new NegotiationRound
        {
            DemandResponseId = dto.DemandResponseId,
            InitiatorVendorId = vendorId,
            RoundNumber = newRoundNumber,
            UnitPrice = dto.UnitPrice,
            TotalPrice = dto.TotalPrice,
            Currency = dto.Currency ?? response.Currency ?? "TRY",
            Quantity = dto.Quantity,
            Unit = dto.Unit ?? response.Unit,
            LeadTimeDays = dto.LeadTimeDays,
            ValidUntil = dto.ValidUntil,
            Notes = dto.Notes,
            TermsAndConditions = dto.TermsAndConditions,
            Status = NegotiationRoundStatuses.Ids.Active,
            ExpiresAt = expiresAt,
            CreatedBy = createdBy
        };

        _db.NegotiationRounds.Add(round);

        // Karsi tarafin vendor ID'sini bul
        var otherVendorId = vendorId == response.Demand.VendorId
            ? response.SupplierVendorId!.Value
            : response.Demand.VendorId;

        // DemandResponse guncelle
        response.CurrentTurnVendorId = otherVendorId;
        response.CurrentRoundNumber = newRoundNumber;
        response.NegotiationExpiresAt = expiresAt;
        response.UpdatedBy = createdBy;

        await _db.SaveChangesAsync();

        // Bildirim gonder
        var notificationUrl = vendorId == response.Demand.VendorId
            ? $"/Demands/SupplierOffers?responseId={dto.DemandResponseId}"
            : $"/Proposals/CompareProposals?responseId={dto.DemandResponseId}";

        await _notificationService.CreateAsync(new NotificationCreateDto
        {
            VendorId = otherVendorId,
            UserId = null,
            Type = NotificationType.NegotiationCounterOffer,
            Title = "Yeni Karsi Teklif",
            Message = $"{response.Demand.Title} icin yeni karsi teklif geldi. (Tur {newRoundNumber})",
            ActionUrl = notificationUrl,
            Icon = "bi-arrow-left-right"
        });

        return MapToDetailDto(round, response.Demand.VendorId);
    }

    // ============================================
    // YANIT ISLEMLERI
    // ============================================

    public async Task<bool> AcceptCounterOfferAsync(int roundId, int vendorId, string? updatedBy)
    {
        var round = await _db.NegotiationRounds
            .Include(r => r.DemandResponse)
                .ThenInclude(dr => dr.Demand)
            .Include(r => r.DemandResponse)
                .ThenInclude(dr => dr.SupplierVendor)
            .FirstOrDefaultAsync(r => r.Id == roundId && !r.IsDeleted);

        if (round == null)
            throw new InvalidOperationException("Pazarlik turu bulunamadi.");

        var response = round.DemandResponse;

        // Sira bu vendor'da mi?
        if (response.CurrentTurnVendorId != vendorId)
            throw new InvalidOperationException("Kabul etme sirasi sizde degil.");

        // Aktif mi?
        if (round.Status != NegotiationRoundStatuses.Ids.Active)
            throw new InvalidOperationException("Bu tur zaten yanitlanmis.");

        // Timeout kontrolu
        if (round.ExpiresAt < DateTime.UtcNow)
        {
            await ExpireNegotiationAsync(response, updatedBy);
            throw new InvalidOperationException("Pazarlik suresi dolmus.");
        }

        var now = DateTime.UtcNow;

        // Turu kabul et
        round.Status = NegotiationRoundStatuses.Ids.Accepted;
        round.RespondedAt = now;
        round.UpdatedBy = updatedBy;

        // DemandResponse guncelle - kabul edilen fiyatlarla
        response.IsNegotiationActive = false;
        response.CurrentTurnVendorId = null;
        response.NegotiationExpiresAt = null;
        response.Status = DemandResponseStatuses.Ids.Accepted;
        response.UpdatedBy = updatedBy;

        // Kabul edilen teklif degerlerini DemandResponse'a yaz
        if (round.UnitPrice.HasValue)
            response.UnitPrice = round.UnitPrice.Value;
        if (round.TotalPrice.HasValue)
            response.TotalPrice = round.TotalPrice.Value;
        if (!string.IsNullOrEmpty(round.Currency))
            response.Currency = round.Currency;
        if (round.Quantity.HasValue)
            response.Quantity = round.Quantity.Value;
        if (!string.IsNullOrEmpty(round.Unit))
            response.Unit = round.Unit;
        if (round.LeadTimeDays.HasValue)
            response.LeadTimeDays = round.LeadTimeDays.Value;

        await _db.SaveChangesAsync();

        // Karsi tarafa bildirim
        var otherVendorId = vendorId == response.Demand.VendorId
            ? response.SupplierVendorId!.Value
            : response.Demand.VendorId;

        await _notificationService.CreateAsync(new NotificationCreateDto
        {
            VendorId = otherVendorId,
            UserId = null,
            Type = NotificationType.NegotiationAccepted,
            Title = "Teklif Kabul Edildi",
            Message = $"{response.Demand.Title} icin teklifiniz kabul edildi!",
            ActionUrl = vendorId == response.Demand.VendorId
                ? $"/Demands/SupplierOffers?responseId={response.Id}"
                : $"/Proposals/CompareProposals?responseId={response.Id}",
            Icon = "bi-check-circle"
        });

        return true;
    }

    public async Task<bool> RejectCounterOfferAsync(int roundId, int vendorId, string? reason, string? updatedBy)
    {
        var round = await _db.NegotiationRounds
            .Include(r => r.DemandResponse)
                .ThenInclude(dr => dr.Demand)
            .Include(r => r.DemandResponse)
                .ThenInclude(dr => dr.SupplierVendor)
            .FirstOrDefaultAsync(r => r.Id == roundId && !r.IsDeleted);

        if (round == null)
            throw new InvalidOperationException("Pazarlik turu bulunamadi.");

        var response = round.DemandResponse;

        // Sira bu vendor'da mi?
        if (response.CurrentTurnVendorId != vendorId)
            throw new InvalidOperationException("Reddetme sirasi sizde degil.");

        // Aktif mi?
        if (round.Status != NegotiationRoundStatuses.Ids.Active)
            throw new InvalidOperationException("Bu tur zaten yanitlanmis.");

        var now = DateTime.UtcNow;

        // Turu reddet
        round.Status = NegotiationRoundStatuses.Ids.Rejected;
        round.RespondedAt = now;
        round.RejectionReason = reason;
        round.UpdatedBy = updatedBy;

        // DemandResponse guncelle
        response.IsNegotiationActive = false;
        response.CurrentTurnVendorId = null;
        response.NegotiationExpiresAt = null;
        response.Status = DemandResponseStatuses.Ids.Rejected;
        response.RejectionReason = reason;
        response.UpdatedBy = updatedBy;

        await _db.SaveChangesAsync();

        // Karsi tarafa bildirim
        var otherVendorId = vendorId == response.Demand.VendorId
            ? response.SupplierVendorId!.Value
            : response.Demand.VendorId;

        await _notificationService.CreateAsync(new NotificationCreateDto
        {
            VendorId = otherVendorId,
            UserId = null,
            Type = NotificationType.NegotiationRejected,
            Title = "Teklif Reddedildi",
            Message = $"{response.Demand.Title} icin pazarlik reddedildi.",
            ActionUrl = vendorId == response.Demand.VendorId
                ? $"/Demands/SupplierOffers?responseId={response.Id}"
                : $"/Proposals/CompareProposals?responseId={response.Id}",
            Icon = "bi-x-circle"
        });

        return true;
    }

    public async Task<bool> WithdrawFromNegotiationAsync(int demandResponseId, int vendorId, string? updatedBy)
    {
        var response = await _db.DemandResponses
            .Include(r => r.Demand)
            .Include(r => r.NegotiationRounds.Where(n => n.Status == NegotiationRoundStatuses.Ids.Active))
            .FirstOrDefaultAsync(r => r.Id == demandResponseId && !r.IsDeleted);

        if (response == null)
            throw new InvalidOperationException("Teklif bulunamadi.");

        // Bu pazarligin taraflarından biri mi?
        if (response.Demand.VendorId != vendorId && response.SupplierVendorId != vendorId)
            throw new InvalidOperationException("Bu pazarliktan cekilme yetkiniz yok.");

        // Aktif pazarlik var mi?
        if (!response.IsNegotiationActive)
            throw new InvalidOperationException("Aktif pazarlik yok.");

        var now = DateTime.UtcNow;

        // Aktif turlari withdraw olarak isaretle
        foreach (var round in response.NegotiationRounds)
        {
            round.Status = NegotiationRoundStatuses.Ids.Withdrawn;
            round.RespondedAt = now;
            round.UpdatedBy = updatedBy;
        }

        // DemandResponse guncelle
        response.IsNegotiationActive = false;
        response.CurrentTurnVendorId = null;
        response.NegotiationExpiresAt = null;
        // Status'u degistirmiyoruz - teklif hala gecerli, sadece pazarlik bitti
        response.UpdatedBy = updatedBy;

        await _db.SaveChangesAsync();

        // Karsi tarafa bildirim
        var otherVendorId = vendorId == response.Demand.VendorId
            ? response.SupplierVendorId
            : response.Demand.VendorId;

        if (otherVendorId.HasValue)
        {
            await _notificationService.CreateAsync(new NotificationCreateDto
            {
                VendorId = otherVendorId.Value,
                UserId = null,
                Type = NotificationType.NegotiationRejected,
                Title = "Pazarlik Sonlandirildi",
                Message = $"{response.Demand.Title} icin pazarlik karsi taraf tarafindan sonlandirildi.",
                ActionUrl = vendorId == response.Demand.VendorId
                    ? $"/Demands/SupplierOffers?responseId={response.Id}"
                    : $"/Proposals/CompareProposals?responseId={response.Id}",
                Icon = "bi-x-octagon"
            });
        }

        return true;
    }

    // ============================================
    // SORGULAMA
    // ============================================

    public async Task<NegotiationHistoryDto?> GetNegotiationHistoryAsync(int demandResponseId, int vendorId)
    {
        var response = await _db.DemandResponses
            .Include(r => r.Demand)
                .ThenInclude(d => d.Vendor)
            .Include(r => r.SupplierVendor)
            .Include(r => r.NegotiationRounds.OrderBy(n => n.RoundNumber))
                .ThenInclude(n => n.Initiator)
            .FirstOrDefaultAsync(r => r.Id == demandResponseId && !r.IsDeleted);

        if (response == null)
            return null;

        // Yetki kontrolu: Bu pazarligin taraflarindan biri mi?
        if (response.Demand.VendorId != vendorId && response.SupplierVendorId != vendorId)
            return null;

        var buyerVendorId = response.Demand.VendorId;

        var history = new NegotiationHistoryDto
        {
            DemandResponseId = demandResponseId,
            DemandTitle = response.Demand.Title,
            BuyerVendorId = buyerVendorId,
            BuyerName = response.Demand.Vendor?.CompanyName ?? "",
            SupplierVendorId = response.SupplierVendorId,
            SupplierName = response.SupplierVendor?.CompanyName,
            CreatedAt = response.CreatedAt,
            InitialUnitPrice = response.UnitPrice,
            InitialTotalPrice = response.TotalPrice,
            InitialCurrency = response.Currency,
            InitialQuantity = response.Quantity,
            InitialUnit = response.Unit,
            InitialLeadTimeDays = response.LeadTimeDays,
            Rounds = response.NegotiationRounds
                .Select(r => MapToListDto(r, buyerVendorId))
                .ToList()
        };

        history.Summary = BuildSummary(response, vendorId);

        return history;
    }

    public async Task<NegotiationSummaryDto?> GetNegotiationSummaryAsync(int demandResponseId, int vendorId)
    {
        var response = await _db.DemandResponses
            .Include(r => r.Demand)
                .ThenInclude(d => d.Vendor)
            .Include(r => r.SupplierVendor)
            .Include(r => r.CurrentTurnVendor)
            .Include(r => r.NegotiationRounds.OrderByDescending(n => n.RoundNumber).Take(1))
                .ThenInclude(n => n.Initiator)
            .FirstOrDefaultAsync(r => r.Id == demandResponseId && !r.IsDeleted);

        if (response == null)
            return null;

        // Yetki kontrolu
        if (response.Demand.VendorId != vendorId && response.SupplierVendorId != vendorId)
            return null;

        return BuildSummary(response, vendorId);
    }

    public async Task<List<ActiveNegotiationDto>> GetMyActiveNegotiationsAsync(int vendorId)
    {
        var responses = await _db.DemandResponses
            .Include(r => r.Demand)
                .ThenInclude(d => d.Vendor)
            .Include(r => r.SupplierVendor)
            .Include(r => r.NegotiationRounds.OrderByDescending(n => n.RoundNumber).Take(1))
            .Where(r => r.IsNegotiationActive
                && r.CurrentTurnVendorId == vendorId
                && !r.IsDeleted)
            .OrderByDescending(r => r.NegotiationExpiresAt)
            .ToListAsync();

        return responses.Select(r => MapToActiveDto(r, vendorId)).ToList();
    }

    public async Task<List<ActiveNegotiationDto>> GetPendingNegotiationsAsync(int vendorId)
    {
        var responses = await _db.DemandResponses
            .Include(r => r.Demand)
                .ThenInclude(d => d.Vendor)
            .Include(r => r.SupplierVendor)
            .Include(r => r.NegotiationRounds.OrderByDescending(n => n.RoundNumber).Take(1))
            .Where(r => r.IsNegotiationActive
                && r.CurrentTurnVendorId != vendorId
                && (r.Demand.VendorId == vendorId || r.SupplierVendorId == vendorId)
                && !r.IsDeleted)
            .OrderByDescending(r => r.NegotiationExpiresAt)
            .ToListAsync();

        return responses.Select(r => MapToActiveDto(r, vendorId)).ToList();
    }

    // ============================================
    // TIMEOUT ISLEMLERI
    // ============================================

    public async Task<int> ProcessExpiredNegotiationsAsync()
    {
        var expiredResponses = await _db.DemandResponses
            .Include(r => r.NegotiationRounds.Where(n => n.Status == NegotiationRoundStatuses.Ids.Active))
            .Where(r => r.IsNegotiationActive
                && r.NegotiationExpiresAt.HasValue
                && r.NegotiationExpiresAt.Value < DateTime.UtcNow
                && !r.IsDeleted)
            .ToListAsync();

        foreach (var response in expiredResponses)
        {
            await ExpireNegotiationAsync(response, "System");
        }

        return expiredResponses.Count;
    }

    public async Task<bool> CheckAndExpireNegotiationAsync(int demandResponseId)
    {
        var response = await _db.DemandResponses
            .Include(r => r.NegotiationRounds.Where(n => n.Status == NegotiationRoundStatuses.Ids.Active))
            .FirstOrDefaultAsync(r => r.Id == demandResponseId && !r.IsDeleted);

        if (response == null || !response.IsNegotiationActive)
            return false;

        if (!response.NegotiationExpiresAt.HasValue || response.NegotiationExpiresAt.Value >= DateTime.UtcNow)
            return false;

        await ExpireNegotiationAsync(response, "System");
        return true;
    }

    // ============================================
    // PRIVATE HELPERS
    // ============================================

    private async Task ExpireNegotiationAsync(DemandResponse response, string? updatedBy)
    {
        var now = DateTime.UtcNow;

        foreach (var round in response.NegotiationRounds.Where(r => r.Status == NegotiationRoundStatuses.Ids.Active))
        {
            round.Status = NegotiationRoundStatuses.Ids.Expired;
            round.RespondedAt = now;
            round.UpdatedBy = updatedBy;
        }

        response.IsNegotiationActive = false;
        response.CurrentTurnVendorId = null;
        response.NegotiationExpiresAt = null;
        response.UpdatedBy = updatedBy;

        await _db.SaveChangesAsync();
    }

    private NegotiationSummaryDto BuildSummary(DemandResponse response, int viewerVendorId)
    {
        var buyerVendorId = response.Demand.VendorId;
        var lastRound = response.NegotiationRounds.FirstOrDefault();

        var summary = new NegotiationSummaryDto
        {
            DemandResponseId = response.Id,
            IsNegotiationActive = response.IsNegotiationActive,
            CurrentRoundNumber = response.CurrentRoundNumber,
            CurrentTurnVendorId = response.CurrentTurnVendorId,
            CurrentTurnVendorName = response.CurrentTurnVendor?.CompanyName,
            IsMyTurn = response.CurrentTurnVendorId == viewerVendorId,
            ExpiresAt = response.NegotiationExpiresAt,
            IsExpired = response.NegotiationExpiresAt.HasValue && response.NegotiationExpiresAt.Value < DateTime.UtcNow,
            TotalRounds = response.CurrentRoundNumber,
            InitialPrice = response.UnitPrice,
            CurrentPrice = lastRound?.UnitPrice ?? response.UnitPrice,
            Currency = lastRound?.Currency ?? response.Currency ?? "TRY",
            Quantity = lastRound?.Quantity ?? response.Quantity,
            LeadTimeDays = lastRound?.LeadTimeDays ?? response.LeadTimeDays,
            LastRound = lastRound != null ? MapToListDto(lastRound, buyerVendorId) : null
        };

        // Kalan sure hesapla
        if (response.NegotiationExpiresAt.HasValue && response.IsNegotiationActive)
        {
            var remaining = response.NegotiationExpiresAt.Value - DateTime.UtcNow;
            if (remaining.TotalSeconds > 0)
            {
                summary.TimeRemaining = $"{(int)remaining.TotalHours}s {remaining.Minutes}dk";
            }
        }

        // Fiyat degisim yuzdesi
        if (summary.InitialPrice.HasValue && summary.InitialPrice.Value > 0 && summary.CurrentPrice.HasValue)
        {
            summary.PriceChangePercent = ((summary.CurrentPrice.Value - summary.InitialPrice.Value) / summary.InitialPrice.Value) * 100;
        }

        return summary;
    }

    private NegotiationRoundListDto MapToListDto(NegotiationRound round, int buyerVendorId)
    {
        var status = NegotiationRoundStatuses.GetById(round.Status);
        return new NegotiationRoundListDto
        {
            Id = round.Id,
            DemandResponseId = round.DemandResponseId,
            RoundNumber = round.RoundNumber,
            InitiatorVendorId = round.InitiatorVendorId,
            InitiatorName = round.Initiator?.CompanyName,
            IsInitiatorBuyer = round.InitiatorVendorId == buyerVendorId,
            UnitPrice = round.UnitPrice,
            TotalPrice = round.TotalPrice,
            Currency = round.Currency,
            Quantity = round.Quantity,
            Unit = round.Unit,
            LeadTimeDays = round.LeadTimeDays,
            ValidUntil = round.ValidUntil,
            Notes = round.Notes,
            Status = round.Status,
            StatusName = status?.Description ?? "",
            StatusCssClass = status?.CssClass ?? "",
            ExpiresAt = round.ExpiresAt,
            CreatedAt = round.CreatedAt,
            RespondedAt = round.RespondedAt
        };
    }

    private NegotiationRoundDetailDto MapToDetailDto(NegotiationRound round, int buyerVendorId)
    {
        var status = NegotiationRoundStatuses.GetById(round.Status);
        return new NegotiationRoundDetailDto
        {
            Id = round.Id,
            DemandResponseId = round.DemandResponseId,
            RoundNumber = round.RoundNumber,
            InitiatorVendorId = round.InitiatorVendorId,
            InitiatorName = round.Initiator?.CompanyName,
            IsInitiatorBuyer = round.InitiatorVendorId == buyerVendorId,
            UnitPrice = round.UnitPrice,
            TotalPrice = round.TotalPrice,
            Currency = round.Currency,
            Quantity = round.Quantity,
            Unit = round.Unit,
            LeadTimeDays = round.LeadTimeDays,
            ValidUntil = round.ValidUntil,
            Notes = round.Notes,
            TermsAndConditions = round.TermsAndConditions,
            RejectionReason = round.RejectionReason,
            Status = round.Status,
            StatusName = status?.Description ?? "",
            StatusCssClass = status?.CssClass ?? "",
            ExpiresAt = round.ExpiresAt,
            CreatedAt = round.CreatedAt,
            RespondedAt = round.RespondedAt
        };
    }

    private ActiveNegotiationDto MapToActiveDto(DemandResponse response, int viewerVendorId)
    {
        var lastRound = response.NegotiationRounds.FirstOrDefault();
        var isMyTurn = response.CurrentTurnVendorId == viewerVendorId;

        // Karsi taraf kim?
        var counterpartyName = viewerVendorId == response.Demand.VendorId
            ? response.SupplierVendor?.CompanyName
            : response.Demand.Vendor?.CompanyName;

        var status = DemandResponseStatuses.GetById(response.Status);

        var dto = new ActiveNegotiationDto
        {
            DemandResponseId = response.Id,
            DemandId = response.DemandId,
            DemandTitle = response.Demand.Title,
            CounterpartyName = counterpartyName,
            CurrentRoundNumber = response.CurrentRoundNumber,
            CurrentPrice = lastRound?.UnitPrice ?? response.UnitPrice,
            Currency = response.Currency,
            ExpiresAt = response.NegotiationExpiresAt,
            IsMyTurn = isMyTurn,
            Status = response.Status,
            StatusName = status?.Description ?? "",
            StatusCssClass = status?.CssClass ?? ""
        };

        // Kalan sure
        if (response.NegotiationExpiresAt.HasValue && response.IsNegotiationActive)
        {
            var remaining = response.NegotiationExpiresAt.Value - DateTime.UtcNow;
            if (remaining.TotalSeconds > 0)
            {
                dto.TimeRemaining = $"{(int)remaining.TotalHours}s {remaining.Minutes}dk";
            }
        }

        return dto;
    }
}
