using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Identifiers;

/// <summary>
/// Steps for minting from the <c>ids</c> record (<c>0001-F19</c> B-063 to B-077, item <c>0437</c>). "A manifest pinned to schema
/// 0.4.0" moves the pin and removes the top-level <c>mint</c> key and every <c>identifiers</c> member for one of the seven
/// record types, which <c>0001-F5</c> B-092 rejects at that pin; B-075's rows put one back. A type is opted in by
/// <c>ids.&lt;type&gt;.mint</c> alone, with no <c>prefix</c>: Nano ID over the lower-case base32 alphabet, 8 characters for
/// <c>adr</c> and <c>lesson</c> and 6 for a per-Feature type, the draws the shipped records admit (B-074). A record has a
/// <c>form</c>, a <c>plain</c> or an <c>id</c> only where a scenario names one, and leaves the rest to the shipped record. A
/// Feature that declares a claim prefix is <c>features/one</c> with the value under <c>claim_prefix</c>, the key the shipped
/// <c>0.4.0</c> manifest names, in its <c>.spec/README.md</c> frontmatter, and every later rewrite of that Feature keeps it. A
/// request "for that Feature" runs in process, as for items <c>0416</c> to <c>0419</c>; a request "for a Feature that
/// declares" a prefix or none launches the built tool, the Feature written with <c>ACTIVITY</c> or with no claim prefix. "The
/// check accepts it as a claim id" writes the minted id as the only row of that Feature's claims table and checks the tree in
/// process, and the claim rule reports nothing for that id.
/// </summary>
public sealed partial class IdGenerationSteps
{
    [Given("^a manifest pinned to schema (0\\.4\\.0) that opts (claim ids|constraint ids) in to Nano ID$")]
    public void GivenAManifestPinnedToSchemaThatOptsATypeInToNanoId(string version, string type)
    {
        GivenAManifestPinnedToSchema(version);
        GivenTheManifestOptsATypeInToNanoId(type);
    }

    [Given("^a Feature that declares the claim prefix \"(.*)\"$")]
    public void GivenAFeatureThatDeclaresTheClaimPrefix(string prefix) => DeclarePrefix($"\"{prefix}\"");

    [Given("^a Feature whose claim prefix is (white space alone|\"S-\", which gives its claims and scenarios one head)$")]
    public void GivenAFeatureWhoseClaimPrefixIs(string prefix) => DeclarePrefix(prefix == "white space alone" ? "\" \"" : "\"S-\"");

    [Given("^the manifest opts " + Kind + " in to Nano ID$")]
    public void GivenTheManifestOptsATypeInToNanoId(string type) =>
        Record(TypeOf(type), record => record["mint"] = new JsonObject
        {
            ["strategy"] = "nanoid",
            ["nanoid"] = new JsonObject { ["alphabet"] = Base32, ["size"] = TypeOf(type) is "adr" or "lesson" ? 8 : 6 },
        });

    [Given("^the manifest opts claim ids in to Nano ID with a size of (\\d+)$")]
    public void GivenTheManifestOptsClaimIdsInToNanoIdWithASizeOf(int size) =>
        Record("claim", record => record["mint"] = new JsonObject { ["strategy"] = "nanoid", ["nanoid"] = new JsonObject { ["size"] = size } });

    [Given("^the manifest opts (claim ids|scenario ids) in to Nano ID with an alphabet of the one letter \"(.)\"$")]
    public void GivenTheManifestOptsATypeInToNanoIdWithAnAlphabetOfTheOneLetter(string type, string letter) =>
        Record(TypeOf(type), record => record["mint"] = new JsonObject { ["strategy"] = "nanoid", ["nanoid"] = new JsonObject { ["alphabet"] = letter } });

    [Given("^the manifest opts claim ids in to Snowflake with an epoch of (\\d{4}-\\d{2}-\\d{2})$")]
    public void GivenTheManifestOptsClaimIdsInToSnowflakeWithAnEpochOf(string epoch) =>
        Record("claim", record => record["mint"] = new JsonObject { ["strategy"] = "snowflake", ["snowflake"] = new JsonObject { ["epoch"] = epoch } });

    [Given("^its " + Kind + " record has the form \"([^\"]*)\"$")]
    public void GivenItsRecordHasTheForm(string type, string form) => Record(TypeOf(type), record => record["form"] = form);

    [Given("^its " + Kind + " record has the form \"([^\"]*)\" and an id that admits (any text|only three digits|nine characters)$")]
    public void GivenItsRecordHasTheFormAndAnIdThatAdmits(string type, string form, string id) =>
        Record(TypeOf(type), record =>
        {
            record["form"] = form;
            record["id"] = id switch
            {
                "any text" => ".*",
                "only three digits" => "[0-9]{3}",
                _ => ".{9}",
            };
        });

    [Given("^its " + Kind + " record has the plain text \"([^\"]*)\"$")]
    public void GivenItsRecordHasThePlainText(string type, string plain) => Record(TypeOf(type), record => record["plain"] = plain);

    [Given("^its " + Kind + " record has no form$")]
    public void GivenItsRecordHasNoForm(string type) => Record(TypeOf(type), static record => record.Remove("form"));

    [Given("^its (other |)records leave their forms and plain texts to the shipped records$")]
    public void GivenItsRecordsLeaveTheirFormsAndPlainTextsToTheShippedRecords(string other)
    {
        var manifest = ReadManifest();
        foreach (var (_, record) in manifest["ids"] as JsonObject ?? [])
        {
            record!.AsObject().Remove("plain");
            if (other.Length == 0)
            {
                record.AsObject().Remove("form");
            }
        }

        WriteManifest(manifest);
    }

    [Given("^its only record for " + Kind + " opts it in to (Nano ID|Snowflake) with no size or encoding$")]
    public void GivenItsOnlyRecordForATypeOptsItInWithNoSizeOrEncoding(string type, string strategy)
    {
        var manifest = ReadManifest();
        manifest["ids"] = new JsonObject
        {
            [TypeOf(type)] = new JsonObject { ["mint"] = new JsonObject { ["strategy"] = strategy == "Nano ID" ? "nanoid" : "snowflake" } },
        };
        WriteManifest(manifest);
    }

    [Given("^it holds (a mint key that opts claim ids in|a claim grammar under identifiers|a claim record whose opt-in names a prefix)$")]
    public void GivenItHoldsAnOldSetting(string setting)
    {
        var manifest = ReadManifest();
        var optIn = new JsonObject { ["strategy"] = "nanoid", ["nanoid"] = new JsonObject { ["alphabet"] = Base32, ["size"] = 6 } };
        switch (setting)
        {
            case "a mint key that opts claim ids in":
                manifest["mint"] = new JsonObject { ["claim"] = optIn };
                break;
            case "a claim grammar under identifiers":
                manifest["identifiers"]!["claim"] = $"^B-[{Base32}]{{6}}$";
                manifest["ids"] = new JsonObject { ["claim"] = new JsonObject { ["mint"] = optIn } };
                break;
            default:
                optIn["prefix"] = "B-";
                manifest["ids"] = new JsonObject { ["claim"] = new JsonObject { ["mint"] = optIn } };
                break;
        }

        WriteManifest(manifest);
    }

    [When(
        "^(a decision number|a claim id|a constraint id|an open question id|a scenario id)"
        + " for a Feature that declares (no claim prefix|no prefix|a prefix) is requested$")]
    public void WhenATypeIdForAFeatureThatDeclaresAPrefixOrNoneIsRequested(string id, string prefix)
    {
        _frontmatter = prefix == "a prefix" ? new Dictionary<string, string>(StringComparer.Ordinal) { [ClaimPrefixKey] = "\"ACTIVITY\"" } : null;
        _request = Request(TypeOf(id), forFeature: true);
    }

    [When("^a claim id is requested for a Feature that declares no prefix$")]
    public void WhenAClaimIdIsRequestedForAFeatureThatDeclaresNoPrefix() => _request = Request("claim", forFeature: true);

    [Then("^the minted id begins with \"(.*)\"$")]
    public void ThenTheMintedIdBeginsWith(string head) => Minted().Should().StartWith(head, LastRequest.Run.Stderr);

    [Then("^the check accepts it as a claim id of that Feature$")]
    public void ThenTheCheckAcceptsItAsAClaimIdOfThatFeature()
    {
        var id = Minted();
        Tree.WriteCoLocatedFeature(
            Feature,
            "0001",
            "F1",
            _frontmatter,
            SpechtTree.SectionsWith(
                "3. Acceptance Criteria",
                $"## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n| {id} | It is minted. | brd | Active |\n"));

        Tree.Run().Violations.Should().NotContain(violation => violation.RuleId == RuleIds.Under(Tree, "SPEC030") && violation.Identifier == id);
    }

    [Then("^the minted id is (\\d+) characters long after \"(.*)\"$")]
    public void ThenTheMintedIdIsCharactersLongAfter(int length, string head)
    {
        var id = Minted();
        id.Should().StartWith(head, LastRequest.Run.Stderr);
        id[head.Length..].Should().HaveLength(length, LastRequest.Run.Stderr);
    }

    [Then("^the minted id, decoded after \"(.*)\", holds the milliseconds from (\\d{4}-\\d{2}-\\d{2}) to noon on (\\d{4}-\\d{2}-\\d{2})$")]
    public void ThenTheMintedIdDecodedAfterAHeadHoldsTheMillisecondsFrom(string head, string epoch, string date)
    {
        var id = Minted();
        id.Should().StartWith(head, LastRequest.Run.Stderr);
        Decoded(Number(id[head.Length..])).Time.Should().Be((long) (Noon(date) - Midnight(epoch)).TotalMilliseconds, LastRequest.Run.Stderr);
    }

    [Then("^exactly one line is written on standard output$")]
    public void ThenExactlyOneLineIsWrittenOnStandardOutput() =>
        LastRequest.Run.Stdout.Should().MatchRegex(@"\A[^\r\n]+(\r?\n)?\z", LastRequest.Run.Stderr);

    private void DeclarePrefix(string value)
    {
        _frontmatter = new Dictionary<string, string>(StringComparer.Ordinal) { [ClaimPrefixKey] = value };
        Tree.WriteCoLocatedFeature(Feature, "0001", "F1", _frontmatter);
    }

    private void Record(string type, Action<JsonObject> edit)
    {
        var manifest = ReadManifest();
        if (manifest["ids"] is not JsonObject ids)
        {
            ids = [];
            manifest["ids"] = ids;
        }

        if (ids[type] is not JsonObject record)
        {
            record = [];
            ids[type] = record;
        }

        edit(record);
        WriteManifest(manifest);
    }

    private const string Kind = "(ADR|lesson|decision|claim|constraint|open question|scenario)(?: ids| numbers)?";

    private const string ClaimPrefixKey = "claim_prefix";

    private static readonly Version IdsVersion = new(0, 4, 0);

    private Dictionary<string, string>? _frontmatter;
}
