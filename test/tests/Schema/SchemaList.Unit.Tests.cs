using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Check;
using Specht.Manifest;
using Specht.Model;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Schema;

/// <summary>Optional pins and catalog order beneath 0001-F7 B-041, B-042, B-084 and B-085.</summary>
[Trait("Tier", "Unit")]
public sealed class SchemaListUnitTests
{
    [Theory]
    [InlineData(false, "{}", typeof(SpechtRootNotFoundException))]
    [InlineData(true, "{", typeof(SpechtManifestUnreadableException))]
    [InlineData(true, "{\"schemaVersion\":\"7.0.0\"}", typeof(SpechtManifestException))]
    public async Task InvalidInput_WhenListing_ShouldRejectItBeforeProducingAResult(bool rootExists, string manifest, Type fault)
    {
        // Given
        var files = new MockFileSystem();
        if (rootExists)
        {
            files.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        }
        var versions = EngineServices.Versions();
        var handler = new SchemaList.Handler(new SpecModelReader(new SpecManifestStore(files)), new SpecManifestParser(versions), versions);

        // When
        Func<Task> action = () => handler.Handle(new SchemaList.Query("repo"), CancellationToken.None);

        // Then
        await action.Should().ThrowAsync<Exception>().Where(exception => exception.GetType() == fault);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("{}", "0.1.0")]
    [InlineData("{\"schemaVersion\":\"0.2.0\"}", "0.2.0")]
    public async Task OptionalManifest_WhenListing_ShouldResolveOnlyItsDeclaredOrDefaultPin(string? manifest, string? pin)
    {
        // Given
        var files = new MockFileSystem();
        files.AddDirectory("repo");
        if (manifest is not null)
        {
            files.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        }
        var versions = EngineServices.Versions();
        var handler = new SchemaList.Handler(new SpecModelReader(new SpecManifestStore(files)), new SpecManifestParser(versions), versions);

        // When
        var result = await handler.Handle(new SchemaList.Query("repo"), CancellationToken.None);

        // Then
        result.Pinned?.ToString().Should().Be(pin);
        result.Versions.Should().Equal(versions.Versions.Select(static version => version.Number));
    }
}
