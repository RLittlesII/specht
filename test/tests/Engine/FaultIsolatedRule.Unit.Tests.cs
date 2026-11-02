using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Manifest;
using Specht.Model;
using Specht.Report;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Engine;

/// <summary>Rule failures are atomic, machine-independent errors that do not stop other rules (0001-F1 B-012 to B-017).</summary>
[Trait("Tier", "Unit")]
public sealed class FaultIsolatedRuleUnitTests
{
    [Fact]
    public void TheSameRuleException_WhenReportedAgain_ShouldRetainItsIdentityAndKeepCopiesEqual()
    {
        // Given
        SpechtModel model = new SpechtModelFixture();
        var secret = Path.GetFullPath("private-failure-detail");
        Exception exception = new FormatException(secret);
        exception.Data["private detail"] = secret;
        var rule = new FaultIsolatedRule(new Rule("SPEC030", () => throw exception));

        // When
        var first = rule.Evaluate(model).Single();
        var same = rule.Evaluate(model).Single();
        exception = new FormatException(secret);
        var different = rule.Evaluate(model).Single();

        // Then
        first.Should().Be(same).And.NotBe(different);
        first.GetHashCode().Should().Be(same.GetHashCode());
        (first with { }).Should().Be(first);
        first.ToString().Should().NotContain(secret);
    }

    [Theory]
    [ClassData(typeof(TypedInputFailureData))]
    public void ATypedInputFailure_WhenARuleRaisesIt_ShouldReachTheCallerUnchanged(Exception failure)
    {
        // Given
        SpechtModel model = new SpechtModelFixture();
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
        SpechtModel model = new SpechtModelFixture();
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
        failure.File.Should().Be(SpechtTree.ManifestPath);
        failure.Line.Should().Be(0);
        failure.Identifier.Should().BeNull();
        failure.Message.Should().Contain("SPEC030").And.Contain(nameof(FormatException)).And.NotContain(secret);
        calls.Should().Be(1);

        IEnumerable<SpechtFinding> Findings()
        {
            for (var index = 0; index < yielded; index++)
            {
                yield return new SpechtFindingFixture().WithMessage($"partial {index}");
            }

            throw new FormatException(secret);
        }
    }

    [Fact]
    public void AHealthyRule_WhenDecorated_ShouldKeepItsIdentityDefaultLevelAndFindings()
    {
        // Given
        SpechtModel model = new SpechtModelFixture();
        SpechtFinding finding = new SpechtFindingFixture();
        var inner = new Rule("SPEC030", () => [finding], SpechtRuleLevel.Off);
        var rule = new FaultIsolatedRule(inner);

        // When
        var actual = rule.Evaluate(model).ToArray();

        // Then
        rule.Id.Should().Be(inner.Id);
        rule.DefaultLevel.Should().Be(SpechtRuleLevel.Off);
        actual.Should().Equal(finding);
    }

    [Theory]
    [ClassData(typeof(FaultLevelData))]
    public void AFaultBesideHealthyRules_WhenTheModelIsEvaluated_ShouldContinueAndKeepTheFaultAtError(string level)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithManifest(JsonNode.Parse(
            $$"""{ "rules": { "SPEC030": "{{level}}" } }""")!.AsObject());
        var calls = 0;
        var fault = new FaultIsolatedRule(new Rule("SPEC030", () =>
        {
            calls++;
            throw new InvalidOperationException("sensitive detail");
        }));
        SpechtFinding healthy = new SpechtFindingFixture();
        SpechtViolation before = new SpechtViolationFixture().WithRuleId("SPEC020");
        SpechtViolation after = new SpechtViolationFixture().WithRuleId("SPEC040");

        // When
        var report = new SpechtEvaluator(
            [new Rule("SPEC020", () => [healthy]), fault, new Rule("SPEC040", () => [healthy])], new SpechtViolationOrder()).Evaluate(model);

        // Then
        report.Violations.Should().Contain(before).And.Contain(after);
        report.Violations.Should().ContainSingle(static violation => violation.RuleId == "SPEC030")
            .Which.Severity.Should().Be(SpechtSeverity.Error);
        report.ErrorCount.Should().Be(3);
        calls.Should().Be(1);
        using var json = JsonDocument.Parse(SpechtReportDocument.From(report).ToJson());
        var serialized = json.RootElement.GetProperty("violations").EnumerateArray()
            .Single(static violation => violation.GetProperty("ruleId").GetString() == "SPEC030");
        serialized.EnumerateObject().Select(static property => property.Name).Should()
            .BeEquivalentTo("ruleId", "severity", "file", "line", "message", "expected");
    }

    [Fact]
    public void ADisabledRule_WhenTheModelIsEvaluated_ShouldNeverRunOrReportAFault()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithManifest(JsonNode.Parse(
            """{ "rules": { "SPEC030": "off" } }""")!.AsObject());
        var calls = 0;
        var fault = new FaultIsolatedRule(new Rule("SPEC030", () =>
        {
            calls++;
            throw new InvalidOperationException();
        }));

        // When
        var report = new SpechtEvaluator([fault], new SpechtViolationOrder()).Evaluate(model);

        // Then
        calls.Should().Be(0);
        report.Violations.Should().BeEmpty();
        report.RulesEvaluated.Should().Be(0);
    }

    private sealed class Rule(string id, Func<IEnumerable<SpechtFinding>> evaluate, SpechtRuleLevel level = SpechtRuleLevel.Error) : ISpechtRule
    {
        public string Id => id;
        public SpechtRuleLevel DefaultLevel => level;
        public IEnumerable<SpechtFinding> Evaluate(SpechtModel model) => evaluate();
    }
}
