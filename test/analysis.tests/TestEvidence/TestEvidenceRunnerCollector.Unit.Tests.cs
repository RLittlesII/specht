using AwesomeAssertions;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Tests.TestEvidence;

[Trait("Tier", "Unit")]
public sealed class TestEvidenceRunnerCollectorUnitTests
{
    [Fact]
    public void XunitFact_WhenRunnerSuppliesStableCaseAndMethodIds_ShouldLinkOneCaseToOneMethod()
    {
        // Given
        var methods = new[] { Method("M:Tests.Sample.Fact") };
        var cases = new[] { new TestEvidenceRunnerCaseRecord("xunit-case-fact", "M:Tests.Sample.Fact", null, "Sample.Fact") };

        // When
        var result = Collect(methods, cases);

        // Then
        result.IsValid.Should().BeTrue();
        result.Discovery.State.Should().Be(TestEvidenceDiscoveryState.Complete);
        result.Discovery.Cases.Should().ContainSingle().Which.MethodSymbolId.Should().Be(methods[0].SymbolId);
    }

    [Fact]
    public void XunitTheory_WhenRunnerSuppliesTwoRows_ShouldKeepOneMethodAndTwoDistinctCases()
    {
        // Given
        var methods = new[] { Method("M:Tests.Sample.Theory") };
        var cases = new[]
        {
            new TestEvidenceRunnerCaseRecord("xunit-theory-row-1", "M:Tests.Sample.Theory", "row-1", "Theory(1)"),
            new TestEvidenceRunnerCaseRecord("xunit-theory-row-2", "M:Tests.Sample.Theory", "row-2", "Theory(2)"),
        };

        // When
        var result = Collect(methods, cases);

        // Then
        result.IsValid.Should().BeTrue();
        result.Discovery.Methods.Should().ContainSingle();
        result.Discovery.Cases.Select(static item => item.CaseId).Should().Equal("xunit-theory-row-1", "xunit-theory-row-2");
        result.Discovery.Cases.Select(static item => item.MethodSymbolId).Distinct().Should().ContainSingle();
    }

    [Fact]
    public void PlaywrightDotNet_WhenRunnerSuppliesCSharpMethodIdentity_ShouldRetainTheMethodLink()
    {
        // Given
        var methods = new[] { Method("M:Tests.Sample.PlaywrightTest") };
        var cases = new[] { new TestEvidenceRunnerCaseRecord("playwright-case", "M:Tests.Sample.PlaywrightTest", null, "Playwright test") };

        // When
        var result = Collect(methods, cases, adapterId: "playwright.dotnet.xunit.v3");

        // Then
        result.IsValid.Should().BeTrue();
        result.Discovery.Cases.Should().ContainSingle().Which.MethodSymbolId.Should().Be(methods[0].SymbolId);
        result.AdapterId.Should().Be("playwright.dotnet.xunit.v3");
    }

    [Fact]
    public void RunnerMethodMetadata_WhenItUniquelyMatchesAnOverload_ShouldResolveToCompilerSymbolId()
    {
        // Given
        var methods = new[]
        {
            Method("M:Tests.Sample.Run(System.Int32)", "System.Int32"),
            Method("M:Tests.Sample.Run(System.String)", "System.String"),
        };
        var cases = new[]
        {
            new TestEvidenceRunnerCaseRecord("runner-case", null, null, "Run(1)",
                new TestEvidenceRunnerMethodRecord("Tests.Sample", "Run", 0, new[] { "System.Int32" })),
        };

        // When
        var result = Collect(methods, cases);

        // Then
        result.IsValid.Should().BeTrue();
        result.Discovery.Cases.Should().ContainSingle().Which.MethodSymbolId.Should().Be("M:Tests.Sample.Run(System.Int32)");
    }

    [Fact]
    public void CustomProducer_WhenItSuppliesCommonRecords_ShouldBeAcceptedWithoutFrameworkAllowlist()
    {
        // Given
        var methods = new[] { Method("M:Tests.Custom.Run") };
        var cases = new[] { new TestEvidenceRunnerCaseRecord("custom-case", "M:Tests.Custom.Run", null, "custom display") };

        // When
        var result = Collect(methods, cases, adapterId: "custom.producer.v7");

        // Then
        result.IsValid.Should().BeTrue();
        result.Discovery.Cases.Should().ContainSingle().Which.CaseId.Should().Be("custom-case");
        result.AdapterId.Should().Be("custom.producer.v7");
    }

    [Fact]
    public void RuntimeOnlyRows_WhenRunnerHasNotEnumeratedThem_ShouldRemainDeferredWithoutInventedCases()
    {
        // Given
        var methods = new[] { Method("M:Tests.Sample.RuntimeTheory") };

        // When
        var result = Collect(methods, Array.Empty<TestEvidenceRunnerCaseRecord>(), enumerationComplete: false);

        // Then
        result.IsValid.Should().BeTrue();
        result.Discovery.State.Should().Be(TestEvidenceDiscoveryState.Deferred);
        result.Discovery.Cases.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, "M:Tests.Sample.Fact")]
    [InlineData("runner-case", null)]
    [InlineData("runner-case", "M:Tests.Missing.Method")]
    public void MissingOrUnmatchedRunnerIdentity_WhenCollected_ShouldReportUnavailableEvidence(string? caseId, string? methodId)
    {
        // Given
        var methods = new[] { Method("M:Tests.Sample.Fact") };
        var cases = new[] { new TestEvidenceRunnerCaseRecord(caseId, methodId, null, "display") };

        // When
        var result = Collect(methods, cases);

        // Then
        result.IsValid.Should().BeTrue();
        result.Discovery.State.Should().Be(TestEvidenceDiscoveryState.Unavailable);
        result.Discovery.Cases.Should().BeEmpty();
        result.Results.State.Should().Be("absent");
    }

    [Fact]
    public void ConflictingRunnerOutcomes_WhenCollectedForOneCase_ShouldReportInvalidEvidence()
    {
        // Given
        var methods = new[] { Method("M:Tests.Sample.Fact") };
        var cases = new[] { new TestEvidenceRunnerCaseRecord("runner-case", "M:Tests.Sample.Fact", null, "same name") };
        var results = new[]
        {
            new TestEvidenceRunnerResultRecord("runner-case", "passed", null),
            new TestEvidenceRunnerResultRecord("runner-case", "failed", "conflict"),
        };

        // When
        var result = Collect(methods, cases, results: results);

        // Then
        result.IsValid.Should().BeFalse();
        result.Results.State.Should().Be("absent");
    }

    [Fact]
    public void DuplicateDisplayNames_WhenRunnerSuppliesDistinctCaseIds_ShouldRemainDistinct()
    {
        // Given
        var methods = new[] { Method("M:Tests.Sample.Theory") };
        var cases = new[]
        {
            new TestEvidenceRunnerCaseRecord("runner-case-1", "M:Tests.Sample.Theory", "row-1", "Same name"),
            new TestEvidenceRunnerCaseRecord("runner-case-2", "M:Tests.Sample.Theory", "row-2", "Same name"),
        };

        // When
        var result = Collect(methods, cases);

        // Then
        result.Discovery.Cases.Select(static item => item.DisplayName).Distinct().Should().ContainSingle();
        result.Discovery.Cases.Select(static item => item.CaseId).Distinct().Should().HaveCount(2);
    }

    [Fact]
    public void RepeatedIdenticalOutcomes_WhenCollected_ShouldRemainOneExecution()
    {
        // Given
        var methods = new[] { Method("M:Tests.Sample.Fact") };
        var cases = new[] { new TestEvidenceRunnerCaseRecord("runner-case", "M:Tests.Sample.Fact", null, "Fact") };
        var results = new[]
        {
            new TestEvidenceRunnerResultRecord("runner-case", "passed", null),
            new TestEvidenceRunnerResultRecord("runner-case", "passed", null),
        };

        // When
        var result = Collect(methods, cases, results: results);

        // Then
        result.IsValid.Should().BeTrue();
        result.Results.State.Should().Be("available");
        result.Results.RunId.Should().Be("execution-run");
        result.Results.Cases.Should().ContainSingle().Which.Outcome.Should().Be("passed");
    }

    private static TestEvidenceRunnerCollectionResult Collect(
        IReadOnlyList<TestEvidenceMethod> methods,
        IReadOnlyList<TestEvidenceRunnerCaseRecord> cases,
        string adapterId = "xunit.v3",
        bool enumerationComplete = true,
        IReadOnlyList<TestEvidenceRunnerResultRecord>? results = null) =>
        new TestEvidenceRunnerCollector().Collect(adapterId, "1", methods, cases, "discovery-session", enumerationComplete,
            results is null ? null : "execution-run", results ?? Array.Empty<TestEvidenceRunnerResultRecord>());

    private static TestEvidenceMethod Method(string symbolId, params string[] parameterTypes)
    {
        var memberId = symbolId.Split('(')[0];
        return new TestEvidenceMethod(symbolId, "Tests.Sample", memberId[(memberId.LastIndexOf('.') + 1)..],
            0, parameterTypes, null);
    }
}
