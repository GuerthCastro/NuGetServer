using NuGet.Versioning;
using NuGetServer.Entities.DTO;
using NuGetServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NuGetServer.Controllers;

[ApiController]
[Route("v3/registrations")]
public class PackageMetadataController : ControllerBase
{
    private readonly IPackageStorageService _packageStorageService;
    private readonly ILogger<PackageMetadataController> _logger;
    private readonly NuGetIndex _nuGetIndex;

    public PackageMetadataController(IPackageStorageService packageStorageService, ILogger<PackageMetadataController> logger, NuGetIndex nuGetIndex)
    {
        _packageStorageService = packageStorageService;
        _logger = logger;
        _nuGetIndex = nuGetIndex;
    }

    [AllowAnonymous]
    [HttpGet("{id}/index.json")]
    public async Task<IActionResult> GetPackageVersions(string id, [FromQuery(Name = "semVerLevel")] string semVerLevel = null)
    {
        var versions = await _packageStorageService.GetPackageVersions(id);
        if (versions == null || !versions.Any())
            return NotFound();
        var baseUrl = _nuGetIndex.BaseUrl;
        var lowerId = id.ToLowerInvariant();
        var normalizedVersions = versions.Select(v => NuGetVersion.Parse(v).ToNormalizedString()).OrderBy(NuGetVersion.Parse).ToList();
        var leaves = new List<RegistrationLeafDto>();
        foreach (var v in normalizedVersions)
        {
            var meta = await _packageStorageService.GetPackageMetadata(id, v);
            var canonicalId = meta?.Id ?? lowerId;
            var published = DateTimeOffset.UtcNow;
            leaves.Add(new RegistrationLeafDto
            {
                Type = "Package",
                PackageContent = $"{baseUrl}/v3/v3-flatcontainer/{lowerId}/{v}/{lowerId}.{v}.nupkg",
                Registration = $"{baseUrl}/v3/registrations/{lowerId}/index.json",
                CatalogEntry = new CatalogEntryDto
                {
                    CanonicalId = canonicalId,
                    Id = id,
                    Version = v,
                    Listed = true,
                    Published = published
                }
            });
        }
        var firstVersion = normalizedVersions.FirstOrDefault() ?? string.Empty;
        var lastVersion = normalizedVersions.LastOrDefault() ?? string.Empty;
        var page = new RegistrationPageDto
        {
            Registration = $"{baseUrl}/v3/registrations/{lowerId}/index.json#page/{firstVersion}/{lastVersion}",
            Type = "catalog:CatalogPage",
            Count = leaves.Count,
            Lower = firstVersion,
            Upper = lastVersion,
            Parent = $"{baseUrl}/v3/registrations/{lowerId}/index.json",
            Items = leaves
        };
        var dto = new RegistrationIndexDto
        {
            Registration = $"{baseUrl}/v3/registrations/{lowerId}/index.json",
            Type = "catalog:CatalogRoot",
            Context = new RegistrationContext { Vocab = "http://schema.nuget.org/schema#", Base = baseUrl },
            Count = 1,
            Items = new[] { page }
        };
        return Ok(dto);
    }

    [AllowAnonymous]
    [HttpGet("{id}/{version}.json")]
    public async Task<IActionResult> GetPackageMetadata(string id, string version)
    {
        try 
        {
            var metadata = await _packageStorageService.GetPackageMetadata(id, version);
            if (metadata == null)
                return NotFound(new ErrorResponse { Message = $"Package '{id}' version '{version}' not found" });

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var lowerId = id.ToLowerInvariant();
            
            var catalogEntry = new PackageCatalogEntry
            {
                Id = $"{baseUrl}/v3/catalog/{lowerId}/{version}.json",
                Type = "PackageDetails",
                Authors = metadata.Authors ?? "",
                Description = metadata.Description ?? "",
                PackageId = metadata.Id,
                Title = metadata.Id,
                Version = metadata.Version,
                Summary = metadata.Description ?? "",
                IsLatestVersion = false, 
                Listed = true,
                Downloads = metadata.DownloadCount,
                IconUrl = "",
                LicenseUrl = "",
                ProjectUrl = "",
                Tags = Array.Empty<string>(),
                Verified = true
            };
            
            var allVersions = await _packageStorageService.GetPackageVersions(id);
            if (allVersions.Any() && version.Equals(allVersions.OrderByDescending(v => v).FirstOrDefault(), StringComparison.OrdinalIgnoreCase))
            {
                catalogEntry.IsLatestVersion = true;
            }

            var response = new PackageMetadata
            {
                Id = $"{baseUrl}/v3/registrations/{lowerId}/{version}.json",
                Type = "Package",
                Context = new RegistrationContext
                {
                    Vocab = "http://schema.nuget.org/schema#",
                    Base = baseUrl
                },
                CatalogEntry = catalogEntry,
                PackageContent = $"{baseUrl}/v3/v3-flatcontainer/{lowerId}/{version}/{lowerId}.{version}.nupkg",
                Registration = $"{baseUrl}/v3/registrations/{lowerId}/index.json"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting package metadata for {Id} {Version}", id, version);
            return StatusCode(500, new ErrorResponse { Message = "Internal server error" });
        }
    }
}
