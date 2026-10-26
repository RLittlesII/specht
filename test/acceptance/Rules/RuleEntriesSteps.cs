using System.IO.Abstractions;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Rules;
using Specht.Tests.Rules;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Acceptance.Rules;

/// <summary>Exercises the engine catalogue for 0280; the command and JSON bindings belong to 0281 and 0282.</summary>
[Binding]
[Scope(Feature = "The rules command")]
public sealed class RuleEntriesSteps
{
    [Given("a repository root holding a manifest that pins schema version {word}")]
    public void GivenRepositoryRoot(string pin)
    {
        _tree = new SpecTree();
        GivenManifestPins(pin);
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
    }

    [Given("the manifest sets no level for the out-of-order-section rule")]
    public void GivenManifestOmitsLevel()
    {
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath)))!.AsObject();
        manifest["rules"]?.AsObject().ContainsKey(SectionRule).Should().NotBe(true);
    }

    [When("the rules are listed")]
    public void WhenRulesAreListed() =>
        _entries = new RuleEntriesReader(EngineServices.Schemas(new FileSystem()), EngineServices.Parser()).Read(Tree.Root);

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
        _entries.Select(static entry => entry.Id).Should().BeEquivalentTo(_version.RuleIds);
        _entries.Select(static entry => entry.Id).Should().OnlyHaveUniqueItems();
    }

    [Then("the out-of-order-section rule is listed in the structure family")]
    public void ThenSectionRuleFamily() => Entry(SectionRule).Family.Should().Be("Structure");

    [Then("the out-of-order-section rule is listed with the default severity {word}")]
    public void ThenSectionRuleDefault(string level) =>
        Entry(SectionRule).DefaultSeverity.ToString().ToLowerInvariant().Should().Be(level);

    [Then("the retired-claim rule is listed with the default severity off")]
    public void ThenRetiredClaimDefault() => Entry("SPEC035").DefaultSeverity.Should().Be(SpecRuleLevel.Off);

    [Then("the out-of-order-section rule is listed with the effective level {word}")]
    public void ThenSectionRuleEffectiveLevel(string level) =>
        Entry(SectionRule).EffectiveLevel.ToString().ToLowerInvariant().Should().Be(level);

    [Then("the listing has an entry for the out-of-order-section rule")]
    public void ThenListingRetainsSectionRule() => _entries.Should().ContainSingle(static entry => entry.Id == SectionRule);

    [Then("the family and the default severity listed for it are the ones its explanation gives")]
    public void ThenListingAgreesWithExplanation()
    {
        Entry(SectionRule).Family.Should().Be(_explanation!.Value("Family"));
        Entry(SectionRule).DefaultSeverity.ToString().Should().Be(_explanation.Value("Default severity"));
    }

    [Then("each entry's rule id follows the one before it in ordinal order")]
    public void ThenListingIsOrdinallyOrdered()
    {
        _entries.Should().NotBeEmpty();
        _entries.Select(static entry => entry.Id).Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    [AfterScenario]
    public void DisposeTree() => _tree?.Dispose();

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No repository root was prepared.");

    private RuleEntry Entry(string id) => _entries.Should().ContainSingle(entry => entry.Id == id).Which;

    private const string SectionRule = "SPEC010";
    private SpecTree? _tree;
    private SchemaVersion? _version;
    private RulePage? _explanation;
    private IReadOnlyList<RuleEntry> _entries = [];
}
