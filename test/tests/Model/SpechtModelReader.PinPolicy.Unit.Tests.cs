using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Model;

/// <summary>
/// The check's schemas under a floating pin policy (<c>0001-F7</c> B-053): over a set holding the embedded <c>0.3.0</c> and
/// that version under the number <c>0.3.1</c> with a Feature schema requiring a key, a manifest pinning <c>0.3.0</c> with
/// <c>latest-patch</c> has the model reader keep <c>0.3.1</c> and build the Feature schema from its text, and has its pinned
/// read for <c>specht frontmatter</c> keep <c>0.3.0</c> and its schema whatever the policy (<c>0001-F8</c> B-006; decision
/// 0021). The manifest is bytes in a <see cref="MockFileSystem"/>; nothing touches the disk.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtModelReaderPinPolicyUnitTests
{
    [Fact]
    public void AManifestPinningAVersionWithLatestPatch_WhenItsSchemasAreRead_ShouldKeepTheHighestPatchAndItsSchemas()
    {
        // Given
        var pinned = EngineServices.Versions().Find(new SemanticVersion(0, 3, 0))! with { FeatureSchema = "{}" };
        var patch = pinned with { Number = new SemanticVersion(0, 3, 1), FeatureSchema = """{ "required": ["in_use_key"] }""" };
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/" + SpechtTree.ManifestPath] = new("""{ "schemaVersion": "0.3.0", "pinPolicy": "latest-patch" }"""),
        });
        var reader = EngineServices.Reader(files, new SchemaVersions([pinned, patch]));

        // When
        var schemas = reader.ReadSchemas("repo", null);

        // Then
        schemas.Version.Number.Should().Be(new SemanticVersion(0, 3, 1));
        schemas.Feature.Evaluate(JsonSerializer.SerializeToElement(new { })).IsValid.Should().BeFalse();
    }

    [Fact]
    public void AManifestPinningAVersionWithLatestPatch_WhenItsPinnedSchemasAreRead_ShouldKeepThePinAndItsSchemas()
    {
        // Given
        var pinned = EngineServices.Versions().Find(new SemanticVersion(0, 3, 0))! with { FeatureSchema = "{}" };
        var patch = pinned with { Number = new SemanticVersion(0, 3, 1), FeatureSchema = """{ "required": ["in_use_key"] }""" };
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/" + SpechtTree.ManifestPath] = new("""{ "schemaVersion": "0.3.0", "pinPolicy": "latest-patch" }"""),
        });
        var reader = EngineServices.Reader(files, new SchemaVersions([pinned, patch]));

        // When
        var schemas = reader.ReadPinnedSchemas("repo");

        // Then
        schemas.Version.Number.Should().Be(new SemanticVersion(0, 3, 0));
        schemas.Feature.Evaluate(JsonSerializer.SerializeToElement(new { })).IsValid.Should().BeTrue();
    }
}
