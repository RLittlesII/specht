using AwesomeAssertions;
using Specht.Report;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Engine;

/// <summary>
/// Exercises each day-one rule family against a synthetic tree - a positive
/// case that must stay green, and a negative case that must actually fire.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtRunnerUnitTests
{
    [Fact]
    public void EveryRuleInTheEngine_WhenItsReportedIdsAreRead_ShouldNameExactlyTheTwentyOneVersion1RuleIds()
    {
        // Given
        var rules = SpecRules.All;

        // When
        var ids = rules.SelectMany(static rule => rule.ReportedIds).ToList();

        // Then
        ids.Should().BeEquivalentTo(
            "SPEC001", "SPEC002", "SPEC003", "SPEC004", "SPEC010", "SPEC011", "SPEC012", "SPEC013", "SPEC020", "SPEC021", "SPEC030",
            "SPEC031", "SPEC040", "SPEC041", "SPEC043", "SPEC044", "SPEC050", "SPEC051", "SPEC052", "SPEC060", "SPEC061");
    }

    [Fact]
    public void ATreeWithOneValidSpecification_WhenChecked_ShouldReportNoViolations()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().BeEmpty();
        report.SpecificationCount.Should().Be(1);
        report.Layouts.Should().Equal(new SpecReportLayout("epics", 1), new SpecReportLayout("features", 0));
    }

    [Fact]
    public void AFrontmatterValueOutsideItsEnum_WhenChecked_ShouldReportSpec002()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC002" && violation.Identifier == "spec_status");
    }

    [Fact]
    public void ASpecificationCarryingAlignmentRejections_WhenChecked_ShouldReportNoViolations()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["alignment_rejections"] = "0" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().BeEmpty();
    }

    [Fact]
    public void AnUnquotedZeroPaddedEpic_WhenChecked_ShouldStayAString()
    {
        // Given - epic: 0001 without quotes, which a YAML deserializer would read as the integer 1
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["epic"] = "0001" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().BeEmpty();
    }

    [Fact]
    public void AnUnscoredSpecificationAsTheTemplateWritesIt_WhenChecked_ShouldReportNoViolations()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string>
        {
            ["title"] = "\"Specification: A thing\"",
            ["description"] = "\"Does the thing.\"",
            ["value"] = "0",
            ["risk"] = "0",
            ["rank"] = "0",
            ["scored_by"] = "null",
            ["scored_on"] = "null",
        });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().BeEmpty();
    }

    [Fact]
    public void AScoredSpecificationWithAZeroValue_WhenChecked_ShouldReportSpec002()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["value"] = "0" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC002" && violation.Identifier == "value");
    }

    [Fact]
    public void ALinkedSpecificationWithNoSyncedAt_WhenChecked_ShouldReportSpec002()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["github_issue"] = "121" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC002");
    }

    [Fact]
    public void AMissingSection_WhenChecked_ShouldReportSpec010()
    {
        // Given
        using var tree = new SpecTree();
        var sections = SpecTree.Sections.Where(static section => !section.StartsWith("## 11.", StringComparison.Ordinal)).ToList();
        tree.WriteFeature("0001", "F1", sections: sections);

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC010" && violation.Identifier == "11. Open Questions");
    }

    [Fact]
    public void ATraceabilityMatrixWithTheWrongHeaders_WhenChecked_ShouldReportSpec013()
    {
        // Given
        using var tree = new SpecTree();
        var sections = SpecTree.Sections
            .Select(static section => section.StartsWith("## 9.", StringComparison.Ordinal)
                ? "## 9. Traceability Matrix\n\n| Claim | Test |\n| ----- | ---- |\n| B-001 | Missing |\n"
                : section)
            .ToList();
        tree.WriteFeature("0001", "F1", sections: sections);

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC013");
    }

    [Fact]
    public void FrontmatterThatDisagreesWithItsDirectory_WhenChecked_ShouldReportSpec011()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["epic"] = "\"0002\"" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC011");
    }

    [Fact]
    public void AClaimWithNoTraceabilityRow_WhenChecked_ShouldReportSpec031()
    {
        // Given
        using var tree = new SpecTree();
        var sections = SpecTree.Sections
            .Select(static section => section.StartsWith("## 3.", StringComparison.Ordinal)
                ? section + "| B-002 | It does another thing. | brd | Active |\n"
                : section)
            .ToList();
        tree.WriteFeature("0001", "F1", sections: sections);

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC031" && violation.Identifier == "B-002");
    }

    [Fact]
    public void ADuplicateTraceabilityRow_WhenChecked_ShouldReportSpec031()
    {
        // Given
        using var tree = new SpecTree();
        var sections = SpecTree.Sections
            .Select(static section => section.StartsWith("## 9.", StringComparison.Ordinal)
                ? section + "| B-001 | It does the thing again | Missing | Missing |\n"
                : section)
            .ToList();
        tree.WriteFeature("0001", "F1", sections: sections);

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC031" && violation.Message.Contains("2 rows", StringComparison.Ordinal));
    }

    [Fact]
    public void AScenarioTagThatResolvesToNoClaim_WhenChecked_ShouldReportSpec021()
    {
        // Given
        using var tree = new SpecTree();
        var path = tree.WriteFeature("0001", "F1");
        tree.WriteFeatureFile(path, "Feature: it\n\n  @B-404\n  Scenario: Phantom\n    Given nothing\n");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC021" && violation.Identifier == "B-404");
    }

    [Fact]
    public void AClaimTagInsideAGherkinComment_WhenChecked_ShouldNotBeTreatedAsATag()
    {
        // Given
        using var tree = new SpecTree();
        var path = tree.WriteFeature("0001", "F1");
        tree.WriteFeatureFile(
            path,
            "Feature: it\n\n  # Superseded: @B-404 and @B-405 were withdrawn\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().NotContain(static violation => violation.RuleId == "SPEC021");
    }

    [Fact]
    public void ASpecificationWithNoCompanionGherkin_WhenChecked_ShouldReportSpec020()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", featureFile: null);

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC020");
    }

    [Fact]
    public void ADeclaredChildWithNoFile_WhenChecked_ShouldReportSpec040()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["children"] = "[\"0001-01\"]" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC040" && violation.Identifier == "0001-01");
    }

    [Fact]
    public void AnItemWhoseIdDisagreesWithItsFileName_WhenChecked_ShouldReportSpec043()
    {
        // Given
        using var tree = new SpecTree();
        var path = tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["children"] = "[\"0001-01\"]" });
        tree.WriteItem(path, "0001-01", "0001-F1", idOverride: "0001-09");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC043");
    }

    [Fact]
    public void AnItemSequenceThatSkipsANumber_WhenChecked_ShouldReportSpec044()
    {
        // Given
        using var tree = new SpecTree();
        var path = tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["children"] = "[\"0001-02\"]" });
        tree.WriteItem(path, "0001-02", "0001-F1");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC044");
    }

    [Fact]
    public void ADependencyDeclaredFromOnlyOneEnd_WhenChecked_ShouldReportSpec051()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["depends_on"] = "[\"F2\"]" });
        tree.WriteFeature("0001", "F2");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC051" && violation.Identifier == "0001-F2");
    }

    [Fact]
    public void ASymmetricDependency_WhenChecked_ShouldReportNoViolations()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["depends_on"] = "[\"F2\"]" });
        tree.WriteFeature("0001", "F2", new Dictionary<string, string> { ["blocks"] = "[\"F1\"]" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().BeEmpty();
    }

    [Fact]
    public void ADependencyOnAFeatureThatDoesNotExist_WhenChecked_ShouldReportSpec050()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["depends_on"] = "[\"0009/F9\"]" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC050" && violation.Identifier == "0009-F9");
    }

    [Fact]
    public void AnApprovedSpecificationWithAMissingTest_WhenChecked_ShouldReportSpec060()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "approved" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC060" && violation.Identifier == "B-001");
    }

    [Fact]
    public void ADraftSpecificationWithAMissingTest_WhenChecked_ShouldNotFail()
    {
        // Given - every § 9 row in this repository currently reads Missing.
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().NotContain(static violation => violation.RuleId == "SPEC060" || violation.RuleId == "SPEC061");
    }
}
