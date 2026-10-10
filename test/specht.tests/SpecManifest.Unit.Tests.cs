using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// The manifest loader over an in-memory file system (0001-F5 B-001, B-002, B-003, B-008, B-012, B-015, B-019, B-020, B-021,
/// B-039, B-040, B-041; 0001-F2 B-005, B-006, B-007; 0001-F7 B-002, B-039): what it rejects and as which failure, what it
/// ignores, the schema version it reads, and what it fills from the default manifest.
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

    /// <summary>Gets each section role and the title the default manifest gives it (decision 0006).</summary>
    public static TheoryData<string, string> DefaultRoles { get; } = new()
    {
        { "claims", "3. Acceptance Criteria" },
        { "matrix", "9. Traceability Matrix" },
        { "signOff", "12. Sign-off" },
    };

    /// <summary>Gets each section role and a title from <c>sections</c> a manifest declares for it in place of the default.</summary>
    public static TheoryData<string, string> DeclaredRoles { get; } = new()
    {
        { "claims", "4. Constraints" },
        { "matrix", "5. Out of Scope" },
        { "signOff", "6. Concern Separation" },
    };

    /// <summary>Gets each marker and the text the default manifest gives it (decision 0006).</summary>
    public static TheoryData<string, string> DefaultMarkers { get; } = new()
    {
        { "missing", "Missing" },
        { "draft", "\U0001F7E1" },
        { "blocked", "\U0001F534" },
        { "approved", "approved" },
    };

    /// <summary>Gets each marker and a text a manifest declares for it in place of the default.</summary>
    public static TheoryData<string, string> DeclaredMarkers { get; } = new()
    {
        { "missing", "TBD" },
        { "draft", "WIP" },
        { "blocked", "HELD" },
        { "approved", "agreed" },
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

    /// <summary>
    /// Gets a manifest's <c>schemaVersion</c>, absent when null, and the version the loader must read (<c>0001-F7</c>
    /// B-001, B-002, B-003).
    /// </summary>
    public static TheoryData<string, string?, SemanticVersion> Pins { get; } = new()
    {
        { "a manifest with no schemaVersion is version 0.1.0", null, new SemanticVersion(0, 1, 0) },
        { "a manifest pinning 0.1.0", "0.1.0", new SemanticVersion(0, 1, 0) },
        { "a manifest pinning another version", "1.2.3", new SemanticVersion(1, 2, 3) },
        { "a pin the tool does not ship still loads; the version set rejects it", "7.0.0", new SemanticVersion(7, 0, 0) },
    };

    /// <summary>
    /// Gets <c>schemaVersion</c> values that are not a <c>major.minor.patch</c> string, as the manifest's JSON writes them,
    /// and the text the rejection names (<c>0001-F7</c> B-039).
    /// </summary>
    public static TheoryData<string, string, string> RejectedPins { get; } = new()
    {
        { "the integer the retired pin was", "1", "1" },
        { "a string of one number", "\"1\"", "1" },
        { "a string of two numbers", "\"0.1\"", "0.1" },
        { "a string with a prefix", "\"v0.1.0\"", "v0.1.0" },
        { "a boolean", "true", "true" },
        { "a null", "null", "null" },
        { "an array", "[1]", "[1]" },
    };

    [Theory]
    [MemberData(nameof(Pins))]
    public void AManifestsSchemaVersion_WhenLoaded_ShouldBeTheVersionItPins(string because, string? pinned, SemanticVersion expected)
    {
        // Given
        var manifest = DefaultManifest();
        if (pinned is not null)
        {
            manifest["schemaVersion"] = pinned;
        }

        // When
        var structure = SpecManifest.Load(Holding(manifest), Root);

        // Then
        structure.SchemaVersion.Should().Be(expected, because);
    }

    [Theory]
    [MemberData(nameof(RejectedPins))]
    public void AManifestWhoseSchemaVersionIsNotMajorMinorPatch_WhenLoaded_ShouldRejectItNamingTheValue(
        string because,
        string pinned,
        string named)
    {
        // Given
        var fileSystem = Holding($"{{ \"schemaVersion\": {pinned}, {DefaultManifest().ToJsonString()[1..]}");

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        load.Should().ThrowExactly<SpechtManifestException>(because)
            .Which.Message.Should().MatchRegex($@"(?<![\w.]){Regex.Escape(named)}(?!\.?\w)", because);
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
        manifest["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Proof") };
        var fileSystem = Holding(manifest);

        // When
        var structure = SpecManifest.Load(fileSystem, Root);

        // Then
        structure.Tables.Should().ContainKey("matrix").WhoseValue.Should().Equal("Claim ID", "Proof");
    }

    [Theory]
    [MemberData(nameof(DefaultRoles))]
    public void AManifestLeavingOutARole_WhenLoaded_ShouldReadTheDefaultManifestsTitle(string role, string title)
    {
        // Given
        var manifest = DefaultManifest();
        var declared = new JsonObject
        {
            ["claims"] = "4. Constraints",
            ["matrix"] = "5. Out of Scope",
            ["signOff"] = "6. Concern Separation",
        };
        declared.Remove(role);
        manifest["roles"] = declared;
        var fileSystem = Holding(manifest);

        // When
        var structure = SpecManifest.Load(fileSystem, Root);

        // Then
        structure.Roles.Should().ContainKey(role).WhoseValue.Should().Be(title);
    }

    [Theory]
    [MemberData(nameof(DeclaredRoles))]
    public void AManifestDeclaringARole_WhenLoaded_ShouldReadItsTitleAsWritten(string role, string title)
    {
        // Given
        var manifest = DefaultManifest();
        manifest["roles"] = new JsonObject
        {
            ["claims"] = "4. Constraints",
            ["matrix"] = "5. Out of Scope",
            ["signOff"] = "6. Concern Separation",
        };
        var fileSystem = Holding(manifest);

        // When
        var structure = SpecManifest.Load(fileSystem, Root);

        // Then
        structure.Roles.Should().ContainKey(role).WhoseValue.Should().Be(title);
    }

    [Theory]
    [MemberData(nameof(DefaultMarkers))]
    public void AManifestLeavingOutAMarker_WhenLoaded_ShouldReadTheDefaultManifestsText(string marker, string text)
    {
        // Given
        var manifest = DefaultManifest();
        var declared = new JsonObject
        {
            ["missing"] = "TBD",
            ["draft"] = "WIP",
            ["blocked"] = "HELD",
            ["approved"] = "agreed",
        };
        declared.Remove(marker);
        manifest["markers"] = declared;
        var fileSystem = Holding(manifest);

        // When
        var structure = SpecManifest.Load(fileSystem, Root);

        // Then
        structure.Markers.Should().ContainKey(marker).WhoseValue.Should().Be(text);
    }

    [Theory]
    [MemberData(nameof(DeclaredMarkers))]
    public void AManifestDeclaringAMarker_WhenLoaded_ShouldReadItsTextAsWritten(string marker, string text)
    {
        // Given
        var manifest = DefaultManifest();
        manifest["markers"] = new JsonObject
        {
            ["missing"] = "TBD",
            ["draft"] = "WIP",
            ["blocked"] = "HELD",
            ["approved"] = "agreed",
        };
        var fileSystem = Holding(manifest);

        // When
        var structure = SpecManifest.Load(fileSystem, Root);

        // Then
        structure.Markers.Should().ContainKey(marker).WhoseValue.Should().Be(text);
    }

    [Theory]
    [InlineData("claims")]
    [InlineData("matrix")]
    [InlineData("signOff")]
    public void AManifestWhoseRoleNamesATitleSectionsLacks_WhenLoaded_ShouldRejectItNamingTheRole(string role)
    {
        // Given
        var manifest = DefaultManifest();
        manifest["roles"] = new JsonObject { [role] = "3. Nowhere" };
        var fileSystem = Holding(manifest);

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        load.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain($"'{role}'");
    }

    [Fact]
    public void AManifestRenamingTheClaimsSectionAndLeavingItsRoleOut_WhenLoaded_ShouldRejectItNamingTheRole()
    {
        // Given
        var manifest = DefaultManifest();
        manifest["sections"]!.AsArray()[2] = "3. Claims";
        var fileSystem = Holding(manifest);

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        load.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain("'claims'");
    }

    [Theory]
    [InlineData("9. Traceability Matrix")]
    [InlineData("Glossary")]
    public void AManifestWhoseTablesKeyIsNotARole_WhenLoaded_ShouldRejectItNamingTheKey(string key)
    {
        // Given
        var manifest = DefaultManifest();
        manifest["tables"] = new JsonObject { [key] = new JsonArray("Claim ID", "Scenario", "Test", "Status") };
        var fileSystem = Holding(manifest);

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        load.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain($"'{key}'");
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("draft")]
    [InlineData("blocked")]
    [InlineData("approved")]
    public void AManifestWhoseMarkerTextIsEmpty_WhenLoaded_ShouldRejectItNamingTheMarker(string marker)
    {
        // Given
        var manifest = DefaultManifest();
        manifest["markers"] = new JsonObject { [marker] = string.Empty };
        var fileSystem = Holding(manifest);

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        load.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain($"'{marker}'");
    }

    [Theory]
    [InlineData("taskFiles")]
    [InlineData("epicFiles")]
    [InlineData("companionFiles")]
    public void AManifestWhoseFileShapeListIsEmpty_WhenLoaded_ShouldRejectItNamingTheKey(string key)
    {
        // Given
        var manifest = DefaultManifest();
        manifest[key] = new JsonArray();
        var fileSystem = Holding(manifest);

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        load.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain(key);
    }

    [Theory]
    [InlineData("build/output")]
    [InlineData("a/b/c")]
    [InlineData("build/")]
    public void AManifestWhoseExclusionEntryHasASlashInsideItAndNoLeadingSlash_WhenLoaded_ShouldRejectItNamingTheEntry(string entry)
    {
        // Given
        var manifest = DefaultManifest();
        manifest["exclusions"] = new JsonArray(entry);
        var fileSystem = Holding(manifest);

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        load.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain($"'{entry}'");
    }

    [Theory]
    [InlineData("bin")]
    [InlineData("/.spec")]
    [InlineData("/docs/generated")]
    public void AManifestWhoseExclusionEntryIsANameOrBeginsWithASlash_WhenLoaded_ShouldAcceptIt(string entry)
    {
        // Given
        var manifest = DefaultManifest();
        manifest["exclusions"] = new JsonArray(entry);
        var fileSystem = Holding(manifest);

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        load.Should().NotThrow();
    }

    [Fact]
    public void AManifestWithTwoMalformedExclusionEntries_WhenLoaded_ShouldRejectItOnceNamingBothInTheManifestsOrder()
    {
        // Given
        string[] faults = ["zeta/output", "alpha/output"];
        var manifest = DefaultManifest();
        manifest["exclusions"] = new JsonArray("zeta/output", "bin", "alpha/output");
        var fileSystem = Holding(manifest);

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        var message = load.Should().ThrowExactly<SpechtManifestException>().Which.Message;
        var positions = faults.Select(fault => message.IndexOf($"'{fault}'", StringComparison.Ordinal)).ToList();
        positions.Should().NotContain(-1, message).And.BeInAscendingOrder(message);
    }

    [Fact]
    public void AManifestWithARoleTwoTablesKeysAndAMarkerAtFault_WhenLoaded_ShouldRejectItOnceNamingTheRoleThenTheKeysInTheManifestsOrderThenTheMarker()
    {
        // Given
        string[] faults = ["claims", "Zeta", "Alpha", "draft"];
        var manifest = DefaultManifest();
        manifest["roles"] = new JsonObject { ["claims"] = "3. Nowhere" };
        manifest["tables"] = new JsonObject { ["Zeta"] = new JsonArray("Claim ID"), ["Alpha"] = new JsonArray("Claim ID") };
        manifest["markers"] = new JsonObject { ["draft"] = string.Empty };
        var fileSystem = Holding(manifest);

        // When
        var load = () => SpecManifest.Load(fileSystem, Root);

        // Then
        var message = load.Should().ThrowExactly<SpechtManifestException>().Which.Message;
        var positions = faults.Select(fault => message.IndexOf($"'{fault}'", StringComparison.Ordinal)).ToList();
        positions.Should().NotContain(-1, message).And.BeInAscendingOrder(message);
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
                "matrix": ["Claim ID", "Scenario", "Test", "Status"]
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
