using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Tests.TestEvidence;

[Trait("Tier", "Unit")]
public sealed class TestEvidenceArtifactReaderUnitTests
{
    [Fact]
    public void VersionOneArtifact_WhenRead_ShouldExposeRunnerCaseMethodAndRunIdentities()
    {
        // Given
        var json = TestEvidenceJson.Valid;

        // When
        var result = new TestEvidenceArtifactReader().Read(json);

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact.Should().NotBeNull();
        result.Artifact!.ProjectPath.Should().Be("test/Example.Tests.csproj");
        result.Artifact.TargetFramework.Should().Be("net10.0");
        result.Artifact.AdapterId.Should().Be("custom.runner");
        result.Artifact.AdapterVersion.Should().Be("1.2.3");
        result.Artifact.Discovery.Methods.Should().ContainSingle().Which.SymbolId.Should().Be("M:Example.Tests.Sample.Run");
        result.Artifact.Discovery.Cases.Should().ContainSingle().Which.CaseId.Should().Be("case-17");
        result.Artifact.Results.RunId.Should().Be("run-2026-01");
    }

    [Theory]
    [ClassData(typeof(InvalidTestEvidenceDocuments))]
    public void UnsupportedOrMalformedArtifact_WhenRead_ShouldReturnContractFailure(string json, string failure)
    {
        // Given

        // When
        var result = new TestEvidenceArtifactReader().Read(json);

        // Then
        result.IsValid.Should().BeFalse();
        result.Failure.Should().Be(failure);
        result.Artifact.Should().BeNull();
    }

    [Fact]
    public void DuplicateDisplayNames_WhenRead_ShouldPreserveDistinctRunnerCaseIdentities()
    {
        // Given
        var json = TestEvidenceJson.WithDuplicateDisplayNames();

        // When
        var result = new TestEvidenceArtifactReader().Read(json);

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact!.Discovery.Cases.Select(static item => item.CaseId).Should().Equal("case-17", "case-18");
        result.Artifact.Discovery.Cases.Select(static item => item.DisplayName).Distinct().Should().ContainSingle();
    }

    [Theory]
    [ClassData(typeof(TestEvidenceFreshnessCases))]
    public void ArtifactIdentity_WhenComparedWithCurrentInputs_ShouldReportFreshness(
        string projectPath, string targetFramework, string sourceFingerprint, string assemblySha256,
        string configurationFingerprint, string policyFingerprint, bool current)
    {
        // Given
        var expected = new TestEvidenceAnalysisInputs(projectPath, targetFramework, sourceFingerprint,
            assemblySha256, configurationFingerprint, policyFingerprint);

        // When
        var result = new TestEvidenceArtifactReader().Read(TestEvidenceJson.Valid, expected);

        // Then
        result.IsValid.Should().BeTrue();
        result.IsCurrent.Should().Be(current);
    }

    [Fact]
    public void DeferredDiscovery_WhenRead_ShouldRemainIncompleteWithoutInventedCases()
    {
        // Given
        var json = TestEvidenceJson.DeferredWithoutCases();

        // When
        var result = new TestEvidenceArtifactReader().Read(json);

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact!.Discovery.State.Should().Be(TestEvidenceDiscoveryState.Deferred);
        result.Artifact.Discovery.Cases.Should().BeEmpty();
    }

    [Fact]
    public void IdenticalResultRecords_WhenRead_ShouldCountOneExecution()
    {
        // Given
        var artifact = JsonNode.Parse(TestEvidenceJson.Valid)!;
        artifact["results"]!["cases"]!.AsArray().Add(new JsonObject
        {
            ["caseId"] = "case-17",
            ["outcome"] = "passed",
            ["detail"] = null,
        });

        // When
        var result = new TestEvidenceArtifactReader().Read(artifact.ToJsonString());

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact!.Results.Cases.Should().ContainSingle();
    }
}
