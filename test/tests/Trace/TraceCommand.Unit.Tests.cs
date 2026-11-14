using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Rules.Matrix;
using Specht.Tests.Shared;
using Specht.Tool.Features.Trace;
using Spectre.Console.Testing;

namespace Specht.Tests.Trace;

/// <summary>
/// The trace command constructed directly over an in-memory file system, without the parser pipeline (<c>0001-F13</c>
/// B-006): the check's read failures fold into the check's exit codes, and a tree whose trace shows a gap exits <c>0</c>,
/// with <c>--json</c> and without it.
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
        var command = new TraceCommand(console, EngineServices.Reader(files), new MatrixResolver(), errors);

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
        var command = new TraceCommand(console, EngineServices.Reader(files), new MatrixResolver(), errors);

        // When
        var exit = await command.ExecuteAsync(null!, new TraceCommand.Settings { Root = root, Json = json }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0, errors.ToString());
    }
}
