using System.Diagnostics;
using System.Globalization;
using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// The verdict over a tree on disk, wherever the root is and whatever the process carries (<c>0001-F1</c> B-006, decision
/// 0006): the baseline tree at another root, under another culture and with an extra environment variable gives the golden
/// report; the library <c>src/specht</c> builds, read from disk, references no clock, no machine name, no environment, no
/// working directory, no local time zone and no current culture, save the one site that scopes the invariant culture
/// around the frontmatter schema evaluation, and the same read over an assembly that does reference each finds it, so the
/// guard is not vacuous; and every violation's file is relative to a deeply nested root, with <c>/</c> separators (B-007).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtRunnerDeterminismIntegrationTests
{
    /// <summary>Gets each type or member the engine must not reference.</summary>
    public static TheoryData<string> Forbidden => [.. EngineAssembly.Forbidden];

    [Fact]
    public void TheBaselineTree_WhenCheckedAtANestedRootUnderAnotherCultureWithAnExtraEnvironmentVariable_ShouldGiveTheGoldenReport()
    {
        // Given
        using var tree = new SpecTree();
        using var moved = new SpecTree(["moved", "elsewhere"]);
        BaselineTree.Write(tree);
        BaselineTree.Write(moved);
        var golden = GoldenReport.Read();
        var variable = "SPECHT_B006_" + Guid.NewGuid().ToString("N");
        var culture = CultureInfo.CurrentCulture;
        var uiCulture = CultureInfo.CurrentUICulture;

        // When
        var here = GoldenReport.Of(tree.Run());
        IReadOnlyList<GoldenReport.Verdict> there;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");
            Environment.SetEnvironmentVariable(variable, "set");
            there = GoldenReport.Of(moved.Run());
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = uiCulture;
        }

        // Then
        here.Should().Equal(golden);
        there.Should().Equal(golden);
    }

    [Fact]
    public void AViolationUnderADeeplyNestedRoot_WhenChecked_ShouldCarryItsFileRelativeToTheRootWithForwardSlashes()
    {
        // Given
        using var tree = new SpecTree(["a", "deeply", "nested", "folder", "on", "this", "machine"]);
        tree.WriteFeature(
            "0001",
            "F1",
            sections: SpecTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another. | brd | Active |\n"));

        // When
        var violations = tree.Run().Violations;

        // Then
        var file = violations.Should().ContainSingle().Subject.File;
        file.Should().Be("epics/0001-epic/F1-feature/spec.md").And.NotContain("\\");
        Path.IsPathRooted(file).Should().BeFalse(file);
    }

    [Fact]
    public void TheEngineAssembly_WhenItsReferencesAreRead_ShouldReferenceNoClockMachineNameEnvironmentOrCurrentCultureOutsideTheExemption()
    {
        // Given
        var library = EngineAssembly.Built;

        // When
        var references = EngineAssembly.Unexempted(EngineAssembly.ForbiddenReferencesIn(library));

        // Then
        references.Should().BeEmpty();
    }

    [Fact]
    public void TheEngineAssembly_WhenItsCurrentCultureReferencesAreRead_ShouldFindThemOnlyInTheFrontmatterRulesInvariantEvaluation()
    {
        // Given
        var library = EngineAssembly.Built;

        // When
        var references = EngineAssembly.ForbiddenReferencesIn(library)
            .Where(static reference => reference.Member == EngineAssembly.Exemption.Member);

        // Then
        references.Should().Equal(EngineAssembly.Exemption);
    }

    [Theory]
    [MemberData(nameof(Forbidden))]
    public void AnAssemblyThatReferencesTheMember_WhenItsReferencesAreRead_ShouldFindItOutsideTheExemption(string member)
    {
        // Given
        var assembly = typeof(MachineReader).Assembly.Location;

        // When
        var references = EngineAssembly.Unexempted(EngineAssembly.ForbiddenReferencesIn(assembly));

        // Then
        references.Should().Contain(reference => reference.Member == member);
    }

    /// <summary>References every forbidden type and member, so this test assembly carries each; it is never called.</summary>
    private static class MachineReader
    {
        public static object[] Read() =>
        [
            Environment.MachineName,
            Environment.GetEnvironmentVariable(nameof(Read)) ?? string.Empty,
            Environment.GetEnvironmentVariables(),
            Environment.CurrentDirectory,
            Directory.GetCurrentDirectory(),
            DateTime.Now,
            DateTime.UtcNow,
            DateTime.Today,
            DateTimeOffset.Now,
            DateTimeOffset.UtcNow,
            TimeProvider.System,
            Stopwatch.GetTimestamp(),
            TimeZoneInfo.Local,
            CultureInfo.CurrentCulture,
            CultureInfo.CurrentUICulture,
        ];
    }
}
