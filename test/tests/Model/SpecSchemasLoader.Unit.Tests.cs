using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Model;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Model;

/// <summary>
/// The manifest stage as an instance over an injected file system (ADR-0005 (a); ADR-0001 stage D, item 0107): the
/// structure is the one the manifest in that file system declares, and the version kept is the one its pin selects from
/// the set handed in. Nothing touches the disk.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecSchemasLoaderUnitTests
{
    [Fact]
    public void AManifestInTheInjectedFileSystem_WhenLoaded_ShouldCarryTheStructureThatManifestDeclares()
    {
        // Given
        var schema = Path.Combine(Root, ".spec", "schema");
        var loader = new SpecSchemasLoader(
            new MockFileSystem(
                new Dictionary<string, MockFileData>
                {
                    [Path.Combine(schema, "spec-structure.schema.json")] = new("""{ "exclusions": ["vendor"] }"""),
                    [Path.Combine(schema, "feature-spec.frontmatter.schema.json")] = new("{}"),
                    [Path.Combine(schema, "task.frontmatter.schema.json")] = new("{}"),
                    [Path.Combine(schema, "epic.frontmatter.schema.json")] = new("{}"),
                }));

        // When
        var schemas = loader.Load(Root);

        // Then
        schemas.Structure.Discovery.Exclusions.Should().Equal("vendor");
    }

    [Fact]
    public void AManifestPinningAVersionOfTheSet_WhenLoadedThroughTheSet_ShouldKeepThatVersion()
    {
        // Given
        SchemaVersion one = new SchemaVersionFixture();
        SchemaVersion two = new SchemaVersionFixture()
            .WithNumber(new SemanticVersion(0, 2, 0))
            .WithRuleIds(new HashSet<string>(["FAKE001"], StringComparer.Ordinal));
        var loader = new SpecSchemasLoader(
            new MockFileSystem(
                new Dictionary<string, MockFileData>
                {
                    [Path.Combine(Root, ".spec", "schema", "spec-structure.schema.json")] = new("""{ "schemaVersion": "0.2.0" }"""),
                }));

        // When
        var schemas = loader.Load(Root, new SchemaVersions([one, two]));

        // Then
        schemas.Version.Should().Be(two);
    }

    private const string Root = "repo";
}
