using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.BackInStock;
using Bridgo.Models.Entities;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class BackInStockService : IBackInStockService
{
    private readonly ApplicationDbContext _context;

    public BackInStockService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BackInStockSubscriptionDto>> GetSubscriptionsAsync(int vendorId)
    {
        return await _context.BackInStockSubscriptions
            .Where(s => s.VendorId == vendorId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new BackInStockSubscriptionDto
            {
                Id = s.Id,
                ProductId = s.ProductId,
                ProductName = s.Product.Name,
                ProductSku = s.Product.SKU,
                IsNotified = s.IsNotified,
                NotifiedAt = s.NotifiedAt,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ServiceResult> SubscribeAsync(int vendorId, int productId)
    {
        var exists = await _context.BackInStockSubscriptions
            .AnyAsync(s => s.VendorId == vendorId && s.ProductId == productId);

        if (exists)
            return ServiceResult.Fail("Bu urun icin zaten aboneliginiz var.");

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return ServiceResult.Fail("Urun bulunamadi.");

        var entry = new BackInStockSubscription
        {
            ProductId = productId,
            VendorId = vendorId,
            IsNotified = false
        };

        _context.BackInStockSubscriptions.Add(entry);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Stok bildirimi aboneligi olusturuldu.");
    }

    public async Task<ServiceResult> UnsubscribeAsync(int vendorId, int subscriptionId)
    {
        var sub = await _context.BackInStockSubscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.VendorId == vendorId);

        if (sub == null)
            return ServiceResult.Fail("Abonelik bulunamadi.");

        sub.IsDeleted = true;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Abonelik iptal edildi.");
    }

    public async Task<bool> IsSubscribedAsync(int vendorId, int productId)
    {
        return await _context.BackInStockSubscriptions
            .AnyAsync(s => s.VendorId == vendorId && s.ProductId == productId);
    }
}
