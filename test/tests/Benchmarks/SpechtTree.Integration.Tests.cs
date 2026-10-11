using System.IO.Abstractions;
using AwesomeAssertions;
using Specht.Benchmarks;

namespace Specht.Tests.Benchmarks;

/// <summary>
/// The benchmark fixture's tree on disk (<c>0109-F1</c> B-015; decision 0004): a tree of each stated size, saved through the
/// store over the real file system, is one the real check reports no violation on, over exactly that many specifications.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtTreeIntegrationTests : IDisposable
{
    [Theory]
    [ClassData(typeof(TreeSizes))]
    public void ATreeOfAStatedSize_WhenChecked_ShouldReportNoViolationOverExactlyThatManySpecifications(int specifications)
    {
        // Given
        var root = Directory.CreateDirectory(Path.Combine(_temporary, "tree")).FullName;
        new SpechtTreeStore(new FileSystem()).Save(SpechtTree.Of(specifications), root);

        // When
        var report = SpechtRunner.Run(root);

        // Then
        report.Violations.Should().BeEmpty();
        report.SpecificationCount.Should().Be(specifications);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Directory.Exists(_temporary))
        {
            Directory.Delete(_temporary, recursive: true);
        }
    }

    private readonly string _temporary = Path.Combine(Path.GetTempPath(), "specht-benchmarks-" + Guid.NewGuid().ToString("N"));
}
