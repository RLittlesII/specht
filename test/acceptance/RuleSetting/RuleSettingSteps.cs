using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Acceptance.RuleSetting;

/// <summary>Exercises the rule setter against a synthetic consumer's unchanged file tree.</summary>
[Binding]
[Scope(Feature = "Setting a rule's level")]
public sealed partial class RuleSettingSteps
{
    [Given("a repository root holding a manifest that pins schema version {word}")]
    public void GivenRoot(string version)
    {
        _tree = new SpecTree();
        Tree.WriteRaw(SpecTree.ManifestPath, $$"""{"schemaVersion":"{{version}}","$comment":"sentinel"}""");
        CaptureManifest();
    }

    [Given("the manifest sets the out-of-order-section rule to {word}")]
    public void GivenLevel(string level)
    {
        var manifest = ReadManifest();
        manifest["rules"] = new JsonObject { ["SPEC010"] = level };
        Tree.WriteRaw(SpecTree.ManifestPath, manifest.ToJsonString());
        CaptureManifest();
    }

    [Given("the manifest sets a level for the missing-frontmatter rule and none for the out-of-order-section rule")]
    public void GivenOtherRule()
    {
        var manifest = ReadManifest();
        manifest["rules"] = new JsonObject { ["SPEC001"] = "off" };
        Tree.WriteRaw(SpecTree.ManifestPath, manifest.ToJsonString());
        CaptureManifest();
    }

    [Given("the manifest carries no rule settings")]
    public void GivenNoRules()
    {
        var manifest = ReadManifest();
        manifest.Remove("rules");
        Tree.WriteRaw(SpecTree.ManifestPath, manifest.ToJsonString());
        CaptureManifest();
    }

    [Given("the manifest carries null rule settings")]
    public void GivenNullRules()
    {
        var manifest = ReadManifest();
        manifest["rules"] = null;
        Tree.WriteRaw(SpecTree.ManifestPath, manifest.ToJsonString());
        CaptureManifest();
    }

    [When("the out-of-order-section rule is set to {word}")]
    public void WhenSet(string level) => Run("SPEC010", level);

    [Then("the manifest sets the out-of-order-section rule to {word}")]
    public void ThenLevel(string level) => ReadManifest()["rules"]!["SPEC010"]!.GetValue<string>().Should().Be(level);

    [Then("the manifest differs from before only in that rule's level")]
    public void ThenOnlyValue() => File.ReadAllText(ManifestPath).Should()
        .Be(System.Text.Encoding.UTF8.GetString(_manifestBefore).Replace("\"error\"", "\"warning\"", StringComparison.Ordinal));

    [Then("the manifest is byte-for-byte as it was")]
    public void ThenUnchanged()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        File.ReadAllBytes(ManifestPath).Should().Equal(_manifestBefore);
    }

    [Then("every other setting in the manifest keeps its value")]
    public void ThenOtherValues()
    {
        var before = JsonNode.Parse(_manifestBefore)!.AsObject();
        var after = ReadManifest();
        before.Remove("rules");
        after.Remove("rules");
        JsonNode.DeepEquals(before, after).Should().BeTrue();
        var previousRules = JsonNode.Parse(_manifestBefore)!["rules"] as JsonObject;
        foreach (var entry in previousRules ?? [])
        {
            if (entry.Key != "SPEC010")
            {
                JsonNode.DeepEquals(entry.Value, ReadManifest()["rules"]![entry.Key]).Should().BeTrue();
            }
        }
    }

    [Then("the manifest's rule settings hold the out-of-order-section rule at warning and nothing else")]
    public void ThenSingleEntry()
    {
        var rules = ReadManifest()["rules"]!.AsObject();
        rules.Should().ContainSingle();
        rules["SPEC010"]!.GetValue<string>().Should().Be("warning");
    }

    [Then("the check accepts the manifest")]
    public void ThenManifestAccepted()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        var parser = EngineServices.Parser();
        parser.PinnedVersion(parser.Parse(File.ReadAllText(ManifestPath))).RuleIds.Should().Contain("SPEC010");
        var check = AcceptanceTool.Launch(Tree.Root);
        check.ExitCode.Should().BeOneOf(0, 1);
    }

    [AfterScenario]
    public void DisposeTree() => _tree?.Dispose();

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No root was prepared.");
    private string ManifestPath => Path.Combine(Tree.Root, SpecTree.ManifestPath);
    private JsonObject ReadManifest() => JsonNode.Parse(File.ReadAllText(ManifestPath))!.AsObject();
    private void CaptureManifest() => _manifestBefore = File.ReadAllBytes(ManifestPath);
    private SpecTree? _tree;
    private byte[] _manifestBefore = [];
}
