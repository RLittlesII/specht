using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Tests.Shared;

namespace Specht.Tests.Discovery;

/// <summary>
/// The companions beside no specification (<c>0001-F6</c> B-013; item 0441): a file a companion glob matches, in a
/// directory the directory part of a layout glob matches, and beside no specification of any layout, found the same
/// through a canned git listing as by walking, and by walking without entering an excluded directory (C-1). Its git
/// command line is a row of <see cref="GitCommandLines"/>.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtDiscoveryOrphanCompanionsUnitTests
{
    [Theory]
    [InlineData("no .git at the root is outside a work tree, so the tree is walked", false)]
    [InlineData("a .git directory at the root is a work tree, so git lists it", true)]
    public void CompanionsInAndOutsideALayoutsDirectory_WhenOrphanCompanionsAreFoundInEitherMode_ShouldListThoseBesideNoSpecificationOfAnyLayout(
        string because,
        bool workTree)
    {
        // Given
        string[] listed =
        [
            SpechtTree.ManifestPath,
            "epics/0001-e/.spec/README.md",
            "epics/0001-e/.spec/x.feature",
            "epics/0001-e/F1-f/f.feature",
            "epics/0001-e/F1-f/spec.md",
            "epics/0001-e/notes/n.feature",
            "src/a/.spec/a.feature",
            "src/b/.spec/README.md",
            "src/b/.spec/b.feature",
            "test/u/t.feature",
        ];
        string[] excluded = [".spec/root.feature", "src/obj/.spec/o.feature"];
        var files = listed.Concat(excluded).ToDictionary(
            static path => $"{Root}/{path}",
            static path => new MockFileData(path == SpechtTree.ManifestPath ? "{}" : string.Empty));
        if (workTree)
        {
            files[$"{Root}/.git"] = new MockDirectoryData();
        }

        var fileSystem = new MockFileSystem(files);
        var git = new CannedGit(new ChildProcessResult(0, string.Concat(listed.Select(static path => path + "\0"))));
        var inputs = EngineServices.Manifest(fileSystem, Root).Discovery;
        var discovery = new SpechtDiscovery(fileSystem, git);
        var specifications = discovery.FindSpecifications(Root, inputs);
        git.Calls.Clear();

        // When
        var orphans = discovery.FindOrphanCompanions(Root, specifications, inputs);

        // Then
        (git.Calls.Count > 0).Should().Be(workTree, because);
        orphans.Should().Equal(["epics/0001-e/notes/n.feature", "src/a/.spec/a.feature"], because);
    }

    [Fact]
    public void AnExcludedDirectoryInALayoutsDirectoryPart_WhenOrphanCompanionsAreFoundByWalking_ShouldListNoneOfItsCompanionsAndNeverOpenIt()
    {
        // Given
        var fileSystem = new RecordingFileSystem(new Dictionary<string, MockFileData>
        {
            [$"{Root}/{SpechtTree.ManifestPath}"] = new("""{ "exclusions": ["obj"] }"""),
            [$"{Root}/src/a/.spec/a.feature"] = new(string.Empty),
            [$"{Root}/src/obj/.spec/o.feature"] = new(string.Empty),
            [$"{Root}/src/obj/x/.spec/y.feature"] = new(string.Empty),
        });
        var inputs = EngineServices.Manifest(fileSystem, Root).Discovery;
        var discovery = new SpechtDiscovery(fileSystem, new UnexpectedChildProcess());
        fileSystem.Recording.Clear();

        // When
        var orphans = discovery.FindOrphanCompanions(Root, [], inputs);

        // Then
        orphans.Should().Equal("src/a/.spec/a.feature");
        fileSystem.Recording.Should().NotBeEmpty("the tree was walked");
        fileSystem.Recording.Select(listing => Relative(fileSystem, listing.Asked))
            .Should().NotContain(static path => path == "src/obj" || path.StartsWith("src/obj/", StringComparison.Ordinal));
        fileSystem.Recording.SelectMany(static listing => listing.Returned).Select(path => Relative(fileSystem, path))
            .Should().NotContain(static path => path.StartsWith("src/obj/", StringComparison.Ordinal));
    }

    private static string Relative(IFileSystem fileSystem, string path) =>
        fileSystem.Path.GetRelativePath(fileSystem.Path.GetFullPath(Root), fileSystem.Path.GetFullPath(path))
            .Replace(fileSystem.Path.DirectorySeparatorChar, '/') is var relative and not "." ? relative : string.Empty;

    private sealed class CannedGit(ChildProcessResult answer) : IChildProcess
    {
        public List<string> Calls { get; } = [];

        public ChildProcessResult? Run(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
        {
            Calls.Add(fileName);
            return answer;
        }
    }

    private const string Root = "repo";
}
