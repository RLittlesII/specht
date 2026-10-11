using AwesomeAssertions;
using Specht.Benchmarks;

namespace Specht.Tests.Benchmarks;

/// <summary>
/// The benchmark fixture's generator over directories on disk (<c>0109-F1</c> B-015, B-016; C-8, C-9; decision 0004): a
/// tree of each stated size is one the real check reports no violation on, over exactly that many specifications, and
/// one size generated into two different roots gives the same relative paths holding the same bytes.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpecTreeGeneratorIntegrationTests : IDisposable
{
    /// <summary>Gets each size decision 0004 names, in specifications.</summary>
    public static TheoryData<int> Sizes => [1, 10, 100, 1000];

    [Theory]
    [MemberData(nameof(Sizes))]
    public void ATreeOfAStatedSize_WhenChecked_ShouldReportNoViolationOverExactlyThatManySpecifications(int specifications)
    {
        // Given
        var root = Directory.CreateDirectory(Path.Combine(_temporary, "tree")).FullName;
        SpecTreeGenerator.Generate(root, specifications);

        // When
        var report = SpechtRunner.Run(root);

        // Then
        report.Violations.Should().BeEmpty();
        report.SpecificationCount.Should().Be(specifications);
    }

    [Theory]
    [MemberData(nameof(Sizes))]
    public void TheSameSizeGeneratedTwice_WhenTheTreesAreCompared_ShouldHoldTheSameRelativePathsWithTheSameContents(int specifications)
    {
        // Given
        var first = Directory.CreateDirectory(Path.Combine(_temporary, "first")).FullName;
        var second = Directory.CreateDirectory(Path.Combine(_temporary, "second", "nested")).FullName;

        // When
        SpecTreeGenerator.Generate(first, specifications);
        SpecTreeGenerator.Generate(second, specifications);

        // Then
        var expected = Files(first);
        var actual = Files(second);
        expected.Should().NotBeEmpty();
        actual.Keys.Should().Equal(expected.Keys);
        foreach (var (path, bytes) in expected)
        {
            actual[path].SequenceEqual(bytes).Should().BeTrue($"{path} holds the same bytes in both trees");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Directory.Exists(_temporary))
        {
            Directory.Delete(_temporary, recursive: true);
        }
    }

    private static SortedDictionary<string, byte[]> Files(string root)
    {
        var files = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            files[Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/')] = File.ReadAllBytes(file);
        }

        return files;
    }

    private readonly string _temporary = Path.Combine(Path.GetTempPath(), "specht-benchmarks-" + Guid.NewGuid().ToString("N"));
}
