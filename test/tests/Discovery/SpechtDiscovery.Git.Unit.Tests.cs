using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Tests.Shared;

namespace Specht.Tests.Discovery;

/// <summary>
/// Discovery inside a git work tree asks git for the file list and walks otherwise (<c>0001-F6</c> B-005, B-006, B-010, C-1,
/// C-2, C-3; item 0008). The file system is a <see cref="MockFileSystem"/> holding a <c>.git</c> where a row puts one, and git is a
/// child process that records each start and answers a canned <c>-z</c> listing, or nothing when it cannot be started.
/// Over one tree both modes give the same specifications, items, epics and companions, the specifications in one ordinal
/// order of root-relative path across every layout, and the child items in one ordinal flat list where one specification
/// sits below another's directory (<c>0001-F6</c> B-007, B-008, decision 0011; item 0009).
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

    [Theory]
    [ClassData(typeof(UnstatedExclusions))]
    public void AnExclusionNoClaimStates_WhenSpecificationsAreDiscoveredInAWorkTree_ShouldNeverStartGit(string because, string exclusion)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [$"{Root}/{SpechtTree.ManifestPath}"] = new($$"""{ "exclusions": ["{{exclusion}}"] }"""),
            [$"{Root}/.git"] = new MockDirectoryData(),
            [$"{Root}/src/a/.spec/README.md"] = new(string.Empty),
        });
        var git = new CannedGit(new ChildProcessResult(0, "src/a/.spec/README.md\0"));
        var inputs = EngineServices.Manifest(fileSystem, Root).Discovery;

        // When
        _ = new SpechtDiscovery(fileSystem, git).FindSpecifications(Root, inputs);

        // Then
        git.Calls.Should().BeEmpty(because);
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

    [Theory]
    [InlineData("no .git at the root is outside a work tree, so the tree is walked", false)]
    [InlineData("a .git directory at the root is a work tree, so git lists it", true)]
    public void SpecificationsInTwoLayoutsWhosePathsInterleave_WhenDiscoveredInEitherMode_ShouldComeOutInOneOrdinalOrderOfRootRelativePath(
        string because,
        bool workTree)
    {
        // Given
        var files = new Dictionary<string, MockFileData>
        {
            [$"{Root}/{SpechtTree.ManifestPath}"] = new("{}"),
            [$"{Root}/a/.spec/README.md"] = new(string.Empty),
            [$"{Root}/epics/0001-e/F1-f/spec.md"] = new(string.Empty),
            [$"{Root}/src/b/.spec/README.md"] = new(string.Empty),
        };
        if (workTree)
        {
            files[$"{Root}/.git"] = new MockDirectoryData();
        }

        var fileSystem = new MockFileSystem(files);
        var git = new CannedGit(new ChildProcessResult(0, "a/.spec/README.md\0epics/0001-e/F1-f/spec.md\0src/b/.spec/README.md\0"));
        var inputs = EngineServices.Manifest(fileSystem, Root).Discovery;

        // When
        var discovered = new SpechtDiscovery(fileSystem, git).FindSpecifications(Root, inputs).Select(static location => location.RelativePath);

        // Then
        git.Calls.Should().HaveCount(workTree ? 1 : 0, because);
        discovered.Should().Equal(["a/.spec/README.md", "epics/0001-e/F1-f/spec.md", "src/b/.spec/README.md"], because);
    }

    [Theory]
    [InlineData("no .git at the root is outside a work tree, so the tree is walked", false)]
    [InlineData("a .git directory at the root is a work tree, so git lists it", true)]
    public void ASpecificationBelowAnothersDirectoryEachWithAnItem_WhenChildItemsAreFoundInEitherMode_ShouldListThemInOneOrdinalOrder(
        string because,
        bool workTree)
    {
        // Given
        string[] tracked =
        [
            SpechtTree.ManifestPath,
            "epics/0001-e/F1-f/0001-01-do.md",
            "epics/0001-e/F1-f/G1-g/0001-02-do.md",
            "epics/0001-e/F1-f/G1-g/spec.md",
            "epics/0001-e/F1-f/spec.md",
        ];
        var files = tracked.ToDictionary(
            static path => $"{Root}/{path}",
            static path => new MockFileData(path == SpechtTree.ManifestPath ? "{}" : string.Empty));
        if (workTree)
        {
            files[$"{Root}/.git"] = new MockDirectoryData();
        }

        var fileSystem = new MockFileSystem(files);
        var git = new CannedGit(new ChildProcessResult(0, string.Concat(tracked.Select(static path => path + "\0"))));
        var structure = EngineServices.Manifest(fileSystem, Root);
        var discovery = new SpechtDiscovery(fileSystem, git);
        var specifications = discovery.FindSpecifications(Root, structure.Discovery);

        // When
        var items = discovery.FindChildItems(Root, specifications, structure.Discovery, structure.Identifiers["task"]);

        // Then
        git.Calls.Should().HaveCount(workTree ? 1 : 0, because);
        specifications.Select(static location => location.RelativePath).Should().Equal(
            ["epics/0001-e/F1-f/G1-g/spec.md", "epics/0001-e/F1-f/spec.md"], because);
        items.Should().Equal(["epics/0001-e/F1-f/0001-01-do.md", "epics/0001-e/F1-f/G1-g/0001-02-do.md"], because);
    }

    [Fact]
    public void AWorkTreeGitListsInFull_WhenDiscoveredThroughGitAndByWalking_ShouldYieldTheSameSpecificationsItemsEpicsAndCompanions()
    {
        // Given
        string[] tracked =
        [
            SpechtTree.ManifestPath,
            "epics/0001-e/F1-f/0001-01-do.md",
            "epics/0001-e/F1-f/spec.md",
            "epics/0001-e/epic.md",
            "src/a/.spec/0001-02-do.md",
            "src/a/.spec/README.md",
            "src/a/.spec/a.feature",
        ];
        var files = tracked.ToDictionary(
            static path => $"{Root}/{path}",
            static path => new MockFileData(path == SpechtTree.ManifestPath ? "{}" : string.Empty));
        files[$"{Root}/.git"] = new MockDirectoryData();
        var fileSystem = new MockFileSystem(files);
        var listing = new CannedGit(
            new ChildProcessResult(0, string.Concat(tracked.Order(StringComparer.Ordinal).Select(static path => path + "\0"))));
        var absent = new CannedGit(null);
        var structure = EngineServices.Manifest(fileSystem, Root);

        // When
        var throughGit = Discover(listing);
        var walked = Discover(absent);

        // Then
        listing.Calls.Should().HaveCount(2);
        absent.Calls.Should().HaveCount(2);
        throughGit.Specifications.Should().NotBeEmpty();
        throughGit.Items.Should().NotBeEmpty();
        throughGit.Epics.Should().NotBeEmpty();
        throughGit.Companions.Should().NotBeEmpty();
        walked.Should().BeEquivalentTo(throughGit);

        ((string Path, string Layout)[] Specifications, string[] Items, string[] Epics, string[] Companions) Discover(IChildProcess git)
        {
            var discovery = new SpechtDiscovery(fileSystem, git);
            var specifications = discovery.FindSpecifications(Root, structure.Discovery);

            return (
                [.. specifications.Select(static location => (location.RelativePath, location.Layout.Name))],
                [.. discovery.FindChildItems(Root, specifications, structure.Discovery, structure.Identifiers["task"])],
                [.. discovery.FindEpics(Root, structure.Discovery)],
                [.. specifications.SelectMany(location => discovery.FindCompanions(Root, location, structure.Discovery))]);
        }
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
