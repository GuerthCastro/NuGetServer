using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NuGetServer.Controllers;
using NuGetServer.Entities.DTO;
using NuGetServer.Services;
using Xunit;

namespace NuGetServer.Tests.Controllers;

public class RegistrationEndpointTests
{
    [Fact]
    public async Task GetPackageVersions_Returns_AllVersions_Normalized_And_Sorted()
    {
        var versions = new List<string> { "1.0.0", "1.0.0-beta", "2.0.0", "1.0.0-alpha" };
        var mockSvc = new Mock<IPackageStorageService>();
        mockSvc.Setup(s => s.GetPackageVersions("Test"))
            .ReturnsAsync(versions);
        mockSvc.Setup(s => s.GetPackageMetadata("Test", It.IsAny<string>()))
            .ReturnsAsync(new NuGetPackageInfo { Id = "Test", Version = "1.0.0" });
        var ctrl = new PackageMetadataController(mockSvc.Object, Mock.Of<Microsoft.Extensions.Logging.ILogger<PackageMetadataController>>());
        var result = await ctrl.GetPackageVersions("Test");
        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        var dto = ok!.Value as RegistrationIndexDto;
        dto.Should().NotBeNull();
        var allVersions = dto!.Items.SelectMany(p => p.Items.Select(l => l.CatalogEntry.Version)).ToArray();
        var sorted = allVersions.OrderBy(v => NuGet.Versioning.NuGetVersion.Parse(v)).ToArray();
        allVersions.Should().BeEquivalentTo(sorted, o => o.WithStrictOrdering());
    }
}
