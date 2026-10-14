using AwesomeAssertions;
using Specht.Benchmarks;
using Specht.Manifest;
using Specht.Tests.Shared;

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
        var resources = new Specht.Resources.SpechtResources();
        var parser = new SpecManifestParser(resources.DefaultManifest, new Specht.Versioning.SchemaVersions(resources.Texts));
        var structure = parser.Parse(resources.DefaultManifest);
        var first = SpechtTree.Of(specifications, resources.DefaultManifestBytes, structure, parser.PinnedVersion(structure));

        // When
        var second = SpechtTree.Of(specifications, resources.DefaultManifestBytes, structure, parser.PinnedVersion(structure));

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
        var resources = new Specht.Resources.SpechtResources();
        var parser = new SpecManifestParser(resources.DefaultManifest, new Specht.Versioning.SchemaVersions(resources.Texts));
        var structure = parser.Parse(resources.DefaultManifest);
        // When
        var tree = SpechtTree.Of(specifications, resources.DefaultManifestBytes, structure, parser.PinnedVersion(structure));

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
        var resources = new Specht.Resources.SpechtResources();
        var parser = new SpecManifestParser(resources.DefaultManifest, new Specht.Versioning.SchemaVersions(resources.Texts));
        var structure = parser.Parse(resources.DefaultManifest);
        // When
        var tree = SpechtTree.Of(specifications, resources.DefaultManifestBytes, structure, parser.PinnedVersion(structure));

        // Then
        tree.Files.Should().HaveCount(4 + (2 * specifications));
        tree.Files.Select(static file => file.RelativePath).Should().OnlyHaveUniqueItems();
    }
    [Fact]
    public void ExplicitManifestBytesAndASelectedVersion_WhenATreeIsBuilt_ShouldPreserveThoseBytesAndSchemaTexts()
    {
        // Given
        var resources = new Specht.Resources.SpechtResources();
        var parser = new SpecManifestParser(resources.DefaultManifest, EngineServices.Versions());
        const string manifest = "{ \"frontmatterSchemas\": { \"feature\": \"custom.json\" } }\r\n";
        var structure = parser.Parse(manifest);
        const string schema = """{"title":"selected schema text"}""";
        var selected = EngineServices.Versions().Find(structure.SchemaVersion)! with { FeatureSchema = schema };
        var bytes = System.Text.Encoding.UTF8.GetBytes(manifest);

        // When
        var tree = SpechtTree.Of(1, bytes, structure, selected);

        // Then
        tree.Files.Single(static file => file.RelativePath == SpecTree.ManifestPath).Content.ToArray().Should().Equal(bytes);
        System.Text.Encoding.UTF8.GetString(tree.Files.Single(static file =>
            file.RelativePath == SpecTree.FrontmatterSchemaFolder + "/custom.json").Content.Span).Should().Be(schema);
    }
}
