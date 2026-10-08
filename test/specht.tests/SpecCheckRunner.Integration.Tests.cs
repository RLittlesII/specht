using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// Exercises the whole pipeline over a tree on disk: discovery across both
/// layouts, schema loading from <c>.spec/schema/</c>, and the written report.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpecCheckRunnerIntegrationTests
{
    [Fact]
    public void ATreeInBothLayouts_WhenChecked_ShouldAcceptBothAndReportMigrationProgress()
    {
        // Given - the state this repository is in while the migration runs.
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
        report.LegacyCount.Should().Be(3);
        report.CoLocatedCount.Should().Be(1);
        report.MigrationSummary.Should().Contain("Migration 25% complete");
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
        SpecCheckRunner.WriteReport(report, path);
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
}
