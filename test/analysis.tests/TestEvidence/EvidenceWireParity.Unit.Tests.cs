using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.Policy;
using Specht.Analysis.TestEvidence;
using Specht.Analysis.Tests.Policy;
using CliConfigurationReader = Specht.Tool.Features.Evidence.Contract.AnalysisConfigurationReader;
using CliEvidenceReader = Specht.Tool.Features.Evidence.Contract.TestEvidenceArtifactReader;

namespace Specht.Analysis.Tests.TestEvidence;

/// <summary>Guards the CLI's independent wire reader and mapping projection against the producer contracts.</summary>
[Trait("Tier", "Unit")]
public sealed class EvidenceWireParityUnitTests
{
    [Fact]
    public void ProducerArtifact_WhenReadByCli_ShouldRetainTheSameVersionOneFields()
    {
        // Given
        var producer = new TestEvidenceArtifactReader().Read(TestEvidenceJson.Valid).Artifact!;
        var emitted = new TestEvidenceArtifactWriter().Write(producer);

        // When
        var cli = new CliEvidenceReader().Read(emitted);

        // Then
        cli.IsValid.Should().BeTrue();
        JsonNode.DeepEquals(JsonSerializer.SerializeToNode(producer), JsonSerializer.SerializeToNode(cli.Artifact)).Should().BeTrue();
    }

    [Theory]
    [ClassData(typeof(InvalidTestEvidenceDocuments))]
    public void InvalidProducerDocument_WhenReadByCli_ShouldGiveTheSameContractFailure(string json, string failure)
    {
        // Given
        var producer = new TestEvidenceArtifactReader().Read(json);

        // When
        var cli = new CliEvidenceReader().Read(json);

        // Then
        producer.Failure.Should().Be(failure);
        cli.Failure.Should().Be(producer.Failure);
    }

    [Fact]
    public void ConsumerMappings_WhenProjectedByCli_ShouldMatchTheAnalyzerProjection()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs.Single(static item => item.Kind == "configuration").Text!)!;
        var mappings = JsonNode.Parse(inputs.Single(static item => item.Kind == "mappings").Text!)!.AsObject()
            .ToDictionary(static entry => entry.Key, static entry => entry.Value!.GetValue<string>(), StringComparer.Ordinal);

        // When
        var analyzer = new AnalysisConfigurationReader().Read(configuration, mappings);
        var cli = new CliConfigurationReader().Read(configuration, mappings);

        // Then
        JsonNode.DeepEquals(cli, analyzer).Should().BeTrue();
    }

    [Fact]
    public void ProducerVersionThreeArtifact_WhenReadByCli_ShouldRetainTheSameCaseFacts()
    {
        // Given
        var producer = new TestEvidenceArtifactReader().Read(TestEvidenceJson.VersionThree).Artifact!;
        var emitted = new TestEvidenceArtifactWriter().Write(producer);

        // When
        var cli = new CliEvidenceReader().Read(emitted);

        // Then
        cli.IsValid.Should().BeTrue();
        JsonNode.DeepEquals(JsonSerializer.SerializeToNode(producer), JsonSerializer.SerializeToNode(cli.Artifact)).Should().BeTrue();
    }
}
