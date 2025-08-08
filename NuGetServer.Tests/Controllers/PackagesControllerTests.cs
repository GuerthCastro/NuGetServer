using NuGetServer.Controllers;
using NuGetServer.Entities.Config;
using NuGetServer.Extensions;
using NuGetServer.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit;

namespace NuGetServer.Tests.Controllers;

public class PackagesControllerTests
{
    private readonly Mock<ILogger<PackagesController>> _logger;
    private readonly ServiceConfig _serviceConfig;
    private readonly Entities.Config.NuGetServer _nuGetServer;
    private readonly Mock<IPackageStorageService> _packageStorageService;
    private readonly PackagesController _controller;

    public PackagesControllerTests()
    {
        _logger = new Mock<ILogger<PackagesController>>();
        _serviceConfig = new ServiceConfig { ServiceName = "{controllerName}-Service" };
        _nuGetServer = new Entities.Config.NuGetServer { ApiKey = "TestApiKey" };
        _packageStorageService = new Mock<IPackageStorageService>();

        _controller = new PackagesController(_logger.Object, _serviceConfig, _nuGetServer, _packageStorageService.Object);
        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
    }

    private IFormFile CreateFakeFormFile(string fileName, string content = "fake content")
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName);
    }

    private void SetRequestFile(IFormFile file)
    {
        var formFileCollection = new FormFileCollection { file };
        _controller.ControllerContext.HttpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), formFileCollection);
    }

    [Fact]
    public void Health_Should_Return_Ok_With_ServiceConfig()
    {
        // Act
        var result = _controller.Health();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<ServiceConfig>();
        ((ServiceConfig)okResult.Value).ServiceName.Should().Contain("PackagesController");
    }

    [Fact]
    public async Task UploadPackage_Should_Return_Unauthorized_When_ApiKey_Invalid()
    {
        // Act
        var result = await _controller.UploadPackage("InvalidKey");

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UploadPackage_Should_Return_BadRequest_When_No_Files()
    {
        // Act
        var result = await _controller.UploadPackage(_nuGetServer.ApiKey);

        // Assert
        result.Should().BeOfType<ObjectResult>()
              .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UploadPackage_Should_Return_BadRequest_When_File_Not_Nupkg()
    {
        // Arrange
        var file = CreateFakeFormFile("Dragonfly.Utils.txt");
        SetRequestFile(file);

        // Act
        var result = await _controller.UploadPackage(_nuGetServer.ApiKey);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadPackage_Should_Return_BadRequest_When_SavePackage_Fails()
    {
        // Arrange
        var file = CreateFakeFormFile("Dragonfly.Utils.1.0.0.nupkg");
        SetRequestFile(file);

        _packageStorageService
            .Setup(s => s.SavePackage(It.IsAny<Stream>(), file.FileName))
            .ReturnsAsync((false, "Save failed"));

        // Act
        var result = await _controller.UploadPackage(_nuGetServer.ApiKey);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UploadPackage_Should_Return_Ok_When_Successful()
    {
        // Arrange
        var file = CreateFakeFormFile("Dragonfly.Utils.1.0.0.nupkg");
        SetRequestFile(file);

        _packageStorageService
            .Setup(s => s.SavePackage(It.IsAny<Stream>(), file.FileName))
            .ReturnsAsync((true, "Package uploaded successfully"));

        // Act
        var result = await _controller.UploadPackage(_nuGetServer.ApiKey);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

        var messageProp = okResult.Value.GetType().GetProperty("message");
        messageProp.Should().NotBeNull();

        var messageValue = messageProp!.GetValue(okResult.Value) as string;
        messageValue.Should().Be($"Package {file.FileName} uploaded successfully");


        _packageStorageService.Verify(s => s.SavePackage(It.IsAny<Stream>(), file.FileName), Times.Once);
    }

    [Fact]
    public async Task UploadPackage_Should_Return_InternalServerError_On_Exception()
    {
        // Arrange
        var file = CreateFakeFormFile("Dragonfly.Utils.1.0.0.nupkg");
        SetRequestFile(file);

        _packageStorageService
            .Setup(s => s.SavePackage(It.IsAny<Stream>(), file.FileName))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.UploadPackage(_nuGetServer.ApiKey);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task DownloadPackage_Should_Return_NotFound_When_Stream_Is_Null()
    {
        // Arrange
        _packageStorageService
            .Setup(s => s.GetPackageStream("Dragonfly.Utils", "1.0.0"))
            .ReturnsAsync((null, null));

        // Act
        var result = await _controller.DownloadPackage("Dragonfly.Utils", "1.0.0");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DownloadPackage_Should_Return_FileStream_When_Package_Exists()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake package"));
        _packageStorageService
            .Setup(s => s.GetPackageStream("Dragonfly.Utils", "1.0.0"))
            .ReturnsAsync((stream, "application/octet-stream"));

        // Act
        var result = await _controller.DownloadPackage("Dragonfly.Utils", "1.0.0");

        // Assert
        var fileResult = result.Should().BeOfType<FileStreamResult>().Subject;
        fileResult.ContentType.Should().Be("application/octet-stream");
    }

    [Fact]
    public async Task DownloadPackage_Should_Return_InternalServerError_On_Exception()
    {
        // Arrange
        _packageStorageService
            .Setup(s => s.GetPackageStream("Dragonfly.Utils", "1.0.0"))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.DownloadPackage("Dragonfly.Utils", "1.0.0");

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }
}
