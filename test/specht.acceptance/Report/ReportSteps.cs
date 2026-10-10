using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;
using Reqnroll;
using specht.Report;
using specht.tests;

namespace specht.acceptance.Report;

/// <summary>
/// Steps for <c>src/specht/Report/.spec/report.feature</c> (0001-F3): 0034 binds B-005, B-007, B-008 and B-021, the claims
/// made "given the document", and 0035 binds B-001. "The check runs with JSON output" runs the engine's runner for the
/// typed report and its document, and launches the built tool with <c>--json</c> over the same root, so B-001, B-008 and
/// B-021 see the tool's real stdout. The two B-006 scenarios stay unbound: the launched tool has no clock seam to set, and
/// a child process's machine name cannot be set; B-006 is proved at the engine. The rest of the file's scenarios stay
/// pending for their items.
/// </summary>
[Binding]
[Scope(Feature = "The report contract")]
public sealed class ReportSteps
{
    [Given("a repository root holding a manifest and the three frontmatter schemas")]
    public void GivenARepositoryRootHoldingAManifestAndTheThreeFrontmatterSchemas() => _tree = new SpecTree();

    [Given("the root holds two specifications in the epics layout and one in the features layout")]
    public void GivenTheRootHoldsTwoSpecificationsInTheEpicsLayoutAndOneInTheFeaturesLayout()
    {
        Tree.WriteFeature("0001", "F1");
        Tree.WriteFeature("0001", "F2");
        Tree.WriteCoLocatedFeature("src/area", "0002", "F1");
    }

    [Given("the root holds a specification whose claim B-002 has no traceability row")]
    public void GivenTheRootHoldsASpecificationWhoseClaimB002HasNoTraceabilityRow() =>
        Tree.WriteFeature("0001", "F1", sections: SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims));

    [Given("the root holds a specification with one violation")]
    public void GivenTheRootHoldsASpecificationWithOneViolation()
    {
        Tree.WriteFeature("0001", "F1", featureFile: null);
        Tree.Run().Violations.Should().ContainSingle();
    }

    [Given("the root holds specifications with violations of every rule family")]
    public void GivenTheRootHoldsSpecificationsWithViolationsOfEveryRuleFamily()
    {
        Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });
        Tree.WriteFeature("0002", "F1", sections: SpecTree.Sections.Where(static section => !section.StartsWith("## 11.", StringComparison.Ordinal)).ToList());
        Tree.WriteFeature("0003", "F1", new Dictionary<string, string> { ["epic"] = "\"0009\"" });
        Tree.WriteFeature("0004", "F1", featureFile: null);
        Tree.WriteFeature("0005", "F1", sections: SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims));
        Tree.WriteFeature("0006", "F1", new Dictionary<string, string> { ["children"] = "[\"0006-01\"]" });
        Tree.WriteFeature("0007", "F1", new Dictionary<string, string> { ["depends_on"] = "[\"F2\"]" });
        Tree.WriteFeature("0007", "F2");
        Tree.WriteFeature("0008", "F1", new Dictionary<string, string> { ["spec_status"] = "approved" });
        Tree.Run().Violations.Select(static violation => violation.RuleId[..6]).Distinct().Should()
            .BeEquivalentTo(["SPEC00", "SPEC01", "SPEC02", "SPEC03", "SPEC04", "SPEC05", "SPEC06"]);
    }

    [Given("the root is a deeply nested directory on this machine")]
    public void GivenTheRootIsADeeplyNestedDirectoryOnThisMachine() =>
        _nested = Path.Combine(Path.GetTempPath(), "specht-report", Guid.NewGuid().ToString("N"), "a", "b", "c", "d", "e", "f");

    [When("the check runs with JSON output")]
    public void WhenTheCheckRunsWithJsonOutput()
    {
        var root = Tree.Root;

        if (_nested is not null)
        {
            Copy(Tree.Root, _nested);
            root = _nested;
        }

        _root = root;
        _report = SpecCheckRunner.Run(root);
        _document = SpecReportDocument.From(_report);
        (_stdout, _stderr, _) = Tool.Launch(root, "--root", ".", "--json");
        _json = _stdout.EndsWith(Environment.NewLine, StringComparison.Ordinal) ? _stdout[..^Environment.NewLine.Length] : _stdout;
    }

    [Then("the standard output parses as a single JSON document")]
    public void ThenTheStandardOutputParsesAsASingleJsonDocument() =>
        _stdout.Invoking(static stdout => JsonDocument.Parse(stdout).Dispose()).Should().NotThrow(_stderr);

    [Then("no diagnostic line and no summary line precede or follow it")]
    public void ThenNoDiagnosticLineAndNoSummaryLinePrecedeOrFollowIt() => Json.Should().Be(Document.ToJson(), _stderr);

    [Then("the document names the schema version checked against")]
    public void ThenTheDocumentNamesTheSchemaVersionCheckedAgainst() => Document.SchemaVersion.Should().Be(1);

    [Then("whether the schemas came from the tool or from the repository's own files")]
    public void ThenWhetherTheSchemasCameFromTheToolOrFromTheRepositorysOwnFiles() =>
        Document.SchemaSource.Should().Be(SpecSchemaSource.Embedded);

    [Then("each layout by name with its specification count")]
    public void ThenEachLayoutByNameWithItsSpecificationCount() =>
        Document.Layouts.Should().BeEquivalentTo([new SpecReportLayout("epics", 2), new SpecReportLayout("features", 1)]);

    [Then("the item count, the count of rule ids evaluated, the error count and the warning count")]
    public void ThenTheItemCountTheCountOfRuleIdsEvaluatedTheErrorCountAndTheWarningCount()
    {
        Document.ItemCount.Should().Be(Report.ItemCount);
        Document.RulesEvaluated.Should().Be(Report.RulesEvaluated);
        Document.ErrorCount.Should().Be(Report.ErrorCount);
        Document.WarningCount.Should().Be(Report.WarningCount);
    }

    [Then("the violation carries the rule id, the severity, the file, the line, the identifier and the message")]
    public void ThenTheViolationCarriesTheRuleIdTheSeverityTheFileTheLineTheIdentifierAndTheMessage()
    {
        var source = Report.Violations.Should().ContainSingle(static violation => violation.Identifier == "B-002").Which;
        var carried = Document.Violations.Should().ContainSingle(static violation => violation.Identifier == "B-002").Which;
        carried.RuleId.Should().Be("SPEC031");
        carried.Severity.Should().Be(SpecSeverity.Error);
        carried.File.Should().Be("epics/0001-epic/F1-feature/spec.md");
        carried.Line.Should().Be(source.Line);
        carried.Message.Should().Be(source.Message);
    }

    [Then("the violation carries what the rule expected")]
    public void ThenTheViolationCarriesWhatTheRuleExpected() =>
        Document.Violations.Should().ContainSingle(static violation => violation.Identifier == "B-002").Which.Expected.Should().NotBeNull();

    [Then("the document validates against the report schema published in this repository")]
    public void ThenTheDocumentValidatesAgainstTheReportSchemaPublishedInThisRepository()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "docs", "schema", "report.schema.json");
        File.Exists(path).Should().BeTrue("the report schema is published at docs/schema/report.schema.json (A-1)");
        using var json = JsonDocument.Parse(Json);
        var result = JsonSchema.FromText(File.ReadAllText(path))
            .Evaluate(json.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });
        result.IsValid.Should().BeTrue(
            string.Join("; ", (result.Details ?? []).Where(static detail => detail.Errors is not null)
                .SelectMany(static detail => detail.Errors!.Select(error => $"{detail.InstanceLocation} {error.Value}"))));
    }

    [Then("every path in the document is relative to the root")]
    public void ThenEveryPathInTheDocumentIsRelativeToTheRoot()
    {
        Document.Violations.Should().NotBeEmpty().And.AllSatisfy(static violation => Path.IsPathRooted(violation.File).Should().BeFalse(violation.File));
        Strings(JsonNode.Parse(Json)).Should().NotContain(value => value.Contains(_root!, StringComparison.Ordinal));
    }

    [Then("no path uses the platform's directory separator where it differs from a forward slash")]
    public void ThenNoPathUsesThePlatformsDirectorySeparatorWhereItDiffersFromAForwardSlash() =>
        Document.Violations.Should().AllSatisfy(static violation => violation.File.Should().NotContain("\\"));

    [AfterScenario]
    public void DeleteTree()
    {
        _tree?.Dispose();

        if (_nested is not null && Directory.Exists(_nested))
        {
            Directory.Delete(_nested, recursive: true);
        }
    }

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No repository root was prepared.");

    private SpecCheckReport Report => _report ?? throw new InvalidOperationException("The check has not run.");

    private SpecReportDocument Document => _document ?? throw new InvalidOperationException("No document was made.");

    private string Json => _json ?? throw new InvalidOperationException("No document was serialized.");

    private static IEnumerable<string> Strings(JsonNode? node) =>
        node switch
        {
            JsonObject members => members.SelectMany(static member => Strings(member.Value)),
            JsonArray elements => elements.SelectMany(Strings),
            JsonValue value when value.TryGetValue<string>(out var text) => [text],
            _ => [],
        };

    private static void Copy(string from, string to)
    {
        foreach (var directory in Directory.GetDirectories(from, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(to, Path.GetRelativePath(from, directory)));
        }

        Directory.CreateDirectory(to);

        foreach (var file in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
        {
            File.Copy(file, Path.Combine(to, Path.GetRelativePath(from, file)));
        }
    }

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";

    private SpecTree? _tree;
    private string? _nested;
    private string? _root;
    private SpecCheckReport? _report;
    private SpecReportDocument? _document;
    private string? _json;
    private string _stdout = string.Empty;
    private string _stderr = string.Empty;
}
