using System.Diagnostics.CodeAnalysis;

namespace NuGetServer.Entities.DTO;

[ExcludeFromCodeCoverage]
public class SearchParameterInfo
{
    public string Query { get; set; } = string.Empty;
    public string RequestUrl { get; set; } = string.Empty;
    public bool AllVersions { get; set; }
    public bool Prerelease { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
