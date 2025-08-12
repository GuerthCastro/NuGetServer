using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NuGetServer.Entities.DTO;

[ExcludeFromCodeCoverage]
public class RegistrationLeafDto
{
    [JsonProperty("@type")]
    public string Type { get; set; } = "Package";

    [JsonProperty("packageContent")]
    public string PackageContent { get; set; } = string.Empty;

    [JsonProperty("registration")]
    public string Registration { get; set; } = string.Empty;

    [JsonProperty("catalogEntry")]
    public CatalogEntryDto CatalogEntry { get; set; } = new();
}
