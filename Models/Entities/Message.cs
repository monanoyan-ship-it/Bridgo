using System.ComponentModel.DataAnnotations;
using Bridgo.Models.Enums;
using Bridgo.Models.Identity;

namespace Bridgo.Models.Entities;

/// <summary>
/// Mesaj konusu/thread - Bir konu altindaki tum mesajlar
/// </summary>
public class MessageThread : BaseEntity
{
    /// <summary>
    /// Konu basligi
    /// </summary>
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Baslatan kullanici/vendor
    /// </summary>
    public int InitiatorVendorId { get; set; }
    public int InitiatorUserId { get; set; }

    /// <summary>
    /// Karsi taraf vendor
    /// </summary>
    public int RecipientVendorId { get; set; }

    /// <summary>
    /// Thread tipi (MessageThreadTypes)
    /// 1: General, 2: Inquiry, 3: Order, 4: Demand, 5: Support
    /// </summary>
    public int ThreadType { get; set; } = MessageThreadTypes.General.Id;

    /// <summary>
    /// Ilgili referans (talep, siparis, urun vb.)
    /// </summary>
    public int? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }

    /// <summary>
    /// Son mesaj tarihi
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// Thread durumu
    /// </summary>
    public bool IsClosed { get; set; } = false;
    public DateTime? ClosedAt { get; set; }

    // === NAVIGATION PROPERTIES ===
    public Vendor? InitiatorVendor { get; set; }
    public ApplicationUser? InitiatorUser { get; set; }
    public Vendor? RecipientVendor { get; set; }
    public ICollection<Message>? Messages { get; set; }
}

/// <summary>
/// Mesaj - Thread icerisindeki tek bir mesaj
/// </summary>
public class Message : BaseEntity
{
    /// <summary>
    /// Hangi thread'e ait
    /// </summary>
    public int ThreadId { get; set; }

    /// <summary>
    /// Mesaji gonderen vendor
    /// </summary>
    public int SenderVendorId { get; set; }

    /// <summary>
    /// Mesaji gonderen kullanici
    /// </summary>
    public int SenderUserId { get; set; }

    /// <summary>
    /// Mesaj icerigi
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Okundu bilgisi
    /// </summary>
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Silindi mi (soft delete her iki taraf icin)
    /// </summary>
    public bool IsDeletedBySender { get; set; } = false;
    public bool IsDeletedByRecipient { get; set; } = false;

    // === NAVIGATION PROPERTIES ===
    public MessageThread? Thread { get; set; }
    public Vendor? SenderVendor { get; set; }
    public ApplicationUser? SenderUser { get; set; }
    public ICollection<MessageAttachment>? Attachments { get; set; }
}

/// <summary>
/// Mesaj eki
/// </summary>
public class MessageAttachment : BaseEntity
{
    public int MessageId { get; set; }

    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    // === NAVIGATION PROPERTIES ===
    public Message? Message { get; set; }
}

/// <summary>
/// Mesaj thread tipleri
/// </summary>
public static class MessageThreadTypes
{
    public static readonly TypeItem General = new(1, "General", "Message.ThreadType.General");
    public static readonly TypeItem Inquiry = new(2, "Inquiry", "Message.ThreadType.Inquiry");
    public static readonly TypeItem Order = new(3, "Order", "Message.ThreadType.Order");
    public static readonly TypeItem Demand = new(4, "Demand", "Message.ThreadType.Demand");
    public static readonly TypeItem Support = new(5, "Support", "Message.ThreadType.Support");

    public static TypeItem[] All => new[] { General, Inquiry, Order, Demand, Support };

    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
}

/// <summary>
/// Mesaj referans tipleri
/// </summary>
public static class MessageReferenceTypes
{
    public static readonly TypeItem Product = new(1, "Product", "Message.Reference.Product");
    public static readonly TypeItem Order = new(2, "Order", "Message.Reference.Order");
    public static readonly TypeItem Demand = new(3, "Demand", "Message.Reference.Demand");
    public static readonly TypeItem Inquiry = new(4, "Inquiry", "Message.Reference.Inquiry");

    public static TypeItem[] All => new[] { Product, Order, Demand, Inquiry };

    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
}
