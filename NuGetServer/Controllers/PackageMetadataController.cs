using NuGetServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGetServer.Entities.DTO;
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
            
            // Fetch actual package info with metadata and download counts
            var latestVersion = versions.OrderByDescending(v => v).FirstOrDefault() ?? "";
            var registrationItemTasks = versions.Select(async v => 
            {
                var metadata = await _packageStorageService.GetPackageMetadata(id, v);
                return new PackageItem
                {
                    Id = $"{baseUrl}/v3/registrations/{lowerId}/{v}.json",
                    Type = "Package",
                    CatalogEntry = new PackageCatalogEntry
                    {
                        Id = $"{baseUrl}/v3/catalog/{lowerId}/{v}.json",
                        Type = "PackageDetails",
                        Authors = metadata?.Authors ?? "",
                        PackageId = id,
                        Version = v,
                        Description = metadata?.Description ?? "",
                        Title = id,
                        Summary = metadata?.Description ?? "",
                        IsLatestVersion = (v == latestVersion), // Only the latest version is marked as latest
                        Listed = true, // All versions are listed
                        Downloads = metadata?.DownloadCount ?? 0 // Use actual download count
                    },
                    PackageContent = $"{baseUrl}/v3/v3-flatcontainer/{lowerId}/{v}/{lowerId}.{v}.nupkg",
                };
            }).ToList();
            
            // Wait for all metadata to be fetched
            var registrationItems = new List<PackageItem>();
            foreach (var task in registrationItemTasks)
            {
                registrationItems.Add(await task);
            }

            var response = new PackageRegistration
            {
                Id = $"{baseUrl}/v3/registrations/{lowerId}/index.json",
                Type = "catalog:CatalogRoot",
                Context = new RegistrationContext
                {
                    Vocab = "http://schema.nuget.org/schema#",
                    Base = baseUrl
                },
                Count = versions.Count,
                Items = new List<RegistrationPage>
                {
                    new RegistrationPage
                    {
                        Id = $"{baseUrl}/v3/registrations/{lowerId}/page/0.json",
                        Type = "catalog:CatalogPage",
                        Count = versions.Count,
                        Lower = versions.OrderBy(v => v).FirstOrDefault() ?? "",
                        Upper = versions.OrderByDescending(v => v).FirstOrDefault() ?? "",
                        Items = registrationItems
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
            
            var catalogEntry = new PackageCatalogEntry
            {
                Id = $"{baseUrl}/v3/catalog/{lowerId}/{version}.json",
                Type = "PackageDetails",
                Authors = metadata.Authors ?? "",
                Description = metadata.Description ?? "",
                PackageId = metadata.Id,
                Title = metadata.Id,
                Version = metadata.Version
            };

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
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
