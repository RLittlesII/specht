using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Trace;

/// <summary>
/// Steps for <c>src/tool/Features/Trace/.spec/trace.feature</c> (<c>0001-F13</c>): item 0442 binds B-001 to B-003, B-006,
/// B-007, B-009 and B-019. Each launches the built tool over a synthetic tree: one Feature specification with claims
/// B-001 to B-003, a traceability row and a tagged scenario for each until a step changes them, and one test file under
/// <c>test/</c> holding the test methods a step names as held. The scenarios of B-004, B-005, B-008, B-010 to B-018 and
/// B-020 stay pending for items 0443 to 0445: each keeps a step no binding here matches.
/// </summary>
[Binding]
[Scope(Feature = "The trace command")]
public sealed class TraceSteps
{
    [Given("a repository whose manifest pins schema version {word}")]
    public void GivenRepository(string pin)
    {
        _tree = new SpechtTree();
        _tree.Pin(pin);
    }

    [Given("a Feature specification with three claims beside one companion scenario file")]
    public void GivenFeatureWithThreeClaims()
    {
        _rows["B-001"] = "| B-001 | Tracing one | Missing | Missing |";
        _rows["B-002"] = "| B-002 | Tracing another | Missing | Missing |";
        _rows["B-003"] = "| B-003 | Tracing a third | Missing | Missing |";
        _scenarios.AddRange([("B-001", "Tracing one"), ("B-002", "Tracing another"), ("B-003", "Tracing a third")]);
    }

    [Given("the manifest declares the test files {string}")]
    public void GivenTestFiles(string glob)
    {
        var path = Path.Combine(Tree.Root, SpechtTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest["tests"] = new JsonObject { ["files"] = new JsonArray(glob) };
        File.WriteAllText(path, manifest.ToJsonString());
    }

    [Given("claim B-002's row is {string}, two scenarios carry its tag, and it names three tests of which two resolve")]
    public void GivenB002RowWithTwoScenariosAndThreeTests(string status)
    {
        _rows["B-002"] = $"| B-002 | Tracing another | `TracesAnother`, `TracesAgain`, `TracesNowhere` | {status} |";
        _scenarios.Add(("B-002", "Tracing again"));
        _held.AddRange(["TracesAnother", "TracesAgain"]);
    }

    [Given("claim B-003 has no traceability row")]
    public void GivenB003HasNoRow() => _rows["B-003"] = null;

    [Given("claim B-002's row is {string} and names two tests")]
    public void GivenB002RowNamingTwoTests(string status)
    {
        _rows["B-002"] = $"| B-002 | Tracing another | `TracesAnother`, `TracesNowhere` | {status} |";
        _held.Add("TracesAnother");
    }

    [Given("claim B-002's row is {string} and names a test a file holds")]
    public void GivenB002RowNamingAHeldTest(string status)
    {
        _rows["B-002"] = $"| B-002 | Tracing another | `TracesAnother` | {status} |";
        _held.Add("TracesAnother");
    }

    [Given("claim B-001 names one test that resolves and one that does not")]
    public void GivenB001NamesAResolvingAndAnUnresolvedTest()
    {
        _rows["B-001"] = "| B-001 | Tracing one | `TracesOne`, `TracesNone` | Partial |";
        _held.Add("TracesOne");
    }

    [Given("a claim whose row is {string} and whose tests do not resolve")]
    public void GivenAClaimWhoseTestsDoNotResolve(string status) =>
        _rows["B-001"] = $"| B-001 | Tracing one | `TracesNowhere`, `TracesNeither` | {status} |";

    [Given("the root named does not exist")]
    public void GivenRootDoesNotExist() => _rootArgument = "absent";

    [Given("the root holds no manifest")]
    public void GivenNoManifest() => File.Delete(Path.Combine(Tree.Root, SpechtTree.ManifestPath));

    [Given("the manifest is one the check rejects")]
    public void GivenRejectedManifest() => Tree.WriteRaw(SpechtTree.ManifestPath, "{");

    [Given("a second Feature specification discovered after the first")]
    public void GivenSecondSpecification()
    {
        _second = true;
        WriteTree();
        EngineServices.Reader().Read(Tree.Root, null).Features.Select(static feature => feature.RelativePath)
            .Should().Equal(FirstPath, SecondPath);
    }

    [Given("claim B-002's row is {string} and names one test a file holds and one no file holds")]
    public void GivenB002RowNamingAHeldAndAMissingTest(string status)
    {
        _rows["B-002"] = $"| B-002 | Tracing another | `TracesAnother`, `TracesNowhere` | {status} |";
        _held.Add("TracesAnother");
    }

    [When("specht trace runs")]
    public void WhenTraceRuns()
    {
        WriteTree();
        _result = AcceptanceTool.Launch(Tree.Root, "trace", "--root", _rootArgument);
    }

    [When("specht trace --json runs")]
    public void WhenTraceRunsAsJson()
    {
        WriteTree();
        _result = AcceptanceTool.Launch(Tree.Root, "trace", "--root", _rootArgument, "--json");
    }

    [When("specht trace --json runs twice")]
    public void WhenTraceRunsAsJsonTwice()
    {
        WhenTraceRunsAsJson();
        _again = AcceptanceTool.Launch(Tree.Root, "trace", "--root", _rootArgument, "--json");
    }

    [When("the check runs with --json")]
    public void WhenTheCheckRunsAsJson()
    {
        WriteTree();
        _result = AcceptanceTool.Launch(Tree.Root, "--json");
    }

    [Then("it prints three lines for that specification")]
    public void ThenThreeLines() =>
        Lines.Where(static line => line.StartsWith(FirstPath + "\t", StringComparison.Ordinal)).Should().HaveCount(3);

    [Then("the line for {word} holds the specification's path, {string}, {string}, {int} scenarios and {int} of {int} tests resolved")]
    public void ThenTheLineHolds(string claim, string id, string status, int scenarios, int resolved, int named) =>
        LineOf(claim).Should().Be($"{FirstPath}\t{id}\t{status}\t{scenarios}\t{resolved}/{named}");

    [Then("the line for {word} shows the status {string} and {int} of {int} tests resolved")]
    public void ThenTheLineShowsStatusAndTests(string claim, string status, int resolved, int named)
    {
        var fields = LineOf(claim).Split('\t');
        fields.Should().HaveCount(5);
        (fields[0], fields[1], fields[2], fields[4]).Should().Be((FirstPath, claim, status, $"{resolved}/{named}"));
    }

    [Then("the line for {word} shows {int} of {int} tests resolved")]
    public void ThenTheLineShowsTests(string claim, int resolved, int named)
    {
        var fields = LineOf(claim).Split('\t');
        fields.Should().HaveCount(5);
        fields[4].Should().Be($"{resolved}/{named}");
    }

    [Then("stdout is one JSON document")]
    public void ThenOneDocument()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        using var document = JsonDocument.Parse(_result.Stdout);
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Then("the document validates against the published trace schema")]
    public void ThenValidatesAgainstTheTraceSchema()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "docs", "schema", "trace.schema.json");
        File.Exists(path).Should().BeTrue("the trace schema is published at docs/schema/trace.schema.json (B-002)");
        using var json = JsonDocument.Parse(_result.Stdout);
        var result = JsonSchema.FromText(File.ReadAllText(path))
            .Evaluate(json.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });
        result.IsValid.Should().BeTrue(Errors(result));
    }

    [Then("that test carries its name and {string}, and no file or line")]
    public void ThenThatTestCarriesNoFileOrLine(string resolution)
    {
        var test = Claim("B-002")["tests"]!.AsArray().Should().ContainSingle().Which!.AsObject();
        test["name"]!.GetValue<string>().Should().Be("TracesAnother");
        test["resolution"]!.GetValue<string>().Should().Be(resolution);
        test.ContainsKey("file").Should().BeFalse();
        test.ContainsKey("line").Should().BeFalse();
    }

    [Then("the specification carries its path, its id and its three claims")]
    public void ThenTheSpecificationCarriesItsPathIdAndClaims()
    {
        var specification = Specification(FirstPath);
        specification["id"]!.GetValue<string>().Should().Be("0001-F1");
        specification["claims"]!.AsArray().Select(static claim => claim!["id"]!.GetValue<string>()).Should().Equal(ClaimIds);
    }

    [Then("claim B-001 carries its line, its claim status, its matrix row's line and four cells, and each scenario's title, file and line")]
    public void ThenB001CarriesItsRowAndScenarios()
    {
        var claim = Claim("B-001");
        claim["line"]!.GetValue<int>().Should().Be(FileLine(FirstPath, "| B-001 | It traces one claim."));
        claim["status"]!.GetValue<string>().Should().Be("Active");
        var row = claim["row"]!;
        (row["line"]!.GetValue<int>(), row["claim"]!.GetValue<string>(), row["scenario"]!.GetValue<string>(),
                row["test"]!.GetValue<string>(), row["status"]!.GetValue<string>())
            .Should().Be((FileLine(FirstPath, "| B-001 | Tracing one |"), "B-001", "Tracing one", "TracesOne, TracesNone", "Partial"));
        claim["scenarios"]!.AsArray()
            .Select(static scenario => (scenario!["title"]!.GetValue<string>(), scenario["file"]!.GetValue<string>(), scenario["line"]!.GetValue<int>()))
            .Should().Equal(("Tracing one", CompanionPath, FileLine(CompanionPath, "  Scenario: Tracing one")));
    }

    [Then("the test that resolves carries its name, {string}, its file and its line")]
    public void ThenTheResolvingTestCarriesItsFileAndLine(string resolution)
    {
        var test = Test("B-001", "TracesOne");
        test["resolution"]!.GetValue<string>().Should().Be(resolution);
        test["file"]!.GetValue<string>().Should().Be(TestPath);
        test["line"]!.GetValue<int>().Should().Be(FileLine(TestPath, "    public void TracesOne("));
    }

    [Then("the test that does not carries its name and {string}, and no file or line")]
    public void ThenTheUnresolvedTestCarriesNoFileOrLine(string resolution)
    {
        var test = Test("B-001", "TracesNone");
        test["resolution"]!.GetValue<string>().Should().Be(resolution);
        test.ContainsKey("file").Should().BeFalse();
        test.ContainsKey("line").Should().BeFalse();
    }

    [Then("the exit code is {int}")]
    public void ThenTheExitCodeIs(int code) => _result.ExitCode.Should().Be(code, _result.Stderr);

    [Then("the two outputs are byte-for-byte the same")]
    public void ThenTheTwoOutputsAreTheSame()
    {
        _result.ExitCode.Should().Be(0, _result.Stderr);
        _again.ExitCode.Should().Be(0, _again.Stderr);
        _result.Stdout.Should().NotBeEmpty();
        _again.Stdout.Should().Be(_result.Stdout);
    }

    [Then("the specifications appear in discovery order and each one's claims in claim order")]
    public void ThenTheSpecificationsAndClaimsAreInOrder()
    {
        Specifications.Select(static specification => specification["path"]!.GetValue<string>()).Should().Equal(FirstPath, SecondPath);
        Specification(FirstPath)["claims"]!.AsArray().Select(static claim => claim!["id"]!.GetValue<string>()).Should().Equal(ClaimIds);
        Specification(SecondPath)["claims"]!.AsArray().Select(static claim => claim!["id"]!.GetValue<string>()).Should().Equal("B-002", "B-001");
    }

    [Then("the report holds no trace of the claims")]
    public void ThenTheReportHoldsNoTrace()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "docs", "schema", "report.schema.json");
        using var json = JsonDocument.Parse(_result.Stdout);
        var result = JsonSchema.FromText(File.ReadAllText(path))
            .Evaluate(json.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });
        result.IsValid.Should().BeTrue(Errors(result));
        MemberNames(JsonNode.Parse(_result.Stdout)).Should().NotContain(["specifications", "claims", "row", "scenarios", "tests", "resolution"]);
    }

    [Then("both tests' resolution is {string}")]
    public void ThenBothTestsResolveAs(string resolution) =>
        Claim("B-002")["tests"]!.AsArray()
            .Select(static test => (test!["name"]!.GetValue<string>(), test["resolution"]!.GetValue<string>()))
            .Should().Equal(("TracesAnother", resolution), ("TracesNowhere", resolution));

    [AfterScenario]
    public void DisposeTree() => _tree?.Dispose();

    private SpechtTree Tree => _tree ?? throw new InvalidOperationException("No repository was prepared.");

    private string[] Lines
    {
        get
        {
            _result.ExitCode.Should().Be(0, _result.Stderr);
            return _result.Stdout.ReplaceLineEndings("\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        }
    }

    private JsonObject[] Specifications
    {
        get
        {
            _result.ExitCode.Should().Be(0, _result.Stderr);
            return JsonNode.Parse(_result.Stdout)!["specifications"]!.AsArray().Select(static specification => specification!.AsObject()).ToArray();
        }
    }

    private static IEnumerable<string> MemberNames(JsonNode? node) => node switch
    {
        JsonObject members => members.SelectMany(static member => MemberNames(member.Value).Prepend(member.Key)),
        JsonArray items => items.SelectMany(MemberNames),
        _ => [],
    };

    private static string Errors(EvaluationResults result) =>
        string.Join("; ", (result.Details ?? []).Where(static detail => detail.Errors is not null)
            .SelectMany(static detail => detail.Errors!.Select(error => $"{detail.InstanceLocation} {error.Value}")));

    private JsonObject Specification(string path) =>
        Specifications.Should().ContainSingle(specification => specification["path"]!.GetValue<string>() == path).Which;

    private JsonObject Claim(string id) =>
        Specification(FirstPath)["claims"]!.AsArray().Should().ContainSingle(claim => claim!["id"]!.GetValue<string>() == id).Which!.AsObject();

    private JsonObject Test(string claim, string name) =>
        Claim(claim)["tests"]!.AsArray().Should().ContainSingle(test => test!["name"]!.GetValue<string>() == name).Which!.AsObject();

    private string LineOf(string claim) =>
        Lines.Should().ContainSingle(line => line.StartsWith(FirstPath + "\t", StringComparison.Ordinal)
            && line.Split('\t').ElementAtOrDefault(1) == claim).Which;

    private int FileLine(string path, string prefix) =>
        Array.FindIndex(File.ReadAllLines(Path.Combine(Tree.Root, path)), line => line.StartsWith(prefix, StringComparison.Ordinal)) + 1;

    private void WriteTree()
    {
        var claims = "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It traces one claim. | brd | Active |\n| B-002 | It traces another. | brd | Active |\n"
            + "| B-003 | It traces a third. | brd | Active |\n";
        var matrix = "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
            + string.Concat(ClaimIds.Select(id => _rows[id]).OfType<string>().Select(static row => row + "\n"));
        var sections = SpechtTree.SectionsWith("3. Acceptance Criteria", claims).ToList();
        sections[sections.FindIndex(static section => section.StartsWith("## 9. Traceability Matrix\n", StringComparison.Ordinal))] = matrix;
        Tree.WriteFeatureFile(
            Tree.WriteCoLocatedFeature(FirstArea, "0001", "F1", null, sections),
            "Feature: Trace\n" + string.Concat(_scenarios.Select(static scenario =>
                $"\n  @{scenario.Tag}\n  Scenario: {scenario.Title}\n    Given a thing\n")));
        Tree.WriteRaw(
            TestPath,
            "namespace Trace;\n\npublic sealed class TraceTests\n{\n"
                + string.Concat(_held.Select(static name => $"    public void {name}() {{ }}\n")) + "}\n");
        if (_second)
        {
            var second = SpechtTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + "| B-002 | It traces second. | brd | Active |\n| B-001 | It traces first. | brd | Active |\n");
            Tree.WriteCoLocatedFeature(SecondArea, "0001", "F2", null, second);
        }
    }

    private const string FirstArea = "src/area";
    private const string FirstPath = FirstArea + "/.spec/README.md";
    private const string CompanionPath = FirstArea + "/.spec/feature.feature";
    private const string SecondArea = "src/second";
    private const string SecondPath = SecondArea + "/.spec/README.md";
    private const string TestPath = "test/TraceTests.cs";

    private static readonly string[] ClaimIds = ["B-001", "B-002", "B-003"];

    private readonly Dictionary<string, string?> _rows = new(StringComparer.Ordinal);
    private readonly List<(string Tag, string Title)> _scenarios = [];
    private readonly List<string> _held = [];
    private SpechtTree? _tree;
    private bool _second;
    private string _rootArgument = ".";
    private (string Stdout, string Stderr, int ExitCode) _result;
    private (string Stdout, string Stderr, int ExitCode) _again;
}
