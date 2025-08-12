using System.Text.Json;
using FluentAssertions;
using NuGetServer.Entities.DTO;
using Xunit;

namespace NuGetServer.Tests.Services;

public class FlatContainerIndexTests
{
    [Fact]
    public void VersionsIndexDto_Should_Serialize_Normalized_And_Sorted()
    {
        var dto = new VersionsIndexDto
        {
            Versions = new[] { "1.0.0", "1.0.0-beta", "2.0.0", "1.0.0-alpha" }
        };
        var sorted = dto.Versions
            .Select(v => NuGet.Versioning.NuGetVersion.Parse(v).ToNormalizedString())
            .OrderBy(v => NuGet.Versioning.NuGetVersion.Parse(v))
            .ToArray();
        var json = JsonSerializer.Serialize(new VersionsIndexDto { Versions = sorted });
        var deserialized = JsonSerializer.Deserialize<VersionsIndexDto>(json);
        deserialized!.Versions.Should().BeEquivalentTo(sorted, o => o.WithStrictOrdering());
    }
}
