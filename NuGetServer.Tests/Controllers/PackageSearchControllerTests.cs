using Bogus;
using NuGetServer.Controllers;
using NuGetServer.Entities.Config;
using NuGetServer.Entities.DTO;
using NuGetServer.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace NuGetServer.Tests.Controllers;

public class NuGetPackageInfoFaker : Faker<NuGetPackageInfo>
{
    public NuGetPackageInfoFaker()
    {
        RuleFor(p => p.Id, f => f.Commerce.ProductName().Replace(" ", ""));
        RuleFor(p => p.Version, f => $"{f.Random.Int(1, 5)}.{f.Random.Int(0, 9)}.{f.Random.Int(0, 9)}");
        RuleFor(p => p.FileName, (f, p) => $"{p.Id}.{p.Version}.nupkg");
        RuleFor(p => p.DownloadUrl, (f, p) => $"https://localhost/nuget/v3-flatcontainer/{p.Id}/{p.Version}/{p.FileName}");
        RuleFor(p => p.Description, f => f.Lorem.Sentence());
        RuleFor(p => p.Authors, f => f.Name.FullName());
    }
}

public class PackageSearchControllerTests
{
    private readonly Mock<ILogger<PackageSearchController>> _logger;
    private readonly Mock<IPackageStorageService> _packageStorageService;
    private readonly PackageSearchController _controller;
    private readonly Entities.Config.NuGetServer _nuGetServer;
    private readonly NuGetIndex _nuGetIndex;

    private readonly NuGetPackageInfoFaker _faker = new();

    public PackageSearchControllerTests()
    {
        _logger = new Mock<ILogger<PackageSearchController>>();
        _packageStorageService = new Mock<IPackageStorageService>();
        _nuGetServer = new Entities.Config.NuGetServer();
        _nuGetIndex = new NuGetIndex { ServiceUrl = "https://localhost" };

        _controller = new PackageSearchController(
            _logger.Object,
            new ServiceConfig(),
            _nuGetServer,
            _nuGetIndex,
            _packageStorageService.Object
        );
    }

    [Fact]
    public void GetIndex_ReturnsExpectedJson()
    {
        // Act
        var result = _controller.GetIndex() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllPackages_ReturnsPackages()
    {
        // Arrange
        var packages = _faker.Generate(3);
        _packageStorageService
            .Setup(s => s.GetAllPackages())
            .ReturnsAsync(packages);

        // Act
        var result = await _controller.GetAllPackages() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();

        var json = JsonSerializer.Serialize(result!.Value);
        var indexResult = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
        int totalHits = int.Parse(indexResult["totalHits"].ToString()!);

        totalHits.Should().Be(3);
    }

    [Fact]
    public async Task GetAllPackages_WhenException_ReturnsProblem()
    {
        // Arrange
        _packageStorageService
            .Setup(s => s.GetAllPackages())
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _controller.GetAllPackages();

        // Assert
        var problem = result as ObjectResult;
        problem.Should().NotBeNull();
        problem!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SearchPackages_NoQuery_ReturnsAllNonPrerelease()
    {
        // Arrange
        var packages = _faker.Generate(2);
        packages[1].Version = $"{packages[1].Version}-beta";

        _packageStorageService.Setup(s => s.GetPackagesWithMetadata())
            .ReturnsAsync(packages);

        // Act
        var result = await _controller.SearchPackages() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var json = JsonSerializer.Serialize(result!.Value);
        var indexResult = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
        int totalHits = int.Parse(indexResult["totalHits"].ToString()!);

        totalHits.Should().Be(1);
    }

    [Fact]
    public async Task SearchPackages_WithQueryAndPrerelease_ReturnsFiltered()
    {
        // Arrange
        var packages = _faker.Generate(2);
        packages[0].Id = "TestA";
        packages[1].Id = "Other";
        packages[1].Version = $"{packages[1].Version}-beta";

        _packageStorageService.Setup(s => s.GetPackagesWithMetadata())
            .ReturnsAsync(packages);

        // Act
        var result = await _controller.SearchPackages(q: "Test", prerelease: true) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();

        var json = JsonSerializer.Serialize(result!.Value);
        var indexResult = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;

        var data = indexResult["data"]
            .EnumerateArray()
            .Select(e => e.GetProperty("id").GetString()!)
            .ToList();

        data.Should().HaveCount(1);
        data[0].Should().Be("TestA");
    }

    [Fact]
    public async Task Autocomplete_NoQuery_ReturnsAllIds()
    {
        // Arrange
        var packages = _faker.Generate(5);
        _packageStorageService.Setup(s => s.GetAllPackages())
            .ReturnsAsync(packages);

        // Act
        var result = await _controller.Autocomplete() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();

        var json = JsonSerializer.Serialize(result!.Value);
        var indexResult = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;

        indexResult.Should().ContainKey("totalHits");
        int totalHits = int.Parse(indexResult["totalHits"].ToString()!);
        totalHits.Should().Be(5);
    }

    [Fact]
    public async Task Autocomplete_WithQuery_FiltersResults()
    {
        // Arrange
        var packages = _faker.Generate(2);
        packages[0].Id = "Alpha";
        packages[1].Id = "Beta";

        _packageStorageService.Setup(s => s.GetAllPackages()).ReturnsAsync(packages);

        // Act
        var result = await _controller.Autocomplete("Alpha") as OkObjectResult;

        // Assert
        result.Should().NotBeNull();

        var json = JsonSerializer.Serialize(result!.Value);
        var indexResult = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
        int totalHits = int.Parse(indexResult["totalHits"].ToString()!);
        var jsonElement = (JsonElement)indexResult["data"];
        var data = jsonElement.EnumerateArray()
                                        .Select(e => e.GetString()!)
                                        .ToList();
        totalHits.Should().Be(1);
        data.Should().Contain("Alpha");
    }

    [Fact]
    public async Task GetPackageVersions_ReturnsExpectedVersions()
    {
        // Arrange
        var versions = new List<string> { "1.0.0", "2.0.0" };
        _packageStorageService.Setup(s => s.GetPackageVersions("MyPkg"))
            .ReturnsAsync(versions);

        // Act
        var result = await _controller.GetPackageVersions("MyPkg") as OkObjectResult;
        var json = JsonSerializer.Serialize(result!.Value);
        var versionResult = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
        var data = JsonSerializer.Deserialize<List<string>>(versionResult["versions"].GetRawText())!;

        // Assert
        result.Should().NotBeNull();
        data.Should().NotBeEmpty();
        data.Should().Contain("1.0.0");
        data.Should().Contain("2.0.0");
    }

    [Fact]
    public async Task DownloadPackage_ReturnsFile_WhenFound()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("dummy");
        var stream = new MemoryStream(bytes);
        _packageStorageService.Setup(s => s.GetPackageStream("Id", "1.0.0"))
            .ReturnsAsync((stream, "application/zip"));

        // Act
        var result = await _controller.DownloadPackage("Id", "1.0.0", "file.nupkg");

        // Assert
        result.Should().BeOfType<FileStreamResult>();
        ((FileStreamResult)result).ContentType.Should().Be("application/zip");
    }

    [Fact]
    public async Task DownloadPackage_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        _packageStorageService.Setup(s => s.GetPackageStream("Id", "1.0.0"))
            .ReturnsAsync((null as Stream, null));

        // Act
        var result = await _controller.DownloadPackage("Id", "1.0.0", "file.nupkg");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DownloadPackage_WhenException_ReturnsProblem()
    {
        // Arrange
        _packageStorageService.Setup(s => s.GetPackageStream("Id", "1.0.0"))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.DownloadPackage("Id", "1.0.0", "file.nupkg");

        // Assert
        var problem = result as ObjectResult;
        problem.Should().NotBeNull();
        problem!.StatusCode.Should().Be(500);
    }
}
