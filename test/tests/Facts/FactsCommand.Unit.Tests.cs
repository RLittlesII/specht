using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Tool.Features.Facts;
using Spectre.Console.Testing;

namespace Specht.Tests.Facts;

/// <summary>
/// The facts command constructed directly over an in-memory file system, without the parser pipeline (<c>0132-F2</c>
/// B-018 to B-021, B-023, B-024): a typed path gives only the specifications it selects, a path selecting none writes
/// nothing on stdout and exits <c>4</c>, the check's read failures fold into the check's exit codes, and a run leaves
/// every file as it was, with <c>--json</c> and without it; without it, each fact's line is written whole and
/// unwrapped, and a selected specification with no fact writes nothing and exits <c>0</c>.
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

    [Fact]
    public async Task APathSelectingSpecifications_WhenExecuted_ShouldWriteOnlyTheSpecificationsItSelects()
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData("""{"schemaVersion":"0.2.0"}"""));
        foreach (var (area, id) in new[] { ("tool", "F1"), ("toolbox", "F2") })
        {
            files.AddFile(files.Path.Combine([root, "src", .. area.Split('/'), ".spec", "README.md"]), new MockFileData(
                $"---\nid: \"{id}\"\nepic: \"0001\"\ntype: feature\nspec_status: draft\nstatus: ready\ndepends_on: []\nblocks: []\nchildren: []\n---\n\n"
                + $"# Specification: {id}\n\n" + string.Join('\n', SpechtTree.Sections)));
        }

        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new FactsCommand(console, EngineServices.Reader(files), errors);

        // When
        var exit = await command.ExecuteAsync(
            null!, new FactsCommand.Settings { Root = root, Json = true, Path = "src/tool" }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0, errors.ToString());
        JsonNode.Parse(console.Output)!["specifications"]!.AsArray().Select(static specification => specification!["path"]!.GetValue<string>())
            .Should().Equal("src/tool/.spec/README.md");
    }

    [Theory]
    [ClassData(typeof(FactsUnselectingPaths))]
    public async Task APathSelectingNoSpecification_WhenExecuted_ShouldWriteNothingOnStdoutAndExitNotFound(string path)
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData("""{"schemaVersion":"0.2.0"}"""));
        foreach (var (area, id) in new[] { ("tool", "F1"), ("tool/Features/Facts", "F2"), ("toolbox", "F3") })
        {
            files.AddFile(files.Path.Combine([root, "src", .. area.Split('/'), ".spec", "README.md"]), new MockFileData(
                $"---\nid: \"{id}\"\nepic: \"0001\"\ntype: feature\nspec_status: draft\nstatus: ready\ndepends_on: []\nblocks: []\nchildren: []\n---\n\n"
                + $"# Specification: {id}\n\n" + string.Join('\n', SpechtTree.Sections)));
        }

        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new FactsCommand(console, EngineServices.Reader(files), errors);

        // When
        var exit = await command.ExecuteAsync(
            null!, new FactsCommand.Settings { Root = root, Json = true, Path = path }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(4, errors.ToString());
        console.Output.Should().BeEmpty();
        errors.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ATreeWithFacts_WhenExecutedWithoutJson_ShouldWriteOneUnwrappedLinePerFactAndExitSuccess()
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData("""{"schemaVersion":"0.2.0"}"""));
        var content = "---\nid: \"F1\"\nepic: \"0001\"\ntype: feature\nspec_status: draft\nstatus: ready\ndepends_on: []\nblocks: []\nchildren: []\n---\n\n"
            + "# Specification: F1\n\n" + string.Join('\n', SpechtTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + "| B-001 | Given a [path] in brackets, this Feature prints one line per fact and never wraps it at the console's width. "
                    + "| brd | Active |\n"));
        files.AddFile(files.Path.Combine(root, "src", "area", ".spec", "README.md"), new MockFileData(content));
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new FactsCommand(console, EngineServices.Reader(files), errors);

        // When
        var exit = await command.ExecuteAsync(null!, new FactsCommand.Settings { Root = root }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0, errors.ToString());
        console.Output.ReplaceLineEndings("\n").Split('\n', StringSplitOptions.RemoveEmptyEntries).Should().Equal(
            $"src/area/.spec/README.md\tclaim\tB-001\t{LineOf(content, "| B-001 |")}\t"
                + "Given a [path] in brackets, this Feature prints one line per fact and never wraps it at the console's width.",
            $"src/area/.spec/README.md\tconstraint\tC-1\t{LineOf(content, "| C-1 |")}\tMust be fast.",
            $"src/area/.spec/README.md\toutOfScope\t1\t{LineOf(content, "| 1 | Other things |")}\tOther things");
    }

    [Fact]
    public async Task APathSelectingSpecifications_WhenExecutedWithoutJson_ShouldWriteOnlyTheLinesOfTheSpecificationsItSelects()
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData("""{"schemaVersion":"0.2.0"}"""));
        var content = string.Empty;
        foreach (var (area, id) in new[] { ("tool", "F1"), ("toolbox", "F2") })
        {
            content = $"---\nid: \"{id}\"\nepic: \"0001\"\ntype: feature\nspec_status: draft\nstatus: ready\ndepends_on: []\nblocks: []\nchildren: []\n---\n\n"
                + $"# Specification: {id}\n\n" + string.Join('\n', SpechtTree.Sections);
            files.AddFile(files.Path.Combine(root, "src", area, ".spec", "README.md"), new MockFileData(content));
        }

        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new FactsCommand(console, EngineServices.Reader(files), errors);

        // When
        var exit = await command.ExecuteAsync(null!, new FactsCommand.Settings { Root = root, Path = "src/tool" }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0, errors.ToString());
        console.Output.ReplaceLineEndings("\n").Split('\n', StringSplitOptions.RemoveEmptyEntries).Should().Equal(
            $"src/tool/.spec/README.md\tclaim\tB-001\t{LineOf(content, "| B-001 |")}\tIt does the thing.",
            $"src/tool/.spec/README.md\tconstraint\tC-1\t{LineOf(content, "| C-1 |")}\tMust be fast.",
            $"src/tool/.spec/README.md\toutOfScope\t1\t{LineOf(content, "| 1 | Other things |")}\tOther things");
    }

    [Fact]
    public async Task APathSelectingASpecificationWithNoFact_WhenExecutedWithoutJson_ShouldWriteNothingOnStdoutAndExitSuccess()
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData("""{"schemaVersion":"0.2.0"}"""));
        files.AddFile(files.Path.Combine(root, "src", "bare", ".spec", "README.md"), new MockFileData(
            "---\nid: \"F1\"\nepic: \"0001\"\ntype: feature\nspec_status: draft\nstatus: ready\ndepends_on: []\nblocks: []\nchildren: []\n---\n\n"
            + "# Specification: F1\n\n" + string.Join('\n', SpechtTree.Sections.Where(static section =>
                !section.StartsWith("## 3. ", StringComparison.Ordinal)
                && !section.StartsWith("## 4. ", StringComparison.Ordinal)
                && !section.StartsWith("## 5. ", StringComparison.Ordinal)))));
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new FactsCommand(console, EngineServices.Reader(files), errors);

        // When
        var exit = await command.ExecuteAsync(null!, new FactsCommand.Settings { Root = root, Path = "src/bare" }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0, errors.ToString());
        console.Output.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(FactsUnselectingPaths))]
    public async Task APathSelectingNoSpecification_WhenExecutedWithoutJson_ShouldWriteNothingOnStdoutAndExitNotFound(string path)
    {
        // Given
        var files = new MockFileSystem();
        var root = files.Path.GetFullPath("repo");
        files.AddFile(files.Path.Combine(root, ".specht", "manifest.json"), new MockFileData("""{"schemaVersion":"0.2.0"}"""));
        foreach (var (area, id) in new[] { ("tool", "F1"), ("tool/Features/Facts", "F2"), ("toolbox", "F3") })
        {
            files.AddFile(files.Path.Combine([root, "src", .. area.Split('/'), ".spec", "README.md"]), new MockFileData(
                $"---\nid: \"{id}\"\nepic: \"0001\"\ntype: feature\nspec_status: draft\nstatus: ready\ndepends_on: []\nblocks: []\nchildren: []\n---\n\n"
                + $"# Specification: {id}\n\n" + string.Join('\n', SpechtTree.Sections)));
        }

        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new FactsCommand(console, EngineServices.Reader(files), errors);

        // When
        var exit = await command.ExecuteAsync(null!, new FactsCommand.Settings { Root = root, Path = path }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(4, errors.ToString());
        console.Output.Should().BeEmpty();
        errors.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [ClassData(typeof(FactsBrokenRoots))]
    public async Task ABrokenRoot_WhenExecutedWithoutJson_ShouldExitWithTheChecksCodeAndWriteNothingOnStdout(bool rootExists, string? manifest, int exitCode)
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
        var exit = await command.ExecuteAsync(null!, new FactsCommand.Settings { Root = root }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(exitCode, errors.ToString());
        console.Output.Should().BeEmpty();
    }

    private static int LineOf(string content, string prefix) =>
        Array.FindIndex(content.Split('\n'), line => line.StartsWith(prefix, StringComparison.Ordinal)) + 1;
}
