using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NuGetServer.Entities.DTO
{
    [ExcludeFromCodeCoverage]
    public class SearchResult
    {
        [JsonProperty("totalHits")]
        public int TotalHits { get; set; }
        
        [JsonProperty("data")]
        public List<SearchResultPackage> Data { get; set; } = new List<SearchResultPackage>();
    }
    
    [ExcludeFromCodeCoverage]
    public class SearchResultPackage
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "Package";
        
        [JsonProperty("registration")]
        public string Registration { get; set; } = string.Empty;
        
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;
        
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonProperty("summary")]
        public string Summary { get; set; } = string.Empty;
        
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonProperty("authors")]
        public string[] Authors { get; set; } = new string[0];
        
        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; } = string.Empty;
        
        [JsonProperty("licenseUrl")]
        public string LicenseUrl { get; set; } = string.Empty;
        
        [JsonProperty("projectUrl")]
        public string ProjectUrl { get; set; } = string.Empty;
        
        [JsonProperty("tags")]
        public string[] Tags { get; set; } = new string[0];
        
        [JsonProperty("totalDownloads")]
        public int TotalDownloads { get; set; } = 0;
        
        [JsonProperty("verified")]
        public bool Verified { get; set; } = true;
        
        [JsonProperty("packageTypes")]
        public List<PackageType> PackageTypes { get; set; } = new List<PackageType> 
        { 
            new PackageType { Name = "Dependency", Version = "" }
        };
        
        [JsonProperty("versions")]
        public List<PackageVersion> Versions { get; set; } = new List<PackageVersion>();
    }
    
    [ExcludeFromCodeCoverage]
    public class PackageType
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;
    }
    
    [ExcludeFromCodeCoverage]
    public class PackageVersion
    {
        [JsonProperty("@id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;
        
        [JsonProperty("downloads")]
        public int Downloads { get; set; } = 0;
        
        [JsonProperty("@type")]
        public string Type { get; set; } = "PackageDetails";
        
        [JsonProperty("listed")]
        public bool Listed { get; set; } = true;
    }
}
