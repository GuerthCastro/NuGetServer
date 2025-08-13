using System.IO.Compression;
using Bogus;
using NuGetServer.Entities.Config;
using NuGetServer.Entities.DTO;
using NuGetServer.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NuGetServer.Tests.Services;

public class PackageStorageServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly PackageStorageService _service;
    private readonly Mock<ILogger<PackageStorageService>> _logger;
    private readonly Entities.Config.NuGetServer _config;
    private readonly NuGetIndex _index;

    public PackageStorageServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        _logger = new Mock<ILogger<PackageStorageService>>();
        _config = new Entities.Config.NuGetServer { PackagesPath = _tempDir };
        _index = new NuGetIndex { ServiceUrl = "http://localhost:5000" };

        _service = new PackageStorageService(_logger.Object, _config, _index);
    }

    [Fact]
    public async Task SavePackage_Should_Save_File_When_Valid()
    {
        // Arrange
        var fileName = "Dragonfly.Core.1.0.0.nupkg";
        await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        // Act
        var (success, message) = await _service.SavePackage(stream, fileName);

        // Assert
        success.Should().BeTrue();
        message.Should().Contain("uploaded successfully");

        var packagePath = Path.Combine(_tempDir, "Dragonfly.Core", "1.0.0", fileName);
        File.Exists(packagePath).Should().BeTrue();
    }

    [Fact]
    public async Task SavePackage_Should_Return_False_When_Not_Nupkg()
    {
        // Arrange
        var fileName = "Dragonfly.Core.txt";
        await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        // Act
        var (success, message) = await _service.SavePackage(stream, fileName);

        // Assert
        success.Should().BeFalse();
        message.Should().Contain("Only .nupkg files are allowed");
    }

    [Fact]
    public async Task GetPackageStream_Should_Return_Stream_When_Exists()
    {
        // Arrange
        var fileName = "Dragonfly.Utils.2.0.0.nupkg";
        var packageDir = Path.Combine(_tempDir, "Dragonfly.Utils", "2.0.0");
        Directory.CreateDirectory(packageDir);

        var filePath = Path.Combine(packageDir, fileName);
        await File.WriteAllBytesAsync(filePath, new byte[] { 10, 20, 30 });

        // Act
        var (stream, contentType) = await _service.GetPackageStream("Dragonfly.Utils", "2.0.0");

        // Assert
        stream.Should().NotBeNull();
        contentType.Should().Be("application/octet-stream");

        await stream!.DisposeAsync();
    }

    [Fact]
    public async Task GetPackageStream_Should_Return_Null_When_Not_Exists()
    {
        // Act
        var (stream, contentType) = await _service.GetPackageStream("Nope", "1.0.0");

        // Assert
        stream.Should().BeNull();
        contentType.Should().BeNull();
    }

    [Fact]
    public async Task DeletePackage_Should_Delete_Existing_Package()
    {
        // Arrange
        var packageDir = Path.Combine(_tempDir, "Dragonfly.Api", "3.0.0");
        Directory.CreateDirectory(packageDir);

        // Act
        var result = await _service.DeletePackage("Dragonfly.Api", "3.0.0");

        // Assert
        result.Should().BeTrue();
        Directory.Exists(packageDir).Should().BeFalse();
    }

    [Fact]
    public async Task DeletePackage_Should_Return_False_When_Not_Exists()
    {
        // Act
        var result = await _service.DeletePackage("Unknown", "1.0.0");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PackageExists_Should_Return_True_When_Package_Exists()
    {
        // Arrange
        var packageDir = Path.Combine(_tempDir, "Dragonfly.Core", "1.0.0");
        Directory.CreateDirectory(packageDir);
        var fileName = Path.Combine(packageDir, "Dragonfly.Core.1.0.0.nupkg");
        await File.WriteAllBytesAsync(fileName, new byte[] { 1, 2, 3 });

        // Act
        var exists = await _service.PackageExists("Dragonfly.Core", "1.0.0");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetPackageVersions_Should_Return_Ordered_List()
    {
        // Arrange
        var packageDir = Path.Combine(_tempDir, "Dragonfly.Tools");
        Directory.CreateDirectory(Path.Combine(packageDir, "1.0.0"));
        Directory.CreateDirectory(Path.Combine(packageDir, "1.0.1"));
        Directory.CreateDirectory(Path.Combine(packageDir, "0.9.0"));

        // Act
        var versions = await _service.GetPackageVersions("Dragonfly.Tools");

        // Assert
        versions.Should().ContainInOrder("0.9.0", "1.0.0", "1.0.1");
    }

    [Fact]
    public async Task GetPackageMetadata_Should_Read_Nuspec_Info()
    {
        // Arrange
        var id = "Dragonfly.Metadata";
        var version = "1.2.3";
        var packageDir = Path.Combine(_tempDir, id, version);
        Directory.CreateDirectory(packageDir);

        var nupkgPath = Path.Combine(packageDir, $"{id}.{version}.nupkg");
        using (var zip = ZipFile.Open(nupkgPath, ZipArchiveMode.Create))
        {
            var nuspecEntry = zip.CreateEntry($"{id}.nuspec");
            using var writer = new StreamWriter(nuspecEntry.Open());
            await writer.WriteAsync($@"
                <package>
                  <metadata>
                    <id>{id}</id>
                    <version>{version}</version>
                    <authors>Guerth</authors>
                    <description>Test package</description>
                  </metadata>
                </package>");
        }

        // Act
        var metadata = await _service.GetPackageMetadata(id, version);

        // Assert
        metadata.Should().NotBeNull();
        metadata!.Id.Should().Be(id);
        metadata.Version.Should().Be(version);
        metadata.Authors.Should().Be("Guerth");
        metadata.Description.Should().Be("Test package");
    }

    [Fact]
    public async Task GetPackageMetadata_Should_Read_Nuspec_Info_With_Namespace()
    {
        // Arrange
        var id = "Dragonfly.NamespaceTest";
        var version = "2.1.0";
        var packageDir = Path.Combine(_tempDir, id, version);
        Directory.CreateDirectory(packageDir);

        var nupkgPath = Path.Combine(packageDir, $"{id}.{version}.nupkg");
        using (var zip = ZipFile.Open(nupkgPath, ZipArchiveMode.Create))
        {
            var nuspecEntry = zip.CreateEntry($"{id}.nuspec");
            using var writer = new StreamWriter(nuspecEntry.Open());
            await writer.WriteAsync($@"<?xml version=""1.0""?>
                <package xmlns=""http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd"">
                  <metadata>
                    <id>{id}</id>
                    <version>{version}</version>
                    <authors>Guerth Castro</authors>
                    <description>Test package with namespace</description>
                  </metadata>
                </package>");
        }

        // Act
        var metadata = await _service.GetPackageMetadata(id, version);

        // Assert
        metadata.Should().NotBeNull();
        metadata!.Id.Should().Be(id);
        metadata.Version.Should().Be(version);
        metadata.Authors.Should().Be("Guerth Castro");
        metadata.Description.Should().Be("Test package with namespace");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }
}


/*
using NuGetServer.Entities.Config;
using NuGetServer.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NuGetServer.Tests;

public class PackageStorageServiceTests
{
    [Fact]
    public async Task SavePackage_InvalidExtension_ReturnsFalse()
    {
        var logger = Mock.Of<ILogger<PackageStorageService>>();
        var config = new Entities.Config.NuGetServer { PackagesPath = Path.GetTempPath() };
        var index = new NuGetIndex();
        var service = new PackageStorageService(logger, config, index);
        var stream = new MemoryStream();
        var (success, message) = await service.SavePackage(stream, "test.txt");
        Assert.False(success);
        Assert.Contains(".nupkg", message);
    }

    [Fact]
    public async Task PackageExists_ReturnsFalseForMissing()
    {
        var logger = Mock.Of<ILogger<PackageStorageService>>();
        var config = new Entities.Config.NuGetServer { PackagesPath = Path.GetTempPath() };
        var index = new NuGetIndex();
        var service = new PackageStorageService(logger, config, index);
        var exists = await service.PackageExists("notfound", "1.0.0");
        Assert.False(exists);
    }
}
*/
