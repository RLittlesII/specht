using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Tests.TestEvidence;

[Trait("Tier", "Unit")]
public sealed class TestEvidenceScenarioReaderUnitTests
{
    [Fact]
    public void ScenarioEvidence_WhenCaseIdIsUnknown_ShouldBeInvalid()
    {
        // Given
        var artifact = JsonNode.Parse(TestEvidenceJson.Valid)!;
        artifact["scenarios"] = new JsonArray(Scenario("scenario-1", 10, "F4", new JsonArray(),
            new JsonArray(CaseLink(null, "missing-case", "resolved"))));

        // When
        var result = new TestEvidenceArtifactReader().Read(artifact.ToJsonString());

        // Then
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ScenarioEvidence_WhenOutlineRowDoesNotMatchDiscoveredCase_ShouldBeInvalid()
    {
        // Given
        var artifact = JsonNode.Parse(TestEvidenceJson.Valid)!;
        artifact["discovery"]!["cases"]![0]!["rowId"] = "row-1";
        artifact["results"] = new JsonObject { ["state"] = "absent" };
        artifact["scenarios"] = new JsonArray(Scenario("scenario-1", 10, "F4", new JsonArray(),
            new JsonArray(CaseLink("row-2", "case-17", "resolved"))));

        // When
        var result = new TestEvidenceArtifactReader().Read(artifact.ToJsonString());

        // Then
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ScenarioEvidence_WhenClaimTargetBelongsToAnotherFeature_ShouldBeInvalid()
    {
        // Given
        var artifact = JsonNode.Parse(TestEvidenceJson.Valid)!;
        artifact["scenarios"] = new JsonArray(Scenario("scenario-1", 10, "F4",
            new JsonArray(Claim("F3")), new JsonArray(CaseLink(null, "case-17", "resolved"))));

        // When
        var result = new TestEvidenceArtifactReader().Read(artifact.ToJsonString());

        // Then
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ScenarioEvidence_WhenResolvedClaimMatchesEffectiveScope_ShouldBeValid()
    {
        // Given
        var artifact = JsonNode.Parse(TestEvidenceJson.Valid)!;
        artifact["scenarios"] = new JsonArray(Scenario("scenario-1", 10, "F4",
            new JsonArray(Claim("F4")), new JsonArray(CaseLink(null, "case-17", "resolved"))));

        // When
        var result = new TestEvidenceArtifactReader().Read(artifact.ToJsonString());

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact!.Scenarios.Should().ContainSingle();
    }

    [Fact]
    public void ScenarioEvidence_WhenTwoOutlinesShareOneScenarioId_ShouldBeInvalid()
    {
        // Given
        var artifact = JsonNode.Parse(TestEvidenceJson.Valid)!;
        artifact["discovery"]!["cases"]!.AsArray().Clear();
        artifact["discovery"]!["cases"]!.AsArray().Add(new JsonObject
        {
            ["caseId"] = "case-18",
            ["methodSymbolId"] = "M:Example.Tests.Sample.Run",
            ["rowId"] = "row-1",
            ["displayName"] = "one",
        });
        artifact["discovery"]!["cases"]!.AsArray().Add(new JsonObject
        {
            ["caseId"] = "case-19",
            ["methodSymbolId"] = "M:Example.Tests.Sample.Run",
            ["rowId"] = "row-2",
            ["displayName"] = "two",
        });
        artifact["results"] = new JsonObject { ["state"] = "absent" };
        artifact["scenarios"] = new JsonArray(
            Scenario("scenario-1", 10, "F4", new JsonArray(), new JsonArray(CaseLink("row-1", "case-18", "resolved"))),
            Scenario("scenario-1", 20, "F4", new JsonArray(), new JsonArray(CaseLink("row-2", "case-19", "resolved"))));

        // When
        var result = new TestEvidenceArtifactReader().Read(artifact.ToJsonString());

        // Then
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ScenarioEvidence_WhenProducerCannotResolveCase_ShouldRetainUnresolvedLinkAndDirectBinding()
    {
        // Given
        var artifact = JsonNode.Parse(TestEvidenceJson.Valid)!;
        artifact["scenarios"] = new JsonArray(Scenario(null, 10, "F4", new JsonArray(),
            new JsonArray(CaseLink(null, null, "unresolved"))));

        // When
        var result = new TestEvidenceArtifactReader().Read(artifact.ToJsonString());

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact!.Bindings.Should().ContainSingle();
        var scenario = result.Artifact.Scenarios.Should().ContainSingle().Which;
        scenario.GetProperty("scenarioId").ValueKind.ToString().Should().Be("Null");
        scenario.GetProperty("cases")[0].GetProperty("status").GetString().Should().Be("unresolved");
    }

    [Fact]
    public void ScenarioEvidence_WhenTwoScenariosShareOneStepMethod_ShouldKeepScenarioLinksSeparate()
    {
        // Given
        var artifact = JsonNode.Parse(TestEvidenceJson.Valid)!;
        artifact["discovery"]!["cases"]!.AsArray().Add(new JsonObject
        {
            ["caseId"] = "case-18",
            ["methodSymbolId"] = "M:Example.Tests.Sample.Run",
            ["rowId"] = null,
            ["displayName"] = "same step method",
        });
        artifact["results"] = new JsonObject { ["state"] = "absent" };
        artifact["scenarios"] = new JsonArray(
            Scenario("scenario-1", 10, "F4", new JsonArray(), new JsonArray(CaseLink(null, "case-17", "resolved"))),
            Scenario("scenario-2", 20, "F4", new JsonArray(), new JsonArray(CaseLink(null, "case-18", "resolved"))));

        // When
        var result = new TestEvidenceArtifactReader().Read(artifact.ToJsonString());

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact!.Scenarios.Should().HaveCount(2);
        result.Artifact.Scenarios.Select(static scenario => scenario.GetProperty("scenarioId").GetString())
            .Should().ContainInOrder("scenario-1", "scenario-2");
        result.Artifact.Discovery.Cases.Select(static item => item.MethodSymbolId).Distinct().Should().ContainSingle();
    }

    [Fact]
    public void ScenarioEvidence_WhenBridgeIsOmitted_ShouldPreserveDirectBindings()
    {
        // Given
        var artifact = JsonNode.Parse(TestEvidenceJson.Valid)!;

        // When
        var result = new TestEvidenceArtifactReader().Read(artifact.ToJsonString());

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact!.Scenarios.Should().BeEmpty();
        result.Artifact.Bindings.Should().ContainSingle();
    }

    private static JsonObject Scenario(string? scenarioId, int line, string feature, JsonArray claims, JsonArray cases) => new()
    {
        ["path"] = "tests/scenarios.feature",
        ["scenarioId"] = scenarioId,
        ["line"] = line,
        ["scope"] = new JsonObject { ["epic"] = "0201", ["feature"] = feature },
        ["claims"] = claims,
        ["cases"] = cases,
    };

    private static JsonObject CaseLink(string? rowId, string? caseId, string status) => new()
    {
        ["rowId"] = rowId,
        ["caseId"] = caseId,
        ["status"] = status,
    };

    private static JsonObject Claim(string feature) => new()
    {
        ["id"] = "B-001",
        ["status"] = "resolved",
        ["isEligible"] = true,
        ["target"] = new JsonObject
        {
            ["kind"] = "claim",
            ["epic"] = "0201",
            ["feature"] = feature,
            ["id"] = "B-001",
            ["state"] = "Active",
            ["featureState"] = "approved",
            ["relativePath"] = "src/feature/.spec/README.md",
            ["line"] = 1,
        },
        ["matches"] = new JsonArray(),
    };
}
