namespace Specht.Tests.Rules.NativeClaims;

/// <summary>
/// The manifest members, the text of each claim file, written in turn to <c>.spec/features/arithmetic/add.feature</c> and
/// <c>.spec/features/arithmetic/subtract.feature</c>, and every claim the model should record, as <c>feature id</c> in set
/// order (<c>0001-F16</c> B-014, B-015, B-018; § 7, item <c>0395</c>): a first file in another language, a first file with
/// no <c>Feature:</c> line, and a first file a second set also matches, each beside a second file only the first set
/// matches.
/// </summary>
public sealed class UnclaimableClaimFiles : TheoryData<string, string[], string[]>
{
    /// <summary>Initializes one row per claim.</summary>
    public UnclaimableClaimFiles()
    {
        Add("{}", ["# language: de\n" + Adds, Subtracts], ["F1 B-002"]);
        Add("{}", ["  @B-001\n  Scenario: Adds\n    Given two numbers\n", Subtracts], ["F1 B-002"]);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" },
              { "glob": ".spec/features/**/add.feature", "feature": "F2" } ] } }
            """,
            [Adds, Subtracts],
            ["F1 B-002"]);
    }

    private const string Adds = "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n";

    private const string Subtracts = "Feature: Arithmetic\n\n  @B-002\n  Scenario: Subtracts\n    Given two numbers\n";
}
