using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NuGetServer.Controllers;
using NuGetServer.Entities.DTO;
using NuGetServer.Services;
using Moq;
using Xunit;

namespace NuGetServer.Tests.Controllers;

public class ControllerResponseDtoTests
{
    [Fact]
    public async Task GetPackageMetadata_Returns_ErrorResponse_Dto()
    {
        var mockSvc = new Mock<IPackageStorageService>();
        mockSvc.Setup(s => s.GetPackageMetadata("Test", "1.0.0")).ReturnsAsync((NuGetPackageInfo?)null);
        var ctrl = new PackageMetadataController(mockSvc.Object, Mock.Of<Microsoft.Extensions.Logging.ILogger<PackageMetadataController>>());
        var result = await ctrl.GetPackageMetadata("Test", "1.0.0");
        var notFound = result as NotFoundObjectResult;
        notFound.Should().NotBeNull();
        notFound!.Value.Should().BeOfType<ErrorResponse>();
    }

    [Fact]
    public void PackageSearchController_EmptyResponse_Dto()
    {
        var ctrl = new PackageSearchController(Mock.Of<Microsoft.Extensions.Logging.ILogger<PackageSearchController>>(), new NuGetServer.Entities.Config.ServiceConfig(), new NuGetServer.Entities.Config.NuGetServer(), new NuGetServer.Entities.Config.NuGetIndex(), Mock.Of<IPackageStorageService>());
        var result = ctrl.GetIndex();
        result.Should().BeOfType<OkObjectResult>();
    }
}
