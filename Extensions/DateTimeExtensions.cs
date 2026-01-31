namespace Bridgo.Extensions;

/// <summary>
/// DateTime extension metodlari
/// PostgreSQL timestamp with time zone icin UTC donusum saglar
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// DateTime'i UTC'ye donusturur.
    /// Kind=Unspecified ise UTC olarak isaretler.
    /// Kind=Local ise ToUniversalTime ile donusturur.
    /// Kind=Utc ise olduğu gibi döndürür.
    /// </summary>
    public static DateTime ToUtcSafe(this DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc) // Unspecified -> UTC olarak isaretle
        };
    }

    /// <summary>
    /// Nullable DateTime icin UTC donusum
    /// </summary>
    public static DateTime? ToUtcSafe(this DateTime? dateTime)
    {
        return dateTime?.ToUtcSafe();
    }
}
