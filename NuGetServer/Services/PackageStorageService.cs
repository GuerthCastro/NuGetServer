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

    public Task<List<NuGetPackageInfo>> GetAllPackages()
    {
        return GetPackages(includeMetadata: true);
    }

    public Task<List<NuGetPackageInfo>> GetPackagesWithMetadata()
    {
        return GetPackages(includeMetadata: true);
    }

    private Task<List<NuGetPackageInfo>> GetPackages(bool includeMetadata)
    {
        var result = new List<NuGetPackageInfo>();
        var baseUrl = _nuGetIndex.ServiceUrl.TrimEnd('/');

        if (!Directory.Exists(_nuGetServer.PackagesPath))
            return Task.FromResult(result);

        foreach (var idDir in Directory.GetDirectories(_nuGetServer.PackagesPath))
        {
            var id = Path.GetFileName(idDir);
            foreach (var versionDir in Directory.GetDirectories(idDir))
            {
                var version = Path.GetFileName(versionDir);
                var packageFile = Path.Combine(versionDir, $"{id}.{version}.nupkg");

                if (File.Exists(packageFile))
                {
                    var packageInfo = new NuGetPackageInfo
                    {
                        Id = id,
                        Version = version,
                        FileName = Path.GetFileName(packageFile),
                        DownloadUrl = $"{baseUrl}/download/{id}/{version}"
                    };

                    if (includeMetadata)
                    {
                        try
                        {
                            using var zip = ZipFile.OpenRead(packageFile);
                            var nuspecEntry = zip.Entries.FirstOrDefault(e => e.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
                            if (nuspecEntry != null)
                            {
                                using var stream = nuspecEntry.Open();
                                var doc = XDocument.Load(stream);
                                
                                // Handle XML namespaces properly
                                var rootElement = doc.Root;
                                if (rootElement != null)
                                {
                                    // Get the namespace (if any) from the root element
                                    XNamespace ns = rootElement.GetDefaultNamespace();
                                    
                                    // Try to find metadata element with or without namespace
                                    var metadata = rootElement.Element(ns + "metadata") ?? rootElement.Element("metadata");
                                    if (metadata != null)
                                    {
                                        packageInfo.Description = GetElementValue(metadata, ns, "description");
                                        packageInfo.Authors = GetElementValue(metadata, ns, "authors");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error reading metadata for package {Id} {Version}", id, version);
                        }
                    }

                    result.Add(packageInfo);
                }
            }
        }
        return Task.FromResult(result);
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

    public Task<NuGetPackageInfo?> GetPackageMetadata(string packageId, string version)
    {
        var packageFolder = Path.Combine(_nuGetServer.PackagesPath, packageId, version);
        if (!Directory.Exists(packageFolder))
        {
            _logger.LogWarning("Package folder not found: {PackageFolder}", packageFolder);
            return Task.FromResult<NuGetPackageInfo?>(null);
        }

        var nupkg = Directory.GetFiles(packageFolder, "*.nupkg").FirstOrDefault();
        if (nupkg == null)
        {
            _logger.LogWarning("No .nupkg file found in folder: {PackageFolder}", packageFolder);
            return Task.FromResult<NuGetPackageInfo?>(null);
        }

        try
        {
            using var zip = ZipFile.OpenRead(nupkg);
            var nuspecEntry = zip.Entries.FirstOrDefault(e => e.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
            if (nuspecEntry == null)
            {
                _logger.LogWarning("No .nuspec file found in package: {NupkgPath}", nupkg);
                return Task.FromResult<NuGetPackageInfo?>(null);
            }

            using var stream = nuspecEntry.Open();
            var doc = XDocument.Load(stream);

            // Handle XML namespaces properly
            var rootElement = doc.Root;
            if (rootElement == null)
            {
                _logger.LogWarning("Invalid XML structure in .nuspec file: {NupkgPath}", nupkg);
                return Task.FromResult<NuGetPackageInfo?>(null);
            }

            // Get the namespace (if any) from the root element
            XNamespace ns = rootElement.GetDefaultNamespace();
            
            // Try to find metadata element with or without namespace
            var metadata = rootElement.Element(ns + "metadata") ?? rootElement.Element("metadata");
            if (metadata == null)
            {
                _logger.LogWarning("No metadata element found in .nuspec file: {NupkgPath}", nupkg);
                return Task.FromResult<NuGetPackageInfo?>(null);
            }

            // Extract metadata with namespace support
            var baseUrl = _nuGetIndex.ServiceUrl.TrimEnd('/');
            
            var result = new NuGetPackageInfo
            {
                Id = GetElementValue(metadata, ns, "id") ?? packageId,
                Version = GetElementValue(metadata, ns, "version") ?? version,
                Description = GetElementValue(metadata, ns, "description"),
                Authors = GetElementValue(metadata, ns, "authors"),
                FileName = Path.GetFileName(nupkg),
                DownloadUrl = $"{baseUrl}/download/{packageId}/{version}"
            };
            
            return Task.FromResult<NuGetPackageInfo?>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading metadata for {PackageId} {Version} from {NupkgPath}", packageId, version, nupkg);
            return Task.FromResult<NuGetPackageInfo?>(null);
        }
    }

    private static string? GetElementValue(XElement parent, XNamespace ns, string elementName)
    {
        // Try with namespace first, then without
        return parent.Element(ns + elementName)?.Value ?? parent.Element(elementName)?.Value;
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
