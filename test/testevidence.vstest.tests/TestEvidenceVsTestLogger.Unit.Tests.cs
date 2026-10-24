using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace Specht.TestEvidence.VsTest.Tests;

/// <summary>The VSTest logger's event handling, driven in process (0201-F3 B-010, B-011, B-013 to B-016, B-020, B-025; decision 0003).</summary>
[Trait("Tier", "Unit")]
public sealed class TestEvidenceVsTestLoggerUnitTests
{
    [Fact]
    public void TwoTheoryRows_WhenDiscovered_ShouldRecordEachRowWithItsOwnCaseIdAgainstOneMethod()
    {
        // Given
        var source = new VsTestSource();
        var first = source.Case(FirstId, "A.LocalCases.Theory", "A.LocalCases", "Theory");
        var second = source.Case(SecondId, "A.LocalCases.Theory", "A.LocalCases", "Theory");

        // When
        source.Attach().Discover(source.Source, first, second);

        // Then
        var artifact = source.Artifact();
        artifact["formatVersion"]!.GetValue<int>().Should().Be(3);
        artifact["discovery"]!["state"]!.GetValue<string>().Should().Be("complete");
        artifact["results"]!["state"]!.GetValue<string>().Should().Be("absent");
        var cases = artifact["discovery"]!["cases"]!.AsArray();
        cases.Select(static item => item!["caseId"]!.GetValue<string>()).Should().BeEquivalentTo(FirstId, SecondId);
        cases.Select(static item => item!["methodSymbolId"]!.GetValue<string>()).Should().AllBe("M:A.LocalCases.Theory(System.Int32)");
        cases.Select(static item => item!["rowId"]).Should().AllSatisfy(static row => row.Should().BeNull());
        cases.Select(static item => item!["selected"]!.GetValue<bool>()).Should().AllBeEquivalentTo(false);
        Case(artifact, FirstId)["displayName"]!.GetValue<string>().Should().Be(first.DisplayName);
    }

    [Theory]
    [ClassData(typeof(RunnerOutcomeRows))]
    public void ARunnerOutcomeBesideAPassingRow_WhenRun_ShouldBeRecordedAgainstThatRowAlone(TestOutcome outcome, string expected)
    {
        // Given
        var source = new VsTestSource();
        var first = source.Case(FirstId, "A.LocalCases.Theory", "A.LocalCases", "Theory");
        var second = source.Case(SecondId, "A.LocalCases.Theory", "A.LocalCases", "Theory");
        var passing = new VsTestResult(first) { Outcome = TestOutcome.Passed };
        var other = new VsTestResult(second) { Outcome = outcome, ErrorMessage = "second row" };

        // When
        source.Attach().Run(source.Source, null, passing, other);

        // Then
        var artifact = source.Artifact();
        artifact["results"]!["state"]!.GetValue<string>().Should().Be("available");
        Result(artifact, FirstId)["outcome"]!.GetValue<string>().Should().Be("passed");
        Result(artifact, SecondId)["outcome"]!.GetValue<string>().Should().Be(expected);
        Result(artifact, SecondId)["detail"]!.GetValue<string>().Should().Be("second row");
        artifact["results"]!["cases"]!.AsArray().Should().HaveCount(2);
    }

    [Fact]
    public void ANamedCaseWithNoResult_WhenRun_ShouldBeSelectedAndNotRun()
    {
        // Given
        var source = new VsTestSource();
        var first = source.Case(FirstId, "A.LocalCases.Theory", "A.LocalCases", "Theory");
        var second = source.Case(SecondId, "A.LocalCases.Theory", "A.LocalCases", "Theory");

        // When
        source.Attach().Run(source.Source, [first, second], new VsTestResult(first) { Outcome = TestOutcome.Passed });

        // Then
        var artifact = source.Artifact();
        Case(artifact, SecondId)["selected"]!.GetValue<bool>().Should().BeTrue();
        Result(artifact, SecondId)["outcome"]!.GetValue<string>().Should().Be("notRun");
    }

    [Fact]
    public void ARunReportingPartOfTheDiscoveredCases_WhenRunAfterDiscovery_ShouldSelectOnlyTheCasesItReported()
    {
        // Given
        var source = new VsTestSource();
        var first = source.Case(FirstId, "A.LocalCases.Theory", "A.LocalCases", "Theory");
        var second = source.Case(SecondId, "A.LocalCases.Theory", "A.LocalCases", "Theory");
        source.Attach().Discover(source.Source, first, second);

        // When
        source.Attach().Run(source.Source, null, new VsTestResult(first) { Outcome = TestOutcome.Passed });

        // Then
        var artifact = source.Artifact();
        artifact["discovery"]!["state"]!.GetValue<string>().Should().Be("complete");
        Case(artifact, FirstId)["selected"]!.GetValue<bool>().Should().BeTrue();
        Case(artifact, SecondId)["selected"]!.GetValue<bool>().Should().BeFalse();
        artifact["results"]!["cases"]!.AsArray().Select(static item => item!["caseId"]!.GetValue<string>()).Should().Equal(FirstId);
    }

    [Fact]
    public void ABoundMethodWithNoDiscoveredCase_WhenDiscovered_ShouldGetNoSyntheticCase()
    {
        // Given
        var source = new VsTestSource();
        var fact = source.Case(FirstId, "A.LocalCases.Fact", "A.LocalCases", "Fact");

        // When
        source.Attach().Discover(source.Source, fact);

        // Then
        var artifact = source.Artifact();
        artifact["bindings"]!.AsArray().Select(static item => item!["symbolId"]!.GetValue<string>()).Should().Contain("M:A.LocalCases.Undiscovered");
        artifact["discovery"]!["cases"]!.AsArray().Select(static item => item!["methodSymbolId"]!.GetValue<string>()).Should()
            .Equal("M:A.LocalCases.Fact");
    }

    [Fact]
    public void ReqnrollGeneratedCases_WhenRunWithoutBindings_ShouldRecordBothCasesAndTheirOutcomes()
    {
        // Given
        var source = new VsTestSource();
        var first = source.Case(FirstId, "A.Shared.SharedClaimsFeature.TwoRows", "A.Shared.SharedClaimsFeature", "TwoRows",
            ("FeatureTitle", "Shared claims"), ("Description", "Two rows"));
        var second = source.Case(SecondId, "A.Shared.SharedClaimsFeature.TwoRows", "A.Shared.SharedClaimsFeature", "TwoRows",
            ("FeatureTitle", "Shared claims"), ("Description", "Two rows"));

        // When
        source.Attach().Run(source.Source, null, new VsTestResult(first) { Outcome = TestOutcome.Passed },
            new VsTestResult(second) { Outcome = TestOutcome.Failed });

        // Then
        var artifact = source.Artifact();
        artifact["bindings"]!.AsArray().Select(static item => item!["symbolId"]!.GetValue<string>()).Should()
            .NotContain("M:A.Shared.SharedClaimsFeature.TwoRows(System.String,System.String[])");
        Case(artifact, FirstId)["methodSymbolId"]!.GetValue<string>().Should().Be("M:A.Shared.SharedClaimsFeature.TwoRows(System.String,System.String[])");
        Case(artifact, SecondId)["methodSymbolId"]!.GetValue<string>().Should().Be("M:A.Shared.SharedClaimsFeature.TwoRows(System.String,System.String[])");
        Result(artifact, FirstId)["outcome"]!.GetValue<string>().Should().Be("passed");
        Result(artifact, SecondId)["outcome"]!.GetValue<string>().Should().Be("failed");
    }

    [Fact]
    public void CategoryAndOtherTraits_WhenDiscovered_ShouldRecordOnlyCategoryValuesDistinctAndInOrdinalOrder()
    {
        // Given
        var source = new VsTestSource();
        var tagged = source.Case(FirstId, "A.Shared.SharedClaimsFeature.TwoRows", "A.Shared.SharedClaimsFeature", "TwoRows",
            ("Category", "visual"), ("FeatureTitle", "Shared claims"), ("Category", "Smoke"), ("Description", "Two rows"),
            ("Category", "visual"), ("category", "lowercase"));

        // When
        source.Attach().Discover(source.Source, tagged);

        // Then
        Case(source.Artifact(), FirstId)["categories"]!.AsArray().Select(static item => item!.GetValue<string>()).Should()
            .Equal("Smoke", "visual");
    }

    [Theory]
    [ClassData(typeof(OwnershipRows))]
    public void ARequiredCaseScope_WhenDiscovered_ShouldRecordOwnershipFromTheCaseScope(string? scope, string[] categories, bool owned)
    {
        // Given
        var source = new VsTestSource(scope);
        var testCase = source.Case(FirstId, "A.LocalCases.Fact", "A.LocalCases", "Fact",
            categories.Select(static category => ("Category", category)).ToArray());

        // When
        source.Attach().Discover(source.Source, testCase);

        // Then
        Case(source.Artifact(), FirstId)["owned"]!.GetValue<bool>().Should().Be(owned);
    }

    [Theory]
    [ClassData(typeof(ResolvedMethodIdentityRows))]
    public void AUniqueTypeAndMethodName_WhenDiscovered_ShouldResolveTheCaseToThatExportedMethod(
        string fullyQualifiedName, string? managedType, string? managedMethod, string expected)
    {
        // Given
        var source = new VsTestSource();
        var testCase = source.Case(FirstId, fullyQualifiedName, managedType, managedMethod);

        // When
        source.Attach().Discover(source.Source, testCase);

        // Then
        Case(source.Artifact(), FirstId)["methodSymbolId"]!.GetValue<string>().Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(UnresolvedMethodIdentityRows))]
    public void AnAmbiguousUnmatchedOrPartialMethodIdentity_WhenDiscovered_ShouldLeaveTheCaseUnresolved(
        string fullyQualifiedName, string? managedType, string? managedMethod)
    {
        // Given
        var source = new VsTestSource();
        var testCase = source.Case(FirstId, fullyQualifiedName, managedType, managedMethod);

        // When
        source.Attach().Discover(source.Source, testCase);

        // Then
        var artifact = source.Artifact();
        artifact["discovery"]!["cases"]!.AsArray().Should().NotContain(static item => item!["caseId"]!.GetValue<string>() == FirstId);
        artifact["discovery"]!["state"]!.GetValue<string>().Should().NotBe("complete");
    }

    private static JsonNode Case(JsonNode artifact, string caseId) =>
        artifact["discovery"]!["cases"]!.AsArray().Single(item => item!["caseId"]!.GetValue<string>() == caseId)!;

    private static JsonNode Result(JsonNode artifact, string caseId) =>
        artifact["results"]!["cases"]!.AsArray().Single(item => item!["caseId"]!.GetValue<string>() == caseId)!;

    private const string FirstId = "11111111-1111-4111-8111-111111111111";
    private const string SecondId = "22222222-2222-4222-8222-222222222222";
}
