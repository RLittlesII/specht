using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Rules;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Acceptance.Rules;

/// <summary>Exercises the text command and its catalogue; JSON bindings belong to the later JSON slice.</summary>
[Binding]
[Scope(Feature = "The rules command")]
public sealed partial class RuleEntriesSteps
{
    [Given("a repository root holding a manifest that pins schema version {word}")]
    public void GivenRepositoryRoot(string pin)
    {
        _tree = new SpecTree();
        GivenManifestPins(pin);
        _manifestBefore = File.ReadAllBytes(Path.Combine(Tree.Root, SpecTree.ManifestPath));
    }

    [Given("the manifest pins schema version {word}")]
    public void GivenManifestPins(string pin)
    {
        var path = Path.Combine(Tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest["schemaVersion"] = pin;
        File.WriteAllText(path, manifest.ToJsonString());
        _version = EngineServices.Versions().Versions.Single(version => version.Number.ToString() == pin);
    }

    [Given("the manifest sets the out-of-order-section rule to {word}")]
    public void GivenManifestSetsLevel(string level)
    {
        var path = Path.Combine(Tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest["rules"] = new JsonObject { [SectionRule] = level };
        File.WriteAllText(path, manifest.ToJsonString());
        _manifestBefore = File.ReadAllBytes(path);
    }

    [Given("the manifest sets no level for the out-of-order-section rule")]
    public void GivenManifestOmitsLevel()
    {
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath)))!.AsObject();
        manifest["rules"]?.AsObject().ContainsKey(SectionRule).Should().NotBe(true);
    }

    [When("the rules are listed")]
    public void WhenRulesAreListed() => _result = AcceptanceTool.Launch(Tree.Root, "rules");

    [When("the out-of-order-section rule is explained")]
    public void WhenSectionRuleIsExplained()
    {
        var (stdout, stderr, exitCode) = AcceptanceTool.Launch(Tree.Root, "--root", ".", "--explain", SectionRule);
        exitCode.Should().Be(0, stderr);
        _explanation = RulePage.Read(_version!.Number, SectionRule, stdout);
    }

    [Then("the listing has one entry for each rule of version {word} and no other entry")]
    public void ThenListingHasPinnedVocabulary(string pin)
    {
        _version!.Number.ToString().Should().Be(pin);
        Entries.Select(static entry => entry[0]).Should().BeEquivalentTo(_version.RuleIds);
        Entries.Select(static entry => entry[0]).Should().OnlyHaveUniqueItems();
    }

    [Then("the out-of-order-section rule is listed in the structure family")]
    public void ThenSectionRuleFamily() => Entry(SectionRule)[1].Should().Be("Structure");

    [Then("the out-of-order-section rule is listed with the default severity {word}")]
    public void ThenSectionRuleDefault(string level) =>
        Entry(SectionRule)[2].Should().Be(level);

    [Then("the retired-claim rule is listed with the default severity off")]
    public void ThenRetiredClaimDefault() => Entry("SPEC035")[2].Should().Be("off");

    [Then("the out-of-order-section rule is listed with the effective level {word}")]
    public void ThenSectionRuleEffectiveLevel(string level) =>
        Entry(SectionRule)[3].Should().Be(level);

    [Then("the listing has an entry for the out-of-order-section rule")]
    public void ThenListingRetainsSectionRule() => Entries.Should().ContainSingle(static entry => entry[0] == SectionRule);

    [Then("the family and the default severity listed for it are the ones its explanation gives")]
    public void ThenListingAgreesWithExplanation()
    {
        Entry(SectionRule)[1].Should().Be(_explanation!.Value("Family"));
        Entry(SectionRule)[2].Should().Be(_explanation!.Value("Default severity")?.ToLowerInvariant());
    }

    [Then("each entry's rule id follows the one before it in ordinal order")]
    public void ThenListingIsOrdinallyOrdered()
    {
        Entries.Should().NotBeEmpty();
        Entries.Select(static entry => entry[0]).Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    [AfterScenario]
    public void DisposeTree() => _tree?.Dispose();

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No repository root was prepared.");

    private string[][] Entries => _result.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(static line => line.TrimEnd('\r').Split('\t')).ToArray();

    private string[] Entry(string id) => Entries.Should().ContainSingle(entry => entry[0] == id).Which;

    private const string SectionRule = "SPEC010";
    private SpecTree? _tree;
    private SchemaVersion? _version;
    private RulePage? _explanation;
    private (string Stdout, string Stderr, int ExitCode) _result;
    private byte[] _manifestBefore = [];
}
