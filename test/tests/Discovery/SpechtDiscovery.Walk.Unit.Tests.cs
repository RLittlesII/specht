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
/// as a walk that opens the directory to reject it.
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

    private static string Relative(IFileSystem fileSystem, string path) =>
        fileSystem.Path.GetRelativePath(fileSystem.Path.GetFullPath(Root), fileSystem.Path.GetFullPath(path))
            .Replace(fileSystem.Path.DirectorySeparatorChar, '/') is var relative and not "." ? relative : string.Empty;

    private sealed record Listing(string Asked, IReadOnlyList<string> Returned);

    private sealed class RecordingFileSystem : MockFileSystem
    {
        public RecordingFileSystem(IDictionary<string, MockFileData> files)
            : base(files) =>
            _directory = new RecordingDirectory(this, base.Directory.GetCurrentDirectory());

        public override IDirectory Directory => _directory ?? base.Directory;

        public List<Listing> Recording => _directory?.Recording ?? [];

        private readonly RecordingDirectory? _directory;
    }

    private sealed class RecordingDirectory(IMockFileDataAccessor files, string currentDirectory) : MockDirectory(files, currentDirectory)
    {
        public List<Listing> Recording { get; } = [];

        public override IEnumerable<string> EnumerateFiles(string path) => Record(path, base.EnumerateFiles(path));

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern) =>
            Record(path, base.EnumerateFiles(path, searchPattern));

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) =>
            Record(path, base.EnumerateFiles(path, searchPattern, searchOption));

        public override IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
            Record(path, base.EnumerateFiles(path, searchPattern, enumerationOptions));

        public override IEnumerable<string> EnumerateDirectories(string path) => Record(path, base.EnumerateDirectories(path));

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern) =>
            Record(path, base.EnumerateDirectories(path, searchPattern));

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption) =>
            Record(path, base.EnumerateDirectories(path, searchPattern, searchOption));

        public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
            Record(path, base.EnumerateDirectories(path, searchPattern, enumerationOptions));

        public override IEnumerable<string> EnumerateFileSystemEntries(string path) => Record(path, base.EnumerateFileSystemEntries(path));

        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern) =>
            Record(path, base.EnumerateFileSystemEntries(path, searchPattern));

        public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption) =>
            Record(path, base.EnumerateFileSystemEntries(path, searchPattern, searchOption));

        public override IEnumerable<string> EnumerateFileSystemEntries(
            string path, string searchPattern, EnumerationOptions enumerationOptions) =>
            Record(path, base.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions));

        public override string[] GetFiles(string path) => Record(path, base.GetFiles(path)).ToArray();

        public override string[] GetFiles(string path, string searchPattern) => Record(path, base.GetFiles(path, searchPattern)).ToArray();

        public override string[] GetFiles(string path, string searchPattern, SearchOption searchOption) =>
            Record(path, base.GetFiles(path, searchPattern, searchOption)).ToArray();

        public override string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
            Record(path, base.GetFiles(path, searchPattern, enumerationOptions)).ToArray();

        public override string[] GetDirectories(string path) => Record(path, base.GetDirectories(path)).ToArray();

        public override string[] GetDirectories(string path, string searchPattern) =>
            Record(path, base.GetDirectories(path, searchPattern)).ToArray();

        public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption) =>
            Record(path, base.GetDirectories(path, searchPattern, searchOption)).ToArray();

        public override string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
            Record(path, base.GetDirectories(path, searchPattern, enumerationOptions)).ToArray();

        public override string[] GetFileSystemEntries(string path) => Record(path, base.GetFileSystemEntries(path)).ToArray();

        public override string[] GetFileSystemEntries(string path, string searchPattern) =>
            Record(path, base.GetFileSystemEntries(path, searchPattern)).ToArray();

        public override string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption) =>
            Record(path, base.GetFileSystemEntries(path, searchPattern, searchOption)).ToArray();

        public override string[] GetFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
            Record(path, base.GetFileSystemEntries(path, searchPattern, enumerationOptions)).ToArray();

        private IReadOnlyList<string> Record(string path, IEnumerable<string> listed)
        {
            var returned = listed.ToList();
            Recording.Add(new Listing(path, returned));
            return returned;
        }
    }

    private const string Root = "repo";
}
