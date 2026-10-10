using System.Text.Json.Nodes;
using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// The runner over a tree on disk and a version set built in memory from the embedded version 1 (<c>0001-F7</c> B-001,
/// B-002, B-014; C-4): the manifest is the tree's, its pin selects the version, and that version's frontmatter schemas and
/// rule vocabulary are the ones the check uses, whatever the tree's own <c>.spec/schema/</c> holds.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpecCheckRunnerVersionsIntegrationTests
{
    /// <summary>Gets a pin, and whether a check under it rejects the priority that version 2 alone rejects.</summary>
    public static TheoryData<int, bool> FrontmatterPins =>
        new()
        {
            { 1, false },
            { 2, true },
        };

    /// <summary>Gets a manifest's pin, absent when null, and the version the report names.</summary>
    public static TheoryData<string, int?, int> ReportedPins =>
        new()
        {
            { "a manifest with no schemaVersion is version 1, not the newest", null, 1 },
            { "a manifest pinning 2", 2, 2 },
        };

    /// <summary>Gets the rule ids left out of version 1's vocabulary, and the rule ids a check reports for a missing traceability row.</summary>
    public static TheoryData<string, string[], string[]> Vocabularies =>
        new()
        {
            { "the whole vocabulary reports the missing row", [], ["SPEC031"] },
            { "a vocabulary without SPEC031 reports nothing", ["SPEC031"], [] },
        };

    [Theory]
    [MemberData(nameof(FrontmatterPins))]
    public void AManifestPinningAVersion_WhenChecked_ShouldValidateFrontmatterWithThatVersionsSchemas(int pinned, bool rejected)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["priority"] = "med" });
        Pin(tree, pinned);
        var one = SchemaVersions.Embedded.Select(1);
        var schema = JsonNode.Parse(one.FeatureSchema)!.AsObject();
        schema["$id"] = "https://github.com/rlittlesii/specht/schema/v2/feature-spec.frontmatter.schema.json";
        schema["properties"]!["priority"]!["enum"] = new JsonArray("critical");
        var versions = new SchemaVersions([one, one with { Number = 2, FeatureSchema = schema.ToJsonString() }]);

        // When
        var report = SpecCheckRunner.Run(tree.Root, versions);

        // Then
        var frontmatter = report.Violations
            .Where(static violation => violation.RuleId is "SPEC001" or "SPEC002" or "SPEC003" or "SPEC004")
            .ToList();
        frontmatter.Any(static violation => violation.RuleId == "SPEC002" && violation.Identifier == "priority").Should().Be(rejected);
        (frontmatter.Count > 0).Should().Be(rejected, string.Join("; ", frontmatter));
    }

    [Theory]
    [MemberData(nameof(ReportedPins))]
    public void AManifestsPin_WhenChecked_ShouldBeTheVersionTheReportNames(string because, int? pinned, int expected)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        Pin(tree, pinned);
        var one = SchemaVersions.Embedded.Select(1);
        var versions = new SchemaVersions([one, one with { Number = 2 }]);

        // When
        var report = SpecCheckRunner.Run(tree.Root, versions);

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
        Pin(tree, 1);
        var shipped = SchemaVersions.Embedded.Select(1);
        var one = shipped with { RuleIds = shipped.RuleIds.Except(removed, StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal) };

        // When
        var report = SpecCheckRunner.Run(tree.Root, new SchemaVersions([one]));

        // Then
        report.Violations.Select(static violation => violation.RuleId).Should().Equal(expected, because);
        report.RulesEvaluated.Should().Be(one.RuleIds.Count, because);
    }

    private static void Pin(SpecTree tree, int? version)
    {
        var path = Path.Combine(tree.Root, SpecManifest.RelativePath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest.Remove("schemaVersion");
        if (version is { } pinned)
        {
            manifest["schemaVersion"] = pinned;
        }

        File.WriteAllText(path, manifest.ToJsonString());
    }

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";
}
