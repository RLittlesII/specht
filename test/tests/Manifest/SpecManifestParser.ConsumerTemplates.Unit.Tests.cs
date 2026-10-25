using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>The key that names the consumer's templates, read in memory at every shipped pin (0001-F7 B-065; 0001-F5 B-012).</summary>
[Trait("Tier", "Unit")]
public sealed class SpecManifestParserConsumerTemplatesUnitTests
{
    public static TheoryData<string> ShippedPins => new() { "0.1.0", "0.2.0" };

    [Theory]
    [MemberData(nameof(ShippedPins))]
    public void AManifestNamingAShippedTemplate_WhenParsed_ShouldNotRejectTheKeyAsUnknown(string pin)
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject { ["schemaVersion"] = pin, ["consumerTemplates"] = new JsonArray("feature.md") };

        // When
        var parse = () => parser.Parse(manifest.ToJsonString());

        // Then
        parse.Should().NotThrow();
    }

    [Fact]
    public void AManifestNamingTemplates_WhenParsed_ShouldExposeTheNamesInOrder()
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.1.0",
            ["consumerTemplates"] = new JsonArray("feature.md", "adr.md"),
        };

        // When
        var structure = parser.Parse(manifest.ToJsonString());

        // Then
        structure.ConsumerTemplates.Should().Equal("feature.md", "adr.md");
    }

    [Fact]
    public void AManifestWithoutTheKey_WhenParsed_ShouldExposeNoConsumerTemplate()
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject { ["schemaVersion"] = "0.1.0" };

        // When
        var structure = parser.Parse(manifest.ToJsonString());

        // Then
        structure.ConsumerTemplates.Should().BeEmpty();
    }
}
