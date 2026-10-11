using System.IO.Abstractions;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Engine.Baseline;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Manifest;

/// <summary>
/// Steps for <c>src/specht/Manifest/.spec/manifest.feature</c> (0001-F5). The manifest is edited in memory from the tree's
/// default copy and written when the check or the tool runs, so a tree's violations are counted under the default manifest
/// first. "The check runs" is the engine's runner; "the tool runs as a command" launches the built tool, the only place a
/// rejection's exit code and streams are seen (B-022, B-023). Under "the frontmatter schemas are read from the root" the
/// check is the model the runner loads, with its schemas loaded from the root's files, evaluated by the frontmatter rule
/// (B-008): the runner selects no on-disk source until <c>0001-F7</c> B-009. A role, a role's headers and a marker are
/// written into the manifest under <c>roles</c>, <c>tables</c> and <c>markers</c> (B-001 to B-003, B-015, B-039, B-040;
/// decision 0006). An empty file-shape list is written under <c>companionFiles</c> (B-041; <c>0001-F6</c> decision 0008).
/// An exclusion entry is written as the whole <c>exclusions</c> list (B-021; <c>0001-F6</c> decision 0003).
/// A rule setting is written under <c>rules</c>, a rule id mapped to its level (B-010, B-011, B-013, B-045, B-046; decision 0008),
/// and "sets no level" leaves the <c>rules</c> key out (B-044).
/// A rule declared by a type name and a library file is written there too, as an object under a rule id the pinned version
/// holds, so that its shape alone is at fault, beside a file standing as the library; nothing compiled is put under the root
/// (B-017). "A rule that reports its violations at warning severity" is a stand-in under a rule id of the pinned version,
/// since no shipped rule gives a warning, and "the check runs with that rule" hands it to the engine's
/// <see cref="SpechtRunner.Evaluate"/> over the model loaded from the root (B-046).
/// </summary>
[Binding]
[Scope(Feature = "The manifest carries the roles")]
public sealed class ManifestSteps
{
    [Given("a repository root holding a manifest and the three frontmatter schemas")]
    public void GivenARepositoryRootHoldingAManifestAndTheThreeFrontmatterSchemas() => _tree = new SpecTree();

    [Given("the manifest is the default one this repository checks itself with")]
    public void GivenTheManifestIsTheDefaultOneThisRepositoryChecksItselfWith() =>
        _manifest = JsonNode.Parse(File.ReadAllText(ManifestPath))!.AsObject();

    [Given("the manifest carries a key the engine does not know")]
    public void GivenTheManifestCarriesAKeyTheEngineDoesNotKnow() => Manifest[UnknownKey] = "synthetic";

    [Given("the manifest carries a {string} key and a {string} key")]
    public void GivenTheManifestCarriesAKeyAndAKey(string first, string second)
    {
        Manifest[first] = "synthetic";
        Manifest[second] = "synthetic";
    }

    [Given("the manifest declares no claim grammar")]
    public void GivenTheManifestDeclaresNoClaimGrammar()
    {
        var identifiers = Manifest["identifiers"]!.AsObject();
        _defaultClaimGrammar = identifiers["claim"]!.GetValue<string>();
        identifiers.Remove("claim");
    }

    [Given("the root holds a specification declaring claim {string}")]
    public void GivenTheRootHoldsASpecificationDeclaringClaim(string claim) =>
        Tree.WriteFeature(
            "0001",
            "F1",
            sections: SpecTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + $"| {claim} | It does the thing. | brd | Active |\n"));

    [Given("the manifest renames the {word} section to {string} in its section list and its {word} role")]
    public void GivenTheManifestRenamesTheSectionInItsSectionListAndItsRole(string section, string title, string role)
    {
        section.Should().Be(role);
        var sections = Manifest["sections"]!.AsArray();
        var index = sections.Select(static listed => listed!.GetValue<string>()).ToList().IndexOf(DefaultTitles[role]);
        sections[index] = title;
        Named("roles")[role] = title;
    }

    [Given("the manifest maps the {word} role to a title that is not in its section list")]
    public void GivenTheManifestMapsTheRoleToATitleThatIsNotInItsSectionList(string role) => Named("roles")[role] = "3. Nowhere";

    [Given("the manifest's headers for the matrix role name the third column {string}")]
    public void GivenTheManifestsHeadersForTheMatrixRoleNameTheThirdColumn(string header) =>
        Manifest["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Scenario", header, "Status") };

    [Given("the manifest declares table headers under {string} and not under a role")]
    public void GivenTheManifestDeclaresTableHeadersUnderAndNotUnderARole(string key) =>
        Manifest["tables"] = new JsonObject { [key] = new JsonArray("Claim ID", "Scenario", "Test", "Status") };

    [Given("the manifest's missing-test cell value is {string}")]
    public void GivenTheManifestsMissingTestCellValueIs(string text) => Named("markers")["missing"] = text;

    [Given("the manifest's {word} sign-off marker is empty")]
    public void GivenTheManifestsSignOffMarkerIsEmpty(string marker) => Named("markers")[marker] = string.Empty;

    [Given("the root holds a specification whose claims table sits under {string}")]
    public void GivenTheRootHoldsASpecificationWhoseClaimsTableSitsUnder(string title) =>
        Tree.WriteFeatureFile(
            Write(
                "F1",
                SpecTree.SectionsWith(
                    DefaultTitles["claims"],
                    $"## {title}\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                        + "| B-001 | It does the thing. | brd | Active |\n| B-1 | Its id is malformed. | brd | Active |\n"
                        + "| B-002 | It has no matrix row. | brd | Active |\n")),
            PhantomTag);

    [Given("the root holds a specification whose traceability table has a {string} column in third place")]
    public void GivenTheRootHoldsASpecificationWhoseTraceabilityTableHasAColumnInThirdPlace(string header) =>
        Write("F1", SpecTree.SectionsWith(DefaultTitles["matrix"], Matrix(DefaultTitles["matrix"], $"Scenario | {header}", "It does the thing | A test")));

    [Given("the root holds a second specification whose traceability table has a {string} column in third place")]
    public void GivenTheRootHoldsASecondSpecificationWhoseTraceabilityTableHasAColumnInThirdPlace(string header) =>
        Write("F2", SpecTree.SectionsWith(DefaultTitles["matrix"], Matrix(DefaultTitles["matrix"], $"Scenario | {header}", "It does the thing | A test")));

    [Given("the root holds a specification whose {string} table lacks one of the matrix role's columns")]
    public void GivenTheRootHoldsASpecificationWhoseTableLacksOneOfTheMatrixRolesColumns(string title) =>
        Write("F1", SpecTree.SectionsWith(DefaultTitles["matrix"], Matrix(title, "Scenario", "It does the thing")));

    [Given("the root holds an approved specification with a {string} cell in its traceability table")]
    public void GivenTheRootHoldsAnApprovedSpecificationWithACellInItsTraceabilityTable(string cell) =>
        _specifications.Add(
            Relative(
                Tree.WriteFeature(
                    "0001",
                    "F1",
                    new Dictionary<string, string> { ["spec_status"] = "approved" },
                    SpecTree.SectionsWith(DefaultTitles["matrix"], Matrix(DefaultTitles["matrix"], "Scenario | Test", $"It does the thing | {cell}")))));

    [Then("the claims are read from that section")]
    public void ThenTheClaimsAreReadFromThatSection()
    {
        _rejection.Should().BeNull();
        _report!.Violations
            .Select(static violation => (violation.RuleId, violation.Identifier))
            .Should()
            .Contain(("SPEC030", "B-1"))
            .And.Contain(("SPEC031", "B-002"))
            .And.Contain(("SPEC021", "B-404"))
            .And.NotContain(("SPEC021", "B-001"));
    }

    [Then("no missing-section violation is reported")]
    public void ThenNoMissingSectionViolationIsReported() =>
        _report!.Violations.Should().NotContain(static violation => violation.RuleId == "SPEC010");

    [Then("the table-header rule reports the second specification")]
    public void ThenTheTableHeaderRuleReportsTheSecondSpecification() => HeaderViolationFiles().Should().Contain(_specifications[1]);

    [Then("it does not report the first")]
    public void ThenItDoesNotReportTheFirst() => HeaderViolationFiles().Should().NotContain(_specifications[0]);

    [Then("the table-header rule reports that specification")]
    public void ThenTheTableHeaderRuleReportsThatSpecification() => HeaderViolationFiles().Should().Contain(_specifications[0]);

    [Then("the approved-with-missing-coverage rule reports that row")]
    public void ThenTheApprovedWithMissingCoverageRuleReportsThatRow()
    {
        _rejection.Should().BeNull();
        _report!.Violations
            .Where(static violation => violation.RuleId == "SPEC060")
            .Select(static violation => (violation.File, violation.Identifier))
            .Should()
            .Equal((_specifications[0], "B-001"));
    }

    [Then("the rejection names the {word} role")]
    public void ThenTheRejectionNamesTheRole(string role) => _rejection!.Message.Should().Contain(role);

    [Then("the rejection names the {word} marker")]
    public void ThenTheRejectionNamesTheMarker(string marker) => _rejection!.Message.Should().Contain(marker);

    [Given("the manifest's companion file list is empty")]
    public void GivenTheManifestsCompanionFileListIsEmpty() => Manifest["companionFiles"] = new JsonArray();

    [Given("the manifest's exclusion list holds the entry {string}")]
    public void GivenTheManifestsExclusionListHoldsTheEntry(string entry) => Manifest["exclusions"] = new JsonArray(entry);

    [Given("the manifest sets the out-of-order-section rule to {word} severity")]
    public void GivenTheManifestSetsTheOutOfOrderSectionRuleToSeverity(string level) => Set(OutOfOrderRule, level);

    [Given("the manifest sets the out-of-order-section rule to {string}")]
    public void GivenTheManifestSetsTheOutOfOrderSectionRuleTo(string value) => Set(OutOfOrderRule, value);

    [Given("the manifest disables the out-of-order-section rule")]
    public void GivenTheManifestDisablesTheOutOfOrderSectionRule() => Set(OutOfOrderRule, "off");

    [Given("the manifest lists rule settings for an id outside the pinned version's vocabulary")]
    public void GivenTheManifestListsRuleSettingsForAnIdOutsideThePinnedVersionsVocabulary() => Set(RuleOutsideTheVocabulary, "warning");

    [Given("the manifest declares a rule by a type name and a library file")]
    public void GivenTheManifestDeclaresARuleByATypeNameAndALibraryFile() =>
        Set(OutOfOrderRule, new JsonObject { ["type"] = "Consumer.Rules.MarkingRule", ["library"] = LibraryFile });

    [Given("that library file exists under the root")]
    public void GivenThatLibraryFileExistsUnderTheRoot() => Tree.WriteRaw(LibraryFile, "A file standing as a rule library.");

    [Given("the library's rule leaves a mark under the root when it runs")]
    public void GivenTheLibrarysRuleLeavesAMarkUnderTheRootWhenItRuns() => _mark = Path.Combine(Tree.Root, "rules", "rule-ran.mark");

    [Given("the root holds a specification with its sections out of order")]
    public void GivenTheRootHoldsASpecificationWithItsSectionsOutOfOrder()
    {
        var sections = SpecTree.Sections.ToList();
        (sections[5], sections[6]) = (sections[6], sections[5]);
        Write("F1", sections);
        var report = Tree.Run();
        report.Violations.Should().ContainSingle().Which.Should().Match<SpecViolation>(static violation =>
            violation.RuleId == OutOfOrderRule && violation.Severity == SpecSeverity.Error);
        _rulesEvaluatedByDefault = report.RulesEvaluated;
    }

    [Given("the manifest sets no level for the out-of-order-section rule")]
    public void GivenTheManifestSetsNoLevelForTheOutOfOrderSectionRule() => Manifest.Remove("rules");

    [Given("a rule that reports its violations at warning severity")]
    public void GivenARuleThatReportsItsViolationsAtWarningSeverity() => _standIn = new WarningRule(OutOfOrderRule);

    [Given("the manifest sets that rule to error severity")]
    public void GivenTheManifestSetsThatRuleToErrorSeverity() => Set(StandIn.Id, "error");

    [Given("the root holds a specification that rule reports")]
    public void GivenTheRootHoldsASpecificationThatRuleReports()
    {
        Write("F1", SpecTree.Sections);
        SpechtRunner.Evaluate(SpecModel.Load(Tree.Root), [StandIn])
            .Violations.Should()
            .ContainSingle()
            .Which.Should()
            .Match<SpecViolation>(violation =>
                violation.RuleId == StandIn.Id && violation.Severity == SpecSeverity.Warning && violation.File == _specifications[0]);
    }

    [When("the check runs with that rule")]
    public void WhenTheCheckRunsWithThatRule()
    {
        File.WriteAllText(ManifestPath, Manifest.ToJsonString());
        _report = SpechtRunner.Evaluate(SpecModel.Load(Tree.Root), [StandIn]);
    }

    [Then("that rule's violation is reported at error severity")]
    public void ThenThatRulesViolationIsReportedAtErrorSeverity() =>
        _report!.Violations.Should()
            .ContainSingle()
            .Which.Should()
            .Match<SpecViolation>(violation =>
                violation.RuleId == StandIn.Id && violation.Severity == SpecSeverity.Error && violation.File == _specifications[0]);

    [Then("the violation is reported at {word} severity")]
    public void ThenTheViolationIsReportedAtSeverity(string severity) =>
        Run.Stdout.Split('\n')
            .Should()
            .ContainSingle(static line => line.Contains(OutOfOrderRule), Run.Stderr)
            .Which.Should()
            .StartWith(_specifications[0])
            .And.Contain($": {severity} {OutOfOrderRule}: ");

    [Then("nothing is reported for that rule")]
    public void ThenNothingIsReportedForThatRule()
    {
        _rejection.Should().BeNull();
        _report!.Violations.Should().NotContain(violation => violation.RuleId == RuleId);
    }

    [Then("the count of rules evaluated excludes it")]
    public void ThenTheCountOfRulesEvaluatedExcludesIt() =>
        _report!.RulesEvaluated.Should().Be(
            (_rulesEvaluatedByDefault ?? throw new InvalidOperationException("No count was taken under the default manifest.")) - 1);

    [Then("the rejection names that rule id")]
    public void ThenTheRejectionNamesThatRuleId() => _rejection!.Message.Should().Contain(RuleId);

    [Then("the mark does not exist")]
    public void ThenTheMarkDoesNotExist() =>
        File.Exists(_mark ?? throw new InvalidOperationException("No mark was named.")).Should().BeFalse();

    [Then("the rejection names the companion file list")]
    public void ThenTheRejectionNamesTheCompanionFileList() => _rejection!.Message.Should().Contain("companionFiles");

    [Then("the rejection names {string}")]
    public void ThenTheRejectionNames(string name) => _rejection!.Message.Should().Contain(name);

    [Given("the frontmatter schemas are read from the root")]
    public void GivenTheFrontmatterSchemasAreReadFromTheRoot() => _fromRoot = true;

    [Given("the manifest names the Feature schema file {string}")]
    public void GivenTheManifestNamesTheFeatureSchemaFile(string name)
    {
        Manifest["frontmatterSchemas"] = new JsonObject { ["feature"] = name };
        _featureSchema = name;
    }

    [Given("the schema folder holds that file and not the default name")]
    public void GivenTheSchemaFolderHoldsThatFileAndNotTheDefaultName()
    {
        var schema = Path.Combine(Tree.Root, ".spec", "schema");
        File.WriteAllText(Path.Combine(schema, FeatureSchema), FeatureSchemaRejectingTheDomain);
        File.Delete(Path.Combine(schema, "feature-spec.frontmatter.schema.json"));
    }

    [Then("every specification's frontmatter is checked against {string}")]
    public void ThenEverySpecificationsFrontmatterIsCheckedAgainst(string name)
    {
        _rejection.Should().BeNull();
        var model = _model ?? throw new InvalidOperationException("The check loaded no model.");
        model.Features.Should().NotBeEmpty();
        _frontmatterViolations
            .Where(static violation => violation.RuleId == "SPEC002" && violation.Identifier == "domain")
            .Select(static violation => violation.File)
            .Should()
            .BeEquivalentTo(model.Features.Select(static feature => feature.RelativePath), name);
    }

    [Then("the malformed-claim-id rule reports {string} against the default claim grammar")]
    public void ThenTheMalformedClaimIdRuleReportsAgainstTheDefaultClaimGrammar(string claim) =>
        _report!.Violations
            .Should()
            .ContainSingle(violation => violation.RuleId == "SPEC030" && violation.Identifier == claim)
            .Which.Message.Should()
            .Contain(_defaultClaimGrammar ?? throw new InvalidOperationException("No claim grammar was removed."));

    [Given("the root holds a specification with three violations")]
    public void GivenTheRootHoldsASpecificationWithThreeViolations()
    {
        var sections = SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims)
            .Where(static section => !section.StartsWith("## 11.", StringComparison.Ordinal))
            .ToList();
        Tree.WriteFeatureFile(Tree.WriteFeature("0001", "F1", sections: sections), PhantomTag);
        Tree.Run().Violations.Should().HaveCount(3);
    }

    [Given("the root holds a specification with no violations")]
    public void GivenTheRootHoldsASpecificationWithNoViolations()
    {
        Tree.WriteFeature("0001", "F1");
        Tree.Run().Violations.Should().BeEmpty();
    }

    [When("the check runs")]
    public void WhenTheCheckRuns()
    {
        File.WriteAllText(ManifestPath, Manifest.ToJsonString());

        try
        {
            if (_fromRoot)
            {
                var loaded = SpecModel.Load(Tree.Root);
                _model = new SpecModel(loaded.Root, loaded.Features, loaded.Items, loaded.Epics, SpecSchemas.Load(new FileSystem(), Tree.Root));
                _frontmatterViolations = new FrontmatterSchemaRule().Evaluate(_model).ToList();
            }
            else
            {
                _report = SpechtRunner.Run(Tree.Root);
            }
        }
        catch (SpechtManifestException rejection)
        {
            _rejection = rejection;
        }
    }

    [When("the tool runs as a command on the root")]
    public void WhenTheToolRunsAsACommandOnTheRoot()
    {
        File.WriteAllText(ManifestPath, Manifest.ToJsonString());
        _run = AcceptanceTool.Launch(Tree.Root, "--root", ".");
    }

    [Then("the exit code is {int}")]
    public void ThenTheExitCodeIs(int code) => Run.ExitCode.Should().Be(code, Run.Stderr);

    [Then("the standard error carries the rejection, naming that key")]
    public void ThenTheStandardErrorCarriesTheRejectionNamingThatKey() => Run.Stderr.Should().Contain(UnknownKey);

    [Then("the standard output is empty")]
    public void ThenTheStandardOutputIsEmpty() => Run.Stdout.Should().BeEmpty();

    [Given("the baseline tree the engine's tests build")]
    public void GivenTheBaselineTreeTheEnginesTestsBuild() => BaselineTree.Write(Tree);

    [Given("the golden report of the violations the engine gave on that tree when the copy landed")]
    public void GivenTheGoldenReportOfTheViolationsTheEngineGaveOnThatTreeWhenTheCopyLanded() => _golden = GoldenReport.Read();

    [When("the check runs with the default manifest")]
    public void WhenTheCheckRunsWithTheDefaultManifest()
    {
        foreach (var key in Manifest.Select(static entry => entry.Key).Where(static key => !key.StartsWith('$')).ToList())
        {
            Manifest.Remove(key);
        }

        WhenTheCheckRuns();
    }

    [Then("the same violations are reported, in the same order, each with the same rule id, severity, file, line, identifier and message")]
    public void ThenTheSameViolationsAreReportedInTheSameOrderEachWithTheSameRuleIdSeverityFileLineIdentifierAndMessage() =>
        GoldenReport.Of(_report ?? throw new InvalidOperationException("The check gave no report."))
            .Should()
            .Equal(_golden ?? throw new InvalidOperationException("No golden report was read."));

    [Then("the manifest is rejected")]
    public void ThenTheManifestIsRejected() => _rejection.Should().NotBeNull();

    [Then("the rejection names that key")]
    public void ThenTheRejectionNamesThatKey() => _rejection!.Message.Should().Contain(UnknownKey);

    [Then("none of the three violations is reported")]
    public void ThenNoneOfTheThreeViolationsIsReported() => _report.Should().BeNull();

    [Then("the manifest is accepted")]
    public void ThenTheManifestIsAccepted()
    {
        _rejection.Should().BeNull();
        _report.Should().NotBeNull();
    }

    [Then("no violation is reported")]
    public void ThenNoViolationIsReported() => _report!.Violations.Should().BeEmpty();

    [AfterScenario]
    public void DeleteTree() => _tree?.Dispose();

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No repository root was prepared.");

    private (string Stdout, string Stderr, int ExitCode) Run => _run ?? throw new InvalidOperationException("The tool was not run.");

    private JsonObject Manifest => _manifest ?? throw new InvalidOperationException("No manifest was read.");

    private string ManifestPath => Path.Combine(Tree.Root, ".spec", "schema", "spec-structure.schema.json");

    private string FeatureSchema => _featureSchema ?? throw new InvalidOperationException("The manifest names no Feature schema file.");

    private string RuleId => _ruleId ?? throw new InvalidOperationException("The manifest sets no rule.");

    private WarningRule StandIn => _standIn ?? throw new InvalidOperationException("No rule was stood in.");

    private static string Matrix(string title, string middleHeaders, string middleCells) =>
        $"## {title}\n\n| Claim ID | {middleHeaders} | Status |\n| --- | {string.Join(" | ", middleHeaders.Split('|').Select(static _ => "---"))} | --- |\n"
            + $"| B-001 | {middleCells} | Covered |\n";

    private JsonObject Named(string key)
    {
        if (Manifest[key] is not JsonObject named)
        {
            named = [];
            Manifest[key] = named;
        }

        return named;
    }

    private void Set(string ruleId, JsonNode setting)
    {
        Named("rules")[ruleId] = setting;
        _ruleId = ruleId;
    }

    private string Write(string id, IReadOnlyList<string> sections)
    {
        var path = Tree.WriteFeature("0001", id, sections: sections);
        _specifications.Add(Relative(path));

        return path;
    }

    private string Relative(string path) => Path.GetRelativePath(Tree.Root, path).Replace(Path.DirectorySeparatorChar, '/');

    private IEnumerable<string> HeaderViolationFiles()
    {
        _rejection.Should().BeNull();

        return _report!.Violations.Where(static violation => violation.RuleId == "SPEC013").Select(static violation => violation.File);
    }

    private sealed class WarningRule(string id) : ISpecRule
    {
        public string Id => id;

        public IReadOnlyList<string> ReportedIds => [id];

        public IEnumerable<SpecViolation> Evaluate(SpecModel model) =>
            model.Features.Select(feature =>
                new SpecViolation(id, SpecSeverity.Warning, feature.RelativePath, 1, null, "A stand-in rule reports this specification."));
    }

    private const string UnknownKey = "glossary";

    private const string OutOfOrderRule = "SPEC010";

    private const string RuleOutsideTheVocabulary = "SPEC999";

    private const string LibraryFile = "rules/consumer-rules.dll";

    private const string FeatureSchemaRejectingTheDomain = """{ "properties": { "domain": { "const": "Elsewhere" } } }""";

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";

    private const string PhantomTag =
        "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n  @B-404\n  Scenario: Phantom\n    Given nothing\n";

    private static readonly Dictionary<string, string> DefaultTitles = new(StringComparer.Ordinal)
    {
        ["claims"] = "3. Acceptance Criteria",
        ["matrix"] = "9. Traceability Matrix",
    };

    private readonly List<string> _specifications = [];
    private SpecTree? _tree;
    private JsonObject? _manifest;
    private SpechtReport? _report;
    private (string Stdout, string Stderr, int ExitCode)? _run;
    private SpechtManifestException? _rejection;
    private string? _defaultClaimGrammar;
    private bool _fromRoot;
    private string? _featureSchema;
    private SpecModel? _model;
    private IReadOnlyList<SpecViolation> _frontmatterViolations = [];
    private IReadOnlyList<GoldenReport.Verdict>? _golden;
    private string? _ruleId;
    private int? _rulesEvaluatedByDefault;
    private string? _mark;
    private WarningRule? _standIn;
}
