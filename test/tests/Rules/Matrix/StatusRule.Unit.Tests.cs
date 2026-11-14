using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules.Matrix;

/// <summary>
/// SPEC033 over an in-memory tree whose matrix headers and markers are renamed (0001-F12 B-012 to B-017, B-028 to B-030, A-1 to A-3, C-3, C-5),
/// alone and beside SPEC023 and SPEC032 under the 0.4.0 pin, which reports them under <c>SPEC4010</c>, <c>SPEC4008</c> and
/// <c>SPEC4009</c> (ADR-0029; decision 0005).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class StatusRuleUnitTests
{
    [Theory]
    [ClassData(typeof(StatusRows))]
    public void AMatrixRow_WhenEvaluated_ShouldReportAStatusItsScenarioAndTestsDoNotBearOut(
        bool declared, bool titles, string scenario, string test, string status, string[]? expected)
    {
        // Given
        var manifest = JsonNode.Parse("""
            {"schemaVersion":"0.4.0","tables":{"matrix":["Claim","Case","Tests","State"]},
             "markers":{"covered":"Proven","partial":"Halfway","missing":"Absent","waived":"Excused","withdrawn":"Retired","superseded":"Replaced"}}
            """)!.AsObject();
        if (declared)
        {
            manifest["tests"] = JsonNode.Parse("""{"files":["test/**/*.cs"]}""");
        }

        if (titles)
        {
            manifest["matrix"] = JsonNode.Parse("""{"scenarioTitles":true}""");
        }

        IFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpechtTree.ManifestPath)] = new(manifest.ToJsonString()),
            [Path.Combine("repo", "src", "sample", ".spec", "README.md")] = new($$"""
                ---
                epic: "0001"
                id: F1
                ---
                ## 3. Acceptance Criteria
                | ID | Claim | Source | Status |
                | --- | --- | --- | --- |
                | B-001 | Behavior | Decision | Active |
                ## 9. Traceability Matrix
                | Claim | Case | Tests | State |
                | --- | --- | --- | --- |
                | B-001 | {{scenario}} | {{test}} | {{status}} |
                """),
            [Path.Combine("repo", "src", "sample", ".spec", "sample.feature")] = new("""
                Feature: Sample

                  @B-001
                  Scenario: First case
                    Given a thing

                  @B-002
                  Scenario: Other case
                    Given a thing
                """),
            [Path.Combine("repo", "test", "Names.cs")] = new("class Names\n{\n    void AlphaTest() {}\n    void BetaTest() {}\n}\n"),
        });
        var model = EngineServices.Reader(fileSystem).Read(fileSystem.Path.GetFullPath("repo"), null);
        var rule = EngineServices.Rules().SingleOrDefault(static rule => rule.Id == "SPEC033");

        // When
        rule.Should().NotBeNull("schema 0.4.0 ships the status rule");
        var findings = rule!.Evaluate(model).ToArray();

        // Then
        findings.Should().HaveCount(expected is null ? 0 : 1);
        if (expected is not null)
        {
            findings[0].File.Should().Be("src/sample/.spec/README.md");
            findings[0].Line.Should().Be(12);
            foreach (var text in expected)
            {
                findings[0].Message.Should().Contain(text);
            }
        }
    }

    [Theory]
    [ClassData(typeof(StatusBesideOtherRules))]
    public void AMatrixRow_WhenTheMissingScenarioUnresolvedTestAndStatusRulesEvaluate_ShouldReportTheStatusBesideSpec4009AndNeverBesideSpec4008(
        string criterion, string companion, string? level, string test, string status, string[] expected)
    {
        // Given
        var manifest = JsonNode.Parse("""
            {"schemaVersion":"0.4.0","tables":{"matrix":["Claim","Case","Tests","State"]},"tests":{"files":["test/**/*.cs"]},
             "markers":{"covered":"Proven","partial":"Halfway","missing":"Absent","waived":"Excused","withdrawn":"Retired","superseded":"Replaced"}}
            """)!.AsObject();
        if (level is not null)
        {
            manifest["rules"] = new JsonObject { ["SPEC4008"] = level };
        }

        IFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpechtTree.ManifestPath)] = new(manifest.ToJsonString()),
            [Path.Combine("repo", "src", "sample", ".spec", "README.md")] = new($$"""
                ---
                epic: "0001"
                id: F1
                ---
                ## 3. Acceptance Criteria
                | ID | Claim | Source | Status |
                | --- | --- | --- | --- |
                | {{criterion}} |
                ## 9. Traceability Matrix
                | Claim | Case | Tests | State |
                | --- | --- | --- | --- |
                | B-001 | First case | {{test}} | {{status}} |
                """),
            [Path.Combine("repo", "src", "sample", ".spec", "sample.feature")] = new(companion),
            [Path.Combine("repo", "test", "Names.cs")] = new("class Names\n{\n    void AlphaTest() {}\n    void BetaTest() {}\n}\n"),
        });
        var model = EngineServices.Reader(fileSystem).Read(fileSystem.Path.GetFullPath("repo"), null);
        var rules = EngineServices.Rules().Where(static rule => rule.Id is "SPEC023" or "SPEC032" or "SPEC033").ToArray();

        // When
        rules.Select(static rule => rule.Id).Should().BeEquivalentTo(["SPEC023", "SPEC032", "SPEC033"], "schema 0.4.0 ships the three rules");
        var violations = new SpechtEvaluator(rules, new SpechtViolationOrder()).Evaluate(model).Violations.ToArray();

        // Then
        violations.Should().OnlyContain(static violation => violation.File == "src/sample/.spec/README.md" && violation.Line == 12);
        violations.Select(static violation => violation.RuleId).Should().BeEquivalentTo(expected);
    }
}
