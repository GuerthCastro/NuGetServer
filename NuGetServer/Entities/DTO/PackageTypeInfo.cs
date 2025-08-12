using System.Diagnostics.CodeAnalysis;

namespace NuGetServer.Entities.DTO;

[ExcludeFromCodeCoverage]
public class PackageTypeInfo
{
    public string Name { get; set; } = "Dependency";
    public string Version { get; set; } = string.Empty;
}
