using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules.NativeClaims;

/// <summary>
/// <c>SPEC4013</c> over a synthetic tree in an in-memory file system, read by the model reader and evaluated by the
/// composed rules (<c>0001-F16</c> B-014 to B-016, B-018, B-019, B-033, B-034, B-002, B-028, C-2, C-3; § 7, item
/// <c>0395</c>): a claim file the reader refuses is reported at its <c># language:</c> header's line or else at line
/// <c>0</c>, and declares no claim; a set whose glob matches no file is reported at the glob as written; a file two sets
/// match is reported once and its claims are under neither set; a Feature-level tag naming another Feature than its set's
/// is reported at the tag's line, and nothing is reported where no form reads the tag; a <c>record</c> the check cannot
/// read is reported at its path as written, and a rooted one at the manifest, with no rooted path in any violation; and
/// nothing is reported without <c>claim.source</c> <c>gherkin</c> or below schema <c>0.4.0</c>. Unless a test says
/// otherwise, the manifest pins <c>0.4.0</c>, sets <c>claim.source</c> <c>gherkin</c> with one set, glob
/// <c>.spec/features/arithmetic/*.feature</c> and feature <c>F1</c>, declares no epic grammar, and excludes <c>obj</c>
/// alone. A path is under the root <c>repo</c>, so <c>../outside/README.md</c> is a file outside it.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ClaimInputFaultRuleUnitTests
{
    [Theory]
    [ClassData(typeof(UnreadableClaimInputs))]
    public void ClaimInputTheCheckCannotTakeClaimsFrom_WhenEvaluated_ShouldBeReportedAsASpec4013ErrorAtItsPlace(
        string members, string[] paths, string[] texts, string[] violations)
    {
        // Given
        var tree = paths.Zip(texts, static (path, text) => (Path: path, Text: text)).ToArray();

        // When
        var report = Evaluate(Read(members, tree));

        // Then
        report.Violations.Where(static violation => violation.RuleId == "SPEC4013")
            .Select(static violation => $"{violation.RuleId} {violation.Severity} {violation.File}({violation.Line})")
            .Should()
            .BeEquivalentTo(violations);
    }

    [Theory]
    [ClassData(typeof(UnclaimableClaimFiles))]
    public void AClaimFileTheCheckCannotTakeClaimsFrom_WhenRead_ShouldRecordNoClaimFromItUnderAnySet(
        string members, string[] texts, string[] claims)
    {
        // Given
        var tree = texts.Select(static (text, index) => (Path: ClaimFiles[index], Text: text)).ToArray();

        // When
        var model = Read(members, tree);

        // Then
        model.ClaimSets.SelectMany(static set => set.Rows.Select(claim => $"{set.FileSet!.Feature} {claim.Id}")).Should().Equal(claims);
    }

    [Theory]
    [ClassData(typeof(FeatureTagFormsReadingNoTag))]
    public void AFeatureLevelTagNoFeatureTagFormReads_WhenEvaluated_ShouldReportNoSpec4013(string members)
    {
        // Given
        (string Path, string Text)[] tree = [(ClaimFiles[0], "@spec.feature:F2\nFeature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n")];

        // When
        var violations = Evaluate(Read(members, tree)).Violations;

        // Then
        violations.Should().NotContain(static violation => violation.RuleId == "SPEC4013");
    }

    [Fact]
    public void ARootedRecord_WhenEvaluated_ShouldPutNoRootedPathInAnyViolation()
    {
        // Given
        const string Members = """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": "/outside/README.md" } ] } }
            """;
        (string Path, string Text)[] tree =
        [
            (ClaimFiles[0], "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n"),
            ("../outside/README.md", "---\nid: F1\nspec_status: approved\n---\n\n# Arithmetic\n"),
        ];

        // When
        var violations = Evaluate(Read(Members, tree)).Violations;

        // Then
        violations.Should().Contain(static violation => violation.RuleId == "SPEC4013");
        violations.Should().NotContain(static violation => violation.File.StartsWith('/')
            || violation.Message.Contains("/outside", StringComparison.Ordinal)
            || (violation.Identifier ?? string.Empty).Contains("/outside", StringComparison.Ordinal));
    }

    [Fact]
    public void AMarkdownTreeUnderZeroFourZero_WhenItsCompanionCannotBeReadAsClaimInput_ShouldReportNoSpec4013()
    {
        // Given
        (string Path, string Text)[] tree =
        [
            ("src/arithmetic/.spec/README.md", "---\nid: F1\n---\n"),
            ("src/arithmetic/.spec/arithmetic.feature", "# language: fr\nFonctionnalité: Arithmétique\n"),
        ];

        // When
        var violations = Evaluate(Read("""{ "claim": { "source": "markdown" } }""", tree)).Violations;

        // Then
        violations.Should().NotContain(static violation => violation.RuleId == "SPEC4013");
    }

    [Fact]
    public void AGherkinClaimSourceUnderZeroThreeOne_WhenEachClaimInputFaultIsPresent_ShouldNeitherReportNorCountSpec4013()
    {
        // Given
        const string Members = """
            { "schemaVersion": "0.3.1", "claim": { "scopes": { "featureTag": "@spec.feature:{feature}" }, "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" },
              { "glob": ".spec/features/sums/*.feature", "feature": "F2", "record": "/outside/README.md" },
              { "glob": ".spec/features/**/subtract.feature", "feature": "F3" } ] } }
            """;
        (string Path, string Text)[] tree =
        [
            (ClaimFiles[0], "# language: fr\nFonctionnalité: Arithmétique\n"),
            (ClaimFiles[1], "@spec.feature:F9\nFeature: Arithmetic\n\n  @B-002\n  Scenario: Subtracts\n    Given two numbers\n"),
        ];

        // When
        var report = Evaluate(Read(Members, tree));

        // Then
        report.Violations.Should().NotContain(static violation => violation.RuleId == "SPEC4013");
        report.RulesEvaluated.Should().Be(ZeroThreeOneRulesEvaluated);
    }

    private static SpechtModel Read(string members, (string Path, string Text)[] files)
    {
        var manifest = JsonNode.Parse(members)!.AsObject();
        manifest["schemaVersion"] ??= "0.4.0";
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

        return EngineServices.Reader(fileSystem).Read(fileSystem.Path.GetFullPath("repo"), null);
    }

    private static SpechtReport Evaluate(SpechtModel model) => new SpechtEvaluator(EngineServices.Rules(), new SpechtViolationOrder()).Evaluate(model);

    private const int ZeroThreeOneRulesEvaluated = 23;

    private static readonly string[] ClaimFiles = [".spec/features/arithmetic/add.feature", ".spec/features/arithmetic/subtract.feature"];
}
