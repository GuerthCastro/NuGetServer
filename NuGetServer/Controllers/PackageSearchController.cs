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
// Route for /v3 and /v3/
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
                new() { Id = $"{baseUrl}/v3/registrations/",    Type = "RegistrationsBaseUrl/3.6.0" },
                new() { Id = $"{baseUrl}/v3/query",             Type = "SearchQueryService/3.0.0-beta" },
                new() { Id = $"{baseUrl}/v3/query",             Type = "SearchQueryService" },
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
            var packages = await _packageStorageService.GetAllPackages();
            return Ok(new { totalHits = packages.Count, data = packages });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing packages");
            return Problem(detail: ex.Message, statusCode: 500, title: "Internal Server Error");
        }
    }

    [AllowAnonymous]
    [HttpGet("query")]
    public async Task<IActionResult> SearchPackages(
        [FromQuery(Name = "q")] string? q = null,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 20,
        [FromQuery(Name = "prerelease")] bool prerelease = false)
    {
        var packages = await _packageStorageService.GetPackagesWithMetadata();

        if (!string.IsNullOrWhiteSpace(q))
            packages = packages.Where(p => p.Id.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!prerelease)
            packages = packages.Where(p => !p.Version.Contains('-')).ToList();

        var paginated = packages.Skip(skip).Take(take).ToList();

        return Ok(new
        {
            totalHits = packages.Count,
            data = paginated.Select(p => new
            {
                id = p.Id,
                version = p.Version,
                versions = new[] { new { version = p.Version } },
                description = p.Description ?? "",
                authors = string.IsNullOrEmpty(p.Authors) ? Array.Empty<string>() : p.Authors.Split(',')
            })
        });
    }

    [AllowAnonymous]
    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery(Name = "q")] string? q = null, [FromQuery] int take = 20)
    {
        var packages = await _packageStorageService.GetAllPackages();

        if (!string.IsNullOrWhiteSpace(q))
            packages = packages.Where(p => p.Id.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        var ids = packages.Select(p => p.Id).Distinct().Take(take).ToArray();
        return Ok(new { totalHits = ids.Length, data = ids });
    }

    [AllowAnonymous]
    [HttpGet("v3-flatcontainer/{id}/index.json")]
    public async Task<IActionResult> GetPackageVersions(string id)
    {
        var versions = await _packageStorageService.GetPackageVersions(id);
        return Ok(new { versions });
    }

    [AllowAnonymous]
    [HttpGet("v3-flatcontainer/{id}/{version}/{fileName}")]
    public async Task<IActionResult> DownloadPackage(string id, string version, string fileName)
    {
        try
        {
            var (stream, contentType) = await _packageStorageService.GetPackageStream(id, version);
            if (stream == null)
                return NotFound("Package not found".ToProblem(404));
            return File(stream, contentType ?? "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading package");
            return Problem(detail: ex.Message, statusCode: 500, title: "Internal Server Error");
        }
    }
}
