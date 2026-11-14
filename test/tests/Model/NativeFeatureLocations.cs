namespace Specht.Tests.Model;

/// <summary>
/// The manifest members of one claim file set, the claim files its glob matches, and the path the native Feature should
/// be located at (<c>0001-F16</c> B-026; OQ-7): its record when the set has one, otherwise its first claim file in
/// ordinal order of relative path.
/// </summary>
public sealed class NativeFeatureLocations : TheoryData<string, string[], string>
{
    /// <summary>Initializes one row with a record and one without.</summary>
    public NativeFeatureLocations()
    {
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] } }
            """,
            [".spec/features/arithmetic/add.feature"],
            ".spec/features/arithmetic/README.md");
        Add("{}", [".spec/features/arithmetic/subtract.feature", ".spec/features/arithmetic/add.feature"], ".spec/features/arithmetic/add.feature");
    }
}
