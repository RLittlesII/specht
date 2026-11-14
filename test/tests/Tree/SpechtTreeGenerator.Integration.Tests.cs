using System.IO.Abstractions;
using AwesomeAssertions;
using Specht.Tests.Shared;

namespace Specht.Tests.Tree;

/// <summary>
/// The engine's generated tree on disk (<c>0109-F1</c> B-015; decisions 0004 and 0005): a tree of each stated size, saved
/// through the store over the real file system, is one the real check reports no violation on, over exactly that many
/// specifications.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtTreeGeneratorIntegrationTests : IDisposable
{
    [Theory]
    [ClassData(typeof(TreeSizes))]
    public void ATreeOfAStatedSize_WhenChecked_ShouldReportNoViolationOverExactlyThatManySpecifications(int specifications)
    {
        // Given
        var root = Directory.CreateDirectory(Path.Combine(_temporary, "tree")).FullName;
        EngineServices.Store(new FileSystem()).Save(EngineServices.Generator().Generate(specifications), root);

        // When
        var report = EngineServices.Check().Run(root, null);

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

    private readonly string _temporary = Path.Combine(Path.GetTempPath(), "specht-tree-" + Guid.NewGuid().ToString("N"));
}
