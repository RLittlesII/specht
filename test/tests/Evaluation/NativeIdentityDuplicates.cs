namespace Specht.Tests.Evaluation;

/// <summary>
/// The manifest members, claim files and specifications of a tree declaring the identity <c>F1</c> twice, and each place
/// <c>SPEC3002</c> should report it as <c>path(line)</c> (<c>0001-F16</c> B-017; OQ-12): a set beside a specification a
/// layout discovers, reported at the specification's id line and at the set's native Feature, line <c>0</c>; and two
/// sets, each at its native Feature, line <c>0</c>.
/// </summary>
public sealed class NativeIdentityDuplicates : TheoryData<string, string[], string[], string[]>
{
    /// <summary>Initializes one row for a set beside a layout's specification and one for two sets.</summary>
    public NativeIdentityDuplicates()
    {
        Add(
            "{}",
            [".spec/features/arithmetic/add.feature"],
            ["src/arithmetic/.spec/README.md"],
            [".spec/features/arithmetic/add.feature(0)", "src/arithmetic/.spec/README.md(2)"]);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" },
              { "glob": ".spec/features/sums/*.feature", "feature": "F1" } ] } }
            """,
            [".spec/features/arithmetic/add.feature", ".spec/features/sums/total.feature"],
            [],
            [".spec/features/arithmetic/add.feature(0)", ".spec/features/sums/total.feature(0)"]);
    }
}
