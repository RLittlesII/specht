using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>
/// The done marker at schema 0.3.0, read in memory over the shipped catalog (<c>0001-F5</c> B-063, B-064; <c>0001-F7</c>
/// decision 0015 call 2): left out it reads as <c>done</c>, named it replaces that default, and empty it is rejected naming
/// the marker.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtManifestParserDoneMarkerUnitTests
{
    [Fact]
    public void AZeroThreeZeroManifestNamingNoDoneMarker_WhenParsed_ShouldReadTheDoneMarkerAsDone()
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0" };

        // When
        var structure = EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        structure.Markers.Should().ContainKey("done").WhoseValue.Should().Be("done");
    }

    [Fact]
    public void AZeroThreeZeroManifestNamingADoneMarker_WhenParsed_ShouldReadThatMarkerInPlaceOfTheDefault()
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0", ["markers"] = new JsonObject { ["done"] = "delivered" } };

        // When
        var structure = EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        structure.Markers.Should().ContainKey("done").WhoseValue.Should().Be("delivered");
    }

    [Fact]
    public void AZeroThreeZeroManifestWithAnEmptyDoneMarker_WhenParsed_ShouldBeRejectedNamingTheDoneMarker()
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.3.0", ["markers"] = new JsonObject { ["done"] = string.Empty } };

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        parse.Should().Throw<SpechtManifestException>().Which.Message.Should().Contain("'done'");
    }
}
