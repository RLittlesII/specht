using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [When("specht schema pin {word} runs")]
    public void WhenSchemaPinRuns(string version)
    {
        _pinTarget = version;
        _filesBefore ??= SnapshotFiles();
        (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Tree.Root, "schema", "pin", version);
    }

    [When("specht schema pin {word} runs against a path that is not a directory")]
    public void WhenSchemaPinRunsAgainstAMissingRoot(string version)
    {
        _filesBefore ??= SnapshotFiles();
        (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Tree.Root, "schema", "pin", version, "--root", "no-such-root");
    }

    [Given("the root's manifest is not valid for a reason other than its schema version")]
    [Given("the root's manifest is not valid")]
    public void GivenTheManifestHasInvalidSettings() =>
        EditManifest(static manifest => manifest["rules"] = new JsonObject { ["SPEC031"] = "invalid" });

    [Given("the manifest uses encoding {word}")]
    public void GivenTheManifestUsesEncoding(string name)
    {
        _manifestEncoding = Encoding.GetEncoding(name);
        var path = Path.Combine(Tree.Root, SpecTree.ManifestPath);
        var text = File.ReadAllText(path);
        File.WriteAllBytes(path, [.. _manifestEncoding.GetPreamble(), .. _manifestEncoding.GetBytes(text)]);
    }

    [Given("a snapshot of the manifest")]
    public void GivenASnapshotOfTheManifest() => _manifestBefore = File.ReadAllBytes(Path.Combine(Tree.Root, SpecTree.ManifestPath));

    [Given("a snapshot of every file under the root")]
    public void GivenASnapshotOfEveryFile() => _filesBefore = SnapshotFiles();

    [Given("its manifest lowers one rule to a warning, disables another and renames the claims section")]
    public void GivenTheManifestHasCustomSettings() => EditManifest(static manifest =>
    {
        manifest["rules"] = new JsonObject { ["SPEC031"] = "warning", ["SPEC020"] = "off" };
        manifest["sections"]!.AsArray()[2] = "3. Release criteria";
        manifest["roles"]!["claims"] = "3. Release criteria";
    });

    [Given("its manifest configures claim tags")]
    public void GivenTheManifestConfiguresClaimTags() =>
        EditManifest(static manifest => manifest["claim"] = new JsonObject { ["tag"] = "@req:{claim}", ["tagIgnoreCase"] = false });

    [Given("the root holds specifications, records and companions")]
    public void GivenTheRootHoldsDocuments()
    {
        Tree.WriteRaw("examples/.spec/README.md", "synthetic specification");
        Tree.WriteRaw("examples/.spec/example.feature", "Feature: synthetic companion");
        Tree.WriteRaw("examples/.spec/decisions/example.md", "synthetic record");
    }

    [Given("the root is pinned to version {word} and its frontmatter schemas and templates are version {word}'s")]
    public void GivenTheRootAndFilesHaveVersions(string pin, string files)
    {
        GivenTheManifestPinsVersion(files);
        var schemas = Path.Combine(Tree.Root, SpecTree.ConfigurationFolder, "schema");
        Directory.Delete(schemas, recursive: true);
        var result = AcceptanceTool.Launch(Tree.Root, "init", "--all");
        result.ExitCode.Should().Be(0, result.Stderr);
        GivenTheManifestPinsVersion(pin);
    }

    [Then("the manifest pins version {word}")]
    public void ThenTheManifestPinsVersion(string version) => ThenTheWrittenManifestPinsVersion(version);

    [Then("the manifest differs from the snapshot only in the version it pins")]
    public void ThenOnlyThePinChangedInTheManifest()
    {
        var preamble = _manifestEncoding.GetPreamble();
        var original = _manifestEncoding.GetString(_manifestBefore!.AsSpan(preamble.Length));
        var before = JsonNode.Parse(original)!;
        var needle = "\"schemaVersion\":" + before["schemaVersion"]!.ToJsonString();
        original.Should().Contain(needle);
        var expected = original.Replace(needle, "\"schemaVersion\":" + JsonSerializer.Serialize(_pinTarget), StringComparison.Ordinal);
        File.ReadAllBytes(Path.Combine(Tree.Root, SpecTree.ManifestPath)).Should().Equal([.. preamble, .. _manifestEncoding.GetBytes(expected)]);
    }

    [Then("the only file changed is the manifest")]
    public void ThenOnlyTheManifestChanged()
    {
        var after = SnapshotFiles();
        after.Keys.Should().BeEquivalentTo(_filesBefore!.Keys);
        after.Where(pair => !pair.Value.SequenceEqual(_filesBefore[pair.Key])).Select(static pair => pair.Key)
            .Should().Equal(SpecTree.ManifestPath);
    }

    [Then("the frontmatter schemas and the templates are still version {word}'s")]
    public void ThenTheSchemaAndTemplateBytesAreUnchanged(string version)
    {
        _ = version;
        var after = SnapshotFiles();
        foreach (var entry in _filesBefore!.Where(static entry => entry.Key != SpecTree.ManifestPath))
        {
            after[entry.Key].Should().Equal(entry.Value);
        }
    }

    [Then("every other manifest key keeps its value")]
    public void ThenEveryOtherManifestKeyKeepsItsValue()
    {
        var before = JsonNode.Parse(_filesBefore![SpecTree.ManifestPath])!.AsObject();
        var after = JsonNode.Parse(File.ReadAllBytes(Path.Combine(Tree.Root, SpecTree.ManifestPath)))!.AsObject();
        before.Remove("schemaVersion");
        after.Remove("schemaVersion");
        JsonNode.DeepEquals(before, after).Should().BeTrue();
    }

    [Then("no file under the root has changed")]
    [Then("nothing is written")]
    public void ThenNoFileHasChanged()
    {
        var after = SnapshotFiles();
        after.Keys.Should().BeEquivalentTo(_filesBefore!.Keys);
        foreach (var entry in _filesBefore)
        {
            after[entry.Key].Should().Equal(entry.Value);
        }
    }

    [Then("the output confirms version {word} and the manifest")]
    public void ThenTheOutputConfirmsThePin(string version) => _stdout.Should().Contain(version).And.Contain(SpecTree.ManifestPath);

    private Dictionary<string, byte[]> SnapshotFiles() => Directory.GetFiles(Tree.Root, "*", SearchOption.AllDirectories)
        .ToDictionary(file => Path.GetRelativePath(Tree.Root, file).Replace('\\', '/'), File.ReadAllBytes, StringComparer.Ordinal);

    private Dictionary<string, byte[]>? _filesBefore;
    private byte[]? _manifestBefore;
    private string? _pinTarget;
    private Encoding _manifestEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
}
