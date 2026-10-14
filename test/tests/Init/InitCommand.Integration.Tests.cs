using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Acceptance;
using Specht.Composition;
using Specht.Tests.Shared;
using Specht.Tool;
using Specht.Tool.Features.Init;
using Specht.Versioning;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Specht.Tests.Init;

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
    public static TheoryData<string> SelectedFiles => new(Files);

    public static TheoryData<string[], string[]> Selections =>
        new()
        {
            { [], [".specht/manifest.json"] },
            {
                ["--schemas"],
                [".specht/manifest.json", ".specht/schema/frontmatter/feature.schema.json", ".specht/schema/frontmatter/task.schema.json"]
            },
            {
                ["--templates"],
                [
                    ".specht/manifest.json",
                    ".specht/templates/adr.md",
                    ".specht/templates/decision.md",
                    ".specht/templates/feature.md",
                    ".specht/templates/lesson.md",
                ]
            },
            {
                ["--all"],
                [
                    ".specht/manifest.json",
                    ".specht/schema/frontmatter/feature.schema.json",
                    ".specht/schema/frontmatter/task.schema.json",
                    ".specht/templates/adr.md",
                    ".specht/templates/decision.md",
                    ".specht/templates/feature.md",
                    ".specht/templates/lesson.md",
                ]
            },
            {
                ["--schemas", "--templates"],
                [
                    ".specht/manifest.json",
                    ".specht/schema/frontmatter/feature.schema.json",
                    ".specht/schema/frontmatter/task.schema.json",
                    ".specht/templates/adr.md",
                    ".specht/templates/decision.md",
                    ".specht/templates/feature.md",
                    ".specht/templates/lesson.md",
                ]
            },
        };

    public static TheoryData<string, string[]> RootsThatAreNotDirectories =>
        new()
        {
            { "no root at all", [] },
            { "a root that is a file", ["repo"] },
        };

    [Theory]
    [MemberData(nameof(SelectedFiles))]
    public void OneSelectedFilePresent_WhenInitRunsWithEveryPart_ShouldListItSkippedAndEveryOtherSelectedFileWritten(string present)
    {
        // Given
        var root = Path.Combine(_sandbox, "repo");
        var path = Path.Combine([root, .. present.Split('/')]);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, present.EndsWith(".json", StringComparison.Ordinal) ? "{}\n" : "# A synthetic template with local edits\n");

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Launch(_sandbox, "init", "--root", "repo", "--all");

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

    [Theory]
    [MemberData(nameof(Selections))]
    public void APartSelection_WhenInitRuns_ShouldWriteAndListExactlyTheSelection(string[] options, string[] expected)
    {
        // Given
        var root = Path.Combine(_sandbox, "repo");
        Directory.CreateDirectory(root);
        var arguments = new[] { "init", "--root", "repo" }.Concat(options).ToArray();

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Launch(_sandbox, arguments);

        // Then
        exitCode.Should().Be(ExitCodes.Success, stdout + stderr);
        var files = Directory.EnumerateFiles(Path.Combine(root, ".specht"), "*", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/'))
            .Order(StringComparer.Ordinal);
        files.Should().Equal(expected);
        stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Should()
            .Equal(expected.Select(static file => $"written {file}"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("--schemas")]
    [InlineData("--templates")]
    [InlineData("--all")]
    public void AnUnshippedPin_WhenInitRuns_ShouldExitThreeWithoutWriting(string? option)
    {
        // Given
        var root = Path.Combine(_sandbox, "repo");
        var manifestPath = Path.Combine(root, SpecTree.ManifestPath);
        const string manifest = "{\r\n  \"schemaVersion\": \"7.0.0\"\r\n}\r\n";
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        File.WriteAllText(manifestPath, manifest);

        // When
        var (stdout, stderr, code) = AcceptanceTool.Launch(_sandbox, ["init", "--root", "repo", .. option is null ? Array.Empty<string>() : [option]]);

        // Then
        code.Should().Be(ExitCodes.InvalidManifest, stdout + stderr);
        stdout.Should().BeEmpty();
        stderr.Should().Contain("schemaVersion 7.0.0 is not shipped").And.Contain(".specht/manifest.json").And.NotContain(_sandbox);
        foreach (var version in EngineServices.Versions().Versions)
        {
            stderr.Should().Contain(version.Number.ToString());
        }

        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Should().ContainSingle();
        File.ReadAllText(manifestPath).Should().Be(manifest);
    }

    [Fact]
    public void ABareRoot_WhenInitRuns_ShouldPinTheNewestEmbeddedSchemaVersion()
    {
        // Given
        Directory.CreateDirectory(_sandbox);
        var newest = EngineServices.Versions().Versions.Max(static version => version.Number);

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Launch(_sandbox, "init", "--root", ".");

        // Then
        exitCode.Should().Be(ExitCodes.Success, stdout + stderr);
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(_sandbox, SpecTree.ManifestPath)));
        manifest.RootElement.GetProperty("schemaVersion").GetString().Should().Be(newest.ToString());
    }

    [Fact]
    public void NoRootOption_WhenInitRuns_ShouldWriteUnderTheWorkingDirectory()
    {
        // Given
        var workingDirectory = Directory.GetCurrentDirectory();
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(workingDirectory);
        var services = new ServiceCollection();
        services.AddSingleton<IFileSystem>(fileSystem);
        services.AddSpechtEngine().AddInit();
        var app = new CommandAppTester(new TypeRegistrar(services), console: new TestConsole().Width(80));
        app.Configure(static config => config.AddInit());

        // When
        var result = app.Run("init");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        fileSystem.File.Exists(Path.Combine(workingDirectory, SpecTree.ManifestPath)).Should().BeTrue(result.Output);
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
        services.AddSingleton<IFileSystem>(fileSystem);
        services.AddSpechtEngine().AddInit();
        var app = new CommandAppTester(new TypeRegistrar(services), console: new TestConsole().Width(80));
        app.Configure(static config => config.AddInit());

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
        ".specht/manifest.json",
        ".specht/schema/frontmatter/feature.schema.json",
        ".specht/schema/frontmatter/task.schema.json",
        ".specht/templates/feature.md",
        ".specht/templates/decision.md",
        ".specht/templates/adr.md",
        ".specht/templates/lesson.md",
    ];

    private readonly string _sandbox = Path.Combine(Path.GetTempPath(), "specht-init-" + Guid.NewGuid().ToString("N"));
}
