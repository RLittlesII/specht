using System.IO.Abstractions.TestingHelpers;
using System.Text;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Tool.Features.Init;

namespace Specht.Tests.Init;

/// <summary>Pins restoration at the manifest version and byte preservation across repeated init runs (0001-F4 B-002, B-012, B-013).</summary>
[Trait("Tier", "Unit")]
public sealed class PinnedInitWriterUnitTests
{
    [Theory]
    [InlineData("0.1.0", true, true)]
    [InlineData("0.1.1", true, false)]
    [InlineData("0.1.0", false, true)]
    [InlineData("0.1.0", false, false)]
    [InlineData(null, true, true)]
    public async Task AnOlderPin_WhenMissingPartsAreInitialized_ShouldWriteOnlyPinnedBytesAndPreserveExistingFiles(
        string? pin, bool schemas, bool templates)
    {
        // Given
        var manifest = pin is null ? "{}\n" : $$"""{"schemaVersion":"{{pin}}"}""";
        var version = pin ?? "0.1.0";
        byte[] localSchema = [0xEF, 0xBB, 0xBF, 0x7B, 0x0D, 0x0A];
        byte[] localTemplate = "# Local edits\r\n"u8.ToArray();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpecTree.ManifestPath)] = new(manifest),
            [Path.Combine("repo", SpecTree.FeatureSchemaPath)] = new(localSchema),
            [Path.Combine("repo", SpecTree.TemplatesFolder, "feature.md")] = new(localTemplate),
        });
        var shipping = new[] { "0.1.0", "0.1.1", "0.2.0" }
            .SelectMany(number => new[]
            {
                "schema/frontmatter/feature.schema.json", "schema/frontmatter/task.schema.json", "templates/feature.md", "templates/adr.md",
            }.Select(path =>
                new KeyValuePair<string, byte[]>(
                    path.Insert(path.IndexOf('/', StringComparison.Ordinal), $"/v{number}"),
                    Encoding.UTF8.GetBytes($"{path} {number}\r\n"))))
            .ToDictionary(static entry => entry.Key, static entry => entry.Value);
        InitWriter writer = new InitWriterFixture().WithFileSystem(fileSystem).WithShippingCopy(shipping);

        // When
        var listing = await writer.Write("repo", schemas, templates, CancellationToken.None);
        var rerun = await writer.Write("repo", schemas, templates, CancellationToken.None);

        // Then
        listing.Count.Should().Be((schemas ? 2 : 0) + (templates ? 2 : 0));
        listing.Where(static entry => !entry.Written).Select(static entry => entry.Path)
            .Should().BeEquivalentTo(new[]
            {
                schemas ? SpecTree.FeatureSchemaPath : null,
                templates ? $"{SpecTree.TemplatesFolder}/feature.md" : null,
            }.OfType<string>());
        rerun.Should().Equal(listing.Select(static entry => (entry.Path, false)));
        fileSystem.File.ReadAllText(Path.Combine("repo", SpecTree.ManifestPath)).Should().Be(manifest);
        fileSystem.File.ReadAllBytes(Path.Combine("repo", SpecTree.FeatureSchemaPath)).Should().Equal(localSchema);
        fileSystem.File.ReadAllBytes(Path.Combine("repo", SpecTree.TemplatesFolder, "feature.md")).Should().Equal(localTemplate);
        fileSystem.File.Exists(Path.Combine("repo", SpecTree.ItemSchemaPath)).Should().Be(schemas);
        fileSystem.File.Exists(Path.Combine("repo", SpecTree.TemplatesFolder, "adr.md")).Should().Be(templates);
        foreach (var (relative, _) in listing.Where(static entry => entry.Written))
        {
            var local = relative[".specht/".Length..];
            var embedded = local.Insert(local.IndexOf('/', StringComparison.Ordinal), $"/v{version}");
            fileSystem.File.ReadAllBytes(Path.Combine("repo", relative)).Should().Equal(shipping[embedded]);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task AnUnshippedPin_WhenAnyPartsAreInitialized_ShouldRefuseBeforeWriting(bool schemas, bool templates)
    {
        // Given
        const string manifest = """{"schemaVersion":"7.0.0"}""";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpecTree.ManifestPath)] = new(manifest),
        });
        InitWriter writer = new InitWriterFixture().WithFileSystem(fileSystem);

        // When
        var write = () => writer.Write("repo", schemas, templates, CancellationToken.None);

        // Then
        (await write.Should().ThrowExactlyAsync<SpechtManifestException>()).Which.Message
            .Should().Contain("schemaVersion 7.0.0 is not shipped").And.Contain("0.1.0").And.Contain("0.2.0");
        fileSystem.AllFiles.Should().ContainSingle();
        fileSystem.File.ReadAllText(Path.Combine("repo", SpecTree.ManifestPath)).Should().Be(manifest);
    }
}
