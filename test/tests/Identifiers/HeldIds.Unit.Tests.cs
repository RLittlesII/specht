using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Identifiers;
using Specht.Tests.Discovery;
using Specht.Tests.Shared;

namespace Specht.Tests.Identifiers;

/// <summary>
/// Where the guard reads the ids a repository-wide type holds (<c>0001-F19</c> B-038, B-039), over a
/// <see cref="MockFileSystem"/> tree under the exclusions a manifest that names none is given, which skip the root's
/// <c>.spec/</c> as no specification.
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
        var held = new HeldIds(new SpechtDiscovery(files, new UnexpectedChildProcess())).Of(type, root, exclusions);

        // Then
        held.Should().Contain(expected);
    }
}
