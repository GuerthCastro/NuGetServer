using System.Diagnostics.CodeAnalysis;

namespace NuGetServer.Entities.Config;

[ExcludeFromCodeCoverage]
public class ServiceConfig
{
    public string? ServiceName { get; set; }
    public string? ServiceVersion { get; set; }

    public string? OS { get; set; }
    public string? Environment { get; set; }
    public DateTime CurrentDate { get { return DateTime.UtcNow; } }
}
