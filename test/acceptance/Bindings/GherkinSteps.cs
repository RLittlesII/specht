using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Reqnroll;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Acceptance.Documentation;
using Specht.Analysis.Bindings;
using Specht.Analysis.Documentation;
using Specht.Analysis.Policy;
using Specht.Analysis.TestEvidence;

namespace Specht.Acceptance.Bindings;

[Binding]
[Scope(Tag = "0201-F7")]
public sealed class GherkinSteps
{
    [Given("the claim file {string} configured as Feature {string}")]
    public void GivenClaimFile(string path, string feature)
    {
        path.Should().Be(ClaimFile);
        feature.Should().Be("arithmetic");
    }

    [Given("a Scenario Outline {string} under the Rule {string} tagged {string} with two example rows")]
    public void GivenOutline(string title, string rule, string tag)
    {
        title.Should().Be("Add two operands");
        rule.Should().Be("Addition returns the sum");
        _outlineTags = tag;
    }

    [Given("the claim tag encoding is {string}")]
    public void GivenEncoding(string form) => _configuration["claim"]!["tag"] = form;

    [Given("the consumer's claim grammar accepts {string}")]
    public void GivenGrammar(string claim) => claim.Should().MatchRegex(_configuration["identifiers"]!["claim"]!.GetValue<string>());

    [Given("no epic identity is configured")]
    public void GivenNoEpic() => _configuration["identifiers"]!.AsObject().ContainsKey("epic").Should().BeFalse();

    [Given("the manifest selects native Gherkin claim input")]
    public void GivenNative() => Records()["claimSource"]!.GetValue<string>().Should().Be("gherkin");

    [Given("the manifest does not select native Gherkin claim input")]
    public void GivenNotNative()
    {
        Records().Remove("claimSource");
        Records().Remove("claimFileSets");
    }

    [Given("Feature {string} is declared by a metadata record with a claims table")]
    public void GivenMarkdownRecord(string feature)
    {
        feature.Should().Be("arithmetic");
        _record = "---\nid: arithmetic\nspec_status: draft\n---\n## 3. Acceptance Criteria\n"
            + "| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n| REQ-FIX-777 | A table claim | Agreement | Active |\n";
        _configuration["claimsSection"] = "3. Acceptance Criteria";
        Analysis()["mappings"]!["/claimsSection"] = "/claimsSection";
    }

    [Given("the area metadata record for Feature {string} contains no claims table")]
    public void GivenRecordWithoutTable(string feature)
    {
        feature.Should().Be("arithmetic");
        UseRecord("draft");
    }

    [Given("the claim file sits in a central specification tree outside every project directory")]
    public void GivenCentralTree() => ClaimFile.Should().NotStartWith("test/");

    [Given("two selected projects reference claim {string}")]
    public void GivenTwoProjects(string claim) => claim.Should().Be(Reference);

    [Given("the consumer's identifier grammar declares no epic")]
    public void GivenGrammarNoEpic() => _configuration["identifiers"]!.AsObject().ContainsKey("epic").Should().BeFalse();

    [Given("a Scenario tagged {string} at line {int} of the claim file")]
    public void GivenScenarioAtLine(string tag, int line) =>
        _leading = string.Concat(Enumerable.Repeat("\n", line - 4)) + "  " + tag + "\n  Scenario: Subtract\n    Given x\n\n";

    [Given("the Rule {string} is tagged {string} and holds two scenarios")]
    public void GivenTaggedRule(string rule, string tag)
    {
        rule.Should().Be("Addition returns the sum");
        _ruleTags = tag;
        _trailing.Add("    Scenario: Add\n      Given x\n\n    Scenario: Subtract\n      Given y\n");
    }

    [Given("the Scenario Outline has been retitled and moved below another scenario")]
    public void GivenRetitled()
    {
        _moved = true;
        _trailing.Add("  @REQ-FIX-002\n  Scenario: Other\n    Given x\n\n");
    }

    [Given("a second scenario in Feature {string} is also tagged {string}")]
    public void GivenSecond(string feature, string tag) => _trailing.Add("    " + tag + "\n    Scenario: Second\n      Given x\n");

    [Given("a scenario tagged {string}")]
    public void GivenScenarioTagged(string tags) => _trailing.Add("    " + tags + "\n    Scenario: Tagged\n      Given x\n");

    [Given("the claim grammar requires three digits after {string}")]
    public void GivenThreeDigits(string prefix) => prefix.Should().Be("REQ-FIX-");

    [Given("the configured claim file is missing")]
    public void GivenMissing() => _missing = true;

    [Given("the configured claim file is not valid Gherkin")]
    public void GivenInvalid() => _invalid = true;

    [Given("a Scenario in the claim file carries no claim tag of its own or from its Rule")]
    public void GivenUntagged() => _trailing.Add("    Scenario: Untagged\n      Given x\n");

    [Given("the Scenario Outline carries a tag the consumer configures as ignored")]
    public void GivenIgnored() => _outlineTags += " @ignore";

    [Given("a mandatory enforcement scope selects the addition production method")]
    public void GivenProductionScope() => (_enforcementRole, _enforcementKind) = ("implements", "method");

    [Given("a mandatory enforcement scope selects the addition test method")]
    public void GivenTestScope() => (_enforcementRole, _enforcementKind) = ("verifies", "testMethod");

    [Given("a mandatory enforcement scope selects the addition test method, which carries no verifies relationship")]
    public void GivenTestScopeNoRelationship()
    {
        GivenTestScope();
        _bare = true;
    }

    [Given("the addition production method carries no implements relationship")]
    public void GivenProductionBare() => _bare = true;

    [Given("the addition test method carries no verifies relationship")]
    public void GivenTestBare() => _bare = true;

    [Given("the addition test method verifies claim {string} of Feature {string}")]
    public void GivenVerifies(string claim, string feature)
    {
        _verifiesClaim = claim;
        _verifiesFeature = feature;
        GivenTestScope();
    }

    [Given("the Scenario Outline carries no lifecycle tag")]
    public void GivenNoLifecycle() => _outlineTags.Should().NotContain("@withdrawn").And.NotContain("@superseded");

    [Given("the consumer configures a tag meaning Withdrawn")]
    public void GivenWithdrawnTag() => _lifecycleTag = " @withdrawn";

    [Given("the consumer configures a tag meaning Superseded")]
    public void GivenSupersededTag() => _lifecycleTag = " @superseded";

    [Given("the Scenario Outline carries that tag")]
    public void GivenCarriesTag() => _outlineTags += _lifecycleTag;

    [Given("the configured metadata record for Feature {string} states {string}")]
    public void GivenRecordState(string feature, string state) => UseRecord(state);

    [Given("no metadata record is configured for Feature {string}")]
    public void GivenNoRecord(string feature) => _record = null;

    [Given("the claim file carries a Feature-level tag naming Feature {string}")]
    public void GivenFeatureTag(string feature) => _featureTagLine = "@spec.feature:" + feature + "\n";

    [Given("the Scenario Outline tagged {string} has a second Examples block tagged {string}")]
    public void GivenSecondExamples(string outline, string tag) => _secondExamples = ExamplesBlock(tag);

    [Given("an Examples block of the Scenario Outline is tagged {string}")]
    public void GivenTaggedExamples(string tag) => _secondExamples = ExamplesBlock(tag);

    [Given("the configured claim file declares the Gherkin language {string}")]
    public void GivenLanguage(string language) => _languageLine = "# language: " + language + "\n";

    [When("the declared analysis is requested")]
    public void WhenAnalyzed()
    {
        _given = Build();
        _before = _given.Select(Describe).ToArray();
        _snapshot = Read(_given);
    }

    [When("the declared analysis is requested for each project")]
    public void WhenAnalyzedForEachProject()
    {
        _snapshot = Read(Build());
        Analysis()["projects"] = new JsonArray(new JsonObject { ["path"] = "other/Other.csproj", ["targetFrameworks"] = new JsonArray("net10.0") });
        _otherProject = Read(Build());
    }

    [When("a relationship names Feature {string} and claim {string}")]
    public void WhenRelationship(string feature, string claim)
    {
        _snapshot = Read(Build());
        _verifiesFeature = feature;
        _verifiesClaim = claim;
    }

    [When("the analyzer writes the test evidence")]
    public void WhenEvidence()
    {
        _snapshot = Read(Build());
        _exported = new TestEvidenceCatalogExporter().Project(_snapshot.Catalog!);
    }

    [Then("the catalogue for Feature {string} holds claim {string} read from the claim file")]
    public void ThenHolds(string feature, string claim)
    {
        var held = Feature(feature);
        held.Claims.Select(static item => item.Id).Should().Equal(claim);
        held.Claims.Single().RelativePath.Should().Be(ClaimFile);
    }

    [Then("the catalogue holds exactly the claims of that claims table")]
    public void ThenTableClaims() =>
        _snapshot!.Catalog!.Features.Single().Claims.Select(static item => item.Id).Should().Equal("REQ-FIX-777");

    [Then("claim {string} belongs to Feature {string}")]
    public void ThenBelongs(string claim, string feature) =>
        _snapshot!.Catalog!.Features.Single(item => item.Claims.Any(entry => entry.Id == claim)).Identity.Feature.Should().Be(feature);

    [Then("the claim identifier is {string} exactly as tagged")]
    public void ThenAsTagged(string claim) => Feature("arithmetic").Claims.Select(static item => item.Id).Should().Equal(claim);

    [Then("no input failure is reported for a missing claims section")]
    public void ThenNoFailure()
    {
        _snapshot!.Failures.Should().BeEmpty();
        _snapshot.Available.Should().BeTrue();
    }

    [Then("both projects resolve the same claim {string} of Feature {string}")]
    public void ThenBothResolve(string claim, string feature)
    {
        var reference = new ClaimReference(null, feature, claim);
        var first = new BindingResolver(_snapshot!.Catalog!, _snapshot.Policy!.Bindings).Resolve(reference);
        var second = new BindingResolver(_otherProject!.Catalog!, _otherProject.Policy!.Bindings).Resolve(reference);
        first.Status.Should().Be(BindingResolution.Resolved);
        second.Status.Should().Be(BindingResolution.Resolved);
        second.Target.Should().BeEquivalentTo(first.Target);
    }

    [Then("it resolves without an epic identity")]
    public void ThenNoEpic()
    {
        var result = new BindingResolver(_snapshot!.Catalog!, _snapshot.Policy!.Bindings)
            .Resolve(new ClaimReference(null, _verifiesFeature, _verifiesClaim));
        result.Status.Should().Be(BindingResolution.Resolved);
        result.Target!.Epic.Should().BeNull();
    }

    [Then("claim {string} is located at that Scenario's path and line {int}")]
    public void ThenLocated(string claim, int line)
    {
        var found = Feature("arithmetic").Claims.Single(item => item.Id == claim);
        found.RelativePath.Should().Be(ClaimFile);
        found.Line.Should().Be(line);
        LineOf("Scenario: Subtract").Should().Be(line);
    }

    [Then("the catalogue holds one claim {string} and not one claim per example row")]
    public void ThenOneClaim(string claim) => Feature("arithmetic").Claims.Select(static item => item.Id).Should().Equal(claim);

    [Then("the claim is located at the Scenario Outline")]
    public void ThenAtOutline()
    {
        var found = Feature("arithmetic").Claims.Single();
        found.Element.Should().Be(BindingClaimElement.ScenarioOutline);
        found.Line.Should().Be(LineOf("Scenario Outline:"));
    }

    [Then("the catalogue holds one claim {string} located at the Rule")]
    public void ThenAtRule(string claim)
    {
        var found = Feature("arithmetic").Claims.Single(item => item.Id == claim);
        found.Element.Should().Be(BindingClaimElement.Rule);
        found.Line.Should().Be(LineOf("Rule:"));
    }

    [Then("no duplicate claim is reported for the two scenarios")]
    public void ThenNoDuplicate() => _snapshot!.Catalog!.Findings.Should().BeEmpty();

    [Then("claim {string} of Feature {string} keeps its identity")]
    public void ThenIdentity(string claim, string feature)
    {
        var result = new BindingResolver(_snapshot!.Catalog!, _snapshot.Policy!.Bindings).Resolve(new ClaimReference(null, feature, claim));
        result.Status.Should().Be(BindingResolution.Resolved);
        result.Target!.Id.Should().Be(claim);
        result.Target.Line.Should().Be(LineOf("Scenario Outline:"));
    }

    [Then("a duplicate-claim diagnostic is reported at each declaration's path and line")]
    public void ThenDuplicate() =>
        _snapshot!.Catalog!.Findings.Where(static finding => finding.DiagnosticId == "SPCT0010")
            .Select(static finding => (finding.RelativePath, finding.Line)).Should()
            .Equal((ClaimFile, LineOf("Scenario Outline:")), (ClaimFile, LineOf("Scenario: Second")));

    [Then("one claim {string} is recorded")]
    public void ThenRecorded(string claim) => Feature("arithmetic").Claims.Select(static item => item.Id).Should().Contain(claim);

    [Then("{string}, {string}, {string} and {string} declare no claim and draw no diagnostic")]
    public void ThenOrdinary(string a, string b, string c, string d)
    {
        Feature("arithmetic").Claims.Select(static item => item.Id).Should().NotContain(new[] { a, b, c, d }.Select(static tag => tag.TrimStart('@')));
        _snapshot!.Catalog!.Findings.Should().BeEmpty();
    }

    [Then("no claim is recorded for {string}")]
    [Then("no claim {string} is recorded")]
    public void ThenNoClaim(string tag) =>
        Feature("arithmetic").Claims.Select(static item => item.Id).Should().NotContain(tag.TrimStart('@'));

    [Then("no malformed-claim diagnostic is reported for {string}")]
    public void ThenNoMalformed(string tag) => _snapshot!.Catalog!.Findings.Should().NotContain(static finding => finding.DiagnosticId == "SPCT0011");

    [Then("an input failure names the claim file")]
    public void ThenFailureNamesFile()
    {
        _snapshot!.Failures.Should().ContainSingle().Which.RelativePath.Should().Be(ClaimFile);
        _snapshot.Available.Should().BeFalse();
    }

    [Then("Feature {string} is not reported as having no claims")]
    public void ThenNotNoClaims(string feature) => _snapshot!.Catalog.Should().BeNull();

    [Then("a missing-claim diagnostic is reported at that Scenario's line")]
    public void ThenMissingClaim()
    {
        var finding = _snapshot!.Catalog!.Findings.Should().ContainSingle().Subject;
        finding.DiagnosticId.Should().Be("SPCT0012");
        finding.RelativePath.Should().Be(ClaimFile);
        finding.Line.Should().Be(LineOf("Scenario: Untagged"));
    }

    [Then("that scenario counts as neither implemented nor passing evidence for claim {string}")]
    public void ThenPlanned(string claim) =>
        Feature("arithmetic").Claims.Single(item => item.Id == claim).Cases.Should().NotBeEmpty().And.OnlyContain(static item => item.Planned);

    [Then("the same required-link diagnostic a Markdown-declared claim receives is reported")]
    public void ThenRequiredLink()
    {
        Analyze().GetAwaiter().GetResult();
        _diagnostics.Select(static diagnostic => diagnostic.Id).Should().Contain("SPCT0007");
    }

    [Then("the same target-resolution failure a Markdown-declared claim receives is reported")]
    public void ThenUnknown()
    {
        Analyze().GetAwaiter().GetResult();
        _diagnostics.Select(static diagnostic => diagnostic.Id).Should().Contain("SPCT0004");
    }

    [Then("the claim file and the area metadata record are unchanged")]
    public void ThenUnchanged() => _given.Select(Describe).Should().Equal(_before);

    [Then("no claims table or other adapter is written")]
    public void ThenNoAdapter() => _given.Should().HaveCount(_before.Length);

    [Then("claim {string} is Active")]
    public void ThenActive(string claim) => Feature("arithmetic").Claims.Single(item => item.Id == claim).State.Should().Be("Active");

    [Then("claim {string} is Withdrawn")]
    public void ThenWithdrawn(string claim) => Feature("arithmetic").Claims.Single(item => item.Id == claim).State.Should().Be("Withdrawn");

    [Then("claim {string} is Superseded")]
    public void ThenSuperseded(string claim) => Feature("arithmetic").Claims.Single(item => item.Id == claim).State.Should().Be("Superseded");

    [Then("Feature {string} is superseded")]
    public void ThenFeatureSuperseded(string feature) => Feature(feature).State.Should().Be("superseded");

    [Then("Feature {string} is current")]
    public void ThenFeatureCurrent(string feature) => Feature(feature).State.Should().Be("current");

    [Then("claim {string} keeps its identity")]
    public void ThenKeeps(string claim) => Feature("arithmetic").Claims.Select(static item => item.Id).Should().Contain(claim);

    [Then("claim {string} is still declared")]
    public void ThenStillDeclared(string claim) => Feature("arithmetic").Claims.Select(static item => item.Id).Should().Contain(claim);

    [Then("the required-link diagnostic is still reported")]
    public void ThenStillReported() => ThenRequiredLink();

    [Then("the rows of the first Examples block remain required")]
    public void ThenFirstRequired() =>
        Feature("arithmetic").Claims.Single().Cases!.Where(static item => !item.Planned).Should().HaveCount(2);

    [Then("only the rows of the {string} Examples block are planned")]
    public void ThenOnlySecondPlanned(string tag) =>
        Feature("arithmetic").Claims.Single().Cases!.Where(static item => item.Planned).Should().ContainSingle();

    [Then("a malformed-claim diagnostic is reported at the path and line of {string}")]
    public void ThenMalformed(string tag)
    {
        var finding = _snapshot!.Catalog!.Findings.Should().ContainSingle().Subject;
        finding.DiagnosticId.Should().Be("SPCT0011");
        finding.RelativePath.Should().Be(ClaimFile);
        finding.Line.Should().Be(LineOf(tag));
    }

    [Then("the evidence holds claim {string} of Feature {string} with its two example rows")]
    public void ThenEvidence(string claim, string feature)
    {
        var held = _exported.Should().ContainSingle(item => item.Feature == feature).Subject;
        held.Claims.Single(item => item.Id == claim).Cases.Should().HaveCount(2);
    }

    private static string ExamplesBlock(string tag) => "\n      " + tag + "\n      Examples:\n        | a | b | sum |\n        | 5 | 5 | 10 |\n";

    private static string Describe(AnalysisInput input) => input.Kind + "|" + input.RelativePath + "|" + input.Text;

    private static AnalysisSnapshot Read(IReadOnlyList<AnalysisInput> inputs) => new AnalysisInputReader().Read(inputs, CancellationToken.None);

    private JsonObject Analysis() => _configuration["codeAnalysis"]!.AsObject();

    private JsonObject Records() => Analysis()["records"]!.AsObject();

    private BindingFeature Feature(string feature) => _snapshot!.Catalog!.Features.Single(item => item.Identity.Feature == feature);

    private void UseRecord(string state)
    {
        _record = "---\nid: arithmetic\nspec_status: " + state + "\n---\n# Arithmetic\n";
        Records()["claimFileSets"] = new JsonArray(new JsonObject
        {
            ["glob"] = "specifications/*.feature",
            ["feature"] = "arithmetic",
            ["record"] = RecordFile,
        });
    }

    private int LineOf(string fragment) => Document().Split('\n').ToList().FindIndex(line => line.Contains(fragment, StringComparison.Ordinal)) + 1;

    private string Document()
    {
        if (_invalid)
        {
            return "Scenario: Orphan\n  Given x\n";
        }

        var outline = (_outlineTags.Length == 0 ? string.Empty : "    " + _outlineTags + "\n")
            + "    Scenario Outline: Add two operands\n      Given <a> and <b>\n      Then the sum is <sum>\n\n"
            + "      Examples:\n        | a | b | sum |\n        | 1 | 2 | 3   |\n        | 2 | 2 | 4   |\n" + _secondExamples;
        var others = string.Join("\n", _trailing);
        var rule = (_ruleTags.Length == 0 ? string.Empty : "  " + _ruleTags + "\n") + "  Rule: Addition returns the sum\n\n";
        var body = _ruleTags.Length != 0 ? rule + others
            : _moved ? others + "\n" + rule + outline
            : rule + outline + (others.Length == 0 ? string.Empty : "\n" + others);
        return _languageLine + _featureTagLine + "Feature: Arithmetic\n\n" + _leading + body;
    }

    private List<AnalysisInput> Build()
    {
        Analysis()["enforcement"] = _enforcementRole is null
            ? new JsonArray()
            : new JsonArray(new JsonObject
            {
                ["project"] = "test/tests.csproj",
                ["include"] = new JsonArray("test/**/*.cs"),
                ["exclude"] = new JsonArray(),
                ["role"] = _enforcementRole,
                ["declarationKinds"] = new JsonArray(_enforcementKind),
            });
        var inputs = new List<AnalysisInput>
        {
            new("configuration", "config/consumer.json", _configuration.ToJsonString()),
            new("schema:feature", "contracts/feature.json",
                """{"type":"object","required":["id","spec_status"],"properties":{"id":{"type":"string"},"spec_status":{"type":"string"}}}"""),
        };
        if (Records().ContainsKey("claimSource"))
        {
            inputs.Add(new AnalysisInput("scenario", ClaimFile, _missing ? null : Document()));
        }

        if (_record is not null)
        {
            inputs.Add(new AnalysisInput("feature", RecordFile, _record));
        }

        return inputs;
    }

    private async Task Analyze()
    {
        _enforcementRole.Should().NotBeNull();
        var annotation = _bare ? string.Empty
            : "    /// <" + _enforcementRole + " feature=\"" + _verifiesFeature + "\" claim=\"" + _verifiesClaim + "\" />\n";
        var source = "public class Example\n{\n" + annotation + "    public void Run() {}\n}";
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DocumentationBindingAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("test/Example.cs", source)
            .AddGlobalOption("build_property.SpechtProjectPath", "test/tests.csproj")
            .AddGlobalOption("build_property.TargetFramework", "net10.0");
        foreach (var input in Build())
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        if (_enforcementKind == "testMethod")
        {
            builder = builder.AddAdditionalText("test/evidence.json", AcceptanceTestEvidence.ForMethod("M:Example.Run"))
                .AddOption("test/evidence.json", "build_metadata.AdditionalFiles.SpechtInputKind", "test-evidence")
                .AddOption("test/evidence.json", "build_metadata.AdditionalFiles.SpechtRelativePath", "test/evidence.json");
        }

        var result = await builder.GenerateAsync(CancellationToken.None);
        _diagnostics = result.AnalyzerResults[typeof(DocumentationBindingAnalyzer)].Diagnostics;
    }

    private const string ClaimFile = "specifications/arithmetic.feature";
    private const string RecordFile = "src/arithmetic/.spec/README.md";
    private const string Reference = "REQ-FIX-001";

    private readonly JsonObject _configuration = JsonNode.Parse("""
        {
          "schemaVersion": "0.2.0",
          "identifiers": { "feature": "^[a-z]+$", "claim": "^REQ-FIX-[0-9]{3}$" },
          "markers": { "approved": "approved", "withdrawn": "Withdrawn", "superseded": "Superseded" },
          "claim": { "tag": "@{claim}", "tagIgnoreCase": false },
          "codeAnalysis": {
            "mappings": {
              "/schemaVersion": "/schemaVersion",
              "/identifiers": "/identifiers",
              "/markers": "/markers",
              "/claim": "/claim"
            },
            "projects": [{ "path": "test/tests.csproj", "targetFrameworks": ["net10.0"] }],
            "features": [{ "feature": "arithmetic" }],
            "enforcement": [],
            "records": {
              "claimSource": "gherkin",
              "claimFileSets": [{ "glob": "specifications/*.feature", "feature": "arithmetic" }],
              "plannedTags": ["@ignore"],
              "withdrawnTags": ["@withdrawn"],
              "supersededTags": ["@superseded"]
            },
            "scenarioScopes": { "featureTag": "@spec.feature:{feature}" }
          }
        }
        """)!.AsObject();

    private readonly List<string> _trailing = new();
    private string _leading = string.Empty;
    private string _languageLine = string.Empty;
    private string _featureTagLine = string.Empty;
    private string _ruleTags = string.Empty;
    private string _outlineTags = "@REQ-FIX-001";
    private string _secondExamples = string.Empty;
    private string _lifecycleTag = string.Empty;
    private string? _record;
    private string? _enforcementRole;
    private string? _enforcementKind;
    private string _verifiesFeature = "arithmetic";
    private string _verifiesClaim = Reference;
    private bool _missing;
    private bool _invalid;
    private bool _moved;
    private bool _bare;
    private AnalysisSnapshot? _snapshot;
    private AnalysisSnapshot? _otherProject;
    private IReadOnlyList<Diagnostic> _diagnostics = Array.Empty<Diagnostic>();
    private IReadOnlyList<TestEvidenceCatalogFeature> _exported = Array.Empty<TestEvidenceCatalogFeature>();
    private string[] _before = Array.Empty<string>();
    private List<AnalysisInput> _given = new();
}
