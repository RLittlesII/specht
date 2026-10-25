using System;
using System.IO;
using System.Linq;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [Given("the root's frontmatter schemas and templates are version {word}'s")]
    public void GivenTheRootFilesAreVersion(string files) => WriteShippedFiles(Tree, files);

    [Given("the root is pinned to version {word} with the embedded source and no upstream schema source")]
    [Given("the root is pinned to version {word} and records no upstream schema source")]
    public void GivenTheRootIsPinnedWithTheEmbeddedSource(string version) => GivenTheManifestPinsVersion(version);

    [Given("its manifest names no template as the consumer's")]
    public void GivenTheManifestNamesNoConsumerTemplate() =>
        File.ReadAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath)).Should().NotContain("templates");

    [Given("one of its templates has been edited by hand")]
    public void GivenOneTemplateHasBeenEditedByHand() =>
        File.WriteAllText(Path.Combine(Tree.Root, SpecTree.TemplatesFolder, "feature.md"), "# A synthetic template with local edits\n");

    [Given("its frontmatter schemas, templates and manifest keys are already version {word}'s")]
    public void GivenTheRootIsAlreadyAtItsPin(string version) => WriteShippedFiles(Tree, version);

    [When("upgrade runs against it")]
    public void WhenUpgradeRunsAgainstIt()
    {
        _filesBefore = SnapshotFiles();
        (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Tree.Root, "upgrade");
    }

    [Then("the frontmatter schemas and the templates are version {word}'s")]
    public void ThenTheFilesAreVersion(string version)
    {
        using var expected = new SpecTree();
        WriteShippedFiles(expected, version);
        var after = SnapshotFiles();
        var shipped = SnapshotFiles(expected.Root).Where(static pair => !pair.Key.EndsWith("manifest.json", StringComparison.Ordinal)).ToList();
        shipped.Should().NotBeEmpty();
        foreach (var (path, bytes) in shipped)
        {
            after[path].Should().Equal(bytes, path);
        }
    }

    [Then("the standard output says the repository is current")]
    public void ThenTheStandardOutputSaysCurrent() => _stdout.Should().Contain(".specht/ is current at schemaVersion");

    [Then("no file was rewritten")]
    public void ThenNoFileWasRewritten() => ThenNoFileHasChanged();

    [Then("the only files changed are the manifest and files under the frontmatter schema folder and the templates folder")]
    public void ThenOnlyTheAllowedFilesChanged()
    {
        _exitCode.Should().Be(0, _stderr);
        var after = SnapshotFiles();
        after.Keys.Should().BeEquivalentTo(_filesBefore!.Keys);
        after.Where(pair => !pair.Value.SequenceEqual(_filesBefore[pair.Key])).Select(static pair => pair.Key)
            .Should().OnlyContain(static path =>
                path == SpecTree.ManifestPath
                || path.StartsWith(SpecTree.FrontmatterSchemaFolder + "/", StringComparison.Ordinal)
                || path.StartsWith(SpecTree.TemplatesFolder + "/", StringComparison.Ordinal));
    }

    private static void WriteShippedFiles(SpecTree tree, string version)
    {
        var path = Path.Combine(tree.Root, SpecTree.ManifestPath);
        var manifest = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var pinned = manifest["schemaVersion"]?.DeepClone();
        manifest["schemaVersion"] = version;
        File.WriteAllText(path, manifest.ToJsonString());
        Directory.Delete(Path.Combine(tree.Root, SpecTree.ConfigurationFolder, "schema"), recursive: true);
        var result = AcceptanceTool.Launch(tree.Root, "init", "--all");
        result.ExitCode.Should().Be(0, result.Stderr);
        manifest["schemaVersion"] = pinned ?? version;
        File.WriteAllText(path, manifest.ToJsonString());
    }

    private static System.Collections.Generic.Dictionary<string, byte[]> SnapshotFiles(string root) =>
        Directory.GetFiles(root, "*", SearchOption.AllDirectories)
            .ToDictionary(file => Path.GetRelativePath(root, file).Replace('\\', '/'), File.ReadAllBytes, StringComparer.Ordinal);
}
