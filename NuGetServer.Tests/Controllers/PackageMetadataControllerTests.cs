using NuGetServer.Controllers;
using NuGetServer.Entities.DTO;
using NuGetServer.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NuGetServer.Tests.Controllers;

public class PackageMetadataControllerTests
{
    private readonly Mock<IPackageStorageService> _packageStorageService;
    private readonly Mock<ILogger<PackageMetadataController>> _logger;
    private readonly PackageMetadataController _controller;

    public PackageMetadataControllerTests()
    {
        _packageStorageService = new Mock<IPackageStorageService>();
        _logger = new Mock<ILogger<PackageMetadataController>>();
        _controller = new PackageMetadataController(_packageStorageService.Object, _logger.Object);
    }

    [Fact]
    public async Task GetPackageVersions_Should_Return_Ok_With_Versions()
    {
        // Arrange
        var packageId = "Dragonfly.Utils";
        var expectedVersions = new List<string> { "1.0.0", "2.0.0" };

        _packageStorageService
            .Setup(s => s.GetPackageVersions(packageId))
            .ReturnsAsync(expectedVersions);

        // Act
        var result = await _controller.GetPackageVersions(packageId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

        // Convertir el Value a Dictionary
        var dict = okResult.Value!
            .GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, p => p.GetValue(okResult.Value));

        dict.Should().ContainKey("versions");
        ((IEnumerable<string>)dict["versions"]!).Should().BeEquivalentTo(expectedVersions);

        _packageStorageService.Verify(s => s.GetPackageVersions(packageId), Times.Once);
    }

    [Fact]
    public async Task GetPackageMetadata_Should_Return_Ok_When_Metadata_Exists()
    {
        // Arrange
        var packageId = "Dragonfly.Utils";
        var version = "1.0.0";

        var metadata = new NuGetPackageInfo
        {
            Id = packageId,
            Version = version,
            Description = "Test package",
            Authors = "Guerth"
        };

        _packageStorageService
            .Setup(s => s.GetPackageMetadata(packageId, version))
            .ReturnsAsync(metadata);

        // Act
        var result = await _controller.GetPackageMetadata(packageId, version);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(metadata);

        _packageStorageService.Verify(s => s.GetPackageMetadata(packageId, version), Times.Once);
    }

    [Fact]
    public async Task GetPackageMetadata_Should_Return_NotFound_When_Metadata_Does_Not_Exist()
    {
        // Arrange
        var packageId = "Dragonfly.Utils";
        var version = "1.0.0";

        _packageStorageService
            .Setup(s => s.GetPackageMetadata(packageId, version))
            .ReturnsAsync((NuGetPackageInfo?)null);

        // Act
        var result = await _controller.GetPackageMetadata(packageId, version);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;

        // Convertir el Value a Dictionary para acceder al mensaje
        var dict = notFoundResult.Value!
            .GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, p => p.GetValue(notFoundResult.Value));

        dict.Should().ContainKey("message");
        dict["message"].Should().Be("Package metadata not found");

        _packageStorageService.Verify(s => s.GetPackageMetadata(packageId, version), Times.Once);
    }

}
