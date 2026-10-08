using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using specht.tool;
using specht.tool.Features.Check;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace specht.tests;

/// <summary>
/// The fold from a report to lines and an exit code (<c>0001-F2</c> B-001, B-003, B-004, B-009), over reports built in
/// memory. The runner is the seam: the engine emits no warning until <c>0001-F5</c>'s rule settings exist, so a
/// warning-only report is reachable only here.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class CheckCommandUnitTests
{
    [Fact]
    public void AReportWithViolations_WhenChecked_ShouldPrintOneDiagnosticLinePerViolationInTheRunnersOrder()
    {
        // Given
        var report = Report(Violation(SpecSeverity.Error, "b/spec.md", 9), Violation(SpecSeverity.Warning, "a/spec.md", 0));

        // When
        var result = Check(report);

        // Then
        Lines(result.Output).Should().Equal("b/spec.md(9): error SPEC031: it is wrong", "a/spec.md: warning SPEC031: it is wrong");
    }

    [Fact]
    public void AViolationWiderThanTheConsole_WhenChecked_ShouldPrintItUnwrappedOnOneLine()
    {
        // Given
        var file = string.Join('/', Enumerable.Repeat("a-long-directory-name", 10)) + "/spec.md";
        var report = Report(Violation(SpecSeverity.Error, file, 12));

        // When
        var result = Check(report);

        // Then
        Lines(result.Output).Should().Equal($"{file}(12): error SPEC031: it is wrong");
    }

    [Fact]
    public void AnErrorViolation_WhenChecked_ShouldExitOne()
    {
        // Given
        var report = Report(Violation(SpecSeverity.Error, "a/spec.md", 3));

        // When
        var result = Check(report);

        // Then
        result.ExitCode.Should().Be(ExitCodes.Violations).And.Be(1);
    }

    [Fact]
    public void NoViolation_WhenChecked_ShouldExitZeroAndPrintNothing()
    {
        // Given
        var report = Report();

        // When
        var result = Check(report);

        // Then
        result.ExitCode.Should().Be(ExitCodes.Success).And.Be(0);
        result.Output.Should().BeEmpty();
    }

    [Fact]
    public void AWarningAlone_WhenCheckedWithoutStrict_ShouldExitZero()
    {
        // Given
        var report = Report(Violation(SpecSeverity.Warning, "a/spec.md", 3));

        // When
        var result = Check(report);

        // Then
        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public void AWarningAlone_WhenCheckedStrict_ShouldExitOne()
    {
        // Given
        var report = Report(Violation(SpecSeverity.Warning, "a/spec.md", 3));

        // When
        var result = Check(report, "--strict");

        // Then
        result.ExitCode.Should().Be(1);
    }

    [Fact]
    public void NoRootOption_WhenChecked_ShouldRunTheWorkingDirectory()
    {
        // Given
        string? checkedRoot = null;

        // When
        Check(root =>
        {
            checkedRoot = root;
            return Report();
        });

        // Then
        checkedRoot.Should().Be(Directory.GetCurrentDirectory());
    }

    [Fact]
    public void ARelativeRootOption_WhenChecked_ShouldRunThatDirectoryResolved()
    {
        // Given
        string? checkedRoot = null;

        // When
        Check(
            root =>
            {
                checkedRoot = root;
                return Report();
            },
            "--root",
            "some/where");

        // Then
        checkedRoot.Should().Be(Path.GetFullPath("some/where"));
    }

    internal static CommandAppResult Check(Func<string, SpecCheckReport> run, params string[] args)
    {
        var services = new ServiceCollection();
        services.AddSingleton(run);

        // 80 columns, what a redirected console gets: wide enough to hide nothing, narrow enough to show a wrapped line.
        var app = new CommandAppTester(new TypeRegistrar(services), console: new TestConsole().Width(80));
        app.SetDefaultCommand<CheckCommand>();

        return app.Run(args);
    }

    internal static string[] Lines(string output) =>
        output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static CommandAppResult Check(SpecCheckReport report, params string[] args) => Check(_ => report, args);

    private static SpecCheckReport Report(params SpecViolation[] violations) => new(0, 0, 0, 0, 0, violations);

    private static SpecViolation Violation(SpecSeverity severity, string file, int line) =>
        new("SPEC031", severity, file, line, null, "it is wrong");
}
