using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// The manifest loader over an in-memory file system (0001-F5 B-012, B-019, B-020; 0001-F2 B-005, B-006, B-007): what it
/// rejects and as which failure, what it ignores, and what it fills from the default manifest.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecManifestUnitTests
{
    public static TheoryData<string, string> DefaultGrammars { get; } = new()
    {
        { "claim", "^B-[0-9]{3}[a-z]?$" },
        { "constraint", "^C-[0-9]+$" },
        { "openQuestion", "^OQ-[0-9]+$" },
        { "task", "^[0-9]{4}-[0-9]{2}$" },
        { "feature", "^F[0-9]+[a-z]?$" },
        { "epic", "^[0-9]{4}$" },
    };

    public static TheoryData<string, MockFileSystem> RootsThatAreNotDirectories =>
        new()
        {
            { "no root at all", new MockFileSystem() },
            { "a root that is a file", new MockFileSystem(new Dictionary<string, MockFileData> { [Root] = new(string.Empty) }) },
        };

    public static TheoryData<string> UnreadableManifests { get; } = new()
    {
        "{ \"sections\": ",
        "[]",
        "{ \"sections\": [1, 2] }",
        "null",
        "{ \"sections\": [null] }",
    };

    [Theory]
    [MemberData(nameof(RootsThatAreNotDirectories))]
    public void ARootThatIsNotADirectory_WhenLoaded_ShouldThrowRootNotFound(string because, MockFileSystem fileSystem)
    {
        // Given
        var root = Root;

        // When
        var load = () => SpecManifest.Load(fileSystem, root);

        // Then
        load.Should().ThrowExactly<SpechtRootNotFoundException>(because);
    }

    [Fact]
    public void ARootWithoutAManifest_WhenLoaded_ShouldThrowManifestNotFoundNamingTheManifestPath()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(Root);

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        load.Should().ThrowExactly<SpechtManifestNotFoundException>().Which.Message.Should().Contain(SpecManifest.RelativePath);
    }

    [Theory]
    [MemberData(nameof(UnreadableManifests))]
    public void AManifestThatDoesNotParseIntoTheManifestShape_WhenLoaded_ShouldThrowUnreadableNamingOnlyTheRelativePath(string text)
    {
        // Given
        var fileSystem = Holding(text);

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        var thrown = load.Should().ThrowExactly<SpechtManifestUnreadableException>().Which;
        thrown.Message.Should().Contain(SpecManifest.RelativePath).And.NotContain(Root + "/");
        thrown.InnerException.Should().NotBeNull();
    }

    [Theory]
    [InlineData("sectons")]
    [InlineData("comment")]
    public void AManifestWithAKeyTheEngineDoesNotKnow_WhenLoaded_ShouldRejectItNamingTheKey(string key)
    {
        // Given
        var manifest = DefaultManifest();
        manifest[key] = "synthetic";
        var fileSystem = Holding(manifest);

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        load.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain(key);
    }

    [Theory]
    [InlineData("$comment")]
    [InlineData("$schema")]
    [InlineData("$id")]
    public void AManifestWithADollarPrefixedKey_WhenLoaded_ShouldReadItAsTheSameManifestWithoutIt(string key)
    {
        // Given
        var annotated = DefaultManifest();
        annotated[key] = "synthetic";
        var plain = DefaultManifest();
        plain.Remove("$comment");

        // When
        var structure = SpecManifest.Load(Holding(annotated), Root);

        // Then
        structure.Should().BeEquivalentTo(SpecManifest.Load(Holding(plain), Root));
    }

    [Theory]
    [MemberData(nameof(DefaultGrammars))]
    public void AManifestLeavingOutAGrammar_WhenLoaded_ShouldReadTheDefaultManifestsGrammar(string name, string grammar)
    {
        // Given
        var manifest = DefaultManifest();
        manifest["identifiers"]!.AsObject().Remove(name);
        var fileSystem = Holding(manifest);

        // When
        var structure = SpecManifest.Load(fileSystem, Root);

        // Then
        structure.Identifiers.Should().ContainKey(name).WhoseValue.Should().Be(grammar);
    }

    [Theory]
    [InlineData("sections")]
    [InlineData("tables")]
    public void AManifestLeavingOutAWholeValue_WhenLoaded_ShouldReadItAsTheDefaultManifests(string key)
    {
        // Given
        var manifest = DefaultManifest();
        manifest.Remove(key);
        var fileSystem = Holding(manifest);

        // When
        var structure = SpecManifest.Load(fileSystem, Root);

        // Then
        structure.Should().BeEquivalentTo(SpecManifest.Load(Holding(DefaultManifest()), Root));
    }

    [Fact]
    public void AManifestDeclaringATableEntry_WhenLoaded_ShouldReadItInPlaceOfTheDefaultsEntry()
    {
        // Given
        var manifest = DefaultManifest();
        manifest["tables"] = new JsonObject { ["9. Traceability Matrix"] = new JsonArray("Claim ID", "Proof") };
        var fileSystem = Holding(manifest);

        // When
        var structure = SpecManifest.Load(fileSystem, Root);

        // Then
        structure.Tables.Should().ContainKey("9. Traceability Matrix").WhoseValue.Should().Equal("Claim ID", "Proof");
    }

    private static MockFileSystem Holding(JsonObject manifest) => Holding(manifest.ToJsonString());

    private static MockFileSystem Holding(string manifest) =>
        new(new Dictionary<string, MockFileData>
        {
            [Path.Combine(Root, ".spec", "schema", "spec-structure.schema.json")] = new(manifest),
        });

    private static JsonObject DefaultManifest() =>
        JsonNode.Parse(
            """
            {
              "$comment": "A synthetic copy of the default manifest's tool-owned keys.",
              "sections": [
                "1. Business Goal",
                "2. User Needs",
                "3. Acceptance Criteria",
                "4. Constraints",
                "5. Out of Scope",
                "6. Concern Separation",
                "7. Technical Design",
                "8. Testing Strategy",
                "9. Traceability Matrix",
                "10. Lessons / Spec Deltas",
                "11. Open Questions",
                "12. Sign-off",
                "Tasks",
                "Scoring"
              ],
              "tables": {
                "9. Traceability Matrix": ["Claim ID", "Scenario", "Test", "Status"]
              },
              "identifiers": {
                "claim": "^B-[0-9]{3}[a-z]?$",
                "constraint": "^C-[0-9]+$",
                "openQuestion": "^OQ-[0-9]+$",
                "task": "^[0-9]{4}-[0-9]{2}$",
                "feature": "^F[0-9]+[a-z]?$",
                "epic": "^[0-9]{4}$"
              }
            }
            """)!.AsObject();

    private const string Root = "repo";
}
