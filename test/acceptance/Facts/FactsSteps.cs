using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Facts;

/// <summary>
/// Steps for <c>src/tool/Features/Facts/.spec/facts.feature</c> (<c>0132-F2</c>): item 0364 binds B-001 to B-004, B-014,
/// B-017, B-021 to B-023 and B-026, item 0365 binds B-005, B-006, B-012, B-013 and B-025, item 0366 binds B-007,
/// B-008 and B-015, and item 0368 binds B-011. Each launches the built
/// tool over a synthetic tree, and a scenario naming no form runs <c>specht facts --json</c>, since the text form is item
/// 0370's. The root sits one folder below a temporary directory, so a run that writes beside the root is seen. The rest
/// of the file's scenarios stay pending for their items.
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

    [Given("a claim of the first specification is withdrawn")]
    public void GivenWithdrawnClaim()
    {
        var marker = JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpechtTree.ManifestPath)))!["markers"]!["withdrawn"]!.GetValue<string>();
        _firstSections = Replace(_firstSections, "3. Acceptance Criteria", ClaimsSection(FirstClaim, $"| B-002 | It gives them as JSON. | brd | {marker} |"));
        _withdrawn = marker;
        WriteSpecifications(Tree);
    }

    [Given("the first specification has two claims sharing one id")]
    public void GivenTwoClaimsSharingOneId()
    {
        _firstSections = Replace(_firstSections, "3. Acceptance Criteria", ClaimsSection(FirstClaim, "| B-001 | It gives the facts again. | brd | Active |"));
        WriteSpecifications(Tree);
        var (_, Stderr, ExitCode) = AcceptanceTool.Launch(Tree.Root, "--json");
        ExitCode.Should().Be(1, Stderr);
    }

    [Given("a claim with a traceability row and a scenario carrying its tag")]
    public void GivenClaimWithCoverage()
    {
        _firstSections = Replace(
            _firstSections,
            "9. Traceability Matrix",
            MatrixSection("| B-001 | Crossing the zebra | `ZebraTests.Crossing` | Covered |", "| B-002 | It gives them as JSON | Missing | Missing |"));
        _firstFeatureFile = FeatureFile.Replace("It gives the facts", "Crossing the zebra", StringComparison.Ordinal);
        WriteSpecifications(Tree);
        var first = EngineServices.Reader().Read(Tree.Root, null).Features.Single(static feature => feature.RelativePath == FirstPath);
        first.FeatureFiles.SelectMany(static file => file.Scenarios)
            .Should().Contain(static scenario => scenario.Title == "Crossing the zebra" && scenario.Tags.Any(static tag => tag.Id == "B-001"));
    }

    [Given("an open question whose resolution reads {string}")]
    public void GivenOpenQuestionResolution(string resolution)
    {
        _firstSections = Replace(_firstSections, "11. Open Questions", OpenQuestionsSection($"| OQ-1 | Which one? | B-001 | {resolution} |"));
        _resolution = resolution;
        WriteSpecifications(Tree);
    }

    [Given("the first specification has no out-of-scope section")]
    public void GivenNoOutOfScopeSection()
    {
        _firstSections = _firstSections.Where(static section => !section.StartsWith("## 5. Out of Scope\n", StringComparison.Ordinal)).ToList();
        WriteSpecifications(Tree);
        EngineServices.Reader().Read(Tree.Root, null).Features.Single(static feature => feature.RelativePath == FirstPath)
            .Document.Section("5. Out of Scope").Should().BeNull();
    }

    [Given("the first specification has an out-of-scope section whose table the parse cannot read")]
    public void GivenUnreadableOutOfScopeTable()
    {
        _firstSections = Replace(
            _firstSections,
            "5. Out of Scope",
            "## 5. Out of Scope\n\n| # | Item | Exclusion Reason |\n| 1 | Claim coverage | Owned by the trace command |\n");
        WriteSpecifications(Tree);
        var section = EngineServices.Reader().Read(Tree.Root, null).Features.Single(static feature => feature.RelativePath == FirstPath)
            .Document.Section("5. Out of Scope");
        section.Should().NotBeNull();
        section!.Rows.Should().BeEmpty();
    }

    [Given("the first specification has one decision record, one architecture decision record and one lesson beside it")]
    public void GivenRecordsBesideFirst()
    {
        Tree.WriteRaw(DecisionPath, "---\ntitle: \"Decision 0001: the records are given\"\ntype: decision\n---\n\n# Decision 0001\n");
        Tree.WriteRaw(
            AdrPath,
            "---\ntitle: \"ADR-0001: Read each record as written\"\ntype: adr\n---\n\n# ADR-0001\n\n## Status\n\nAccepted, 2026-10-12, by the owner.\n");
        Tree.WriteRaw(LessonPath, "---\ntitle: \"Lesson 0001: a record is read as written\"\ntype: lesson\n---\n\n# Lesson 0001\n");
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

    [Then("each claim carries its id, its text, its status and the line it is on")]
    public void ThenEachClaim()
    {
        foreach (var path in new[] { FirstPath, SecondPath })
        {
            Specification(path)["claims"].Should().BeOfType<JsonArray>().Which.Select(static claim => (
                    claim!["id"]!.GetValue<string>(),
                    claim["text"]!.GetValue<string>(),
                    claim["status"]!.GetValue<string>(),
                    claim["line"]!.GetValue<int>()))
                .Should().Equal(
                    ("B-001", "It gives the facts.", "Active", LineOf(path, "| B-001 |")),
                    ("B-002", "It gives them as JSON.", "Active", LineOf(path, "| B-002 |")));
        }
    }

    [Then("each constraint carries its id, its text, what it rules out and the line it is on")]
    public void ThenEachConstraint()
    {
        foreach (var path in new[] { FirstPath, SecondPath })
        {
            Specification(path)["constraints"].Should().BeOfType<JsonArray>().Which.Select(static constraint => (
                    constraint!["id"]!.GetValue<string>(),
                    constraint["text"]!.GetValue<string>(),
                    constraint["rulesOut"]!.GetValue<string>(),
                    constraint["line"]!.GetValue<int>()))
                .Should().Equal(
                    ("C-1", "Offline and read-only.", "A network call.", LineOf(path, "| C-1 |")),
                    ("C-2", "One parser.", "A second parser.", LineOf(path, "| C-2 |")));
        }
    }

    [Then("each out-of-scope row carries its number, its item, its reason and the line it is on")]
    public void ThenEachOutOfScopeRow()
    {
        foreach (var path in new[] { FirstPath, SecondPath })
        {
            Specification(path)["outOfScope"].Should().BeOfType<JsonArray>().Which.Select(static row => (
                    row!["number"]!.GetValue<string>(),
                    row["item"]!.GetValue<string>(),
                    row["reason"]!.GetValue<string>(),
                    row["line"]!.GetValue<int>()))
                .Should().Equal(
                    ("1", "Claim coverage", "Owned by the trace command", LineOf(path, "| 1 | Claim coverage |")),
                    ("2", "Any write", "Not asked for", LineOf(path, "| 2 | Any write |")));
        }
    }

    [Then("the open question carries its id, its question, what it blocks, its resolution and the line it is on")]
    public void ThenOpenQuestion() =>
        Specification(FirstPath)["openQuestions"].Should().BeOfType<JsonArray>().Which.Select(static question => (
                question!["id"]!.GetValue<string>(),
                question["question"]!.GetValue<string>(),
                question["blocks"]!.GetValue<string>(),
                question["resolution"]!.GetValue<string>(),
                question["line"]!.GetValue<int>()))
            .Should().Equal(("OQ-1", "Which one?", "B-001", _resolution, LineOf(FirstPath, "| OQ-1 |")));

    [Then("it is given as {word}")]
    public void ThenGivenAs(string state)
    {
        state.Should().BeOneOf("resolved", "unresolved");
        Specification(FirstPath)["openQuestions"].Should().BeOfType<JsonArray>().Which.Should().ContainSingle()
            .Which!["resolved"]!.GetValue<bool>().Should().Be(state == "resolved");
    }

    [Then("the first specification carries no out-of-scope rows")]
    public void ThenNoOutOfScopeRows() =>
        Specification(FirstPath)["outOfScope"].Should().BeOfType<JsonArray>().Which.Should().BeEmpty();

    [Then("it still carries its claims, constraints and open questions")]
    public void ThenStillCarriesOtherRows()
    {
        var first = Specification(FirstPath);
        first["claims"].Should().BeOfType<JsonArray>().Which
            .Select(static claim => (claim!["id"]!.GetValue<string>(), claim["line"]!.GetValue<int>()))
            .Should().Equal(("B-001", LineOf(FirstPath, "| B-001 |")), ("B-002", LineOf(FirstPath, "| B-002 |")));
        first["constraints"].Should().BeOfType<JsonArray>().Which
            .Select(static constraint => (constraint!["id"]!.GetValue<string>(), constraint["line"]!.GetValue<int>()))
            .Should().Equal(("C-1", LineOf(FirstPath, "| C-1 |")), ("C-2", LineOf(FirstPath, "| C-2 |")));
        first["openQuestions"].Should().BeOfType<JsonArray>().Which
            .Select(static question => (question!["id"]!.GetValue<string>(), question["line"]!.GetValue<int>()))
            .Should().Equal(("OQ-1", LineOf(FirstPath, "| OQ-1 |")));
    }

    [Then("the first specification carries three records")]
    public void ThenThreeRecords() =>
        Specification(FirstPath)["records"].Should().BeOfType<JsonArray>().Which.Should().HaveCount(3);

    [Then("each record carries its kind, its number, its title, its status and its path relative to the root")]
    public void ThenEachRecord() =>
        Specification(FirstPath)["records"].Should().BeOfType<JsonArray>().Which.Select(static record => (
                record!["kind"]!.GetValue<string>(),
                record["number"]!.GetValue<string>(),
                record["title"]!.GetValue<string>(),
                record["status"]?.GetValue<string>(),
                record["path"]!.GetValue<string>()))
            .Should().Equal(
                ("adr", "0001", "ADR-0001: Read each record as written", "Accepted", AdrPath),
                ("decision", "0001", "Decision 0001: the records are given", null, DecisionPath),
                ("lesson", "0001", "Lesson 0001: a record is read as written", null, LessonPath));

    [Then("that claim is given with the withdrawn status")]
    public void ThenWithdrawnClaimGiven() =>
        Specification(FirstPath)["claims"].Should().BeOfType<JsonArray>().Which
            .Select(static claim => (claim!["id"]!.GetValue<string>(), claim["status"]!.GetValue<string>(), claim["line"]!.GetValue<int>()))
            .Should().Contain(("B-002", _withdrawn, LineOf(FirstPath, "| B-002 |")));

    [Then("the command succeeds")]
    public void ThenSucceeds() => _result.ExitCode.Should().Be(0, _result.Stderr);

    [Then("both claims are given")]
    public void ThenBothClaimsGiven() =>
        Specification(FirstPath)["claims"].Should().BeOfType<JsonArray>().Which
            .Select(static claim => (claim!["id"]!.GetValue<string>(), claim["text"]!.GetValue<string>(), claim["line"]!.GetValue<int>()))
            .Should().Equal(
                ("B-001", "It gives the facts.", LineOf(FirstPath, "| B-001 | It gives the facts. |")),
                ("B-001", "It gives the facts again.", LineOf(FirstPath, "| B-001 | It gives the facts again. |")));

    [Then("the claim carries no traceability row, no scenario and no test")]
    public void ThenNoCoverage()
    {
        Specification(FirstPath)["claims"].Should().BeOfType<JsonArray>().Which
            .Select(static claim => claim!["id"]!.GetValue<string>()).Should().Contain("B-001");
        _result.Stdout.Should().NotContainEquivalentOf("zebra").And.NotContain("Covered");
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

    private static IReadOnlyList<string> Replace(IReadOnlyList<string> sections, string title, string replacement) =>
        sections.Select(section => section.StartsWith($"## {title}\n", StringComparison.Ordinal) ? replacement : section).ToList();

    private static string ClaimsSection(params string[] rows) =>
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + string.Concat(rows.Select(static row => row + "\n"));

    private static string OpenQuestionsSection(params string[] rows) =>
        "## 11. Open Questions\n\n| ID | Question | Blocks | Resolution |\n| -- | -------- | ------ | ---------- |\n"
            + string.Concat(rows.Select(static row => row + "\n"));

    private static string MatrixSection(params string[] rows) =>
        "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
            + string.Concat(rows.Select(static row => row + "\n"));

    private void WriteSpecifications(SpechtTree tree)
    {
        tree.WriteFeatureFile(tree.WriteCoLocatedFeature("src/first", "0001", "F1", _first, _firstSections), _firstFeatureFile);
        tree.WriteFeatureFile(tree.WriteCoLocatedFeature("src/second", "0001", "F2", _second, Sections), FeatureFile);
    }

    private int LineOf(string path, string prefix) =>
        Array.FindIndex(File.ReadAllLines(Path.Combine(Tree.Root, path)), line => line.StartsWith(prefix, StringComparison.Ordinal)) + 1;

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
    private const string AdrPath = "src/first/.spec/adr/0001-read-each-record-as-written.md";
    private const string DecisionPath = "src/first/.spec/decisions/0001-the-records-are-given.md";
    private const string LessonPath = "src/first/.spec/lessons/0001-a-record-is-read-as-written.md";

    private const string FirstClaim = "| B-001 | It gives the facts. | brd | Active |";

    private const string FeatureFile =
        "Feature: it\n\n  @B-001\n  Scenario: It gives the facts\n    Given a thing\n\n  @B-002\n  Scenario: It gives them as JSON\n    Given a thing\n";

    private static readonly IReadOnlyList<string> Sections = Replace(
        Replace(
            Replace(
                Replace(
                    SpechtTree.SectionsWith("11. Open Questions", OpenQuestionsSection("| OQ-1 | Which one? | B-001 | Open |")),
                    "3. Acceptance Criteria",
                    ClaimsSection(FirstClaim, "| B-002 | It gives them as JSON. | brd | Active |")),
                "4. Constraints",
                "## 4. Constraints\n\n| ID | Constraint | Rules Out |\n| -- | ---------- | --------- |\n"
                    + "| C-1 | Offline and read-only. | A network call. |\n| C-2 | One parser. | A second parser. |\n"),
            "5. Out of Scope",
            "## 5. Out of Scope\n\n| # | Item | Exclusion Reason |\n| - | ---- | ---------------- |\n"
                + "| 1 | Claim coverage | Owned by the trace command |\n| 2 | Any write | Not asked for |\n"),
        "9. Traceability Matrix",
        MatrixSection("| B-001 | It gives the facts | Missing | Missing |", "| B-002 | It gives them as JSON | Missing | Missing |"));

    private readonly Dictionary<string, string> _first = new(StringComparer.Ordinal) { ["spec_status"] = "approved", ["status"] = "ready" };
    private readonly Dictionary<string, string> _second = new(StringComparer.Ordinal) { ["spec_status"] = "draft", ["status"] = "in-progress" };
    private IReadOnlyList<string> _firstSections = Sections;
    private string _firstFeatureFile = FeatureFile;
    private string _withdrawn = string.Empty;
    private string _resolution = string.Empty;
    private SpechtTree? _tree;
    private SpechtTree? _other;
    private string _rootArgument = ".";
    private JsonArray? _violations;
    private Dictionary<string, byte[]> _before = [];
    private (string Stdout, string Stderr, int ExitCode) _result;
    private (string Stdout, string Stderr, int ExitCode) _otherResult;
}
