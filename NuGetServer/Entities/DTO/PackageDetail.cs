using System.Diagnostics.CodeAnalysis;

namespace NuGetServer.Entities.DTO;

[ExcludeFromCodeCoverage]
public class PackageDetail
{
    public string Type { get; set; } = "Package";
    public string Registration { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string[] Authors { get; set; } = Array.Empty<string>();
    public string IconUrl { get; set; } = string.Empty;
    public string LicenseUrl { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public long TotalDownloads { get; set; }
    public bool Verified { get; set; } = true;
    public List<PackageTypeInfo> PackageTypes { get; set; } = new();
    public List<VersionDetail> Versions { get; set; } = new();
}
