using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules.Matrix;

/// <summary>SPEC022 over an in-memory tree whose matrix headers are renamed (0001-F12 B-008 to B-010).</summary>
[Trait("Tier", "Unit")]
public sealed class ScenarioTitleRuleUnitTests
{
    [Theory]
    [ClassData(typeof(ScenarioTitleCells))]
    public void AScenarioCell_WhenEvaluated_ShouldReportEachTitleNoScenarioCarryingTheRowTagHasOnlyUnderTitleMatching(
        string? matrix, string cell, string[] expected)
    {
        // Given
        var manifest = JsonNode.Parse("""
            {"schemaVersion":"0.3.0","tables":{"matrix":["Claim","Case","Tests","State"]}}
            """)!.AsObject();
        if (matrix is not null)
        {
            manifest["matrix"] = JsonNode.Parse(matrix);
        }

        IFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpechtTree.ManifestPath)] = new(manifest.ToJsonString()),
            [Path.Combine("repo", "src", "sample", ".spec", "README.md")] = new($$"""
                ---
                epic: "0001"
                id: F1
                ---
                ## 9. Traceability Matrix
                | Claim | Case | Tests | State |
                | --- | --- | --- | --- |
                | B-001 | {{cell}} | `AlphaTest` | Partial |
                """),
            [Path.Combine("repo", "src", "sample", ".spec", "sample.feature")] = new("""
                Feature: Sample

                  @B-001
                  Scenario: First case
                    Given a thing

                  @B-001
                  Scenario Outline: Second case
                    Given <thing>

                    Examples:
                      | thing |
                      | one   |

                  @B-002
                  Scenario: Setting a level succeeds
                    Given a thing
                """),
        });
        var model = EngineServices.Loader(fileSystem).Load(fileSystem.Path.GetFullPath("repo"));
        var rule = EngineServices.Rules().SingleOrDefault(static rule => rule.Id == "SPEC022");

        // When
        rule.Should().NotBeNull("schema 0.3.0 ships the scenario-title rule");
        var findings = rule!.Evaluate(model).ToArray();

        // Then
        findings.Should().HaveCount(expected.Length);
        findings.Should().OnlyContain(static finding => finding.File == "src/sample/.spec/README.md" && finding.Line == 8);
        foreach (var title in expected)
        {
            findings.Should().ContainSingle(finding => finding.Message.Contains(title, StringComparison.Ordinal));
        }
    }
}
