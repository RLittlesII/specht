using AwesomeAssertions;
using Specht.Benchmarks;

namespace Specht.Tests.Benchmarks;

/// <summary>
/// The benchmark fixture's tree, in memory (<c>0109-F1</c> B-016; C-8, C-9; decision 0004): a tree built for a size holds
/// the same files, in the same order, at the same root-relative paths with the same contents every time it is built, and
/// names no file by anything but a path under its root.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtTreeUnitTests
{
    [Theory]
    [ClassData(typeof(TreeSizes))]
    public void TheSameSizeBuiltTwice_WhenTheTreesAreCompared_ShouldHoldTheSameRelativePathsInTheSameOrderWithTheSameContents(int specifications)
    {
        // Given
        var first = SpechtTree.Of(specifications);

        // When
        var second = SpechtTree.Of(specifications);

        // Then
        first.Files.Should().NotBeEmpty();
        second.Files.Select(static file => file.RelativePath).Should().Equal(first.Files.Select(static file => file.RelativePath));
        foreach (var (expected, actual) in first.Files.Zip(second.Files))
        {
            actual.Content.Span.SequenceEqual(expected.Content.Span).Should().BeTrue($"{expected.RelativePath} holds the same bytes in both trees");
        }
    }

    [Theory]
    [ClassData(typeof(TreeSizes))]
    public void ATreeOfAStatedSize_WhenBuilt_ShouldNameEveryFileByAForwardSlashedPathRelativeToItsRoot(int specifications)
    {
        // Given
        // When
        var tree = SpechtTree.Of(specifications);

        // Then
        tree.Files.Should().AllSatisfy(static file =>
        {
            file.RelativePath.Should().NotContain("\\").And.NotContain(":");
            file.RelativePath.Split('/').Should().NotContain(string.Empty).And.NotContain(".").And.NotContain("..");
        });
    }

    [Theory]
    [ClassData(typeof(TreeSizes))]
    public void ATreeOfAStatedSize_WhenBuilt_ShouldHoldTheFourSchemaFilesAndTwoFilesForEachSpecificationAtDistinctPaths(int specifications)
    {
        // Given
        // When
        var tree = SpechtTree.Of(specifications);

        // Then
        tree.Files.Should().HaveCount(4 + (2 * specifications));
        tree.Files.Select(static file => file.RelativePath).Should().OnlyHaveUniqueItems();
    }
}
