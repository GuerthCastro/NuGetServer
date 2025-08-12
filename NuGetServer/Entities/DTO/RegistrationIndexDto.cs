using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NuGetServer.Entities.DTO;

[ExcludeFromCodeCoverage]
public class RegistrationIndexDto
{
    [JsonProperty("@id")]
    public string Registration { get; set; } = string.Empty;

    [JsonProperty("@type")]
    public string Type { get; set; } = "catalog:CatalogRoot";

    [JsonProperty("@context")]
    public RegistrationContext Context { get; set; } = new RegistrationContext();

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("items")]
    public IReadOnlyList<RegistrationPageDto> Items { get; set; } = [];
}
