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

            // Format response according to NuGet v3 protocol with enhanced format for Visual Studio 2022
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var packageBaseUrl = $"{baseUrl}/v3/registrations/{id}";
            var lowerId = id.ToLowerInvariant();
            
            var response = new
            {
                @context = new
                {
                    vocab = "http://schema.nuget.org/schema#",
                    @base = baseUrl
                },
                @id = $"{baseUrl}/v3/registrations/{lowerId}/index.json",
                @type = "catalog:CatalogRoot",
                count = versions.Count,
                items = new[]
                {
                    new
                    {
                        @id = $"{baseUrl}/v3/registrations/{lowerId}/page/0.json",
                        @type = "catalog:CatalogPage",
                        count = versions.Count,
                        lower = versions.OrderBy(v => v).FirstOrDefault(),
                        upper = versions.OrderByDescending(v => v).FirstOrDefault(),
                        items = versions.Select(v => new
                        {
                            @id = $"{baseUrl}/v3/registrations/{lowerId}/{v}.json",
                            @type = "Package",
                            catalogEntry = new
                            {
                                @id = $"{baseUrl}/v3/catalog/{lowerId}/{v}.json",
                                @type = "PackageDetails",
                                authors = "",
                                id = id,
                                version = v,
                                description = "",
                                title = id
                            },
                            packageContent = $"{baseUrl}/v3/v3-flatcontainer/{lowerId}/{v}/{lowerId}.{v}.nupkg",
                            registration = $"{baseUrl}/v3/registrations/{lowerId}/{v}.json"
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

            // Format response according to NuGet v3 protocol with enhanced format for VS2022
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var lowerId = id.ToLowerInvariant();
            
            var response = new
            {
                @context = new
                {
                    vocab = "http://schema.nuget.org/schema#",
                    base = baseUrl
                },
                @id = $"{baseUrl}/v3/registrations/{lowerId}/{version}.json",
                @type = "Package",
                catalogEntry = new
                {
                    @id = $"{baseUrl}/v3/catalog/{lowerId}/{version}.json",
                    @type = "PackageDetails",
                    authors = metadata.Authors ?? "",
                    description = metadata.Description ?? "",
                    id = metadata.Id,
                    title = metadata.Id,
                    version = metadata.Version,
                    summary = metadata.Description ?? "",
                    tags = new string[] { },
                    packageTypes = new[] { new { name = "Dependency", version = "" } },
                    licenseUrl = "",
                    projectUrl = "",
                    iconUrl = ""
                },
                packageContent = $"{baseUrl}/v3/v3-flatcontainer/{lowerId}/{version}/{lowerId}.{version}.nupkg",
                registration = $"{baseUrl}/v3/registrations/{lowerId}/index.json"
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
