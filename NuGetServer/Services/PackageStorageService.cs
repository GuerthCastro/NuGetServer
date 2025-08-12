using NuGet.Versioning;
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
    private static readonly string DOWNLOAD_COUNT_FILE = "download_count.json";

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
        var flatContainerRoot = Path.Combine(_nuGetServer.PackagesPath, "v3-flatcontainer");
        Directory.CreateDirectory(flatContainerRoot);
        var idLower = packageId.ToLowerInvariant();
        var flatIdDir = Path.Combine(flatContainerRoot, idLower);
        Directory.CreateDirectory(flatIdDir);
        var indexPath = Path.Combine(flatIdDir, "index.json");
        List<string> allVersions = new();
        if (File.Exists(indexPath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(indexPath);
                var dto = System.Text.Json.JsonSerializer.Deserialize<NuGetServer.Entities.DTO.VersionsIndexDto>(json);
                if (dto?.Versions != null)
                    allVersions.AddRange(dto.Versions);
            }
            catch { }
        }
        if (!allVersions.Contains(version, StringComparer.OrdinalIgnoreCase))
            allVersions.Add(version);
        var normalized = allVersions
            .Select(v => v.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(v => NuGet.Versioning.NuGetVersion.Parse(v).ToNormalizedString())
            .OrderBy(v => NuGet.Versioning.NuGetVersion.Parse(v))
            .ToArray();
        var indexDto = new NuGetServer.Entities.DTO.VersionsIndexDto { Versions = normalized };
        var outJson = System.Text.Json.JsonSerializer.Serialize(indexDto);
        await File.WriteAllTextAsync(indexPath, outJson);
        _logger.LogInformation("Package {PackageId} {Version} saved at {Path}", packageId, version, savePath);
        return (true, $"Package {fileName} uploaded successfully");
    }

    public async Task<(Stream? Stream, string? ContentType)> GetPackageStream(string packageId, string version)
    {
        // First try exact case
        var packageFolder = Path.Combine(_nuGetServer.PackagesPath, packageId, version);
        
        // If not found, try case insensitive search for package ID
        if (!Directory.Exists(packageFolder))
        {
            var rootDir = new DirectoryInfo(_nuGetServer.PackagesPath);
            var matchingDir = rootDir.GetDirectories()
                .FirstOrDefault(dir => string.Equals(dir.Name, packageId, StringComparison.OrdinalIgnoreCase));
                
            if (matchingDir != null)
            {
                _logger.LogInformation("Found package directory with case-insensitive match: {ActualName}", matchingDir.Name);
                
                // Now look for version folder
                var versionDir = matchingDir.GetDirectories()
                    .FirstOrDefault(dir => string.Equals(dir.Name, version, StringComparison.OrdinalIgnoreCase));
                    
                if (versionDir != null)
                {
                    _logger.LogInformation("Found version directory with case-insensitive match: {ActualVersion}", versionDir.Name);
                    packageFolder = versionDir.FullName;
                }
                else
                {
                    _logger.LogWarning("Version directory not found for {PackageId} {Version}", packageId, version);
                    return (null, null);
                }
            }
            else
            {
                _logger.LogWarning("Package directory not found for {PackageId}", packageId);
                return (null, null);
            }
        }
        
        var packagePath = Directory.GetFiles(packageFolder, "*.nupkg").FirstOrDefault();
        if (packagePath == null)
        {
            _logger.LogWarning("No .nupkg file found in folder: {PackageFolder}", packageFolder);
            return (null, null);
        }
        
        _logger.LogInformation("Found package file: {PackagePath}", packagePath);

        // Increment download count
        await IncrementDownloadCount(packageId, version);

        var stream = new FileStream(packagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (stream, "application/octet-stream");
    }
    
    private async Task IncrementDownloadCount(string packageId, string version)
    {
        try 
        {
            // First try exact case
            var packageFolder = Path.Combine(_nuGetServer.PackagesPath, packageId, version);
            
            // If not found, try case insensitive search for package ID and version
            if (!Directory.Exists(packageFolder))
            {
                var rootDir = new DirectoryInfo(_nuGetServer.PackagesPath);
                var matchingDir = rootDir.GetDirectories()
                    .FirstOrDefault(dir => string.Equals(dir.Name, packageId, StringComparison.OrdinalIgnoreCase));
                    
                if (matchingDir != null)
                {
                    // Now look for version folder
                    var versionDir = matchingDir.GetDirectories()
                        .FirstOrDefault(dir => string.Equals(dir.Name, version, StringComparison.OrdinalIgnoreCase));
                        
                    if (versionDir != null)
                    {
                        _logger.LogInformation("Using case-insensitive match for download count: {ActualId}/{ActualVersion}", 
                            matchingDir.Name, versionDir.Name);
                        packageFolder = versionDir.FullName;
                        packageId = matchingDir.Name; // Use actual casing
                        version = versionDir.Name; // Use actual casing
                    }
                    else
                    {
                        _logger.LogWarning("Version directory not found for download count: {PackageId} {Version}", packageId, version);
                        return;
                    }
                }
                else
                {
                    _logger.LogWarning("Package directory not found for download count: {PackageId}", packageId);
                    return;
                }
            }
            
            if (!Directory.Exists(packageFolder))
            {
                _logger.LogWarning("Package folder not found for download count: {PackageFolder}", packageFolder);
                return;
            }
                
            var countFile = Path.Combine(packageFolder, DOWNLOAD_COUNT_FILE);
            int count = 1;
            
            if (File.Exists(countFile))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(countFile);
                    count = System.Text.Json.JsonSerializer.Deserialize<int>(json) + 1;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read download count for {PackageId} {Version}, resetting to 1", packageId, version);
                    count = 1;
                }
            }
            
            var newJson = System.Text.Json.JsonSerializer.Serialize(count);
            await File.WriteAllTextAsync(countFile, newJson);
            _logger.LogInformation("Updated download count for {PackageId} {Version} to {Count}, saved to {File}", 
                packageId, version, count, countFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to increment download count for {PackageId} {Version}", packageId, version);
        }
    }
    
    private async Task<int> GetDownloadCount(string packageId, string version)
    {
        var packageFolder = Path.Combine(_nuGetServer.PackagesPath, packageId, version);
        if (!Directory.Exists(packageFolder))
            return 0;
            
        var countFile = Path.Combine(packageFolder, DOWNLOAD_COUNT_FILE);
        if (!File.Exists(countFile))
            return 0;
            
        try
        {
            var json = await File.ReadAllTextAsync(countFile);
            return System.Text.Json.JsonSerializer.Deserialize<int>(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read download count for {PackageId} {Version}", packageId, version);
            return 0;
        }
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

    private async Task<List<NuGetPackageInfo>> GetPackages(bool includeMetadata)
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
                    var downloadCount = await GetDownloadCount(id, version);
                        
                    var packageInfo = new NuGetPackageInfo
                    {
                        Id = id,
                        Version = version,
                        FileName = Path.GetFileName(packageFile),
                        DownloadUrl = $"{baseUrl}/download/{id}/{version}",
                        DownloadCount = downloadCount
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
        return result.OrderByDescending(i => i.Version).ToList();
    }

    public Task<List<string>> GetPackageVersions(string packageId)
    {
        // First try exact case
        var packageDir = Path.Combine(_nuGetServer.PackagesPath, packageId);
        
        // If not found, try case insensitive search
        if (!Directory.Exists(packageDir))
        {
            var rootDir = new DirectoryInfo(_nuGetServer.PackagesPath);
            var matchingDir = rootDir.GetDirectories()
                .FirstOrDefault(dir => string.Equals(dir.Name, packageId, StringComparison.OrdinalIgnoreCase));
                
            if (matchingDir != null)
            {
                _logger.LogInformation("Found package directory with case-insensitive match: {ActualName}", matchingDir.Name);
                packageDir = matchingDir.FullName;
                packageId = matchingDir.Name; // Use the actual case of the directory
            }
            else
            {
                _logger.LogWarning("Package directory not found for {PackageId}", packageId);
                return Task.FromResult(new List<string>());
            }
        }

        // Get directories and ensure they contain valid packages
        var versions = Directory.GetDirectories(packageDir)
            .Select(Path.GetFileName)
            .Where(v => v != null && Directory.GetFiles(Path.Combine(packageDir, v), "*.nupkg").Any()) // Only return versions with nupkg files
            .Select(v => v!) // Non-null assertion after filtering out null values
            .OrderBy(v => v)
            .ToList();
            
        _logger.LogInformation("Found versions for package {PackageId}: {@Versions}", packageId, versions);
        return Task.FromResult(versions);
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
        {
            _logger.LogWarning("Package folder not found: {PackageFolder}", packageFolder);
            return null;
        }

        var nupkg = Directory.GetFiles(packageFolder, "*.nupkg").FirstOrDefault();
        if (nupkg == null)
        {
            _logger.LogWarning("No .nupkg file found in folder: {PackageFolder}", packageFolder);
            return null;
        }

        try
        {
            using var zip = ZipFile.OpenRead(nupkg);
            var nuspecEntry = zip.Entries.FirstOrDefault(e => e.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
            if (nuspecEntry == null)
            {
                _logger.LogWarning("No .nuspec file found in package: {NupkgPath}", nupkg);
                return null;
            }

            using var stream = nuspecEntry.Open();
            var doc = XDocument.Load(stream);

            // Handle XML namespaces properly
            var rootElement = doc.Root;
            if (rootElement == null)
            {
                _logger.LogWarning("Invalid XML structure in .nuspec file: {NupkgPath}", nupkg);
                return null;
            }

            // Get the namespace (if any) from the root element
            XNamespace ns = rootElement.GetDefaultNamespace();
            
            // Try to find metadata element with or without namespace
            var metadata = rootElement.Element(ns + "metadata") ?? rootElement.Element("metadata");
            if (metadata == null)
            {
                _logger.LogWarning("No metadata element found in .nuspec file: {NupkgPath}", nupkg);
                return null;
            }

            // Get download count
            var downloadCount = await GetDownloadCount(packageId, version);

            // Extract metadata with namespace support
            var baseUrl = _nuGetIndex.ServiceUrl.TrimEnd('/');
            
            var result = new NuGetPackageInfo
            {
                Id = GetElementValue(metadata, ns, "id") ?? packageId,
                Version = GetElementValue(metadata, ns, "version") ?? version,
                Description = GetElementValue(metadata, ns, "description"),
                Authors = GetElementValue(metadata, ns, "authors"),
                FileName = Path.GetFileName(nupkg),
                DownloadUrl = $"{baseUrl}/download/{packageId}/{version}",
                DownloadCount = downloadCount
            };
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading metadata for {PackageId} {Version} from {NupkgPath}", packageId, version, nupkg);
            return null;
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
