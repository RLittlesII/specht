namespace Specht.Tests.Evaluation;

/// <summary>
/// The manifest members of a tree holding one claim file and one specification in each layout the members name, and the
/// report's <c>layouts</c> as <c>name count</c> (<c>0001-F16</c> B-026; OQ-11, OQ-16; C-3): a set counted under
/// <c>claims</c> after the manifest's layouts; a manifest layout named <c>claims</c> keeping its own entry, the set's
/// following the manifest's; and no <c>claims</c> entry under a Markdown claim source.
/// </summary>
public sealed class NativeReportLayouts : TheoryData<string, string[], string[]>
{
    /// <summary>Initializes the default layouts, a manifest layout named <c>claims</c> and a Markdown claim source.</summary>
    public NativeReportLayouts()
    {
        Add("{}", ["src/arithmetic/.spec/README.md"], ["epics 0", "features 1", "claims 1"]);
        Add(
            """
            { "layouts": [
              { "name": "claims", "glob": "docs/**/spec.md" },
              { "name": "features", "glob": "**/.spec/README.md" } ] }
            """,
            ["docs/claims/spec.md", "src/arithmetic/.spec/README.md"],
            ["claims 1", "features 1", "claims 1"]);
        Add("""{ "claim": { "source": "markdown" } }""", ["src/arithmetic/.spec/README.md"], ["epics 0", "features 1"]);
    }
}
