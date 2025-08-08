using NuGetServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NuGetServer.Controllers;

[ApiController]
[Route("nuget/metadata")]
public class PackageMetadataController : ControllerBase
{
    private readonly IPackageStorageService _packageStorageService;
    private readonly ILogger<PackageMetadataController> _logger;

    public PackageMetadataController(
        IPackageStorageService packageStorageService,
        ILogger<PackageMetadataController> logger)
    {
        _packageStorageService = packageStorageService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("{id}/index.json")]
    public async Task<IActionResult> GetPackageVersions(string id)
    {
        var versions = await _packageStorageService.GetPackageVersions(id);
        return Ok(new { versions });
    }

    [AllowAnonymous]
    [HttpGet("{id}/{version}.json")]
    public async Task<IActionResult> GetPackageMetadata(string id, string version)
    {
        var metadata = await _packageStorageService.GetPackageMetadata(id, version);
        if (metadata == null)
            return NotFound(new { message = "Package metadata not found" });

        return Ok(metadata);
    }
}
