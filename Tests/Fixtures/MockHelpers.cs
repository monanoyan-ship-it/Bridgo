using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Moq;
using Bridgo.Models.Identity;
using Bridgo.Services;

namespace Bridgo.Tests.Fixtures;

public static class MockHelpers
{
    public static Mock<ILocalizationService> CreateLocalizationService()
    {
        var mock = new Mock<ILocalizationService>();

        // T() metodunu mockla - gelen key'i aynen dondur
        mock.Setup(x => x.T(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string key, string defaultValue) => defaultValue ?? key);

        return mock;
    }

    public static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // Varsayilan davranislar
        mock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());

        return mock;
    }

    public static Mock<IWebHostEnvironment> CreateWebHostEnvironment(string contentRootPath = "C:\\TestApp")
    {
        var mock = new Mock<IWebHostEnvironment>();
        mock.Setup(x => x.ContentRootPath).Returns(contentRootPath);
        mock.Setup(x => x.WebRootPath).Returns(Path.Combine(contentRootPath, "wwwroot"));
        return mock;
    }
}
