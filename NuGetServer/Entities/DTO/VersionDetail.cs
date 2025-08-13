using System.Diagnostics.CodeAnalysis;

namespace NuGetServer.Entities.DTO;

[ExcludeFromCodeCoverage]
public class VersionDetail
{
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public long Downloads { get; set; }
    public string Type { get; set; } = "PackageDetails";
}
