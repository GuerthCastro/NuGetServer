using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NuGetServer.Entities.DTO
{
    [ExcludeFromCodeCoverage]
    public class ServiceResource
    {
        [JsonProperty("@id")]
        public string Id { get; set; } = default!;

        [JsonProperty("@type")]
        public string Type { get; set; } = default!;
    }
}
