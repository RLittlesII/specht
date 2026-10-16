using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;
using Reqnroll;
using Specht.Model;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Report;

/// <summary>
/// Steps for <c>src/specht/Report/.spec/report.feature</c> (0001-F3): 0034 binds B-005, B-007, B-008 and B-021, the claims
/// made "given the document", and 0035 binds B-001. "The check runs with JSON output" runs the engine's runner for the
/// typed report and its document, and launches the built tool with <c>--json</c> over the same root, so B-001, B-008 and
/// B-021 see the tool's real stdout. The two B-006 scenarios stay unbound: the launched tool has no clock seam to set, and
/// a child process's machine name cannot be set; B-006 is proved at the engine. 0037 binds B-009, B-010, B-011 and B-026,
/// what a frontmatter, section or table violation's <c>expected</c> carries, read from the launched tool's document and
/// compared with the tree's own manifest and Feature schema. The rest of the file's scenarios stay pending for their items.
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

    [Given("the root holds a specification whose frontmatter {word} is {string}")]
    public void GivenTheRootHoldsASpecificationWhoseFrontmatterKeyIs(string key, string value) =>
        Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { [key] = value });

    [Given("the root holds a specification whose frontmatter has no priority")]
    public void GivenTheRootHoldsASpecificationWhoseFrontmatterHasNoPriority()
    {
        var path = Tree.WriteFeature("0001", "F1");
        File.WriteAllText(path, File.ReadAllText(path).Replace("priority: med\n", string.Empty, StringComparison.Ordinal));
    }

    [Given("the root holds a specification whose frontmatter priority is \"urgent\" and whose scored_by is null, the template's value")]
    public void GivenTheRootHoldsASpecificationWhoseFrontmatterPriorityIsUrgentAndWhoseScoredByIsNull() =>
        Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["priority"] = "urgent", ["scored_by"] = "null" });

    [Given("the root holds a specification missing its constraints section")]
    public void GivenTheRootHoldsASpecificationMissingItsConstraintsSection() =>
        Tree.WriteFeature(
            "0001",
            "F1",
            sections: SpecTree.Sections.Where(static section => !section.StartsWith("## 4.", StringComparison.Ordinal)).ToList());

    [Given("the root holds a specification whose traceability table has a column named {string}")]
    public void GivenTheRootHoldsASpecificationWhoseTraceabilityTableHasAColumnNamed(string column) =>
        Tree.WriteFeature(
            "0001",
            "F1",
            sections: SpecTree.SectionsWith(
                "9. Traceability Matrix",
                $"## 9. Traceability Matrix\n\n| Claim ID | Scenario | {column} | Status |\n| - | - | - | - |\n"
                    + "| B-001 | It does the thing | Missing | Missing |\n"));

    [Given("the root holds a specification with no frontmatter")]
    public void GivenTheRootHoldsASpecificationWithNoFrontmatter() =>
        Tree.WriteRaw("epics/0001-epic/F1-feature/spec.md", "# Specification: F1\n\n" + string.Join('\n', SpecTree.Sections));

    [Given("the frontmatter schema files are deleted from the root")]
    public void GivenTheFrontmatterSchemaFilesAreDeletedFromTheRoot() =>
        Directory.Delete(Path.Combine(Tree.Root, SpecTree.FrontmatterSchemaFolder), recursive: true);

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
        _report = EngineServices.Runner().Run(root);
        _document = SpecReportDocument.From(_report);
        (_stdout, _stderr, _) = AcceptanceTool.Launch(root, "--root", ".", "--json");
        _json = _stdout.EndsWith(Environment.NewLine, StringComparison.Ordinal) ? _stdout[..^Environment.NewLine.Length] : _stdout;
    }

    [Then("the standard output parses as a single JSON document")]
    public void ThenTheStandardOutputParsesAsASingleJsonDocument() =>
        _stdout.Invoking(static stdout => JsonDocument.Parse(stdout).Dispose()).Should().NotThrow(_stderr);

    [Then("no diagnostic line and no summary line precede or follow it")]
    public void ThenNoDiagnosticLineAndNoSummaryLinePrecedeOrFollowIt() => Json.Should().Be(Document.ToJson(), _stderr);

    [Then("the document names the schema version checked against")]
    public void ThenTheDocumentNamesTheSchemaVersionCheckedAgainst()
    {
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath)))!.AsObject();
        Document.SchemaVersion.Should().Be(manifest["schemaVersion"]!.GetValue<string>());
    }

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

    [Then("the violation's expectation names the Feature schema file by its path under the configuration folder")]
    [Then("the violation's expectation names the Feature schema file by the path it has under the configuration folder")]
    public void ThenTheViolationsExpectationNamesTheFeatureSchemaFileByItsPathUnderTheConfigurationFolder()
    {
        var path = SpecTree.FrontmatterSchemaFolder + "/" + Manifest["frontmatterSchemas"]!["feature"]!.GetValue<string>();
        Violations(static violation => violation["ruleId"]!.GetValue<string>() is "SPEC001" or "SPEC002").Should().NotBeEmpty(Json)
            .And.AllSatisfy(violation => (violation["expected"]?["schema"]?.GetValue<string>()).Should().Be(path, Json));
    }

    [Then("the key {string}")]
    [Then("the violation's expectation names the key {string}")]
    public void ThenTheViolationsExpectationNamesTheKey(string key)
    {
        _key = key;
        Frontmatter(key).Should().NotBeEmpty(Json)
            .And.AllSatisfy(violation => (violation["expected"]?["key"]?.GetValue<string>()).Should().Be(key, Json));
    }

    [Then("the violation's expectation names no key")]
    public void ThenTheViolationsExpectationNamesNoKey()
    {
        _key = null;
        Frontmatter(null).Should().NotBeEmpty(Json)
            .And.AllSatisfy(violation => (violation["expected"] as JsonObject).Should().NotBeNull(Json).And.NotContainKey("key"));
    }

    [Then("the failed constraint {string} with the values the schema allows for it")]
    [Then("the failed constraint {string} with the type the schema allows for it")]
    [Then("the failed constraint {string} with the keys the schema requires")]
    public void ThenTheFailedConstraintWithTheSchemasValueForIt(string keyword)
    {
        var place = _key is null ? FeatureSchema : FeatureSchema["properties"]![_key]!;
        var constraints = new JsonObject { [keyword] = place[keyword]!.DeepClone() };
        Frontmatter(_key).Should().ContainSingle(
            violation => JsonNode.DeepEquals(violation["expected"]!["constraints"], constraints),
            "{0} carries {1}",
            Json,
            constraints.ToJsonString());
    }

    [Then("the violation's expectation for the key {string} names no failed constraint")]
    public void ThenTheViolationsExpectationForTheKeyNamesNoFailedConstraint(string key) =>
        Frontmatter(key).Should().NotBeEmpty(Json)
            .And.AllSatisfy(violation => (violation["expected"]?["constraints"] as JsonObject).Should().NotBeNull(Json).And.BeEmpty(Json));

    [Then("the violation's expectation carries the section titles the manifest contracts, in order")]
    public void ThenTheViolationsExpectationCarriesTheSectionTitlesTheManifestContractsInOrder() =>
        Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC010").Should().NotBeEmpty(Json)
            .And.AllSatisfy(violation => JsonNode.DeepEquals(violation["expected"]!["sections"], Manifest["sections"]).Should().BeTrue(Json));

    [Then("the violation's expectation names the traceability section")]
    public void ThenTheViolationsExpectationNamesTheTraceabilitySection() =>
        (Table["expected"]?["section"]?.GetValue<string>()).Should().Be(Manifest["roles"]!["matrix"]!.GetValue<string>(), Json);

    [Then("carries the ordered header list the manifest gives that section")]
    public void ThenCarriesTheOrderedHeaderListTheManifestGivesThatSection() =>
        JsonNode.DeepEquals(Table["expected"]!["headers"], Manifest["tables"]!["matrix"]).Should().BeTrue(Json);

    [Then("the keys that schema requires, in the schema's order")]
    public void ThenTheKeysThatSchemaRequiresInTheSchemasOrder() =>
        JsonNode.DeepEquals(
                Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC001")
                    .Should().ContainSingle(Json).Which["expected"]!["required"],
                FeatureSchema["required"])
            .Should().BeTrue(Json);

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

    private SpechtReport Report => _report ?? throw new InvalidOperationException("The check has not run.");

    private SpecReportDocument Document => _document ?? throw new InvalidOperationException("No document was made.");

    private string Json => _json ?? throw new InvalidOperationException("No document was serialized.");

    private JsonObject Manifest => JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath)))!.AsObject();

    private JsonNode FeatureSchema =>
        JsonNode.Parse(File.ReadAllText(
            Path.Combine(Tree.Root, SpecTree.FrontmatterSchemaFolder, Manifest["frontmatterSchemas"]!["feature"]!.GetValue<string>())))!;

    private JsonNode Table =>
        Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC013").Should().ContainSingle(Json).Which;

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

    private List<JsonNode> Violations(Func<JsonNode, bool> predicate) =>
        JsonNode.Parse(Json)!["violations"]!.AsArray().OfType<JsonNode>().Where(predicate).ToList();

    private List<JsonNode> Frontmatter(string? key) =>
        Violations(violation => violation["ruleId"]!.GetValue<string>() == "SPEC002" && violation["identifier"]?.GetValue<string>() == key);

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";

    private SpecTree? _tree;
    private string? _nested;
    private string? _root;
    private SpechtReport? _report;
    private SpecReportDocument? _document;
    private string? _json;
    private string? _key;
    private string _stdout = string.Empty;
    private string _stderr = string.Empty;
}
