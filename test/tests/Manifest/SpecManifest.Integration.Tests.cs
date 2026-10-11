using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>
/// The manifest loader inside the runner, over a tree on disk (0001-F5 B-018, C-5): a rejected manifest stops the run
/// before any rule reports.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpecManifestIntegrationTests
{
    [Fact]
    public void AManifestWithAnUnknownKeyBesideThreeViolations_WhenChecked_ShouldRejectItWithNoReport()
    {
        // Given
        using var tree = new SpecTree();
        var sections = SpecTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n")
            .Where(static section => !section.StartsWith("## 11.", StringComparison.Ordinal))
            .ToList();
        var spec = tree.WriteFeature("0001", "F1", sections: sections);
        tree.WriteFeatureFile(
            spec,
            "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n  @B-404\n  Scenario: Phantom\n    Given nothing\n");
        tree.Run().Violations.Should().HaveCount(3);
        var path = Path.Combine(tree.Root, ".spec", "schema", "spec-structure.schema.json");
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest["glossary"] = "synthetic";
        File.WriteAllText(path, manifest.ToJsonString());

        // When
        var run = SpechtRunner.Run(tree.Root);

        // Then
        run.Value.Should().BeOfType<ManifestRejected>().Which.Message.Should().Contain("glossary");
    }
}
