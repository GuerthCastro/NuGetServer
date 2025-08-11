using NuGetServer.Entities.Config;
using NuGetServer.Extensions;
using NuGetServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NuGetServer.Controllers;

[ApiController]
[Route("v3")]
public class PackagesController : ControllerBase
{
    private readonly ILogger<PackagesController> _logger;
    private readonly ServiceConfig _serviceConfig;
    private readonly Entities.Config.NuGetServer _nuGetServer;
    private readonly IPackageStorageService _packageStorageService;

    public PackagesController(ILogger<PackagesController> logger, ServiceConfig serviceConfig, Entities.Config.NuGetServer settings, IPackageStorageService packageStorageService)
    {
        _logger = logger;
        _serviceConfig = serviceConfig;
        _serviceConfig.ServiceName = _serviceConfig.ServiceName.Replace("{controllerName}", GetType().Name);
        _nuGetServer = settings;
        _packageStorageService = packageStorageService;
    }

    [AllowAnonymous]
    [HttpGet("health")]
    public IActionResult Health() => Ok(_serviceConfig);

    [HttpPut("upload")]
    [RequestSizeLimit(52428800)]
    public async Task<IActionResult> UploadPackage([FromHeader(Name = "X-Api-Key")] string? apiKey)
    {
        try
        {
            if (apiKey != _nuGetServer.ApiKey)
                return Unauthorized("Invalid API key".ToProblem(401));

            if (Request.Form.Files.Count == 0)
                return BadRequest("No files uploaded".ToProblem(400));

            var file = Request.Form.Files[0];
            if (!file.FileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .nupkg files are allowed".ToProblem(400));

            await using var stream = file.OpenReadStream();
            var (success, message) = await _packageStorageService.SavePackage(stream, file.FileName);

            if (!success)
                return BadRequest(message.ToProblem(400));

            _logger.LogInformation("Package {PackageFile} uploaded successfully", file.FileName);
            return Ok(new { message = $"Package {file.FileName} uploaded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading package");
            return Problem(detail: ex.Message, statusCode: 500, title: "Internal Server Error");
        }
    }

    [HttpGet("download/{id}/{version}")]
    public async Task<IActionResult> DownloadPackage(string id, string version)
    {
        try
        {
            var (stream, contentType) = await _packageStorageService.GetPackageStream(id, version);
            if (stream == null)
                return NotFound("Package not found".ToProblem(404));
            return File(stream, contentType ?? "application/octet-stream", $"{id}.{version}.nupkg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading package");
            return Problem(detail: ex.Message, statusCode: 500, title: "Internal Server Error");
        }
    }
}
