using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bridgo.Models.Entities;

/// <summary>
/// Uygulama log kayitlari
/// Serilog tarafindan PostgreSQL'e yazilir
/// </summary>
[Table("AppLogs")]
public class AppLog
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Log zamani (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Log seviyesi (Verbose, Debug, Information, Warning, Error, Fatal)
    /// </summary>
    [MaxLength(50)]
    public string? Level { get; set; }

    /// <summary>
    /// Log mesaji
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Mesaj sablonu (Serilog structured logging)
    /// </summary>
    public string? MessageTemplate { get; set; }

    /// <summary>
    /// Hata detaylari (exception stack trace)
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Ek ozellikler (JSON formatinda)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? Properties { get; set; }

    /// <summary>
    /// Makine/Sunucu adi
    /// </summary>
    [MaxLength(100)]
    public string? MachineName { get; set; }

    /// <summary>
    /// Kullanici adi (varsa)
    /// </summary>
    [MaxLength(256)]
    public string? UserName { get; set; }

    /// <summary>
    /// HTTP istek yolu (varsa)
    /// </summary>
    [MaxLength(500)]
    public string? RequestPath { get; set; }

    /// <summary>
    /// HTTP metodu (GET, POST, vb.)
    /// </summary>
    [MaxLength(10)]
    public string? RequestMethod { get; set; }

    /// <summary>
    /// Kaynak dosya/sinif adi
    /// </summary>
    [MaxLength(500)]
    public string? SourceContext { get; set; }

    /// <summary>
    /// Islem/Action adi
    /// </summary>
    [MaxLength(256)]
    public string? ActionName { get; set; }
}
