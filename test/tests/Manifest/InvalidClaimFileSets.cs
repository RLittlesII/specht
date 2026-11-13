namespace Specht.Tests.Manifest;

/// <summary>
/// The members of a manifest pinned to schema 0.4.0 whose claim file sets are invalid: <c>claim.files</c> absent, empty,
/// or holding an entry with no <c>glob</c> or no <c>feature</c> under <c>gherkin</c>, or beside <c>markdown</c>
/// (<c>0001-F16</c> B-029); and an entry with an <c>epic</c> while the manifest declares no epic grammar (B-027).
/// </summary>
public sealed class InvalidClaimFileSets : TheoryData<string>
{
    /// <summary>Initializes one row for each invalid set the claims state.</summary>
    public InvalidClaimFileSets()
    {
        Add("""{ "claim": { "source": "gherkin" } }""");
        Add("""{ "claim": { "source": "gherkin", "files": [] } }""");
        Add("""{ "claim": { "source": "gherkin", "files": [ { "feature": "arithmetic" } ] } }""");
        Add("""{ "claim": { "source": "gherkin", "files": [ { "glob": ".spec/features/arithmetic/*.feature" } ] } }""");
        Add("""
            { "claim": { "source": "markdown",
              "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "arithmetic" } ] } }
            """);
        Add("""
            { "claim": { "source": "gherkin",
              "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "arithmetic", "epic": "0001" } ] } }
            """);
    }
}
