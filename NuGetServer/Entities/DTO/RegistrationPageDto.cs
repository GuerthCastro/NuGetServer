using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NuGetServer.Entities.DTO;

[ExcludeFromCodeCoverage]
public class RegistrationPageDto
{
    [JsonProperty("@id")]
    public string Registration { get; set; } = string.Empty;

    [JsonProperty("@type")]
    public string Type { get; set; } = "catalog:CatalogPage";

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("lower")]
    public string Lower { get; set; } = string.Empty;

    [JsonProperty("upper")]
    public string Upper { get; set; } = string.Empty;

    [JsonProperty("parent")]
    public string Parent { get; set; } = string.Empty;

    [JsonProperty("items")]
    public IReadOnlyList<RegistrationLeafDto> Items { get; set; } = [];
}
