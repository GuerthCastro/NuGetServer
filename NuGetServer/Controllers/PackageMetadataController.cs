using NuGetServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NuGetServer.Controllers;

[ApiController]
[Route("v3/registrations")]
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
        try
        {
            var versions = await _packageStorageService.GetPackageVersions(id);
            if (versions == null || !versions.Any())
                return NotFound(new { message = $"Package '{id}' not found" });

            // Format response according to NuGet v3 protocol
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var packageBaseUrl = $"{baseUrl}/v3/registrations/{id}";
            
            var response = new
            {
                count = versions.Count,
                items = new[]
                {
                    new
                    {
                        items = versions.Select(v => new
                        {
                            catalogEntry = new
                            {
                                id = id,
                                version = v
                            },
                            packageContent = $"{baseUrl}/v3/v3-flatcontainer/{id.ToLowerInvariant()}/{v}/{id.ToLowerInvariant()}.{v}.nupkg"
                        }).ToArray()
                    }
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting package versions for {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [AllowAnonymous]
    [HttpGet("{id}/{version}.json")]
    public async Task<IActionResult> GetPackageMetadata(string id, string version)
    {
        try 
        {
            var metadata = await _packageStorageService.GetPackageMetadata(id, version);
            if (metadata == null)
                return NotFound(new { message = $"Package '{id}' version '{version}' not found" });

            // Format response according to NuGet v3 protocol
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            
            var response = new
            {
                catalogEntry = metadata,
                packageContent = $"{baseUrl}/v3/v3-flatcontainer/{id.ToLowerInvariant()}/{version}/{id.ToLowerInvariant()}.{version}.nupkg",
                registration = $"{baseUrl}/v3/registrations/{id}/{version}.json"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting package metadata for {Id} {Version}", id, version);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
