using System.Text;
using AwesomeAssertions;
using Specht.Resources;
using Specht.Tests.Shared;
using Specht.Versioning;
using TestTree = Specht.Tests.Shared.SpechtTree;

namespace Specht.Tests.Tree;

/// <summary>
/// The engine's tree generator, in memory (<c>0109-F1</c> B-016; C-7, C-8, C-9; decisions 0004 and 0005): a tree generated
/// for a size holds the same files, in the same order, at the same root-relative paths with the same contents every time
/// it is generated, names no file by anything but a path under its root, and holds the shipped manifest's bytes and the
/// pinned version's frontmatter schemas under the names that manifest gives them.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtTreeGeneratorUnitTests
{
    [Theory]
    [ClassData(typeof(TreeSizes))]
    public void TheSameSizeGeneratedTwice_WhenTheTreesAreCompared_ShouldHoldTheSameRelativePathsInTheSameOrderWithTheSameContents(int specifications)
    {
        // Given
        var generator = EngineServices.Generator();
        var first = generator.Generate(specifications);

        // When
        var second = generator.Generate(specifications);

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
    public void ATreeOfAStatedSize_WhenGenerated_ShouldNameEveryFileByAForwardSlashedPathRelativeToItsRoot(int specifications)
    {
        // Given
        var generator = EngineServices.Generator();

        // When
        var tree = generator.Generate(specifications);

        // Then
        tree.Files.Should().AllSatisfy(static file =>
        {
            file.RelativePath.Should().NotContain("\\").And.NotContain(":");
            file.RelativePath.Split('/').Should().NotContain(string.Empty).And.NotContain(".").And.NotContain("..");
        });
    }

    [Theory]
    [ClassData(typeof(TreeSizes))]
    public void ATreeOfAStatedSize_WhenGenerated_ShouldHoldFiveFixedFilesAndTwoFilesForEachSpecificationAtDistinctPaths(int specifications)
    {
        // Given
        var generator = EngineServices.Generator();

        // When
        var tree = generator.Generate(specifications);

        // Then
        tree.Files.Should().HaveCount(5 + (2 * specifications));
        tree.Files.Select(static file => file.RelativePath).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void APinnedVersionWithItsOwnSchemaTexts_WhenATreeIsGenerated_ShouldHoldTheShippedManifestBytesAndThoseTextsUnderTheManifestsNames()
    {
        // Given
        var resources = new SpechtResources();
        var structure = EngineServices.Parser().Parse(resources.DefaultManifest);
        var pinned = EngineServices.Versions().Find(structure.SchemaVersion)! with
        {
            FeatureSchema = """{"title":"pinned feature schema"}""",
            ItemSchema = """{"title":"pinned task schema"}""",
            EpicSchema = """{"title":"pinned epic schema"}""",
        };
        var generator = EngineServices.Generator(new SchemaVersions([pinned]));

        // When
        var tree = generator.Generate(1);

        // Then
        var files = tree.Files.ToDictionary(static file => file.RelativePath, static file => file.Content, StringComparer.Ordinal);
        files[TestTree.ManifestPath].ToArray().Should().Equal(resources.DefaultManifestBytes.ToArray());
        Encoding.UTF8.GetString(files[TestTree.FrontmatterSchemaFolder + "/" + structure.FrontmatterSchemas["feature"]].Span).Should().Be(pinned.FeatureSchema);
        Encoding.UTF8.GetString(files[TestTree.FrontmatterSchemaFolder + "/" + structure.FrontmatterSchemas["task"]].Span).Should().Be(pinned.ItemSchema);
        Encoding.UTF8.GetString(files[TestTree.FrontmatterSchemaFolder + "/" + structure.FrontmatterSchemas["epic"]].Span).Should().Be(pinned.EpicSchema);
    }
}
