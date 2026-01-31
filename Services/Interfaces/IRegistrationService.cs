using Bridgo.DTOs.Auth;

namespace Bridgo.Services.Interfaces;

public interface IRegistrationService
{
    /// <summary>
    /// Yeni firma ve kullanici kaydı
    /// </summary>
    Task<RegistrationResult> RegisterVendorAsync(MultiStepRegisterDto model, int userId);

    /// <summary>
    /// Mevcut firmaya katilma istegi ile kayit
    /// </summary>
    Task<RegistrationResult> RegisterWithJoinRequestAsync(RegisterWithJoinDto model, int userId);

    /// <summary>
    /// Email domain'ine gore firma eslestirme
    /// </summary>
    Task<VendorSearchResult?> CheckDomainMatchAsync(string email);

    /// <summary>
    /// Firma arama (kayit sirasinda)
    /// </summary>
    Task<List<VendorSearchResult>> SearchCompaniesAsync(string query);
}

// Result DTOs
public class RegistrationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? VendorId { get; set; }
    public Dictionary<string, string>? Errors { get; set; }
}

public class VendorSearchResult
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? City { get; set; }
}
