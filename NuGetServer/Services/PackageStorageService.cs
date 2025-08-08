using NuGetServer.Entities.Config;
using NuGetServer.Entities.DTO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NuGetServer.Services;

public class PackageStorageService : IPackageStorageService
{
    private readonly ILogger<PackageStorageService> _logger;
    private readonly Entities.Config.NuGetServer _nuGetServer;
    private readonly NuGetIndex _nuGetIndex;
    private static readonly Regex versionRegex = new(@"(?<version>\d+\.\d+\.\d+(-[0-9A-Za-z\-.]+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public PackageStorageService(ILogger<PackageStorageService> logger, Entities.Config.NuGetServer nuGetServer, NuGetIndex nuGetIndex)
    {
        _logger = logger;
        _nuGetServer = nuGetServer;
        _nuGetIndex = nuGetIndex;

        if (!Directory.Exists(_nuGetServer.PackagesPath))
        {
            Directory.CreateDirectory(_nuGetServer.PackagesPath);
        }
    }

    public async Task<(bool Success, string Message)> SavePackage(Stream packageStream, string fileName)
    {
        if (!fileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
            return (false, "Only .nupkg files are allowed");

        var (packageId, version) = ExtractPackageInfo(fileName);
        if (string.IsNullOrEmpty(packageId) || string.IsNullOrEmpty(version))
            return (false, "Invalid package name format. Expected <PackageId>.<Version>.nupkg");

        var packageFolder = Path.Combine(_nuGetServer.PackagesPath, packageId, version);
        Directory.CreateDirectory(packageFolder);

        var savePath = Path.Combine(packageFolder, fileName);
        await using (var fileStream = new FileStream(savePath, FileMode.Create))
        {
            await packageStream.CopyToAsync(fileStream);
        }

        _logger.LogInformation("Package {PackageId} {Version} saved at {Path}", packageId, version, savePath);
        return (true, $"Package {fileName} uploaded successfully");
    }

    public async Task<(Stream? Stream, string? ContentType)> GetPackageStream(string packageId, string version)
    {
        var packageFolder = Path.Combine(_nuGetServer.PackagesPath, packageId, version);
        if (!Directory.Exists(packageFolder)) return (null, null);

        var packagePath = Directory.GetFiles(packageFolder, "*.nupkg").FirstOrDefault();
        if (packagePath == null) return (null, null);

        var stream = new FileStream(packagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (stream, "application/octet-stream");
    }

    public Task<bool> DeletePackage(string packageId, string version)
    {
        var packageFolder = Path.Combine(_nuGetServer.PackagesPath, packageId, version);
        if (!Directory.Exists(packageFolder)) return Task.FromResult(false);

        try
        {
            Directory.Delete(packageFolder, true);
            _logger.LogInformation("Package {PackageId} {Version} deleted.", packageId, version);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting package {PackageId} {Version}", packageId, version);
            return Task.FromResult(false);
        }
    }

    public async Task<List<NuGetPackageInfo>> GetAllPackages()
    {
        var result = new List<NuGetPackageInfo>();
        var baseUrl = _nuGetIndex.ServiceUrl.TrimEnd('/');

        if (!Directory.Exists(_nuGetServer.PackagesPath))
            return result;

        foreach (var idDir in Directory.GetDirectories(_nuGetServer.PackagesPath))
        {
            var id = Path.GetFileName(idDir);
            foreach (var versionDir in Directory.GetDirectories(idDir))
            {
                var version = Path.GetFileName(versionDir);
                var packageFile = Path.Combine(versionDir, $"{id}.{version}.nupkg");

                if (File.Exists(packageFile))
                {
                    result.Add(new NuGetPackageInfo
                    {
                        Id = id,
                        Version = version,
                        FileName = Path.GetFileName(packageFile),
                        DownloadUrl = $"{baseUrl}/download/{id}/{version}"
                    });
                }
            }
        }
        return result;
    }

    public async Task<List<NuGetPackageInfo>> GetPackagesWithMetadata()
    {
        var result = new List<NuGetPackageInfo>();
        var basePackages = await GetAllPackages();

        foreach (var pkg in basePackages)
        {
            string? description = null;
            string? authors = null;

            try
            {
                using var zip = ZipFile.OpenRead(Path.Combine(_nuGetServer.PackagesPath, pkg.Id, pkg.Version, pkg.FileName));
                var nuspecEntry = zip.Entries.FirstOrDefault(e => e.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
                if (nuspecEntry != null)
                {
                    using var stream = nuspecEntry.Open();
                    var doc = XDocument.Load(stream);
                    description = doc.Root?.Element("metadata")?.Element("description")?.Value;
                    authors = doc.Root?.Element("metadata")?.Element("authors")?.Value;
                }
            }
            catch
            {
                // Ignore metadata read errors
            }

            result.Add(new NuGetPackageInfo
            {
                Id = pkg.Id,
                Version = pkg.Version,
                FileName = pkg.FileName,
                DownloadUrl = pkg.DownloadUrl,
                Description = description,
                Authors = authors
            });
        }

        return result;
    }

    public Task<List<string>> GetPackageVersions(string packageId)
    {
        var packageDir = Path.Combine(_nuGetServer.PackagesPath, packageId);
        if (!Directory.Exists(packageDir))
            return Task.FromResult(new List<string>());

        return Task.FromResult(Directory.GetDirectories(packageDir)
                        .Select(Path.GetFileName)
                        .OrderBy(v => v)
                        .ToList());
    }

    public Task<bool> PackageExists(string packageId, string version)
    {
        var packageFolder = Path.Combine(_nuGetServer.PackagesPath, packageId, version);
        return Task.FromResult(Directory.Exists(packageFolder) && Directory.GetFiles(packageFolder, "*.nupkg").Any());
    }

    public async Task<NuGetPackageInfo?> GetPackageMetadata(string packageId, string version)
    {
        var packageFolder = Path.Combine(_nuGetServer.PackagesPath, packageId, version);
        if (!Directory.Exists(packageFolder))
            return null;

        var nupkg = Directory.GetFiles(packageFolder, "*.nupkg").FirstOrDefault();
        if (nupkg == null)
            return null;

        try
        {
            using var zip = ZipFile.OpenRead(nupkg);
            var nuspecEntry = zip.Entries.FirstOrDefault(e => e.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
            if (nuspecEntry == null)
                return null;

            using var stream = nuspecEntry.Open();
            var doc = XDocument.Load(stream);

            var metadata = doc.Root?.Element("metadata");
            if (metadata == null)
                return null;

            return new NuGetPackageInfo
            {
                Id = metadata.Element("id")?.Value ?? packageId,
                Version = metadata.Element("version")?.Value ?? version,
                Description = metadata.Element("description")?.Value,
                Authors = metadata.Element("authors")?.Value,
                FileName = Path.GetFileName(nupkg),
                DownloadUrl = $"nuget/download/{packageId}/{version}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading metadata for {PackageId} {Version}", packageId, version);
            return null;
        }
    }

    private (string packageId, string version) ExtractPackageInfo(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);

        var matches = versionRegex.Matches(name);
        if (matches.Count == 0)
            return (string.Empty, string.Empty);

        var version = matches[^1].Groups["version"].Value;
        var packageId = name.Substring(0, name.LastIndexOf("." + version, StringComparison.OrdinalIgnoreCase));

        return (packageId, version);
    }
}
