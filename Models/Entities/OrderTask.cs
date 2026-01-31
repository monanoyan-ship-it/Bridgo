namespace Bridgo.Models.Entities;

/// <summary>
/// Siparis gorevi - Odeme alindiktan sonra tum taraflara gorev listesi duser
/// </summary>
public class OrderTask : BaseEntity
{
    /// <summary>
    /// Siparis ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Katilimci ID (gorevi yapacak taraf)
    /// </summary>
    public int ParticipantId { get; set; }

    /// <summary>
    /// Gorev basligi
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gorev aciklamasi
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gorev tipi (TaskTypes)
    /// 1: PrepareOrder (Siparis hazirla)
    /// 2: ShipOrder (Kargoya ver)
    /// 3: PrepareDocuments (Evrak hazirla)
    /// 4: CustomsClearance (Gumruk islemi yap)
    /// 5: ArrangeTransport (Tasimayi duzenle)
    /// 6: IssueInsurance (Sigorta policesi duzenle)
    /// 7: ConfirmDelivery (Teslimi onayla)
    /// 8: ProvideTracking (Takip bilgisi ver)
    /// 99: Custom (Ozel gorev)
    /// </summary>
    public int TaskType { get; set; }

    /// <summary>
    /// Gorev sirasi (1, 2, 3...)
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Bagli oldugu gorev ID (bu gorev bitmeden baslayamaz)
    /// </summary>
    public int? DependsOnTaskId { get; set; }

    /// <summary>
    /// Son tarih
    /// </summary>
    public DateTime? DueDate { get; set; }

    // === DURUM ===

    /// <summary>
    /// Gorev durumu (TaskStatuses)
    /// 1: Pending (Bekliyor)
    /// 2: InProgress (Devam ediyor)
    /// 3: Completed (Tamamlandi)
    /// 4: Blocked (Engellendi)
    /// 5: Skipped (Gecildi)
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// Tamamlanma tarihi
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Tamamlayan kullanici
    /// </summary>
    public int? CompletedByUserId { get; set; }

    /// <summary>
    /// Tamamlama notu
    /// </summary>
    public string? CompletionNotes { get; set; }

    // === EK BILGI ===

    /// <summary>
    /// Referans verisi (JSON) - ornegin kargo bilgisi, evrak linki vb.
    /// </summary>
    public string? ReferenceData { get; set; }

    /// <summary>
    /// Zorunlu mu
    /// </summary>
    public bool IsRequired { get; set; } = true;

    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual OrderParticipant? Participant { get; set; }
    public virtual OrderTask? DependsOnTask { get; set; }
}
