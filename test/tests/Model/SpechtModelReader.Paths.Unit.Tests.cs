using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>
/// The model the reader builds holds each emitted file kind, and every directory, by its root-relative path and no root
/// (0001-F1 B-007; 0001-F3 B-021; item 0166 criterion 5).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtModelReaderPathsUnitTests
{
    [Theory]
    [InlineData("tree")]
    [InlineData("tree/nested")]
    public void FilesUnderTheRoot_WhenLoaded_ShouldCarryRelativePathsWithForwardSlashes(string root)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine(root, SpechtTree.ManifestPath)] = new("{}"),
            [Path.Combine(root, "src", "area", ".spec", "README.md")] = new("---\nepic: '0001'\nid: F1\n---\n"),
            [Path.Combine(root, "src", "area", ".spec", "area.feature")] = new("Feature: area\n"),
            [Path.Combine(root, "src", "area", ".spec", "0001-01-do.md")] = new("---\nid: '0001-01'\n---\n"),
            [Path.Combine(root, "epics", "0001-example", "epic.md")] = new("---\nid: '0001'\n---\n"),
        });
        var reader = EngineServices.Reader(fileSystem);

        // When
        var model = reader.Read(fileSystem.Path.GetFullPath(root), null);

        // Then
        model.Features.Should().ContainSingle().Which.RelativePath.Should().Be("src/area/.spec/README.md");
        model.Features[0].FeatureFiles.Should().ContainSingle().Which.RelativePath.Should().Be("src/area/.spec/area.feature");
        model.Items.Should().ContainSingle().Which.RelativePath.Should().Be("src/area/.spec/0001-01-do.md");
        model.Epics.Should().ContainSingle().Which.RelativePath.Should().Be("epics/0001-example/epic.md");
    }

    [Fact]
    public void ATreeUnderARoot_WhenRead_ShouldHoldEveryPathAndDirectoryRelativeToThatRoot()
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("tree", SpechtTree.ManifestPath)] = new(
                """
                {
                  "schemaVersion": "0.2.0",
                  "layouts": [{ "name": "root", "glob": "README.md" }, { "name": "features", "glob": "**/.spec/README.md" }],
                  "tests": { "files": ["test/**/*.cs"] }
                }
                """),
            [Path.Combine("tree", "README.md")] = new("---\nepic: '0001'\nid: F1\n---\n"),
            [Path.Combine("tree", "0001-01-top.md")] = new("---\nid: '0001-01'\n---\n"),
            [Path.Combine("tree", "src", "area", ".spec", "README.md")] = new("---\nepic: '0001'\nid: F2\n---\n"),
            [Path.Combine("tree", "src", "area", ".spec", "area.feature")] = new("Feature: area\n"),
            [Path.Combine("tree", "src", "area", ".spec", "0001-02-do.md")] = new("---\nid: '0001-02'\n---\n"),
            [Path.Combine("tree", "epics", "0001-example", "epic.md")] = new("---\nid: '0001'\n---\n"),
            [Path.Combine("tree", "test", "Area.Unit.Tests.cs")] = new("class AreaUnitTests;\n"),
        });
        var reader = EngineServices.Reader(fileSystem);

        // When
        var model = reader.Read(fileSystem.Path.GetFullPath("tree"), null);

        // Then
        model.Features.Select(static feature => (feature.RelativePath, feature.Location.Directory, feature.Document.RelativePath))
            .Should().Equal(
                ("README.md", string.Empty, "README.md"),
                ("src/area/.spec/README.md", "src/area/.spec", "src/area/.spec/README.md"));
        model.Features.SelectMany(static feature => feature.FeatureFiles).Select(static file => file.RelativePath)
            .Should().Equal("src/area/.spec/area.feature");
        model.Items.Select(static item => (item.RelativePath, item.ParentDirectory))
            .Should().Equal(("0001-01-top.md", string.Empty), ("src/area/.spec/0001-02-do.md", "src/area/.spec"));
        model.Epics.Select(static epic => epic.RelativePath).Should().Equal("epics/0001-example/epic.md");
        model.TestFiles.Select(static file => file.RelativePath).Should().Equal("test/Area.Unit.Tests.cs");
    }
}
