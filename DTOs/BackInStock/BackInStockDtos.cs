namespace Bridgo.DTOs.BackInStock;

public class BackInStockSubscriptionDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public bool IsNotified { get; set; }
    public DateTime? NotifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BackInStockSubscribeDto
{
    public int ProductId { get; set; }
}
