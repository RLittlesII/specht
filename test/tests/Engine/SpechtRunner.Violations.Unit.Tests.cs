using AwesomeAssertions;
using Specht.Tests.Shared;

namespace Specht.Tests.Engine;

/// <summary>
/// The violation paths the happy-path tests never reach: every rule id a rule
/// says it reports has at least one tree here that makes it fire.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtRunnerViolationsUnitTests
{
    [Fact]
    public void ASpecificationWithNoFrontmatter_WhenChecked_ShouldReportSpec001()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw("epics/0001-epic/F1-feature/spec.md", "# Specification: F1\n\n" + string.Join('\n', SpecTree.Sections));
        tree.WriteRaw("epics/0001-epic/F1-feature/feature.feature", "Feature: it\n");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC001");
    }

    [Fact]
    public void FrontmatterThatIsNotValidYaml_WhenChecked_ShouldReportSpec001()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw("epics/0001-epic/F1-feature/spec.md", "---\nid: [unclosed\n---\n\n# Specification: F1\n");
        tree.WriteRaw("epics/0001-epic/F1-feature/feature.feature", "Feature: it\n");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC001");
    }

    [Fact]
    public void FrontmatterWithNoClosingDelimiter_WhenChecked_ShouldReportSpec001()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw("epics/0001-epic/F1-feature/spec.md", "---\nid: \"F1\"\n\n# Specification: F1\n");
        tree.WriteRaw("epics/0001-epic/F1-feature/feature.feature", "Feature: it\n");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC001");
    }

    [Fact]
    public void AnItemWhoseStatusIsOutsideItsEnum_WhenChecked_ShouldReportSpec003()
    {
        // Given
        using var tree = new SpecTree();
        var path = tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["children"] = "[\"0001-01\"]" });
        tree.WriteItem(path, "0001-01", "0001-F1", frontmatter: new Dictionary<string, string> { ["status"] = "nearly" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC003" && violation.Identifier == "status");
    }

    [Fact]
    public void AnEpicWhosePriorityIsOutsideItsEnum_WhenChecked_ShouldReportSpec004()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        tree.WriteEpic("0001", new Dictionary<string, string> { ["priority"] = "urgent" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC004" && violation.Identifier == "priority");
    }

    [Fact]
    public void AValidEpic_WhenChecked_ShouldReportNoViolations()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        tree.WriteEpic("0001", new Dictionary<string, string> { ["children"] = "[\"0001-F1\"]" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().BeEmpty();
    }

    [Fact]
    public void ASectionDeclaredTwice_WhenChecked_ShouldReportSpec010()
    {
        // Given
        using var tree = new SpecTree();
        var sections = SpecTree.Sections.Append("## 7. Technical Design\n\nAgain.\n").ToList();
        tree.WriteFeature("0001", "F1", sections: sections);

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC010" && violation.Identifier == "7. Technical Design" && violation.Message.Contains("2 times"));
    }

    [Fact]
    public void SectionsOutOfOrder_WhenChecked_ShouldReportSpec010()
    {
        // Given
        using var tree = new SpecTree();
        var sections = SpecTree.Sections.ToList();
        (sections[5], sections[6]) = (sections[6], sections[5]);
        tree.WriteFeature("0001", "F1", sections: sections);

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC010" && violation.Message.Contains("out of order"));
    }

    [Fact]
    public void ATraceabilityMatrixWithNoTable_WhenChecked_ShouldReportSpec013()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", sections: SpecTree.SectionsWith("9. Traceability Matrix", "## 9. Traceability Matrix\n\nNone yet.\n"));

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC013" && violation.Message.Contains("carries no table"));
    }

    [Fact]
    public void FrontmatterEpicThatDisagreesWithItsDirectory_WhenChecked_ShouldReportSpec011()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["epic"] = "\"0002\"" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC011" && violation.Message.Contains("epic '0002'"));
    }

    [Fact]
    public void TwoCompanionGherkinFiles_WhenChecked_ShouldReportSpec020()
    {
        // Given
        using var tree = new SpecTree();
        var path = tree.WriteFeature("0001", "F1");
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "second.feature"), "Feature: another\n");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC020" && violation.Message.Contains("found 2"));
    }

    [Fact]
    public void AClaimIdThatBreaksTheGrammar_WhenChecked_ShouldReportSpec030()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature(
            "0001",
            "F1",
            sections: SpecTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n| B-1 | Short id. | brd | Active |\n"));

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC030" && violation.Identifier == "B-1");
    }

    [Fact]
    public void AClaimDeclaredTwice_WhenChecked_ShouldReportSpec030()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature(
            "0001",
            "F1",
            sections: SpecTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + "| B-001 | It does the thing. | brd | Active |\n| B-001 | It does it again. | brd | Active |\n"));

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC030" && violation.Identifier == "B-001" && violation.Message.Contains("twice"));
    }

    [Fact]
    public void ATraceabilityRowForAClaimThatDoesNotExist_WhenChecked_ShouldReportSpec031()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature(
            "0001",
            "F1",
            sections: SpecTree.SectionsWith(
                "9. Traceability Matrix",
                "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
                    + "| B-001 | It does the thing | Missing | Missing |\n| B-999 | Ghost | Missing | Missing |\n"));

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC031" && violation.Identifier == "B-999");
    }

    [Fact]
    public void ADeclaredChildThatResolvesToTwoFiles_WhenChecked_ShouldReportSpec040()
    {
        // Given
        using var tree = new SpecTree();
        var path = tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["children"] = "[\"0001-01\"]" });
        tree.WriteItem(path, "0001-01", "0001-F1", slug: "first");
        tree.WriteItem(path, "0001-01", "0001-F1", slug: "second");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC040" && violation.Message.Contains("resolves to 2 files"));
    }

    [Fact]
    public void ADeclaredSpikeWhoseFileIsATask_WhenChecked_ShouldReportSpec041()
    {
        // Given
        using var tree = new SpecTree();
        var path = tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spikes"] = "[\"0001-01\"]" });
        tree.WriteItem(path, "0001-01", "0001-F1", type: "task");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC041" && violation.Identifier == "0001-01");
    }

    [Fact]
    public void ADeclaredSpikeWithNoFile_WhenChecked_ShouldReportSpec041()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spikes"] = "[\"0001-16\"]" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC041" && violation.Identifier == "0001-16");
    }

    [Fact]
    public void ASpikeCitedFromAnotherFeature_WhenChecked_ShouldResolveRepositoryWide()
    {
        // Given
        using var tree = new SpecTree();
        var owner = tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["children"] = "[\"0001-01\"]" });
        tree.WriteItem(owner, "0001-01", "0001-F1", type: "spike");
        tree.WriteFeature("0001", "F2", new Dictionary<string, string> { ["spikes"] = "[\"0001-01\"]" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().BeEmpty();
    }

    [Fact]
    public void AnItemWhoseParentDisagreesWithItsFeature_WhenChecked_ShouldReportSpec043()
    {
        // Given
        using var tree = new SpecTree();
        var path = tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["children"] = "[\"0001-01\"]" });
        tree.WriteItem(path, "0001-01", "0001-F9");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC043" && violation.Message.Contains("parent '0001-F9'"));
    }

    [Fact]
    public void AnItemIdUsedByTwoFeatures_WhenChecked_ShouldReportSpec044()
    {
        // Given
        using var tree = new SpecTree();
        var first = tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["children"] = "[\"0001-01\"]" });
        var second = tree.WriteFeature("0001", "F2", new Dictionary<string, string> { ["children"] = "[\"0001-01\"]" });
        tree.WriteItem(first, "0001-01", "0001-F1");
        tree.WriteItem(second, "0001-01", "0001-F2");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC044" && violation.Message.Contains("already used"));
    }

    [Fact]
    public void AFeatureThatDependsOnItself_WhenChecked_ShouldReportSpec052()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["depends_on"] = "[\"F1\"]" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC052" && violation.Message.Contains("itself"));
    }

    [Fact]
    public void ADependencyCycle_WhenChecked_ShouldReportSpec052()
    {
        // Given - F1 -> F2 -> F3 -> F1, every edge declared from both ends so SPEC051 stays quiet.
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["depends_on"] = "[\"F2\"]", ["blocks"] = "[\"F3\"]" });
        tree.WriteFeature("0001", "F2", new Dictionary<string, string> { ["depends_on"] = "[\"F3\"]", ["blocks"] = "[\"F1\"]" });
        tree.WriteFeature("0001", "F3", new Dictionary<string, string> { ["depends_on"] = "[\"F1\"]", ["blocks"] = "[\"F2\"]" });

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().NotContain(static violation => violation.RuleId == "SPEC051");
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC052" && violation.Message.Contains("cycle"));
    }

    [Fact]
    public void ABlocksEdgeDeclaredFromOnlyOneEnd_WhenChecked_ShouldReportSpec051()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["blocks"] = "[\"F2\"]" });
        tree.WriteFeature("0001", "F2");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation =>
            violation.RuleId == "SPEC051" && violation.Message.Contains("does not declare depends_on"));
    }

    [Fact]
    public void AnApprovedSpecificationWithADraftSignOff_WhenChecked_ShouldReportSpec061()
    {
        // Given - § 9 is fully covered, so SPEC060 stays quiet and only the sign-off disagrees.
        using var tree = new SpecTree();
        tree.WriteFeature(
            "0001",
            "F1",
            new Dictionary<string, string> { ["spec_status"] = "approved" },
            SpecTree.SectionsWith(
                "9. Traceability Matrix",
                "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
                    + "| B-001 | It does the thing | It.Unit.Tests.cs | Done |\n"));

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().NotContain(static violation => violation.RuleId == "SPEC060");
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC061");
    }

    [Fact]
    public void AnApprovedSpecificationFullyCoveredAndSignedOff_WhenChecked_ShouldReportNoViolations()
    {
        // Given
        using var tree = new SpecTree();
        var sections = SpecTree.SectionsWith(
            "9. Traceability Matrix",
            "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
                + "| B-001 | It does the thing | It.Unit.Tests.cs | Done |\n");
        sections = sections
            .Select(static section => section.StartsWith("## 12. Sign-off\n", StringComparison.Ordinal) ? SignedOff : section)
            .ToList();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "approved" }, sections);

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().BeEmpty();
    }

    [Fact]
    public void AClaimIdWrittenAsACodeSpan_WhenChecked_ShouldStillBeAClaim()
    {
        // Given - the id is `B-001` in § 3; a code span is not a literal
        using var tree = new SpecTree();
        var sections = SpecTree.SectionsWith(
            "3. Acceptance Criteria",
            "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n"
                + "| -- | ----- | ------ | ------ |\n"
                + "| `B-001` | It does the thing. | brd | Active |\n");
        tree.WriteFeature("0001", "F1", sections: sections);

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().BeEmpty();
    }

    private const string SignedOff =
        "## 12. Sign-off\n\n| Section | Status | Reviewer | Note |\n| ------- | ------ | -------- | ---- |\n"
        + "| 1-5 | \U0001F7E2 | spec-reviewer | Approved |\n";
}
