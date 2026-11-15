namespace Specht.Tests.Rules.NativeClaims;

/// <summary>
/// The manifest members, the text of each claim file, written in turn to <c>.spec/features/arithmetic/add.feature</c> and
/// <c>.spec/features/arithmetic/subtract.feature</c>, and each place <c>SPEC4011</c> should report as <c>path(line)</c>
/// (<c>0001-F16</c> B-010; § 7, item <c>0393</c>): every case keyword untagged; ordinary, mistyped and planned tags above
/// the keyword; a claim on one scenario beside an untagged one; a claimed Rule before an unclaimed one; a claimed
/// Feature; a claim tag on an Examples block alone; a claim id in a title alone; a file in another language and one with
/// no <c>Feature:</c> line, each beside an untagged file; and one file two sets match.
/// </summary>
public sealed class UnclaimedScenarios : TheoryData<string, string[], string[]>
{
    /// <summary>Initializes one row per case of § 7's table.</summary>
    public UnclaimedScenarios()
    {
        Add(
            "{}",
            [
                "Feature: Arithmetic\n\n  Scenario: Adds\n    Given two numbers\n\n  Example: Adds one\n    Given one number\n\n"
                + "  Scenario Outline: Adds <a>\n    Given <a>\n\n    Examples:\n      | a |\n      | 1 |\n\n"
                + "  Scenario Template: Adds <b>\n    Given <b>\n\n    Examples:\n      | b |\n      | 2 |\n",
            ],
            [$"{AddFile}(3)", $"{AddFile}(6)", $"{AddFile}(9)", $"{AddFile}(16)"]);
        Add("{}", ["Feature: Arithmetic\n\n  @smoke\n  @B-01\n  Scenario: Adds\n    Given two numbers\n"], [$"{AddFile}(5)"]);
        Add("{}", ["Feature: Arithmetic\n\n  @ignore\n  Scenario: Adds\n    Given two numbers\n"], [$"{AddFile}(4)"]);
        Add(
            "{}",
            ["Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n\n  Scenario: Subtracts\n    Given two numbers\n"],
            [$"{AddFile}(7)"]);
        Add(
            "{}",
            [
                "Feature: Arithmetic\n\n  @B-001\n  Rule: Sums\n\n    Scenario: Adds\n      Given two numbers\n\n"
                + "  Rule: Differences\n\n    Scenario: Subtracts\n      Given two numbers\n",
            ],
            [$"{AddFile}(11)"]);
        Add("{}", ["@B-001\nFeature: Arithmetic\n\n  Scenario: Adds\n    Given two numbers\n", Untagged], [$"{SubtractFile}(3)"]);
        Add(
            "{}",
            ["Feature: Arithmetic\n\n  Scenario Outline: Adds <a>\n    Given <a>\n\n    @B-001\n    Examples:\n      | a |\n      | 1 |\n"],
            [$"{AddFile}(3)"]);
        Add("{}", ["Feature: Arithmetic\n\n  Scenario: B-001 adds two numbers\n    Given two numbers\n"], [$"{AddFile}(3)"]);
        Add("{}", ["# language: fr\nFonctionnalité: Arithmétique\n\n  Scénario: Ajoute\n    Soit deux nombres\n", Untagged], [$"{SubtractFile}(3)"]);
        Add("{}", ["  Scenario: Adds\n    Given two numbers\n", Untagged], [$"{SubtractFile}(3)"]);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" },
              { "glob": ".spec/features/**/*.feature", "feature": "F2" } ] } }
            """,
            ["Feature: Arithmetic\n\n  Scenario: Adds\n    Given two numbers\n"],
            [$"{AddFile}(3)"]);
    }

    private const string AddFile = ".spec/features/arithmetic/add.feature";

    private const string SubtractFile = ".spec/features/arithmetic/subtract.feature";

    private const string Untagged = "Feature: Subtraction\n\n  Scenario: Subtracts\n    Given two numbers\n";
}
