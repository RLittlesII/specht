using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// The feature-file rule over a tree on disk under a manifest that renames the claims section (<c>0001-F5</c> B-001,
/// decision 0006): a scenario tag resolves against the section the claims role names. On disk because the rule reads the
/// companion file's tags from its path.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class FeatureFileRuleIntegrationTests
{
    [Fact]
    public void ATagDeclaredOnlyUnderTheTitleNoRoleNames_WhenChecked_ShouldReportSpec021OnItAndNotOnTheTagTheClaimsRolesSectionDeclares()
    {
        // Given
        using var tree = new SpecTree();
        var path = Path.Combine(tree.Root, ".spec", "schema", "spec-structure.schema.json");
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var titles = manifest["sections"]!.AsArray();
        titles[titles.Select(static title => title!.GetValue<string>()).ToList().IndexOf("3. Acceptance Criteria")] = "3. Claims";
        manifest["roles"] = new JsonObject { ["claims"] = "3. Claims" };
        File.WriteAllText(path, manifest.ToJsonString());
        var sections = SpecTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Claims\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + "| B-001 | It does the thing. | brd | Active |\n")
            .Append(
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + "| B-404 | Under the title no role names. | brd | Active |\n")
            .ToList();
        tree.WriteFeatureFile(
            tree.WriteFeature("0001", "F1", sections: sections),
            "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n  @B-404\n  Scenario: Phantom\n    Given nothing\n");

        // When
        var report = tree.Run();

        // Then
        report.Violations
            .Where(static violation => violation.RuleId == "SPEC021")
            .Select(static violation => violation.Identifier)
            .Should()
            .Equal("B-404");
    }
}
