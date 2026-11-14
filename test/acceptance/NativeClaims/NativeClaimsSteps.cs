using System.IO.Abstractions;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Acceptance.NativeClaims;

/// <summary>
/// Steps for <c>src/specht/Rules/NativeClaims/.spec/native-claims.feature</c> (0001-F16), bound for the claim members of
/// item 0389, B-002, B-027, B-028, B-029 and B-031, and for the native claim reader of item 0390, B-001, B-004 to B-007,
/// B-011 to B-013, B-025 and B-030. The other scenarios bind only the Background and are left out as pending. A root is a
/// synthetic tree holding one co-located Markdown specification, so a claim read from a claims table is observable, and
/// its manifest declares no epic grammar, as the Background's claim file set carries no epic (B-027), and does not exclude
/// the root's <c>.spec</c> folder, which holds the claim files the scenarios name. The manifest is edited in memory and
/// written when the check runs; "the check runs" launches the built tool on the root, the only place an exit code and its
/// streams are seen. The catalogue is printed by no command before item 0391, so a step that reads it reads the model the
/// engine builds of the same root. "The output of the same tree before this Feature" is written here, not produced by the
/// tool under test: this tree's clean output under schema <c>0.3.1</c>, exit <c>0</c>, an empty stderr and the four summary
/// lines that <c>specht.tool</c> <c>0.5.35-preview</c>, which predates this Feature, prints for it (B-028).
/// </summary>
[Binding]
[Scope(Feature = "Native Gherkin claims")]
public sealed class NativeClaimsSteps
{
    [Given("a root with a manifest pinned to schema {string}")]
    public void GivenARootWithAManifestPinnedToSchema(string pin)
    {
        _tree = new SpechtTree();
        _tree.WriteCoLocatedFeature("src/arithmetic", "0001", "F1");
        Manifest = JsonNode.Parse(File.ReadAllText(ManifestPath))!.AsObject();
        Manifest["schemaVersion"] = pin;
        Manifest["identifiers"]!.AsObject().Remove("epic");
        Manifest["exclusions"] = new JsonArray(Manifest["exclusions"]!.AsArray()
            .Select(static entry => entry!.GetValue<string>())
            .Where(static entry => entry != "/.spec")
            .Select(static entry => (JsonNode?) JsonValue.Create(entry))
            .ToArray());
    }

    [Given("the manifest sets {string} to {string}")]
    public void GivenTheManifestSetsTo(string path, string value) => Set(path, JsonValue.Create(value));

    [Given("the manifest sets {string} to the number {int}")]
    public void GivenTheManifestSetsToTheNumber(string path, int value) => Set(path, JsonValue.Create(value));

    [Given("{string} holds one claim file set with glob {string} and feature {string}")]
    public void GivenHoldsOneClaimFileSetWithGlobAndFeature(string path, string glob, string feature) =>
        Set(path, new JsonArray(new JsonObject { ["glob"] = glob, ["feature"] = feature }));

    [Given("the manifest leaves {string} and {string} at their defaults")]
    public void GivenTheManifestLeavesAndAtTheirDefaults(string first, string second)
    {
        Remove(first);
        Remove(second);
    }

    [Given("the manifest has no {string}")]
    public void GivenTheManifestHasNo(string path) => Remove(path);

    [Given("the manifest declares {string}")]
    public void GivenTheManifestDeclares(string path) => Set(path, JsonValue.Create("^[0-9]{4}$"));

    [Given("the manifest declares no epic grammar")]
    public void GivenTheManifestDeclaresNoEpicGrammar() => Remove("identifiers.epic");

    [Given("the claim file set has epic {string} and feature {string}")]
    public void GivenTheClaimFileSetHasEpicAndFeature(string epic, string feature)
    {
        ClaimFileSet["epic"] = epic;
        ClaimFileSet["feature"] = feature;
    }

    [Given("the claim file set has epic {string}")]
    public void GivenTheClaimFileSetHasEpic(string epic) => ClaimFileSet["epic"] = epic;

    [Given("the claim file set has no epic")]
    public void GivenTheClaimFileSetHasNoEpic() => ClaimFileSet.Remove("epic");

    [Given("the manifest is pinned to schema {string}")]
    public void GivenTheManifestIsPinnedToSchema(string pin) => Manifest["schemaVersion"] = pin;

    [Given("{string} has no member beyond {string} and {string}")]
    public void GivenHasNoMemberBeyondAnd(string path, string first, string second)
    {
        var claim = (JsonObject) Node(path)!;
        foreach (var member in claim.Select(static entry => entry.Key).Where(member => member != first && member != second).ToList())
        {
            claim.Remove(member);
        }
    }

    [Given("a tree the check passes without {string}")]
    public void GivenATreeTheCheckPassesWithout(string path)
    {
        Remove(path);
        _baseline = Launch();
        _baseline.Value.ExitCode.Should().Be(0, _baseline.Value.Stderr);
    }

    [Given("the manifest's {string} is absent")]
    public void GivenTheManifestsIsAbsent(string path) => Remove(path);

    [Given("the manifest's {string} is an empty list")]
    public void GivenTheManifestsIsAnEmptyList(string path) => Set(path, new JsonArray());

    [Given("the manifest's {string} is one entry with no {string}")]
    public void GivenTheManifestsIsOneEntryWithNo(string path, string member) => ((JsonObject) ((JsonArray) Node(path)!)[0]!).Remove(member);

    [Given("the manifest's {string} is one entry beside {string} {string}")]
    public void GivenTheManifestsIsOneEntryBeside(string path, string source, string value) => Set(source, JsonValue.Create(value));

    [Given("the manifest's {string} is one entry beside no {string}")]
    public void GivenTheManifestsIsOneEntryBesideNo(string path, string source) => Remove(source);

    [Given("{string} declares scenarios tagged {string} and {string}")]
    public void GivenDeclaresScenariosTaggedAnd(string path, string first, string second) =>
        Tree.WriteRaw(
            path,
            $"Feature: Arithmetic\n\n  {first}\n  Scenario: Adds\n    Given two numbers\n\n  {second}\n  Scenario: Adds three\n    Given three numbers\n");

    [Given(@"^""([^""]*)"" carries ""([^""]*)"" on a (Feature|Rule|Scenario|Scenario Outline) at line (\d+)$")]
    public void GivenCarriesOnAnElementAtLine(string path, string tag, string element, int line)
    {
        var lines = new List<string>();
        if (element != "Feature")
        {
            lines.Add("Feature: Arithmetic");
        }

        if (line - 2 < lines.Count)
        {
            throw new InvalidOperationException(
                $"A {element} at line {line} leaves no line above it for {tag}: a tag sits on a line before the keyword it declares on.");
        }

        while (lines.Count < line - 2)
        {
            lines.Add(string.Empty);
        }

        lines.Add(tag);
        lines.AddRange(element switch
        {
            "Feature" => ["Feature: Arithmetic", string.Empty, "  Scenario: Adds", "    Given two numbers"],
            "Rule" => ["  Rule: Sums", string.Empty, "    Scenario: Adds", "      Given two numbers"],
            "Scenario" => ["  Scenario: Adds", "    Given two numbers"],
            _ => ["  Scenario Outline: Adds <a>", "    Given <a>", string.Empty, "    Examples:", "      | a |", "      | 1 |"],
        });
        Tree.WriteRaw(path, string.Join('\n', lines) + "\n");
    }

    [Given(@"^""([^""]*)"" has a scenario tagged (.+)$")]
    public void GivenHasAScenarioTagged(string path, string tags)
    {
        var written = string.Join(' ', Regex.Matches(tags, "\"([^\"]*)\"").Select(static match => match.Groups[1].Value));
        Tree.WriteRaw(path, $"Feature: Arithmetic\n\n  {written}\n  Scenario: Adds\n    Given two numbers\n");
        _keywordLines["Scenario"] = [4];
    }

    [Given("{string} has a Scenario Outline tagged {string} with three Examples rows")]
    public void GivenHasAScenarioOutlineTaggedWithThreeExamplesRows(string path, string tag)
    {
        Tree.WriteRaw(
            path,
            $"Feature: Arithmetic\n\n  {tag}\n  Scenario Outline: Adds <a>\n    Given <a>\n\n"
            + "    Examples:\n      | a |\n      | 1 |\n      | 2 |\n      | 3 |\n");
        _keywordLines["Scenario Outline"] = [4];
    }

    [Given("{string} has a Scenario Outline tagged {string} with an Examples block tagged {string}")]
    public void GivenHasAScenarioOutlineTaggedWithAnExamplesBlockTagged(string path, string tag, string blockTag)
    {
        Tree.WriteRaw(
            path,
            $"Feature: Arithmetic\n\n  {tag}\n  Scenario Outline: Adds <a>\n    Given <a>\n\n"
            + $"    {blockTag}\n    Examples:\n      | a |\n      | 1 |\n      | 2 |\n");
        _keywordLines["Examples rows"] = [10, 11];
    }

    [Given("{string} has a Rule tagged {string} holding two untagged scenarios")]
    public void GivenHasARuleTaggedHoldingTwoUntaggedScenarios(string path, string tag)
    {
        Tree.WriteRaw(
            path,
            $"Feature: Arithmetic\n\n  {tag}\n  Rule: Sums\n\n    Scenario: Adds two\n      Given two numbers\n\n"
            + "    Scenario: Adds three\n      Given three numbers\n");
        _keywordLines["Rule"] = [4];
        _keywordLines["Rule scenarios"] = [6, 9];
        _claimFile = path;
    }

    [Given(@"^""([^""]*)"" carries ""([^""]*)"" on (a Scenario|a Scenario Outline|a Rule with two children|the Feature)$")]
    public void GivenCarriesOn(string path, string tag, string element)
    {
        var lines = ScenarioSetLayout.ToArray();
        lines[element switch
        {
            "the Feature" => 0,
            "a Scenario" => 2,
            "a Scenario Outline" => 5,
            _ => 12,
        }] = tag;
        Tree.WriteRaw(path, string.Join('\n', lines) + "\n");
        _keywordLines["Scenario"] = [4];
        _keywordLines["Scenario Outline"] = [7];
        _keywordLines["Rule scenarios"] = [16, 19];
    }

    [Given("a catalogue built from {string}")]
    public void GivenACatalogueBuiltFrom(string path)
    {
        _claimFile = path;
        Tree.WriteRaw(
            path,
            "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n\n  @B-002\n  Scenario: Subtracts\n    Given two numbers\n");
        _run = Launch();
        _catalogue = Catalogue();
        _catalogue.Should().NotBeEmpty("the file declares B-001 and B-002");
    }

    [Given("the file's scenarios are reordered and retitled with their tags kept")]
    public void GivenTheFilesScenariosAreReorderedAndRetitledWithTheirTagsKept() =>
        Tree.WriteRaw(
            _claimFile ?? throw new InvalidOperationException("No claim file was written."),
            "Feature: Arithmetic\n\n  @B-002\n  Scenario: Takes one number from another\n    Given two numbers\n\n"
            + "  @B-001\n  Scenario: Sums two numbers\n    Given two numbers\n");

    [Given("the manifest sets {string} to {string} and {string} to {string}")]
    public void GivenTheManifestSetsToAndTo(string first, string firstTag, string second, string secondTag)
    {
        Set(first, new JsonArray(firstTag));
        Set(second, new JsonArray(secondTag));
    }

    [Given("the manifest leaves {string} at its default")]
    public void GivenTheManifestLeavesAtItsDefault(string path) => Remove(path);

    [Given("the glob matches {string} and {string}")]
    public void GivenTheGlobMatchesAnd(string first, string second)
    {
        Tree.WriteRaw(first, "Feature: First\n\n  @B-001\n  Scenario: Subtracts\n    Given two numbers\n");
        Tree.WriteRaw(second, "Feature: Second\n\n  @B-002\n  Scenario: Adds\n    Given two numbers\n");
    }

    [When("the check runs")]
    public void WhenTheCheckRuns() => _run = Launch();

    [When("the check runs again")]
    public void WhenTheCheckRunsAgain() => _run = Launch();

    [When("the check runs twice")]
    public void WhenTheCheckRunsTwice()
    {
        _run = Launch();
        _second = Launch();
    }

    [Then("the exit code is {int}")]
    public void ThenTheExitCodeIs(int code) => Run.ExitCode.Should().Be(code, Run.Stderr);

    [Then("stderr names {string}")]
    public void ThenStderrNames(string key) => Run.Stderr.Should().Contain(key);

    [Then("no violation is printed")]
    public void ThenNoViolationIsPrinted() => Run.Stdout.Should().NotMatchRegex(@": (error|warning) SPEC[0-9]");

    [Then("every claim comes from a specification's claims table")]
    public void ThenEveryClaimComesFromASpecificationsClaimsTable()
    {
        Run.ExitCode.Should().Be(0, Run.Stderr);
        var model = EngineServices.Reader().Read(Tree.Root, null);
        model.Features.Should().ContainSingle();
        model.Claims.Should().OnlyContain(claims => model.Features.Contains(claims.Key) && claims.Value.Section != null);
        model.Claims.Values.SelectMany(static claims => claims.Rows).Select(static claim => claim.Id).Should().Equal("B-001");
    }

    [Then("the claim file set declares no Feature")]
    public void ThenTheClaimFileSetDeclaresNoFeature()
    {
        var structure = EngineServices.Manifest(new FileSystem(), Tree.Root);
        structure.ClaimInput!.Source.Should().Be(SpechtClaimSource.Markdown);
        structure.ClaimInput.Files.Should().BeEmpty();
    }

    [Then("Feature {string} has the identity {string} {string}")]
    public void ThenFeatureHasTheIdentity(string feature, string epic, string expectedFeature)
    {
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        EngineServices.Manifest(new FileSystem(), Tree.Root).ClaimInput!.Files.Should().ContainSingle()
            .Which.Should().Match<SpechtClaimFileSet>(set => set.Feature == feature && set.Epic == epic && feature == expectedFeature);
    }

    [Then("the output equals the output of the same tree before this Feature")]
    public void ThenTheOutputEqualsTheOutputOfTheSameTreeBeforeThisFeature() =>
        Run.Should().Be((OutputBeforeThisFeature, string.Empty, 0));

    [Then("the output equals the output without {string}")]
    public void ThenTheOutputEqualsTheOutputWithout(string path) =>
        Run.Should().Be(_baseline ?? throw new InvalidOperationException($"No run without {path} was recorded."));

    [Then("Feature {string} has the claims {string} and {string}")]
    public void ThenFeatureHasTheClaimsAnd(string feature, string first, string second) =>
        ClaimSet(feature).Rows.Select(static claim => claim.Id).Should().Equal(first, second);

    [Then("no claims table is read for Feature {string}")]
    public void ThenNoClaimsTableIsReadForFeature(string feature) => ClaimSet(feature).Section.Should().BeNull();

    [Then("Feature {string} has the claim {string} located at {string} line {int}")]
    public void ThenFeatureHasTheClaimLocatedAtLine(string feature, string id, string path, int line)
    {
        var claim = ClaimSet(feature).Rows.Should().ContainSingle(claim => claim.Id == id).Subject;
        claim.Path.Should().Be(path);
        claim.Line.Should().Be(line);
    }

    [Then("Feature {string} has exactly the claim {string}")]
    public void ThenFeatureHasExactlyTheClaim(string feature, string id) =>
        ClaimSet(feature).Rows.Select(static claim => claim.Id).Should().Equal(id);

    [Then("no violation names {string} or {string}")]
    public void ThenNoViolationNamesOr(string first, string second) =>
        Diagnostics().Should().NotContain(line => line.Contains(first, StringComparison.Ordinal) || line.Contains(second, StringComparison.Ordinal));

    [Then("Feature {string} has the claim {string} exactly once")]
    public void ThenFeatureHasTheClaimExactlyOnce(string feature, string id)
    {
        ClaimSet(feature).Rows.Should().ContainSingle(claim => claim.Id == id);
        _claimId = id;
    }

    [Then(@"^the claim is located at the (Rule|Scenario Outline)'s keyword line$")]
    public void ThenTheClaimIsLocatedAtTheElementsKeywordLine(string element) =>
        Claim(_claimId).Line.Should().Be(_keywordLines[element].Single());

    [Then("no violation names either scenario")]
    public void ThenNoViolationNamesEitherScenario()
    {
        var places = _keywordLines["Rule scenarios"].Select(line => $"{_claimFile}({line})").Concat(["Adds two", "Adds three"]).ToArray();
        Diagnostics().Should().NotContain(line => places.Any(place => line.Contains(place, StringComparison.Ordinal)));
    }

    [Then("every claim has the same identifier, Feature identity and lifecycle as before")]
    public void ThenEveryClaimHasTheSameIdentifierFeatureIdentityAndLifecycleAsBefore() =>
        Catalogue().Should().BeEquivalentTo(_catalogue ?? throw new InvalidOperationException("No catalogue was built before."));

    [Then("the claim {string} has the status {string}")]
    public void ThenTheClaimHasTheStatus(string id, string status) => Claim(id).Declaration!.State.Should().Be(status);

    [Then("Feature {string} has the claim {string}")]
    public void ThenFeatureHasTheClaim(string feature, string id)
    {
        ClaimSet(feature).Rows.Should().ContainSingle(claim => claim.Id == id);
        _claimId = id;
    }

    [Then("the scenario is marked planned in the catalogue")]
    public void ThenTheScenarioIsMarkedPlannedInTheCatalogue() =>
        Claim(_claimId).Declaration!.Cases.Should().ContainSingle(item => item.Line == _keywordLines["Scenario"].Single())
            .Which.Planned.Should().BeTrue();

    [Then("that block's rows are marked planned in the catalogue")]
    public void ThenThatBlocksRowsAreMarkedPlannedInTheCatalogue()
    {
        var rows = _keywordLines["Examples rows"];
        Claim(_claimId).Declaration!.Cases.Where(item => rows.Contains(item.Line)).Select(static item => (item.Line, item.Planned))
            .Should().Equal(rows.Select(static row => (row, true)));
    }

    [Then(@"^the scenarios of claim ""([^""]*)"" are "
        + "(that Scenario|that Scenario Outline|both scenarios under the Rule|every Scenario and Scenario Outline in the file)$")]
    public void ThenTheScenariosOfClaimAre(string id, string scenarios) =>
        Claim(id).Declaration!.Scenarios.Should().Equal(scenarios switch
        {
            "that Scenario" => _keywordLines["Scenario"],
            "that Scenario Outline" => _keywordLines["Scenario Outline"],
            "both scenarios under the Rule" => _keywordLines["Rule scenarios"],
            _ => [.. _keywordLines["Scenario"], .. _keywordLines["Scenario Outline"], .. _keywordLines["Rule scenarios"]],
        });

    [Then("the two outputs are byte-identical")]
    public void ThenTheTwoOutputsAreByteIdentical() =>
        (_second ?? throw new InvalidOperationException("The check was not run twice.")).Should().Be(Run);

    [Then("the claims of {string} precede those of {string}")]
    public void ThenTheClaimsOfPrecedeThoseOf(string first, string second)
    {
        var paths = Model().ClaimSets.SelectMany(static set => set.Rows).Select(static claim => claim.Path).ToList();
        paths.Should().Contain(first).And.Contain(second);
        paths.FindLastIndex(path => path == first).Should().BeLessThan(paths.FindIndex(path => path == second));
    }

    [AfterScenario]
    public void DeleteTree() => _tree?.Dispose();

    private SpechtTree Tree => _tree ?? throw new InvalidOperationException("No root was prepared.");

    private (string Stdout, string Stderr, int ExitCode) Run => _run ?? throw new InvalidOperationException("The check was not run.");

    private JsonObject ClaimFileSet => (JsonObject) ((JsonArray) Node("claim.files")!)[0]!;

    private string ManifestPath => Path.Combine(Tree.Root, SpechtTree.ManifestPath);

    private JsonObject Manifest { get => field ?? throw new InvalidOperationException("No manifest was read."); set; }

    private SpechtModel Model() => EngineServices.Reader().Read(Tree.Root, null);

    private SpechtClaims ClaimSet(string feature)
    {
        Run.ExitCode.Should().NotBe(3, Run.Stderr);
        return Model().ClaimSets.Should().ContainSingle(set => set.FileSet!.Feature == feature).Subject;
    }

    private SpechtClaim Claim(string? id) =>
        Model().ClaimSets.SelectMany(static set => set.Rows).Should().ContainSingle(claim => claim.Id == id).Subject;

    private (string Id, string? Epic, string Feature, string State)[] Catalogue() =>
        Model().ClaimSets
            .SelectMany(static set => set.Rows.Select(claim => (claim.Id, set.FileSet!.Epic, set.FileSet.Feature, claim.Declaration!.State)))
            .ToArray();

    private string[] Diagnostics() =>
        Run.Stdout.Split('\n').Where(static line => Regex.IsMatch(line, ": (error|warning) SPEC[0-9]")).ToArray();

    private (string Stdout, string Stderr, int ExitCode) Launch()
    {
        File.WriteAllText(ManifestPath, Manifest.ToJsonString());
        return AcceptanceTool.Launch(Tree.Root, "--root", ".");
    }

    private JsonNode? Node(string path) =>
        path.Split('.').Aggregate<string, JsonNode?>(Manifest, static (node, member) => node?[member]);

    private void Set(string path, JsonNode? value)
    {
        var members = path.Split('.');
        var parent = Manifest;
        foreach (var member in members[..^1])
        {
            if (parent[member] is not JsonObject child)
            {
                child = [];
                parent[member] = child;
            }

            parent = child;
        }

        parent[members[^1]] = value;
    }

    private void Remove(string path)
    {
        var members = path.Split('.');
        (members.Length == 1 ? Manifest : Node(string.Join('.', members[..^1])) as JsonObject)?.Remove(members[^1]);
    }

    private static readonly string OutputBeforeThisFeature = string.Join(
        Environment.NewLine,
        "specifications: epics 0, features 1",
        "items: 0",
        "rules evaluated: 23",
        "errors: 0, warnings: 0",
        string.Empty);

    private static readonly string[] ScenarioSetLayout =
    [
        string.Empty,
        "Feature: Arithmetic",
        string.Empty,
        "  Scenario: Adds",
        "    Given two numbers",
        string.Empty,
        "  Scenario Outline: Adds <a>",
        "    Given <a>",
        string.Empty,
        "    Examples:",
        "      | a |",
        "      | 1 |",
        string.Empty,
        "  Rule: Sums",
        string.Empty,
        "    Scenario: Adds three",
        "      Given three numbers",
        string.Empty,
        "    Scenario: Adds four",
        "      Given four numbers",
    ];

    private readonly Dictionary<string, int[]> _keywordLines = new(StringComparer.Ordinal);

    private (string Id, string? Epic, string Feature, string State)[]? _catalogue;

    private (string Stdout, string Stderr, int ExitCode)? _second;

    private string? _claimFile;

    private string? _claimId;

    private SpechtTree? _tree;

    private (string Stdout, string Stderr, int ExitCode)? _run;

    private (string Stdout, string Stderr, int ExitCode)? _baseline;
}
