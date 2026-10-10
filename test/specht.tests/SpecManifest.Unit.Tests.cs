using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// The manifest loader over an in-memory file system (0001-F5 B-008, B-012, B-019, B-020; 0001-F2 B-005, B-006, B-007): what it
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

    /// <summary>Gets each frontmatter schema kind and the file name the default manifest gives it (decision 0003).</summary>
    public static TheoryData<string, string> DefaultFrontmatterSchemas { get; } = new()
    {
        { "feature", "feature-spec.frontmatter.schema.json" },
        { "task", "task.frontmatter.schema.json" },
        { "epic", "epic.frontmatter.schema.json" },
    };

    /// <summary>Gets each frontmatter schema kind and a file name a manifest declares for it in place of the default.</summary>
    public static TheoryData<string, string> DeclaredFrontmatterSchemas { get; } = new()
    {
        { "feature", "feature.json" },
        { "task", "item.json" },
        { "epic", "epic.json" },
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

    /// <summary>Gets a manifest's <c>schemaVersion</c>, absent when null, and the version the loader must read (<c>0001-F7</c> B-002, B-003).</summary>
    public static TheoryData<string, int?, int> Pins { get; } = new()
    {
        { "a manifest with no schemaVersion is version 1", null, 1 },
        { "a manifest pinning 1", 1, 1 },
        { "a pin the tool does not ship still loads; the version set rejects it", 7, 7 },
    };

    /// <summary>Gets <c>schemaVersion</c> values that are not an integer of at least 1, as JSON.</summary>
    public static TheoryData<string> RejectedPins { get; } = new()
    {
        "0",
        "-1",
        "1.5",
        "\"1\"",
        "true",
        "null",
        "[1]",
    };

    [Theory]
    [MemberData(nameof(Pins))]
    public void AManifestsSchemaVersion_WhenLoaded_ShouldBeTheVersionItPins(string because, int? pinned, int expected)
    {
        // Given
        var manifest = DefaultManifest();
        if (pinned is { } version)
        {
            manifest["schemaVersion"] = version;
        }

        // When
        var structure = SpecManifest.Load(Holding(manifest), Root);

        // Then
        structure.SchemaVersion.Should().Be(expected, because);
    }

    [Theory]
    [MemberData(nameof(RejectedPins))]
    public void AManifestWhoseSchemaVersionIsNotAnIntegerOfAtLeastOne_WhenLoaded_ShouldRejectIt(string pinned)
    {
        // Given
        var fileSystem = Holding($"{{ \"schemaVersion\": {pinned}, {DefaultManifest().ToJsonString()[1..]}");

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        load.Should().Throw<Exception>().Which.Should().Match<Exception>(static thrown =>
            thrown is SpechtManifestException || thrown is SpechtManifestUnreadableException);
    }

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
    [MemberData(nameof(DefaultFrontmatterSchemas))]
    public void AManifestLeavingOutAFrontmatterSchemaKind_WhenLoaded_ShouldReadTheDefaultManifestsFileName(string kind, string name)
    {
        // Given
        var manifest = DefaultManifest();
        var declared = new JsonObject
        {
            ["feature"] = "feature.json",
            ["task"] = "item.json",
            ["epic"] = "epic.json",
        };
        declared.Remove(kind);
        manifest["frontmatterSchemas"] = declared;
        var fileSystem = Holding(manifest);

        // When
        var structure = SpecManifest.Load(fileSystem, Root);

        // Then
        structure.FrontmatterSchemas.Should().ContainKey(kind).WhoseValue.Should().Be(name);
    }

    [Theory]
    [MemberData(nameof(DeclaredFrontmatterSchemas))]
    public void AManifestDeclaringAFrontmatterSchemaFileName_WhenLoaded_ShouldReadItAsWritten(string kind, string name)
    {
        // Given
        var manifest = DefaultManifest();
        manifest["frontmatterSchemas"] = new JsonObject
        {
            ["feature"] = "feature.json",
            ["task"] = "item.json",
            ["epic"] = "epic.json",
        };
        var fileSystem = Holding(manifest);

        // When
        var structure = SpecManifest.Load(fileSystem, Root);

        // Then
        structure.FrontmatterSchemas.Should().ContainKey(kind).WhoseValue.Should().Be(name);
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
              },
              "frontmatterSchemas": {
                "feature": "feature-spec.frontmatter.schema.json",
                "task": "task.frontmatter.schema.json",
                "epic": "epic.frontmatter.schema.json"
              }
            }
            """)!.AsObject();

    private const string Root = "repo";
}
