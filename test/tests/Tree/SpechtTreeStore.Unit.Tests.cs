using System.IO.Abstractions.TestingHelpers;
using System.Text;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Tree;

namespace Specht.Tests.Tree;

/// <summary>
/// The store that persists a tree, over an in-memory file system (<c>0109-F1</c> B-016; C-9; decision 0005): saving a tree
/// under a root writes each of the tree's files at its relative path under that root, holding the tree's bytes, and writes
/// no other file.
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

    private const string Root = "tree";
}
