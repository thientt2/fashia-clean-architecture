using Fashia.Application.Categories.Commands.CreateCategory;
using Fashia.Application.Common.Behaviours;
using Fashia.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Fashia.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<CreateCategoryCommand>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<CreateCategoryCommand>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<CreateCategoryCommand>(
            _logger.Object,
            _user.Object,
            _identityService.Object
        );

        await requestLogger.Process(
            new CreateCategoryCommand { Name = "Shoes" },
            CancellationToken.None
        );

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<CreateCategoryCommand>(
            _logger.Object,
            _user.Object,
            _identityService.Object
        );

        await requestLogger.Process(
            new CreateCategoryCommand { Name = "Shoes" },
            CancellationToken.None
        );

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}
