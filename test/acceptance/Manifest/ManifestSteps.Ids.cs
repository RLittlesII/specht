using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Evaluation;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Acceptance.Manifest;

/// <summary>
/// Steps for schema 0.4.0's <c>ids</c> key (<c>0001-F5</c> B-081 to B-095, B-097 to B-101; A-8; decision 0024; item
/// <c>0436</c>). A record is written under <c>ids</c> as the members a step names and nothing else, so what it leaves out
/// is the shipped record's (B-097); "a record for" a type is a plain text and an id the type's numbered ids fit. A type is
/// named as the scenario writes it - "claim", "open question", "ADR" - and read as its record's key. "A base32 character"
/// is one of Crockford's lowercase thirty-two, and "an id that admits any text" is <c>.*</c>. The old id settings are the
/// top-level <c>mint</c> key and a grammar under <c>identifiers</c>, shaped as schema 0.3.1 has them. A shipped default
/// grammar is composed from the shipped 0.4.0 manifest's record as A-8 composes it for a Feature without a prefix: the
/// <c>plain</c> text without its closing <c>{id}</c>, matched as literal text, then a whole match of the <c>id</c>,
/// case-sensitive. "Only the rejection" on standard error is one line naming the manifest.
/// </summary>
public sealed partial class ManifestSteps
{
    [Given("^the manifest's ids key holds a record for (.+)$")]
    public void GivenTheManifestsIdsKeyHoldsARecordFor(string types)
    {
        foreach (var type in TypesNamed(types))
        {
            Ids()[type] = JsonNode.Parse(NumberedRecords[type]);
        }
    }

    [Given("the manifest's ids key is a list")]
    public void GivenTheManifestsIdsKeyIsAList() => Manifest["ids"] = new JsonArray();

    [Given("the manifest's ids key gives claim IDs a text in place of a record")]
    public void GivenTheManifestsIdsKeyGivesClaimIdsATextInPlaceOfARecord() => Ids()["claim"] = "B-{id}";

    [Given("^the manifest's ids key gives claim IDs a record with a (prefix|size)$")]
    public void GivenTheManifestsIdsKeyGivesClaimIdsARecordWithA(string member)
    {
        var record = JsonNode.Parse(NumberedRecords["claim"])!.AsObject();
        record[member] = member == "prefix" ? JsonValue.Create("B-") : JsonValue.Create(3);
        Ids()["claim"] = record;
    }

    [Given("the manifest gives claim IDs a record of a plain text, an id pattern and a {string} annotation")]
    public void GivenTheManifestGivesClaimIdsARecordWithAnAnnotation(string annotation)
    {
        var record = JsonNode.Parse(NumberedRecords["claim"])!.AsObject();
        record[annotation] = "An annotation the engine ignores.";
        Ids()["claim"] = record;
    }

    [Given("^the manifest gives (claim IDs|ADR numbers) the (form|plain text) \"(.*)\"$")]
    public void GivenTheManifestGivesATypeTheMember(string type, string member, string text) =>
        Ids()[TypesNamed(type).Single()] = new JsonObject { [Member(member)] = text };

    [Given("^the manifest gives claim IDs a record whose id pattern is (the empty text|a number|\"\\[0-9\", which does not compile)$")]
    public void GivenTheManifestGivesClaimIdsARecordWhoseIdPatternIs(string pattern)
    {
        _idPattern = pattern switch
        {
            "the empty text" => string.Empty,
            "a number" => "7",
            _ => "[0-9",
        };
        Ids()["claim"] = new JsonObject
        {
            ["plain"] = "B-{id}",
            ["id"] = pattern == "a number" ? JsonValue.Create(7) : JsonValue.Create(_idPattern),
        };
    }

    [Given("^the claim record has the form \"([^\"]*)\" and an id of six or thirteen base32 characters$")]
    public void GivenTheClaimRecordHasTheFormAndABase32Id(string form)
    {
        IdsRecord("claim")["form"] = form;
        IdsRecord("claim")["id"] = $"{Base32}{{6}}|{Base32}{{13}}";
    }

    [Given("^the claim record has the form \"([^\"]*)\", the plain text \"([^\"]*)\" and an id of three digits$")]
    public void GivenTheClaimRecordHasTheFormThePlainTextAndAThreeDigitId(string form, string plain)
    {
        IdsRecord("claim")["form"] = form;
        IdsRecord("claim")["plain"] = plain;
        IdsRecord("claim")["id"] = "[0-9]{3}";
    }

    [Given("^the claim record has the form \"([^\"]*)\" and an id that admits any text$")]
    public void GivenTheClaimRecordHasTheFormAndAnIdThatAdmitsAnyText(string form)
    {
        IdsRecord("claim")["form"] = form;
        IdsRecord("claim")["id"] = ".*";
    }

    [Given("^the (claim|scenario) record has the form \"([^\"]*)\"$")]
    public void GivenTheRecordHasTheForm(string type, string form) => IdsRecord(type)["form"] = form;

    [Given("the claim record holds only an id of six base32 characters")]
    public void GivenTheClaimRecordHoldsOnlyAnIdOfSixBase32Characters() => Ids()["claim"] = new JsonObject { ["id"] = $"{Base32}{{6}}" };

    [Given("^the claim record has the (form|plain text) \"([^\"]*)\" and the scenario record the (?:form|plain text) \"([^\"]*)\"$")]
    public void GivenTheClaimAndScenarioRecordsHave(string member, string claim, string scenario)
    {
        IdsRecord("claim")[Member(member)] = claim;
        IdsRecord("scenario")[Member(member)] = scenario;
    }

    [Given("^the manifest pins schema version \"([^\"]*)\" and declares no (.+) record$")]
    public void GivenTheManifestPinsSchemaVersionAndDeclaresNoRecord(string pin, string type)
    {
        GivenTheManifestPinsSchemaVersion(pin);
        (Manifest["ids"] as JsonObject)?.Remove(TypesNamed(type).Single());
    }

    [Given("Activity declares claim_prefix {string}")]
    public void GivenActivityDeclaresAPrefix(string prefix)
    {
        var feature = Prefixed("Activity");
        feature.Frontmatter[ClaimPrefixKey] = Quoted(prefix);
        WritePrefixed(feature);
    }

    [Given("Activity declares the following claims:")]
    public void GivenActivityDeclaresTheFollowingClaims(DataTable claims)
    {
        var feature = Prefixed("Activity");
        feature.Claims = [.. claims.Rows.Select(static row => row["Claim ID"])];
        WritePrefixed(feature);
    }

    [Given("Activity declares claim_prefix {string} and claims {string}, {string} and {string}")]
    public void GivenActivityDeclaresAPrefixAndThreeClaims(string prefix, string first, string second, string third) =>
        WriteActivity(prefix, first, second, third);

    [Given("Activity declares claim_prefix {string} and claims {string} and {string}")]
    public void GivenActivityDeclaresAPrefixAndTwoClaims(string prefix, string first, string second) => WriteActivity(prefix, first, second);

    [Given("a Feature without a prefix declares claims {string} and {string}")]
    public void GivenAFeatureWithoutAPrefixDeclaresClaims(string first, string second) =>
        GivenAnotherFeatureDeclaresNoPrefixAndHasClaims(first, second);

    [Given("Activity's companion tags one scenario with {string} and {string}")]
    public void GivenActivitysCompanionTagsOneScenarioWith(string first, string second)
    {
        var feature = Prefixed("Activity");
        feature.Tags = [$"{first} {second}"];
        WritePrefixed(feature);
    }

    [Given("the manifest gives constraint, open question, decision, ADR and lesson IDs records of their own")]
    public void GivenTheManifestGivesTheUnreadTypesRecordsOfTheirOwn()
    {
        foreach (var (type, head) in UnreadRecords)
        {
            Ids()[type] = new JsonObject { ["plain"] = head + "{id}", ["id"] = "[a-z]+" };
        }
    }

    [Given("it opts claim IDs in to minting")]
    public void GivenItOptsClaimIdsInToMinting() =>
        IdsRecord("claim")["mint"] = new JsonObject
        {
            ["strategy"] = "nanoid",
            ["nanoid"] = new JsonObject { ["alphabet"] = "0123456789abcdefghjkmnpqrstvwxyz", ["size"] = 6 },
        };

    [Given("^the manifest holds (an? .+)$")]
    public void GivenTheManifestHoldsAnOldIdSetting(string setting)
    {
        var mint = Regex.Match(setting, "^a mint key that opts (.+) in$");
        if (mint.Success)
        {
            Manifest["mint"] = new JsonObject { [TypesNamed(mint.Groups[1].Value).Single()] = new JsonObject { ["strategy"] = "nanoid" } };
            return;
        }

        var grammar = Regex.Match(setting, "^an? (.+) grammar under identifiers$");
        grammar.Success.Should().BeTrue("'{0}' names an old id setting", setting);
        var type = TypesNamed(grammar.Groups[1].Value).Single();
        Named("identifiers")[type] = OldGrammars[type];
    }

    [Given("the manifest declares task, Feature and epic grammars under identifiers")]
    public void GivenTheManifestDeclaresTaskFeatureAndEpicGrammarsUnderIdentifiers() =>
        Manifest["identifiers"] = new JsonObject
        {
            ["task"] = "^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$",
            ["feature"] = "^F[0-9]+[a-z]?$",
            ["epic"] = "^[0-9]{4}$",
        };

    [Then("^the rejection names (the ids key|the task member|the claim record|the claim record's prefix|the claim record's size)$")]
    public void ThenTheRejectionNamesTheIdsMember(string member) =>
        _rejection!.Message.Should().Contain(member switch
        {
            "the ids key" => "ids",
            "the task member" => "ids.task",
            "the claim record" => "ids.claim",
            "the claim record's prefix" => "ids.claim.prefix",
            _ => "ids.claim.size",
        });

    [Then("^the rejection names (claim IDs|ADR numbers) and \"(.*)\"$")]
    public void ThenTheRejectionNamesTheTypeAndTheText(string type, string text) =>
        _rejection!.Message.Should().Contain($"ids.{TypesNamed(type).Single()}").And.Contain(text);

    [Then("the rejection names claim IDs and that pattern")]
    public void ThenTheRejectionNamesClaimIdsAndThatPattern() =>
        _rejection!.Message.Should().Contain("ids.claim").And.Contain(_idPattern switch
        {
            null => throw new InvalidOperationException("No id pattern was written."),
            "" => "empty",
            _ => _idPattern,
        });

    [Then("^the rejection names the ids record for (.+)$")]
    public void ThenTheRejectionNamesTheIdsRecordFor(string type) =>
        _rejection!.Message.Should().Contain($"ids.{TypesNamed(type).Single()}");

    [Then("the rejection names the claim and scenario records")]
    public void ThenTheRejectionNamesTheClaimAndScenarioRecords() =>
        _rejection!.Message.Should().Contain("ids.claim").And.Contain("ids.scenario");

    [Then("the standard error carries only the rejection")]
    public void ThenTheStandardErrorCarriesOnlyTheRejection() => OnlyRejection();

    [Then("the standard error carries only the rejection naming the ids key")]
    public void ThenTheStandardErrorCarriesOnlyTheRejectionNamingTheIdsKey() => OnlyRejection().Should().Contain("'ids'");

    [Then("^for a Feature that declares no claim prefix the shipped default (.+) grammar admits \"(.*)\"$")]
    [Then("^the shipped default (.+) grammar admits \"(.*)\"$")]
    public void ThenTheShippedDefaultGrammarAdmits(string type, string id)
    {
        _grammar = ShippedGrammar(TypesNamed(type).Single());
        _grammar.IsMatch(id).Should().BeTrue("the shipped 0.4.0 {0} grammar {1} admits it", type, _grammar);
    }

    [Then("it rejects {string}")]
    public void ThenItRejects(string id)
    {
        var grammar = _grammar ?? throw new InvalidOperationException("No grammar was composed.");
        grammar.IsMatch(id).Should().BeFalse("the grammar {0} rejects it", grammar);
    }

    [Then("the report equals the report for the same tree with those records and the opt-in removed")]
    public void ThenTheReportEqualsTheReportWithoutThoseRecords()
    {
        _rejection.Should().BeNull();
        var withRecords = _report!.Violations;
        foreach (var (type, _) in UnreadRecords)
        {
            Ids().Remove(type);
        }

        IdsRecord("claim").Remove("mint");
        if (IdsRecord("claim").Count == 0)
        {
            Ids().Remove("claim");
        }

        WhenTheCheckRuns();

        _rejection.Should().BeNull();
        _report!.Violations.Should().Equal(withRecords);
    }

    [Then("the claim ID check reports only {string}")]
    public void ThenTheClaimIdCheckReportsOnlyOne(string claim) =>
        _prefixed.Keys.SelectMany(ClaimIdViolations).Select(static violation => violation.Identifier).Should().Equal(claim);

    [Then("the claim ID check reports Activity's claim prefix, naming the head {string}")]
    public void ThenTheClaimIdCheckReportsActivitysClaimPrefix(string head)
    {
        var lines = File.ReadAllLines(Path.Combine(Tree.Root, Prefixed("Activity").Path));
        var line = Array.FindIndex(lines, static text => text.StartsWith($"{ClaimPrefixKey}:", StringComparison.Ordinal)) + 1;
        var violation = ClaimIdViolations("Activity").Should().ContainSingle().Subject;
        violation.Line.Should().Be(line);
        violation.Message.Should().Contain($"'{head}'");
    }

    [Then("it reports no claim of Activity")]
    public void ThenItReportsNoClaimOfActivity() =>
        ClaimIdViolations("Activity").Should().NotContain(violation => Prefixed("Activity").Claims.Contains(violation.Identifier));

    [Then("no claim tag in Activity's companion is unresolved")]
    public void ThenNoClaimTagInActivitysCompanionIsUnresolved()
    {
        _rejection.Should().BeNull();
        _report!.Violations.Should().NotContain(violation =>
            violation.RuleId == RuleIds.Under(Manifest, "SPEC021") && violation.File.StartsWith(ActivityFolder, StringComparison.Ordinal));
    }

    [Then("the check reports nothing for Activity")]
    public void ThenTheCheckReportsNothingForActivity()
    {
        _rejection.Should().BeNull();
        _report!.Violations.Where(violation => violation.File.StartsWith(ActivityFolder, StringComparison.Ordinal)).Should().BeEmpty();
    }

    private string ActivityFolder => Prefixed("Activity").Path[..(Prefixed("Activity").Path.LastIndexOf('/') + 1)];

    private static Regex ShippedGrammar(string type)
    {
        var shipped = EngineServices.Versions().Find(IdsFrom);
        shipped.Should().NotBeNull("schema 0.4.0 ships the ids records");
        var record = JsonNode.Parse(shipped!.Manifest)!["ids"]?[type];
        record.Should().NotBeNull("schema 0.4.0's manifest holds the {0} record", type);
        var plain = record!["plain"]!.GetValue<string>();
        plain.Should().EndWith("{id}");
        return new Regex($"^{Regex.Escape(plain[..^"{id}".Length])}(?:{record["id"]!.GetValue<string>()})$", RegexOptions.CultureInvariant);
    }

    private static IEnumerable<string> TypesNamed(string phrase)
    {
        var names = phrase.EndsWith(" IDs", StringComparison.Ordinal) ? phrase[..^4]
            : phrase.EndsWith(" numbers", StringComparison.Ordinal) ? phrase[..^8]
            : phrase;
        return names.Replace(" and ", ", ", StringComparison.Ordinal)
            .Split(", ")
            .Select(static name => name switch
            {
                "open question" => "openQuestion",
                "ADR" => "adr",
                _ => name,
            });
    }

    private static string Member(string member) => member == "plain text" ? "plain" : member;

    private string OnlyRejection() =>
        Run.Stderr.Split('\n', StringSplitOptions.RemoveEmptyEntries).Should().ContainSingle().Which.Should()
            .StartWith("specht: .specht/manifest.json:").And.Subject;

    private void WriteActivity(string prefix, params string[] claims)
    {
        var feature = Prefixed("Activity");
        feature.Frontmatter[ClaimPrefixKey] = Quoted(prefix);
        feature.Claims = claims;
        WritePrefixed(feature);
    }

    private JsonObject Ids() => Named("ids");

    private JsonObject IdsRecord(string type)
    {
        if (Ids()[type] is not JsonObject record)
        {
            record = [];
            Ids()[type] = record;
        }

        return record;
    }

    private const string Base32 = "[0-9a-hjkmnp-tv-z]";

    private static readonly SemanticVersion IdsFrom = new(0, 4, 0);

    private static readonly string[] OldIdGrammars = ["claim", "constraint", "openQuestion", "scenario", "decision", "adr", "lesson"];

    private static readonly Dictionary<string, string> NumberedRecords = new(StringComparer.Ordinal)
    {
        ["claim"] = """{ "plain": "B-{id}", "id": "[0-9]{3}[a-z]?" }""",
        ["constraint"] = """{ "plain": "C-{id}", "id": "[0-9]+" }""",
        ["openQuestion"] = """{ "plain": "OQ-{id}", "id": "[0-9]+" }""",
        ["scenario"] = """{ "plain": "S-{id}", "id": "[0-9]+" }""",
        ["decision"] = """{ "plain": "{id}", "id": "[0-9]{4}" }""",
        ["adr"] = """{ "plain": "{id}", "id": "[0-9]{4}" }""",
        ["lesson"] = """{ "plain": "{id}", "id": "[0-9]{4}" }""",
        ["task"] = """{ "plain": "T-{id}", "id": "[0-9]+" }""",
    };

    private static readonly Dictionary<string, string> OldGrammars = new(StringComparer.Ordinal)
    {
        ["claim"] = "^B-[0-9]{3}[a-z]?$",
        ["constraint"] = "^C-[0-9]+$",
        ["openQuestion"] = "^OQ-[0-9]+$",
        ["scenario"] = "^S-[0-9]+$",
        ["decision"] = "^[0-9]{4}$",
    };

    private static readonly (string Type, string Head)[] UnreadRecords =
        [("constraint", "CON-"), ("openQuestion", "Q-"), ("decision", "D-"), ("adr", "ADR-"), ("lesson", "L-")];

    private string? _idPattern;
    private Regex? _grammar;
}
