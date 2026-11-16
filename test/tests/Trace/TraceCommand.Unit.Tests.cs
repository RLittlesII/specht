using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Report;
using Specht.Rules.Matrix;
using Specht.Tests.Shared;
using Specht.Tool.Features.Trace;
using Spectre.Console.Testing;

namespace Specht.Tests.Trace;

/// <summary>
/// The trace command constructed directly over an in-memory file system, without the parser pipeline (<c>0001-F13</c>
/// B-006): the check's read failures fold into the check's exit codes, and a tree whose trace shows a gap exits <c>0</c>,
/// with <c>--json</c> and without it; and, with <c>--report</c>, the document written to the path through the store (B-013 to
/// B-015, B-017, B-018), none written on a broken root.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class TraceCommandUnitTests
{
    [Theory]
    [ClassData(typeof(TraceBrokenRoots))]
    public async Task ABrokenRoot_WhenExecuted_ShouldExitWithTheChecksCode(bool rootExists, string? manifest, int exitCode)
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        if (rootExists)
        {
            files.AddDirectory(root);
        }

        if (manifest is not null)
        {
            files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData(manifest));
        }

        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new TraceCommand(console, EngineServices.Reader(files), new MatrixResolver(), new SpechtReportStore(files), errors);

        // When
        var exit = await command.ExecuteAsync(null!, new TraceCommand.Settings { Root = root, Json = true }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(exitCode, errors.ToString());
    }

    [Theory]
    [ClassData(typeof(TraceForms))]
    public async Task ACoveredRowWhoseTestsDoNotResolve_WhenExecuted_ShouldExitSuccess(bool json)
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddFile(
            files.Path.Combine(root, ".specht", "manifest.json"),
            new MockFileData("""{"schemaVersion":"0.2.0","tests":{"files":["test/**/*.cs"]}}"""));
        files.AddFile(files.Path.Combine(root, "test", "TraceTests.cs"), new MockFileData("public sealed class TraceTests { }"));
        files.AddFile(files.Path.Combine(root, "src", "trace", ".spec", "README.md"), new MockFileData(
            "---\nid: \"F1\"\nepic: \"0001\"\ntype: feature\nspec_status: draft\nstatus: ready\ndepends_on: []\nblocks: []\nchildren: []\n---\n\n"
            + "# Specification: F1\n\n" + string.Join('\n', SpechtTree.SectionsWith(
                "9. Traceability Matrix",
                "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
                    + "| B-001 | It does the thing | `TracesNowhere` | Covered |\n"))));
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new TraceCommand(console, EngineServices.Reader(files), new MatrixResolver(), new SpechtReportStore(files), errors);

        // When
        var exit = await command.ExecuteAsync(null!, new TraceCommand.Settings { Root = root, Json = json }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0, errors.ToString());
    }

    [Theory]
    [ClassData(typeof(TraceForms))]
    public async Task AReportPath_WhenExecuted_ShouldWriteTheJsonDocumentThereAndKeepStandardOutputAsWithoutIt(bool json)
    {
        // Given
        var files = new MockFileSystem();
        var root = Tree(files);
        files.AddFile(files.Path.GetFullPath("out/new/trace.json"), new MockFileData("stale"));
        var console = new TestConsole();
        var plain = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);

        // When
        var withReport = await new TraceCommand(console, EngineServices.Reader(files), new MatrixResolver(), new SpechtReportStore(files), errors)
            .ExecuteAsync(
                null!,
                new TraceCommand.Settings { Root = root, Json = json, Report = "out/new/trace.json" },
                TestContext.Current.CancellationToken);
        var without = await new TraceCommand(plain, EngineServices.Reader(files), new MatrixResolver(), new SpechtReportStore(files), errors)
            .ExecuteAsync(null!, new TraceCommand.Settings { Root = root, Json = true }, TestContext.Current.CancellationToken);

        // Then
        withReport.Should().Be(0, errors.ToString());
        without.Should().Be(0, errors.ToString());
        var written = files.File.ReadAllText(files.Path.GetFullPath("out/new/trace.json"));
        written.Should().NotContain("stale").And.Be(plain.Output.TrimEnd('\r', '\n'));
        if (json)
        {
            console.Output.TrimEnd('\r', '\n').Should().Be(written);
        }
    }

    [Theory]
    [ClassData(typeof(TraceBrokenRoots))]
    public async Task ABrokenRootAndAReportPath_WhenExecuted_ShouldExitAsWithoutItAndWriteNothing(
        bool rootExists, string? manifest, int exitCode)
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        if (rootExists)
        {
            files.AddDirectory(root);
        }

        if (manifest is not null)
        {
            files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData(manifest));
        }

        var before = files.AllFiles.Count();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new TraceCommand(new TestConsole(), EngineServices.Reader(files), new MatrixResolver(), new SpechtReportStore(files), errors);

        // When
        var exit = await command.ExecuteAsync(
            null!,
            new TraceCommand.Settings { Root = root, Report = "out/trace.json" },
            TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(exitCode, errors.ToString());
        files.AllFiles.Count().Should().Be(before);
        files.FileExists(files.Path.GetFullPath("out/trace.json")).Should().BeFalse();
    }

    private static string Tree(MockFileSystem files)
    {
        var root = files.Path.GetFullPath("repo");
        files.AddFile(
            files.Path.Combine(root, ".specht", "manifest.json"),
            new MockFileData("""{"schemaVersion":"0.2.0","tests":{"files":["test/**/*.cs"]}}"""));
        files.AddFile(files.Path.Combine(root, "test", "TraceTests.cs"), new MockFileData("public sealed class TraceTests { }"));
        files.AddFile(files.Path.Combine(root, "src", "trace", ".spec", "README.md"), new MockFileData(
            "---\nid: \"F1\"\nepic: \"0001\"\ntype: feature\nspec_status: draft\nstatus: ready\ndepends_on: []\nblocks: []\nchildren: []\n---\n\n"
            + "# Specification: F1\n\n" + string.Join('\n', SpechtTree.SectionsWith(
                "9. Traceability Matrix",
                "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
                    + "| B-001 | It does the thing | `TracesNowhere` | Covered |\n"))));
        return root;
    }
}
