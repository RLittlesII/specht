using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Identifiers;
using Specht.Model;
using Specht.Tests.Discovery;
using Specht.Tests.Shared;
using Specht.Tree;

namespace Specht.Tests.Identifiers;

/// <summary>
/// Where the guard reads the ids a repository-wide type holds (<c>0001-F19</c> B-038, B-039), over a
/// <see cref="MockFileSystem"/> tree under the exclusions a manifest that names none is given, which skip the root's
/// <c>.spec/</c> as no specification. A folder, relative to the root, names the Feature whose <c>.spec/</c> it holds, and a
/// folder that holds none, a legacy <c>epics/</c> Feature folder among them, names no Feature (B-059, B-060; A-7). A
/// per-Feature type's ids are held in that Feature's places: the file names in its <c>.spec/decisions/</c>, the first cells
/// of its <c>.spec/README.md</c> tables and the tags of its companion, less the <c>@</c> (B-040 to B-042; item <c>0418</c>). A
/// Feature's claim prefix is the text its <c>.spec/README.md</c> frontmatter holds under the key the manifest names, and nothing
/// for no key (A-9; B-063, item <c>0437</c>).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class HeldIdsUnitTests
{
    [Theory]
    [ClassData(typeof(HeldPlaces))]
    public void ATreeWithFilesInTheRootsAndAFeaturesFolders_WhenATypesHeldIdsAreRead_ShouldListTheNameOfEachFileInEveryFolderOfThatType(
        string type, string[] expected)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/.specht/manifest.json"] = new("""{ "schemaVersion": "0.3.1" }"""),
            ["repo/.spec/adr/a-root.md"] = new("# Held\n"),
            ["repo/src/area/.spec/adr/b-feature.md"] = new("# Held\n"),
            ["repo/.spec/lessons/c-root.md"] = new("# Held\n"),
            ["repo/src/area/.spec/lessons/d-feature.md"] = new("# Held\n"),
        });
        var root = files.Path.GetFullPath("repo");
        var exclusions = EngineServices.Manifest(files, root).Discovery.Exclusions;

        // When
        var held = new HeldIds(
            new SpechtDiscovery(files, new UnexpectedChildProcess()),
            new SpechtTreeStore(files),
            new SpechtDocumentParser(new FrontmatterReader()),
            new FeatureTagParser()).Of(type, root, new SpechtDiscoveryInputs([], exclusions, [], [], []), null);

        // Then
        held.Should().Contain(expected);
    }

    [Fact]
    public void AFolderHoldingAFeaturesSpec_WhenItsFeatureIsFound_ShouldGiveTheSpecificationTheCheckListsForThatFeature()
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/.specht/manifest.json"] = new("""{ "schemaVersion": "0.3.1" }"""),
            ["repo/features/one/.spec/README.md"] = new("# Specification: F1\n"),
            ["repo/features/two/.spec/README.md"] = new("# Specification: F2\n"),
        });
        var root = files.Path.GetFullPath("repo");
        var inputs = EngineServices.Manifest(files, root).Discovery;
        var discovery = new SpechtDiscovery(files, new UnexpectedChildProcess());
        var listed = discovery.FindSpecifications(root, inputs).Single(static location => location.RelativePath == "features/two/.spec/README.md");

        // When
        var feature = new HeldIds(
            discovery,
            new SpechtTreeStore(files),
            new SpechtDocumentParser(new FrontmatterReader()),
            new FeatureTagParser()).FeatureAt(root, inputs, "features/two");

        // Then
        feature.Should().Be(listed);
    }

    [Theory]
    [ClassData(typeof(FoldersHoldingNoFeature))]
    public void AFolderHoldingNoFeaturesSpec_WhenItsFeatureIsFound_ShouldGiveNoSpecification(string folder)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/.specht/manifest.json"] = new("""{ "schemaVersion": "0.3.1" }"""),
            ["repo/features/one/.spec/README.md"] = new("# Specification: F1\n"),
            ["repo/features/two/notes.md"] = new("# Notes\n"),
            ["repo/epics/0002-epic/F1-feature/spec.md"] = new("# Specification: F1\n"),
        });
        var root = files.Path.GetFullPath("repo");
        var inputs = EngineServices.Manifest(files, root).Discovery;

        // When
        var feature = new HeldIds(
            new SpechtDiscovery(files, new UnexpectedChildProcess()),
            new SpechtTreeStore(files),
            new SpechtDocumentParser(new FrontmatterReader()),
            new FeatureTagParser()).FeatureAt(root, inputs, folder);

        // Then
        feature.Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(PerFeaturePlaces))]
    public void AFeatureHoldingADecisionTableRowsAndAScenarioTag_WhenAPerFeatureTypesHeldIdsAreReadForIt_ShouldListTheNameHeldInThatTypesPlace(
        string type, string expected)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/.specht/manifest.json"] = new("""{ "schemaVersion": "0.3.1" }"""),
            ["repo/features/one/.spec/README.md"] = new(
                "# Specification: F1\n\n## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| B-abc123 | Held. |\n\n"
                + "## 4. Constraints\n\n| ID | Constraint |\n| -- | ---------- |\n| C-abc123 | Held. |\n\n"
                + "## 11. Open Questions\n\n| ID | Question |\n| -- | -------- |\n| OQ-abc123 | Held? |\n"),
            ["repo/features/one/.spec/decisions/abc123-held.md"] = new("# Held\n"),
            ["repo/features/one/.spec/one.feature"] = new("Feature: it\n\n  @S-abc123\n  Scenario: It is held\n    Given a thing\n"),
        });
        var root = files.Path.GetFullPath("repo");
        var inputs = EngineServices.Manifest(files, root).Discovery;
        var discovery = new SpechtDiscovery(files, new UnexpectedChildProcess());
        var feature = discovery.FindSpecifications(root, inputs).Single();

        // When
        var held = new HeldIds(
            discovery,
            new SpechtTreeStore(files),
            new SpechtDocumentParser(new FrontmatterReader()),
            new FeatureTagParser()).Of(type, root, inputs, feature);

        // Then
        held.Should().Contain(expected);
    }

    [Theory]
    [ClassData(typeof(ClaimPrefixKeys))]
    public void AFeatureDeclaringAClaimPrefix_WhenItsClaimPrefixIsRead_ShouldGiveTheTextUnderTheKeyNamed(string? key, string? expected)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/.specht/manifest.json"] = new("""{ "schemaVersion": "0.4.0" }"""),
            ["repo/features/one/.spec/README.md"] = new("---\nclaim_prefix: \"ACTIVITY\"\n---\n# Specification: F1\n"),
        });
        var root = files.Path.GetFullPath("repo");
        var inputs = EngineServices.Manifest(files, root).Discovery;
        var discovery = new SpechtDiscovery(files, new UnexpectedChildProcess());
        var feature = discovery.FindSpecifications(root, inputs).Single();

        // When
        var prefix = new HeldIds(
            discovery,
            new SpechtTreeStore(files),
            new SpechtDocumentParser(new FrontmatterReader()),
            new FeatureTagParser()).ClaimPrefixOf(root, feature, key);

        // Then
        prefix.Should().Be(expected);
    }
}
