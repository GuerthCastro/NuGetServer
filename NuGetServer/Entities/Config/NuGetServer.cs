using System.Diagnostics.CodeAnalysis;

namespace NuGetServer.Entities.Config;

[ExcludeFromCodeCoverage]
public class NuGetServer
{
    public string ApiKey { get; set; } = "2eb59533-6a28-4b0e-b728-52f78a40b067";
    public string PackagesPath { get; set; } = "/var/nuget/packages";
}
