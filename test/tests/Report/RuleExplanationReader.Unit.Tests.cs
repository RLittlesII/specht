using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Report;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Report;

/// <summary>0040: the pinned page is returned whole without reading schemas or the specification tree (0001-F3 B-019, B-020).</summary>
[Trait("Tier", "Unit")]
public sealed class RuleExplanationReaderUnitTests
{
    [Theory]
    [InlineData("0.1.0")]
    [InlineData("0.2.0")]
    public void ASelectedPin_WhenExplained_ShouldReturnThatVersionsEntirePage(string pin)
    {
        // Given
        var versions = new SchemaVersions(EngineServices.Versions().Versions.Select(static version => version with
        {
            RulePages = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["SPEC031"] = $"---\ntitle: Rule {version.Number}\ntype: reference\n---\n\n# SPEC031\n[{version.Number}]\n",
            },
        }));
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/.specht/manifest.json"] = new($$"""{"schemaVersion":"{{pin}}"}"""),
        });
        var reader = new RuleExplanationReader(EngineServices.Schemas(fileSystem, versions), new SpecManifestParser(versions));

        // When
        var text = reader.Read("repo", "SPEC031");

        // Then
        text.Should().Be($"---\ntitle: Rule {pin}\ntype: reference\n---\n\n# SPEC031\n[{pin}]\n");
    }

    [Theory]
    [InlineData("SPEC999")]
    [InlineData("SPEC034")]
    [InlineData("spec031")]
    public void AnIdOutsideThePinnedVocabulary_WhenExplained_ShouldReturnNoPage(string id)
    {
        // Given
        var versions = EngineServices.Versions();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/.specht/manifest.json"] = new("""{"schemaVersion":"0.1.0"}"""),
        });
        var reader = new RuleExplanationReader(EngineServices.Schemas(fileSystem, versions), new SpecManifestParser(versions));

        // When
        var text = reader.Read("repo", id);

        // Then
        text.Should().BeNull();
    }
}
