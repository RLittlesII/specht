using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Tool.Features.Facts;
using Spectre.Console.Testing;

namespace Specht.Tests.Facts;

/// <summary>
/// The facts command constructed directly over an in-memory file system, without the parser pipeline (<c>0132-F2</c>
/// B-021, B-023): the check's read failures fold into the check's exit codes, and a run leaves every file as it was.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class FactsCommandUnitTests
{
    [Theory]
    [ClassData(typeof(FactsBrokenRoots))]
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
        var command = new FactsCommand(console, EngineServices.Reader(files), errors);

        // When
        var exit = await command.ExecuteAsync(null!, new FactsCommand.Settings { Root = root, Json = true }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(exitCode, errors.ToString());
    }

    [Fact]
    public async Task ATreeWithAFeature_WhenExecuted_ShouldCreateModifyOrDeleteNoFile()
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData("""{"schemaVersion":"0.2.0"}"""));
        files.AddFile(files.Path.Combine(root, "src", "first", ".spec", "README.md"), new MockFileData(
            "---\nid: \"F1\"\nepic: \"0001\"\ntype: feature\nspec_status: draft\nstatus: ready\ndepends_on: []\nblocks: []\nchildren: []\n---\n\n"
            + "# Specification: F1\n\n" + string.Join('\n', SpechtTree.Sections)));
        files.AddFile(files.Path.GetFullPath("outside/sentinel.txt"), new MockFileData("unchanged"));
        var before = files.AllFiles.ToDictionary(static path => path, path => files.File.ReadAllText(path));
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new FactsCommand(console, EngineServices.Reader(files), errors);

        // When
        var exit = await command.ExecuteAsync(null!, new FactsCommand.Settings { Root = root, Json = true }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0, errors.ToString());
        files.AllFiles.ToDictionary(static path => path, path => files.File.ReadAllText(path)).Should().BeEquivalentTo(before);
    }
}
