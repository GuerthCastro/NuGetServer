using System.Diagnostics.CodeAnalysis;

namespace NuGetServer.Entities.DTO;

[ExcludeFromCodeCoverage]
public class VersionsResponse
{
    public List<string> Versions { get; set; } = new();
}
