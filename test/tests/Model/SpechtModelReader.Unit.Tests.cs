using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>
/// The model reader over a synthetic tree held in an injected file system (ADR-0001 stage D; ADR-0005 (a), items 0107 and
/// 0166): the specification, companion, item and epic the model carries are the ones that file system holds, each under
/// its root-relative path, read through the stores the reader sequences. Nothing touches the disk. The root is the file
/// system's own full path of <c>repo</c>, because an in-memory file system lists a relative directory's files by their full
/// paths, which the real one does not. A Feature in two layouts reaches the model, and so <c>SPEC012</c>'s first
/// declaration and the dependency graph's kept twin, in discovery's one ordinal order of root-relative path (<c>0001-F6</c>
/// B-008; item 0009).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtModelReaderUnitTests
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
            [Path.Combine("repo", SpechtTree.ManifestPath)] = new(disabled ? """{"rules":{"SPEC020":"off","SPEC021":"off"}}""" : "{}"),
            [Path.Combine("repo", "src", "area", ".spec", "README.md")] = new("---\nepic: '0001'\nid: F1\n---\n"),
            [Path.Combine("repo", "src", "area", ".spec", "area.feature")] = new("Feature: area\n") { AllowedFileShare = FileShare.None },
        });
        if (multiple)
        {
            fileSystem.AddFile(Path.Combine("repo", "src", "area", ".spec", "second.feature"), new MockFileData("Feature: second\n"));
        }

        var reader = EngineServices.Reader(fileSystem);

        // When
        var loading = () => reader.Read(fileSystem.Path.GetFullPath("repo"), null);

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
            [Path.Combine("repo", SpechtTree.ManifestPath)] = new("""{"rules":{"SPEC020":"off","SPEC021":"off"}}"""),
            [Path.Combine("repo", "src", "area", ".spec", "README.md")] = new("---\nepic: '0001'\nid: F1\n---\n"),
            [companion] = new("Feature: example\n  @B-999\n"),
        });
        var reader = EngineServices.Reader(fileSystem);
        var model = reader.Read(fileSystem.Path.GetFullPath("repo"), null);
        fileSystem.File.Delete(companion);
        var rule = new ClaimTagRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        var finding = findings.Should().ContainSingle().Subject;
        rule.Id.Should().Be("SPEC021");
        finding.File.Should().Be("src/area/.spec/area.feature");
        finding.Line.Should().Be(2);
        finding.Identifier.Should().Be("B-999");
    }

    [Fact]
    public void ATreeHeldInTheInjectedFileSystem_WhenLoaded_ShouldCarryItsSpecificationWithItsCompanionItsItemAndItsEpic()
    {
        // Given
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine("repo", SpechtTree.ManifestPath)] = new("{}"),
                [Path.Combine("repo", "src", "area", ".spec", "README.md")] = new("---\nepic: \"0001\"\nid: F1\n---\n\n# Specification: F1\n"),
                [Path.Combine("repo", "src", "area", ".spec", "area.feature")] = new("Feature: it\n"),
                [Path.Combine("repo", "src", "area", ".spec", "0001-01-do.md")] = new("---\nid: \"0001-01\"\n---\n\n# Item\n"),
                [Path.Combine("repo", "epics", "0001-example", "epic.md")] = new("---\nid: \"0001\"\n---\n\n# Epic\n"),
            });
        var root = fileSystem.Path.GetFullPath("repo");
        var reader = EngineServices.Reader(fileSystem);

        // When
        var model = reader.Read(root, null);

        // Then
        var feature = model.Features.Should().ContainSingle().Subject;
        feature.RelativePath.Should().Be("src/area/.spec/README.md");
        feature.Identity.Should().Be("0001-F1");
        feature.FeatureFiles.Select(static file => Path.GetFileName(file.RelativePath)).Should().Equal("area.feature");
        model.Items.Select(static item => (item.RelativePath, item.Id)).Should().Equal(("src/area/.spec/0001-01-do.md", "0001-01"));
        model.Epics.Select(static epic => (epic.RelativePath, epic.Id)).Should().Equal(("epics/0001-example/epic.md", "0001"));
    }

    [Fact]
    public void AFeatureInBothLayoutsWhoseCoLocatedPathSortsFirst_WhenLoaded_ShouldDeclareTheCoLocatedCopyFirstAndKeepTheEpicsCopysEdges()
    {
        // Given
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine("repo", SpechtTree.ManifestPath)] = new("{}"),
                [Path.Combine("repo", "docs", "x", ".spec", "README.md")] = new("---\nepic: \"0001\"\nid: F1\ndepends_on: [F2]\n---\n"),
                [Path.Combine("repo", "epics", "0001-e", "F1-f", "spec.md")] = new("---\nepic: \"0001\"\nid: F1\ndepends_on: [F3]\n---\n"),
            });
        var root = fileSystem.Path.GetFullPath("repo");
        var reader = EngineServices.Reader(fileSystem);

        // When
        var model = reader.Read(root, null);

        // Then
        model.Features.Select(static feature => feature.RelativePath).Should().Equal("docs/x/.spec/README.md", "epics/0001-e/F1-f/spec.md");
        new DuplicateIdentityRule().Evaluate(model).Select(static finding => finding.Expected!["firstDeclaration"]!.GetValue<string>())
            .Should().Equal("docs/x/.spec/README.md", "docs/x/.spec/README.md");
        model.DependencyGraph.DependsOn["0001-F1"].Should().BeEquivalentTo(["0001-F3"]);
    }
}
