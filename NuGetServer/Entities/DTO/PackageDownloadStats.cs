using System.Diagnostics.CodeAnalysis;

namespace NuGetServer.Entities.DTO
{
    [ExcludeFromCodeCoverage]
    public class PackageDownloadStats
    {
        public string Id { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public int DownloadCount { get; set; }
    }
}
