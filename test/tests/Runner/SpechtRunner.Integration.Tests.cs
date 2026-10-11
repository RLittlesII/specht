using AwesomeAssertions;
using Specht.Discovery;
using Specht.Model;
using Specht.Report;

namespace Specht.Tests.Runner;

/// <summary>
/// Exercises the whole pipeline over a tree on disk: discovery across both
/// layouts, schema loading from <c>.spec/schema/</c>, and the written report.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtRunnerIntegrationTests
{
    [Fact]
    public void ATreeInBothLayouts_WhenChecked_ShouldAcceptBothAndCountTheSpecificationsOfEachLayout()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        tree.WriteFeature("0001", "F2");
        tree.WriteFeature("0002", "F1");
        tree.WriteCoLocatedFeature("src/Backplane/Features/Ingestion", "0003", "F1");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().BeEmpty();
        report.SpecificationCount.Should().Be(4);
        report.Layouts.Should().Equal(new SpecReportLayout("epics", 3), new SpecReportLayout("features", 1));
    }

    [Fact]
    public void OneSpecificationInBothLayoutsAtOnce_WhenChecked_ShouldReportSpec012()
    {
        // Given - a migration that copied the specification without removing
        // the original, which is the failure mode SPEC012 exists to catch.
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        tree.WriteCoLocatedFeature("src/Backplane/Features/Ingestion", "0001", "F1");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(static violation => violation.RuleId == "SPEC012");
        report.Violations.Where(static violation => violation.RuleId == "SPEC012").Should().HaveCount(2);
    }

    [Fact]
    public void ARepositoryWideSpecDirectory_WhenChecked_ShouldNotBeTreatedAsASpecification()
    {
        // Given - the root .spec/ holds adr/, lessons/, templates/ and schema/.
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        File.WriteAllText(Path.Combine(tree.Root, ".spec", "README.md"), "# Records\n");

        // When
        var report = tree.Run();

        // Then
        report.SpecificationCount.Should().Be(1);
        report.Violations.Should().BeEmpty();
    }

    [Fact]
    public void AReportWrittenToDisk_WhenRead_ShouldCarryRelativePathsOnly()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });
        var report = tree.Run();
        var path = Path.Combine(tree.Root, ".artifacts", "spec-check", "spec-check.json");

        // When
        SpechtRunner.WriteReport(report, path);
        var json = File.ReadAllText(path);

        // Then
        json.Should().NotContain(tree.Root);
        json.Should().Contain("epics/0001-epic/F1-feature/spec.md");
        json.Should().Contain("SPEC002");
    }

    [Fact]
    public void AnItemFileBesideAMigratedSpecification_WhenChecked_ShouldResolve()
    {
        // Given
        using var tree = new SpecTree();
        var path = tree.WriteCoLocatedFeature(
            "src/Backplane/Features/Ingestion",
            "0001",
            "F1",
            new Dictionary<string, string> { ["children"] = "[\"0001-01\"]" });
        tree.WriteItem(path, "0001-01", "0001-F1");

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().BeEmpty();
        report.ItemCount.Should().Be(1);
    }

    [Fact]
    public void ATraceabilitySectionHoldingAGridTable_WhenChecked_ShouldReportNoTableAndReadNoRowAsAClaim()
    {
        // Given
        using var tree = new SpecTree();
        var path = SpecDiscovery.Relative(
            tree.Root,
            tree.WriteFeature(
                "0001",
                "F1",
                sections: SpecTree.SectionsWith(
                    "9. Traceability Matrix",
                    "## 9. Traceability Matrix\n\n"
                        + "+----------+-------------------+---------+---------+\n"
                        + "| Claim ID | Scenario          | Test    | Status  |\n"
                        + "+==========+===================+=========+=========+\n"
                        + "| B-001    | It does the thing | Missing | Missing |\n"
                        + "+----------+-------------------+---------+---------+\n"
                        + "| B-999    | A grid-only row   | Missing | Missing |\n"
                        + "+----------+-------------------+---------+---------+\n")));

        // When
        var violations = tree.Run().Violations;

        // Then
        violations.Should().ContainSingle(static violation => violation.RuleId == "SPEC013")
            .Which.Should().Match<SpecViolation>(violation => violation.File == path && violation.Identifier == "9. Traceability Matrix");
        violations.Should().ContainSingle(static violation => violation.RuleId == "SPEC031")
            .Which.Identifier.Should().Be("B-001");
        violations.Should().NotContain(static violation => violation.Identifier == "B-999");
    }
}
