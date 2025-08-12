using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NuGetServer.Entities.DTO;

[ExcludeFromCodeCoverage]
public class RegistrationContext
{
    [JsonProperty("@vocab")]
    public string Vocab { get; set; } = "http://schema.nuget.org/schema#";

    [JsonProperty("@base")]
    public string Base { get; set; } = string.Empty;
}
