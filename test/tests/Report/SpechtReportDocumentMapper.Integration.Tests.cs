using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// The report document the mapper makes from a check over a tree on disk (<c>0001-F3</c> B-005; item <c>0167</c>): the
/// schema version and source a manifest with no pin is checked against.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtReportDocumentMapperIntegrationTests
{
    [Fact]
    public void ATreeWhoseManifestPinsNoVersion_WhenItsDocumentIsMade_ShouldNameSchemaVersionZeroOneZeroFromTheEmbeddedSet()
    {
        // Given
        using var tree = new SpechtTree();
        var manifestPath = Path.Combine(tree.Root, SpechtTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
        manifest.Remove("schemaVersion");
        File.WriteAllText(manifestPath, manifest.ToJsonString());
        tree.WriteFeature("0001", "F1");

        // When
        var document = new SpechtReportDocumentMapper().Map(tree.Run());

        // Then
        document.SchemaVersion.Should().Be("0.1.0");
        document.SchemaSource.Should().Be(SpechtSchemaSource.Embedded);
    }
}
