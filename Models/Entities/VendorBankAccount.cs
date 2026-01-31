namespace Bridgo.Models.Entities;

/// <summary>
/// Vendor banka hesabi bilgileri
/// </summary>
public class VendorBankAccount : BaseEntity
{
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public string BankName { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public string? SwiftCode { get; set; }
    public string? BranchCode { get; set; }
    public string? AccountNumber { get; set; }

    public bool IsDefault { get; set; }
    public bool IsVerified { get; set; }
}
