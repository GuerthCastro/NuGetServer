using System.Diagnostics.CodeAnalysis;

namespace NuGetServer.Entities.DTO;

[ExcludeFromCodeCoverage]
public class AutocompleteResponse
{
    public int TotalHits { get; set; }
    public string[] Data { get; set; } = Array.Empty<string>();
}
