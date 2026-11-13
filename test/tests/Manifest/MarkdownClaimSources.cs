namespace Specht.Tests.Manifest;

/// <summary>
/// The members of a manifest pinned to schema 0.4.0 that set no <c>claim.source</c>, or set it to <c>markdown</c>, among
/// them one whose <c>codeAnalysis</c> section selects Gherkin claims for the analyzer (<c>0001-F16</c> B-002, C-7).
/// </summary>
public sealed class MarkdownClaimSources : TheoryData<string>
{
    /// <summary>Initializes one row for each way a manifest leaves the claim source Markdown.</summary>
    public MarkdownClaimSources()
    {
        Add("{}");
        Add("""{ "claim": {} }""");
        Add("""{ "claim": { "tag": "@{claim}" } }""");
        Add("""{ "claim": { "source": "markdown" } }""");
        Add("""
            { "codeAnalysis": { "records": { "claimSource": "gherkin",
              "claimFileSets": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "arithmetic" } ] } } }
            """);
    }
}
