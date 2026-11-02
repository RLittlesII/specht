using System.IO.Abstractions;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Tool.Features.Init;

namespace Specht.Acceptance.Versioning;

/// <summary>
/// The delivery axis of the missing-coverage rule at schema 0.3.0 (0001-F7 B-118 to B-124; decision 0015), item 0321.
/// "The manifest pins schema version ..." writes the unquoted pin; the specification is co-located at
/// <c>src/sample/.spec/README.md</c>, the tree's canonical sections carrying a § 9 row of <c>Missing</c> for B-001 and a
/// § 12 row awaiting review, with the frontmatter's <c>spec_status</c> and delivery <c>status</c> the step names, and the
/// tree's own <c>spec_status</c> where the step names none. "The check runs" launches the built tool with <c>--json</c>.
/// B-123 reads the pinned vocabulary through the engine's <see cref="RuleEntriesReader"/> and runs the engine in process
/// over a done Feature with a missing row, so it asserts which ids report that row and not which ids the version holds:
/// the owner adds rules to the unpublished 0.3.0. B-124 reads the tool's embedded feature template.
/// </summary>
public sealed partial class VersioningSteps
{
    [Given(@"^the manifest pins schema version (\d+\.\d+\.\d+)$")]
    public void GivenTheManifestPinsUnquotedSchemaVersion(string version) => WriteSchemaVersion(JsonValue.Create(version));

    [Given(@"^the root holds a specification whose delivery status is (done|in progress) and whose traceability table has a missing test$")]
    public void GivenTheRootHoldsASpecificationWithAMissingTest(string status) => WriteSample(null, status);

    [Given(@"^the root holds an? (draft|approved) specification whose delivery status is (done|in progress) and whose traceability table has a missing test$")]
    public void GivenTheRootHoldsASpecificationAtASpecStatusWithAMissingTest(string specStatus, string status) =>
        WriteSample(specStatus, status);

    [Given("the root holds a specification in review whose delivery status is done and whose sign-off has a row awaiting review")]
    public void GivenTheRootHoldsASpecificationInReviewAwaitingSignOff() => WriteSample("in-review", "done");

    [When("its vocabulary is read")]
    public void WhenItsVocabularyIsRead()
    {
        WriteSchemaVersion(JsonValue.Create(_embedded!.Number.ToString()));
        WriteSample(null, "done");
        _entries = new RuleEntriesReader(EngineServices.Schemas(new FileSystem()), EngineServices.Parser()).Read(Tree.Root);
        _report = EngineServices.Runner().Run(Tree.Root);
    }

    [When("its feature template is read")]
    public void WhenItsFeatureTemplateIsRead() =>
        _template = Encoding.UTF8.GetString(
            InitWriter.ShippingCopy(typeof(InitWriter).Assembly)[$"templates/v{_embedded!.Number}/feature.md"]);

    [Then("the missing test is reported by the missing-coverage rule")]
    public void ThenTheMissingTestIsReportedByTheMissingCoverageRule() =>
        Violations
            .Where(static violation => violation["ruleId"]!.GetValue<string>() == MissingCoverageRule
                                       && violation["file"]!.GetValue<string>() == SamplePath)
            .Select(static violation => violation["identifier"]?.GetValue<string>())
            .Should()
            .Equal("B-001");

    [Then("the missing-coverage rule reports nothing")]
    public void ThenTheMissingCoverageRuleReportsNothing() => ThenTheRuleReportsNothing(MissingCoverageRule);

    [Then("the sign-off rule reports nothing")]
    public void ThenTheSignOffRuleReportsNothing() => ThenTheRuleReportsNothing(SignOffRule);

    [Then("the missing-coverage message is {string}")]
    public void ThenTheMissingCoverageMessageIs(string message) =>
        Violations
            .Where(static violation => violation["ruleId"]!.GetValue<string>() == MissingCoverageRule)
            .Select(static violation => violation["message"]!.GetValue<string>())
            .Should()
            .Equal(message);

    [Then("the missing-coverage rule is SPEC060 at error severity")]
    public void ThenTheMissingCoverageRuleIsSpec060AtErrorSeverity()
    {
        _entries.Should().ContainSingle(static entry => entry.Id == MissingCoverageRule)
            .Which.DefaultSeverity.Should().Be(SpechtRuleLevel.Error);
        Report.Violations.Where(static violation => violation.RuleId == MissingCoverageRule)
            .Select(static violation => (violation.Identifier, violation.Severity))
            .Should()
            .Equal(("B-001", SpechtSeverity.Error));
    }

    [Then("no other rule id reports missing coverage")]
    public void ThenNoOtherRuleIdReportsMissingCoverage()
    {
        var line = File.ReadAllLines(Path.Combine(Tree.Root, SamplePath)).ToList().IndexOf(MissingRow) + 1;
        line.Should().BePositive("the sample's § 9 carries the row '{0}'", MissingRow);
        Report.Violations
            .Where(violation => violation.File == SamplePath && violation.Line == line)
            .Select(static violation => violation.RuleId)
            .Should()
            .Equal(MissingCoverageRule);
    }

    [Then("its traceability guidance says a missing row does not block the specification reaching approved")]
    public void ThenItsTraceabilityGuidanceSaysAMissingRowDoesNotBlockApproval() =>
        TraceabilityGuidance.Should().Contain("it does not block spec_status reaching approved;");

    [Then("that the missing-coverage rule reports it once the delivery status is done, unless the manifest lowers the rule")]
    public void ThenTheMissingCoverageRuleReportsItOnceTheDeliveryStatusIsDone() =>
        TraceabilityGuidance.Should().Contain(
            "it does not block spec_status reaching approved; `SPEC060` reports it once status is done, unless the manifest lowers the rule");

    private string TraceabilityGuidance
    {
        get
        {
            var template = _template ?? throw new InvalidOperationException("No feature template was read.");
            var start = template.IndexOf("## 9. ", StringComparison.Ordinal);
            start.Should().BeGreaterThanOrEqualTo(0, "the feature template has a § 9");
            var end = template.IndexOf("## 10. ", start, StringComparison.Ordinal);
            return Regex.Replace(end < 0 ? template[start..] : template[start..end], @"\s+", " ");
        }
    }

    private void ThenTheRuleReportsNothing(string ruleId)
    {
        ThenTheCheckCompletes();
        Violations.Should().NotContain(violation => violation["ruleId"]!.GetValue<string>() == ruleId);
    }

    private void WriteSample(string? specStatus, string status)
    {
        var frontmatter = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["status"] = status == "in progress" ? "in-progress" : status,
        };
        if (specStatus is not null)
        {
            frontmatter["spec_status"] = specStatus;
        }

        Tree.WriteCoLocatedFeature("src/sample", "0001", "F1", frontmatter);
    }

    private const string MissingCoverageRule = "SPEC060";

    private const string SignOffRule = "SPEC061";

    private const string SamplePath = "src/sample/.spec/README.md";

    private const string MissingRow = "| B-001 | It does the thing | Missing | Missing |";

    private IReadOnlyList<RuleEntry> _entries = [];

    private string? _template;
}
