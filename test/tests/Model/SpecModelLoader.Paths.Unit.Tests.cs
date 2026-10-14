using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>The loader maps each emitted file kind to a root-relative path (0001-F1 B-007; 0001-F3 B-021).</summary>
[Trait("Tier", "Unit")]
public sealed class SpecModelLoaderPathsUnitTests
{
    [Theory]
    [InlineData("tree")]
    [InlineData("tree/nested")]
    public void FilesUnderTheRoot_WhenLoaded_ShouldCarryRelativePathsWithForwardSlashes(string root)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine(root, SpecTree.ManifestPath)] = new("{}"),
            [Path.Combine(root, "src", "area", ".spec", "README.md")] = new("---\nepic: '0001'\nid: F1\n---\n"),
            [Path.Combine(root, "src", "area", ".spec", "area.feature")] = new("Feature: area\n"),
            [Path.Combine(root, "src", "area", ".spec", "0001-01-do.md")] = new("---\nid: '0001-01'\n---\n"),
            [Path.Combine(root, "epics", "0001-example", "epic.md")] = new("---\nid: '0001'\n---\n"),
        });
        var loader = new SpecModelLoader(fileSystem, new FrontmatterReader(fileSystem), new SpecDiscovery(fileSystem),
            EngineServices.Schemas(fileSystem), new FeatureTagParser());

        // When
        var model = loader.Load(fileSystem.Path.GetFullPath(root));

        // Then
        model.Features.Should().ContainSingle().Which.RelativePath.Should().Be("src/area/.spec/README.md");
        model.Features[0].FeatureFiles.Should().ContainSingle().Which.RelativePath.Should().Be("src/area/.spec/area.feature");
        model.Items.Should().ContainSingle().Which.RelativePath.Should().Be("src/area/.spec/0001-01-do.md");
        model.Epics.Should().ContainSingle().Which.RelativePath.Should().Be("epics/0001-example/epic.md");
    }
}
