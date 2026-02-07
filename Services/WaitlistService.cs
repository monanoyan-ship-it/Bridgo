using Bridgo.Models.Entities;
using Bridgo.Repositories;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class WaitlistService : IWaitlistService
{
    private readonly IUnitOfWork _unitOfWork;

    public WaitlistService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool success, string message)> JoinAsync(string email, string? source, string? ipAddress, string? userAgent)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var exists = await _unitOfWork.WaitlistEntries.AnyAsync(w => w.Email == normalizedEmail);
        if (exists)
            return (true, "You're already on the list!");

        var entry = new WaitlistEntry
        {
            Email = normalizedEmail,
            IpAddress = ipAddress,
            UserAgent = userAgent is { Length: > 500 } ? userAgent[..500] : userAgent,
            Source = source ?? "landing-page"
        };

        await _unitOfWork.WaitlistEntries.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Thank you! You're on the list.");
    }

    public async Task<int> GetCountAsync()
    {
        return await _unitOfWork.WaitlistEntries.CountAsync();
    }
}
