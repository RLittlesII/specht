namespace Specht.Tests.Model;

/// <summary>
/// The members of a schema 0.4.0 manifest that selects Gherkin claims, the tags on one scenario, and the claims those
/// tags declare: a tag outside the claim tag form, or whose captured text fails the claim grammar or begins with a longer
/// scenario head, declares nothing (<c>0001-F16</c> B-005; C-1; <c>0001-F5</c> B-095).
/// </summary>
public sealed class OrdinaryClaimTags : TheoryData<string, string, string[]>
{
    /// <summary>Initializes one row for each way a tag is ordinary.</summary>
    public OrdinaryClaimTags()
    {
        Add("{}", "@B-001 @smoke @B-01", ["B-001"]);
        Add("{}", "@B-001 @b-002", ["B-001"]);
        Add("""{ "claim": { "tag": "@claim:{claim}" } }""", "@claim:B-001 @B-002", ["B-001"]);
        Add(
            """{ "ids": { "claim": { "plain": "AC-{id}", "id": ".+" }, "scenario": { "plain": "AC-SCENARIO-{id}" } } }""",
            "@AC-001 @AC-SCENARIO-m7p2qr",
            ["AC-001"]);
        Add(
            """
            { "claim": { "tagIgnoreCase": true },
              "ids": { "claim": { "plain": "AC-{id}", "id": ".+" }, "scenario": { "plain": "AC-SCENARIO-{id}" } } }
            """,
            "@AC-001 @ac-scenario-m7p2qr",
            ["AC-001"]);
    }
}
