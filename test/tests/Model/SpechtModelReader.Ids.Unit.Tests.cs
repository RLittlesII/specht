using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>
/// A synthetic tree in an in-memory file system, read by the model reader against <see cref="IdsVersion"/>'s catalog and
/// evaluated by the composed rules, under a manifest pinned to schema 0.4.0 (<c>0001-F5</c> B-086 to B-088, B-091, B-095,
/// B-097, B-098, B-100, B-101; A-8; decision 0024; item <c>0436</c>). A Feature's claim grammar is its head, the claim
/// record's <c>form</c> with its prefix or else the <c>plain</c>, without the closing <c>{id}</c> and matched as literal
/// text, then a whole match of the record's <c>id</c>, case-sensitive; what a manifest's record leaves out is the default
/// record's. Each row is the manifest's members beside <c>schemaVersion</c>, as JSON.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtModelReaderIdsUnitTests
{
    [Theory]
    [ClassData(typeof(ClaimRecordGrammars))]
    public void ClaimIdsUnderAZeroFourZeroClaimRecord_WhenEvaluated_ShouldReportEveryIdThatIsNotTheHeadThenAWholeMatchOfTheRecordsId(
        string members,
        string? prefix,
        string[] claims,
        string[] reported)
    {
        // When
        var violations = Check(members, (Activity, Feature(prefix, claims)));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC4003" && violation.File == Activity)
            .Select(static violation => violation.Identifier)
            .Should()
            .Equal(reported);
    }

    [Fact]
    public void AClaimOutsideTheClaimRecordsForm_WhenEvaluated_ShouldBeReportedAtItsDeclarationLineNamingTheAuthoredIdAndTheHead()
    {
        // When
        var violations = Check(
            """{ "ids": { "claim": { "form": "{prefix}-{id}", "id": "[0-9a-hjkmnp-tv-z]{6}|[0-9a-hjkmnp-tv-z]{13}" } } }""",
            (Activity, Feature("ACTIVITY", ["ACTIVITY-k3x9ab", "ACTIVITY-001"])));

        // Then
        var finding = violations.Where(static violation => violation.RuleId == "SPEC4003").Should().ContainSingle().Subject;
        finding.File.Should().Be(Activity);
        finding.Line.Should().Be(10);
        finding.Identifier.Should().Be("ACTIVITY-001");
        finding.Message.Should().Contain("'ACTIVITY-001'").And.Contain("'ACTIVITY-'");
    }

    [Fact]
    public void AMatrixAndCompanionCitingAClaimRecordShapedIdAndAnUndeclaredOne_WhenEvaluated_ShouldReportOnlyTheUndeclaredOne()
    {
        // When
        var violations = Check(
            """{ "ids": { "claim": { "form": "{prefix}-{id}", "id": "[0-9a-hjkmnp-tv-z]{6}|[0-9a-hjkmnp-tv-z]{13}" } } }""",
            (Activity, Feature("ACTIVITY", ["ACTIVITY-k3x9ab"])
                + "\n## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -- | -- | -- | -- |\n"
                + "| ACTIVITY-k3x9ab | It holds | Missing | Missing |\n| ACTIVITY-m7p2qr | It holds | Missing | Missing |\n"),
            (Companion, Tagged("@ACTIVITY-k3x9ab", "@ACTIVITY-m7p2qr")));

        // Then
        violations.Where(static violation => violation.RuleId is "SPEC4004" or "SPEC4002")
            .Select(static violation => (violation.RuleId, violation.File, violation.Identifier))
            .Should()
            .BeEquivalentTo([("SPEC4004", Activity, "ACTIVITY-m7p2qr"), ("SPEC4002", Companion, "ACTIVITY-m7p2qr")]);
    }

    [Theory]
    [ClassData(typeof(LongerScenarioHeadTags))]
    public void ACompanionTagBeginningWithALongerScenarioHead_WhenEvaluated_ShouldNotBeReadAsAClaimTag(string members, string tag)
    {
        // When
        var violations = Check(
            members,
            (Activity, Feature("ACTIVITY", ["ACTIVITY-k3x9ab"])),
            (Companion, Tagged("@ACTIVITY-k3x9ab", tag)));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC4002").Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(SharedHeadPrefixes))]
    public void AFeatureWhosePrefixGivesItsClaimsAndScenariosOneHead_WhenEvaluated_ShouldBeReportedAtItsClaimPrefixNamingTheHeadAndNoClaimOfIt(
        string members,
        string prefix)
    {
        // When
        var violations = Check(members, (Activity, Feature(prefix, ["B-001"])));

        // Then
        var finding = violations.Where(static violation => violation.RuleId == "SPEC4003").Should().ContainSingle().Subject;
        finding.File.Should().Be(Activity);
        finding.Line.Should().Be(2);
        finding.Identifier.Should().BeEquivalentTo("S-");
        finding.Message.Should().Contain($"'{finding.Identifier}'");
    }

    [Theory]
    [ClassData(typeof(ScenarioHeadClaims))]
    public void ClaimIdsWhoseTextAfterTheClaimHeadBeginsWithTheRestOfALongerScenarioHead_WhenEvaluated_ShouldEachBeReported(
        string members,
        string[] claims,
        string[] reported)
    {
        // When
        var violations = Check(members, (Activity, Feature("ACTIVITY", claims)));

        // Then
        violations.Where(static violation => violation.RuleId == "SPEC4003" && violation.File == Activity)
            .Select(static violation => violation.Identifier)
            .Should()
            .Equal(reported);
    }

    [Fact]
    public void RecordsTheCheckDoesNotRead_WhenRemovedFromAZeroFourZeroManifest_ShouldLeaveTheReportUnchanged()
    {
        // Given
        (string Path, string Text)[] tree =
        [
            (Activity, Feature("ACTIVITY", ["ACTIVITY-001", "ACTIVITY-k3x9ab"])),
            (Companion, Tagged("@ACTIVITY-k3x9ab", "@S-1")),
        ];
        const string unread = """
            { "ids": {
              "claim": { "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "0123456789abcdefghjkmnpqrstvwxyz", "size": 6 } } },
              "constraint": { "plain": "CON-{id}", "id": "[a-z]+" },
              "openQuestion": { "plain": "Q-{id}", "id": "[a-z]+" },
              "scenario": { "id": "[a-z]+" },
              "decision": { "plain": "D-{id}", "id": "[a-z]+" },
              "adr": { "plain": "ADR-{id}", "id": "[a-z]+" },
              "lesson": { "plain": "L-{id}", "id": "[a-z]+" } } }
            """;

        // When
        var withRecords = Check(unread, tree);
        var without = Check("{}", tree);

        // Then
        withRecords.Should().Equal(without);
    }

    private static string Feature(string? prefix, IEnumerable<string> claims) =>
        (prefix is null ? "---\nid: F1\n---\n" : $"---\nclaim_prefix: '{prefix}'\n---\n")
        + "\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n"
        + string.Concat(claims.Select(static claim => $"| {claim} | It holds. |\n"));

    private static string Tagged(string first, string second) =>
        $"Feature: activity\n\n  {first}\n  Scenario: held\n    Given it\n\n  {second}\n  Scenario: other\n    Given it\n";

    private static IReadOnlyList<SpechtViolation> Check(string members, params (string Path, string Text)[] files)
    {
        var manifest = JsonNode.Parse(members)!.AsObject();
        manifest.Insert(0, "schemaVersion", IdsVersion.Number.ToString());
        var fileSystem = new MockFileSystem(
            files.Append((Path: SpechtTree.ManifestPath, Text: manifest.ToJsonString()))
                .ToDictionary(
                    static file => Path.Combine(["repo", .. file.Path.Split('/')]),
                    static file => new MockFileData(file.Text)));

        return new SpechtEvaluator(EngineServices.Rules(), new SpechtViolationOrder())
            .Evaluate(EngineServices.Reader(fileSystem, IdsVersion.Catalog()).Read(fileSystem.Path.GetFullPath("repo"), null))
            .Violations;
    }

    private const string Activity = "src/activity/.spec/README.md";

    private const string Companion = "src/activity/.spec/activity.feature";
}
