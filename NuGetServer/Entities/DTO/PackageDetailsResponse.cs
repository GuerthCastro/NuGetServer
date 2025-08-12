using System.Diagnostics.CodeAnalysis;

namespace NuGetServer.Entities.DTO;

[ExcludeFromCodeCoverage]
public class PackageDetailsResponse
{
    public int TotalHits { get; set; }
    public List<PackageDetail> Data { get; set; } = new();
}
