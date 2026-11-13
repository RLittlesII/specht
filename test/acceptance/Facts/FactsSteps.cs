using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Facts;

/// <summary>
/// Steps for <c>src/tool/Features/Facts/.spec/facts.feature</c> (<c>0132-F2</c>): item 0364 binds B-001 to B-004, B-014,
/// B-017, B-021 to B-023 and B-026. Each launches the built tool over a synthetic tree, and a scenario naming no form runs
/// <c>specht facts --json</c>, since the text form is item 0370's. The root sits one folder below a temporary directory,
/// so a run that writes beside the root is seen. The rest of the file's scenarios stay pending for their items.
/// </summary>
[Binding]
[Scope(Feature = "The facts command")]
public sealed class FactsSteps
{
    [Given("a repository whose manifest pins schema version {word}")]
    public void GivenRepository(string pin)
    {
        _tree = new SpechtTree(["repo"]);
        _tree.Pin(pin);
    }

    [Given("two Feature specifications, each with claims, constraints, out-of-scope rows and open questions")]
    public void GivenTwoSpecifications() => WriteSpecifications(Tree);

    [Given("the first specification depends on the second")]
    public void GivenFirstDependsOnSecond()
    {
        _first["depends_on"] = "[\"F2\"]";
        WriteSpecifications(Tree);
    }

    [Given("the second specification lists the first among what it blocks")]
    public void GivenSecondBlocksFirst()
    {
        _second["blocks"] = "[\"F1\"]";
        WriteSpecifications(Tree);
    }

    [Given("the first specification has a violation the check reports")]
    public void GivenFirstHasViolation()
    {
        _first["spec_status"] = "nearly";
        WriteSpecifications(Tree);
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(Tree.Root, "--json");
        ExitCode.Should().Be(1, Stderr);
        _violations = JsonNode.Parse(Stdout)!["violations"]!.AsArray();
        _violations.Should().NotBeEmpty();
    }

    [Given("the same specifications under a manifest pinning schema version {word}")]
    public void GivenSameSpecificationsUnderPin(string pin)
    {
        _other = new SpechtTree();
        _other.Pin(pin);
        WriteSpecifications(_other);
    }

    [Given("the root does not exist")]
    public void GivenRootDoesNotExist() => _rootArgument = "absent";

    [Given("the root holds no manifest")]
    public void GivenNoManifest() => File.Delete(Path.Combine(Tree.Root, SpechtTree.ManifestPath));

    [Given("the manifest is invalid")]
    public void GivenInvalidManifest() => Tree.WriteRaw(SpechtTree.ManifestPath, "{");

    [Given("the manifest pins a version the tool does not ship")]
    public void GivenUnshippedPin() => Tree.Pin("7.0.0");

    [Given("the repository holds an epic file the check discovers")]
    public void GivenEpicFile()
    {
        Tree.WriteEpic("0001");
        EngineServices.Reader().Read(Tree.Root, null).Epics.Select(static epic => epic.RelativePath).Should().Equal(EpicPath);
    }

    [When("the facts are given")]
    [When("the facts are given as JSON")]
    public void WhenFactsAreGiven()
    {
        _before = Snapshot();
        _result = AcceptanceTool.Launch(Tree.Root, "facts", "--root", _rootArgument, "--json");
    }

    [When("the facts are given as JSON under each pin")]
    public void WhenFactsAreGivenUnderEachPin()
    {
        WhenFactsAreGiven();
        _otherResult = AcceptanceTool.Launch(_other!.Root, "facts", "--json");
    }

    [When("the facts are given twice")]
    public void WhenFactsAreGivenTwice()
    {
        WhenFactsAreGiven();
        _otherResult = AcceptanceTool.Launch(Tree.Root, "facts", "--root", _rootArgument, "--json");
    }

    [Then("they hold both specifications, in the order the check discovers them")]
    public void ThenBothInDiscoveryOrder()
    {
        var discovered = EngineServices.Reader().Read(Tree.Root, null).Features.Select(static feature => feature.RelativePath).ToArray();
        discovered.Should().HaveCount(2);
        Paths.Should().Equal(discovered);
    }

    [Then("the output is one JSON document")]
    public void ThenOneDocument()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        using var document = JsonDocument.Parse(_result.Stdout);
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Then("the document validates against the published facts schema")]
    public void ThenValidatesAgainstSchema()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "docs", "schema", "facts.schema.json");
        File.Exists(path).Should().BeTrue("the facts schema is published at docs/schema/facts.schema.json (OQ-1)");
        using var json = JsonDocument.Parse(_result.Stdout);
        var result = JsonSchema.FromText(File.ReadAllText(path))
            .Evaluate(json.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });
        result.IsValid.Should().BeTrue(
            string.Join("; ", (result.Details ?? []).Where(static detail => detail.Errors is not null)
                .SelectMany(static detail => detail.Errors!.Select(error => $"{detail.InstanceLocation} {error.Value}"))));
    }

    [Then("each specification carries its path relative to the root, its qualified Feature id, its document maturity and its delivery status")]
    public void ThenIdentityAndStatuses()
    {
        Specifications.Select(static specification => (
                specification["path"]!.GetValue<string>(),
                specification["id"]!.GetValue<string>(),
                specification["specStatus"]!.GetValue<string>(),
                specification["status"]!.GetValue<string>()))
            .Should().BeEquivalentTo([
                (FirstPath, "0001-F1", _first["spec_status"], _first["status"]),
                (SecondPath, "0001-F2", _second["spec_status"], _second["status"]),
            ]);
    }

    [Then("the first specification carries the second among what it depends on")]
    public void ThenFirstDependsOnSecond() =>
        Specification(FirstPath)["dependsOn"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("F2");

    [Then("the second carries the first among what it blocks")]
    public void ThenSecondBlocksFirst() =>
        Specification(SecondPath)["blocks"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("F1");

    [Then("the document carries no violation, no count and no verdict")]
    public void ThenNoVerdict()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        MemberPaths(JsonNode.Parse(_result.Stdout), string.Empty).Select(static path => path[(path.LastIndexOf('.') + 1)..]).Should()
            .NotContain(["violations", "errorCount", "warningCount", "rulesEvaluated", "itemCount", "ruleId", "severity"]);
        foreach (var violation in _violations!)
        {
            _result.Stdout.Should().NotContain(violation!["ruleId"]!.GetValue<string>());
            _result.Stdout.Should().NotContain(violation["message"]!.GetValue<string>());
        }
    }

    [Then("both documents carry the same members")]
    public void ThenSameMembers()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        _otherResult.ExitCode.Should().Be(0, _otherResult.Stderr);
        var members = new[] { _result.Stdout, _otherResult.Stdout }
            .Select(static output => MemberPaths(JsonNode.Parse(output), string.Empty).Distinct().Order(StringComparer.Ordinal).ToArray())
            .ToArray();
        members[0].Should().Contain("specifications[].path");
        members[1].Should().Equal(members[0]);
    }

    [Then("the command exits with code {int}")]
    public void ThenExitCode(int code) => _result.ExitCode.Should().Be(code, _result.Stderr);

    [Then("both outputs are byte-identical")]
    public void ThenByteIdentical()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        _otherResult.ExitCode.Should().Be(0, _otherResult.Stderr);
        _result.Stdout.Should().NotBeEmpty();
        _otherResult.Stdout.Should().Be(_result.Stdout);
    }

    [Then("no file under the root or elsewhere is created, modified or deleted")]
    public void ThenNothingWritten()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        Snapshot().Should().BeEquivalentTo(_before);
    }

    [Then("they hold the two Feature specifications and not the epic")]
    public void ThenFeaturesNotEpic() => Paths.Should().BeEquivalentTo([FirstPath, SecondPath]).And.NotContain(EpicPath);

    [AfterScenario]
    public void DisposeTrees()
    {
        _tree?.Dispose();
        _other?.Dispose();
    }

    private JsonObject[] Specifications
    {
        get
        {
            _result.ExitCode.Should().Be(0, _result.Stderr);
            return JsonNode.Parse(_result.Stdout)!["specifications"]!.AsArray().Select(static specification => specification!.AsObject()).ToArray();
        }
    }

    private string[] Paths => Specifications.Select(static specification => specification["path"]!.GetValue<string>()).ToArray();

    private SpechtTree Tree => _tree ?? throw new InvalidOperationException("No repository was prepared.");

    private static IEnumerable<string> MemberPaths(JsonNode? node, string prefix) => node switch
    {
        JsonObject members => members.SelectMany(member => MemberPaths(member.Value, prefix.Length == 0 ? member.Key : $"{prefix}.{member.Key}")
            .Prepend(prefix.Length == 0 ? member.Key : $"{prefix}.{member.Key}")),
        JsonArray items => items.SelectMany(item => MemberPaths(item, prefix + "[]")),
        _ => [],
    };

    private void WriteSpecifications(SpechtTree tree)
    {
        tree.WriteCoLocatedFeature("src/first", "0001", "F1", _first, Sections);
        tree.WriteCoLocatedFeature("src/second", "0001", "F2", _second, Sections);
    }

    private Dictionary<string, byte[]> Snapshot()
    {
        var outside = Path.GetDirectoryName(Tree.Root)!;
        return Directory.GetFiles(outside, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(outside, path).Replace('\\', '/'), static path => File.ReadAllBytes(path));
    }

    private JsonObject Specification(string path) =>
        Specifications.Should().ContainSingle(specification => specification["path"]!.GetValue<string>() == path).Which;

    private const string FirstPath = "src/first/.spec/README.md";
    private const string SecondPath = "src/second/.spec/README.md";
    private const string EpicPath = "epics/0001-epic/epic.md";

    private static readonly IReadOnlyList<string> Sections = SpechtTree.SectionsWith(
        "11. Open Questions",
        "## 11. Open Questions\n\n| ID | Question | Blocks | Resolution |\n| -- | -------- | ------ | ---------- |\n| OQ-1 | Which one? | B-001 | Open |\n");

    private readonly Dictionary<string, string> _first = new(StringComparer.Ordinal) { ["spec_status"] = "approved", ["status"] = "ready" };
    private readonly Dictionary<string, string> _second = new(StringComparer.Ordinal) { ["spec_status"] = "draft", ["status"] = "in-progress" };
    private SpechtTree? _tree;
    private SpechtTree? _other;
    private string _rootArgument = ".";
    private JsonArray? _violations;
    private Dictionary<string, byte[]> _before = [];
    private (string Stdout, string Stderr, int ExitCode) _result;
    private (string Stdout, string Stderr, int ExitCode) _otherResult;
}
