using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Tests.Shared;

namespace Specht.Tests.Discovery;

/// <summary>
/// Discovery outside git walks the tree and declines to enter an excluded directory, so no child of one is enumerated
/// (<c>0001-F6</c> B-006, C-1; item 0007). The file system records every listing it is asked for and every path a listing
/// returns, through every listing overload of its directory, so a full enumeration filtered afterwards fails as surely
/// as a walk that opens the directory to reject it. The same recording shows that the walk never lists a symbolic link to a
/// directory, the link its parent's listing returns (<c>0001-F6</c> B-011; item 0009).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtDiscoveryWalkUnitTests
{
    [Theory]
    [ClassData(typeof(ExcludedDirectories))]
    public void AnExcludedDirectory_WhenDiscoveryWalksTheTree_ShouldListItsParentAndNeitherItNorAnythingBelowIt(
        string finder,
        string exclusion,
        string[] excluded,
        string[] found)
    {
        // Given
        var fileSystem = new RecordingFileSystem(new Dictionary<string, MockFileData>
        {
            [$"{Root}/{SpechtTree.ManifestPath}"] = new($$"""{ "exclusions": ["{{exclusion}}"] }"""),
            [$"{Root}/src/area/.spec/README.md"] = new(string.Empty),
            [$"{Root}/obj/one/two/three/.spec/README.md"] = new(string.Empty),
            [$"{Root}/obj/one/d.cs"] = new(string.Empty),
            [$"{Root}/a/b/x/.spec/README.md"] = new(string.Empty),
            [$"{Root}/a/c/.spec/README.md"] = new(string.Empty),
            [$"{Root}/epics/0001-example/epic.md"] = new(string.Empty),
            [$"{Root}/epics/obj/0002-example/epic.md"] = new(string.Empty),
            [$"{Root}/test/a.cs"] = new(string.Empty),
            [$"{Root}/test/obj/c.cs"] = new(string.Empty),
        });
        var inputs = EngineServices.Manifest(fileSystem, Root).Discovery;
        var discovery = new SpechtDiscovery(fileSystem, new UnexpectedChildProcess());
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
        discovered.Should().BeEquivalentTo(found, finder);
        var asked = fileSystem.Recording.Select(listing => Relative(fileSystem, listing.Asked)).ToList();
        var returned = fileSystem.Recording.SelectMany(static listing => listing.Returned).Select(path => Relative(fileSystem, path)).ToList();
        asked.Should().NotContain(path => excluded.Any(directory => path == directory || path.StartsWith(directory + "/", StringComparison.Ordinal)));
        returned.Should().NotContain(path => excluded.Any(directory => path.StartsWith(directory + "/", StringComparison.Ordinal)));
        asked.Should().Contain(excluded.Select(static directory => directory.Contains('/') ? directory[..directory.LastIndexOf('/')] : string.Empty));
    }

    [Fact]
    public void ALinkToADirectory_WhenDiscoveryWalksTheTree_ShouldListItsParentAndNeverListThroughIt()
    {
        // Given
        var fileSystem = new RecordingFileSystem(new Dictionary<string, MockFileData>
        {
            [$"{Root}/{SpechtTree.ManifestPath}"] = new("{}"),
            [$"{Root}/src/a/.spec/README.md"] = new(string.Empty),
            ["outside/x/.spec/README.md"] = new(string.Empty),
        });
        fileSystem.Directory.CreateSymbolicLink($"{Root}/linked", "../outside");
        var inputs = EngineServices.Manifest(fileSystem, Root).Discovery;
        var discovery = new SpechtDiscovery(fileSystem, new UnexpectedChildProcess());
        fileSystem.Recording.Clear();

        // When
        var discovered = discovery.FindSpecifications(Root, inputs).Select(static location => location.RelativePath);

        // Then
        discovered.Should().Equal("src/a/.spec/README.md");
        fileSystem.Recording.Select(listing => Relative(fileSystem, listing.Asked))
            .Should().NotContain(static path => path == "linked" || path.StartsWith("linked/", StringComparison.Ordinal));
        fileSystem.Recording.Where(listing => Relative(fileSystem, listing.Asked).Length == 0)
            .SelectMany(static listing => listing.Returned)
            .Select(path => Relative(fileSystem, path))
            .Should().Contain("linked");
    }

    private static string Relative(IFileSystem fileSystem, string path) =>
        fileSystem.Path.GetRelativePath(fileSystem.Path.GetFullPath(Root), fileSystem.Path.GetFullPath(path))
            .Replace(fileSystem.Path.DirectorySeparatorChar, '/') is var relative and not "." ? relative : string.Empty;

    private const string Root = "repo";
}
