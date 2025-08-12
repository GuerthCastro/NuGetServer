using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace NuGetServer.Entities.DTO
{
    [ExcludeFromCodeCoverage]
    public class PackageRegistration
    {
        [JsonProperty("@id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("@type")]
        public string Type { get; set; } = "catalog:CatalogRoot";

        [JsonProperty("@context")]
        public RegistrationContext Context { get; set; } = new RegistrationContext();

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("items")]
        public List<RegistrationPage> Items { get; set; } = new List<RegistrationPage>();
    }

    [ExcludeFromCodeCoverage]
    public class RegistrationContext
    {
        [JsonProperty("@vocab")]
        public string Vocab { get; set; } = "http://schema.nuget.org/schema#";

        [JsonProperty("@base")]
        public string Base { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class RegistrationPage
    {
        [JsonProperty("@id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("@type")]
        public string Type { get; set; } = "catalog:CatalogPage";

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("lower")]
        public string Lower { get; set; } = string.Empty;

        [JsonProperty("upper")]
        public string Upper { get; set; } = string.Empty;

        [JsonProperty("items")]
        public List<PackageItem> Items { get; set; } = new List<PackageItem>();
    }

    [ExcludeFromCodeCoverage]
    public class PackageItem
    {
        [JsonProperty("@id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("@type")]
        public string Type { get; set; } = "Package";
        
        [JsonProperty("listed")]
        public bool Listed { get; set; } = true;

        [JsonProperty("catalogEntry")]
        public PackageCatalogEntry CatalogEntry { get; set; } = new PackageCatalogEntry();

        [JsonProperty("packageContent")]
        public string PackageContent { get; set; } = string.Empty;
    }

    [ExcludeFromCodeCoverage]
    public class PackageCatalogEntry
    {
        [JsonProperty("@id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("@type")]
        public string Type { get; set; } = "PackageDetails";

        [JsonProperty("authors")]
        public string Authors { get; set; } = string.Empty;

        [JsonProperty("packageId")]
        public string PackageId { get; set; } = string.Empty;

        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonProperty("summary")]
        public string Summary { get; set; } = string.Empty;
        
        [JsonProperty("isLatestVersion")]
        public bool IsLatestVersion { get; set; } = true;
        
        [JsonProperty("listed")]
        public bool Listed { get; set; } = true;
        
        [JsonProperty("downloads")]
        public int Downloads { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class PackageMetadata
    {
        [JsonProperty("@id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("@type")]
        public string Type { get; set; } = "Package";

        [JsonProperty("@context")]
        public RegistrationContext Context { get; set; } = new RegistrationContext();

        [JsonProperty("catalogEntry")]
        public PackageCatalogEntry CatalogEntry { get; set; } = new PackageCatalogEntry();

        [JsonProperty("packageContent")]
        public string PackageContent { get; set; } = string.Empty;

        [JsonProperty("registration")]
        public string Registration { get; set; } = string.Empty;
    }
}
