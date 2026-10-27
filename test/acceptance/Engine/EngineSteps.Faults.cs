using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Specht.Composition;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Engine.Baseline;
using Specht.Tests.Shared;
using Specht.Tool;

namespace Specht.Acceptance.Engine;

public sealed partial class EngineSteps
{
    [Given("one rule rejects an invalid schema input")]
    public void GivenOneRuleRejectsAnInvalidSchemaInput()
    {
        _inputFailure = new SpechtManifestException(".specht/schema/frontmatter/feature.schema.json: invalid schema.");
        _fault = new FailingRule(rejection: _inputFailure);
    }

    [Then("the caller receives the input rejection instead of a rule failure report")]
    public void ThenTheCallerReceivesTheInputRejectionInsteadOfARuleFailureReport()
    {
        _inputRejection.Should().BeSameAs(_inputFailure);
        _report.Should().BeNull();
    }

    [Given("the root holds specifications that break several rules")]
    public void GivenTheRootHoldsSpecificationsThatBreakSeveralRules() => BaselineTree.Write(Tree);

    [Given("one rule fails while it is evaluated")]
    [Given("that rule fails while it is evaluated")]
    public void GivenOneRuleFailsWhileItIsEvaluated() => _fault = new FailingRule();

    [Given("one rule finds two violations and then fails")]
    public void GivenOneRuleFindsTwoViolationsAndThenFails()
    {
        BaselineTree.Write(Tree);
        new ClaimRule().Evaluate(EngineServices.Loader().Load(Tree.Root)).Take(2).Should().HaveCount(2);
        _fault = new FailingRule(2);
    }

    [Given("the manifest sets one rule to warning severity")]
    public void GivenTheManifestSetsOneRuleToWarningSeverity()
    {
        var path = Path.Combine(Tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest["rules"] = new JsonObject { ["SPEC030"] = "warning" };
        File.WriteAllText(path, manifest.ToJsonString());
    }

    [Given("one rule fails with an error whose text holds an absolute path on this machine")]
    public void GivenOneRuleFailsWithAnErrorWhoseTextHoldsAnAbsolutePathOnThisMachine()
    {
        _faultText = Path.Combine(Tree.Root, "sensitive-input");
        _fault = new FailingRule(message: _faultText);
    }

    [Then("every other rule's violations are reported as they are when no rule fails")]
    public void ThenEveryOtherRulesViolationsAreReportedAsTheyAreWhenNoRuleFails()
    {
        var expected = Tree.Run().Violations.Where(static violation => violation.RuleId is not ("SPEC030" or "SPEC031")).ToArray();
        expected.Should().NotBeEmpty();
        Report.Violations.Where(static violation => violation.RuleId != "SPEC030").Should().Equal(expected);
    }

    [Then("exactly one violation is reported under that rule's id, as an error")]
    [Then("the rule's failure is reported as an error")]
    public void ThenTheRulesFailureIsReportedAsAnError() => Failure.Severity.Should().Be(SpecSeverity.Error);

    [Then("it points at the manifest as a whole, with no identifier")]
    public void ThenItPointsAtTheManifestAsAWholeWithNoIdentifier()
    {
        Failure.File.Should().Be(SpecManifestStore.RelativePath);
        Failure.Line.Should().Be(0);
        Failure.Identifier.Should().BeNull();
    }

    [Then("neither of the two violations is reported")]
    public void ThenNeitherOfTheTwoViolationsIsReported()
    {
        var partial = new ClaimRule().Evaluate(EngineServices.Loader().Load(Tree.Root)).Take(2).ToArray();
        partial.Should().HaveCount(2);
        Report.Violations.Should().NotContain(violation => partial.Contains(violation));
    }

    [Then("only the rule's failure is reported under its id")]
    public void ThenOnlyTheRulesFailureIsReportedUnderItsId() => Failure.Message.Should().Contain(nameof(FormatException));

    [Then("the failure's message names the rule and the kind of error")]
    public void ThenTheFailuresMessageNamesTheRuleAndTheKindOfError() =>
        Failure.Message.Should().Contain("SPEC030").And.Contain(nameof(FormatException));

    [Then("it carries none of the error's own text")]
    public void ThenItCarriesNoneOfTheErrorsOwnText() => Failure.Message.Should().NotContain(_faultText!);

    private SpecViolation Failure => Report.Violations.Should().ContainSingle(static violation => violation.RuleId == "SPEC030").Which;

    private ISpecRule[] FaultRules()
    {
        using var resolver = (TypeResolver)new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();
        return ((IEnumerable<ISpecRule>)resolver.Resolve(typeof(IEnumerable<ISpecRule>))!)
            .Select(rule => rule.Id == "SPEC030" ? new FaultIsolatedRule(_fault!) : rule).ToArray();
    }

    private FailingRule? _fault;
    private string? _faultText;
    private SpechtManifestException? _inputFailure;
    private Exception? _inputRejection;
}
