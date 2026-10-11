using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Benchmarks;

namespace Specht.Tests.Benchmarks;

/// <summary>
/// The store that persists a benchmark's tree, over an in-memory file system (<c>0109-F1</c> B-016; C-9): saving a tree
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
        var tree = SpechtTree.Of(10);
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(Root);
        var store = new SpechtTreeStore(fileSystem);

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
