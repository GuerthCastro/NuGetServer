using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NuGetServer.Entities.DTO;

[ExcludeFromCodeCoverage]
public class CatalogEntryDto
{
    [JsonProperty("@id")]
    public string CanonicalId { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;

    [JsonProperty("listed")]
    public bool Listed { get; set; }

    [JsonProperty("published")]
    public DateTimeOffset Published { get; set; }
}
