using System.Buffers;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Tree;

namespace Specht.Tests.Tree;

/// <summary>
/// The store that persists and loads a tree, over an in-memory file system (<c>0109-F1</c> B-016; C-9; decision 0005; item
/// <c>0166</c>): saving a tree under a root writes each of the tree's files at its relative path under that root, holding
/// the tree's bytes, and writes no other file; loading reads exactly the root-relative paths it is given, in that order,
/// and a file's text is decoded as <see cref="File.ReadAllText(string)"/> decodes it, whether its bytes are backed by an
/// array or by memory that is not one (<c>0001-F1</c> B-004).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtTreeStoreUnitTests
{
    [Fact]
    public void ATree_WhenSavedUnderARoot_ShouldWriteExactlyItsFilesUnderThatRootWithItsContents()
    {
        // Given
        var tree = new Specht.Tree.SpechtTree(
        [
            new SpechtTreeFile(".specht/manifest.json", Encoding.UTF8.GetBytes("{}\n")),
            new SpechtTreeFile("features/feature-0001/.spec/README.md", Encoding.UTF8.GetBytes("---\ntitle: \"Größe\"\n---\n")),
            new SpechtTreeFile("features/feature-0001/.spec/feature-0001.feature", new byte[] { 0xEF, 0xBB, 0xBF, 0x0D, 0x0A }),
        ]);
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(Root);
        var store = EngineServices.Store(fileSystem);

        // When
        store.Save(tree, Root);

        // Then
        var root = fileSystem.Path.GetFullPath(Root);
        var written = fileSystem.AllFiles.ToDictionary(
            file => fileSystem.Path.GetRelativePath(root, file).Replace(fileSystem.Path.DirectorySeparatorChar, '/'),
            fileSystem.File.ReadAllBytes,
            StringComparer.Ordinal);
        written.Keys.Should().BeEquivalentTo(tree.Files.Select(static file => file.RelativePath));
        foreach (var file in tree.Files)
        {
            written[file.RelativePath].AsSpan().SequenceEqual(file.Content.Span).Should().BeTrue($"{file.RelativePath} holds the tree's bytes");
        }
    }

    [Fact]
    public void ARootHoldingFilesNotListed_WhenLoaded_ShouldReadExactlyTheListedFilesInTheListedOrderWithTheirBytes()
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine(Root, "a.md")] = new(Encoding.UTF8.GetBytes("first")),
            [Path.Combine(Root, "b", "c.md")] = new(Encoding.UTF8.GetBytes("second")),
            [Path.Combine(Root, "d.md")] = new(Encoding.UTF8.GetBytes("not listed")),
        });
        var store = EngineServices.Store(fileSystem);

        // When
        var tree = store.Load(Root, ["b/c.md", "a.md"]);

        // Then
        tree.Files.Select(static file => (file.RelativePath, Encoding.UTF8.GetString(file.Content.Span)))
            .Should().Equal(("b/c.md", "second"), ("a.md", "first"));
    }

    [Theory]
    [ClassData(typeof(EncodedTexts))]
    public void AFileEncodedWithOrWithoutAByteOrderMark_WhenLoadedAndItsTextTaken_ShouldDecodeAsReadAllTextDecodes(
        string because, byte[] bytes, string expected)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { [Path.Combine(Root, "README.md")] = new(bytes) });
        var store = EngineServices.Store(fileSystem);

        // When
        var text = store.Load(Root, ["README.md"]).Files.Single().Text();

        // Then
        text.Should().Be(expected, because);
    }

    [Theory]
    [ClassData(typeof(EncodedTexts))]
    public void AFileWhoseBytesAreNotBackedByAnArray_WhenItsTextTaken_ShouldDecodeAsReadAllTextDecodes(
        string because, byte[] bytes, string expected)
    {
        // Given
        using var memory = new MemoryWithoutAnArray(bytes);
        var file = new SpechtTreeFile("README.md", memory.Memory);

        // When
        var text = file.Text();

        // Then
        text.Should().Be(expected, because);
    }

    private sealed class MemoryWithoutAnArray(byte[] bytes) : MemoryManager<byte>
    {
        public override Span<byte> GetSpan() => bytes;

        public override MemoryHandle Pin(int elementIndex = 0) => throw new NotSupportedException();

        public override void Unpin()
        {
        }

        protected override void Dispose(bool disposing)
        {
        }
    }

    private const string Root = "tree";
}
