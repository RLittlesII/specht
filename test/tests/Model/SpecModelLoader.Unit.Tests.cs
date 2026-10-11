using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Model;

namespace Specht.Tests.Model;

/// <summary>
/// The model loader over a synthetic tree held in an injected file system (ADR-0001 stage D; ADR-0005 (a), item 0107):
/// the specification, companion, item and epic the model carries are the ones that file system holds, each under its
/// root-relative path. Nothing touches the disk. The root is the file system's own full path of <c>repo</c>, because an
/// in-memory file system lists a relative directory's files by their full paths, which the real one does not.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecModelLoaderUnitTests
{
    [Fact]
    public void ATreeHeldInTheInjectedFileSystem_WhenLoaded_ShouldCarryItsSpecificationWithItsCompanionItsItemAndItsEpic()
    {
        // Given
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine("repo", ".spec", "schema", "spec-structure.schema.json")] = new("{}"),
                [Path.Combine("repo", "src", "area", ".spec", "README.md")] = new("---\nepic: \"0001\"\nid: F1\n---\n\n# Specification: F1\n"),
                [Path.Combine("repo", "src", "area", ".spec", "area.feature")] = new("Feature: it\n"),
                [Path.Combine("repo", "src", "area", ".spec", "0001-01-do.md")] = new("---\nid: \"0001-01\"\n---\n\n# Item\n"),
                [Path.Combine("repo", "epics", "0001-example", "epic.md")] = new("---\nid: \"0001\"\n---\n\n# Epic\n"),
            });
        var root = fileSystem.Path.GetFullPath("repo");
        var loader = new SpecModelLoader(
            fileSystem,
            new FrontmatterReader(fileSystem),
            new SpecDiscovery(fileSystem),
            new SpecSchemasLoader(fileSystem));

        // When
        var model = loader.Load(root);

        // Then
        var feature = model.Features.Should().ContainSingle().Subject;
        feature.RelativePath.Should().Be("src/area/.spec/README.md");
        feature.Identity.Should().Be("0001-F1");
        feature.FeatureFiles.Select(static file => Path.GetFileName(file)).Should().Equal("area.feature");
        model.Items.Select(static item => (item.RelativePath, item.Id)).Should().Equal(("src/area/.spec/0001-01-do.md", "0001-01"));
        model.Epics.Select(static epic => (epic.RelativePath, epic.Id)).Should().Equal(("epics/0001-example/epic.md", "0001"));
    }
}
