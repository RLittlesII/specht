using System.IO.Abstractions;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Acceptance.NativeClaims;

/// <summary>
/// Steps for <c>src/specht/Rules/NativeClaims/.spec/native-claims.feature</c> (0001-F16), bound for the claim members of
/// item 0389: B-002, B-027, B-028, B-029 and B-031. The other scenarios bind only the Background and are left out as
/// pending. A root is a synthetic tree holding one co-located Markdown specification, so a claim read from a claims table is
/// observable, and its manifest declares no epic grammar, as the Background's claim file set carries no epic (B-027). The
/// manifest is edited in memory and written when the check runs; "the check runs" launches the built tool on the root, the
/// only place an exit code and its streams are seen. "The output of the same tree before this Feature" is written here, not
/// produced by the tool under test: this tree's clean output under schema <c>0.3.1</c>, exit <c>0</c>, an empty stderr and
/// the four summary lines that <c>specht.tool</c> <c>0.5.35-preview</c>, which predates this Feature, prints for it (B-028).
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

    [When("the check runs")]
    public void WhenTheCheckRuns() => _run = Launch();

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

    [AfterScenario]
    public void DeleteTree() => _tree?.Dispose();

    private SpechtTree Tree => _tree ?? throw new InvalidOperationException("No root was prepared.");

    private (string Stdout, string Stderr, int ExitCode) Run => _run ?? throw new InvalidOperationException("The check was not run.");

    private JsonObject ClaimFileSet => (JsonObject) ((JsonArray) Node("claim.files")!)[0]!;

    private string ManifestPath => Path.Combine(Tree.Root, SpechtTree.ManifestPath);

    private JsonObject Manifest { get => field ?? throw new InvalidOperationException("No manifest was read."); set; }

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

    private SpechtTree? _tree;

    private (string Stdout, string Stderr, int ExitCode)? _run;

    private (string Stdout, string Stderr, int ExitCode)? _baseline;
}
