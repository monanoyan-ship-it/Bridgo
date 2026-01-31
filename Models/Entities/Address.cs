using System.ComponentModel.DataAnnotations;

namespace Bridgo.Models.Entities;

public class Address : BaseEntity
{
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;

    /// <summary>
    /// Adres tipi (AddressTypes static class'tan ID)
    /// </summary>
    public int AddressTypeId { get; set; } = 1; // Default: Billing

    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;      // Adres basligi (Merkez, Depo, vb.)

    // === ADRES BILGILERI ===

    /// <summary>
    /// Ulke ID (Countries tablosundan)
    /// </summary>
    public int? CountryId { get; set; }
    public Country? CountryEntity { get; set; }

    /// <summary>
    /// Eyalet/Il ID (States tablosundan)
    /// </summary>
    public int? StateId { get; set; }
    public State? StateEntity { get; set; }

    [MaxLength(100)]
    public string City { get; set; } = string.Empty;        // Sehir/Ilce

    [MaxLength(100)]
    public string District { get; set; } = string.Empty;    // Mahalle/Semt

    [Required]
    [MaxLength(90)]
    public string AddressLine { get; set; } = string.Empty;  // Cadde/Sokak, Bina no, kat, daire

    [MaxLength(10)]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Adres tarifi (yol tarifi, belirgin noktalar vb.)
    /// </summary>
    public string? AddressDescription { get; set; }

    // === ILETISIM BILGILERI ===

    [MaxLength(100)]
    public string? ContactName { get; set; }               // Bu adresteki yetkili

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    // === KONUM ===
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // === DURUM ===
    public bool IsDefault { get; set; } = false;           // Varsayilan adres mi
    public bool IsActive { get; set; } = true;

    // Tam adres helper
    public string FullAddress
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(AddressLine)) parts.Add(AddressLine);
            if (!string.IsNullOrEmpty(District)) parts.Add(District);
            if (!string.IsNullOrEmpty(City)) parts.Add(City);
            if (StateEntity != null) parts.Add(StateEntity.Name);
            if (CountryEntity != null) parts.Add(CountryEntity.Name);
            return string.Join(", ", parts);
        }
    }
}
