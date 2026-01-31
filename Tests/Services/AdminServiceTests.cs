using FluentAssertions;
using Bridgo.Data;
using Bridgo.Models.Entities;
using Bridgo.Services;
using Bridgo.Tests.Fixtures;

namespace Bridgo.Tests.Services;

public class AdminServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AdminService _service;

    public AdminServiceTests()
    {
        _context = TestDbContextFactory.Create();
        var localizationService = MockHelpers.CreateLocalizationService();
        var userManager = MockHelpers.CreateUserManager();
        var webHostEnvironment = MockHelpers.CreateWebHostEnvironment();

        _service = new AdminService(
            _context,
            localizationService.Object,
            userManager.Object,
            webHostEnvironment.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Dashboard Tests

    [Fact]
    public async Task GetDashboardStatsAsync_EmptyDatabase_ReturnsZeroCounts()
    {
        // Act
        var result = await _service.GetDashboardStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.VendorCount.Should().Be(0);
        result.UserCount.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WithVendors_ReturnsCorrectCount()
    {
        // Arrange
        _context.Vendors.AddRange(
            new Vendor { CompanyName = "Vendor 1", Email = "v1@test.com", VendorStatusId = 2 },
            new Vendor { CompanyName = "Vendor 2", Email = "v2@test.com", VendorStatusId = 2 },
            new Vendor { CompanyName = "Vendor 3", Email = "v3@test.com", VendorStatusId = 1 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetDashboardStatsAsync();

        // Assert
        result.VendorCount.Should().Be(3);
    }

    [Fact]
    public async Task GetRecentVendorsAsync_ReturnsOrderedByCreatedAt()
    {
        // Arrange - Add vendors with clear time differences (seconds apart to avoid precision issues)
        var baseTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var oldVendor = new Vendor { CompanyName = "Old Vendor", Email = "old@test.com", VendorStatusId = 2, CreatedAt = baseTime };
        var medVendor = new Vendor { CompanyName = "Medium Vendor", Email = "med@test.com", VendorStatusId = 2, CreatedAt = baseTime.AddMinutes(5) };
        var newVendor = new Vendor { CompanyName = "New Vendor", Email = "new@test.com", VendorStatusId = 2, CreatedAt = baseTime.AddMinutes(10) };

        _context.Vendors.Add(oldVendor);
        await _context.SaveChangesAsync();
        _context.Vendors.Add(medVendor);
        await _context.SaveChangesAsync();
        _context.Vendors.Add(newVendor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetRecentVendorsAsync(3);

        // Assert - Ordered by CreatedAt descending (newest first)
        result.Should().HaveCount(3);
        result[0].CompanyName.Should().Be("New Vendor");
        result[1].CompanyName.Should().Be("Medium Vendor");
        result[2].CompanyName.Should().Be("Old Vendor");
    }

    [Fact]
    public async Task GetRecentVendorsAsync_RespectsCount()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            _context.Vendors.Add(new Vendor { CompanyName = $"Vendor {i}", Email = $"v{i}@test.com", VendorStatusId = 2 });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetRecentVendorsAsync(5);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetCapabilityStatsAsync_ReturnsAllCapabilities()
    {
        // Act
        var result = await _service.GetCapabilityStatsAsync();

        // Assert
        result.Should().NotBeEmpty();
    }

    #endregion

    #region Vendor Tests

    [Fact]
    public async Task GetVendorsAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetVendorsAsync(null, null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVendorsAsync_WithSearch_FiltersCorrectly()
    {
        // Arrange
        _context.Vendors.AddRange(
            new Vendor { CompanyName = "ABC Company", Email = "abc@test.com", VendorStatusId = 2 },
            new Vendor { CompanyName = "XYZ Corp", Email = "xyz@test.com", VendorStatusId = 2 },
            new Vendor { CompanyName = "ABC International", Email = "abci@test.com", VendorStatusId = 2 }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetVendorsAsync("ABC", null);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(v => v.CompanyName.Contains("ABC"));
    }

    [Fact]
    public async Task GetVendorByIdAsync_ExistingVendor_ReturnsVendor()
    {
        // Arrange
        var vendor = new Vendor { CompanyName = "Test Vendor", Email = "test@test.com", VendorStatusId = 2 };
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetVendorByIdAsync(vendor.Id);

        // Assert
        result.Should().NotBeNull();
        result!.CompanyName.Should().Be("Test Vendor");
    }

    [Fact]
    public async Task GetVendorByIdAsync_NonExistingVendor_ReturnsNull()
    {
        // Act
        var result = await _service.GetVendorByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ApproveVendorAsync_PendingVerificationVendor_ChangesStatusToActive()
    {
        // Arrange - VendorStatuses: PendingVerification=2, Active=3
        var vendor = new Vendor { CompanyName = "Pending", Email = "p@test.com", VendorStatusId = 2 };
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ApproveVendorAsync(vendor.Id);

        // Assert
        result.Success.Should().BeTrue();
        var updated = await _context.Vendors.FindAsync(vendor.Id);
        updated!.VendorStatusId.Should().Be(3); // Active
    }

    [Fact]
    public async Task SuspendVendorAsync_ActiveVendor_ChangesStatusToSuspended()
    {
        // Arrange - VendorStatuses: Active=3, Suspended=4
        var vendor = new Vendor { CompanyName = "Active", Email = "a@test.com", VendorStatusId = 3 };
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SuspendVendorAsync(vendor.Id);

        // Assert
        result.Success.Should().BeTrue();
        var updated = await _context.Vendors.FindAsync(vendor.Id);
        updated!.VendorStatusId.Should().Be(4); // Suspended
    }

    #endregion

    #region User Tests

    [Fact]
    public async Task GetUsersAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetUsersAsync(null, null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ToggleUserStatusAsync_ActiveUser_DeactivatesUser()
    {
        // Arrange
        var user = new Bridgo.Models.Identity.ApplicationUser
        {
            UserName = "testuser",
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ToggleUserStatusAsync(user.Id);

        // Assert
        result.Success.Should().BeTrue();
        var updated = await _context.Users.FindAsync(user.Id);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleUserStatusAsync_InactiveUser_ActivatesUser()
    {
        // Arrange
        var user = new Bridgo.Models.Identity.ApplicationUser
        {
            UserName = "testuser",
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = false
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ToggleUserStatusAsync(user.Id);

        // Assert
        result.Success.Should().BeTrue();
        var updated = await _context.Users.FindAsync(user.Id);
        updated!.IsActive.Should().BeTrue();
    }

    #endregion

    #region Language Tests

    [Fact]
    public async Task GetLanguagesAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetLanguagesAsync(false);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLanguagesAsync_OnlyActive_FiltersInactive()
    {
        // Arrange
        _context.Languages.AddRange(
            new Language { Name = "Active", LanguageCulture = "en", UniqueSeoCode = "en", IsActive = true },
            new Language { Name = "Inactive", LanguageCulture = "de", UniqueSeoCode = "de", IsActive = false }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetLanguagesAsync(true);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active");
    }

    #endregion
}
