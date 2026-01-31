namespace Bridgo.Models.Entities;

/// <summary>
/// Siparise katilan taraflar (Satici + Servis Saglayicilar)
/// Odeme alindiktan sonra her tarafin gorev listesi olusur
/// </summary>
public class OrderParticipant : BaseEntity
{
    /// <summary>
    /// Siparis ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Katilimci firma ID
    /// </summary>
    public int VendorId { get; set; }

    /// <summary>
    /// Katilimci rolu (ParticipantRoles)
    /// 1: Seller (Urun saticisi)
    /// 2: LogisticsProvider (Lojistik)
    /// 3: CustomsBroker (Gumruk)
    /// 4: InsuranceProvider (Sigorta)
    /// 5: Investor (Yatirimci/Finansman)
    /// </summary>
    public int Role { get; set; }

    /// <summary>
    /// Ilgili servis teklifi ID (servis saglayicilar icin)
    /// </summary>
    public int? ServiceQuoteId { get; set; }

    /// <summary>
    /// Bu tarafin alacagi tutar
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Para birimi
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Platform komisyonu
    /// </summary>
    public decimal CommissionAmount { get; set; }

    /// <summary>
    /// Net odeme tutari (Amount - Commission)
    /// </summary>
    public decimal NetAmount { get; set; }

    // === DURUM ===

    /// <summary>
    /// Katilimci durumu (ParticipantStatuses)
    /// 1: Pending (Odeme bekleniyor)
    /// 2: Active (Gorev basladı)
    /// 3: Completed (Tamamlandi)
    /// 4: Disputed (Anlasmazlik)
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// Gorev tamamlandi mi
    /// </summary>
    public bool IsTaskCompleted { get; set; }

    /// <summary>
    /// Gorev tamamlanma tarihi
    /// </summary>
    public DateTime? TaskCompletedAt { get; set; }

    /// <summary>
    /// Odeme yapildi mi
    /// </summary>
    public bool IsPaid { get; set; }

    /// <summary>
    /// Odeme tarihi
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Odeme referansi
    /// </summary>
    public string? PaymentReference { get; set; }

    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual Vendor? Vendor { get; set; }
    public virtual OrderServiceQuote? ServiceQuote { get; set; }
    public virtual ICollection<OrderTask> Tasks { get; set; } = new List<OrderTask>();
}
