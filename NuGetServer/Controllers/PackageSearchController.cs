using NuGetServer.Entities.Config;
using NuGetServer.Entities.DTO;
using NuGetServer.Extensions;
using NuGetServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace NuGetServer.Controllers;

[ApiController]
[Route("v3")]
public class PackageSearchController : ControllerBase
{
    private readonly ILogger<PackageSearchController> _logger;
    private readonly Entities.Config.NuGetServer _nuGetServer;
    private readonly NuGetIndex _nuGetIndex;
    private readonly IPackageStorageService _packageStorageService;

    public PackageSearchController(ILogger<PackageSearchController> logger, ServiceConfig serviceConfig, Entities.Config.NuGetServer nuGetServer, NuGetIndex nuGetIndex, IPackageStorageService packageStorageService)
    {
        _logger = logger;
        _nuGetServer = nuGetServer;
        _nuGetIndex = nuGetIndex;
        _packageStorageService = packageStorageService;
    }


    [AllowAnonymous]
    [HttpHead("index.json")]
    public IActionResult HeadIndex()
    {
        Response.ContentType = "application/json";
        return Ok();
    }

    [AllowAnonymous]
    [HttpHead("search")]
    public IActionResult HeadSearch()
    {
        Response.ContentType = "application/json";
        return Ok();
    }

    [AllowAnonymous]
    [HttpHead("autocomplete")]
    public IActionResult HeadAutocomplete()
    {
        Response.ContentType = "application/json";
        return Ok();
    }


    [AllowAnonymous]
    [HttpGet("")]
    [HttpGet("index.json")]
    public IActionResult GetIndex()
    {
        var baseUrl = _nuGetIndex.ServiceUrl.TrimEnd('/');
        var index = new ServiceIndex
        {
            Resources = new()
            {
                new() { Id = $"{baseUrl}/v3/v3-flatcontainer/", Type = "PackageBaseAddress/3.0.0" },
                new() { Id = $"{baseUrl}/v3/registrations/",    Type = "RegistrationsBaseUrl" },
                new() { Id = $"{baseUrl}/v3/registrations/",    Type = "RegistrationsBaseUrl/3.0.0-rc" },
                new() { Id = $"{baseUrl}/v3/registrations/",    Type = "RegistrationsBaseUrl/3.6.0" },
                new() { Id = $"{baseUrl}/v3/query",             Type = "SearchQueryService/3.0.0-beta" },
                new() { Id = $"{baseUrl}/v3/query",             Type = "SearchQueryService" },
                new() { Id = $"{baseUrl}/v3/search",            Type = "SearchQueryService/3.0.0-beta" },
                new() { Id = $"{baseUrl}/v3/search",            Type = "SearchQueryService" },
                new() { Id = $"{baseUrl}/v3/autocomplete",      Type = "SearchAutocompleteService/3.0.0-beta" },
                new() { Id = $"{baseUrl}/v3/autocomplete",      Type = "SearchAutocompleteService" },
                new() { Id = $"{baseUrl}/v3/upload",            Type = "PackagePublish/2.0.0" }
            }
        };

        return Ok(index);
    }

    [AllowAnonymous]
    [HttpGet("list")]
    public async Task<IActionResult> GetAllPackages()
    {
        try
        {
            var allPackages = await _packageStorageService.GetPackagesWithMetadata();
            
            var packagesByIdWithVersions = allPackages
                .GroupBy(p => p.Id)
                .ToDictionary(
                    g => g.Key, 
                    g => g.OrderByDescending(p => p.Version).ToList()
                );
                
            var response = new PackageDetailsResponse
            {
                TotalHits = packagesByIdWithVersions.Count,
                Data = new List<PackageDetail>()
            };

            foreach (var packageId in packagesByIdWithVersions.Keys)
            {
                var packageVersions = packagesByIdWithVersions[packageId];
                var latestPackage = packageVersions.First();
                
                var baseUrl = _nuGetIndex.ServiceUrl.TrimEnd('/');
                var lowerId = latestPackage.Id.ToLowerInvariant();
                
                var packageDetail = new PackageDetail
                {
                    Type = "Package",
                    Registration = $"{baseUrl}/v3/registrations/{lowerId}/index.json",
                    Id = latestPackage.Id,
                    Version = latestPackage.Version,
                    Description = latestPackage.Description ?? "",
                    Summary = latestPackage.Description ?? "",
                    Title = latestPackage.Id,
                    Authors = string.IsNullOrEmpty(latestPackage.Authors) ? Array.Empty<string>() : latestPackage.Authors.Split(','),
                    IconUrl = "",
                    LicenseUrl = "",
                    ProjectUrl = "",
                    Tags = Array.Empty<string>(),
                    TotalDownloads = 0,
                    Verified = true,
                    PackageTypes = new List<PackageTypeInfo> { new PackageTypeInfo { Name = "Dependency", Version = "" } },
                    Versions = packageVersions.Select(p => new VersionDetail 
                    { 
                        Id = $"{baseUrl}/v3/registrations/{lowerId}/{p.Version}.json",
                        Version = p.Version,
                        Downloads = 0,
                        Type = "PackageDetails"
                    }).ToList()
                };
                
                response.Data.Add(packageDetail);
            }
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing packages");
            return Problem(detail: ex.Message, statusCode: 500, title: "Internal Server Error");
        }
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<IActionResult> SearchPackagesV3(
        [FromQuery(Name = "q")] string? q = null,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 20,
        [FromQuery(Name = "prerelease")] bool prerelease = false,
        [FromQuery(Name = "allVersions")] bool allVersions = false)
    {
        return await SearchPackages(q, skip, take, prerelease, allVersions);
    }

    [AllowAnonymous]
    [HttpGet("query")]
    public async Task<IActionResult> SearchPackages(
        [FromQuery(Name = "q")] string? q = null,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 20,
        [FromQuery(Name = "prerelease")] bool prerelease = false,
        [FromQuery(Name = "allVersions")] bool allVersions = false)
    {
        try
        {
            var allPackages = await _packageStorageService.GetPackagesWithMetadata();

            if (!string.IsNullOrWhiteSpace(q))
                allPackages = allPackages.Where(p => p.Id.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!prerelease)
                allPackages = allPackages.Where(p => !p.Version.Contains('-')).ToList();

            var packagesByIdWithVersions = allPackages
                .GroupBy(p => p.Id)
                .ToDictionary(
                    g => g.Key, 
                    g => g.OrderByDescending(p => p.Version).ToList()
                );

            var paginated = packagesByIdWithVersions.Keys
                .Skip(skip)
                .Take(take)
                .ToList();

            var response = new PackageDetailsResponse
            {
                TotalHits = packagesByIdWithVersions.Count,
                Data = new List<PackageDetail>()
            };

            _logger.LogInformation($"Search requested with allVersions={allVersions}");

            foreach (var packageId in paginated)
            {
                var packageVersions = packagesByIdWithVersions[packageId];
                var baseUrl = _nuGetIndex.ServiceUrl.TrimEnd('/');
                var lowerId = packageId.ToLowerInvariant();
                
                if (allVersions)
                {
                    foreach (var package in packageVersions)
                    {
                        var packageDetail = new PackageDetail
                        {
                            Type = "Package",
                            Registration = $"{baseUrl}/v3/registrations/{lowerId}/index.json",
                            Id = package.Id,
                            Version = package.Version,
                            Description = package.Description ?? "",
                            Summary = package.Description ?? "",
                            Title = package.Id,
                            Authors = string.IsNullOrEmpty(package.Authors) ? Array.Empty<string>() : package.Authors.Split(','),
                            IconUrl = "",
                            LicenseUrl = "",
                            ProjectUrl = "",
                            Tags = Array.Empty<string>(),
                            TotalDownloads = package.DownloadCount,
                            Verified = true,
                            PackageTypes = new List<PackageTypeInfo> { new PackageTypeInfo { Name = "Dependency", Version = "" } },
                            Versions = new List<VersionDetail>
                            {
                                new VersionDetail
                                {
                                    Id = $"{baseUrl}/v3/registrations/{lowerId}/{package.Version}.json",
                                    Version = package.Version,
                                    Downloads = package.DownloadCount,
                                    Type = "PackageDetails"
                                }
                            }
                        };
                        
                        response.Data.Add(packageDetail);
                    }
                }
                else
                {
                    var latestPackage = packageVersions.First();
                
                    var packageDetail = new PackageDetail
                    {
                        Type = "Package",
                        Registration = $"{baseUrl}/v3/registrations/{lowerId}/index.json",
                        Id = latestPackage.Id,
                        Version = latestPackage.Version,
                        Description = latestPackage.Description ?? "",
                        Summary = latestPackage.Description ?? "",
                        Title = latestPackage.Id,
                        Authors = string.IsNullOrEmpty(latestPackage.Authors) ? Array.Empty<string>() : latestPackage.Authors.Split(','),
                        IconUrl = "",
                        LicenseUrl = "",
                        ProjectUrl = "",
                        Tags = Array.Empty<string>(),
                        TotalDownloads = packageVersions.Sum(p => p.DownloadCount),
                        Verified = true,
                        PackageTypes = new List<PackageTypeInfo> { new PackageTypeInfo { Name = "Dependency", Version = "" } },
                        Versions = packageVersions.Select(p => new VersionDetail 
                        { 
                            Id = $"{baseUrl}/v3/registrations/{lowerId}/{p.Version}.json",
                            Version = p.Version,
                            Downloads = p.DownloadCount,
                            Type = "PackageDetails"
                        }).ToList()
                    };
                    
                    response.Data.Add(packageDetail);
                }
            }
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in search endpoint: {Message}", ex.Message);
            return Problem(detail: ex.Message, statusCode: 500, title: "Internal Server Error");
        }
    }

    [AllowAnonymous]
    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery(Name = "q")] string? q = null, [FromQuery] int take = 20)
    {
        var packages = await _packageStorageService.GetAllPackages();

        if (!string.IsNullOrWhiteSpace(q))
            packages = packages.Where(p => p.Id.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        var ids = packages.Select(p => p.Id).Distinct().Take(take).ToArray();
        
        var response = new AutocompleteResponse
        {
            TotalHits = ids.Length,
            Data = ids
        };
        
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("v3-flatcontainer/{id}/index.json")]
    public async Task<IActionResult> GetPackageVersions(string id)
    {
        _logger.LogInformation("Received request for package versions: {Id}", id);
        var versions = await _packageStorageService.GetPackageVersions(id);
        Response.Headers.Append("Content-Type", "application/json");
        
        var response = new VersionsResponse
        {
            Versions = versions
        };
        
        return Ok(response);
    }
    
    [AllowAnonymous]
    [HttpGet("v3-flatcontainer/{id}/{version}/index.json")]
    public async Task<IActionResult> GetPackageVersionInfo(string id, string version)
    {
        if (await _packageStorageService.PackageExists(id, version))
        {
            _logger.LogInformation("Package {Id} {Version} exists", id, version);
            return Ok(new { });  // Empty JSON object is fine here
        }
        return NotFound();
    }

    [AllowAnonymous]
    [HttpGet("v3-flatcontainer/{id}/{version}/{fileName}")]
    public async Task<IActionResult> DownloadPackage(string id, string version, string fileName)
    {
        try
        {
            _logger.LogInformation("Downloading package {Id} {Version} file: {FileName}", id, version, fileName);
            var (stream, contentType) = await _packageStorageService.GetPackageStream(id, version);
            if (stream == null)
            {
                _logger.LogWarning("Package {Id} {Version} not found for download", id, version);
                return NotFound("Package not found".ToProblem(404));
            }
            
            _logger.LogInformation("Serving package file with content type: {ContentType}", contentType);
            return File(stream, contentType ?? "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading package");
            return Problem(detail: ex.Message, statusCode: 500, title: "Internal Server Error");
        }
    }
}
