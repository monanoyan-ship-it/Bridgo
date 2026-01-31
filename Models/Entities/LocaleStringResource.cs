namespace Bridgo.Models.Entities;

public class LocaleStringResource
{
    public int Id { get; set; }  // Auto-increment
    public int LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    public string ResourceName { get; set; } = string.Empty;   // Common.Save, Vendor.CompanyName
    public string ResourceValue { get; set; } = string.Empty;  // Kaydet, Sirket Adi
}
