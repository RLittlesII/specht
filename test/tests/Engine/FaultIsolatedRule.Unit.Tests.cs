using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Model;
using Specht.Report;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Engine;

/// <summary>Rule failures are atomic, machine-independent errors that do not stop other rules (0001-F1 B-012 to B-016).</summary>
[Trait("Tier", "Unit")]
public sealed class FaultIsolatedRuleUnitTests
{
    [Theory]
    [ClassData(typeof(TypedInputFailureData))]
    public void ATypedInputFailure_WhenARuleRaisesIt_ShouldReachTheCallerUnchanged(Exception failure)
    {
        // Given
        SpecModel model = new SpecModelFixture();
        var rule = new FaultIsolatedRule(new Rule("SPEC030", () => throw failure));

        // When
        var actual = Record.Exception(() => rule.Evaluate(model).ToArray());

        // Then
        actual.Should().BeSameAs(failure);
    }

    [Theory]
    [ClassData(typeof(RuleFailureData))]
    public void ARuleThatThrows_WhenEvaluated_ShouldReplaceAllItsOutputWithOneSafeError(int yielded, bool immediate)
    {
        // Given
        SpecModel model = new SpecModelFixture();
        var calls = 0;
        var secret = Path.GetFullPath("sensitive-input");
        var rule = new FaultIsolatedRule(new Rule("SPEC030", () =>
        {
            calls++;
            return immediate ? throw new FormatException(secret) : Findings();
        }));

        // When
        var actual = rule.Evaluate(model).ToArray();

        // Then
        var failure = actual.Should().ContainSingle().Which;
        failure.RuleId.Should().Be("SPEC030");
        failure.Severity.Should().Be(SpecSeverity.Error);
        failure.File.Should().Be(SpecTree.ManifestPath);
        failure.Line.Should().Be(0);
        failure.Identifier.Should().BeNull();
        failure.Message.Should().Contain("SPEC030").And.Contain(nameof(FormatException)).And.NotContain(secret);
        calls.Should().Be(1);

        IEnumerable<SpecViolation> Findings()
        {
            for (var index = 0; index < yielded; index++)
            {
                yield return new SpecViolationFixture().WithRuleId("SPEC030").WithMessage($"partial {index}");
            }

            throw new FormatException(secret);
        }
    }

    [Fact]
    public void AHealthyRule_WhenDecorated_ShouldKeepItsIdentityDefaultLevelAndFindings()
    {
        // Given
        SpecModel model = new SpecModelFixture();
        SpecViolation finding = new SpecViolationFixture().WithRuleId("SPEC030");
        var inner = new Rule("SPEC030", () => [finding], SpecRuleLevel.Off);
        var rule = new FaultIsolatedRule(inner);

        // When
        var actual = rule.Evaluate(model).ToArray();

        // Then
        rule.Id.Should().Be(inner.Id);
        rule.ReportedIds.Should().Equal(inner.ReportedIds);
        rule.DefaultLevel.Should().Be(SpecRuleLevel.Off);
        actual.Should().Equal(finding);
    }

    [Theory]
    [ClassData(typeof(FaultLevelData))]
    public void AFaultBesideHealthyRules_WhenTheModelIsEvaluated_ShouldContinueAndKeepTheFaultAtError(string level)
    {
        // Given
        SpecModel model = new SpecModelFixture().WithManifest(JsonNode.Parse(
            $$"""{ "rules": { "SPEC030": "{{level}}" } }""")!.AsObject());
        var calls = 0;
        var fault = new FaultIsolatedRule(new Rule("SPEC030", () =>
        {
            calls++;
            throw new InvalidOperationException("sensitive detail");
        }));
        SpecViolation before = new SpecViolationFixture().WithRuleId("SPEC020");
        SpecViolation after = new SpecViolationFixture().WithRuleId("SPEC040");

        // When
        var report = SpechtRunner.Evaluate(model,
            [new Rule("SPEC020", () => [before]), fault, new Rule("SPEC040", () => [after])]);

        // Then
        report.Violations.Should().Contain(before).And.Contain(after);
        report.Violations.Should().ContainSingle(static violation => violation.RuleId == "SPEC030")
            .Which.Severity.Should().Be(SpecSeverity.Error);
        report.ErrorCount.Should().Be(3);
        calls.Should().Be(1);
        using var json = JsonDocument.Parse(SpecReportDocument.From(report).ToJson());
        var serialized = json.RootElement.GetProperty("violations").EnumerateArray()
            .Single(static violation => violation.GetProperty("ruleId").GetString() == "SPEC030");
        serialized.EnumerateObject().Select(static property => property.Name).Should()
            .BeEquivalentTo("ruleId", "severity", "file", "line", "message", "expected");
    }

    [Fact]
    public void ADisabledRule_WhenTheModelIsEvaluated_ShouldNeverRunOrReportAFault()
    {
        // Given
        SpecModel model = new SpecModelFixture().WithManifest(JsonNode.Parse(
            """{ "rules": { "SPEC030": "off", "SPEC031": "off" } }""")!.AsObject());
        var calls = 0;
        var fault = new FaultIsolatedRule(new Rule("SPEC030", () =>
        {
            calls++;
            throw new InvalidOperationException();
        }));

        // When
        var report = SpechtRunner.Evaluate(model, [fault]);

        // Then
        calls.Should().Be(0);
        report.Violations.Should().BeEmpty();
        report.RulesEvaluated.Should().Be(0);
    }

    private sealed class Rule(string id, Func<IEnumerable<SpecViolation>> evaluate, SpecRuleLevel level = SpecRuleLevel.Error) : ISpecRule
    {
        public string Id => id;
        public IReadOnlyList<string> ReportedIds => id == "SPEC030" ? [id, "SPEC031"] : [id];
        public SpecRuleLevel DefaultLevel => level;
        public IEnumerable<SpecViolation> Evaluate(SpecModel model) => evaluate();
    }
}
