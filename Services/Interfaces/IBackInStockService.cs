using Bridgo.DTOs.BackInStock;

namespace Bridgo.Services.Interfaces;

public interface IBackInStockService
{
    Task<List<BackInStockSubscriptionDto>> GetSubscriptionsAsync(int vendorId);
    Task<ServiceResult> SubscribeAsync(int vendorId, int productId);
    Task<ServiceResult> UnsubscribeAsync(int vendorId, int subscriptionId);
    Task<bool> IsSubscribedAsync(int vendorId, int productId);
}
