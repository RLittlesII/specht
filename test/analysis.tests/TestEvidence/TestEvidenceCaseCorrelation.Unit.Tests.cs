using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Tests.TestEvidence;

[Trait("Tier", "Unit")]
public sealed class TestEvidenceCaseCorrelationUnitTests
{
    [Fact]
    public void DiscoveryAndResultsWithDifferentSessionIds_WhenCorrelated_ShouldMatchStableCaseIds()
    {
        // Given
        var discovery = new TestEvidenceArtifactReader().Read(TestEvidenceJson.WithDuplicateDisplayNames()).Artifact!;
        var resultNode = JsonNode.Parse(TestEvidenceJson.WithDuplicateDisplayNames())!;
        resultNode["discovery"]!["sessionId"] = "a-later-discovery-session";
        resultNode["results"]!["runId"] = "a-later-execution-run";
        resultNode["results"]!["cases"] = new JsonArray
        {
            new JsonObject { ["caseId"] = "case-17", ["outcome"] = "passed", ["detail"] = null },
            new JsonObject { ["caseId"] = "case-18", ["outcome"] = "failed", ["detail"] = "assertion" },
        };
        var execution = new TestEvidenceArtifactReader().Read(resultNode.ToJsonString()).Artifact!;

        // When
        var result = new TestEvidenceCaseCorrelation().Correlate(discovery, execution);

        // Then
        result.IsCurrent.Should().BeTrue();
        result.Matches.Select(static match => match.Case.CaseId).Should().Equal("case-17", "case-18");
        result.Matches.Select(static match => match.Outcome.Outcome).Should().Equal("passed", "failed");
    }

    [Fact]
    public void ResultsWithChangedAssemblyIdentity_WhenCorrelated_ShouldBeExcludedAsStale()
    {
        // Given
        var discovery = new TestEvidenceArtifactReader().Read(TestEvidenceJson.Valid).Artifact!;
        var executionNode = JsonNode.Parse(TestEvidenceJson.Valid)!;
        executionNode["assembly"]!["sha256"] = new string('f', 64);
        var execution = new TestEvidenceArtifactReader().Read(executionNode.ToJsonString()).Artifact!;

        // When
        var result = new TestEvidenceCaseCorrelation().Correlate(discovery, execution);

        // Then
        result.IsCurrent.Should().BeFalse();
        result.Matches.Should().BeEmpty();
    }

    [Fact]
    public void ResultsAbsent_WhenCorrelated_ShouldProduceNoPassingMatches()
    {
        // Given
        var discovery = new TestEvidenceArtifactReader().Read(TestEvidenceJson.Valid).Artifact!;
        var executionNode = JsonNode.Parse(TestEvidenceJson.Valid)!;
        executionNode["results"] = new JsonObject { ["state"] = "absent" };
        var execution = new TestEvidenceArtifactReader().Read(executionNode.ToJsonString()).Artifact!;

        // When
        var result = new TestEvidenceCaseCorrelation().Correlate(discovery, execution);

        // Then
        result.IsCurrent.Should().BeTrue();
        result.Matches.Should().BeEmpty();
    }

    [Fact]
    public void DeferredCasesSuppliedByRunnerExecution_WhenCorrelated_ShouldUseRunnerCases()
    {
        // Given
        var discovery = new TestEvidenceArtifactReader().Read(TestEvidenceJson.DeferredWithoutCases()).Artifact!;
        var execution = new TestEvidenceArtifactReader().Read(TestEvidenceJson.Valid).Artifact!;

        // When
        var result = new TestEvidenceCaseCorrelation().Correlate(discovery, execution);

        // Then
        result.IsCurrent.Should().BeTrue();
        result.Matches.Should().ContainSingle().Which.Case.CaseId.Should().Be("case-17");
    }
}
