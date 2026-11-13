using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Tool.Features.Evidence;

namespace Specht.Tests.Evidence;

/// <summary>
/// The claim grammar <c>specht evidence</c> reads from the manifest's <c>codeAnalysis</c> mappings (<c>0201-F5</c> B-045
/// to B-047; <c>0201-F1</c> B-023 to B-025, decision 0006; item <c>0436</c>). The synthetic consumer holds a claim record
/// under <c>idRecords.claim</c> and maps it to <c>/ids/claim</c>, the projection's mirror of the manifest's
/// <c>ids.claim</c>; a manifest that maps the record maps no <c>identifiers.claim</c> unless a row says so. The record's
/// grammar is its <c>plain</c> ahead of <c>{id}</c>, matched as literal text, then a whole match of its <c>id</c>,
/// case-sensitive.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class EvidencePolicyClaimRecordUnitTests
{
    [Theory]
    [ClassData(typeof(EvidenceClaimRecordMatches))]
    public void AClaimUnderAMappedClaimRecord_WhenMatched_ShouldMatchExactlyWhenItIsThePlainHeadThenAWholeMatchOfTheId(
        string plain,
        string id,
        string claim,
        bool matches)
    {
        // Given
        var policy = EvidencePolicy.Read(Configuration(new JsonObject { ["plain"] = plain, ["id"] = id }, false));

        // When
        var match = policy.Matches("claim", claim);

        // Then
        match.Should().Be(matches);
    }

    [Theory]
    [ClassData(typeof(EvidenceOlderClaimGrammarMatches))]
    public void AClaimUnderTheOlderClaimGrammarAndNoClaimRecord_WhenMatched_ShouldMatchExactlyWhenTheOlderGrammarAdmitsIt(string claim, bool matches)
    {
        // Given
        var policy = EvidencePolicy.Read(Configuration(null, true));

        // When
        var match = policy.Matches("claim", claim);

        // Then
        match.Should().Be(matches);
    }

    [Theory]
    [ClassData(typeof(EvidenceUnreadableClaimRecords))]
    public void AMappedClaimRecordTheAnalyzerRejects_WhenProjected_ShouldBeRejected(string record, bool older)
    {
        // Given
        var configuration = Configuration(JsonNode.Parse(record)!.AsObject(), older);

        // When
        var read = () => EvidencePolicy.Read(configuration);

        // Then
        read.Should().Throw<EvidenceConfigurationException>();
    }

    private static JsonObject Configuration(JsonObject? record, bool older)
    {
        var configuration = JsonNode.Parse("""
            {
              "revision":"0.1.0",
              "idPatterns":{"feature":"^cap:[a-z]+$","claim":"^need/[0-9]+$"},
              "claimHeading":"3. Acceptance Criteria",
              "codeAnalysis":{
                "mappings":{"/schemaVersion":"/revision","/identifiers":"/idPatterns",
                  "/claimsSection":"/claimHeading","/policy":"/codeAnalysis"},
                "projects":[{"path":"test/tests.csproj","targetFrameworks":["net10.0"]}],
                "features":[{"feature":"cap:read"}],
                "requiredLinks":{"test":true},
                "exemptions":[]
              }
            }
            """)!.AsObject();
        if (!older)
        {
            configuration["idPatterns"]!.AsObject().Remove("claim");
        }

        if (record is not null)
        {
            configuration["idRecords"] = new JsonObject { ["claim"] = record };
            configuration["codeAnalysis"]!["mappings"]!["/ids/claim"] = "/idRecords/claim";
        }

        return configuration;
    }
}
