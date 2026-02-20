namespace Bridgo.DTOs.RewardPoints;

public class RewardPointSummaryDto
{
    public int TotalPoints { get; set; }
    public int EarnedThisMonth { get; set; }
    public int SpentThisMonth { get; set; }
}

public class RewardPointHistoryDto
{
    public int Id { get; set; }
    public int Points { get; set; }
    public int ActionTypeId { get; set; }
    public string ActionTypeName { get; set; } = string.Empty;
    public string ActionTypeCssClass { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? OrderId { get; set; }
    public int BalanceAfter { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RewardPointFilterDto
{
    public int? ActionTypeId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
