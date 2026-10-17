using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules.Matrix;

/// <summary>The real runner selects, grades and counts versioned waiver rules (0001-F12 B-020, B-021; 0001-F5 B-061, B-062).</summary>
[Trait("Tier", "Integration")]
public sealed class WaiverAndRetiredRulesIntegrationTests
{
    [Theory]
    [ClassData(typeof(WaiverRuleLevels))]
    public void TheShippedRules_WhenTheRunnerChecksATree_ShouldHonorThePinOwnOffDefaultsAndExplicitOverrides(
        string pin, string? level, int findings, int evaluated)
    {
        // Given
        using var tree = new SpecTree();
        var sections = SpecTree.Sections.Select(static section => section.Split('\n')[0] switch
        {
            "## 3. Acceptance Criteria" => "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n"
                + "| B-001 | Retired | Decision | Withdrawn |\n",
            "## 9. Traceability Matrix" => "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| --- | --- | --- | --- |\n"
                + "| B-001 | A waiver | | Waived |\n",
            _ => section,
        }).ToArray();
        var path = tree.WriteFeature("0001", "F1", sections: sections);
        tree.WriteFeatureFile(path, "Feature: Example\n@B-001\nScenario: Old case\n  Given a thing\n");
        var manifest = new JsonObject { ["schemaVersion"] = pin };
        if (level is not null)
        {
            manifest["rules"] = new JsonObject { ["SPEC034"] = level, ["SPEC035"] = level };
        }

        tree.WriteRaw(SpecTree.ManifestPath, manifest.ToJsonString());

        // When
        var report = EngineServices.Runner().Run(tree.Root);

        // Then
        report.Violations.Should().HaveCount(findings);
        report.RulesEvaluated.Should().Be(evaluated);
        if (findings > 0)
        {
            report.Violations.Select(static violation => violation.RuleId).Should().BeEquivalentTo("SPEC034", "SPEC035");
            report.Violations.Should().OnlyContain(violation => violation.Severity == (level == "warning" ? SpecSeverity.Warning : SpecSeverity.Error));
            report.Violations.Should().OnlyContain(static violation => violation.File == "epics/0001-epic/F1-feature/spec.md" && violation.Line > 0);
        }
    }
}
