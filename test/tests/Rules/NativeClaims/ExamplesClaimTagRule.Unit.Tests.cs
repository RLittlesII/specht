using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules.NativeClaims;

/// <summary>
/// <c>SPEC4012</c> over a synthetic tree in an in-memory file system, read by the model reader and evaluated by the
/// composed rules (<c>0001-F16</c> B-009, B-002, B-028, C-3; § 7, item <c>0394</c>): a tag on an Examples block that the
/// claim tag form and the claim grammar read is reported at its claim file's path and the tag's line, at error by default,
/// with the claim id as its identifier, once however many sets match the file; it declares no claim, so it is no duplicate
/// and leaves an outline it alone tags without a claim; and nothing is reported without <c>claim.source</c>
/// <c>gherkin</c> or below schema <c>0.4.0</c>. Unless a test says otherwise, the manifest pins <c>0.4.0</c>, sets
/// <c>claim.source</c> <c>gherkin</c> with one set, glob <c>.spec/features/arithmetic/*.feature</c> and feature
/// <c>F1</c>, declares no epic grammar, and excludes <c>obj</c> alone.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ExamplesClaimTagRuleUnitTests
{
    [Theory]
    [ClassData(typeof(ExamplesClaimTags))]
    public void AClaimTagOnAnExamplesBlock_WhenEvaluated_ShouldBeReportedAsASpec4012ErrorAtTheTagsLineNamingTheClaimIdItWouldDeclare(
        string members, string[] texts, string[] violations)
    {
        // Given
        var tree = texts.Select(static (text, index) => (Path: ClaimFiles[index], Text: text)).ToArray();

        // When
        var report = Evaluate(Read(members, tree));

        // Then
        report.Violations.Where(static violation => violation.RuleId is "SPEC4012" or "SPEC4011" or "SPEC4003")
            .Select(static violation => $"{violation.RuleId} {violation.Severity} {violation.File}({violation.Line}) {violation.Identifier}".TrimEnd())
            .Should()
            .BeEquivalentTo(violations);
    }

    [Fact]
    public void AClaimTagOnAnExamplesBlockAlone_WhenRead_ShouldDeclareNoClaimForItsId()
    {
        // Given
        (string Path, string Text)[] tree =
        [
            (
                ClaimFiles[0],
                "Feature: Arithmetic\n\n  @B-001\n  Scenario Outline: Adds <a>\n    Given <a>\n\n    @B-005\n    Examples:\n      | a |\n      | 1 |\n"),
        ];

        // When
        var model = Read("{}", tree);

        // Then
        model.ClaimSets.Should().ContainSingle().Which.Rows.Select(static claim => claim.Id).Should().Equal("B-001");
    }

    [Fact]
    public void AMarkdownTreeUnderZeroFourZero_WhenItsCompanionHasAClaimTagOnAnExamplesBlock_ShouldReportNoSpec4012()
    {
        // Given
        (string Path, string Text)[] tree =
        [
            ("src/arithmetic/.spec/README.md", "---\nid: F1\n---\n"),
            (
                "src/arithmetic/.spec/arithmetic.feature",
                "Feature: Arithmetic\n\n  @B-001\n  Scenario Outline: Adds <a>\n    Given <a>\n\n    @B-005\n    Examples:\n      | a |\n      | 1 |\n"),
        ];

        // When
        var violations = Evaluate(Read("""{ "claim": { "source": "markdown" } }""", tree)).Violations;

        // Then
        violations.Should().NotContain(static violation => violation.RuleId == "SPEC4012");
    }

    [Fact]
    public void AGherkinClaimSourceUnderZeroThreeOne_WhenAClaimFileHasAClaimTagOnAnExamplesBlock_ShouldNeitherReportNorCountSpec4012()
    {
        // Given
        (string Path, string Text)[] tree =
        [
            (
                ClaimFiles[0],
                "Feature: Arithmetic\n\n  @B-001\n  Scenario Outline: Adds <a>\n    Given <a>\n\n    @B-005\n    Examples:\n      | a |\n      | 1 |\n"),
        ];

        // When
        var report = Evaluate(Read("""{ "schemaVersion": "0.3.1" }""", tree));

        // Then
        report.Violations.Should().NotContain(static violation => violation.RuleId == "SPEC4012");
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
