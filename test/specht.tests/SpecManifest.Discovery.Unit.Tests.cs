using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// The manifest loader's read of the discovery keys over an in-memory file system (<c>0001-F6</c> B-001, B-002, B-003,
/// A-2, C-7; decisions 0004 and 0008): each of <c>layouts</c>, <c>exclusions</c>, <c>taskFiles</c>, <c>epicFiles</c> and
/// <c>companionFiles</c> is read as the manifest writes it, and as the default manifest's value when the manifest leaves
/// it out. The three file-shape keys are lists.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecManifestDiscoveryUnitTests
{
    /// <summary>Gets each file-shape key and the list of one shape the default manifest gives it (B-003).</summary>
    public static TheoryData<string, string[]> DefaultFileShapes { get; } = new()
    {
        { "taskFiles", ["{task}-*.md"] },
        { "epicFiles", ["epics/**/epic.md"] },
        { "companionFiles", ["*.feature"] },
    };

    /// <summary>Gets each file-shape key and a list of shapes a manifest declares for it in place of the default.</summary>
    public static TheoryData<string, string[]> DeclaredFileShapes { get; } = new()
    {
        { "taskFiles", ["{task}-*.markdown", "{task}-*.txt"] },
        { "epicFiles", ["portfolio/*/epic.md", "archive/**/epic.md"] },
        { "companionFiles", ["*.gherkin", "*.story"] },
    };

    [Theory]
    [MemberData(nameof(DefaultFileShapes))]
    public void AManifestLeavingOutAFileShape_WhenLoaded_ShouldReadTheDefaultManifestsShape(string key, string[] shapes)
    {
        // Given
        var fileSystem = Holding("{}");

        // When
        var discovery = SpecManifest.Load(fileSystem, Root).Discovery;

        // Then
        Shape(discovery, key).Should().BeEquivalentTo(shapes);
    }

    [Theory]
    [MemberData(nameof(DeclaredFileShapes))]
    public void AManifestDeclaringAFileShape_WhenLoaded_ShouldReadItAsWritten(string key, string[] shapes)
    {
        // Given
        var fileSystem = Holding(new JsonObject { [key] = new JsonArray([.. shapes]) }.ToJsonString());

        // When
        var discovery = SpecManifest.Load(fileSystem, Root).Discovery;

        // Then
        Shape(discovery, key).Should().BeEquivalentTo(shapes);
    }

    [Fact]
    public void AManifestLeavingOutItsLayouts_WhenLoaded_ShouldReadTheDefaultManifestsTwoInItsOrder()
    {
        // Given
        var fileSystem = Holding("{}");

        // When
        var discovery = SpecManifest.Load(fileSystem, Root).Discovery;

        // Then
        discovery.Layouts.Should().Equal(
            new SpecLayout("epics", "epics/**/spec.md"),
            new SpecLayout("features", "**/.spec/README.md"));
    }

    [Fact]
    public void AManifestDeclaringItsLayouts_WhenLoaded_ShouldReadEachNameAndGlobAsWrittenInTheOrderDeclared()
    {
        // Given
        var fileSystem = Holding(
            """
            {
              "layouts": [
                { "name": "documentation", "glob": "docs/**/specification.md" },
                { "name": "beside-code", "glob": "**/.spec/README.md" }
              ]
            }
            """);

        // When
        var discovery = SpecManifest.Load(fileSystem, Root).Discovery;

        // Then
        discovery.Layouts.Should().Equal(
            new SpecLayout("documentation", "docs/**/specification.md"),
            new SpecLayout("beside-code", "**/.spec/README.md"));
    }

    [Fact]
    public void AManifestLeavingOutItsExclusions_WhenLoaded_ShouldReadTheDefaultManifestsNine()
    {
        // Given
        var fileSystem = Holding("{}");

        // When
        var discovery = SpecManifest.Load(fileSystem, Root).Discovery;

        // Then
        discovery.Exclusions.Should().BeEquivalentTo(
            ".git", ".artifacts", ".skillfile", ".claude", "graphify-out", "bin", "obj", "node_modules", "/.spec");
    }

    [Fact]
    public void AManifestDeclaringItsExclusions_WhenLoaded_ShouldReadTheListItDeclares()
    {
        // Given
        var fileSystem = Holding("""{ "exclusions": ["vendor", "/generated"] }""");

        // When
        var discovery = SpecManifest.Load(fileSystem, Root).Discovery;

        // Then
        discovery.Exclusions.Should().BeEquivalentTo("vendor", "/generated");
    }

    private static IReadOnlyList<string> Shape(SpecDiscoveryInputs discovery, string key) =>
        key switch
        {
            "taskFiles" => discovery.TaskFiles,
            "epicFiles" => discovery.EpicFiles,
            "companionFiles" => discovery.CompanionFiles,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Not a file-shape key."),
        };

    private static MockFileSystem Holding(string manifest) =>
        new(new Dictionary<string, MockFileData>
        {
            [Path.Combine(Root, ".spec", "schema", "spec-structure.schema.json")] = new(manifest),
        });

    private const string Root = "repo";
}
