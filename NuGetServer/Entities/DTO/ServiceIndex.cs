using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NuGetServer.Entities.DTO
{
    [ExcludeFromCodeCoverage]
    public class ServiceIndex
    {
        [JsonProperty("version")]
        public string Version { get; set; } = "3.0.0";

        [JsonProperty("resources")]
        public List<ServiceResource> Resources { get; set; } = new();
    }
}
