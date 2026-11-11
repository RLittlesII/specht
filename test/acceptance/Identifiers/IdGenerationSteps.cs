using System.Globalization;
using System.IO.Abstractions;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Reqnroll;
using Specht.Discovery;
using Specht.Identifiers;
using Specht.Tests.Engine.Baseline;
using Specht.Tests.Shared;
using Specht.Tool;
using Specht.Tool.Features.Mint;
using Spectre.Console.Testing;

namespace Specht.Acceptance.Identifiers;

/// <summary>
/// Steps for <c>src/specht/Identifiers/.spec/id-generation.feature</c> (<c>0001-F19</c>). The root is a synthetic tree
/// holding this repository's default manifest; "a tree whose check reports a violation" adds the baseline tree under the
/// manifest's own pin. "Opts claim ids in to Nano ID" writes a <c>claim</c> entry under the top-level <c>mint</c> key in
/// C-12's shape and, as A-6 reads a scenario, a claim grammar in <c>identifiers</c> that admits the ids it mints. "The
/// tree is checked" launches the built tool on the root, and "the id settings removed" is the same manifest without
/// <c>mint</c> (A-1). An invalid manifest ends the check with <see cref="ExitCodes.InvalidManifest"/>.
/// </summary>
/// <remarks>
/// A request (A-2, item <c>0415</c>) launches <c>specht mint &lt;type&gt; --root .</c>, with <c>--feature features/one</c> when
/// it is made for a Feature, a co-located Feature written there. Entries are written for <c>adr</c> and <c>lesson</c> and, where
/// a scenario names no prefix (B-055, item <c>0417</c>), for each of C-13's types, each with a grammar admitting what it mints
/// (A-6) under any prefix the type may carry; the scenarios of item <c>0418</c>'s per-Feature types keep a step no binding
/// matches and stay pending. Where a scenario says the ADR grammar admits only four-digit ADR numbers (B-011, B-012, item
/// <c>0420</c>), it replaces the grammar the entry was written with, and no Snowflake id under either encoding matches it,
/// so the request launches the built tool whatever its clock reads and its draws are. An <c>adr</c> entry names a prefix or an encoding where a scenario names
/// one (item <c>0417</c>): B-009's prefix, B-033's and B-054's encodings, and B-053's prefixes that are not text. The built tool's
/// draws are random, so an ADR the tree holds is written only where the entry's alphabet has one character and its one
/// candidate is known (item <c>0416</c>): the guard's refusals, B-017's guard row, B-036 and B-037. The scenarios of B-034,
/// B-035, B-038 and B-039 name a first or an eighth candidate of a random draw and keep a step no binding matches. "No ADR number is minted" is nothing on
/// standard output and then an exit of <c>2</c>, <c>3</c> or <c>4</c> (A-8, B-017; OQ-11's default). The part of an id after
/// its prefix is the line less the entry's <c>prefix</c>; <c>adr</c> and <c>lesson</c> have none (B-055). "The drawn
/// characters" with a prefix ahead of them is the line starting with that prefix, the rest the entry's size of characters
/// from its alphabet.
/// <para>
/// A Snowflake entry (item <c>0419</c>) is written for <c>adr</c>. A launched tool reads the machine's clock and draws at
/// random, and no option fixes either (§ 7), so a scenario that names the time, a draw, or the candidate that would be minted
/// first runs the request in process: <see cref="MintCommand"/> over the engine's own reader and a <see cref="Minter"/> on the
/// root's file system, with a clock that reads noon on 2026-11-07, universal time, unless the scenario names another time,
/// and draws of 0 unless it names one (B-007, B-022, B-024 to B-026, B-029). Every other request launches the built tool. "The
/// candidate that would be minted first" is the id the same request mints before the tree holds it. A number is decoded
/// under the entry's encoding, <c>base32</c> when it names none (B-056), and its widths, 5, 5 and 12 bits when it names none
/// (B-023). Where a scenario reads a field the launched tool would draw, the entry names it: B-023's sequence start, and the
/// datacenter, worker and sequence start of the encoding scenarios, B-030 to B-032 and B-056, whose number is read back to
/// those fields. A scenario leaving one field unnamed names the other two, so its one draw is that field's.
/// </para>
/// </remarks>
[Binding]
[Scope(Feature = "Id generation")]
public sealed class IdGenerationSteps
{
    [Given("a repository root with a valid Specht manifest")]
    public void GivenARepositoryRootWithAValidSpechtManifest() => _tree = new SpechtTree();

    [Given("a tree whose check reports a violation")]
    public void GivenATreeWhoseCheckReportsAViolation()
    {
        var pin = ReadManifest()["schemaVersion"]!.GetValue<string>();
        BaselineTree.Write(Tree);
        Tree.Pin(pin);
    }

    [Given("a manifest pinned to schema {word}")]
    public void GivenAManifestPinnedToSchema(string version) => Tree.Pin(version);

    [Given("a manifest that opts claim ids in to Nano ID")]
    [Given("it opts claim ids in to Nano ID")]
    public void GivenAManifestThatOptsClaimIdsInToNanoId()
    {
        var manifest = ReadManifest();
        manifest["identifiers"]!["claim"] = "^B-([0-9]{3}[a-z]?|[0-9a-z]{6})$";
        manifest["mint"] = JsonNode.Parse(
            """
            {
              "claim": {
                "strategy": "nanoid",
                "nanoid": { "alphabet": "0123456789abcdefghjkmnpqrstvwxyz", "size": 6 },
                "prefix": "B-",
                "encoding": "none",
                "attempts": 8
              }
            }
            """);
        WriteManifest(manifest);
    }

    [Given("^it declares a grammar for (ADR numbers|lesson numbers|decision numbers|scenario ids)$")]
    public void GivenItDeclaresAGrammarFor(string type)
    {
        var manifest = ReadManifest();
        manifest["identifiers"]![type switch
        {
            "ADR numbers" => "adr",
            "lesson numbers" => "lesson",
            "decision numbers" => "decision",
            _ => "scenario",
        }] = type == "scenario ids" ? "^S-[0-9]+$" : "^[0-9]{4}$";
        WriteManifest(manifest);
    }

    [Given("^a manifest that opts no type in$")]
    public void GivenAManifestThatOptsNoTypeIn()
    {
        var manifest = ReadManifest();
        manifest.Remove("mint");
        WriteManifest(manifest);
    }

    [Given("^a manifest that opts only lesson numbers in$")]
    public void GivenAManifestThatOptsOnlyLessonNumbersIn()
    {
        GivenAManifestThatOptsNoTypeIn();
        OptIn("lesson", $$"""{ "strategy": "nanoid", "nanoid": { "alphabet": "{{Base32}}", "size": 8 } }""", $"^[{Base32}]{{8}}$");
    }

    [Given("^a manifest that opts (ADR numbers|lesson numbers) in to Nano ID$")]
    public void GivenAManifestThatOptsATypeInToNanoId(string type) =>
        OptIn(TypeOf(type), $$"""{ "strategy": "nanoid", "nanoid": { "alphabet": "{{Base32}}", "size": 8 } }""", $"^[{Base32}]{{8}}$");

    [Given("^(?:a manifest that|it) opts (ADR numbers|lesson numbers) in to Nano ID with a size of (.+)$")]
    public void GivenAManifestThatOptsATypeInToNanoIdWithASizeOf(string type, string size)
    {
        var text = Regex.Match(size, "^\"(.*)\", given as text$");
        var nanoid = new JsonObject { ["size"] = text.Success ? JsonValue.Create(text.Groups[1].Value) : JsonNode.Parse(size) };
        OptIn(TypeOf(type), new JsonObject { ["strategy"] = "nanoid", ["nanoid"] = nanoid }.ToJsonString(), AnyId);
    }

    [Given("^a manifest that opts ADR numbers in to Nano ID with an alphabet of lowercase letters and digits$")]
    public void GivenAManifestThatOptsAdrNumbersInToNanoIdWithAnAlphabetOfLowercaseLettersAndDigits() =>
        OptIn("adr", """{ "strategy": "nanoid", "nanoid": { "alphabet": "abcdefghijklmnopqrstuvwxyz0123456789", "size": 32 } }""", AnyId);

    [Given("^a manifest that opts ADR numbers in to Nano ID with no alphabet$")]
    public void GivenAManifestThatOptsAdrNumbersInToNanoIdWithNoAlphabet() =>
        OptIn("adr", """{ "strategy": "nanoid", "nanoid": { "size": 32 } }""", AnyId);

    [Given("^a manifest that opts (ADR numbers|lesson numbers) in to Nano ID with no size$")]
    public void GivenAManifestThatOptsATypeInToNanoIdWithNoSize(string type) =>
        OptIn(TypeOf(type), $$"""{ "strategy": "nanoid", "nanoid": { "alphabet": "{{Base32}}" } }""", AnyId);

    [Given("^a manifest that lists ADR numbers (without a strategy|with a number in place of a strategy)$")]
    public void GivenAManifestThatListsAdrNumbersWithNoStrategy(string strategy) =>
        OptIn(
            "adr",
            strategy == "without a strategy"
                ? $$"""{ "nanoid": { "alphabet": "{{Base32}}", "size": 8 } }"""
                : $$"""{ "strategy": 7, "nanoid": { "alphabet": "{{Base32}}", "size": 8 } }""",
            AnyId);

    [Given("^a manifest that lists ADR numbers with the strategy \"(.*)\"$")]
    public void GivenAManifestThatListsAdrNumbersWithTheStrategy(string strategy) =>
        OptIn(
            "adr",
            new JsonObject
            {
                ["strategy"] = strategy,
                ["nanoid"] = new JsonObject { ["alphabet"] = Base32, ["size"] = 8 },
            }.ToJsonString(),
            AnyId);

    [Given("^a manifest that opts ADR numbers in to Nano ID with its Nano ID settings given as (a number|a list|text)$")]
    public void GivenAManifestThatOptsAdrNumbersInToNanoIdWithItsNanoIdSettingsGivenAs(string value) =>
        OptIn(
            "adr",
            new JsonObject
            {
                ["strategy"] = "nanoid",
                ["nanoid"] = value switch
                {
                    "a number" => JsonValue.Create(8),
                    "a list" => new JsonArray("ab", 2),
                    _ => JsonValue.Create("ab"),
                },
            }.ToJsonString(),
            AnyId);

    [Given("^a manifest that opts ADR numbers in to Nano ID with (an empty alphabet|a number in place of an alphabet)$")]
    public void GivenAManifestThatOptsAdrNumbersInToNanoIdWithAnAlphabetThatIsNotText(string alphabet) =>
        OptIn(
            "adr",
            new JsonObject
            {
                ["strategy"] = "nanoid",
                ["nanoid"] = new JsonObject
                {
                    ["alphabet"] = alphabet == "an empty alphabet" ? JsonValue.Create(string.Empty) : JsonValue.Create(5),
                    ["size"] = 4,
                },
            }.ToJsonString(),
            AnyId);

    [Given("^a manifest pinned to a schema version that has no id settings$")]
    public void GivenAManifestPinnedToASchemaVersionThatHasNoIdSettings() => Tree.Pin("0.3.0");

    [Given("^the manifest sets them$")]
    public void GivenTheManifestSetsThem()
    {
        var (_, stderr, exitCode) = AcceptanceTool.Launch(Tree.Root, "--root", ".");
        exitCode.Should().NotBe(ExitCodes.InvalidManifest, $"the manifest is valid under its pin until it sets them: {stderr}");
        OptIn("adr", $$"""{ "strategy": "nanoid", "nanoid": { "alphabet": "{{Base32}}", "size": 8 } }""", $"^[{Base32}]{{8}}$");
    }

    [Given("^it declares no grammar for ADR numbers$")]
    public void GivenItDeclaresNoGrammarForAdrNumbers()
    {
        var manifest = ReadManifest();
        manifest["identifiers"]!.AsObject().Remove("adr");
        WriteManifest(manifest);
    }

    [Given("^(an ADR number requested of a manifest that opts no type in|an ADR number requested for a Feature)$")]
    public void GivenARequestThatPrintsNoId(string request)
    {
        if (request.EndsWith("for a Feature", StringComparison.Ordinal))
        {
            GivenAManifestThatOptsATypeInToNanoId("ADR numbers");
            _pending = ("adr", true);
        }
        else
        {
            GivenAManifestThatOptsNoTypeIn();
            _pending = ("adr", false);
        }
    }

    [Given("^an ADR number requested of a tree holding every candidate it tries$")]
    public void GivenAnAdrNumberRequestedOfATreeHoldingEveryCandidateItTries()
    {
        OptIn("adr", """{ "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } }""", "^z{3}$");
        HoldEveryAdrCandidate();
        _pending = ("adr", false);
    }

    [Given("^a manifest that opts ADR numbers in to Nano ID with no number of attempts$")]
    public void GivenAManifestThatOptsAdrNumbersInToNanoIdWithNoNumberOfAttempts() =>
        OptIn("adr", """{ "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } }""", "^z{3}$");

    [Given("^a manifest that opts ADR numbers in to Nano ID with (\\d+) attempts$")]
    public void GivenAManifestThatOptsAdrNumbersInToNanoIdWithAttempts(int attempts) =>
        OptIn(
            "adr",
            new JsonObject
            {
                ["strategy"] = "nanoid",
                ["nanoid"] = new JsonObject { ["alphabet"] = "z", ["size"] = 3 },
                ["attempts"] = attempts,
            }.ToJsonString(),
            "^z{3}$");

    [Given("^a manifest that opts ADR numbers in to Nano ID with a number of attempts of (.+)$")]
    public void GivenAManifestThatOptsAdrNumbersInToNanoIdWithANumberOfAttemptsOf(string attempts)
    {
        var text = Regex.Match(attempts, "^\"(.*)\", given as text$");
        OptIn(
            "adr",
            new JsonObject
            {
                ["strategy"] = "nanoid",
                ["nanoid"] = new JsonObject { ["alphabet"] = Base32, ["size"] = 8 },
                ["attempts"] = text.Success ? JsonValue.Create(text.Groups[1].Value) : JsonNode.Parse(attempts),
            }.ToJsonString(),
            AnyId);
    }

    [Given("^a manifest that opts ADR numbers in with the prefix \"(.*)\"$")]
    public void GivenAManifestThatOptsAdrNumbersInWithThePrefix(string prefix) =>
        OptIn(
            "adr",
            new JsonObject
            {
                ["strategy"] = "nanoid",
                ["nanoid"] = new JsonObject { ["alphabet"] = Base32, ["size"] = 8 },
                ["prefix"] = prefix,
            }.ToJsonString(),
            $"^{Regex.Escape(prefix)}[{Base32}]{{8}}$");

    [Given("^a manifest that opts ADR numbers in to Nano ID with the encoding \"(.*)\"$")]
    public void GivenAManifestThatOptsAdrNumbersInToNanoIdWithTheEncoding(string encoding) =>
        OptIn(
            "adr",
            new JsonObject
            {
                ["strategy"] = "nanoid",
                ["nanoid"] = new JsonObject { ["alphabet"] = Base32, ["size"] = 8 },
                ["encoding"] = encoding,
            }.ToJsonString(),
            AnyId);

    [Given("^a manifest that opts ADR numbers in to Nano ID with a prefix given as (the number 7|a list|a group of named settings|a yes-or-no value)$")]
    public void GivenAManifestThatOptsAdrNumbersInToNanoIdWithAPrefixGivenAs(string prefix) =>
        OptIn(
            "adr",
            new JsonObject
            {
                ["strategy"] = "nanoid",
                ["nanoid"] = new JsonObject { ["alphabet"] = Base32, ["size"] = 8 },
                ["prefix"] = prefix switch
                {
                    "the number 7" => JsonValue.Create(7),
                    "a list" => new JsonArray("ADR-"),
                    "a group of named settings" => new JsonObject { ["text"] = "ADR-" },
                    _ => JsonValue.Create(true),
                },
            }.ToJsonString(),
            AnyId);

    [Given(
        "^a manifest that opts (ADR numbers|lesson numbers|decision numbers|claim ids|constraint ids|open question ids|scenario ids)"
        + " in to Nano ID with no prefix$")]
    public void GivenAManifestThatOptsATypeInToNanoIdWithNoPrefix(string type) =>
        OptIn(TypeOf(type), $$"""{ "strategy": "nanoid", "nanoid": { "alphabet": "{{Base32}}", "size": 8 } }""", $"^(?:[A-Z]+-)?[{Base32}]{{8}}$");

    [Given("^a manifest that opts ADR numbers in to Snowflake$")]
    public void GivenAManifestThatOptsAdrNumbersInToSnowflake() => OptIn("adr", """{ "strategy": "snowflake" }""", AnyId);

    [Given("^a manifest that opts ADR numbers in to Snowflake with an epoch of (\\d{4}-\\d{2}-\\d{2})$")]
    public void GivenAManifestThatOptsAdrNumbersInToSnowflakeWithAnEpochOf(string epoch) =>
        OptInSnowflake(new JsonObject { ["epoch"] = epoch }, null);

    [Given("^a manifest that opts ADR numbers in to Snowflake with no epoch$")]
    public void GivenAManifestThatOptsAdrNumbersInToSnowflakeWithNoEpoch() => OptInSnowflake([], null);

    [Given("^a manifest that opts ADR numbers in to Snowflake with a datacenter of (\\d+), a worker of (\\d+) and no field widths$")]
    public void GivenAManifestThatOptsAdrNumbersInToSnowflakeWithADatacenterAWorkerAndNoFieldWidths(int datacenter, int worker) =>
        OptInSnowflake(new JsonObject { ["datacenter"] = datacenter, ["worker"] = worker, ["sequenceStart"] = SequenceStart }, null);

    [Given("^a manifest that opts ADR numbers in to Snowflake with no (datacenter|worker|sequence start)$")]
    public void GivenAManifestThatOptsAdrNumbersInToSnowflakeWithNo(string field)
    {
        var snowflake = new JsonObject { ["datacenter"] = 0, ["worker"] = 0, ["sequenceStart"] = 0 };
        snowflake.Remove(FieldOf(field));
        OptInSnowflake(snowflake, null);
    }

    [Given("^a manifest that opts ADR numbers in to Snowflake with a (datacenter|worker) of (\\d+)$")]
    public void GivenAManifestThatOptsAdrNumbersInToSnowflakeWithAFieldOf(string field, int value) =>
        OptInSnowflake(new JsonObject { [FieldOf(field)] = value }, null);

    [Given("^a manifest that opts ADR numbers in to Snowflake with the encoding \"(.*)\"$")]
    public void GivenAManifestThatOptsAdrNumbersInToSnowflakeWithTheEncoding(string encoding) => OptInSnowflake(EveryField(), encoding);

    [Given("^a manifest that opts ADR numbers in to Snowflake with no encoding$")]
    public void GivenAManifestThatOptsAdrNumbersInToSnowflakeWithNoEncoding() => OptInSnowflake(EveryField(), null);

    [Given("^its ADR grammar admits only four-digit ADR numbers$")]
    public void GivenItsAdrGrammarAdmitsOnlyFourDigitAdrNumbers()
    {
        var manifest = ReadManifest();
        manifest["identifiers"]!["adr"] = "^[0-9]{4}$";
        WriteManifest(manifest);
    }

    [Given("^the time is noon on (\\d{4}-\\d{2}-\\d{2}), universal time$")]
    public void GivenTheTimeIsNoonOn(string date) => Fixed.Readings = [Noon(date)];

    [Given("^the random draw for the (?:datacenter|worker|sequence start) is (\\d+)$")]
    public void GivenTheRandomDrawIs(int draw) => Fixed.Draws = [draw];

    [Given("^an ADR already numbered with the ADR number that would be minted first$")]
    public async Task GivenAnAdrAlreadyNumberedWithTheAdrNumberThatWouldBeMintedFirst()
    {
        var (_, (stdout, stderr, exitCode)) = await InProcess("adr", Fixed);
        exitCode.Should().Be(ExitCodes.Success, stderr);
        Tree.WriteRaw($".spec/adr/{stdout.TrimEnd('\n').TrimEnd('\r')}-held.md", "# Held\n");
    }

    [Given("^the clock reads earlier for the second ADR number than for the first$")]
    public void GivenTheClockReadsEarlierForTheSecondAdrNumberThanForTheFirst() =>
        Fixed.Readings = [Fixed.Readings[0], Fixed.Readings[0].AddMilliseconds(-1)];

    [Given("^ADRs that are already numbered$")]
    public void GivenAdrsThatAreAlreadyNumbered()
    {
        Tree.WriteRaw(".spec/adr/0001-first.md", "# First\n");
        Tree.WriteRaw(".spec/adr/0002-second.md", "# Second\n");
        _before = Snapshot();
    }

    [Given("^ADRs already numbered with each of the first (?:eight|three) ADR numbers that would be minted$")]
    public void GivenAdrsAlreadyNumberedWithEachOfTheFirstAdrNumbersThatWouldBeMinted() => HoldEveryAdrCandidate();

    [When("the tree is checked")]
    public void WhenTheTreeIsChecked() => _run = AcceptanceTool.Launch(Tree.Root, "--root", ".");

    [When("^(an ADR number|a lesson number) is requested$")]
    public async Task WhenATypeIdIsRequested(string id) =>
        _request = _fixed is null ? Request(TypeOf(id), forFeature: false) : await InProcess(TypeOf(id), _fixed);

    [When("^(an ADR number|a lesson number|a decision number|a claim id|a constraint id|an open question id|a scenario id) is requested for a Feature$")]
    public void WhenATypeIdIsRequestedForAFeature(string id) => _request = Request(TypeOf(id), forFeature: true);

    [When("^an ADR number and a lesson number are requested$")]
    public void WhenAnAdrNumberAndALessonNumberAreRequested()
    {
        _requests["adr"] = Request("adr", forFeature: false).Run;
        _requests["lesson"] = Request("lesson", forFeature: false).Run;
    }

    [When("^the request ends$")]
    public void WhenTheRequestEnds()
    {
        var (type, forFeature) = _pending ?? throw new InvalidOperationException("No request was described.");
        _request = Request(type, forFeature);
    }

    [Then("the violations reported are the ones the same tree gets with the id settings removed")]
    public void ThenTheViolationsReportedAreTheOnesTheSameTreeGetsWithTheIdSettingsRemoved()
    {
        var manifest = ReadManifest();
        manifest.Remove("mint");
        WriteManifest(manifest);
        var (stdout, stderr, exitCode) = AcceptanceTool.Launch(Tree.Root, "--root", ".");

        exitCode.Should().Be(ExitCodes.Violations, stderr);
        Run.ExitCode.Should().Be(exitCode, Run.Stderr);
        Run.Stdout.Should().Be(stdout);
    }

    [Then("the check does not end as it does for an invalid manifest")]
    public void ThenTheCheckDoesNotEndAsItDoesForAnInvalidManifest()
    {
        Run.ExitCode.Should().NotBe(ExitCodes.InvalidManifest, Run.Stderr);
    }

    [Then("^nothing is written on standard output$")]
    public void ThenNothingIsWrittenOnStandardOutput() => LastRequest.Run.Stdout.Should().BeEmpty(LastRequest.Run.Stderr);

    [Then("^the request ends with a failure code the check already gives$")]
    [Then("^it ends with a failure code the check already gives$")]
    public void ThenTheRequestEndsWithAFailureCodeTheCheckAlreadyGives() =>
        LastRequest.Run.ExitCode.Should().BeOneOf(FailureCodes, LastRequest.Run.Stderr);

    [Then("^standard output is one line holding only the minted number$")]
    public void ThenStandardOutputIsOneLineHoldingOnlyTheMintedNumber()
    {
        var line = Regex.Match(LastRequest.Run.Stdout, @"\A([^\r\n]+)(\r?\n)?\z");

        line.Success.Should().BeTrue($"standard output is one line, not '{LastRequest.Run.Stdout}': {LastRequest.Run.Stderr}");
        line.Groups[1].Value.Should().MatchRegex(ReadManifest()["identifiers"]![LastRequest.Type]!.GetValue<string>());
    }

    [Then("^the request ends in success$")]
    public void ThenTheRequestEndsInSuccess() => LastRequest.Run.ExitCode.Should().Be(ExitCodes.Success, LastRequest.Run.Stderr);

    [Then("^the minted (?:id|number) is (\\d+) characters long after its prefix$")]
    public void ThenTheMintedIdIsCharactersLongAfterItsPrefix(int length) => AfterPrefix(LastRequest).Should().HaveLength(length);

    [Then("^the (ADR|lesson) number is (\\d+) characters long after its prefix$")]
    public void ThenTheTypeNumberIsCharactersLongAfterItsPrefix(string type, int length) =>
        AfterPrefix((TypeOf(type), _requests[TypeOf(type)])).Should().HaveLength(length);

    [Then("^every character of the minted number after its prefix is a lowercase letter or a digit$")]
    public void ThenEveryCharacterIsALowercaseLetterOrADigit() =>
        AfterPrefix(LastRequest).Should().NotBeEmpty().And.MatchRegex("^[a-z0-9]+$");

    [Then("^every character of the minted number after its prefix is a digit or a lower-case letter other than i, l, o and u$")]
    public void ThenEveryCharacterIsLowerCaseBase32() =>
        AfterPrefix(LastRequest).Should().NotBeEmpty().And.MatchRegex("^[0-9a-hjkmnp-tv-z]+$");

    [Then("^the minted number starts with \"(.*)\"$")]
    public void ThenTheMintedNumberStartsWith(string prefix) => LastRequest.Run.Stdout.Should().StartWith(prefix, LastRequest.Run.Stderr);

    [Then("^the minted id is the drawn characters with (nothing|\"[^\"]*\") ahead of them$")]
    public void ThenTheMintedIdIsTheDrawnCharactersWithAPrefixAheadOfThem(string ahead)
    {
        var prefix = ahead == "nothing" ? string.Empty : ahead.Trim('"');
        var nanoid = ReadManifest()["mint"]![LastRequest.Type]!["nanoid"]!;
        var line = LastRequest.Run.Stdout.TrimEnd('\n').TrimEnd('\r');

        line.Should().StartWith(prefix, LastRequest.Run.Stderr);
        line[prefix.Length..].Should().MatchRegex(
            $"^[{Regex.Escape(nanoid["alphabet"]!.GetValue<string>())}]{{{nanoid["size"]!.GetValue<int>()}}}$",
            LastRequest.Run.Stderr);
    }

    [Then("^no ADR number is minted$")]
    public void ThenNoAdrNumberIsMinted()
    {
        LastRequest.Run.Stdout.Should().BeEmpty(LastRequest.Run.Stderr);
        LastRequest.Run.ExitCode.Should().BeOneOf(FailureCodes, LastRequest.Run.Stderr);
    }

    [Then("^the request ends as it does for something named on the command line that is not found$")]
    public void ThenTheRequestEndsAsItDoesForSomethingNamedOnTheCommandLineThatIsNotFound() =>
        LastRequest.Run.ExitCode.Should().Be(ExitCodes.NotFound, LastRequest.Run.Stderr);

    [Then("^the request ends as it does for an invalid manifest$")]
    public void ThenTheRequestEndsAsItDoesForAnInvalidManifest() =>
        LastRequest.Run.ExitCode.Should().Be(ExitCodes.InvalidManifest, LastRequest.Run.Stderr);

    [Then("^no file under the root or elsewhere has been created, modified or deleted$")]
    public void ThenNoFileUnderTheRootOrElsewhereHasBeenCreatedModifiedOrDeleted() =>
        Snapshot().Should().BeEquivalentTo(_before ?? throw new InvalidOperationException("No tree was numbered."), LastRequest.Run.Stderr);

    [Then(
        "^the minted number, decoded, holds the milliseconds from (?:midnight on )?(\\d{4}-\\d{2}-\\d{2})(?:, universal time,)?"
        + " to noon on (\\d{4}-\\d{2}-\\d{2})$")]
    public void ThenTheMintedNumberDecodedHoldsTheMillisecondsFrom(string epoch, string date) =>
        Decoded().Time.Should().Be((long) (Noon(date) - Midnight(epoch)).TotalMilliseconds, LastRequest.Run.Stderr);

    [Then("^the minted number, decoded, holds the (datacenter|worker|sequence) (\\d+)$")]
    public void ThenTheMintedNumberDecodedHoldsTheField(string field, long value)
    {
        var (_, datacenter, worker, sequence) = Decoded();
        (field switch
        {
            "datacenter" => datacenter,
            "worker" => worker,
            _ => sequence,
        }).Should().Be(value, LastRequest.Run.Stderr);
    }

    [Then("^the minted number's lowest (\\d+) bits are its sequence$")]
    public void ThenTheMintedNumbersLowestBitsAreItsSequence(int bits)
    {
        (Number() & ((1L << bits) - 1)).Should().Be(SequenceStart, LastRequest.Run.Stderr);
        _below = bits;
    }

    [Then("^the (\\d+) bits above (?:them|those) hold the (?:worker|datacenter) (\\d+)$")]
    public void ThenTheBitsAboveHold(int bits, long value)
    {
        ((Number() >> _below) & ((1L << bits) - 1)).Should().Be(value, LastRequest.Run.Stderr);
        _below += bits;
    }

    [Then("^the minted number after its prefix, read as lower-case base32, is the Snowflake number$")]
    public void ThenTheMintedNumberAfterItsPrefixReadAsLowerCaseBase32IsTheSnowflakeNumber()
    {
        AfterPrefix(LastRequest).Should().MatchRegex($"^[{Base32}]+$", LastRequest.Run.Stderr);
        ShouldHoldTheEntrysFields();
    }

    [Then("^the minted number after its prefix is the Snowflake number in decimal digits$")]
    public void ThenTheMintedNumberAfterItsPrefixIsTheSnowflakeNumberInDecimalDigits()
    {
        AfterPrefix(LastRequest).Should().MatchRegex("^[0-9]+$", LastRequest.Run.Stderr);
        ShouldHoldTheEntrysFields();
    }

    [AfterScenario]
    public void DisposeTree() => _tree?.Dispose();

    private SpechtTree Tree => _tree ?? throw new InvalidOperationException("No root was prepared.");

    private string ManifestPath => Path.Combine(Tree.Root, SpechtTree.ManifestPath);

    private (string Stdout, string Stderr, int ExitCode) Run => _run ?? throw new InvalidOperationException("The tree was not checked.");

    private (string Type, (string Stdout, string Stderr, int ExitCode) Run) LastRequest =>
        _request ?? throw new InvalidOperationException("No id was requested.");

    private FixedRun Fixed => _fixed ??= new FixedRun();

    private static string TypeOf(string words) => Types.First(pair => words.Contains(pair.Words, StringComparison.Ordinal)).Type;

    private static DateTimeOffset Midnight(string date) =>
        DateTimeOffset.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

    private static DateTimeOffset Noon(string date) => Midnight(date).AddHours(12);

    private static string FieldOf(string words) => words switch
    {
        "datacenter" => "datacenter",
        "worker" => "worker",
        _ => "sequenceStart",
    };

    private static JsonObject EveryField() => new() { ["datacenter"] = 3, ["worker"] = 9, ["sequenceStart"] = SequenceStart };

    private JsonObject ReadManifest() => JsonNode.Parse(File.ReadAllText(ManifestPath))!.AsObject();

    private void WriteManifest(JsonObject manifest) => File.WriteAllText(ManifestPath, manifest.ToJsonString());

    private void OptIn(string type, string entry, string grammar)
    {
        var manifest = ReadManifest();
        manifest["identifiers"]![type] = grammar;
        if (manifest["mint"] is not JsonObject mint)
        {
            mint = [];
            manifest["mint"] = mint;
        }

        mint[type] = JsonNode.Parse(entry);
        WriteManifest(manifest);
    }

    private void HoldEveryAdrCandidate()
    {
        var nanoid = ReadManifest()["mint"]!["adr"]!["nanoid"]!;
        var alphabet = nanoid["alphabet"]!.GetValue<string>();
        if (alphabet.Length != 1)
        {
            throw new InvalidOperationException($"Only a one-character alphabet has known candidates, not '{alphabet}'.");
        }

        Tree.WriteRaw($".spec/adr/{new string(alphabet[0], nanoid["size"]!.GetValue<int>())}-held.md", "# Held\n");
    }

    private (string Type, (string Stdout, string Stderr, int ExitCode) Run) Request(string type, bool forFeature)
    {
        if (!forFeature)
        {
            return (type, AcceptanceTool.Launch(Tree.Root, "mint", type, "--root", "."));
        }

        Tree.WriteCoLocatedFeature(Feature, "0001", "F1");

        return (type, AcceptanceTool.Launch(Tree.Root, "mint", type, "--feature", Feature, "--root", "."));
    }

    private void OptInSnowflake(JsonObject snowflake, string? encoding)
    {
        var entry = new JsonObject { ["strategy"] = "snowflake", ["snowflake"] = snowflake };
        if (encoding is not null)
        {
            entry["encoding"] = encoding;
        }

        OptIn("adr", entry.ToJsonString(), AnyId);
    }

    private async Task<(string Type, (string Stdout, string Stderr, int ExitCode) Run)> InProcess(string type, FixedRun run)
    {
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var minter = new Minter(
            new FixedDraws(run.Draws), new HeldIds(new SpechtDiscovery(new FileSystem(), new ChildProcess())), new FixedClock(run.Readings));
        var exitCode = await new MintCommand(console, EngineServices.Reader(), minter, errors)
            .ExecuteAsync(null!, new MintCommand.Settings { Type = type, Root = Tree.Root }, CancellationToken.None);

        return (type, (console.Output, errors.ToString(), exitCode));
    }

    private long Number()
    {
        var digits = AfterPrefix(LastRequest);
        var encoding = ReadManifest()["mint"]![LastRequest.Type]!["encoding"]?.GetValue<string>() ?? "base32";
        return encoding == "none"
            ? long.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture)
            : digits.Aggregate(0L, static (number, character) => (number << 5) | (long) Base32.IndexOf(character, StringComparison.Ordinal));
    }

    private (long Time, long Datacenter, long Worker, long Sequence) Decoded()
    {
        var snowflake = ReadManifest()["mint"]![LastRequest.Type]!["snowflake"];
        var datacenterBits = snowflake?["datacenterBits"]?.GetValue<int>() ?? 5;
        var workerBits = snowflake?["workerBits"]?.GetValue<int>() ?? 5;
        var sequenceBits = snowflake?["sequenceBits"]?.GetValue<int>() ?? 12;
        var number = Number();

        return (
            number >> (datacenterBits + workerBits + sequenceBits),
            (number >> (workerBits + sequenceBits)) & ((1L << datacenterBits) - 1),
            (number >> sequenceBits) & ((1L << workerBits) - 1),
            number & ((1L << sequenceBits) - 1));
    }

    private void ShouldHoldTheEntrysFields()
    {
        var snowflake = ReadManifest()["mint"]![LastRequest.Type]!["snowflake"]!;
        var (_, datacenter, worker, sequence) = Decoded();
        datacenter.Should().Be(snowflake["datacenter"]!.GetValue<int>(), LastRequest.Run.Stderr);
        worker.Should().Be(snowflake["worker"]!.GetValue<int>(), LastRequest.Run.Stderr);
        sequence.Should().Be(snowflake["sequenceStart"]!.GetValue<int>(), LastRequest.Run.Stderr);
    }

    private Dictionary<string, (string Text, DateTime Written)> Snapshot() =>
        Directory.EnumerateFileSystemEntries(Tree.Root, "*", SearchOption.AllDirectories).ToDictionary(
            path => Path.GetRelativePath(Tree.Root, path),
            static path => File.Exists(path) ? (File.ReadAllText(path), File.GetLastWriteTimeUtc(path)) : (string.Empty, Directory.GetLastWriteTimeUtc(path)),
            StringComparer.Ordinal);

    private string AfterPrefix((string Type, (string Stdout, string Stderr, int ExitCode) Run) request)
    {
        var line = request.Run.Stdout.TrimEnd('\n').TrimEnd('\r');
        var prefix = ReadManifest()["mint"]?[request.Type]?["prefix"]?.GetValue<string>() ?? string.Empty;
        line.Should().StartWith(prefix, request.Run.Stderr);

        return line[prefix.Length..];
    }

    private sealed class FixedRun
    {
        public DateTimeOffset[] Readings { get; set; } = [new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero)];

        public int[] Draws { get; set; } = [0];
    }

    private sealed class FixedClock(DateTimeOffset[] readings) : IClock
    {
        public DateTimeOffset GetUtcNow() => readings[Math.Min(_read++, readings.Length - 1)];

        private int _read;
    }

    private sealed class FixedDraws(int[] draws) : IRandomSource
    {
        public int Next(int maxExclusive)
        {
            var draw = draws[_drawn++ % draws.Length];
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(draw, maxExclusive);
            return draw;
        }

        private int _drawn;
    }

    private const string Base32 = "0123456789abcdefghjkmnpqrstvwxyz";

    private const string AnyId = "^[0-9a-z]+$";

    private const int SequenceStart = 2709;

    private const string Feature = "features/one";

    private static readonly (string Words, string Type)[] Types =
    [
        ("ADR", "adr"),
        ("lesson", "lesson"),
        ("decision", "decision"),
        ("claim", "claim"),
        ("constraint", "constraint"),
        ("open question", "openQuestion"),
        ("scenario", "scenario"),
    ];

    private static readonly int[] FailureCodes = [ExitCodes.MissingInput, ExitCodes.InvalidManifest, ExitCodes.NotFound];

    private readonly Dictionary<string, (string Stdout, string Stderr, int ExitCode)> _requests = new(StringComparer.Ordinal);

    private SpechtTree? _tree;

    private (string Stdout, string Stderr, int ExitCode)? _run;

    private (string Type, (string Stdout, string Stderr, int ExitCode) Run)? _request;

    private (string Type, bool ForFeature)? _pending;

    private FixedRun? _fixed;

    private Dictionary<string, (string Text, DateTime Written)>? _before;

    private int _below;
}
