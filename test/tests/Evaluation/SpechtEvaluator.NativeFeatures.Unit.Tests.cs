using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Tests.Shared;

namespace Specht.Tests.Evaluation;

/// <summary>
/// A synthetic tree in an in-memory file system under a schema 0.4.0 manifest whose <c>claim.source</c> is
/// <c>gherkin</c>, read by the model reader and evaluated by the composed rules (<c>0001-F16</c> B-003, B-017, B-026,
/// B-036; § 7, item <c>0391</c>): a record's frontmatter and the items beside it are held as any specification's, and a
/// set with no record has none to hold; one identity declared by a set and a specification, or by two sets, is reported
/// at each, a set at its native Feature's path, line <c>0</c>; the report's <c>layouts</c> count native Features under
/// <c>claims</c>, after the manifest's layouts; a set that makes no Feature is counted and compared with none; and a rule
/// that resolves claims through the matrix neither reports nor fails over a native Feature, whose verdicts § 7's skip
/// table leaves to item <c>0396</c>. Unless
/// a test says otherwise, the manifest holds one set, glob <c>.spec/features/arithmetic/*.feature</c> and feature
/// <c>F1</c>, with no epic grammar, and excludes <c>obj</c> alone. A finding is read under the id the evaluator reports at
/// the pin.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtEvaluatorNativeFeaturesUnitTests
{
    [Theory]
    [ClassData(typeof(RecordFrontmatterFindings))]
    public void ARecordOrItsAbsence_WhenEvaluated_ShouldBeHeldToTheFeatureAndItemSchemasOnlyWhereARecordIsAuthored(
        string members, string? recordText, string ruleId, string[] files)
    {
        // Given
        (string Path, string Text)[] tree =
        [
            (Add, AddsTwoNumbers),
            (".spec/features/arithmetic/0001-01-add.md", "---\nid: nonsense\n---\n\n# Add\n"),
            .. recordText is null ? Array.Empty<(string Path, string Text)>() : [(".spec/features/arithmetic/README.md", recordText)],
        ];

        // When
        var violations = Evaluate(members, tree).Violations;

        // Then
        violations.Where(violation => violation.RuleId == ruleId).Select(static violation => violation.File).Distinct().Should().Equal(files);
    }

    [Theory]
    [ClassData(typeof(NativeIdentityDuplicates))]
    public void OneIdentityDeclaredBySetsOrBySetAndSpecification_WhenEvaluated_ShouldReportSpec3002AtEachASetAtItsNativeFeaturesPathLineZero(
        string members, string[] claimFiles, string[] specifications, string[] places)
    {
        // Given
        var tree = claimFiles.Select(static file => (Path: file, Text: AddsTwoNumbers))
            .Concat(specifications.Select(static specification => (Path: specification, Text: "---\nid: F1\n---\n")))
            .ToArray();

        // When
        var violations = Evaluate(members, tree).Violations;

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC3002")
            .Select(static violation => (Place: $"{violation.File}({violation.Line})", violation.Identifier))
            .Should()
            .BeEquivalentTo(places.Select(static place => (Place: place, Identifier: (string?) "F1")));
    }

    [Theory]
    [ClassData(typeof(NativeReportLayouts))]
    public void NativeFeatures_WhenEvaluated_ShouldBeCountedUnderClaimsAfterTheManifestsLayoutsOnlyUnderAGherkinClaimSource(
        string members, string[] specifications, string[] layouts)
    {
        // Given
        var tree = specifications.Select(static specification => (Path: specification, Text: "---\nid: F5\n---\n"))
            .Append((Path: Add, Text: AddsTwoNumbers))
            .ToArray();

        // When
        var report = Evaluate(members, tree);

        // Then
        report.Layouts.Select(static layout => $"{layout.Layout} {layout.SpecificationCount}").Should().Equal(layouts);
    }

    [Fact]
    public void AClaimFileSetWithNoRecordAndNoClaimFile_WhenEvaluated_ShouldBeCountedUnderClaimsAsNoneAndComparedWithNoIdentity()
    {
        // Given
        (string Path, string Text)[] tree = [("src/arithmetic/.spec/README.md", "---\nid: F1\n---\n")];

        // When
        var report = Evaluate("{}", tree);

        // Then
        report.Layouts.Last().Should().Be(new SpechtReportLayout("claims", 0));
        report.Violations.Should().NotContain(static violation => violation.RuleId == "SPEC3002");
    }

    [Theory]
    [ClassData(typeof(NativeMatrixRuleSilences))]
    public void AMatrixRuleOverANativeFeatureWithAClaim_WhenEvaluated_ShouldReportNothingAndNotFail(string members, string ruleId)
    {
        // Given
        (string Path, string Text)[] tree =
        [
            (Add, AddsTwoNumbers),
            (".spec/features/arithmetic/README.md",
                "---\nid: F1\n---\n\n## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| --- | --- | --- | --- |\n"
                + "| B-001 | Adds | | Covered |\n"),
        ];

        // When
        var violations = Evaluate(members, tree).Violations;

        // Then
        violations.Where(violation => violation.RuleId == ruleId).Select(static violation => violation.ToString()).Should().BeEmpty();
    }

    private static SpechtReport Evaluate(string members, (string Path, string Text)[] files)
    {
        var manifest = JsonNode.Parse(members)!.AsObject();
        manifest["schemaVersion"] = "0.4.0";
        manifest["exclusions"] = new JsonArray("obj");
        if (manifest["claim"] is not JsonObject claim)
        {
            claim = [];
            manifest["claim"] = claim;
        }

        claim["source"] ??= "gherkin";
        if (claim["source"]!.GetValue<string>() == "gherkin")
        {
            claim["files"] ??= new JsonArray(new JsonObject { ["glob"] = ".spec/features/arithmetic/*.feature", ["feature"] = "F1" });
        }

        var fileSystem = new MockFileSystem(
            files.Append((Path: SpechtTree.ManifestPath, Text: manifest.ToJsonString()))
                .ToDictionary(
                    static file => Path.Combine(["repo", .. file.Path.Split('/')]),
                    static file => new MockFileData(file.Text)));
        var model = EngineServices.Reader(fileSystem).Read(fileSystem.Path.GetFullPath("repo"), null);

        return new SpechtEvaluator(EngineServices.Rules(), new SpechtViolationOrder()).Evaluate(model);
    }

    private const string Add = ".spec/features/arithmetic/add.feature";

    private const string AddsTwoNumbers = "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n";
}
