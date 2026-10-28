using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>Pins engine-owned text ordering and preservation (0001-F9 B-019, B-020).</summary>
[Trait("Tier", "Unit")]
public sealed class ManifestTextUnitTests
{
    [Fact]
    public void ReorderedDeclarations_WhenFormatted_ShouldFollowTheExactDefaultAndKeepExtensionValues()
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["consumer/.specht/manifest.json"] = new("""
                {"$z":{"keep":[null,3]},"roles":{"claims":"3. Acceptance Criteria"},
                 "rules":{"SPEC020":"off","SPEC010":"warning"},"schemaVersion":"0.1.0","$a":null}
                """),
        });
        var versions = EngineServices.Versions();
        var schemas = EngineServices.Schemas(files);
        var reader = new EffectiveManifestReader(schemas, new RuleEntriesReader(schemas, EngineServices.Parser()), versions);
        var defaults = JsonNode.Parse(versions.Versions[0].Manifest)!.AsObject();
        var before = files.File.ReadAllText("consumer/.specht/manifest.json");

        // When
        var text = reader.ReadText("consumer");
        var document = JsonNode.Parse(text)!.AsObject();

        // Then
        document.Select(static entry => entry.Key).Where(defaults.ContainsKey).Should()
            .Equal(defaults.Select(static entry => entry.Key).Where(document.ContainsKey));
        document.Select(static entry => entry.Key).Where(key => !defaults.ContainsKey(key)).Take(2).Should().Equal("$a", "$z");
        document["rules"]!.AsObject().Select(static entry => entry.Key).Should().BeInAscendingOrder(StringComparer.Ordinal);
        document["roles"]!["claims"]!.GetValue<string>().Should().Be("3. Acceptance Criteria");
        document.Should().ContainKey("$a").WhoseValue.Should().BeNull();
        JsonNode.DeepEquals(document["$z"], JsonNode.Parse("{\"keep\":[null,3]}")).Should().BeTrue();
        text.Should().Contain("\n  \"");
        reader.ReadText("consumer").Should().Be(text);
        files.File.ReadAllText("consumer/.specht/manifest.json").Should().Be(before);
    }
}
