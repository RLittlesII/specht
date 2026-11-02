using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules.Matrix;

/// <summary>SPEC032 over an in-memory tree with consumer-owned headers, markers and exclusions (0001-F12 B-001 to B-007).</summary>
[Trait("Tier", "Unit")]
public sealed class UnresolvedTestRuleUnitTests
{
    [Theory]
    [ClassData(typeof(UnresolvedTestCells))]
    public void ATestCell_WhenEvaluated_ShouldReportAnUnresolvedNameOrACellWithoutInlineCodeAtItsRow(
        string? tests, string cell, string status, string? expected)
    {
        // Given
        var manifest = JsonNode.Parse("""
            {"schemaVersion":"0.3.0","exclusions":["obj"],
             "tables":{"matrix":["Claim","Case","Tests","State"]},
             "markers":{"missing":"Absent","covered":"Done","waived":"Excused","withdrawn":"Retired","superseded":"Replaced"}}
            """)!.AsObject();
        if (tests is not null)
        {
            manifest["tests"] = JsonNode.Parse(tests);
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
                | B-002 | A case | {{cell}} | {{status}} |
                """),
            [Path.Combine("repo", "test", "Names.cs")] = new("class ReportSteps\n{\n    void AlphaTest() {}\n    void BetaTest() {}\n}\n"),
            [Path.Combine("repo", "test", "obj", "Hidden.cs")] = new("void HiddenTest() {}\n"),
            [Path.Combine("repo", "test", "Forms.fs")] = new("let DeclaredTest \n// CommentedTest\n"),
        });
        var model = EngineServices.Loader(fileSystem).Load(fileSystem.Path.GetFullPath("repo"));
        var rule = EngineServices.Rules().SingleOrDefault(static rule => rule.Id == "SPEC032");

        // When
        rule.Should().NotBeNull("schema 0.3.0 ships the unresolved-test rule");
        var findings = rule!.Evaluate(model).ToArray();

        // Then
        findings.Should().HaveCount(expected is null ? 0 : 1);
        if (expected is not null)
        {
            findings[0].File.Should().Be("src/sample/.spec/README.md");
            findings[0].Line.Should().Be(8);
            findings[0].Identifier.Should().Be("B-002");
            findings[0].Message.Should().Contain(expected);
        }
    }
}
