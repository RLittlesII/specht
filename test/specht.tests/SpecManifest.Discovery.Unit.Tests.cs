using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// The manifest loader's read of the discovery keys over an in-memory file system (<c>0001-F6</c> B-002, B-003, A-2;
/// decision 0004): each of <c>exclusions</c>, <c>taskFiles</c>, <c>epicFiles</c> and <c>companionFiles</c> is read as the
/// manifest writes it, and as the default manifest's value when the manifest leaves it out. What <c>layouts</c> is read as
/// is pinned where a layout is used, in <see cref="SpecDiscoveryManifestUnitTests"/>.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecManifestDiscoveryUnitTests
{
    /// <summary>Gets each file-shape key and the shape the default manifest gives it (B-003).</summary>
    public static TheoryData<string, string> DefaultFileShapes { get; } = new()
    {
        { "taskFiles", "{task}-*.md" },
        { "epicFiles", "epics/**/epic.md" },
        { "companionFiles", "*.feature" },
    };

    /// <summary>Gets each file-shape key and a shape a manifest declares for it in place of the default.</summary>
    public static TheoryData<string, string> DeclaredFileShapes { get; } = new()
    {
        { "taskFiles", "{task}-*.markdown" },
        { "epicFiles", "portfolio/*/epic.md" },
        { "companionFiles", "*.gherkin" },
    };

    [Theory]
    [MemberData(nameof(DefaultFileShapes))]
    public void AManifestLeavingOutAFileShape_WhenLoaded_ShouldReadTheDefaultManifestsShape(string key, string shape)
    {
        // Given
        var fileSystem = Holding("{}");

        // When
        var discovery = SpecManifest.Load(fileSystem, Root).Discovery;

        // Then
        Shape(discovery, key).Should().Be(shape);
    }

    [Theory]
    [MemberData(nameof(DeclaredFileShapes))]
    public void AManifestDeclaringAFileShape_WhenLoaded_ShouldReadItAsWritten(string key, string shape)
    {
        // Given
        var fileSystem = Holding($$"""{ "{{key}}": "{{shape}}" }""");

        // When
        var discovery = SpecManifest.Load(fileSystem, Root).Discovery;

        // Then
        Shape(discovery, key).Should().Be(shape);
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

    private static string Shape(SpecDiscoveryInputs discovery, string key) =>
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
