using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Tests.Shared;

namespace Specht.Tests.Discovery;

/// <summary>
/// Discovery inside a git work tree asks git for the file list and walks otherwise (<c>0001-F6</c> B-005, B-006, B-010, C-1,
/// C-2; item 0008). The file system is a <see cref="MockFileSystem"/> holding a <c>.git</c> where a row puts one, and git is a
/// child process that records each start and answers a canned <c>-z</c> listing, or nothing when it cannot be started.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtDiscoveryGitUnitTests
{
    [Theory]
    [ClassData(typeof(GitWorkTrees))]
    public void ARootAndTheGitEntryAtOrAboveIt_WhenSpecificationsAreDiscovered_ShouldListThroughAStartedGitInAWorkTreeAndWalkOtherwise(
        string because,
        string root,
        string? gitDirectory,
        string? gitFile,
        bool gitStarts,
        int starts,
        string[] found)
    {
        // Given
        var files = new Dictionary<string, MockFileData>
        {
            [$"{root}/{SpechtTree.ManifestPath}"] = new("{}"),
            [$"{root}/src/a/.spec/README.md"] = new(string.Empty),
            [$"{root}/src/b/.spec/README.md"] = new(string.Empty),
        };
        if (gitDirectory is not null)
        {
            files[gitDirectory] = new MockDirectoryData();
        }

        if (gitFile is not null)
        {
            files[gitFile] = new MockFileData("gitdir: elsewhere");
        }

        var fileSystem = new MockFileSystem(files);
        var git = new CannedGit(gitStarts ? new ChildProcessResult(0, "src/a/.spec/README.md\0") : null);
        var inputs = EngineServices.Manifest(fileSystem, root).Discovery;

        // When
        var discovered = new SpechtDiscovery(fileSystem, git).FindSpecifications(root, inputs).Select(static location => location.RelativePath);

        // Then
        git.Calls.Should().HaveCount(starts, because);
        discovered.Should().Equal(found, because);
    }

    [Theory]
    [ClassData(typeof(GitCommandLines))]
    public void AManifestsGlobsAndExclusions_WhenAFinderAsksGit_ShouldRunGitLsFilesInTheRootWithTheirPathspecsAndExcludePatterns(
        string because,
        string root,
        string manifest,
        string finder,
        string[] arguments)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [$"{root}/{SpechtTree.ManifestPath}"] = new(manifest),
            ["repo/.git"] = new MockDirectoryData(),
        });
        var git = new CannedGit(new ChildProcessResult(0, string.Empty));
        var inputs = EngineServices.Manifest(fileSystem, root).Discovery;
        var discovery = new SpechtDiscovery(fileSystem, git);

        // When
        _ = finder switch
        {
            "specifications" => discovery.FindSpecifications(root, inputs).Count,
            "epics" => discovery.FindEpics(root, inputs).Count,
            "test files" => discovery.FindTestFiles(root, ["test/**/*.cs", "**/*.fs"], inputs.Exclusions).Count,
            _ => throw new ArgumentOutOfRangeException(nameof(finder), finder, "Not a finder."),
        };

        // Then
        var call = git.Calls.Should().ContainSingle(because).Subject;
        call.FileName.Should().Be("git", because);
        call.WorkingDirectory.Should().Be(fileSystem.Path.GetFullPath(root), because);
        call.Arguments.Should().Equal(arguments, because);
    }

    [Fact]
    public void GitsNulSeparatedListing_WhenSpecificationsAreDiscovered_ShouldKeepEachExistingFileUnderTheRootOnceInOrdinalOrder()
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [$"{Root}/{SpechtTree.ManifestPath}"] = new("{}"),
            [$"{Root}/.git"] = new MockDirectoryData(),
            [$"{Root}/src/a/.spec/README.md"] = new(string.Empty),
            [$"{Root}/src/b/.spec/README.md"] = new(string.Empty),
            [$"{Root}/src/B/.spec/README.md"] = new(string.Empty),
            [$"{Root}/src/c/.spec/README.md"] = new(string.Empty),
            ["x/.spec/README.md"] = new(string.Empty),
        });
        var git = new CannedGit(new ChildProcessResult(
            0,
            "src/b/.spec/README.md\0src/a/.spec/README.md\0src/a/.spec/README.md\0gone/.spec/README.md\0../x/.spec/README.md\0src/B/.spec/README.md\0"));
        var inputs = EngineServices.Manifest(fileSystem, Root).Discovery;

        // When
        var discovered = new SpechtDiscovery(fileSystem, git).FindSpecifications(Root, inputs).Select(static location => location.RelativePath);

        // Then
        git.Calls.Should().ContainSingle();
        discovered.Should().Equal("src/B/.spec/README.md", "src/a/.spec/README.md", "src/b/.spec/README.md");
    }

    [Theory]
    [InlineData("specifications", "src/area/.spec/README.md")]
    [InlineData("epics", "epics/0001-example/epic.md")]
    [InlineData("test files", "test/a.cs")]
    public void AGitWorkTree_WhenAFinderListsItThroughGit_ShouldOpenNoDirectoryAndFindOnlyWhatGitListed(string finder, string found)
    {
        // Given
        var fileSystem = new RecordingFileSystem(new Dictionary<string, MockFileData>
        {
            [$"{Root}/{SpechtTree.ManifestPath}"] = new("{}"),
            [$"{Root}/.git"] = new MockDirectoryData(),
            [$"{Root}/.gitignore"] = new("dist\n"),
            [$"{Root}/src/area/.spec/README.md"] = new(string.Empty),
            [$"{Root}/dist/one/two/three/.spec/README.md"] = new(string.Empty),
            [$"{Root}/epics/0001-example/epic.md"] = new(string.Empty),
            [$"{Root}/dist/epics/0002-example/epic.md"] = new(string.Empty),
            [$"{Root}/test/a.cs"] = new(string.Empty),
            [$"{Root}/dist/test/b.cs"] = new(string.Empty),
        });
        var git = new CannedGit(new ChildProcessResult(0, ".gitignore\0src/area/.spec/README.md\0epics/0001-example/epic.md\0test/a.cs\0"));
        var inputs = EngineServices.Manifest(fileSystem, Root).Discovery;
        var discovery = new SpechtDiscovery(fileSystem, git);
        fileSystem.Recording.Clear();

        // When
        IEnumerable<string> discovered = finder switch
        {
            "specifications" => discovery.FindSpecifications(Root, inputs).Select(static location => location.RelativePath),
            "epics" => discovery.FindEpics(Root, inputs),
            "test files" => discovery.FindTestFiles(Root, ["**/*.cs"], inputs.Exclusions),
            _ => throw new ArgumentOutOfRangeException(nameof(finder), finder, "Not a finder."),
        };

        // Then
        fileSystem.Recording.Select(static listing => listing.Asked).Should().BeEmpty(finder);
        discovered.Should().Equal([found], finder);
    }

    private sealed record Call(string FileName, IReadOnlyList<string> Arguments, string WorkingDirectory);

    private sealed class CannedGit(ChildProcessResult? answer) : IChildProcess
    {
        public List<Call> Calls { get; } = [];

        public ChildProcessResult? Run(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
        {
            Calls.Add(new Call(fileName, [.. arguments], workingDirectory));
            return answer;
        }
    }

    private const string Root = "repo";
}
