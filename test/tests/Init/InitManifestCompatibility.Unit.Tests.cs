using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Tool.Features.Init;

namespace Specht.Tests.Init;

/// <summary>Pins init's manifest and version refusal before writes (0001-F4 B-013, B-017; 0001-F5 C-5).</summary>
[Trait("Tier", "Unit")]
public sealed class InitManifestCompatibilityUnitTests
{
    [Theory]
    [ClassData(typeof(RejectedInitManifests))]
    public async Task ARejectedManifest_WhenEveryPartIsInitialized_ShouldRefuseBeforeWriting(string manifest, string reason)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpecTree.ManifestPath)] = new(manifest),
            [Path.Combine("repo", SpecTree.TemplatesFolder, "feature.md")] = new("Local template edits\r\n"),
            [Path.Combine("repo", "README.md")] = new("Synthetic repository\n"),
        });
        var before = fileSystem.AllFiles.ToDictionary(
            path => fileSystem.Path.GetRelativePath("repo", path),
            fileSystem.File.ReadAllBytes);
        InitWriter writer = new InitWriterFixture().WithFileSystem(fileSystem);

        // When
        var write = () => writer.Write("repo", schemas: true, templates: true, CancellationToken.None);

        // Then
        var failure = (await write.Should().ThrowAsync<Exception>()).Which;
        (failure is SpechtManifestException or SpechtManifestUnreadableException).Should().BeTrue();
        failure.Message.Should().Contain(".specht/manifest.json").And.Contain(reason);
        fileSystem.AllFiles.Select(path => fileSystem.Path.GetRelativePath("repo", path)).Should().BeEquivalentTo(before.Keys);
        foreach (var (path, bytes) in before)
        {
            fileSystem.File.ReadAllBytes(Path.Combine("repo", path)).Should().Equal(bytes);
        }
    }

    [Fact]
    public async Task AnExistingManifestWithAnUnshippedPinAndRuleId_WhenSchemasAreInitialized_ShouldRejectThePinBeforeWriting()
    {
        // Given
        const string manifest = """{"schemaVersion":"7.0.0","rules":{"SPEC999":"off"}}""";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpecTree.ManifestPath)] = new(manifest),
        });
        InitWriter writer = new InitWriterFixture()
            .WithFileSystem(fileSystem)
            .WithShippingCopy(new Dictionary<string, byte[]> { ["schema/v0.1.0/frontmatter/task.schema.json"] = "{}"u8.ToArray() });

        // When
        var write = () => writer.Write("repo", schemas: true, templates: false, CancellationToken.None);

        // Then
        (await write.Should().ThrowExactlyAsync<SpechtManifestException>()).Which.Message.Should().Contain("schemaVersion 7.0.0 is not shipped");
        fileSystem.File.ReadAllText(Path.Combine("repo", SpecTree.ManifestPath)).Should().Be(manifest);
        fileSystem.File.Exists(Path.Combine("repo", SpecTree.ItemSchemaPath)).Should().BeFalse();
    }

    [Theory]
    [InlineData("[")]
    [InlineData("[]")]
    public async Task AnExistingManifestWithUnreadableShape_WhenSchemasAreInitialized_ShouldKeepItsManifestFailure(string manifest)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpecTree.ManifestPath)] = new(manifest),
        });
        InitWriter writer = new InitWriterFixture()
            .WithFileSystem(fileSystem)
            .WithShippingCopy(new Dictionary<string, byte[]> { ["schema/v0.1.0/frontmatter/task.schema.json"] = "{}"u8.ToArray() });

        // When
        var write = () => writer.Write("repo", schemas: true, templates: false, CancellationToken.None);

        // Then
        (await write.Should().ThrowExactlyAsync<SpechtManifestUnreadableException>()).Which.Message
            .Should().Be(".specht/manifest.json: the manifest does not parse into the manifest's shape.");
    }
}
