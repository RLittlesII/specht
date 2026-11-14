using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Tool.Features.Init;

namespace Specht.Acceptance.Versioning;

/// <summary>
/// Item 0438 binds B-051's schema 0.4.0 boundary and B-155 to B-161 over the Background root, pinned to 0.4.0, or to 0.3.1
/// for B-160's boundary, whose manifest holds no <c>ids</c>, no <c>mint</c> and no <c>identifiers</c> grammar but the task,
/// Feature and epic ones. Each premise writes the id settings it names into that manifest and keeps what it wrote; the built
/// tool's <c>upgrade</c>, or its <c>schema pin</c>, runs over the root, and each outcome is read from the manifest after the
/// run, its standard error and its exit code. B-161's accepted claim ids are the claim record the engine's reader takes from
/// the root after the run, compared with the record schema 0.4.0's shipped manifest holds. A <c>mint</c> entry for epic, Feature
/// or task ids, names outside the seven <c>0001-F19</c> C-13 types, is written under the lower-case name the manifest's
/// <c>identifiers</c> uses, and its refusal is read as that entry named <c>mint.&lt;name&gt;</c> and no <c>ids.</c> member named.
/// </summary>
public sealed partial class VersioningSteps
{
    [Given("its manifest still opts claim ids in to Nano ID under the mint key")]
    public void GivenItsManifestStillOptsClaimIdsInUnderTheMintKey() => OptIn("claim", new JsonObject { ["strategy"] = "nanoid" });

    [Given("^its manifest declares the customised (\\S+) grammar \"([^\"]*)\" under identifiers$")]
    public void GivenItsManifestDeclaresTheCustomisedGrammar(string type, string grammar) => Declare(type, grammar);

    [Given("^its manifest opts claim ids in to Nano ID with a size of (\\d+) and the prefix \"([^\"]*)\"$")]
    public void GivenItsManifestOptsClaimIdsInWithASizeAndAPrefix(int size, string prefix) =>
        OptIn("claim", new JsonObject { ["strategy"] = "nanoid", ["nanoid"] = new JsonObject { ["size"] = size }, ["prefix"] = prefix });

    [Given("^its manifest's mint entry for claim ids is the text \"([^\"]*)\" rather than a set of settings$")]
    public void GivenItsManifestsMintEntryForClaimIdsIsText(string text) => OptIn("claim", JsonValue.Create(text));

    [Given("^its manifest opts (epic|Feature|task) ids in to Nano ID under the mint key$")]
    public void GivenItsManifestOptsATypeOutsideTheSevenInUnderTheMintKey(string type) =>
        OptIn(type.ToLowerInvariant(), new JsonObject { ["strategy"] = "nanoid" });

    [Given("its manifest declares claim, constraint and open question grammars under identifiers")]
    public void GivenItsManifestDeclaresClaimConstraintAndOpenQuestionGrammars()
    {
        Declare("claim", "^B-[0-9]{4}$");
        Declare("constraint", "^CON-[0-9]+$");
        Declare("openQuestion", "^Q-[0-9]+$");
    }

    [Given("its manifest opts ADR numbers in to Nano ID")]
    public void GivenItsManifestOptsAdrNumbersIn() => OptIn("adr", new JsonObject { ["strategy"] = "nanoid" });

    [Given("its manifest declares task, Feature and epic grammars and a claim grammar under identifiers")]
    public void GivenItsManifestDeclaresTaskFeatureEpicAndClaimGrammars()
    {
        Declare("task", "^(?<epic>[0-9]{4})-(?<sequence>[0-9]{3})$");
        Declare("feature", "^F[0-9]+$");
        Declare("epic", "^[0-9]{4}$");
        Declare("claim", "^B-[0-9]{4}$");
    }

    [Given("its manifest lowers one rule to a warning")]
    public void GivenItsManifestLowersOneRuleToAWarning()
    {
        _consumerRules = new JsonObject { ["SPEC4004"] = "warning" };
        EditManifest(manifest => manifest["rules"] = _consumerRules.DeepClone());
    }

    [Given("^its manifest declares the claim grammar \"([^\"]*)\" under identifiers, with no anchors$")]
    public void GivenItsManifestDeclaresAnUnanchoredClaimGrammar(string grammar) => Declare("claim", grammar);

    [Given("^its manifest opts decision numbers in to Nano ID with the prefix \"([^\"]*)\" and declares no decision grammar$")]
    public void GivenItsManifestOptsDecisionNumbersInWithAPrefix(string prefix)
    {
        OptIn("decision", new JsonObject { ["strategy"] = "nanoid", ["prefix"] = prefix });
        ManifestAfter()["identifiers"]?["decision"].Should().BeNull();
    }

    [Given("^its manifest declares the claim and scenario grammars \"([^\"]*)\" under identifiers$")]
    public void GivenItsManifestDeclaresTheClaimAndScenarioGrammars(string grammar)
    {
        Declare("claim", grammar);
        Declare("scenario", grammar);
    }

    [Given("its manifest holds a claim record under ids and a claim grammar under identifiers")]
    public void GivenItsManifestHoldsAClaimRecordAndAClaimGrammar()
    {
        HoldClaimRecord();
        Declare("claim", "^B-[0-9]{5}$");
    }

    [Given("its manifest holds a claim record under ids and no old id setting")]
    public void GivenItsManifestHoldsAClaimRecordAndNoOldIdSetting()
    {
        HoldClaimRecord();
        var manifest = ManifestAfter();
        manifest.Should().NotContainKey("mint");
        (manifest["identifiers"] as JsonObject ?? []).Select(static member => member.Key).Should().BeSubsetOf(new[] { "task", "feature", "epic" });
    }

    [Given("its manifest opts claim ids in to Nano ID and declares a claim grammar under identifiers")]
    public void GivenItsManifestOptsClaimIdsInAndDeclaresAClaimGrammar()
    {
        OptIn("claim", new JsonObject { ["strategy"] = "nanoid" });
        Declare("claim", "^B-[0-9]{4}$");
    }

    [Given("^its manifest declares under identifiers the claim grammar schema (\\S+) ships$")]
    public void GivenItsManifestDeclaresTheShippedClaimGrammar(string version) =>
        Declare("claim", ShippedManifest(version)["identifiers"]!["claim"]!.GetValue<string>());

    [Then("the manifest pins schema version {word}")]
    public void ThenTheManifestPinsSchemaVersion(string version)
    {
        _exitCode.Should().Be(0, _stderr);
        ManifestAfter()["schemaVersion"]!.GetValue<string>().Should().Be(version);
    }

    [Then("the opt-in is as the consumer set it")]
    public void ThenTheOptInIsAsTheConsumerSetIt() => JsonNode.DeepEquals(ManifestAfter()["mint"], _consumerMint).Should().BeTrue();

    [Then("^the manifest's (\\S+) record has the plain text \"([^\"]*)\" and the id \"([^\"]*)\"$")]
    public void ThenTheManifestsRecordHasThePlainTextAndTheId(string type, string plain, string id)
    {
        var record = MovedRecord(type);
        record["plain"]!.GetValue<string>().Should().Be(plain);
        record["id"]!.GetValue<string>().Should().Be(id);
    }

    [Then("^the manifest's claim record opts claim ids in to Nano ID with a size of (\\d+)$")]
    public void ThenTheClaimRecordOptsClaimIdsInWithASize(int size)
    {
        var mint = MovedRecord("claim")["mint"]!;
        mint["strategy"]!.GetValue<string>().Should().Be("nanoid");
        mint["nanoid"]!["size"]!.GetValue<int>().Should().Be(size);
    }

    [Then("the claim record's opt-in names no prefix")]
    public void ThenTheClaimRecordsOptInNamesNoPrefix() => MovedRecord("claim")["mint"]!.AsObject().Should().NotContainKey("prefix");

    [Then("^the manifest's claim record carries the text \"([^\"]*)\" as its mint entry, as the consumer wrote it$")]
    public void ThenTheClaimRecordCarriesTheTextAsItsMintEntry(string text)
    {
        var mint = MovedRecord("claim").AsObject()["mint"];
        mint.Should().BeAssignableTo<JsonValue>();
        mint!.GetValue<string>().Should().Be(text);
    }

    [Then("the manifest holds no mint key")]
    public void ThenTheManifestHoldsNoMintKey()
    {
        _exitCode.Should().Be(0, _stderr);
        ManifestAfter().Should().NotContainKey("mint");
    }

    [Then("it declares no claim, constraint or open question grammar under identifiers")]
    public void ThenItDeclaresNoClaimConstraintOrOpenQuestionGrammar() =>
        (ManifestAfter()["identifiers"] as JsonObject ?? []).Select(static member => member.Key)
            .Should().NotContain(new[] { "claim", "constraint", "openQuestion" });

    [Then("the task, Feature and epic grammars and the warning are as the consumer set them")]
    public void ThenTheTaskFeatureAndEpicGrammarsAndTheWarningAreAsSet()
    {
        _exitCode.Should().Be(0, _stderr);
        var after = ManifestAfter();
        foreach (var type in new[] { "task", "feature", "epic" })
        {
            after["identifiers"]![type]!.GetValue<string>().Should().Be(_consumerGrammars[type], type);
        }

        JsonNode.DeepEquals(after["rules"], _consumerRules).Should().BeTrue(after["rules"]?.ToJsonString());
    }

    [Then("^the standard error names the (claim grammar|decision prefix|claim and scenario grammars)$")]
    public void ThenTheStandardErrorNamesTheMember(string member)
    {
        foreach (var named in MemberNames[member])
        {
            ThenTheStandardErrorNames(named);
        }
    }

    [Then("^the standard error names the (epic|Feature|task) mint entry as the consumer wrote it$")]
    public void ThenTheStandardErrorNamesTheMintEntryAsWritten(string type)
    {
        var name = type.ToLowerInvariant();
        _consumerMint.Should().ContainKey(name);
        ThenTheStandardErrorNames($"mint.{name}");
    }

    [Then("the standard error names no ids record")]
    public void ThenTheStandardErrorNamesNoIdsRecord() => _stderr.Should().NotMatchRegex(@"(?<![\w.])ids\.\w");

    [Then("the claim record is as the consumer set it")]
    public void ThenTheClaimRecordIsAsTheConsumerSetIt() => JsonNode.DeepEquals(MovedRecord("claim"), _consumerClaimRecord).Should().BeTrue();

    [Then("the manifest holds no other ids record")]
    public void ThenTheManifestHoldsNoOtherIdsRecord() =>
        ManifestAfter()["ids"]!.AsObject().Select(static member => member.Key).Should().Equal("claim");

    [Then("the opt-in and the claim grammar are as the consumer set them")]
    public void ThenTheOptInAndTheClaimGrammarAreAsSet()
    {
        _exitCode.Should().Be(0, _stderr);
        var after = ManifestAfter();
        JsonNode.DeepEquals(after["mint"], _consumerMint).Should().BeTrue(after["mint"]?.ToJsonString());
        after["identifiers"]!["claim"]!.GetValue<string>().Should().Be(_consumerGrammars["claim"]);
    }

    [Then("the manifest holds no claim record under ids")]
    public void ThenTheManifestHoldsNoClaimRecordUnderIds()
    {
        _exitCode.Should().Be(0, _stderr);
        ManifestAfter()["ids"]?["claim"].Should().BeNull();
    }

    [Then("^the claim IDs the check accepts are those schema (\\S+)'s shipped claim record admits$")]
    public void ThenTheClaimIdsTheCheckAcceptsAreThoseTheShippedRecordAdmits(string version)
    {
        var shipped = ShippedManifest(version)["ids"]!["claim"]!;
        var expected = new SpechtIdRecord(
            shipped["plain"]!.GetValue<string>(),
            shipped["form"]?.GetValue<string>(),
            shipped["id"]!.GetValue<string>());
        EngineServices.Reader().ReadManifest(Tree.Root).Ids!["claim"].Should().Be(expected);
    }

    private static JsonObject ShippedManifest(string version) =>
        JsonNode.Parse(InitWriter.ShippingCopy(typeof(InitWriter).Assembly)[$"manifest/v{version}/manifest.json"])!.AsObject();

    private void OptIn(string type, JsonNode entry)
    {
        _consumerMint[type] = entry.DeepClone();
        EditManifest(manifest => manifest["mint"] = _consumerMint.DeepClone());
    }

    private void Declare(string type, string grammar)
    {
        _consumerGrammars[type] = grammar;
        EditManifest(manifest =>
        {
            if (manifest["identifiers"] is not JsonObject identifiers)
            {
                identifiers = [];
                manifest["identifiers"] = identifiers;
            }

            identifiers[type] = grammar;
        });
    }

    private void HoldClaimRecord()
    {
        _consumerClaimRecord = new JsonObject { ["plain"] = "B-{id}", ["id"] = "[0-9]{4}" };
        EditManifest(manifest => manifest["ids"] = new JsonObject { ["claim"] = _consumerClaimRecord.DeepClone() });
    }

    private JsonNode MovedRecord(string type)
    {
        _exitCode.Should().Be(0, _stderr);
        return ManifestAfter()["ids"]?[type] ?? throw new InvalidOperationException($"The manifest holds no ids.{type} record.");
    }

    private static readonly Dictionary<string, string[]> MemberNames = new(StringComparer.Ordinal)
    {
        ["claim grammar"] = ["identifiers.claim"],
        ["decision prefix"] = ["mint.decision.prefix"],
        ["claim and scenario grammars"] = ["identifiers.claim", "identifiers.scenario"],
    };

    private readonly JsonObject _consumerMint = [];

    private readonly Dictionary<string, string> _consumerGrammars = new(StringComparer.Ordinal);

    private JsonObject _consumerRules = [];

    private JsonObject _consumerClaimRecord = [];
}
