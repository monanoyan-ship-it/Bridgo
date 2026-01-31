using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bridgo.Authorization;
using Bridgo.Data;
using Bridgo.DTOs.Vendor;
using Bridgo.Models.Entities;
using Bridgo.Models.Identity;
using Bridgo.Services.Interfaces;

namespace Bridgo.Controllers.Api;

/// <summary>
/// Company Panel API Controller
/// </summary>
[Authorize]
[ApiController]
[Route("api/company")]
public class CompanyApiController : ControllerBase
{
    private readonly IVendorService _vendorService;
    private readonly ICompanyService _companyService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public CompanyApiController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _vendorService = vendorService;
        _companyService = companyService;
        _userManager = userManager;
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task<int?> GetUserVendorIdAsync()
    {
        var userId = GetUserId();
        if (userId == 0) return null;
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.VendorId;
    }

    /// <summary>
    /// Firma bilgilerini getir
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<VendorDto>> Get()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var vendor = await _vendorService.GetByIdAsync(vendorId.Value);
        if (vendor == null)
            return NotFound();

        return Ok(vendor);
    }

    /// <summary>
    /// Firma bilgilerini guncelle
    /// </summary>
    [HttpPut]
    public async Task<ActionResult> Update([FromBody] VendorDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId.ToString());

        var success = await _vendorService.UpdateVendorAsync(vendorId.Value, dto, user?.Email);
        if (!success)
            return BadRequest(new { message = "Guncelleme basarisiz" });

        return Ok(new { success = true, message = "Firma bilgileri guncellendi" });
    }

    /// <summary>
    /// Firma adreslerini listele
    /// </summary>
    [HttpGet("addresses")]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetAddresses()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var addresses = await _vendorService.GetAddressesByVendorIdAsync(vendorId.Value);
        return Ok(addresses);
    }

    /// <summary>
    /// Firma capability'lerini kaydet (ilk kurulum icin)
    /// </summary>
    [HttpPost("capabilities")]
    public async Task<ActionResult> SaveCapabilities([FromBody] SaveCapabilitiesRequest request)
    {
        if (request?.CapabilityIds == null || !request.CapabilityIds.Any())
            return BadRequest(new { success = false, message = "En az bir kullanim amaci secmelisiniz." });

        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return BadRequest(new { success = false, message = "Firma bulunamadi." });

        var added = await _companyService.AddVendorCapabilitiesAsync(vendorId.Value, request.CapabilityIds);

        if (added == 0)
            return BadRequest(new { success = false, message = "Secilen roller eklenemedi. Lutfen tekrar deneyin." });

        return Ok(new { success = true, message = $"{added} kullanim amaci eklendi." });
    }

    /// <summary>
    /// Firma capability'lerini getir
    /// </summary>
    [HttpGet("capabilities")]
    public async Task<ActionResult<IEnumerable<VendorCapabilityDto>>> GetCapabilities()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var capabilities = await _companyService.GetVendorCapabilitiesAsync(vendorId.Value);
        return Ok(capabilities);
    }

    #region Geography (Countries & States)

    /// <summary>
    /// Tum ulkeleri listele
    /// </summary>
    [AllowAnonymous]
    [HttpGet("geography/countries")]
    public async Task<ActionResult> GetCountries()
    {
        var countries = await _context.Countries
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Iso2Code,
                c.PhoneCode,
                c.FlagEmoji
            })
            .ToListAsync();

        return Ok(countries);
    }

    /// <summary>
    /// Ulkeye gore eyaletleri listele
    /// </summary>
    [AllowAnonymous]
    [HttpGet("geography/countries/{countryId}/states")]
    public async Task<ActionResult> GetStatesByCountry(int countryId)
    {
        var states = await _context.States
            .Where(s => s.CountryId == countryId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Code,
                s.Type
            })
            .ToListAsync();

        return Ok(states);
    }

    #endregion

    #region Address CRUD

    /// <summary>
    /// Yeni adres ekle
    /// </summary>
    [HttpPost("addresses")]
    public async Task<ActionResult> CreateAddress([FromBody] AddressCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        // AddressLine validasyonu
        if (string.IsNullOrWhiteSpace(dto.AddressLine))
            return BadRequest(new { message = "Adres satiri zorunludur" });

        var address = new Address
        {
            VendorId = vendorId.Value,
            AddressTypeId = dto.AddressTypeId,
            Title = dto.Title,
            CountryId = dto.CountryId,
            StateId = dto.StateId,
            City = dto.City,
            District = dto.District,
            AddressLine = dto.AddressLine,
            PostalCode = dto.PostalCode,
            AddressDescription = dto.AddressDescription,
            ContactName = dto.ContactName,
            ContactPhone = dto.ContactPhone,
            IsDefault = dto.IsDefault,
            IsActive = true,
            CreatedBy = User.Identity?.Name
        };

        // Eger bu varsayilan olarak isteniyorsa diger varsayilani kaldir
        if (dto.IsDefault)
        {
            var existingDefaults = await _context.Addresses
                .Where(a => a.VendorId == vendorId.Value && a.IsDefault && !a.IsDeleted)
                .ToListAsync();
            foreach (var def in existingDefaults)
                def.IsDefault = false;
        }

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Adres eklendi", id = address.Id });
    }

    /// <summary>
    /// Adres guncelle
    /// </summary>
    [HttpPut("addresses/{id}")]
    public async Task<ActionResult> UpdateAddress(int id, [FromBody] AddressCreateDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        // AddressLine validasyonu
        if (string.IsNullOrWhiteSpace(dto.AddressLine))
            return BadRequest(new { message = "Adres satiri zorunludur" });

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.VendorId == vendorId.Value && !a.IsDeleted);

        if (address == null)
            return NotFound(new { message = "Adres bulunamadi" });

        address.AddressTypeId = dto.AddressTypeId;
        address.Title = dto.Title;
        address.CountryId = dto.CountryId;
        address.StateId = dto.StateId;
        address.City = dto.City;
        address.District = dto.District;
        address.AddressLine = dto.AddressLine;
        address.AddressDescription = dto.AddressDescription;
        address.PostalCode = dto.PostalCode;
        address.ContactName = dto.ContactName;
        address.ContactPhone = dto.ContactPhone;
        address.UpdatedAt = DateTime.UtcNow;
        address.UpdatedBy = User.Identity?.Name;

        // Varsayilan degisikligi
        if (dto.IsDefault && !address.IsDefault)
        {
            var existingDefaults = await _context.Addresses
                .Where(a => a.VendorId == vendorId.Value && a.IsDefault && a.Id != id && !a.IsDeleted)
                .ToListAsync();
            foreach (var def in existingDefaults)
                def.IsDefault = false;
        }
        address.IsDefault = dto.IsDefault;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Adres guncellendi" });
    }

    /// <summary>
    /// Adres sil (soft delete)
    /// </summary>
    [HttpDelete("addresses/{id}")]
    public async Task<ActionResult> DeleteAddress(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.VendorId == vendorId.Value && !a.IsDeleted);

        if (address == null)
            return NotFound(new { message = "Adres bulunamadi" });

        if (address.IsDefault)
            return BadRequest(new { message = "Varsayilan adres silinemez" });

        address.IsDeleted = true;
        address.UpdatedAt = DateTime.UtcNow;
        address.UpdatedBy = User.Identity?.Name;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Adres silindi" });
    }

    #endregion

    #region Billing Info

    /// <summary>
    /// Fatura bilgilerini getir
    /// </summary>
    [HttpGet("billing-info")]
    public async Task<ActionResult> GetBillingInfo()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var vendor = await _context.Vendors.FindAsync(vendorId.Value);
        if (vendor == null)
            return NotFound();

        return Ok(new
        {
            companyName = vendor.CompanyName,
            taxOffice = vendor.TaxOffice,
            taxNumber = vendor.TaxNumber,
            billingAddressId = vendor.BillingAddressId
        });
    }

    /// <summary>
    /// Fatura bilgilerini guncelle
    /// </summary>
    [HttpPut("billing-info")]
    public async Task<ActionResult> UpdateBillingInfo([FromBody] BillingInfoDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var vendor = await _context.Vendors.FindAsync(vendorId.Value);
        if (vendor == null)
            return NotFound();

        // Adres kontrolu - secilen adres bu firmaya ait mi?
        if (dto.BillingAddressId.HasValue)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == dto.BillingAddressId.Value && a.VendorId == vendorId.Value && !a.IsDeleted);
            if (address == null)
                return BadRequest(new { message = "Secilen adres bulunamadi" });
        }

        vendor.CompanyName = dto.CompanyName;
        vendor.TaxOffice = dto.TaxOffice;
        vendor.TaxNumber = dto.TaxNumber;
        vendor.BillingAddressId = dto.BillingAddressId;
        vendor.UpdatedAt = DateTime.UtcNow;
        vendor.UpdatedBy = User.Identity?.Name;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Fatura bilgileri kaydedildi" });
    }

    #endregion

    #region Bank Accounts

    /// <summary>
    /// Banka hesaplarini listele
    /// </summary>
    [HttpGet("bank-accounts")]
    public async Task<ActionResult> GetBankAccounts()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var accounts = await _context.VendorBankAccounts
            .Where(b => b.VendorId == vendorId.Value && !b.IsDeleted)
            .OrderByDescending(b => b.IsDefault)
            .ThenBy(b => b.BankName)
            .Select(b => new
            {
                id = b.Id,
                bankName = b.BankName,
                accountHolder = b.AccountHolder,
                iban = b.IBAN,
                swiftCode = b.SwiftCode,
                isDefault = b.IsDefault,
                isVerified = b.IsVerified
            })
            .ToListAsync();

        return Ok(accounts);
    }

    /// <summary>
    /// Banka hesabi ekle
    /// </summary>
    [HttpPost("bank-accounts")]
    public async Task<ActionResult> AddBankAccount([FromBody] BankAccountDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.BankName) || string.IsNullOrWhiteSpace(dto.IBAN))
            return BadRequest(new { message = "Banka adi ve IBAN zorunludur" });

        // Eger varsayilan olarak isaretlenmisse diger varsayilanlari kaldir
        if (dto.IsDefault)
        {
            var existingDefaults = await _context.VendorBankAccounts
                .Where(b => b.VendorId == vendorId.Value && b.IsDefault && !b.IsDeleted)
                .ToListAsync();
            foreach (var def in existingDefaults)
                def.IsDefault = false;
        }

        var account = new VendorBankAccount
        {
            VendorId = vendorId.Value,
            BankName = dto.BankName,
            AccountHolder = dto.AccountHolder ?? string.Empty,
            IBAN = dto.IBAN,
            SwiftCode = dto.SwiftCode,
            IsDefault = dto.IsDefault,
            CreatedBy = User.Identity?.Name
        };

        _context.VendorBankAccounts.Add(account);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = account.Id,
            bankName = account.BankName,
            accountHolder = account.AccountHolder,
            iban = account.IBAN,
            isDefault = account.IsDefault
        });
    }

    /// <summary>
    /// Banka hesabi guncelle
    /// </summary>
    [HttpPut("bank-accounts/{id}")]
    public async Task<ActionResult> UpdateBankAccount(int id, [FromBody] BankAccountDto dto)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var account = await _context.VendorBankAccounts
            .FirstOrDefaultAsync(b => b.Id == id && b.VendorId == vendorId.Value && !b.IsDeleted);

        if (account == null)
            return NotFound(new { message = "Banka hesabi bulunamadi" });

        if (string.IsNullOrWhiteSpace(dto.BankName) || string.IsNullOrWhiteSpace(dto.IBAN))
            return BadRequest(new { message = "Banka adi ve IBAN zorunludur" });

        // Eger varsayilan olarak isaretlenmisse diger varsayilanlari kaldir
        if (dto.IsDefault && !account.IsDefault)
        {
            var existingDefaults = await _context.VendorBankAccounts
                .Where(b => b.VendorId == vendorId.Value && b.IsDefault && b.Id != id && !b.IsDeleted)
                .ToListAsync();
            foreach (var def in existingDefaults)
                def.IsDefault = false;
        }

        account.BankName = dto.BankName;
        account.AccountHolder = dto.AccountHolder ?? string.Empty;
        account.IBAN = dto.IBAN;
        account.SwiftCode = dto.SwiftCode;
        account.IsDefault = dto.IsDefault;
        account.UpdatedAt = DateTime.UtcNow;
        account.UpdatedBy = User.Identity?.Name;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = account.Id,
            bankName = account.BankName,
            accountHolder = account.AccountHolder,
            iban = account.IBAN,
            swiftCode = account.SwiftCode,
            isDefault = account.IsDefault
        });
    }

    /// <summary>
    /// Banka hesabi sil
    /// </summary>
    [HttpDelete("bank-accounts/{id}")]
    public async Task<ActionResult> DeleteBankAccount(int id)
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        var account = await _context.VendorBankAccounts
            .FirstOrDefaultAsync(b => b.Id == id && b.VendorId == vendorId.Value && !b.IsDeleted);

        if (account == null)
            return NotFound(new { message = "Banka hesabi bulunamadi" });

        account.IsDeleted = true;
        account.UpdatedAt = DateTime.UtcNow;
        account.UpdatedBy = User.Identity?.Name;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Banka hesabi silindi" });
    }

    #endregion

    #region Payments

    /// <summary>
    /// Odeme gecmisini listele
    /// </summary>
    [HttpGet("payments")]
    public async Task<ActionResult> GetPayments()
    {
        var vendorId = await GetUserVendorIdAsync();
        if (!vendorId.HasValue)
            return Forbid();

        // Firmanin siparislerine ait odemeleri getir
        var payments = await _context.StripePayments
            .Include(p => p.Order)
            .Where(p => p.Order != null && p.Order.SellerVendorId == vendorId.Value)
            .OrderByDescending(p => p.CreatedAt)
            .Take(50)
            .Select(p => new
            {
                id = p.Id,
                date = p.PaidAt ?? p.CreatedAt,
                description = p.Order != null ? "Siparis #" + p.OrderId : "Odeme",
                amount = p.Amount,
                currency = p.Currency,
                status = p.Status ?? "pending"
            })
            .ToListAsync();

        return Ok(payments);
    }

    #endregion
}

/// <summary>
/// Fatura bilgileri DTO
/// </summary>
public class BillingInfoDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? TaxOffice { get; set; }
    public string? TaxNumber { get; set; }
    public int? BillingAddressId { get; set; }
}

/// <summary>
/// Banka hesabi DTO
/// </summary>
public class BankAccountDto
{
    public string BankName { get; set; } = string.Empty;
    public string? AccountHolder { get; set; }
    public string IBAN { get; set; } = string.Empty;
    public string? SwiftCode { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// Adres olusturma/guncelleme DTO
/// </summary>
public class AddressCreateDto
{
    public int AddressTypeId { get; set; } = 1;  // Default: Billing
    public string Title { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public int? StateId { get; set; }           // Eyalet ID (ABD, Almanya vb. icin)
    public string City { get; set; } = string.Empty;  // Il
    public string District { get; set; } = string.Empty;  // Ilce
    public string AddressLine { get; set; } = string.Empty;
    public string? AddressDescription { get; set; }
    public string? PostalCode { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsDefault { get; set; }
}

public class SaveCapabilitiesRequest
{
    public List<int> CapabilityIds { get; set; } = new();
}
