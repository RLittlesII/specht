using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Rules;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Acceptance.Init;

public sealed partial class InitSteps
{
    [Then("the manifest written sets a level for every rule of the version it pins")]
    public void ThenTheManifestWrittenSetsALevelForEveryRuleOfTheVersionItPins()
    {
        var (rules, version) = WrittenRuleDefaults();
        rules.Select(static pair => pair.Key).Should().Contain(version.RuleIds);
    }

    [Then("each level the manifest written sets is that rule's own default severity")]
    public void ThenEachLevelTheManifestWrittenSetsIsThatRulesOwnDefaultSeverity()
    {
        var (rules, version) = WrittenRuleDefaults();
        rules.Should().NotBeEmpty("the manifest exposes the pinned rules");
        foreach (var (id, level) in rules)
        {
            version.RuleIds.Should().Contain(id);
            var page = RulePage.Read(version.Number, id, version.RulePages[id]);
            var severity = page.Value("Default severity");
            severity.Should().NotBeNullOrEmpty($"{id} documents its default level");
            level!.GetValue<string>().Should().Be(severity!.ToLowerInvariant(), id);
        }
    }

    [Then("the manifest written sets a level for no rule outside the version it pins")]
    public void ThenTheManifestWrittenSetsALevelForNoRuleOutsideTheVersionItPins()
    {
        var (rules, version) = WrittenRuleDefaults();
        rules.Select(static pair => pair.Key).Except(version.RuleIds, StringComparer.Ordinal).Should().BeEmpty();
    }

    private (JsonObject Rules, SchemaVersion Version) WrittenRuleDefaults()
    {
        var manifest = JsonNode.Parse(File.ReadAllText(Written(Manifest)))!.AsObject();
        var pin = manifest["schemaVersion"]!.GetValue<string>();
        var version = EngineServices.Versions().Versions.Single(version => version.Number.ToString() == pin);
        var rules = manifest["rules"] as JsonObject;
        rules.Should().NotBeNull("init exposes rule defaults in the written manifest");
        return (rules!, version);
    }
}
