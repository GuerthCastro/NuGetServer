using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NuGetServer.Entities.DTO
{
    [ExcludeFromCodeCoverage]
    public class ServiceIndexContext
    {
        [JsonProperty("@vocab")]
        public string Vocab { get; set; } = "http://schema.nuget.org/services#";

        [JsonProperty("comment")]
        public string Comment { get; set; } = "http://www.w3.org/2000/01/rdf-schema#comment";
        
        [JsonProperty("label")]
        public string Label { get; set; } = "http://www.w3.org/2000/01/rdf-schema#label";
    }
}
