using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>
/// The read of one frontmatter schema file under a root (<c>0001-F7</c> B-024, B-077; item 0092): the bytes of the file of
/// that name in the root's frontmatter schema folder, and nothing when that folder holds no file of the name, wherever
/// else under the root one sits. Nothing touches the disk.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecManifestStoreUnitTests
{
    [Fact]
    public void AFileInTheRootsFrontmatterSchemaFolder_WhenReadByItsName_ShouldBeItsBytes()
    {
        // Given
        byte[] bytes = [0xEF, 0xBB, 0xBF, (byte)'{', (byte)'}', (byte)'\r', (byte)'\n'];
        SpecManifestStore store = new SpecManifestStoreFixture().WithFileSystem(
            new MockFileSystem(new Dictionary<string, MockFileData> { [Path.Combine(Root, SpecTree.ItemSchemaPath)] = new(bytes) }));

        // When
        var read = store.ReadFrontmatterSchema(Root, "task.schema.json");

        // Then
        read.Should().Equal(bytes);
    }

    [Fact]
    public void AFileOfThatNameOnlyOutsideTheRootsFrontmatterSchemaFolder_WhenReadByItsName_ShouldBeNull()
    {
        // Given
        SpecManifestStore store = new SpecManifestStoreFixture().WithFileSystem(
            new MockFileSystem(
                new Dictionary<string, MockFileData>
                {
                    [Path.Combine(Root, SpecTree.ManifestPath)] = new("{}"),
                    [Path.Combine(Root, SpecTree.ConfigurationFolder, "schema", "task.schema.json")] = new("{}"),
                    [Path.Combine(Root, "task.schema.json")] = new("{}"),
                }));

        // When
        var read = store.ReadFrontmatterSchema(Root, "task.schema.json");

        // Then
        read.Should().BeNull();
    }

    [Fact]
    public void AManifestWhoseTextIsNotJson_WhenRead_ShouldReturnItsUnchangedTextWithoutParsing()
    {
        // Given
        const string text = "not JSON\r\n";
        SpecManifestStore store = new SpecManifestStoreFixture().WithFileSystem(new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine(Root, SpecTree.ManifestPath)] = new(text),
        }));

        // When
        var read = store.ReadManifest(Root);

        // Then
        read.Should().Be(text);
        store.RootExists(Root).Should().BeTrue();
        store.ManifestExists(Root).Should().BeTrue();
        store.RetiredManifestExists(Root).Should().BeFalse();
    }

    private const string Root = "repo";
}
