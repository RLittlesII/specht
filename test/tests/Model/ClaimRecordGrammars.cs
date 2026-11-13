namespace Specht.Tests.Model;

/// <summary>
/// The members of a schema 0.4.0 manifest, a Feature's claim prefix or none, the claims it declares and those
/// <c>SPEC4003</c> reports (<c>0001-F5</c> B-077, B-086, B-097, B-098; A-8): a prefixed Feature under the claim record's
/// <c>form</c>, the shipped <c>form</c> with no separator, a Feature without a prefix under the <c>plain</c>, a record
/// filled from the default, a head of literal punctuation, an alternation matched whole, and a head compared with case
/// whatever the tag case setting says.
/// </summary>
public sealed class ClaimRecordGrammars : TheoryData<string, string?, string[], string[]>
{
    /// <summary>Initializes the rows.</summary>
    public ClaimRecordGrammars()
    {
        Add(
            """{ "ids": { "claim": { "form": "{prefix}-{id}", "id": "[0-9a-hjkmnp-tv-z]{6}|[0-9a-hjkmnp-tv-z]{13}" } } }""",
            "ACTIVITY",
            ["ACTIVITY-k3x9ab", "ACTIVITY-0d2kq8v1m3n4p", "ACTIVITY-001", "ACTIVITY001", "ACTIVITY-K3X9AB", "ACTIVITY-k3x9a", "B-k3x9ab"],
            ["ACTIVITY-001", "ACTIVITY001", "ACTIVITY-K3X9AB", "ACTIVITY-k3x9a", "B-k3x9ab"]);
        Add("{}", "CI-AC-", ["CI-AC-001", "CI-AC-k3x9ab", "CI-AC-1"], ["CI-AC-1"]);
        Add("""{ "ids": { "claim": { "id": "[0-9a-hjkmnp-tv-z]{6}" } } }""", null, ["B-k3x9ab", "C-k3x9ab"], ["C-k3x9ab"]);
        Add("""{ "ids": { "claim": { "plain": "R-{id}" } } }""", "CI-AC-", ["CI-AC-k3x9ab", "R-k3x9ab"], ["R-k3x9ab"]);
        Add(
            """{ "ids": { "claim": { "form": "{prefix}-{id}", "plain": "B-{id}", "id": "[0-9]{3}" } } }""",
            null,
            ["B-001", "ACTIVITY-001"],
            ["ACTIVITY-001"]);
        Add("""{ "ids": { "claim": { "form": "{prefix}-{id}" } } }""", "CI.AC", ["CI.AC-k3x9ab", "CIXAC-k3x9ab"], ["CIXAC-k3x9ab"]);
        Add(
            """{ "ids": { "claim": { "form": "{prefix}-{id}", "id": "[0-9]{3}|[a-z]{6}" } } }""",
            "ACTIVITY",
            ["ACTIVITY-001", "ACTIVITY-abcdef", "ACTIVITY-001abcdef", "xACTIVITY-001"],
            ["ACTIVITY-001abcdef", "xACTIVITY-001"]);
        Add(
            """{ "claim": { "tagIgnoreCase": true }, "ids": { "claim": { "form": "{prefix}-{id}" } } }""",
            "ACTIVITY",
            ["ACTIVITY-k3x9ab", "activity-k3x9ab"],
            ["activity-k3x9ab"]);
    }
}
