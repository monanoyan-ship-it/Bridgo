using Microsoft.EntityFrameworkCore;
using Bridgo.Data;

namespace Bridgo.Tests.Fixtures;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static async Task<ApplicationDbContext> CreateWithSeedAsync(string? dbName = null)
    {
        var context = Create(dbName);
        await SeedTestDataAsync(context);
        return context;
    }

    private static async Task SeedTestDataAsync(ApplicationDbContext context)
    {
        // Seed Languages
        if (!context.Languages.Any())
        {
            context.Languages.AddRange(
                new Bridgo.Models.Entities.Language { Id = 1, Name = "English", LanguageCulture = "en-US", UniqueSeoCode = "en", IsActive = true, IsDefault = true },
                new Bridgo.Models.Entities.Language { Id = 2, Name = "Turkish", LanguageCulture = "tr-TR", UniqueSeoCode = "tr", IsActive = true }
            );
        }

        // Seed Vendors
        if (!context.Vendors.Any())
        {
            context.Vendors.AddRange(
                new Bridgo.Models.Entities.Vendor { Id = 1, CompanyName = "Test Vendor 1", Email = "vendor1@test.com", VendorStatusId = 2, IsProfileComplete = true },
                new Bridgo.Models.Entities.Vendor { Id = 2, CompanyName = "Test Vendor 2", Email = "vendor2@test.com", VendorStatusId = 1, IsProfileComplete = false }
            );
        }

        await context.SaveChangesAsync();
    }
}
