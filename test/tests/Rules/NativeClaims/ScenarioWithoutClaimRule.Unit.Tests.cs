using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules.NativeClaims;

/// <summary>
/// <c>SPEC4011</c> over a synthetic tree in an in-memory file system, read by the model reader and evaluated by the
/// composed rules (<c>0001-F16</c> B-010, B-002, B-028, C-3; § 7, item <c>0393</c>): a Scenario or Scenario Outline in a
/// claim file with no claim of its own, its Rule's or its Feature's is reported at its claim file's path and keyword line,
/// at warning by default, once however many sets match the file; and nothing is reported without <c>claim.source</c>
/// <c>gherkin</c> or below schema <c>0.4.0</c>. Unless a test says otherwise, the manifest pins <c>0.4.0</c>, sets
/// <c>claim.source</c> <c>gherkin</c> with one set, glob <c>.spec/features/arithmetic/*.feature</c> and feature
/// <c>F1</c>, declares no epic grammar, and excludes <c>obj</c> alone.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ScenarioWithoutClaimRuleUnitTests
{
    [Theory]
    [ClassData(typeof(UnclaimedScenarios))]
    public void AScenarioInAClaimFile_WhenEvaluated_ShouldBeReportedAsASpec4011WarningAtItsKeywordLineOnlyWhenItCarriesNoClaimOfItsOwnItsRulesOrItsFeatures(
        string members, string[] texts, string[] places)
    {
        // Given
        var tree = texts.Select(static (text, index) => (Path: ClaimFiles[index], Text: text)).ToArray();

        // When
        var violations = Evaluate(members, tree).Violations;

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC4011")
            .Select(static violation => $"{violation.Severity} {violation.File}({violation.Line})")
            .Should()
            .BeEquivalentTo(places.Select(static place => $"{SpechtSeverity.Warning} {place}"));
    }

    [Fact]
    public void AMarkdownTreeUnderZeroFourZero_WhenItsCompanionHoldsAnUntaggedScenario_ShouldReportNoSpec4011()
    {
        // Given
        (string Path, string Text)[] tree =
        [
            ("src/arithmetic/.spec/README.md", "---\nid: F1\n---\n"),
            ("src/arithmetic/.spec/arithmetic.feature", "Feature: Arithmetic\n\n  Scenario: Adds\n    Given two numbers\n"),
        ];

        // When
        var violations = Evaluate("""{ "claim": { "source": "markdown" } }""", tree).Violations;

        // Then
        violations.Should().NotContain(static violation => violation.RuleId == "SPEC4011");
    }

    [Fact]
    public void AGherkinClaimSourceUnderZeroThreeOne_WhenAClaimFileHoldsAnUntaggedScenario_ShouldNeitherReportNorCountSpec4011()
    {
        // Given
        (string Path, string Text)[] tree = [(ClaimFiles[0], "Feature: Arithmetic\n\n  Scenario: Adds\n    Given two numbers\n")];

        // When
        var report = Evaluate("""{ "schemaVersion": "0.3.1" }""", tree);

        // Then
        report.Violations.Should().NotContain(static violation => violation.RuleId == "SPEC4011");
        report.RulesEvaluated.Should().Be(ZeroThreeOneRulesEvaluated);
    }

    private static SpechtReport Evaluate(string members, (string Path, string Text)[] files)
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
        var model = EngineServices.Reader(fileSystem).Read(fileSystem.Path.GetFullPath("repo"), null);

        return new SpechtEvaluator(EngineServices.Rules(), new SpechtViolationOrder()).Evaluate(model);
    }

    private const int ZeroThreeOneRulesEvaluated = 23;

    private static readonly string[] ClaimFiles = [".spec/features/arithmetic/add.feature", ".spec/features/arithmetic/subtract.feature"];
}
