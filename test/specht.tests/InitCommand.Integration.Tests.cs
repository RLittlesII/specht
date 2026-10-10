using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using specht.acceptance;
using specht.tool;
using specht.tool.Features.Init;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace specht.tests;

/// <summary>
/// <c>specht init</c> (<c>0001-F4</c> B-006, B-007, B-009, B-011, C-4). Through the built tool: with any one of the eight
/// files already under the root, the run exits <c>0</c> and stdout names each of the eight once, relative to the root with
/// <c>/</c> separators, that one as skipped and every other as written; the scenario holds the feature template alone,
/// this holds every file, over the composition root and the real shipping copy. Through Spectre's command tester over an
/// in-memory file system: with no <c>--root</c>, the writer is handed the working directory, and a root that is not a
/// directory is folded into the missing-input exit code with nothing written. The tester captures stdout alone, so the
/// message on stderr is the acceptance tier's to pin.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class InitCommandIntegrationTests : IDisposable
{
    public static TheoryData<string> EightFiles => new(Files);

    public static TheoryData<string, string[]> RootsThatAreNotDirectories =>
        new()
        {
            { "no root at all", [] },
            { "a root that is a file", ["repo"] },
        };

    [Theory]
    [MemberData(nameof(EightFiles))]
    public void OneOfTheEightFilesPresent_WhenInitRuns_ShouldListItSkippedAndEveryOtherWrittenRelativeToTheRoot(string present)
    {
        // Given
        var root = Path.Combine(_sandbox, "repo");
        var path = Path.Combine([root, .. present.Split('/')]);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.Copy(Path.Combine([AppContext.BaseDirectory, .. present.Split('/')]), path);

        // When
        var (stdout, stderr, exitCode) = Tool.Launch(_sandbox, "init", "--root", "repo");

        // Then
        exitCode.Should().Be(ExitCodes.Success, stdout + stderr);
        var lines = stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var file in Files)
        {
            var (marker, other) = file == present ? ("skipped", "written") : ("written", "skipped");
            lines.Where(line => line.Contains(file, StringComparison.Ordinal))
                .Should()
                .ContainSingle($"stdout lists {file} once; {stdout}{stderr}")
                .Which.Should()
                .Contain(marker)
                .And.NotContain(other);
        }

        stdout.Should().NotContain(_sandbox).And.NotContain("\\");
    }

    [Fact]
    public void NoRootOption_WhenInitRuns_ShouldWriteUnderTheWorkingDirectory()
    {
        // Given
        var workingDirectory = Directory.GetCurrentDirectory();
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(workingDirectory);
        var shippingCopy = new Dictionary<string, byte[]> { ["templates/v1/feature.md"] = "# Shipped\n"u8.ToArray() };
        var services = new ServiceCollection();
        services.AddSingleton(new InitWriter(shippingCopy, fileSystem));
        var app = new CommandAppTester(new TypeRegistrar(services), console: new TestConsole().Width(80));
        app.Configure(static config => config.AddCommand<InitCommand>("init"));

        // When
        var result = app.Run("init");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        fileSystem.File.Exists(Path.Combine(workingDirectory, ".spec", "templates", "feature.md")).Should().BeTrue(result.Output);
    }

    [Theory]
    [MemberData(nameof(RootsThatAreNotDirectories))]
    public void ARootThatIsNotADirectory_WhenInitRuns_ShouldExitWithMissingInputAndWriteNothing(string because, string[] files)
    {
        // Given
        var workingDirectory = Directory.GetCurrentDirectory();
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(workingDirectory);
        foreach (var file in files)
        {
            fileSystem.AddFile(Path.Combine(workingDirectory, file), new MockFileData(string.Empty));
        }

        string[] Paths() => fileSystem.AllPaths.Select(path => Path.GetRelativePath(workingDirectory, path)).ToArray();
        var before = Paths();
        var services = new ServiceCollection();
        services.AddSingleton(new InitWriter(new Dictionary<string, byte[]> { ["templates/v1/feature.md"] = "# Shipped\n"u8.ToArray() }, fileSystem));
        var app = new CommandAppTester(new TypeRegistrar(services), console: new TestConsole().Width(80));
        app.Configure(static config => config.AddCommand<InitCommand>("init"));

        // When
        var result = app.Run("init", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(ExitCodes.MissingInput, because);
        Paths().Should().Equal(before, because);
    }

    public void Dispose()
    {
        if (Directory.Exists(_sandbox))
        {
            Directory.Delete(_sandbox, recursive: true);
        }
    }

    private static readonly string[] Files =
    [
        ".spec/schema/spec-structure.schema.json",
        ".spec/schema/feature-spec.frontmatter.schema.json",
        ".spec/schema/task.frontmatter.schema.json",
        ".spec/schema/epic.frontmatter.schema.json",
        ".spec/templates/feature.md",
        ".spec/templates/decision.md",
        ".spec/templates/adr.md",
        ".spec/templates/lesson.md",
    ];

    private readonly string _sandbox = Path.Combine(Path.GetTempPath(), "specht-init-" + Guid.NewGuid().ToString("N"));
}
