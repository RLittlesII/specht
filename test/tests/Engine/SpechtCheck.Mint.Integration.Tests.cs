using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Report;
using Specht.Tests.Engine.Baseline;
using Specht.Tests.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Engine;

/// <summary>
/// The check over the baseline tree pinned to schema <c>0.3.1</c> (<c>0001-F19</c> B-014; C-6, C-8): a manifest that
/// opts in every type C-13 lists, each with a grammar in <c>identifiers</c> (A-6), gets the report document the same tree
/// gets with the <c>mint</c> key removed.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtCheckMintIntegrationTests
{
    [Fact]
    public void TheBaselineTreeWithAManifestOptingTypesIn_WhenChecked_ShouldGiveTheReportItGetsWithTheMintKeyRemoved()
    {
        // Given
        using var tree = new SpechtTree();
        BaselineTree.Write(tree);
        tree.Pin("0.3.1");
        var path = Path.Combine(tree.Root, SpechtTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var identifiers = manifest["identifiers"]!.AsObject();
        var mint = new JsonObject();
        foreach (var row in new MintedTypes())
        {
            identifiers[row.Data.Item1] = row.Data.Item2;
            mint[row.Data.Item1] = JsonNode.Parse(row.Data.Item3);
        }

        File.WriteAllText(path, manifest.ToJsonString());
        var without = tree.Run();
        manifest["mint"] = mint;
        File.WriteAllText(path, manifest.ToJsonString());

        // When
        var check = () => new SpechtReportSerializer().Serialize(new SpechtReportDocumentMapper().Map(tree.Run()));

        // Then
        without.Violations.Should().NotBeEmpty();
        check.Should().NotThrow().Which.Should().Be(new SpechtReportSerializer().Serialize(new SpechtReportDocumentMapper().Map(without)));
    }
}
