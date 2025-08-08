using NuGetServer.Controllers;
using NuGetServer.Entities.Config;
using NuGetServer.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit;

namespace NuGetServer.Tests.Controllers;

public class PackagePublishControllerTests
{
    private readonly Mock<ILogger<PackagePublishController>> _logger;
    private readonly Mock<IPackageStorageService> _packageStorageService;
    private readonly Entities.Config.NuGetServer _nuGetServer;
    private readonly PackagePublishController _controller;

    public PackagePublishControllerTests()
    {
        _logger = new Mock<ILogger<PackagePublishController>>();
        _packageStorageService = new Mock<IPackageStorageService>();
        _nuGetServer = new Entities.Config.NuGetServer
        {
            ApiKey = "DragonflyKey123"
        };

        _controller = new PackagePublishController(_logger.Object, _packageStorageService.Object, _nuGetServer)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static IFormFile CreateFakeFormFile(string fileName, string content = "fake content")
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName);
    }

    private void SetRequestFile(IFormFile file)
    {
        var files = new FormFileCollection { file };
        var form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);
        _controller.ControllerContext.HttpContext.Request.Form = form;
    }

    [Fact]
    public async Task PublishPackage_Should_Return_Unauthorized_When_ApiKey_Invalid()
    {
        // Act
        var result = await _controller.PublishPackage("InvalidKey");

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task PublishPackage_Should_Return_BadRequest_When_No_Files()
    {
        // Act
        var result = await _controller.PublishPackage(_nuGetServer.ApiKey);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }


    [Fact]
    public async Task PublishPackage_Should_Return_BadRequest_When_File_Is_Not_Nupkg()
    {
        // Arrange
        var file = CreateFakeFormFile("Dragonfly.Utils.txt");
        SetRequestFile(file);

        // Act
        var result = await _controller.PublishPackage(_nuGetServer.ApiKey);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PublishPackage_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var file = CreateFakeFormFile("Dragonfly.Utils.1.0.0.nupkg");
        SetRequestFile(file);

        _packageStorageService
            .Setup(s => s.SavePackage(It.IsAny<Stream>(), file.FileName))
            .ReturnsAsync((true, "Package saved successfully"));

        // Act
        var result = await _controller.PublishPackage(_nuGetServer.ApiKey);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

        var messageProp = okResult.Value.GetType().GetProperty("message");
        messageProp.Should().NotBeNull();

        var messageValue = messageProp!.GetValue(okResult.Value) as string;
        messageValue.Should().Be("Package saved successfully");

        _packageStorageService.Verify(s => s.SavePackage(It.IsAny<Stream>(), file.FileName), Times.Once);
    }

    [Fact]
    public async Task DeletePackage_Should_Return_Unauthorized_When_ApiKey_Invalid()
    {
        // Act
        var result = await _controller.DeletePackage("InvalidKey", "Dragonfly.Utils", "1.0.0");

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task DeletePackage_Should_Return_NotFound_When_Fails()
    {
        // Arrange
        _packageStorageService
            .Setup(s => s.DeletePackage("Dragonfly.Utils", "1.0.0"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeletePackage(_nuGetServer.ApiKey, "Dragonfly.Utils", "1.0.0");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeletePackage_Should_Return_Ok_When_Successful()
    {
        // Arrange
        _packageStorageService
            .Setup(s => s.DeletePackage("Dragonfly.Utils", "1.0.0"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeletePackage(_nuGetServer.ApiKey, "Dragonfly.Utils", "1.0.0");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value!;
        var message = response.GetType().GetProperty("message")!.GetValue(response)!.ToString();

        message.Should().Be("Package Dragonfly.Utils 1.0.0 deleted successfully");

        _packageStorageService.Verify(s => s.DeletePackage("Dragonfly.Utils", "1.0.0"), Times.Once);
    }

}
