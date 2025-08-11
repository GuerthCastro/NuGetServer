using NuGetServer.Extensions;
using NuGetServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace NuGetServer.Controllers;

[ApiController]
[Route("v3")]
public class PackagePublishController : ControllerBase
{
    private readonly ILogger<PackagePublishController> _logger;
    private readonly IPackageStorageService _packageStorageService;
    private readonly Entities.Config.NuGetServer _nuGetServer;

    public PackagePublishController(ILogger<PackagePublishController> logger, IPackageStorageService packageStorageService, Entities.Config.NuGetServer nuGetServer)
    {
        _logger = logger;
        _packageStorageService = packageStorageService;
        _nuGetServer = nuGetServer;
    }

    [HttpPut]
    [RequestSizeLimit(52428800)] // 50 MB
    public async Task<IActionResult> PublishPackage([FromHeader(Name = "X-Api-Key")] string? apiKey)
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

            var result = await _packageStorageService.SavePackage(file.OpenReadStream(), file.FileName);

            if (!result.Success)
                return BadRequest(result.Message.ToProblem(400));

            _logger.LogInformation("NuGet package {File} published successfully.", file.FileName);
            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing package");
            return Problem(detail: ex.Message, statusCode: 500, title: "Internal Server Error");
        }
    }

    [HttpDelete("{id}/{version}")]
    public async Task<IActionResult> DeletePackage(
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        string id,
        string version)
    {
        try
        {
            if (apiKey != _nuGetServer.ApiKey)
                return Unauthorized("Invalid API key".ToProblem(401));

            var success = await _packageStorageService.DeletePackage(id, version);
            if (!success)
                return NotFound("Package not found".ToProblem(404));

            _logger.LogInformation("NuGet package {Id} {Version} deleted successfully.", id, version);
            return Ok(new { message = $"Package {id} {version} deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting package");
            return Problem(detail: ex.Message, statusCode: 500, title: "Internal Server Error");
        }
    }
}
