using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Tree;
using Specht.Versioning;

namespace Specht.Tests.Schema;

/// <summary>
/// Optional pins and catalog order beneath 0001-F7 B-041, B-042, B-084 and B-085, and the version the check uses beside the
/// pin under a floating pin policy (B-148, B-149), over a set holding the embedded 0.3.0 under each number named.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SchemaListUnitTests
{
    [Theory]
    [InlineData(false, "{}", typeof(SpechtRootNotFoundException))]
    [InlineData(true, "{", typeof(SpechtManifestUnreadableException))]
    [InlineData(true, "{\"schemaVersion\":\"7.0.0\"}", typeof(SpechtManifestException))]
    public void InvalidInput_WhenListing_ShouldRejectItBeforeProducingAResult(bool rootExists, string manifest, Type fault)
    {
        // Given
        var files = new MockFileSystem();
        if (rootExists)
        {
            files.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        }

        var versions = EngineServices.Versions();
        var handler = new SchemaVersionManager(new SpechtManifestStore(files), new SpechtManifestParser(versions),
            new ManifestPinEditor(new SpechtManifestParser(versions), versions), versions);

        // When
        Action action = () => handler.List("repo");

        // Then
        action.Should().Throw<Exception>().Where(exception => exception.GetType() == fault);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("{}", "0.1.0")]
    [InlineData("{\"schemaVersion\":\"0.2.0\"}", "0.2.0")]
    public void OptionalManifest_WhenListing_ShouldResolveOnlyItsDeclaredOrDefaultPin(string? manifest, string? pin)
    {
        // Given
        var files = new MockFileSystem();
        files.AddDirectory("repo");
        if (manifest is not null)
        {
            files.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        }

        var versions = EngineServices.Versions();
        var handler = new SchemaVersionManager(new SpechtManifestStore(files), new SpechtManifestParser(versions),
            new ManifestPinEditor(new SpechtManifestParser(versions), versions), versions);

        // When
        var (Versions, Pinned, InUse) = handler.List("repo");

        // Then
        Pinned?.ToString().Should().Be(pin);
        Versions.Should().Equal(versions.Versions.Select(static version => version.Number));
    }

    [Theory]
    [ClassData(typeof(PinPolicyListings))]
    public void AManifestPinningAVersionWithLatestPatch_WhenListing_ShouldReturnThePinAndTheVersionTheCheckUses(string[] held, string inUse)
    {
        // Given
        var files = new MockFileSystem();
        files.AddFile("repo/.specht/manifest.json", new MockFileData("""{ "schemaVersion": "0.3.0", "pinPolicy": "latest-patch" }"""));
        var embedded = EngineServices.Versions().Find(new SemanticVersion(0, 3, 0))!;
        var versions = new SchemaVersions(held.Select(number => embedded with { Number = Version(number) }));
        var handler = new SchemaVersionManager(new SpechtManifestStore(files), new SpechtManifestParser(versions),
            new ManifestPinEditor(new SpechtManifestParser(versions), versions), versions);

        // When
        var (Versions, Pinned, InUse) = handler.List("repo");

        // Then
        Pinned.Should().Be(new SemanticVersion(0, 3, 0));
        InUse.Should().Be(Version(inUse));
    }

    private static SemanticVersion Version(string text) =>
        SemanticVersion.TryParse(text, out var version) ? version : throw new FormatException(text);
}
