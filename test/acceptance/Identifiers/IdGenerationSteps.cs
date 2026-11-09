using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Engine.Baseline;
using Specht.Tests.Shared;
using Specht.Tool;

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
/// it is made for a Feature, a co-located Feature written there. Only <c>adr</c> and <c>lesson</c> entries are written here,
/// each with a grammar admitting what it mints (A-6), so the scenarios of other items' types, of Snowflake, of a prefix, an
/// encoding or a number of attempts keep a step no binding matches and stay pending. "No ADR number is minted" is nothing on
/// standard output and then an exit of <c>2</c>, <c>3</c> or <c>4</c> (A-8, B-017; OQ-11's default). The part of an id after
/// its prefix is the line less the entry's <c>prefix</c>; <c>adr</c> and <c>lesson</c> have none under every answer to OQ-14.
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

    [When("the tree is checked")]
    public void WhenTheTreeIsChecked() => _run = AcceptanceTool.Launch(Tree.Root, "--root", ".");

    [When("^(an ADR number|a lesson number) is requested$")]
    public void WhenATypeIdIsRequested(string id) => _request = Request(TypeOf(id), forFeature: false);

    [When("^(an ADR number|a lesson number) is requested for a Feature$")]
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

    [Then("^no ADR number is minted$")]
    public void ThenNoAdrNumberIsMinted()
    {
        LastRequest.Run.Stdout.Should().BeEmpty(LastRequest.Run.Stderr);
        LastRequest.Run.ExitCode.Should().BeOneOf(FailureCodes, LastRequest.Run.Stderr);
    }

    [Then("^the request ends as it does for an invalid manifest$")]
    public void ThenTheRequestEndsAsItDoesForAnInvalidManifest() =>
        LastRequest.Run.ExitCode.Should().Be(ExitCodes.InvalidManifest, LastRequest.Run.Stderr);

    [AfterScenario]
    public void DisposeTree() => _tree?.Dispose();

    private SpechtTree Tree => _tree ?? throw new InvalidOperationException("No root was prepared.");

    private string ManifestPath => Path.Combine(Tree.Root, SpechtTree.ManifestPath);

    private (string Stdout, string Stderr, int ExitCode) Run => _run ?? throw new InvalidOperationException("The tree was not checked.");

    private (string Type, (string Stdout, string Stderr, int ExitCode) Run) LastRequest =>
        _request ?? throw new InvalidOperationException("No id was requested.");

    private static string TypeOf(string words) => words.Contains("ADR", StringComparison.Ordinal) ? "adr" : "lesson";

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

    private (string Type, (string Stdout, string Stderr, int ExitCode) Run) Request(string type, bool forFeature)
    {
        if (!forFeature)
        {
            return (type, AcceptanceTool.Launch(Tree.Root, "mint", type, "--root", "."));
        }

        Tree.WriteCoLocatedFeature(Feature, "0001", "F1");

        return (type, AcceptanceTool.Launch(Tree.Root, "mint", type, "--feature", Feature, "--root", "."));
    }

    private string AfterPrefix((string Type, (string Stdout, string Stderr, int ExitCode) Run) request)
    {
        var line = request.Run.Stdout.TrimEnd('\n').TrimEnd('\r');
        var prefix = ReadManifest()["mint"]?[request.Type]?["prefix"]?.GetValue<string>() ?? string.Empty;
        line.Should().StartWith(prefix, request.Run.Stderr);

        return line[prefix.Length..];
    }

    private const string Base32 = "0123456789abcdefghjkmnpqrstvwxyz";

    private const string AnyId = "^[0-9a-z]+$";

    private const string Feature = "features/one";

    private static readonly int[] FailureCodes = [ExitCodes.MissingInput, ExitCodes.InvalidManifest, ExitCodes.NotFound];

    private readonly Dictionary<string, (string Stdout, string Stderr, int ExitCode)> _requests = new(StringComparer.Ordinal);

    private SpechtTree? _tree;

    private (string Stdout, string Stderr, int ExitCode)? _run;

    private (string Type, (string Stdout, string Stderr, int ExitCode) Run)? _request;

    private (string Type, bool ForFeature)? _pending;
}
