using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;

namespace Specht.Tests.Runner;

/// <summary>
/// The runner over a tree on disk and a version set built in memory from the embedded version 0.1.0 (<c>0001-F7</c>
/// B-001, B-002, B-014, B-055; C-4): the manifest is the tree's, its pin selects exactly the version it names, and that
/// version's frontmatter schemas and rule vocabulary are the ones the check uses, whatever the tree's own
/// <c>.spec/schema/</c> holds.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtRunnerVersionsIntegrationTests
{
    /// <summary>Gets a pin, and whether a check under it rejects the priority that version 1.0.0 alone rejects.</summary>
    public static TheoryData<string, bool> FrontmatterPins =>
        new()
        {
            { "0.1.0", false },
            { "1.0.0", true },
        };

    /// <summary>Gets a manifest's pin, absent when null, and the version the report names over 0.1.0, 0.1.1 and 0.2.0.</summary>
    public static TheoryData<string, string?, SemanticVersion> ReportedPins =>
        new()
        {
            { "a manifest with no schemaVersion is version 0.1.0, not the newest", null, new SemanticVersion(0, 1, 0) },
            { "a pin of 0.1.0 is exact beside a later patch and a later minor", "0.1.0", new SemanticVersion(0, 1, 0) },
            { "a manifest pinning 0.2.0", "0.2.0", new SemanticVersion(0, 2, 0) },
        };

    /// <summary>Gets the rule ids left out of version 0.1.0's vocabulary, and the rule ids a check reports for a missing traceability row.</summary>
    public static TheoryData<string, string[], string[]> Vocabularies =>
        new()
        {
            { "the whole vocabulary reports the missing row", [], ["SPEC031"] },
            { "a vocabulary without SPEC031 reports nothing", ["SPEC031"], [] },
        };

    [Theory]
    [MemberData(nameof(FrontmatterPins))]
    public void AManifestPinningAVersion_WhenChecked_ShouldValidateFrontmatterWithThatVersionsSchemas(string pinned, bool rejected)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["priority"] = "med" });
        Pin(tree, pinned);
        var shipped = SchemaVersions.Embedded.Select(new SemanticVersion(0, 1, 0));
        var schema = JsonNode.Parse(shipped.FeatureSchema)!.AsObject();
        schema["properties"]!["priority"]!["enum"] = new JsonArray("critical");
        var versions = new SchemaVersions(
            [shipped, shipped with { Number = new SemanticVersion(1, 0, 0), FeatureSchema = schema.ToJsonString() }]);

        // When
        var report = SpechtRunner.Run(tree.Root, versions);

        // Then
        var frontmatter = report.Violations
            .Where(static violation => violation.RuleId is "SPEC001" or "SPEC002" or "SPEC003" or "SPEC004")
            .ToList();
        frontmatter.Any(static violation => violation.RuleId == "SPEC002" && violation.Identifier == "priority").Should().Be(rejected);
        (frontmatter.Count > 0).Should().Be(rejected, string.Join("; ", frontmatter));
    }

    [Theory]
    [MemberData(nameof(ReportedPins))]
    public void AManifestsPin_WhenChecked_ShouldBeTheVersionTheReportNames(string because, string? pinned, SemanticVersion expected)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        Pin(tree, pinned);
        var shipped = SchemaVersions.Embedded.Select(new SemanticVersion(0, 1, 0));
        var versions = new SchemaVersions(
            [shipped, shipped with { Number = new SemanticVersion(0, 1, 1) }, shipped with { Number = new SemanticVersion(0, 2, 0) }]);

        // When
        var report = SpechtRunner.Run(tree.Root, versions);

        // Then
        report.SchemaVersion.Should().Be(expected, because);
    }

    [Theory]
    [MemberData(nameof(Vocabularies))]
    public void APinnedVersionsVocabulary_WhenChecked_ShouldBeExactlyTheRuleIdsEvaluated(string because, string[] removed, string[] expected)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", sections: SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims));
        Pin(tree, "0.1.0");
        var shipped = SchemaVersions.Embedded.Select(new SemanticVersion(0, 1, 0));
        var pinned = shipped with { RuleIds = shipped.RuleIds.Except(removed, StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal) };

        // When
        var report = SpechtRunner.Run(tree.Root, new SchemaVersions([pinned]));

        // Then
        report.Violations.Select(static violation => violation.RuleId).Should().Equal(expected, because);
        report.RulesEvaluated.Should().Be(pinned.RuleIds.Count, because);
    }

    private static void Pin(SpecTree tree, string? version)
    {
        var path = Path.Combine(tree.Root, SpecManifest.RelativePath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest.Remove("schemaVersion");
        if (version is not null)
        {
            manifest["schemaVersion"] = version;
        }

        File.WriteAllText(path, manifest.ToJsonString());
    }

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";
}
