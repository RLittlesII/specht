using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules.Matrix;

/// <summary>SPEC023 over an in-memory tree whose matrix headers and markers are renamed (0001-F12 B-011, B-026, B-027, A-3, C-3, C-5).</summary>
[Trait("Tier", "Unit")]
public sealed class MissingScenarioRuleUnitTests
{
    [Theory]
    [ClassData(typeof(MissingScenarioRows))]
    public void AClaimAndItsMatrixRows_WhenEvaluated_ShouldReportEachNonExemptRowOfALiveClaimNoCompanionTagCarries(
        string claimStatus, string companion, string[] rowStatuses, int[] reported)
    {
        // Given
        IFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpechtTree.ManifestPath)] = new("""
                {"schemaVersion":"0.4.0","tables":{"matrix":["Claim","Case","Tests","State"]},
                 "markers":{"missing":"Absent","waived":"Excused","withdrawn":"Retired","superseded":"Replaced"}}
                """),
            [Path.Combine("repo", "src", "sample", ".spec", "README.md")] = new($$"""
                ---
                epic: "0001"
                id: F1
                ---
                ## 3. Acceptance Criteria
                | ID | Claim | Source | Status |
                | --- | --- | --- | --- |
                | B-001 | Behavior | Decision | {{claimStatus}} |
                ## 9. Traceability Matrix
                | Claim | Case | Tests | State |
                | --- | --- | --- | --- |
                {{string.Concat(rowStatuses.Select(static status => $"| B-001 | A scenario | `AlphaTest` | {status} |\n"))}}
                """),
            [Path.Combine("repo", "src", "sample", ".spec", "sample.feature")] = new(companion),
        });
        var model = EngineServices.Reader(fileSystem).Read(fileSystem.Path.GetFullPath("repo"), null);
        var rule = EngineServices.Rules().SingleOrDefault(static rule => rule.Id == "SPEC023");

        // When
        rule.Should().NotBeNull("schema 0.4.0 ships the missing-scenario rule");
        var findings = rule!.Evaluate(model).ToArray();

        // Then
        findings.Should().OnlyContain(static finding => finding.File == "src/sample/.spec/README.md");
        findings.Select(static finding => finding.Line).Should().BeEquivalentTo(reported);
    }
}
