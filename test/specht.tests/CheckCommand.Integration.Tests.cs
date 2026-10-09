using System.Text.Json;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using specht.Report;
using specht.tool;
using specht.tool.Features.Check;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace specht.tests;

/// <summary>
/// The command through Spectre's command tester (<c>0001-F2</c> B-001, B-002, B-003, B-004, B-005, B-006, B-007, B-008, B-009, B-013;
/// C-7): over a runner returning a report built in memory, and over the real runner and a synthetic tree on disk. The runner
/// is the seam: the engine emits no warning until <c>0001-F5</c>'s rule settings exist, so a warning-only report is
/// reachable only in memory. The tester captures stdout alone, so an input failure's stderr message is the acceptance
/// tier's to pin.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class CheckCommandIntegrationTests
{
    /// <summary>Gets reports whose lines must come out one per violation, in the runner's order, unwrapped, then the summary.</summary>
    public static TheoryData<string, SpecCheckReport> Reports =>
        new()
        {
            { "no violation prints the summary alone", new SpecCheckReportFixture() },
            {
                "two violations keep the runner's order, and a violation with no line has no position",
                new SpecCheckReportFixture().WithViolations(
                    new SpecViolationFixture().WithFile("b/spec.md").WithLine(9),
                    new SpecViolationFixture().WithSeverity(SpecSeverity.Warning).WithLine(0))
            },
            {
                "a violation wider than the 80-column console is not wrapped",
                new SpecCheckReportFixture().WithViolations(
                    new SpecViolationFixture().WithFile(string.Join('/', Enumerable.Repeat("a-long-directory-name", 10)) + "/spec.md"))
            },
        };

    /// <summary>Gets the severities a report carries, the arguments, and the exit code the claim states for them.</summary>
    public static TheoryData<SpecSeverity[], string[], int> Verdicts =>
        new()
        {
            { [SpecSeverity.Error], [], 1 },
            { [], [], 0 },
            { [SpecSeverity.Warning], [], 0 },
            { [SpecSeverity.Warning], ["--strict"], 1 },
        };

    /// <summary>Gets the arguments, and the root the runner must be handed for them.</summary>
    public static TheoryData<string[], string> Roots =>
        new()
        {
            { [], Directory.GetCurrentDirectory() },
            { ["--root", "some/where"], Path.GetFullPath("some/where") },
        };

    /// <summary>Gets input failures, each arranged on an otherwise clean tree, returning the root to name, and the claimed code.</summary>
    public static TheoryData<string, Func<SpecTree, string>, int> InputFailures =>
        new()
        {
            { "a root that does not exist (B-005)", static tree => Path.Combine(tree.Root, "no-such-root"), 2 },
            { "a root that is a file (B-005)", static tree => tree.WriteRaw("a-file", string.Empty), 2 },
            {
                "a root with no manifest (B-006)",
                static tree =>
                {
                    File.Delete(Path.Combine(tree.Root, SpecManifest.RelativePath));
                    return tree.Root;
                },
                2
            },
            {
                "a manifest that is not well-formed JSON (B-007)",
                static tree =>
                {
                    tree.WriteRaw(SpecManifest.RelativePath, "{ \"sections\": ");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose top level is an array (B-007)",
                static tree =>
                {
                    tree.WriteRaw(SpecManifest.RelativePath, "[]");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose sections are not an array of strings (B-007)",
                static tree =>
                {
                    tree.WriteRaw(SpecManifest.RelativePath, "{ \"sections\": [1, 2] }");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest that is the JSON literal null (B-007)",
                static tree =>
                {
                    tree.WriteRaw(SpecManifest.RelativePath, "null");
                    return tree.Root;
                },
                3
            },
            {
                "a manifest whose sections hold a null (B-007)",
                static tree =>
                {
                    tree.WriteRaw(SpecManifest.RelativePath, "{ \"sections\": [null] }");
                    return tree.Root;
                },
                3
            },
        };

    [Theory]
    [MemberData(nameof(Reports))]
    public void AReport_WhenChecked_ShouldPrintOneUnwrappedLinePerViolationInTheRunnersOrderThenTheSummary(string because, SpecCheckReport report)
    {
        // Given
        var expected = string.Join(
            '\n',
            report.Violations.Select(static violation => violation.ToString()).Concat(SpecReportDocument.From(report).SummaryLines()));

        // When
        var result = Check(_ => report);

        // Then
        result.Output.Should().Be(expected, because);
    }

    [Theory]
    [MemberData(nameof(Verdicts))]
    public void AReportOfSeverities_WhenChecked_ShouldExitWithTheClaimedCode(SpecSeverity[] severities, string[] args, int expected)
    {
        // Given
        SpecCheckReport report = new SpecCheckReportFixture().WithViolations(
            [.. severities.Select(static severity => (SpecViolation)new SpecViolationFixture().WithSeverity(severity))]);

        // When
        var result = Check(_ => report, args);

        // Then
        result.ExitCode.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(Roots))]
    public void ARootOption_WhenChecked_ShouldRunTheRootItResolvesTo(string[] args, string expected)
    {
        // Given
        string? checkedRoot = null;

        // When
        Check(
            root =>
            {
                checkedRoot = root;
                return new SpecCheckReportFixture();
            },
            args);

        // Then
        checkedRoot.Should().Be(expected);
    }

    [Fact]
    public void ATreeWithViolations_WhenChecked_ShouldPrintTheRunnersViolationsAsRootRelativeLines()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });
        tree.WriteFeature("0001", "F2", sections: SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims));
        var report = tree.Run();
        var violations = report.Violations.Select(static violation => violation.ToString()).ToArray();

        // When
        var result = Check(SpecCheckRunner.Run, "--root", tree.Root);

        // Then
        violations.Should().HaveCountGreaterThan(1);
        result.Output.Should().Be(string.Join('\n', violations.Concat(SpecReportDocument.From(report).SummaryLines())));
        result.Output.Should().NotContain(tree.Root).And.NotContain("\\");
        result.ExitCode.Should().Be(1);
    }

    [Fact]
    public void ACleanTree_WhenChecked_ShouldPrintOnlyTheSummaryAndExitZero()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        var report = tree.Run();

        // When
        var result = Check(SpecCheckRunner.Run, "--root", tree.Root);

        // Then
        report.Violations.Should().BeEmpty();
        result.Output.Should().Be(string.Join('\n', SpecReportDocument.From(report).SummaryLines()));
        result.ExitCode.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(Reports))]
    public void AReport_WhenCheckedWithJson_ShouldPrintOnlyItsDocument(string because, SpecCheckReport report)
    {
        // Given
        var expected = SpecReportDocument.From(report).ToJson();

        // When
        var result = Check(_ => report, "--json");

        // Then
        result.Output.Should().Be(expected, because);
        result.Invoking(static parsed => JsonDocument.Parse(parsed.Output).Dispose()).Should().NotThrow(because);
    }

    [Fact]
    public void ATreeWithViolations_WhenCheckedWithJson_ShouldPrintTheDocumentAndNoLine()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });
        tree.WriteFeature("0001", "F2", sections: SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims));
        var report = tree.Run();
        var document = SpecReportDocument.From(report);

        // When
        var result = Check(SpecCheckRunner.Run, "--root", tree.Root, "--json");

        // Then
        report.Violations.Should().HaveCountGreaterThan(1);
        result.Output.Should().Be(document.ToJson());
        foreach (var line in report.Violations.Select(static violation => violation.ToString()).Concat(document.SummaryLines()))
        {
            result.Output.Should().NotContain(line);
        }
    }

    [Theory]
    [MemberData(nameof(InputFailures))]
    public void AnInputFailure_WhenChecked_ShouldPrintNothingOnStdoutAndExitWithTheClaimedCode(
        string because,
        Func<SpecTree, string> arrange,
        int expected)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        var root = arrange(tree);

        // When
        var result = Check(SpecCheckRunner.Run, "--root", root);

        // Then
        result.Output.Should().BeEmpty(because);
        result.ExitCode.Should().Be(expected, because);
    }

    [Fact]
    public void AHelpRequest_WhenChecked_ShouldNameTheRootAndStrictOptionsAndExitZero()
    {
        // Given
        SpecCheckReport report = new SpecCheckReportFixture();

        // When
        var result = Check(_ => report, "--help");

        // Then
        result.Output.Should().Contain("--root").And.Contain("--strict");
        result.ExitCode.Should().Be(0);
    }

    private static CommandAppResult Check(Func<string, SpecCheckReport> run, params string[] args)
    {
        var services = new ServiceCollection();
        services.AddSingleton(run);

        var app = new CommandAppTester(new TypeRegistrar(services), console: new TestConsole().Width(80));
        app.SetDefaultCommand<CheckCommand>();

        return app.Run(args);
    }

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";
}
