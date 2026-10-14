using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Schema;

/// <summary>Input precedence and write scope beneath pin's CLI and selector paths.</summary>
[Trait("Tier", "Unit")]
public sealed class SchemaPinUnitTests
{
    [Theory]
    [InlineData(false, null, "7.0.0", typeof(SpechtRootNotFoundException))]
    [InlineData(true, null, "7.0.0", typeof(SpechtManifestNotFoundException))]
    [InlineData(true, "{", "7.0.0", typeof(SpechtManifestUnreadableException))]
    [InlineData(true, "null", "7.0.0", typeof(SpechtManifestUnreadableException))]
    [InlineData(true, "{\"rules\":{\"SPEC031\":\"invalid\"}}", "7.0.0", typeof(SpechtManifestException))]
    [InlineData(true, "{}", "7.0.0", typeof(SchemaVersionNotFoundException))]
    [InlineData(true, "{}", "0.1", typeof(SchemaVersionNotFoundException))]
    public async Task InvalidInput_WhenPinning_ShouldReportTheFirstFaultAndWriteNothing(bool rootExists, string? manifest, string target, Type fault)
    {
        // Given
        var files = new MockFileSystem();
        if (rootExists)
        {
            files.AddDirectory("repo");
        }
        if (manifest is not null)
        {
            files.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        }
        var versions = EngineServices.Versions();
        var handler = new SchemaVersionManager(new SpecManifestStore(files), new SpecManifestParser(versions),
            new ManifestPinEditor(new SpecManifestParser(versions), versions), versions);
        var before = files.AllFiles.ToDictionary(static path => path, files.File.ReadAllBytes);

        // When
        Func<Task> action = () => handler.Pin("repo", target, CancellationToken.None);

        // Then
        await action.Should().ThrowAsync<Exception>().Where(exception => exception.GetType() == fault);
        files.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var entry in before)
        {
            files.File.ReadAllBytes(entry.Key).Should().Equal(entry.Value);
        }
    }

    [Fact]
    public async Task CancelledWrite_WhenStagingAManifest_ShouldPreserveTheOriginalAndLeaveNoTemporaryFile()
    {
        // Given
        var files = new MockFileSystem();
        files.AddFile("repo/.specht/manifest.json", new MockFileData("original bytes"));
        var store = new SpecManifestStore(files);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var before = files.AllFiles.ToArray();

        // When
        Func<Task> action = () => store.WriteManifest("repo", "replacement bytes"u8.ToArray(), cancellation.Token);

        // Then
        await action.Should().ThrowAsync<OperationCanceledException>();
        files.AllFiles.Should().BeEquivalentTo(before);
        files.File.ReadAllText("repo/.specht/manifest.json").Should().Be("original bytes");
    }
}
