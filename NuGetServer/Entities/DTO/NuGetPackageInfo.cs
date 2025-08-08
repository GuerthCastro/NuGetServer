using System.Diagnostics.CodeAnalysis;

namespace NuGetServer.Entities.DTO;
[ExcludeFromCodeCoverage]
public class NuGetPackageInfo
{
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Authors { get; set; }
}
