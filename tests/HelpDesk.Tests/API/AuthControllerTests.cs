using HelpDesk.API.Controllers;
using HelpDesk.Application.Auth;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Users;
using HelpDesk.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HelpDesk.Tests.API;

public class AuthControllerTests
{
    [Fact]
    public async Task DeleteAccount_WithAuthenticatedUser_ShouldReturnNoContent()
    {
        Guid userId = Guid.NewGuid();
        Mock<IAuthService> authService = new();
        Mock<IUserManagementService> userManagementService = new();
        Mock<ICurrentUser> currentUser = new();

        currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        userManagementService
            .Setup(x => x.DeleteAccountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        AuthController controller = new(
            authService.Object,
            userManagementService.Object,
            currentUser.Object);

        IActionResult result = await controller.DeleteAccount(CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteAccount_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        Guid userId = Guid.NewGuid();
        Mock<IUserManagementService> userManagementService = new();
        Mock<ICurrentUser> currentUser = new();

        currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        userManagementService
            .Setup(x => x.DeleteAccountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        AuthController controller = new(
            Mock.Of<IAuthService>(),
            userManagementService.Object,
            currentUser.Object);

        IActionResult result = await controller.DeleteAccount(CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
