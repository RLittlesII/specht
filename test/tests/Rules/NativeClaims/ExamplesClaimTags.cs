namespace Specht.Tests.Rules.NativeClaims;

/// <summary>
/// The manifest members, the text of each claim file, written in turn to <c>.spec/features/arithmetic/add.feature</c> and
/// <c>.spec/features/arithmetic/subtract.feature</c>, and each <c>SPEC4012</c> and <c>SPEC4011</c> violation the check
/// should report, as <c>id severity path(line)</c> and the identifier where it has one (<c>0001-F16</c> B-009, B-010; § 7,
/// item <c>0394</c>): a claim tag on an <c>Examples:</c> and on a <c>Scenarios:</c> block; above other tags and among
/// them; two on one block and one on each of two blocks; ordinary, mistyped and planned tags beside a claimed block; a
/// claim tag in another case while claim tags ignore case; the outline's only claim tag on its block; the outline claimed
/// by its Rule and by its Feature; the block naming the outline's own claim; one file two sets match; and a file the
/// reader cannot read beside a claimed block.
/// </summary>
public sealed class ExamplesClaimTags : TheoryData<string, string[], string[]>
{
    /// <summary>Initializes one row per case of § 7's table.</summary>
    public ExamplesClaimTags()
    {
        Add("{}", [Outline + "    @B-005\n" + Rows], [$"SPEC4012 Error {AddFile}(7) B-005"]);
        Add("{}", [Outline + "    @B-005\n    Scenarios:\n      | a |\n      | 1 |\n"], [$"SPEC4012 Error {AddFile}(7) B-005"]);
        Add("{}", [Outline + "    @smoke\n    @B-005\n    @fast\n" + Rows], [$"SPEC4012 Error {AddFile}(8) B-005"]);
        Add("{}", [Outline + "    @B-005 @B-006\n" + Rows], [$"SPEC4012 Error {AddFile}(7) B-005", $"SPEC4012 Error {AddFile}(7) B-006"]);
        Add(
            "{}",
            [Outline + "    @B-005\n" + Rows + "\n    @B-006\n" + Rows],
            [$"SPEC4012 Error {AddFile}(7) B-005", $"SPEC4012 Error {AddFile}(12) B-006"]);
        Add("{}", [Outline + "    @smoke @B-05 @ignore\n" + Rows, Outline + "    @B-005\n" + Rows], [$"SPEC4012 Error {SubtractFile}(7) B-005"]);
        Add("""{ "claim": { "tagIgnoreCase": true } }""", [Outline + "    @b-005\n" + Rows], [$"SPEC4012 Error {AddFile}(7) b-005"]);
        Add(
            "{}",
            ["Feature: Arithmetic\n\n  Scenario Outline: Adds <a>\n    Given <a>\n\n    @B-005\n    Examples:\n      | a |\n      | 1 |\n"],
            [$"SPEC4012 Error {AddFile}(6) B-005", $"SPEC4011 Warning {AddFile}(3)"]);
        Add(
            "{}",
            [
                "Feature: Arithmetic\n\n  @B-001\n  Rule: Sums\n\n    Scenario Outline: Adds <a>\n      Given <a>\n\n"
                + "      @B-005\n      Examples:\n        | a |\n        | 1 |\n",
            ],
            [$"SPEC4012 Error {AddFile}(9) B-005"]);
        Add(
            "{}",
            ["@B-001\nFeature: Arithmetic\n\n  Scenario Outline: Adds <a>\n    Given <a>\n\n    @B-005\n    Examples:\n      | a |\n      | 1 |\n"],
            [$"SPEC4012 Error {AddFile}(7) B-005"]);
        Add("{}", [Outline + "    @B-001\n" + Rows], [$"SPEC4012 Error {AddFile}(7) B-001"]);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" },
              { "glob": ".spec/features/**/*.feature", "feature": "F2" } ] } }
            """,
            [Outline + "    @B-005\n" + Rows],
            [$"SPEC4012 Error {AddFile}(7) B-005"]);
        Add("{}", ["Feature: Arithmetic\n\n    @B-005\n" + Rows, Outline + "    @B-006\n" + Rows], [$"SPEC4012 Error {SubtractFile}(7) B-006"]);
    }

    private const string AddFile = ".spec/features/arithmetic/add.feature";

    private const string SubtractFile = ".spec/features/arithmetic/subtract.feature";

    private const string Outline = "Feature: Arithmetic\n\n  @B-001\n  Scenario Outline: Adds <a>\n    Given <a>\n\n";

    private const string Rows = "    Examples:\n      | a |\n      | 1 |\n";
}
