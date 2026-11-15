namespace Specht.Tests.Rules.NativeClaims;

/// <summary>
/// The manifest members, the root-relative path and text of each file, and each <c>SPEC4013</c> violation the check
/// should report, as <c>id severity path(line)</c> (<c>0001-F16</c> B-014 to B-016, B-018, B-019, B-033, B-034; § 7, item
/// <c>0395</c>): a <c># language: fr</c> header on line 2 beside an English one; a file with no <c>Feature:</c>
/// line, an empty file, and each other text the reader refuses; a set whose glob matches no file, with and without a
/// <c>record</c>; one file two sets match; a Feature-level tag naming another Feature beside one that agrees and a file
/// with none; and a <c>record</c> naming no file, a directory, a path outside the root, and a rooted path. Every row
/// expects one violation.
/// </summary>
public sealed class UnreadableClaimInputs : TheoryData<string, string[], string[], string[]>
{
    /// <summary>Initializes one row per case of § 7's testability table, and one per cause B-015 names.</summary>
    public UnreadableClaimInputs()
    {
        Add("{}", [AddFile, SubtractFile], ["# A comment\n# language: fr\n" + Adds, "# language: en\n" + Subtracts], [$"SPEC4013 Error {AddFile}(2)"]);
        Add("{}", [AddFile], ["  @B-001\n  Scenario: Adds\n    Given two numbers\n"], [$"SPEC4013 Error {AddFile}(0)"]);
        Add("{}", [AddFile], [string.Empty], [$"SPEC4013 Error {AddFile}(0)"]);
        Add("{}", [AddFile], [Adds + "\nFeature: Again\n"], [$"SPEC4013 Error {AddFile}(0)"]);
        Add(
            "{}",
            [AddFile],
            ["Feature: Arithmetic\n\n  @B-001\n  Scenario Outline: Adds <a>\n    Given <a>\n\n    Examples:\n      | a |\n      | 1 | 2 |\n"],
            [$"SPEC4013 Error {AddFile}(0)"]);
        Add("{}", [AddFile], ["Feature: Arithmetic\n\n  @B-001 smoke\n  Scenario: Adds\n    Given two numbers\n"], [$"SPEC4013 Error {AddFile}(0)"]);
        Add("{}", [AddFile], [Adds + "      \"\"\"\n      text\n"], [$"SPEC4013 Error {AddFile}(0)"]);
        Add("{}", [AddFile], [Adds + "\n  @B-002\n"], [$"SPEC4013 Error {AddFile}(0)"]);
        Add("{}", [AddFile], ["Feature: Arithmetic\n\n    Examples:\n      | a |\n      | 1 |\n"], [$"SPEC4013 Error {AddFile}(0)"]);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" },
              { "glob": ".spec/features/sums/*.feature", "feature": "F2" } ] } }
            """,
            [AddFile],
            [Adds],
            ["SPEC4013 Error .spec/features/sums/*.feature(0)"]);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" },
              { "glob": ".spec/features/sums/*.feature", "feature": "F2", "record": ".spec/features/sums/README.md" } ] } }
            """,
            [AddFile, ".spec/features/sums/README.md"],
            [Adds, "---\nid: F2\n---\n\n# Sums\n"],
            ["SPEC4013 Error .spec/features/sums/*.feature(0)"]);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" },
              { "glob": ".spec/features/**/add.feature", "feature": "F2" } ] } }
            """,
            [AddFile, SubtractFile],
            [Adds, Subtracts],
            [$"SPEC4013 Error {AddFile}(0)"]);
        Add(
            """{ "claim": { "scopes": { "featureTag": "@spec.feature:{feature}" } } }""",
            [AddFile, SubtractFile, TotalFile],
            ["@smoke\n@spec.feature:F2\n" + Adds, "@spec.feature:F1\n" + Subtracts, "Feature: Totals\n\n  @B-003\n  Scenario: Totals\n    Given numbers\n"],
            [$"SPEC4013 Error {AddFile}(2)"]);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] } }
            """,
            [AddFile],
            [Adds],
            ["SPEC4013 Error .spec/features/arithmetic/README.md(0)"]);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic" } ] } }
            """,
            [AddFile],
            [Adds],
            ["SPEC4013 Error .spec/features/arithmetic(0)"]);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": "../outside/README.md" } ] } }
            """,
            [AddFile, "../outside/README.md"],
            [Adds, OutsideRecord],
            ["SPEC4013 Error ../outside/README.md(0)"]);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": "/outside/README.md" } ] } }
            """,
            [AddFile, "../outside/README.md"],
            [Adds, OutsideRecord],
            ["SPEC4013 Error .specht/manifest.json(0)"]);
    }

    private const string AddFile = ".spec/features/arithmetic/add.feature";

    private const string SubtractFile = ".spec/features/arithmetic/subtract.feature";

    private const string TotalFile = ".spec/features/arithmetic/total.feature";

    private const string Adds = "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n";

    private const string Subtracts = "Feature: Arithmetic\n\n  @B-002\n  Scenario: Subtracts\n    Given two numbers\n";

    private const string OutsideRecord = "---\nid: F1\nspec_status: approved\n---\n\n# Arithmetic\n";
}
