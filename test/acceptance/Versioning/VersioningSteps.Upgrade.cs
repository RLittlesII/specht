using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;
using Specht.Tool.Features.Init;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [Given("the root's frontmatter schemas and templates are version {word}'s")]
    public void GivenTheRootFilesAreVersion(string files) => WriteShippedFiles(Tree, files);

    [Given("the root is pinned to version {word} with the embedded source and no upstream schema source")]
    [Given("the root is pinned to version {word} and records no upstream schema source")]
    public void GivenTheRootIsPinnedWithTheEmbeddedSource(string version) => GivenTheManifestPinsVersion(version);

    [Given("its manifest names no template as the consumer's")]
    public void GivenTheManifestNamesNoConsumerTemplate() => ManifestAfter().ContainsKey("consumerTemplates").Should().BeFalse();

    [Given("one of its templates has been edited by hand")]
    public void GivenOneTemplateHasBeenEditedByHand() =>
        File.WriteAllText(Path.Combine(Tree.Root, SpecTree.TemplatesFolder, "feature.md"), "# A synthetic template with local edits\n");

    [Given("its frontmatter schemas, templates and manifest keys are already version {word}'s")]
    public void GivenTheRootIsAlreadyAtItsPin(string version)
    {
        WriteShippedFiles(Tree, version);
        EditManifest(manifest =>
        {
            foreach (var (key, value) in DefinedKeys(version).Where(member => !manifest.ContainsKey(member.Key)))
            {
                manifest[key] = value!.DeepClone();
            }
        });
    }

    [Given("the root is pinned to version {word} and its frontmatter schemas and templates are already version {word}'s")]
    public void GivenTheRootIsPinnedAndItsFilesAreAlreadyVersion(string pin, string files) => GivenTheRootAndFilesHaveVersions(pin, files);

    [Given("its manifest lacks a key version {word} defines")]
    public void GivenTheManifestLacksAKeyTheVersionDefines(string version)
    {
        var missing = DefinedKeys(version).Last().Key;
        EditManifest(manifest => manifest.Remove(missing));
    }

    [Given("its manifest already carries a key version {word} defines, set to a value of the consumer's")]
    public void GivenTheManifestCarriesADefinedKeyWithTheConsumersValue(string version)
    {
        DefinedKeys(version).Should().Contain(static member => member.Key == ConsumerValuedKey);
        _consumerValue = new JsonArray("vendor");
        EditManifest(manifest => manifest[ConsumerValuedKey] = _consumerValue.DeepClone());
    }

    [Given("its manifest declares no epic grammar")]
    public void GivenItsManifestDeclaresNoEpicGrammar() =>
        EditManifest(static manifest => manifest.Remove("identifiers").Should().BeTrue());

    [When("upgrade runs against it")]
    public void WhenUpgradeRunsAgainstIt()
    {
        _filesBefore = SnapshotFiles();
        (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Tree.Root, "upgrade");
    }

    [When("upgrade runs against a path that is not a directory")]
    public void WhenUpgradeRunsAgainstAPathThatIsNotADirectory()
    {
        _filesBefore = SnapshotFiles();
        (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Tree.Root, "upgrade", "--root", "no-such-root");
    }

    [Given("the root's manifest records an upstream schema source and explicitly selects the embedded source")]
    public void GivenTheManifestRecordsAnUpstreamSchemaSourceAndExplicitlySelectsTheEmbeddedSource()
    {
        EditManifest(static manifest => manifest["schemaVersion"] = "0.2.0");
        GivenTheManifestRecordsAnUpstreamSchemaSource();
        GivenManifestEmbeddedSource();
    }

    [Then("the standard error names version {word} and the versions the tool ships, as the check's does")]
    public void ThenTheStandardErrorNamesVersionAndTheVersionsTheToolShipsAsTheChecksDoes(string version)
    {
        ThenTheStandardErrorNamesVersionAndTheVersionsTheToolShips(version);
        _stderr.Should().Be(AcceptanceTool.Launch(Tree.Root).Stderr);
    }

    [Then("the standard error names {word}, as the check's does")]
    public void ThenTheStandardErrorNamesAsTheChecksDoes(string value)
    {
        ThenTheStandardErrorNames(value);
        _stderr.Should().Be(AcceptanceTool.Launch(Tree.Root).Stderr);
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

    [Then("the warning, the disabled rule and the renamed section are as the consumer set them")]
    public void ThenTheConsumersSettingsAreAsSet()
    {
        var after = ManifestAfter();
        JsonNode.DeepEquals(after["rules"], new JsonObject { ["SPEC031"] = "warning", ["SPEC020"] = "off" }).Should().BeTrue();
        after["sections"]![2]!.GetValue<string>().Should().Be("3. Release criteria");
        after["roles"]!["claims"]!.GetValue<string>().Should().Be("3. Release criteria");
    }

    [Then("the key version {word} defines keeps the consumer's value")]
    public void ThenTheDefinedKeyKeepsTheConsumersValue(string version)
    {
        DefinedKeys(version).Should().Contain(static member => member.Key == ConsumerValuedKey);
        JsonNode.DeepEquals(ManifestAfter()[ConsumerValuedKey], _consumerValue).Should().BeTrue();
    }

    [Then("every other key version {word} defines is added")]
    public void ThenEveryOtherDefinedKeyIsAdded(string version)
    {
        var before = JsonNode.Parse(_filesBefore![SpecTree.ManifestPath])!.AsObject();
        var after = ManifestAfter();
        var lacking = DefinedKeys(version).Where(member => !before.ContainsKey(member.Key)).ToList();
        lacking.Should().NotBeEmpty();
        foreach (var (key, value) in lacking)
        {
            JsonNode.DeepEquals(after[key], value).Should().BeTrue(key);
        }
    }

    [Then("the manifest declares no epic grammar")]
    public void ThenTheManifestDeclaresNoEpicGrammar()
    {
        _exitCode.Should().Be(0, _stderr);
        ManifestAfter()["identifiers"]?["epic"].Should().BeNull();
    }

    [Then("the standard output names version {word} and each file rewritten")]
    public void ThenTheStandardOutputNamesTheVersionAndEachFileRewritten(string version)
    {
        _exitCode.Should().Be(0, _stderr);
        var lines = OutputLines();
        var rewritten = RewrittenFiles();
        rewritten.Should().NotBeEmpty();
        lines.Should().Contain(line => Regex.IsMatch(line, Naming(version)));
        foreach (var path in rewritten)
        {
            lines.Should().Contain(line => Regex.IsMatch(line, Naming(path)), path);
        }
    }

    [Then("the standard output carries no diff of any file")]
    public void ThenTheStandardOutputCarriesNoDiff()
    {
        var pin = ManifestAfter()["schemaVersion"]!.GetValue<string>();
        var rewritten = RewrittenFiles();
        OutputLines().Should().OnlyContain(line =>
            Regex.IsMatch(line, Naming(pin)) || rewritten.Any(path => Regex.IsMatch(line, Naming(path))));
    }

    [Then("the standard output names version {word} and the manifest")]
    public void ThenTheStandardOutputNamesTheVersionAndTheManifest(string version)
    {
        var lines = OutputLines();
        lines.Should().Contain(line => Regex.IsMatch(line, Naming(version)));
        lines.Should().Contain(static line => Regex.IsMatch(line, Naming(SpecTree.ManifestPath)));
    }

    private static List<KeyValuePair<string, JsonNode?>> DefinedKeys(string version) =>
        JsonNode.Parse(InitWriter.ShippingCopy(typeof(InitWriter).Assembly)[$"manifest/v{version}/manifest.json"])!.AsObject()
            .Where(static member => member.Key is not ("schemaVersion" or "identifiers") && !member.Key.StartsWith('$'))
            .ToList();

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

    private JsonObject ManifestAfter() => JsonNode.Parse(File.ReadAllBytes(Path.Combine(Tree.Root, SpecTree.ManifestPath)))!.AsObject();

    private List<string> OutputLines() =>
        _stdout.Split('\n').Select(static line => line.TrimEnd('\r')).Where(static line => line.Length > 0).ToList();

    private List<string> RewrittenFiles()
    {
        var after = SnapshotFiles();
        return after.Where(pair => !_filesBefore!.TryGetValue(pair.Key, out var bytes) || !pair.Value.SequenceEqual(bytes))
            .Select(static pair => pair.Key)
            .ToList();
    }

    private const string ConsumerValuedKey = "exclusions";

    private JsonArray _consumerValue = [];
}
