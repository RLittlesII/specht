using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Manifest;

namespace Specht.Tests.Discovery;

/// <summary>
/// Discovery over the inputs a manifest declares, on an in-memory file system (<c>0001-F6</c> B-001, B-002, B-003, B-009,
/// B-012, C-4, C-6, C-7; decisions 0003, 0004 and 0008): which layouts a specification is found in and which one it carries,
/// how a glob is matched, what an exclusion skips, and which files are items, epics and companions under a list of one
/// entry and of several. Each test's inputs are the ones the loader reads from
/// the manifest the test writes, so a key the manifest leaves out is the default manifest's.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecDiscoveryManifestUnitTests
{
    /// <summary>Gets a manifest, and which of the tree's four markdown files are specifications under it (B-001, C-7).</summary>
    public static TheoryData<string, string, string[]> Layouts { get; } = new()
    {
        {
            "the default manifest declares the epics and the features layout",
            "{}",
            [LegacySpecification, CoLocatedSpecification]
        },
        {
            "a third layout added to the manifest is discovered",
            """
            {
              "layouts": [
                { "name": "epics", "glob": "epics/**/spec.md" },
                { "name": "features", "glob": "**/.spec/README.md" },
                { "name": "documentation", "glob": "docs/**/specification.md" }
              ]
            }
            """,
            [LegacySpecification, CoLocatedSpecification, DocumentationSpecification]
        },
        {
            "a declared list is the whole list, so a default layout it leaves out is not discovered",
            """{ "layouts": [{ "name": "features", "glob": "**/.spec/README.md" }] }""",
            [CoLocatedSpecification]
        },
    };

    /// <summary>Gets a layout's glob, a file's root-relative path, and whether the glob matches it (C-6).</summary>
    public static TheoryData<string, string, string, bool> Globs { get; } = new()
    {
        { "a ** segment matches no directory", "epics/**/spec.md", "epics/spec.md", true },
        { "a ** segment matches several directories", "epics/**/spec.md", "epics/one/two/three/spec.md", true },
        { "a leading ** segment matches no directory", "**/.spec/README.md", ".spec/README.md", true },
        { "a leading ** segment matches several directories", "**/.spec/README.md", "src/one/two/.spec/README.md", true },
        { "a * matches a run of characters within one segment", "docs/*/spec-*.md", "docs/guide/spec-one.md", true },
        { "a * does not cross a /", "docs/*/spec.md", "docs/one/two/spec.md", false },
        { "a literal matches itself", "docs/spec.md", "docs/spec.md", true },
        { "a literal matches nothing else", "docs/spec.md", "docs/specs.md", false },
        { "a . is a literal, not any character", "docs/spec.md", "docs/specxmd", false },
        { "a letter matches in its own case only", "docs/**/Spec.md", "docs/guide/spec.md", false },
        { "a glob is matched from the root, not from any directory", "epics/**/spec.md", "src/epics/one/spec.md", false },
        { "a glob is matched to the end of the path", "docs/*", "docs/guide/spec.md", false },
    };

    /// <summary>Gets a manifest's exclusion list, a specification's root-relative path, and whether it is still discovered (B-002).</summary>
    public static TheoryData<string, string, string, bool> Exclusions { get; } = new()
    {
        { "a bare name excludes that directory at the root", """["generated"]""", "generated/One/.spec/README.md", false },
        { "a bare name excludes that directory at any depth", """["generated"]""", "src/generated/Two/.spec/README.md", false },
        { "a bare name excludes everything below that directory", """["vendor"]""", "src/vendor/one/two/three/.spec/README.md", false },
        { "a bare name is a whole directory name", """["gen"]""", "src/generated/Two/.spec/README.md", true },
        { "a bare name applies to every layout", """["archive"]""", "epics/archive/F1-feature/spec.md", false },
        { "a leading / excludes the path at the root", """["/.spec"]""", ".spec/README.md", false },
        { "a leading / excludes that place only", """["/.spec"]""", "src/Thing/.spec/README.md", true },
        { "a leading / names a path of several segments", """["/src/generated"]""", "src/generated/One/.spec/README.md", false },
        { "a leading / is anchored at the root", """["/generated"]""", "src/generated/Two/.spec/README.md", true },
        { "nothing is excluded by code", "[]", "node_modules/package/.spec/README.md", true },
        { "the root .spec is excluded by the manifest alone", "[]", ".spec/README.md", true },
    };

    /// <summary>Gets a manifest, and which of the files beside the specification are items under it (B-003).</summary>
    public static TheoryData<string, string, string[]> TaskFiles { get; } = new()
    {
        { "the default shape is the default task grammar, then -*.md", "{}", ["0001-01-do.md"] },
        {
            "{task} follows the manifest's task grammar",
            """{ "identifiers": { "task": "^[0-9]{4}-[0-9]{3}$" } }""",
            ["0001-001-do.md"]
        },
        { "{task} follows a task grammar with no epic part", """{ "identifiers": { "task": "^T[0-9]+$" } }""", ["T7-do.md"] },
        { "a declared shape replaces the default", """{ "taskFiles": ["{task}-*.markdown"] }""", ["0001-01-do.markdown"] },
    };

    /// <summary>Gets a manifest, and which of the tree's files are epic files under it (B-002, B-003).</summary>
    public static TheoryData<string, string, string[]> EpicFiles { get; } = new()
    {
        {
            "the default glob is epics/**/epic.md",
            "{}",
            ["epics/0001-example/epic.md", "epics/archive/0003-example/epic.md"]
        },
        { "a declared glob replaces the default", """{ "epicFiles": ["portfolio/*/epic.md"] }""", ["portfolio/0002-example/epic.md"] },
        { "an exclusion applies to an epic file", """{ "exclusions": ["archive"] }""", ["epics/0001-example/epic.md"] },
    };

    /// <summary>Gets a manifest, and which of the files beside the specification are its companions under it (B-003).</summary>
    public static TheoryData<string, string, string[]> CompanionFiles { get; } = new()
    {
        { "the default glob is *.feature", "{}", ["first.feature"] },
        { "a declared glob replaces the default", """{ "companionFiles": ["*.gherkin"] }""", ["second.gherkin"] },
    };

    /// <summary>Gets a file-shape key, a manifest giving it two entries, and the files each entry alone matches (B-012).</summary>
    public static TheoryData<string, string, string[]> ListsOfSeveralEntries { get; } = new()
    {
        {
            "taskFiles",
            """{ "taskFiles": ["{task}-*.md", "{task}-*.markdown"] }""",
            ["src/sample/.spec/0001-01-do.md", "src/sample/.spec/0001-02-do.markdown"]
        },
        {
            "epicFiles",
            """{ "epicFiles": ["epics/**/epic.md", "portfolio/*/epic.md"] }""",
            ["epics/0001-example/epic.md", "portfolio/0002-example/epic.md"]
        },
        {
            "companionFiles",
            """{ "companionFiles": ["*.feature", "*.gherkin"] }""",
            ["src/sample/.spec/first.feature", "src/sample/.spec/second.gherkin"]
        },
    };

    [Theory]
    [MemberData(nameof(Layouts))]
    public void AManifestsLayouts_WhenSpecificationsAreDiscovered_ShouldFindASpecificationInEachLayoutItDeclaresAndInNoOther(
        string because,
        string manifest,
        string[] expected)
    {
        // Given
        var fileSystem = Tree(manifest, LegacySpecification, CoLocatedSpecification, DocumentationSpecification, "src/area/README.md");
        var inputs = SpecManifest.Load(fileSystem, Root).Discovery;

        // When
        var found = SpecDiscovery.FindSpecifications(fileSystem, Root, inputs);

        // Then
        found.Select(static location => location.RelativePath).Should().BeEquivalentTo(expected, because);
    }

    [Fact]
    public void AManifestsLayouts_WhenSpecificationsAreDiscovered_ShouldGiveEachSpecificationTheLayoutWhoseGlobMatchedIt()
    {
        // Given
        var fileSystem = Tree(
            """
            {
              "layouts": [
                { "name": "old-tree", "glob": "epics/**/spec.md" },
                { "name": "beside-code", "glob": "**/.spec/README.md" },
                { "name": "documentation", "glob": "docs/**/specification.md" }
              ]
            }
            """,
            LegacySpecification,
            CoLocatedSpecification,
            DocumentationSpecification);
        var inputs = SpecManifest.Load(fileSystem, Root).Discovery;

        // When
        var found = SpecDiscovery.FindSpecifications(fileSystem, Root, inputs);

        // Then
        found.ToDictionary(static location => location.RelativePath, static location => location.Layout).Should().Equal(
            new Dictionary<string, SpecLayout>
            {
                [LegacySpecification] = new("old-tree", "epics/**/spec.md"),
                [CoLocatedSpecification] = new("beside-code", "**/.spec/README.md"),
                [DocumentationSpecification] = new("documentation", "docs/**/specification.md"),
            });
    }

    [Theory]
    [MemberData(nameof(Globs))]
    public void ALayoutsGlob_WhenSpecificationsAreDiscovered_ShouldMatchARootRelativePathByTheManifestsDialect(
        string because,
        string glob,
        string path,
        bool matches)
    {
        // Given
        var fileSystem = Tree($$"""{ "layouts": [{ "name": "only", "glob": "{{glob}}" }], "exclusions": [] }""", path);
        var inputs = SpecManifest.Load(fileSystem, Root).Discovery;
        string[] expected = matches ? [path] : [];

        // When
        var found = SpecDiscovery.FindSpecifications(fileSystem, Root, inputs);

        // Then
        found.Select(static location => location.RelativePath).Should().Equal(expected, because);
    }

    [Theory]
    [MemberData(nameof(Exclusions))]
    public void AManifestsExclusions_WhenSpecificationsAreDiscovered_ShouldSkipABareNameAtAnyDepthAndALeadingSlashPathAtThatPlace(
        string because,
        string exclusions,
        string path,
        bool discovered)
    {
        // Given
        var fileSystem = Tree($$"""{ "exclusions": {{exclusions}} }""", path);
        var inputs = SpecManifest.Load(fileSystem, Root).Discovery;
        string[] expected = discovered ? [path] : [];

        // When
        var found = SpecDiscovery.FindSpecifications(fileSystem, Root, inputs);

        // Then
        found.Select(static location => location.RelativePath).Should().Equal(expected, because);
    }

    [Theory]
    [MemberData(nameof(TaskFiles))]
    public void AManifestsTaskFileShape_WhenChildItemsAreDiscovered_ShouldFindTheFilesBesideASpecificationWhoseNamesMatchIt(
        string because,
        string manifest,
        string[] expected)
    {
        // Given
        SpecLocation specification = new SpecLocationFixture();
        var fileSystem = Tree(
            manifest,
            Beside("0001-01-do.md"),
            Beside("0001-001-do.md"),
            Beside("0001-01-do.markdown"),
            Beside("0001-03-do.MD"),
            Beside("0001-04.md"),
            Beside("T7-do.md"),
            Beside("notes.md"),
            Beside("nested/0001-02-do.md"));
        var structure = SpecManifest.Load(fileSystem, Root);

        // When
        var found = SpecDiscovery.FindChildItems(fileSystem, [specification], structure.Discovery, structure.Identifiers["task"]);

        // Then
        found.Select(path => Relative(fileSystem, path)).Should().BeEquivalentTo(expected.Select(Beside), because);
    }

    [Theory]
    [MemberData(nameof(EpicFiles))]
    public void AManifestsEpicGlob_WhenEpicsAreDiscovered_ShouldFindTheFilesItMatchesOutsideAnExcludedDirectory(
        string because,
        string manifest,
        string[] expected)
    {
        // Given
        var fileSystem = Tree(
            manifest,
            "epics/0001-example/epic.md",
            "epics/0001-example/notes.md",
            "epics/archive/0003-example/epic.md",
            "portfolio/0002-example/epic.md");
        var inputs = SpecManifest.Load(fileSystem, Root).Discovery;

        // When
        var found = SpecDiscovery.FindEpics(fileSystem, Root, inputs);

        // Then
        found.Select(path => Relative(fileSystem, path)).Should().BeEquivalentTo(expected, because);
    }

    [Theory]
    [MemberData(nameof(CompanionFiles))]
    public void AManifestsCompanionGlob_WhenCompanionsAreDiscovered_ShouldFindTheFilesBesideTheSpecificationWhoseNamesMatchIt(
        string because,
        string manifest,
        string[] expected)
    {
        // Given
        SpecLocation specification = new SpecLocationFixture();
        var fileSystem = Tree(
            manifest,
            Beside("README.md"),
            Beside("first.feature"),
            Beside("second.gherkin"),
            Beside("third.FEATURE"),
            Beside("nested/fourth.feature"),
            Beside("nested/fifth.gherkin"));
        var inputs = SpecManifest.Load(fileSystem, Root).Discovery;

        // When
        var found = SpecDiscovery.FindCompanions(fileSystem, specification, inputs);

        // Then
        found.Select(path => Relative(fileSystem, path)).Should().BeEquivalentTo(expected.Select(Beside), because);
    }

    [Theory]
    [MemberData(nameof(ListsOfSeveralEntries))]
    public void AFileShapeListOfSeveralEntries_WhenItsFilesAreDiscovered_ShouldFindAFileMatchingAnyOneEntry(
        string key,
        string manifest,
        string[] expected)
    {
        // Given
        SpecLocation specification = new SpecLocationFixture();
        var fileSystem = Tree(
            manifest,
            Beside("0001-01-do.md"),
            Beside("0001-02-do.markdown"),
            Beside("0001-03-do.txt"),
            Beside("first.feature"),
            Beside("second.gherkin"),
            Beside("third.story"),
            "epics/0001-example/epic.md",
            "portfolio/0002-example/epic.md",
            "archive/0003-example/epic.md");
        var structure = SpecManifest.Load(fileSystem, Root);

        // When
        var found = key switch
        {
            "taskFiles" => SpecDiscovery.FindChildItems(fileSystem, [specification], structure.Discovery, structure.Identifiers["task"]),
            "epicFiles" => SpecDiscovery.FindEpics(fileSystem, Root, structure.Discovery),
            "companionFiles" => SpecDiscovery.FindCompanions(fileSystem, specification, structure.Discovery),
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Not a file-shape key."),
        };

        // Then
        found.Select(path => Relative(fileSystem, path)).Should().BeEquivalentTo(expected, key);
    }

    private static MockFileSystem Tree(string manifest, params string[] files)
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, ".spec", "schema", "spec-structure.schema.json")] = new(manifest),
            });

        foreach (var file in files)
        {
            fileSystem.AddFile(Path.Combine([Root, .. file.Split('/')]), new MockFileData(string.Empty));
        }

        return fileSystem;
    }

    private static string Beside(string name) => $"src/sample/.spec/{name}";

    private static string Relative(MockFileSystem fileSystem, string path) =>
        fileSystem.Path
            .GetRelativePath(fileSystem.Path.GetFullPath(Root), fileSystem.Path.GetFullPath(path))
            .Replace(fileSystem.Path.DirectorySeparatorChar, '/');

    private const string Root = "repo";

    private const string LegacySpecification = "epics/0001-epic/F1-feature/spec.md";

    private const string CoLocatedSpecification = "src/area/.spec/README.md";

    private const string DocumentationSpecification = "docs/guide/specification.md";
}
