using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using AwesomeAssertions;
using Reqnroll;
using Specht.Analysis.Bindings;
using Specht.Analysis.Policy;

namespace Specht.Acceptance.Bindings;

/// <summary>
/// Steps for the claim grammar the analysis reads from the manifest's <c>ids</c> claim record (<c>0201-F1</c> B-023 to
/// B-025; decision 0006; item <c>0436</c>). A synthetic consumer manifest projects its <c>ids.claim</c> record to the
/// analysis policy through <c>codeAnalysis.mappings</c> as <c>/ids/claim</c>, beside or without the older grammar under
/// <c>identifiers.claim</c>, and one Feature record declares the first claim ID. "The declared analysis is requested"
/// reads those inputs as the analyzer does; a claim ID is valid when a reference to it is not invalid under the policy
/// that reading gives, and the policy is rejected as invalid when the reading leaves no analysis available.
/// </summary>
public sealed partial class BindingSteps
{
    [Given("analysis policy that supplies the manifest's ids claim record and no older claim grammar")]
    public void GivenAnalysisPolicyThatSuppliesTheIdsClaimRecord() =>
        _claimRecordPolicy = ClaimRecordPolicy(new JsonObject { ["plain"] = "need/{id}", ["id"] = "[0-9]+" }, false);

    [Given("a claim ID that is the record's plain text ahead of its id followed by a whole match of its id")]
    public void GivenAClaimIdThatIsThePlainHeadThenTheId() => _claimRecordIds.Add("need/7");

    [Given("a claim ID with the same id under different leading text")]
    public void GivenAClaimIdWithTheSameIdUnderDifferentLeadingText() => _claimRecordIds.Add("want/7");

    [Given("analysis policy that supplies the older claim grammar and no ids claim record")]
    public void GivenAnalysisPolicyThatSuppliesTheOlderClaimGrammar()
    {
        _claimRecordPolicy = ClaimRecordPolicy(null, true);
        _claimRecordIds.AddRange(["need/7", "want/7"]);
    }

    [Given("^analysis policy that supplies an ids claim record that (.+)$")]
    public void GivenAnalysisPolicyThatSuppliesAnIdsClaimRecordThat(string defect) =>
        _claimRecordPolicy = defect switch
        {
            "sits beside the older claim grammar" => ClaimRecordPolicy(new JsonObject { ["plain"] = "need/{id}", ["id"] = "[0-9]+" }, true),
            "has no plain text" => ClaimRecordPolicy(new JsonObject { ["id"] = "[0-9]+" }, false),
            "has no id" => ClaimRecordPolicy(new JsonObject { ["plain"] = "need/{id}" }, false),
            "has plain text that does not end with its only id" =>
                ClaimRecordPolicy(new JsonObject { ["plain"] = "need/{id}-x", ["id"] = "[0-9]+" }, false),
            _ => ClaimRecordPolicy(new JsonObject { ["plain"] = "need/{id}", ["id"] = "[0-9" }, false),
        };

    [Then("the first claim ID is valid")]
    public void ThenTheFirstClaimIdIsValid() => ClaimRecordResolution(_claimRecordIds[0]).Should().Be(BindingResolution.Resolved);

    [Then("the second claim ID is invalid")]
    public void ThenTheSecondClaimIdIsInvalid() => ClaimRecordResolution(_claimRecordIds[1]).Should().Be(BindingResolution.Invalid);

    [Then("claim IDs and claim references are validated by the older claim grammar")]
    public void ThenClaimIdsAndClaimReferencesAreValidatedByTheOlderClaimGrammar()
    {
        ThenTheFirstClaimIdIsValid();
        ThenTheSecondClaimIdIsInvalid();
    }

    [Then("the findings are those the analysis gave before the ids claim record was read")]
    public void ThenTheFindingsAreThoseTheAnalysisGaveBefore() => _claimRecordSnapshot!.Catalog!.Findings.Should().BeEmpty();

    [Then("the policy is rejected as invalid")]
    public void ThenThePolicyIsRejectedAsInvalid()
    {
        _claimRecordSnapshot!.Available.Should().BeFalse();
        _claimRecordSnapshot.Failures.Should().NotBeEmpty();
    }

    private static List<AnalysisInput> ClaimRecordPolicy(JsonObject? record, bool older)
    {
        var identifiers = new JsonObject { ["feature"] = "^cap:[a-z]+$" };
        var mappings = new JsonObject
        {
            ["/schemaVersion"] = "/schemaVersion",
            ["/identifiers"] = "/identifiers",
            ["/markers"] = "/markers",
            ["/claimsSection"] = "/claimsSection",
            ["/policy"] = "/codeAnalysis",
        };
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.4.0",
            ["identifiers"] = identifiers,
            ["claimsSection"] = "3. Acceptance Criteria",
            ["markers"] = new JsonObject { ["approved"] = "approved", ["withdrawn"] = "Withdrawn", ["superseded"] = "Superseded" },
        };
        if (older)
        {
            identifiers["claim"] = "^need/[0-9]+$";
        }

        if (record is not null)
        {
            manifest["ids"] = new JsonObject { ["claim"] = record };
            mappings["/ids/claim"] = "/ids/claim";
        }

        manifest["codeAnalysis"] = new JsonObject
        {
            ["mappings"] = mappings,
            ["projects"] = new JsonArray(new JsonObject { ["path"] = "test/tests.csproj", ["targetFrameworks"] = new JsonArray("net10.0") }),
            ["features"] = new JsonArray(new JsonObject { ["feature"] = "cap:read" }),
            ["enforcement"] = new JsonArray(),
        };
        return
        [
            new("configuration", ".specht/manifest.json", manifest.ToJsonString()),
            new("schema:feature", "contracts/feature.json",
                """{"type":"object","required":["id","spec_status"],"properties":{"id":{"type":"string"},"spec_status":{"type":"string"}}}"""),
            new("feature", "src/read/.spec/README.md",
                "---\nid: cap:read\nspec_status: draft\n---\n## 3. Acceptance Criteria\n| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n"
                + "| need/7 | A synthetic need | Agreement | Active |\n"),
        ];
    }

    private BindingResolution ClaimRecordResolution(string claim)
    {
        var snapshot = _claimRecordSnapshot!;
        snapshot.Available.Should().BeTrue(
            "the policy supplies a claim grammar the analysis reads, not {0}",
            string.Join("; ", snapshot.Failures.Select(static failure => failure.RelativePath + ": " + failure.Message)));
        return new BindingResolver(snapshot.Catalog!, snapshot.Policy!.Bindings).Resolve(new ClaimReference(null, "cap:read", claim)).Status;
    }

    private void WhenTheClaimRecordPolicyIsRead() =>
        _claimRecordSnapshot = new AnalysisInputReader().Read(_claimRecordPolicy!, CancellationToken.None);

    private readonly List<string> _claimRecordIds = [];
    private List<AnalysisInput>? _claimRecordPolicy;
    private AnalysisSnapshot? _claimRecordSnapshot;
}
