namespace Bridgo.Models.Entities;

/// <summary>
/// Capability-Modul eslestirmesi (Katman 1: Gorunurluk)
/// Hangi capability hangi modulleri gorebilir/kullanabilir?
/// Admin Panel'den yonetilir
/// CapabilityId, TypeDefinitions.Capabilities'deki ID'lere karsilik gelir
/// </summary>
public class CapabilityModuleMapping
{
    public int Id { get; set; }

    /// <summary>
    /// Capabilities.Ids'den bir deger (Seller=2, Buyer=3, vb.)
    /// </summary>
    public int CapabilityId { get; set; }

    public int PlatformModuleId { get; set; }

    // Navigation (Capability artik static class)
    public PlatformModule PlatformModule { get; set; } = null!;
}
