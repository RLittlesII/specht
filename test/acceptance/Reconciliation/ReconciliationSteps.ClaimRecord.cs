using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Reconciliation;

/// <summary>
/// The claim grammar <c>specht evidence</c> reads from the consumer's mapped ids claim record (<c>0201-F5</c> B-045 to
/// B-047; item <c>0436</c>). The synthetic consumer keeps its record under <c>idRecords.claim</c> and maps it to the
/// policy's <c>/ids/claim</c> through <c>codeAnalysis.mappings</c>, as it maps its older grammar from
/// <c>idPatterns</c> to <c>/identifiers</c>; "no older claim grammar" removes <c>idPatterns.claim</c>. The record's
/// plain text is <c>need/{id}</c> and its id <c>[0-9]+</c>, so <c>need/7</c> is its head then a whole match of its id and
/// <c>want/7</c> the same id under different leading text. A claim is reconciled when the report lists it; it violates
/// its grammar when the report holds an invalid-input finding naming it in the Feature record and lists no claim.
/// </summary>
public sealed partial class ReconciliationSteps
{
    [Given("a consumer whose manifest maps its ids claim record and no older claim grammar")]
    public void GivenAConsumerWhoseManifestMapsItsIdsClaimRecord() =>
        MapClaimRecord(new JsonObject { ["plain"] = "need/{id}", ["id"] = "[0-9]+" }, false);

    [Given("^a selected Feature whose current claim ID is (the record's plain text ahead of its id, then a whole match of its id|"
        + "a whole match of its id under different leading text)$")]
    public void GivenASelectedFeatureWhoseCurrentClaimIdIs(string claim)
    {
        _claimRecordId = claim.StartsWith("the record's", StringComparison.Ordinal) ? "need/7" : "want/7";
        var record = Path.Combine(_tree.Root, "src", "read", ".spec", "README.md");
        File.WriteAllText(record, File.ReadAllText(record).Replace("| need/7 |", $"| {_claimRecordId} |", StringComparison.Ordinal));
    }

    [Given("a consumer whose manifest maps the older claim grammar and no ids claim record")]
    public void GivenAConsumerWhoseManifestMapsTheOlderClaimGrammar() => MapClaimRecord(null, true);

    [Given("^a consumer whose manifest maps an ids claim record that (.+)$")]
    public void GivenAConsumerWhoseManifestMapsAnIdsClaimRecordThat(string defect) =>
        MapClaimRecord(
            defect switch
            {
                "has no plain text" => new JsonObject { ["id"] = "[0-9]+" },
                "has no id" => new JsonObject { ["plain"] = "need/{id}" },
                "has plain text that does not end with its only id" => new JsonObject { ["plain"] = "need/{id}-x", ["id"] = "[0-9]+" },
                "has an id that is not a usable pattern" => new JsonObject { ["plain"] = "need/{id}", ["id"] = "[0-9" },
                _ => new JsonObject { ["plain"] = "need/{id}", ["id"] = "[0-9]+" },
            },
            defect == "sits beside the older claim grammar");

    [Then("the claim is reconciled")]
    public void ThenTheClaimIsReconciled()
    {
        _result.ExitCode.Should().Be(0, _result.Stdout + _result.Stderr);
        ReconciledClaims().Should().Equal(_claimRecordId);
    }

    [Then("the claim is reported as violating its grammar, not reconciled")]
    public void ThenTheClaimIsReportedAsViolatingItsGrammar()
    {
        Report()["findings"]!.AsArray().Should().Contain(item => item!["kind"]!.GetValue<string>() == "invalidInput"
            && item["path"]!.GetValue<string>() == "src/read/.spec/README.md"
            && item["identity"]!.GetValue<string>() == _claimRecordId);
        ReconciledClaims().Should().NotContain(_claimRecordId);
    }

    [Then("claim IDs are read and validated by the older claim grammar")]
    public void ThenClaimIdsAreReadAndValidatedByTheOlderClaimGrammar() => ReconciledClaims().Should().Equal("need/7");

    [Then("the report is the one specht evidence gave before the ids claim record was read")]
    public void ThenTheReportIsTheOneSpechtEvidenceGaveBefore()
    {
        _result.ExitCode.Should().Be(0, _result.Stdout + _result.Stderr);
        Kinds().Should().BeEmpty();
    }

    private string[] ReconciledClaims() => Report()["claims"]!.AsArray().Select(static claim => claim!["id"]!.GetValue<string>()).ToArray();

    private void MapClaimRecord(JsonObject? record, bool older) =>
        _tree.EditManifest(manifest =>
        {
            if (!older)
            {
                manifest["idPatterns"]!.AsObject().Remove("claim");
            }

            if (record is not null)
            {
                manifest["idRecords"] = new JsonObject { ["claim"] = record };
                manifest["codeAnalysis"]!["mappings"]!["/ids/claim"] = "/idRecords/claim";
            }
        });

    private string _claimRecordId = "need/7";
}
