namespace Specht.Tests.Manifest;

/// <summary>
/// The members of a manifest pinned to schema 0.4.0 that select Gherkin claims through one claim file set, with and
/// without an epic grammar, and the epic and Feature of the identity the set names (<c>0001-F16</c> B-027).
/// </summary>
public sealed class ClaimFileSetIdentities : TheoryData<string, string?, string>
{
    /// <summary>Initializes one row under an epic grammar and one under none.</summary>
    public ClaimFileSetIdentities()
    {
        Add("""
            { "identifiers": { "epic": "^[0-9]{4}$" }, "claim": { "source": "gherkin",
              "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "arithmetic", "epic": "0001" } ] } }
            """, "0001", "arithmetic");
        Add("""
            { "claim": { "source": "gherkin",
              "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "arithmetic" } ] } }
            """, null, "arithmetic");
    }
}
