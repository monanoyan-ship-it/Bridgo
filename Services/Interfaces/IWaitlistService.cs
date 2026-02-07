namespace Bridgo.Services.Interfaces;

public interface IWaitlistService
{
    Task<(bool success, string message)> JoinAsync(string email, string? source, string? ipAddress, string? userAgent);
    Task<int> GetCountAsync();
}
