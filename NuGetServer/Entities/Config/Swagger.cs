using System.Diagnostics.CodeAnalysis;

namespace NuGetServer.Entities.Config;

[ExcludeFromCodeCoverage]
public class Swagger
{
    public string Title { get; set; } = "Dragonfly NuGet Server";
    public string Version { get; set; } = "v1";
    public string Description { get; set; } = "Simple private NuGet feed";
}
