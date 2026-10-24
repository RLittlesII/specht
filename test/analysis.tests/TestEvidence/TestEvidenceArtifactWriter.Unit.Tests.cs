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
            ["path"] = "src/feature/.spec/tests.feature",
            ["scenarioId"] = "scenario-1",
            ["line"] = 10,
            ["scope"] = new JsonObject { ["epic"] = "0201", ["feature"] = "F4" },
            ["claims"] = new JsonArray(),
            ["cases"] = new JsonArray(new JsonObject
            {
                ["rowId"] = null,
                ["caseId"] = "case-17",
                ["status"] = "resolved",
            }),
        });
        var artifact = new TestEvidenceArtifactReader().Read(source.ToJsonString()).Artifact!;

        // When
        var output = new TestEvidenceArtifactWriter().Write(artifact);
        var result = new TestEvidenceArtifactReader().Read(output);

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact!.Scenarios.Should().ContainSingle();
        result.Artifact.Scenarios[0].GetProperty("scenarioId").GetString().Should().Be("scenario-1");
        result.Artifact.Scenarios[0].GetProperty("cases")[0].GetProperty("caseId").GetString().Should().Be("case-17");
    }

    [Fact]
    public void VersionThreeArtifact_WhenWrittenAndReadAgain_ShouldPreserveCategoriesOwnershipAndSelection()
    {
        // Given
        var original = new TestEvidenceArtifactReader().Read(TestEvidenceJson.VersionThree).Artifact!;

        // When
        var json = new TestEvidenceArtifactWriter().Write(original);
        var result = new TestEvidenceArtifactReader().Read(json);

        // Then
        JsonNode.Parse(json)!["formatVersion"]!.GetValue<int>().Should().Be(3);
        result.IsValid.Should().BeTrue();
        result.Artifact!.Discovery.Cases.Should().BeEquivalentTo(original.Discovery.Cases);
    }

    [Fact]
    public void CasesStatingNoOwnershipOrSelection_WhenWritten_ShouldStayVersionOne()
    {
        // Given
        var original = new TestEvidenceArtifactReader().Read(TestEvidenceJson.Valid).Artifact!;

        // When
        var json = new TestEvidenceArtifactWriter().Write(original);

        // Then
        var written = JsonNode.Parse(json)!;
        written["formatVersion"]!.GetValue<int>().Should().Be(1);
        written["discovery"]!["cases"]![0]!.AsObject().Select(static member => member.Key).Should()
            .BeEquivalentTo("caseId", "methodSymbolId", "rowId", "displayName");
    }

    [Fact]
    public void NativeCatalog_WhenWritten_ShouldBeVersionTwoCarryingTheCatalogMember()
    {
        // Given
        var artifact = new TestEvidenceArtifactReader().Read(TestEvidenceJson.Valid).Artifact! with { Catalog = TestEvidenceJson.CatalogFeatures };

        // When
        var written = JsonNode.Parse(new TestEvidenceArtifactWriter().Write(artifact))!;

        // Then
        written["formatVersion"]!.GetValue<int>().Should().Be(2);
        JsonNode.DeepEquals(written["catalog"], JsonNode.Parse(TestEvidenceJson.CatalogJson)).Should().BeTrue();
    }

    [Fact]
    public void NativeCatalogWithCaseFacts_WhenWritten_ShouldBeVersionThreeCarryingTheCatalogMember()
    {
        // Given
        var artifact = new TestEvidenceArtifactReader().Read(TestEvidenceJson.VersionThree).Artifact! with { Catalog = TestEvidenceJson.CatalogFeatures };

        // When
        var written = JsonNode.Parse(new TestEvidenceArtifactWriter().Write(artifact))!;

        // Then
        written["formatVersion"]!.GetValue<int>().Should().Be(3);
        JsonNode.DeepEquals(written["catalog"], JsonNode.Parse(TestEvidenceJson.CatalogJson)).Should().BeTrue();
    }

    [Fact]
    public void NativeCatalogArtifact_WhenWrittenAndReadAgain_ShouldReadBackEqual()
    {
        // Given
        var artifact = new TestEvidenceArtifactReader().Read(TestEvidenceJson.Valid).Artifact! with { Catalog = TestEvidenceJson.CatalogFeatures };

        // When
        var result = new TestEvidenceArtifactReader().Read(new TestEvidenceArtifactWriter().Write(artifact));

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact!.Catalog.Should().BeEquivalentTo(TestEvidenceJson.CatalogFeatures, static options => options.WithStrictOrdering());
    }

    [Fact]
    public void ArtifactWithoutACatalog_WhenWritten_ShouldCarryNoCatalogMember()
    {
        // Given
        var artifact = new TestEvidenceArtifactReader().Read(TestEvidenceJson.Valid).Artifact!;

        // When
        var written = JsonNode.Parse(new TestEvidenceArtifactWriter().Write(artifact))!.AsObject();

        // Then
        written["formatVersion"]!.GetValue<int>().Should().Be(1);
        written.ContainsKey("catalog").Should().BeFalse();
    }
}
