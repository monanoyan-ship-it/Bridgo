namespace Bridgo.Models.Entities;

/// <summary>
/// Vendor'in sahip oldugu capability'ler (many-to-many)
/// Bir vendor birden fazla capability'ye sahip olabilir
/// CapabilityId, TypeDefinitions.Capabilities'deki ID'lere karsilik gelir
/// </summary>
public class VendorCapabilityMapping : BaseEntity
{
    public int VendorId { get; set; }

    /// <summary>
    /// Capabilities.Ids'den bir deger (Seller=2, Buyer=3, vb.)
    /// </summary>
    public int CapabilityId { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation (sadece Vendor, Capability artik static class)
    public Vendor Vendor { get; set; } = null!;
}
