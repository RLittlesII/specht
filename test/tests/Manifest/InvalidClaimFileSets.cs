namespace Specht.Tests.Manifest;

/// <summary>
/// The members of a manifest pinned to schema 0.4.0 whose claim file sets are invalid: <c>claim.files</c> absent, empty,
/// or holding an entry with no <c>glob</c> or no <c>feature</c> under <c>gherkin</c>, or beside <c>markdown</c> or no
/// <c>claim.source</c> (<c>0001-F16</c> B-029); an entry with an <c>epic</c> while the manifest declares no epic
/// grammar, or without one while it declares one (B-027); and an entry whose <c>feature</c> the feature grammar, or whose
/// <c>epic</c> the epic grammar, does not match (B-038).
/// </summary>
public sealed class InvalidClaimFileSets : TheoryData<string>
{
    /// <summary>Initializes one row for each invalid set the claims state.</summary>
    public InvalidClaimFileSets()
    {
        Add("""{ "claim": { "source": "gherkin" } }""");
        Add("""{ "claim": { "source": "gherkin", "files": [] } }""");
        Add("""{ "claim": { "source": "gherkin", "files": [ { "feature": "F1" } ] } }""");
        Add("""{ "claim": { "source": "gherkin", "files": [ { "glob": ".spec/features/arithmetic/*.feature" } ] } }""");
        Add("""
            { "claim": { "source": "markdown",
              "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" } ] } }
            """);
        Add("""
            { "claim": { "source": "gherkin",
              "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "epic": "0001" } ] } }
            """);
        Add("""
            { "claim": { "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" } ] } }
            """);
        Add("""
            { "identifiers": { "epic": "^[0-9]{4}$" }, "claim": { "source": "gherkin",
              "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" } ] } }
            """);
        Add("""
            { "claim": { "source": "gherkin",
              "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "arithmetic" } ] } }
            """);
        Add("""
            { "identifiers": { "epic": "^[0-9]{4}$" }, "claim": { "source": "gherkin",
              "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "arithmetic", "epic": "0001" } ] } }
            """);
        Add("""
            { "identifiers": { "epic": "^[0-9]{4}$" }, "claim": { "source": "gherkin",
              "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "epic": "1" } ] } }
            """);
    }
}
