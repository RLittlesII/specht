using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Tool.Features.Init;

namespace Specht.Tests.Init;

/// <summary>Pins init's existing text-only validation and shape-failure category (0001-F1 C-9; 0001-F5 C-5).</summary>
[Trait("Tier", "Unit")]
public sealed class InitManifestCompatibilityUnitTests
{
    [Fact]
    public async Task AnExistingManifestWithAnUnshippedPinAndRuleId_WhenSchemasAreInitialized_ShouldKeepTextOnlyValidation()
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
        await writer.Write("repo", schemas: true, templates: false, CancellationToken.None);

        // Then
        fileSystem.File.ReadAllText(Path.Combine("repo", SpecTree.ManifestPath)).Should().Be(manifest);
        fileSystem.File.ReadAllText(Path.Combine("repo", SpecTree.ItemSchemaPath)).Should().Be("{}");
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
