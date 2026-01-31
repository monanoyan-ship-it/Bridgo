using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Notification;
using Bridgo.DTOs.ProductInquiry;
using Bridgo.Extensions;
using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class ProductInquiryService : IProductInquiryService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public ProductInquiryService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    // ============================================
    // BUYER (ALICI) OPERATIONS
    // ============================================

    public async Task<ProductInquirySearchResultDto> GetBuyerInquiriesAsync(int buyerVendorId, ProductInquiryFilterDto filter)
    {
        var query = _context.ProductInquiries
            .Include(i => i.Product)
            .Include(i => i.SellerVendor)
            .Include(i => i.Responses)
            .Where(i => i.BuyerVendorId == buyerVendorId)
            .AsQueryable();

        // Filtreler
        if (filter.Status.HasValue)
            query = query.Where(i => i.Status == filter.Status);

        if (!string.IsNullOrEmpty(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(i => i.Product != null && i.Product.Name.ToLower().Contains(search));
        }

        if (filter.HasResponse.HasValue)
        {
            if (filter.HasResponse.Value)
                query = query.Where(i => i.Responses.Any());
            else
                query = query.Where(i => !i.Responses.Any());
        }

        var totalCount = await query.CountAsync();

        query = filter.SortDescending
            ? query.OrderByDescending(i => i.CreatedAt)
            : query.OrderBy(i => i.CreatedAt);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(i => new ProductInquiryListDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product != null ? i.Product.Name : null,
                ProductSku = i.Product != null ? i.Product.SKU : null,
                ProductImageUrl = i.Product != null && i.Product.Images.Any() ? i.Product.Images.First().Url : null,
                BuyerVendorId = i.BuyerVendorId,
                SellerVendorId = i.SellerVendorId,
                SellerCompanyName = i.SellerVendor != null ? i.SellerVendor.CompanyName : null,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Message = i.Message,
                Status = i.Status,
                StatusText = GetStatusText(i.Status),
                IsReadBySeller = i.IsReadBySeller,
                ResponseCount = i.Responses.Count,
                HasResponse = i.Responses.Any(),
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        return new ProductInquirySearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<ProductInquiryDetailDto?> GetInquiryForBuyerAsync(int inquiryId, int buyerVendorId)
    {
        var inquiry = await _context.ProductInquiries
            .Include(i => i.Product)
            .Include(i => i.SellerVendor)
            .Include(i => i.DeliveryAddress)
                .ThenInclude(a => a!.CountryEntity)
            .Include(i => i.Responses)
            .FirstOrDefaultAsync(i => i.Id == inquiryId && i.BuyerVendorId == buyerVendorId);

        if (inquiry == null) return null;

        return MapToDetailDto(inquiry);
    }

    public async Task<ProductInquiryDetailDto> CreateInquiryAsync(int buyerVendorId, ProductInquiryCreateDto dto, string? createdBy)
    {
        // Urun bilgisini al
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

        if (product == null)
            throw new InvalidOperationException("Urun bulunamadi.");

        if (product.VendorId == buyerVendorId)
            throw new InvalidOperationException("Kendi urunleriniz icin istek olusturulamaz.");

        var inquiry = new ProductInquiry
        {
            ProductId = dto.ProductId,
            BuyerVendorId = buyerVendorId,
            SellerVendorId = product.VendorId,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            Message = dto.Message,
            SpecialRequirements = dto.SpecialRequirements,
            // Teslimat adresi: DTO'dan gelen bilgilerle yeni adres oluşturulabilir
            // Şimdilik null bırakıyoruz, adres seçimi UI'da yapılabilir
            DeliveryAddressId = null,
            DesiredDeliveryDate = dto.DesiredDeliveryDate.ToUtcSafe(),
            Status = ProductInquiryStatuses.Pending.Id,
            CreatedBy = createdBy
        };

        _context.ProductInquiries.Add(inquiry);
        await _context.SaveChangesAsync();

        // Saticiya bildirim gonder
        var buyerVendor = await _context.Vendors.FindAsync(buyerVendorId);
        await _notificationService.CreateAsync(new NotificationCreateDto
        {
            VendorId = product.VendorId,
            UserId = null,
            Type = NotificationType.NewProductInquiry,
            Title = "Yeni Urun Istegi",
            Message = $"{buyerVendor?.CompanyName ?? "Bir alici"} '{product.Name}' urunu icin fiyat istegi gonderdi.",
            EntityType = "ProductInquiry",
            EntityId = inquiry.Id,
            ActionUrl = "/Demands/SupplierOffers",
            Icon = "bi-cart-plus"
        });

        // Reload with includes
        var result = await GetInquiryForBuyerAsync(inquiry.Id, buyerVendorId);
        return result!;
    }

    public async Task<bool> CancelInquiryAsync(int inquiryId, int buyerVendorId, string? updatedBy)
    {
        var inquiry = await _context.ProductInquiries
            .FirstOrDefaultAsync(i => i.Id == inquiryId && i.BuyerVendorId == buyerVendorId);

        if (inquiry == null) return false;

        if (inquiry.Status == ProductInquiryStatuses.Accepted.Id)
            throw new InvalidOperationException("Kabul edilmis istek iptal edilemez.");

        inquiry.Status = ProductInquiryStatuses.Cancelled.Id;
        inquiry.UpdatedBy = updatedBy;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AcceptResponseAsync(int responseId, int buyerVendorId, string? updatedBy)
    {
        var response = await _context.ProductInquiryResponses
            .Include(r => r.Inquiry)
            .FirstOrDefaultAsync(r => r.Id == responseId && r.Inquiry != null && r.Inquiry.BuyerVendorId == buyerVendorId);

        if (response == null) return false;

        response.Status = ProductInquiryResponseStatuses.Accepted.Id;
        response.UpdatedBy = updatedBy;

        // Ana istegi de guncelle
        response.Inquiry!.Status = ProductInquiryStatuses.Accepted.Id;
        response.Inquiry.UpdatedBy = updatedBy;

        // Diger yanitlari reddet
        var otherResponses = await _context.ProductInquiryResponses
            .Where(r => r.InquiryId == response.InquiryId && r.Id != responseId && r.Status == ProductInquiryResponseStatuses.Pending.Id)
            .ToListAsync();

        foreach (var other in otherResponses)
        {
            other.Status = ProductInquiryResponseStatuses.Rejected.Id;
        }

        await _context.SaveChangesAsync();

        // Saticiya bildirim
        await _notificationService.CreateAsync(new NotificationCreateDto
        {
            VendorId = response.Inquiry.SellerVendorId,
            UserId = null,
            Type = NotificationType.ResponseAccepted,
            Title = "Teklifiniz Kabul Edildi",
            Message = "Gondermis oldugunuz teklif alici tarafindan kabul edildi.",
            EntityType = "ProductInquiryResponse",
            EntityId = response.Id,
            ActionUrl = "/Demands/SupplierOffers",
            Icon = "bi-check-circle"
        });

        return true;
    }

    public async Task<bool> RejectResponseAsync(int responseId, int buyerVendorId, string? notes, string? updatedBy)
    {
        var response = await _context.ProductInquiryResponses
            .Include(r => r.Inquiry)
            .FirstOrDefaultAsync(r => r.Id == responseId && r.Inquiry != null && r.Inquiry.BuyerVendorId == buyerVendorId);

        if (response == null) return false;

        response.Status = ProductInquiryResponseStatuses.Rejected.Id;
        response.UpdatedBy = updatedBy;

        // Ana istegi de guncelle (rejected)
        response.Inquiry!.Status = ProductInquiryStatuses.Rejected.Id;
        response.Inquiry.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<BuyerInquiryStatsDto> GetBuyerStatsAsync(int buyerVendorId)
    {
        var inquiries = await _context.ProductInquiries
            .Where(i => i.BuyerVendorId == buyerVendorId)
            .ToListAsync();

        return new BuyerInquiryStatsDto
        {
            TotalInquiries = inquiries.Count,
            PendingInquiries = inquiries.Count(i => i.Status == ProductInquiryStatuses.Pending.Id || i.Status == ProductInquiryStatuses.Viewed.Id),
            RespondedInquiries = inquiries.Count(i => i.Status == ProductInquiryStatuses.Responded.Id),
            AcceptedOffers = inquiries.Count(i => i.Status == ProductInquiryStatuses.Accepted.Id)
        };
    }

    // ============================================
    // SELLER (SATICI) OPERATIONS
    // ============================================

    public async Task<ProductInquirySearchResultDto> GetSellerInquiriesAsync(int sellerVendorId, ProductInquiryFilterDto filter)
    {
        var query = _context.ProductInquiries
            .Include(i => i.Product)
            .Include(i => i.BuyerVendor)
            .Include(i => i.Responses)
            .Where(i => i.SellerVendorId == sellerVendorId)
            .AsQueryable();

        // Filtreler
        if (filter.Status.HasValue)
            query = query.Where(i => i.Status == filter.Status);

        if (!string.IsNullOrEmpty(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(i =>
                (i.Product != null && i.Product.Name.ToLower().Contains(search)) ||
                (i.BuyerVendor != null && i.BuyerVendor.CompanyName != null && i.BuyerVendor.CompanyName.ToLower().Contains(search)));
        }

        if (filter.ProductId.HasValue)
            query = query.Where(i => i.ProductId == filter.ProductId);

        var totalCount = await query.CountAsync();

        query = filter.SortDescending
            ? query.OrderByDescending(i => i.CreatedAt)
            : query.OrderBy(i => i.CreatedAt);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(i => new ProductInquiryListDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product != null ? i.Product.Name : null,
                ProductSku = i.Product != null ? i.Product.SKU : null,
                ProductImageUrl = i.Product != null && i.Product.Images.Any() ? i.Product.Images.First().Url : null,
                BuyerVendorId = i.BuyerVendorId,
                BuyerCompanyName = i.BuyerVendor != null ? i.BuyerVendor.CompanyName : null,
                SellerVendorId = i.SellerVendorId,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Message = i.Message,
                Status = i.Status,
                StatusText = GetStatusText(i.Status),
                IsReadBySeller = i.IsReadBySeller,
                ResponseCount = i.Responses.Count,
                HasResponse = i.Responses.Any(),
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        return new ProductInquirySearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<ProductInquiryDetailDto?> GetInquiryForSellerAsync(int inquiryId, int sellerVendorId)
    {
        var inquiry = await _context.ProductInquiries
            .Include(i => i.Product)
            .Include(i => i.BuyerVendor)
            .Include(i => i.DeliveryAddress)
                .ThenInclude(a => a!.CountryEntity)
            .Include(i => i.Responses)
            .FirstOrDefaultAsync(i => i.Id == inquiryId && i.SellerVendorId == sellerVendorId);

        if (inquiry == null) return null;

        return MapToDetailDto(inquiry);
    }

    public async Task<bool> MarkAsReadAsync(int inquiryId, int sellerVendorId)
    {
        var inquiry = await _context.ProductInquiries
            .FirstOrDefaultAsync(i => i.Id == inquiryId && i.SellerVendorId == sellerVendorId);

        if (inquiry == null) return false;

        if (!inquiry.IsReadBySeller)
        {
            inquiry.IsReadBySeller = true;
            inquiry.ReadAt = DateTime.UtcNow;

            if (inquiry.Status == ProductInquiryStatuses.Pending.Id)
                inquiry.Status = ProductInquiryStatuses.Viewed.Id;

            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<ProductInquiryResponseDto> CreateResponseAsync(int sellerVendorId, ProductInquiryResponseCreateDto dto, string? createdBy)
    {
        var inquiry = await _context.ProductInquiries
            .FirstOrDefaultAsync(i => i.Id == dto.InquiryId && i.SellerVendorId == sellerVendorId);

        if (inquiry == null)
            throw new InvalidOperationException("Istek bulunamadi veya yetkiniz yok.");

        if (inquiry.Status == ProductInquiryStatuses.Cancelled.Id)
            throw new InvalidOperationException("Iptal edilmis istege yanit verilemez.");

        var response = new ProductInquiryResponse
        {
            InquiryId = dto.InquiryId,
            UnitPrice = dto.UnitPrice,
            TotalPrice = dto.TotalPrice,
            Currency = dto.Currency,
            OfferedQuantity = dto.OfferedQuantity,
            OfferedUnit = dto.OfferedUnit,
            LeadTimeDays = dto.LeadTimeDays,
            EstimatedDeliveryDate = dto.EstimatedDeliveryDate.ToUtcSafe(),
            ValidUntil = dto.ValidUntil.ToUtcSafe(),
            Notes = dto.Notes,
            TermsAndConditions = dto.TermsAndConditions,
            Status = ProductInquiryResponseStatuses.Pending.Id,
            CreatedBy = createdBy
        };

        _context.ProductInquiryResponses.Add(response);

        // Ana istegi guncelle
        inquiry.Status = ProductInquiryStatuses.Responded.Id;

        await _context.SaveChangesAsync();

        // Aliciya bildirim gonder
        var sellerVendor = await _context.Vendors.FindAsync(sellerVendorId);
        var product = await _context.Products.FindAsync(inquiry.ProductId);

        await _notificationService.CreateAsync(new NotificationCreateDto
        {
            VendorId = inquiry.BuyerVendorId,
            UserId = null,
            Type = NotificationType.ProductInquiryResponse,
            Title = "Istediginize Yanit Geldi",
            Message = $"{sellerVendor?.CompanyName ?? "Satici"} '{product?.Name}' urunu icin teklif gonderdi.",
            EntityType = "ProductInquiryResponse",
            EntityId = response.Id,
            ActionUrl = "/Dashboard/MyInquiries",
            Icon = "bi-reply"
        });

        return new ProductInquiryResponseDto
        {
            Id = response.Id,
            InquiryId = response.InquiryId,
            UnitPrice = response.UnitPrice,
            TotalPrice = response.TotalPrice,
            Currency = response.Currency,
            OfferedQuantity = response.OfferedQuantity,
            OfferedUnit = response.OfferedUnit,
            LeadTimeDays = response.LeadTimeDays,
            EstimatedDeliveryDate = response.EstimatedDeliveryDate,
            ValidUntil = response.ValidUntil,
            Notes = response.Notes,
            TermsAndConditions = response.TermsAndConditions,
            Status = response.Status,
            StatusText = GetResponseStatusText(response.Status),
            IsReadByBuyer = response.IsReadByBuyer,
            CreatedAt = response.CreatedAt
        };
    }

    public async Task<SellerInquiryStatsDto> GetSellerStatsAsync(int sellerVendorId)
    {
        var inquiries = await _context.ProductInquiries
            .Where(i => i.SellerVendorId == sellerVendorId)
            .ToListAsync();

        return new SellerInquiryStatsDto
        {
            TotalInquiries = inquiries.Count,
            UnreadInquiries = inquiries.Count(i => !i.IsReadBySeller),
            PendingResponse = inquiries.Count(i => i.Status == ProductInquiryStatuses.Pending.Id || i.Status == ProductInquiryStatuses.Viewed.Id),
            AcceptedOffers = inquiries.Count(i => i.Status == ProductInquiryStatuses.Accepted.Id)
        };
    }

    public async Task<int> GetUnreadCountAsync(int sellerVendorId)
    {
        return await _context.ProductInquiries
            .CountAsync(i => i.SellerVendorId == sellerVendorId && !i.IsReadBySeller);
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private ProductInquiryDetailDto MapToDetailDto(ProductInquiry inquiry)
    {
        return new ProductInquiryDetailDto
        {
            Id = inquiry.Id,
            ProductId = inquiry.ProductId,
            ProductName = inquiry.Product?.Name,
            ProductSku = inquiry.Product?.SKU,
            ProductImageUrl = inquiry.Product?.Images?.FirstOrDefault()?.Url,
            ProductDescription = inquiry.Product?.Description,
            ProductPrice = inquiry.Product?.Price,
            ProductCurrency = inquiry.Product?.Currency,
            BuyerVendorId = inquiry.BuyerVendorId,
            BuyerCompanyName = inquiry.BuyerVendor?.CompanyName,
            BuyerEmail = inquiry.BuyerVendor?.Email,
            BuyerPhone = inquiry.BuyerVendor?.Phone,
            SellerVendorId = inquiry.SellerVendorId,
            SellerCompanyName = inquiry.SellerVendor?.CompanyName,
            Quantity = inquiry.Quantity,
            Unit = inquiry.Unit,
            Message = inquiry.Message,
            SpecialRequirements = inquiry.SpecialRequirements,
            // Teslimat adresi (Address tablosundan)
            DeliveryCountryId = inquiry.DeliveryAddress?.CountryId,
            DeliveryCountryName = inquiry.DeliveryAddress?.CountryEntity?.Name,
            DeliveryCity = inquiry.DeliveryAddress?.City,
            DeliveryAddress = inquiry.DeliveryAddress?.AddressLine,
            DesiredDeliveryDate = inquiry.DesiredDeliveryDate,
            // İletişim bilgileri (Buyer Vendor'dan)
            ContactName = inquiry.BuyerVendor?.CompanyName,
            ContactEmail = inquiry.BuyerVendor?.Email,
            ContactPhone = inquiry.BuyerVendor?.Phone,
            Status = inquiry.Status,
            StatusText = GetStatusText(inquiry.Status),
            IsReadBySeller = inquiry.IsReadBySeller,
            ReadAt = inquiry.ReadAt,
            Responses = inquiry.Responses.Select(r => new ProductInquiryResponseDto
            {
                Id = r.Id,
                InquiryId = r.InquiryId,
                UnitPrice = r.UnitPrice,
                TotalPrice = r.TotalPrice,
                Currency = r.Currency,
                OfferedQuantity = r.OfferedQuantity,
                OfferedUnit = r.OfferedUnit,
                LeadTimeDays = r.LeadTimeDays,
                EstimatedDeliveryDate = r.EstimatedDeliveryDate,
                ValidUntil = r.ValidUntil,
                Notes = r.Notes,
                TermsAndConditions = r.TermsAndConditions,
                Status = r.Status,
                StatusText = GetResponseStatusText(r.Status),
                IsReadByBuyer = r.IsReadByBuyer,
                ReadByBuyerAt = r.ReadByBuyerAt,
                CreatedAt = r.CreatedAt
            }).ToList(),
            CreatedAt = inquiry.CreatedAt
        };
    }

    private static string GetStatusText(int status)
    {
        var item = ProductInquiryStatuses.GetById(status);
        return item?.Description ?? "Bilinmiyor";
    }

    private static string GetResponseStatusText(int status)
    {
        var item = ProductInquiryResponseStatuses.GetById(status);
        return item?.Description ?? "Bilinmiyor";
    }
}
