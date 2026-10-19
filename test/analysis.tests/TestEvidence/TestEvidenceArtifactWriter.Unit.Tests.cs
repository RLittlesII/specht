using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Tests.TestEvidence;

[Trait("Tier", "Unit")]
public sealed class TestEvidenceArtifactWriterUnitTests
{
    [Fact]
    public void ReadArtifact_WhenWrittenAndReadAgain_ShouldPreserveEvidenceIdentities()
    {
        // Given
        var original = new TestEvidenceArtifactReader().Read(TestEvidenceJson.Valid).Artifact!;

        // When
        var json = new TestEvidenceArtifactWriter().Write(original);
        var result = new TestEvidenceArtifactReader().Read(json);

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact!.ProjectPath.Should().Be(original.ProjectPath);
        result.Artifact.Discovery.Methods.Should().BeEquivalentTo(original.Discovery.Methods);
        result.Artifact.Discovery.Cases.Should().BeEquivalentTo(original.Discovery.Cases);
        result.Artifact.Bindings.Single().Target!.Id.Should().Be("B-001");
        result.Artifact.Results.Cases.Single().Outcome.Should().Be("passed");
    }

    [Fact]
    public void IdenticalArtifact_WhenWrittenTwice_ShouldProduceByteIdenticalJson()
    {
        // Given
        var artifact = new TestEvidenceArtifactReader().Read(TestEvidenceJson.Valid).Artifact!;
        var writer = new TestEvidenceArtifactWriter();

        // When
        var first = writer.Write(artifact);
        var second = writer.Write(artifact);

        // Then
        second.Should().Be(first);
    }

    [Fact]
    public void OptionalScenarioRecords_WhenWritten_ShouldBePreserved()
    {
        // Given
        var source = JsonNode.Parse(TestEvidenceJson.Valid)!;
        source["scenarios"] = new JsonArray(new JsonObject
        {
            ["scenarioPath"] = "src/feature/.spec/tests.feature",
            ["generatorId"] = "scenario-1",
        });
        var artifact = new TestEvidenceArtifactReader().Read(source.ToJsonString()).Artifact!;

        // When
        var output = new TestEvidenceArtifactWriter().Write(artifact);
        var result = new TestEvidenceArtifactReader().Read(output);

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact!.Scenarios.Should().ContainSingle();
        result.Artifact.Scenarios[0].GetProperty("generatorId").GetString().Should().Be("scenario-1");
    }
}
