using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Bridgo.Configuration;
using Bridgo.Data;
using Bridgo.DTOs.Evrim;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Evrim Gumruk API servisi implementasyonu
/// </summary>
public class EvrimApiService : IEvrimApiService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly EvrimApiSettings _settings;
    private readonly ILogger<EvrimApiService> _logger;
    private readonly SemaphoreSlim _rateLimiter = new(1, 1);
    private DateTime _lastRequestTime = DateTime.MinValue;

    private const string TokenCacheKey = "EvrimApiToken";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public EvrimApiService(
        ApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IOptions<EvrimApiSettings> settings,
        ILogger<EvrimApiService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
    }

    #region Authentication

    public async Task<string?> GetTokenAsync()
    {
        // Cache'den token al
        if (_cache.TryGetValue(TokenCacheKey, out string? cachedToken))
            return cachedToken;

        try
        {
            var loginRequest = new EvrimLoginRequest
            {
                KullaniciAdi = _settings.Username,
                Sifre = _settings.Password,
                FirmaKodu = _settings.FirmaKodu
            };

            var response = await SendRequestAsync<EvrimLoginResponse>(
                HttpMethod.Post,
                "/api/auth/login",
                loginRequest,
                useAuth: false);

            if (response?.Basarili == true && !string.IsNullOrEmpty(response.Token))
            {
                // Token'i cache'le
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.TokenExpirationMinutes));
                _cache.Set(TokenCacheKey, response.Token, cacheOptions);

                return response.Token;
            }

            _logger.LogError("Evrim login basarisiz: {HataMesaji}", response?.HataMesaji);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Evrim login istegi sirasinda hata olustu");
            return null;
        }
    }

    public async Task<ConnectionTestResultDto> TestConnectionAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new ConnectionTestResultDto
                {
                    Success = false,
                    Message = "Authentication basarisiz - kimlik bilgilerini kontrol edin",
                    TestTime = DateTime.UtcNow
                };
            }

            return new ConnectionTestResultDto
            {
                Success = true,
                Message = "Evrim API baglantisi basarili",
                ApiVersion = "1.0",
                TestTime = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Evrim baglanti testi basarisiz");
            return new ConnectionTestResultDto
            {
                Success = false,
                Message = $"Baglanti hatasi: {ex.Message}",
                TestTime = DateTime.UtcNow
            };
        }
    }

    #endregion

    #region Beyanname CRUD

    public async Task<EvrimDeclarationCreateResponse> CreateExportDeclarationAsync(int orderId, int vendorId, int userId)
    {
        return await CreateDeclarationInternalAsync(orderId, vendorId, userId, CustomsDeclarationTypes.Ids.Export);
    }

    public async Task<EvrimDeclarationCreateResponse> CreateImportDeclarationAsync(int orderId, int vendorId, int userId)
    {
        return await CreateDeclarationInternalAsync(orderId, vendorId, userId, CustomsDeclarationTypes.Ids.Import);
    }

    public async Task<EvrimDeclarationCreateResponse> CreateETGBDeclarationAsync(int orderId, int vendorId, int userId)
    {
        return await CreateDeclarationInternalAsync(orderId, vendorId, userId, CustomsDeclarationTypes.Ids.ETGB);
    }

    private async Task<EvrimDeclarationCreateResponse> CreateDeclarationInternalAsync(
        int orderId, int vendorId, int userId, int declarationTypeId)
    {
        try
        {
            // Siparis ve ilgili bilgileri getir
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.BuyerVendor)
                .Include(o => o.SellerVendor)
                .Include(o => o.ShippingAddress)
                .FirstOrDefaultAsync(o => o.Id == orderId && (o.BuyerVendorId == vendorId || o.SellerVendorId == vendorId));

            if (order == null)
                return new EvrimDeclarationCreateResponse { Basarili = false, HataMesaji = "Siparis bulunamadi" };

            // Evrim request olustur
            var evrimRequest = BuildDeclarationRequest(order, declarationTypeId);

            // Evrim API'ye gonder
            var endpoint = declarationTypeId == CustomsDeclarationTypes.Ids.ETGB
                ? "/api/beyanname/etgb"
                : "/api/beyanname/olustur";

            var evrimResponse = await SendRequestAsync<EvrimDeclarationCreateResponse>(
                HttpMethod.Post,
                endpoint,
                evrimRequest,
                useAuth: true);

            // Local DB'ye kaydet
            var declaration = new CustomsDeclaration
            {
                OrderId = orderId,
                DeclarationTypeId = declarationTypeId,
                EvrimDosyaNo = evrimResponse?.DosyaNo,
                EvrimDosyaTipi = CustomsDeclarationTypes.ToEvrimDosyaTipi(declarationTypeId),
                DeclarationNumber = evrimResponse?.BeyannameNo,
                StatusId = evrimResponse?.Basarili == true
                    ? CustomsDeclarationStatuses.Ids.Pending
                    : CustomsDeclarationStatuses.Ids.Error,
                RegistrationDate = evrimResponse?.TescilTarihi,
                CustomsOfficeCode = order.CustomsOfficeCode,
                RegimeCode = order.CustomsRegimeCode,
                DeliveryTerms = Incoterms.GetById(order.IncotermId ?? 0)?.SystemName,
                DeliveryPlace = order.IncotermLocation,
                EvrimResponse = JsonSerializer.Serialize(evrimResponse, JsonOptions),
                LastSyncAt = DateTime.UtcNow,
                LastError = evrimResponse?.Basarili == true ? null : evrimResponse?.HataMesaji,
                CreatedByUserId = userId
            };

            _context.CustomsDeclarations.Add(declaration);
            await _context.SaveChangesAsync();

            return evrimResponse ?? new EvrimDeclarationCreateResponse
            {
                Basarili = false,
                HataMesaji = "Evrim API'den yanit alinamadi"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beyanname olusturma sirasinda hata - OrderId: {OrderId}", orderId);
            return new EvrimDeclarationCreateResponse
            {
                Basarili = false,
                HataMesaji = $"Sistem hatasi: {ex.Message}"
            };
        }
    }

    public async Task<EvrimDeclarationStatusResponse> GetDeclarationStatusAsync(string dosyaNo, string dosyaTipi)
    {
        try
        {
            var response = await SendRequestAsync<EvrimDeclarationStatusResponse>(
                HttpMethod.Get,
                $"/api/beyanname/durum?dosyaNo={dosyaNo}&dosyaTipi={dosyaTipi}",
                null,
                useAuth: true);

            return response ?? new EvrimDeclarationStatusResponse
            {
                Basarili = false,
                HataMesaji = "Evrim API'den yanit alinamadi"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beyanname durum sorgusu sirasinda hata - DosyaNo: {DosyaNo}", dosyaNo);
            return new EvrimDeclarationStatusResponse
            {
                Basarili = false,
                HataMesaji = $"Sistem hatasi: {ex.Message}"
            };
        }
    }

    public async Task<bool> SyncDeclarationStatusAsync(int declarationId)
    {
        try
        {
            var declaration = await _context.CustomsDeclarations.FindAsync(declarationId);
            if (declaration == null || string.IsNullOrEmpty(declaration.EvrimDosyaNo))
                return false;

            var statusResponse = await GetDeclarationStatusAsync(
                declaration.EvrimDosyaNo,
                declaration.EvrimDosyaTipi ?? "IHR");

            if (statusResponse.Basarili)
            {
                declaration.StatusId = CustomsDeclarationStatuses.FromEvrimStatus(statusResponse.Durum);
                declaration.DeclarationNumber = statusResponse.BeyannameNo ?? declaration.DeclarationNumber;
                declaration.RegistrationDate = statusResponse.TescilTarihi ?? declaration.RegistrationDate;
                declaration.ClosingDate = statusResponse.KapanisTarihi;
                declaration.CustomsOfficeName = statusResponse.GumrukIdaresiAdi;
                declaration.TaxDue = statusResponse.OdenecekVergi;
                declaration.TaxPaid = statusResponse.OdenenVergi;
                declaration.EvrimResponse = JsonSerializer.Serialize(statusResponse, JsonOptions);
                declaration.LastSyncAt = DateTime.UtcNow;
                declaration.LastError = null;

                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                declaration.LastError = statusResponse.HataMesaji;
                declaration.LastSyncAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beyanname durum senkronizasyonu sirasinda hata - DeclarationId: {DeclarationId}", declarationId);
            return false;
        }
    }

    public async Task SyncAllPendingDeclarationsAsync()
    {
        try
        {
            var activeStatusIds = CustomsDeclarationStatuses.ActiveStatuses.Select(s => s.Id).ToList();

            var pendingDeclarations = await _context.CustomsDeclarations
                .Where(d => !d.IsDeleted
                    && activeStatusIds.Contains(d.StatusId)
                    && !string.IsNullOrEmpty(d.EvrimDosyaNo))
                .ToListAsync();

            _logger.LogInformation("Senkronize edilecek beyanname sayisi: {Count}", pendingDeclarations.Count);

            foreach (var declaration in pendingDeclarations)
            {
                try
                {
                    await SyncDeclarationStatusAsync(declaration.Id);
                    // Rate limiting icin bekle
                    await Task.Delay(_settings.RateLimitMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Beyanname senkronizasyonu sirasinda hata - Id: {Id}", declaration.Id);
                }
            }

            _logger.LogInformation("Beyanname senkronizasyonu tamamlandi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu beyanname senkronizasyonu sirasinda hata");
        }
    }

    #endregion

    #region Internal CRUD

    public async Task<List<CustomsDeclarationListDto>> GetDeclarationsByOrderAsync(int orderId, int vendorId)
    {
        var declarations = await _context.CustomsDeclarations
            .Where(d => d.OrderId == orderId && !d.IsDeleted)
            .Include(d => d.Order)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        // Vendor yetkisi kontrolu
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || (order.BuyerVendorId != vendorId && order.SellerVendorId != vendorId))
            return new List<CustomsDeclarationListDto>();

        return declarations.Select(d => MapToListDto(d)).ToList();
    }

    public async Task<CustomsDeclarationDetailDto?> GetDeclarationDetailAsync(int declarationId, int vendorId)
    {
        var declaration = await _context.CustomsDeclarations
            .Include(d => d.Order)
            .FirstOrDefaultAsync(d => d.Id == declarationId && !d.IsDeleted);

        if (declaration == null)
            return null;

        // Vendor yetkisi kontrolu
        var order = declaration.Order;
        if (order == null || (order.BuyerVendorId != vendorId && order.SellerVendorId != vendorId))
            return null;

        var dto = new CustomsDeclarationDetailDto
        {
            Id = declaration.Id,
            OrderId = declaration.OrderId,
            OrderNumber = order.OrderNumber,
            DeclarationTypeId = declaration.DeclarationTypeId,
            DeclarationTypeText = CustomsDeclarationTypes.GetById(declaration.DeclarationTypeId)?.Description,
            DeclarationTypeIcon = CustomsDeclarationTypes.GetById(declaration.DeclarationTypeId)?.Icon,
            EvrimDosyaNo = declaration.EvrimDosyaNo,
            DeclarationNumber = declaration.DeclarationNumber,
            StatusId = declaration.StatusId,
            StatusText = CustomsDeclarationStatuses.GetById(declaration.StatusId)?.Description,
            StatusCssClass = CustomsDeclarationStatuses.GetById(declaration.StatusId)?.CssClass,
            StatusIcon = CustomsDeclarationStatuses.GetById(declaration.StatusId)?.Icon,
            CustomsOffice = declaration.CustomsOfficeName ?? declaration.CustomsOfficeCode,
            RegimeCode = declaration.RegimeCode,
            RegistrationDate = declaration.RegistrationDate,
            ClosingDate = declaration.ClosingDate,
            LastSyncAt = declaration.LastSyncAt,
            LastError = declaration.LastError,
            CreatedAt = declaration.CreatedAt,
            EvrimResponse = declaration.EvrimResponse,
            DeliveryTerms = declaration.DeliveryTerms,
            DeliveryPlace = declaration.DeliveryPlace,
            TaxDue = declaration.TaxDue,
            TaxPaid = declaration.TaxPaid,
            Notes = declaration.Notes,
            Documents = new List<CustomsDocumentDto>()
        };

        // Evrim'den belge listesini cek (opsiyonel - performans icin lazy load edilebilir)
        if (!string.IsNullOrEmpty(declaration.EvrimDosyaNo))
        {
            try
            {
                var documents = await GetDocumentListAsync(declarationId, vendorId);
                dto.Documents = documents.Select(d => new CustomsDocumentDto
                {
                    DocumentGuid = d.BelgeId,
                    DocumentTypeCode = d.BelgeTipiKodu,
                    DocumentTypeName = d.BelgeTipiAdi,
                    DocumentNumber = d.BelgeNo,
                    DocumentDate = d.BelgeTarihi,
                    FileName = d.DosyaAdi,
                    FileSize = d.DosyaBoyut,
                    UploadedAt = d.YuklemeTarihi
                }).ToList();
                dto.DocumentCount = dto.Documents.Count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Belge listesi alinamadi - DeclarationId: {Id}", declarationId);
            }
        }

        return dto;
    }

    #endregion

    #region Belge Islemleri

    public async Task<EvrimDocumentUploadResponse> UploadDocumentAsync(UploadCustomsDocumentDto request, int vendorId)
    {
        try
        {
            var declaration = await _context.CustomsDeclarations
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.Id == request.DeclarationId && !d.IsDeleted);

            if (declaration == null || string.IsNullOrEmpty(declaration.EvrimDosyaNo))
                return new EvrimDocumentUploadResponse { Basarili = false, HataMesaji = "Beyanname bulunamadi" };

            // Vendor yetkisi kontrolu
            var order = declaration.Order;
            if (order == null || (order.BuyerVendorId != vendorId && order.SellerVendorId != vendorId))
                return new EvrimDocumentUploadResponse { Basarili = false, HataMesaji = "Yetki hatasi" };

            var evrimRequest = new EvrimDocumentUploadRequest
            {
                DosyaNo = declaration.EvrimDosyaNo,
                DosyaTipi = declaration.EvrimDosyaTipi ?? "IHR",
                BelgeTipiKodu = request.DocumentTypeCode,
                BelgeNo = request.DocumentNumber,
                BelgeTarihi = request.DocumentDate,
                DosyaAdi = request.FileName,
                DosyaIcerik = request.FileContent,
                MimeType = request.MimeType,
                Aciklama = request.Description
            };

            var response = await SendRequestAsync<EvrimDocumentUploadResponse>(
                HttpMethod.Post,
                "/api/belge/yukle",
                evrimRequest,
                useAuth: true);

            return response ?? new EvrimDocumentUploadResponse
            {
                Basarili = false,
                HataMesaji = "Evrim API'den yanit alinamadi"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge yukleme sirasinda hata - DeclarationId: {Id}", request.DeclarationId);
            return new EvrimDocumentUploadResponse
            {
                Basarili = false,
                HataMesaji = $"Sistem hatasi: {ex.Message}"
            };
        }
    }

    public async Task<List<EvrimDocumentListItem>> GetDocumentListAsync(int declarationId, int vendorId)
    {
        try
        {
            var declaration = await _context.CustomsDeclarations
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.Id == declarationId && !d.IsDeleted);

            if (declaration == null || string.IsNullOrEmpty(declaration.EvrimDosyaNo))
                return new List<EvrimDocumentListItem>();

            // Vendor yetkisi kontrolu
            var order = declaration.Order;
            if (order == null || (order.BuyerVendorId != vendorId && order.SellerVendorId != vendorId))
                return new List<EvrimDocumentListItem>();

            var response = await SendRequestAsync<List<EvrimDocumentListItem>>(
                HttpMethod.Get,
                $"/api/belge/liste?dosyaNo={declaration.EvrimDosyaNo}&dosyaTipi={declaration.EvrimDosyaTipi}",
                null,
                useAuth: true);

            return response ?? new List<EvrimDocumentListItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge listesi alinamadi - DeclarationId: {Id}", declarationId);
            return new List<EvrimDocumentListItem>();
        }
    }

    public async Task<EvrimDocumentDownloadResponse> DownloadDocumentAsync(string guid, int vendorId)
    {
        try
        {
            // Not: Burada sadece guid ile indirme yapilir, vendor kontrolu opsiyonel
            var response = await SendRequestAsync<EvrimDocumentDownloadResponse>(
                HttpMethod.Get,
                $"/api/belge/indir?guid={guid}",
                null,
                useAuth: true);

            return response ?? new EvrimDocumentDownloadResponse
            {
                Basarili = false,
                HataMesaji = "Evrim API'den yanit alinamadi"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge indirme sirasinda hata - Guid: {Guid}", guid);
            return new EvrimDocumentDownloadResponse
            {
                Basarili = false,
                HataMesaji = $"Sistem hatasi: {ex.Message}"
            };
        }
    }

    #endregion

    #region Helper Methods

    private async Task<T?> SendRequestAsync<T>(HttpMethod method, string endpoint, object? body, bool useAuth)
        where T : class
    {
        // Rate limiting
        await _rateLimiter.WaitAsync();
        try
        {
            var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
            if (timeSinceLastRequest.TotalMilliseconds < _settings.RateLimitMs)
            {
                await Task.Delay(_settings.RateLimitMs - (int)timeSinceLastRequest.TotalMilliseconds);
            }

            var client = _httpClientFactory.CreateClient("EvrimApi");
            client.BaseAddress = new Uri(_settings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

            var request = new HttpRequestMessage(method, endpoint);

            if (useAuth)
            {
                var token = await GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, JsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            _lastRequestTime = DateTime.UtcNow;
            var response = await client.SendAsync(request);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<T>(responseContent, JsonOptions);
            }

            _logger.LogWarning("Evrim API hatasi: {StatusCode} - {Content}", response.StatusCode, responseContent);
            return null;
        }
        finally
        {
            _rateLimiter.Release();
        }
    }

    private EvrimDeclarationCreateRequest BuildDeclarationRequest(Order order, int declarationTypeId)
    {
        var isExport = declarationTypeId == CustomsDeclarationTypes.Ids.Export ||
                       declarationTypeId == CustomsDeclarationTypes.Ids.ETGB;

        var request = new EvrimDeclarationCreateRequest
        {
            DosyaTipi = CustomsDeclarationTypes.ToEvrimDosyaTipi(declarationTypeId),
            RejimKodu = order.CustomsRegimeCode ?? (isExport ? "1000" : "4000"),
            GumrukIdaresiKodu = order.CustomsOfficeCode ?? "3400",
            TeslimSekli = Incoterms.GetById(order.IncotermId ?? Incoterms.Ids.FOB)?.SystemName ?? "FOB",
            TeslimYeri = order.IncotermLocation,
            TasimaSekliKodu = order.TransportModeCode ?? 4, // 4: Havayolu default
            TasimaAraciKimlik = order.TransportIdentity,
            KonteynerVar = false,
            ToplamFaturaTutar = order.TotalAmount,
            FaturaDovizi = order.Currency ?? "USD",
            ToplamBrutAgirlik = order.Items.Sum(i => (i.Product?.Weight ?? 0) * i.Quantity),
            ToplamNetAgirlik = order.Items.Sum(i => (i.Product?.Weight ?? 0) * i.Quantity * 0.95m),
            ToplamKapAdedi = order.Items.Sum(i => i.Quantity)
        };

        // Ihracatci/Ithalatci bilgileri
        if (isExport)
        {
            request.Ihracatci = BuildCompanyDto(order.SellerVendor);
            request.Ithalatci = BuildCompanyDto(order.BuyerVendor);
            request.VarisUlkeKodu = order.ShippingAddress?.CountryEntity?.Iso2Code ?? "US";
        }
        else
        {
            request.Ihracatci = BuildCompanyDto(order.BuyerVendor);
            request.Ithalatci = BuildCompanyDto(order.SellerVendor);
            request.VarisUlkeKodu = "TR";
        }

        // Kalemler
        int siraNo = 1;
        foreach (var item in order.Items)
        {
            var product = item.Product;
            if (product == null) continue;

            request.Kalemler.Add(new EvrimDeclarationItemDto
            {
                SiraNo = siraNo++,
                GtipKodu = product.HSCode ?? "00000000",
                TicariTanim = product.ShortDescription ?? product.Name,
                MenseUlkeKodu = product.CountryOfOrigin ?? "TR",
                Miktar = item.Quantity,
                OlcuBirimi = item.Unit ?? "AD",
                BrutAgirlik = (product.Weight ?? 0) * item.Quantity,
                NetAgirlik = (product.Weight ?? 0) * item.Quantity * 0.95m,
                FaturaTutari = item.TotalPrice,
                FaturaDovizi = order.Currency ?? "USD",
                Marka = product.Brand,
                Model = product.Model
            });
        }

        return request;
    }

    private EvrimDeclarationCompanyDto BuildCompanyDto(Vendor? vendor)
    {
        if (vendor == null)
            return new EvrimDeclarationCompanyDto();

        return new EvrimDeclarationCompanyDto
        {
            VergiNo = vendor.TaxNumber ?? "",
            Unvan = vendor.CompanyName,
            Adres = null, // Address entity'sinden alinabilir
            UlkeKodu = "TR", // Default
            Telefon = vendor.Phone,
            Eposta = vendor.Email
        };
    }

    private CustomsDeclarationListDto MapToListDto(CustomsDeclaration declaration)
    {
        var typeItem = CustomsDeclarationTypes.GetById(declaration.DeclarationTypeId);
        var statusItem = CustomsDeclarationStatuses.GetById(declaration.StatusId);

        return new CustomsDeclarationListDto
        {
            Id = declaration.Id,
            OrderId = declaration.OrderId,
            OrderNumber = declaration.Order?.OrderNumber,
            DeclarationTypeId = declaration.DeclarationTypeId,
            DeclarationTypeText = typeItem?.Description,
            DeclarationTypeIcon = typeItem?.Icon,
            EvrimDosyaNo = declaration.EvrimDosyaNo,
            DeclarationNumber = declaration.DeclarationNumber,
            StatusId = declaration.StatusId,
            StatusText = statusItem?.Description,
            StatusCssClass = statusItem?.CssClass,
            StatusIcon = statusItem?.Icon,
            CustomsOffice = declaration.CustomsOfficeName ?? declaration.CustomsOfficeCode,
            RegimeCode = declaration.RegimeCode,
            RegistrationDate = declaration.RegistrationDate,
            ClosingDate = declaration.ClosingDate,
            LastSyncAt = declaration.LastSyncAt,
            LastError = declaration.LastError,
            CreatedAt = declaration.CreatedAt
        };
    }

    #endregion
}
