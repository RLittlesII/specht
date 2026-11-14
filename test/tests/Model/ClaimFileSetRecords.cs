namespace Specht.Tests.Model;

/// <summary>
/// The manifest members and record text of one claim file set, with the lifecycle, dependency edges and record path the
/// native Feature it makes should carry (<c>0001-F16</c> B-003; A-3, C-4): a record that sets <c>spec_status</c> and
/// <c>depends_on</c>; no record, current with no edge; and a manifest whose <c>claim.lifecycleKey</c> names another key.
/// </summary>
public sealed class ClaimFileSetRecords : TheoryData<string, string?, string, string[], string?>
{
    /// <summary>Initializes one row for a record, one for none and one for a lifecycle key the manifest names.</summary>
    public ClaimFileSetRecords()
    {
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] } }
            """,
            "---\nid: F1\nspec_status: approved\ndepends_on: [F2]\n---\n\n# Arithmetic\n",
            "approved",
            ["F2"],
            ".spec/features/arithmetic/README.md");
        Add("{}", null, "current", [], null);
        Add(
            """
            { "claim": { "lifecycleKey": "maturity", "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] } }
            """,
            "---\nid: F1\nspec_status: approved\nmaturity: draft\n---\n\n# Arithmetic\n",
            "draft",
            [],
            ".spec/features/arithmetic/README.md");
    }
}
