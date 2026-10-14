using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

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
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void AnUnreadableCompanion_WhenLoadingBeforeRuleSelection_ShouldFailEvenWithDisabledRulesOrMultipleCompanions(
        bool disabled, bool multiple)
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpecTree.ManifestPath)] = new(disabled ? """{"rules":{"SPEC020":"off","SPEC021":"off"}}""" : "{}"),
            [Path.Combine("repo", "src", "area", ".spec", "README.md")] = new("---\nepic: '0001'\nid: F1\n---\n"),
            [Path.Combine("repo", "src", "area", ".spec", "area.feature")] = new("Feature: area\n") { AllowedFileShare = FileShare.None },
        });
        if (multiple)
        {
            fileSystem.AddFile(Path.Combine("repo", "src", "area", ".spec", "second.feature"), new MockFileData("Feature: second\n"));
        }

        var loader = new SpecModelLoader(fileSystem, new FrontmatterReader(fileSystem), new SpecDiscovery(fileSystem),
            EngineServices.Schemas(fileSystem), new FeatureTagParser());

        // When
        var loading = () => loader.Load(fileSystem.Path.GetFullPath("repo"));

        // Then
        loading.Should().Throw<IOException>();
    }

    [Fact]
    public void ACompanionRemovedAfterLoadingWithItsRulesDisabled_WhenEvaluated_ShouldUseItsLoadedTagsAndRelativePath()
    {
        // Given
        var companion = Path.Combine("repo", "src", "area", ".spec", "area.feature");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpecTree.ManifestPath)] = new("""{"rules":{"SPEC020":"off","SPEC021":"off"}}"""),
            [Path.Combine("repo", "src", "area", ".spec", "README.md")] = new("---\nepic: '0001'\nid: F1\n---\n"),
            [companion] = new("Feature: example\n  @B-999\n"),
        });
        var loader = new SpecModelLoader(fileSystem, new FrontmatterReader(fileSystem), new SpecDiscovery(fileSystem),
            EngineServices.Schemas(fileSystem), new FeatureTagParser());
        var model = loader.Load(fileSystem.Path.GetFullPath("repo"));
        fileSystem.File.Delete(companion);

        // When
        var violations = new FeatureFileRule().Evaluate(model).ToList();

        // Then
        var violation = violations.Should().ContainSingle().Subject;
        violation.RuleId.Should().Be("SPEC021");
        violation.File.Should().Be("src/area/.spec/area.feature");
        violation.Line.Should().Be(2);
        violation.Identifier.Should().Be("B-999");
    }

    [Fact]
    public void ATreeHeldInTheInjectedFileSystem_WhenLoaded_ShouldCarryItsSpecificationWithItsCompanionItsItemAndItsEpic()
    {
        // Given
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine("repo", SpecTree.ManifestPath)] = new("{}"),
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
            EngineServices.Schemas(fileSystem), new FeatureTagParser());

        // When
        var model = loader.Load(root);

        // Then
        var feature = model.Features.Should().ContainSingle().Subject;
        feature.RelativePath.Should().Be("src/area/.spec/README.md");
        feature.Identity.Should().Be("0001-F1");
        feature.FeatureFiles.Select(static file => Path.GetFileName(file.RelativePath)).Should().Equal("area.feature");
        model.Items.Select(static item => (item.RelativePath, item.Id)).Should().Equal(("src/area/.spec/0001-01-do.md", "0001-01"));
        model.Epics.Select(static epic => (epic.RelativePath, epic.Id)).Should().Equal(("epics/0001-example/epic.md", "0001"));
    }
}
