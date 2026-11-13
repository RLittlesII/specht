using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.Bindings;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Tests.Policy;

/// <summary>
/// The claim grammar the analysis reads from its policy projection (<c>0201-F1</c> B-023 to B-025; decision 0006; item
/// <c>0436</c>). The synthetic consumer's configuration holds a claim record under <c>idRecords.claim</c>, and its
/// mappings project it to <c>/ids/claim</c>, the projection's mirror of the manifest's <c>ids.claim</c>; a projection
/// that supplies the record carries no <c>identifiers.claim</c> unless a row says so. The record's grammar is its
/// <c>plain</c> ahead of <c>{id}</c>, matched as literal text, then a whole match of its <c>id</c>, case-sensitive.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ClaimRecordProjectionUnitTests
{
    [Theory]
    [ClassData(typeof(ClaimRecordDeclarations))]
    public void AFeatureClaimUnderAProjectedClaimRecord_WhenRead_ShouldBeAvailableExactlyWhenItIsThePlainHeadThenAWholeMatchOfTheId(
        string plain,
        string id,
        string claim,
        bool available)
    {
        // Given
        var inputs = Inputs(new JsonObject { ["plain"] = plain, ["id"] = id }, false, claim);

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().Be(available);
    }

    [Theory]
    [ClassData(typeof(ClaimRecordReferences))]
    public void AClaimReferenceUnderAProjectedClaimRecord_WhenResolved_ShouldBeInvalidExactlyWhenItIsNotThePlainHeadThenAWholeMatchOfTheId(
        string claim,
        BindingResolution status)
    {
        // Given
        var snapshot = new AnalysisInputReader().Read(
            Inputs(new JsonObject { ["plain"] = "need/{id}", ["id"] = "[0-9]+" }, false, "need/7"),
            TestContext.Current.CancellationToken);

        // When
        BindingResolution? resolution = snapshot.Available
            ? new BindingResolver(snapshot.Catalog!, snapshot.Policy!.Bindings).Resolve(new ClaimReference(null, "cap:read", claim)).Status
            : null;

        // Then
        resolution.Should().Be(status);
    }

    [Theory]
    [ClassData(typeof(OlderClaimGrammarClaims))]
    public void AFeatureClaimUnderTheOlderClaimGrammarAndNoClaimRecord_WhenRead_ShouldBeAvailableExactlyWhenTheOlderGrammarAdmitsIt(
        string claim,
        bool available)
    {
        // Given
        var inputs = Inputs(null, true, claim);

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().Be(available);
    }

    [Theory]
    [ClassData(typeof(UnreadableClaimRecords))]
    public void AProjectedClaimRecordTheAnalysisCannotReadAlone_WhenRead_ShouldLeaveTheAnalysisUnavailable(string record, bool older)
    {
        // Given
        var inputs = Inputs(JsonNode.Parse(record)!.AsObject(), older, "need/7");

        // When
        var snapshot = new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

        // Then
        snapshot.Available.Should().BeFalse();
        snapshot.Failures.Should().NotBeEmpty();
    }

    private static List<AnalysisInput> Inputs(JsonObject? record, bool older, string claim)
    {
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!.AsObject();
        if (!older)
        {
            configuration["idPatterns"]!.AsObject().Remove("claim");
        }

        var mappings = JsonNode.Parse(inputs[1].Text!)!.AsObject();
        if (record is not null)
        {
            configuration["idRecords"] = new JsonObject { ["claim"] = record };
            mappings["/ids/claim"] = "/idRecords/claim";
        }

        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };
        inputs[1] = inputs[1] with { Text = mappings.ToJsonString() };
        inputs[3] = inputs[3] with { Text = inputs[3].Text!.Replace("| need/7 |", $"| {claim} |", StringComparison.Ordinal) };
        return inputs;
    }
}
