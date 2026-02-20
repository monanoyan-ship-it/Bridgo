using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

/// <summary>
/// Firma profil sayfasindan gonderilen iletisim mesajlari.
/// Ziyaretciler (giris yapmamis dahil) firmaya mesaj gonderebilir.
/// </summary>
public class ProfileContactRequest : BaseEntity
{
    /// <summary>
    /// Mesajin gonderildigi profil
    /// </summary>
    public int CapabilityProfileId { get; set; }

    /// <summary>
    /// Giris yapmis kullanicinin firma ID'si (opsiyonel)
    /// </summary>
    public int? SenderVendorId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SenderName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string SenderEmail { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? SenderPhone { get; set; }

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // === NAVIGATION ===
    public CapabilityProfile CapabilityProfile { get; set; } = null!;
}
