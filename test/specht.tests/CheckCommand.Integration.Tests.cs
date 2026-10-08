using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// The command over the real runner and a synthetic tree on disk (<c>0001-F2</c> B-001, B-003, B-013; C-7).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class CheckCommandIntegrationTests
{
    [Fact]
    public void ATreeWithViolations_WhenChecked_ShouldPrintTheRunnersViolationsAsRootRelativeLines()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });
        tree.WriteFeature("0001", "F2", sections: SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims));
        var expected = tree.Run().Violations.Select(static violation => violation.ToString()).ToArray();

        // When
        var result = CheckCommandUnitTests.Check(SpecCheckRunner.Run, "--root", tree.Root);

        // Then
        expected.Should().HaveCountGreaterThan(1);
        CheckCommandUnitTests.Lines(result.Output).Should().Equal(expected);
        result.Output.Should().NotContain(tree.Root).And.NotContain("\\");
        result.ExitCode.Should().Be(1);
    }

    [Fact]
    public void ACleanTree_WhenChecked_ShouldPrintNothingAndExitZero()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");

        // When
        var result = CheckCommandUnitTests.Check(SpecCheckRunner.Run, "--root", tree.Root);

        // Then
        result.Output.Should().BeEmpty();
        result.ExitCode.Should().Be(0);
    }

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";
}
