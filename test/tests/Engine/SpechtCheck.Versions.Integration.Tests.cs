using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Report;
using Specht.Tests.Engine.Baseline;
using Specht.Tests.Shared;
using Specht.Tests.Versioning;
using Specht.Versioning;

namespace Specht.Tests.Engine;

/// <summary>
/// The check over a tree on disk and a version set built in memory from the embedded version 0.1.0 (<c>0001-F7</c>
/// B-001, B-002, B-014, B-055, B-151, B-152, B-154; C-4): the manifest is the tree's, its pin selects exactly the version it names, and that
/// version's frontmatter schemas and rule vocabulary are the ones the check uses, whatever the tree's own
/// <c>.specht/schema/frontmatter/</c> holds. On the shipped versions, the baseline tree pinned to 0.2.0 or 0.2.1 is
/// reported under three-digit ids and pinned to 0.3.0 under four-digit ids, each in that version's vocabulary (B-151,
/// B-152). Pinned to a shipped version and then to the shipped version higher than it in its patch alone, the baseline
/// tree gets the same report document but for the schema version it names (B-154). A tree setting an optional frontmatter
/// field the higher version's schema defines and the lower one rejects is the exception (decision 0023): <c>claim_prefix</c>
/// is reported by <c>SPEC1002</c> under 0.3.0 and selects the claim grammar <c>SPEC4003</c> checks under 0.3.1.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtCheckVersionsIntegrationTests
{
    /// <summary>Gets a pin, and whether a check under it rejects the priority that version 1.0.0 alone rejects.</summary>
    public static TheoryData<string, bool> FrontmatterPins =>
        new()
        {
            { "0.1.0", false },
            { "1.0.0", true },
        };

    /// <summary>Gets a manifest's pin, absent when null, and the version the report names over 0.1.0, 0.2.0 and 1.0.0.</summary>
    public static TheoryData<string, string?, SemanticVersion> ReportedPins =>
        new()
        {
            { "a manifest with no schemaVersion is version 0.1.0, not the newest", null, new SemanticVersion(0, 1, 0) },
            { "a pin of 0.1.0 is exact beside a later minor and a later major", "0.1.0", new SemanticVersion(0, 1, 0) },
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
        using var tree = new SpechtTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["priority"] = "med" });
        Pin(tree, pinned);
        var shipped = EngineServices.Versions().Find(new SemanticVersion(0, 1, 0))!;
        var tagDefaults = EngineServices.Versions().Find(new SemanticVersion(0, 2, 0))!.Manifest;
        var schema = JsonNode.Parse(shipped.FeatureSchema)!.AsObject();
        schema["properties"]!["priority"]!["enum"] = new JsonArray("critical");
        var versions = new SchemaVersions(
            [shipped, shipped with { Number = new SemanticVersion(1, 0, 0), FeatureSchema = schema.ToJsonString(), Manifest = tagDefaults }]);

        // When
        var report = EngineServices.Check(versions).Run(tree.Root, null);

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
        using var tree = new SpechtTree();
        tree.WriteFeature("0001", "F1");
        Pin(tree, pinned);
        var shipped = EngineServices.Versions().Find(new SemanticVersion(0, 1, 0))!;
        var tagDefaults = EngineServices.Versions().Find(new SemanticVersion(0, 2, 0))!.Manifest;
        var versions = new SchemaVersions(
            [shipped, shipped with { Number = new SemanticVersion(0, 2, 0), Manifest = tagDefaults },
                shipped with { Number = new SemanticVersion(1, 0, 0), Manifest = tagDefaults }]);

        // When
        var report = EngineServices.Check(versions).Run(tree.Root, null);

        // Then
        report.SchemaVersion.Should().Be(expected, because);
    }

    [Theory]
    [MemberData(nameof(Vocabularies))]
    public void APinnedVersionsVocabulary_WhenChecked_ShouldBeExactlyTheRuleIdsEvaluated(string because, string[] removed, string[] expected)
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteFeature("0001", "F1", sections: SpechtTree.SectionsWith("3. Acceptance Criteria", TwoClaims));
        Pin(tree, "0.1.0");
        var shipped = EngineServices.Versions().Find(new SemanticVersion(0, 1, 0))!;
        var pinned = shipped with { RuleIds = shipped.RuleIds.Except(removed, StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal) };

        // When
        var report = EngineServices.Check(new SchemaVersions([pinned])).Run(tree.Root, null);

        // Then
        report.Violations.Select(static violation => violation.RuleId).Should().Equal(expected, because);
        report.RulesEvaluated.Should().Be(pinned.RuleIds.Count, because);
    }

    [Theory]
    [ClassData(typeof(ShippedVocabularies))]
    public void TheBaselineTreePinnedToAShippedVersion_WhenChecked_ShouldReportEveryViolationUnderAnIdOfThatVersionsVocabulary(
        string version,
        string[] vocabulary)
    {
        // Given
        using var tree = new SpechtTree();
        BaselineTree.Write(tree);
        tree.Pin(version);

        // When
        var report = tree.Run();

        // Then
        report.Violations.Select(static violation => violation.RuleId).Should().NotBeEmpty().And.AllSatisfy(id => id.Should().BeOneOf(vocabulary));
    }

    [Theory]
    [ClassData(typeof(ShippedPatchNeighbours))]
    public void TheBaselineTreePinnedToAShippedVersionAndToItsPatchNeighbour_WhenChecked_ShouldGiveTheSameReportTheSchemaVersionAside(
        string lower,
        string higher)
    {
        // Given
        using var tree = new SpechtTree();
        BaselineTree.Write(tree);

        // When
        var reports = new[] { lower, higher }
            .Select(version =>
            {
                tree.Pin(version);
                var document = JsonNode.Parse(new SpechtReportSerializer().Serialize(new SpechtReportDocumentMapper().Map(tree.Run())))!.AsObject();
                document.Remove("schemaVersion");
                return document.ToJsonString();
            })
            .ToList();

        // Then
        reports[1].Should().Be(reports[0]);
    }

    [Fact]
    public void ATreeSettingAnOptionalFieldOnlyThePatchDefines_WhenCheckedUnderAShippedVersionAndItsPatchNeighbour_ShouldReportItUnderEachVersionsOwnRule()
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteCoLocatedFeature("src/ci", "0001", "F1", new Dictionary<string, string> { ["claim_prefix"] = "\"CI-AC-\"" });

        // When
        tree.Pin("0.3.0");
        var lower = tree.Run().Violations.Where(static violation => violation.File == "src/ci/.spec/README.md").ToList();
        tree.Pin("0.3.1");
        var higher = tree.Run().Violations.Where(static violation => violation.File == "src/ci/.spec/README.md").ToList();

        // Then
        lower.Should().Contain(static violation => violation.RuleId == "SPEC1002" && violation.Identifier == "claim_prefix");
        lower.Should().NotContain(static violation => violation.RuleId == "SPEC4003");
        higher.Should().NotContain(static violation => violation.RuleId == "SPEC1002");
        var finding = higher.Where(static violation => violation.RuleId == "SPEC4003").Should().ContainSingle().Subject;
        finding.Identifier.Should().Be("B-001");
        finding.Message.Should().Contain("'B-001'").And.Contain("'CI-AC-'");
    }

    private static void Pin(SpechtTree tree, string? version)
    {
        var path = Path.Combine(tree.Root, SpechtTree.ManifestPath);
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
