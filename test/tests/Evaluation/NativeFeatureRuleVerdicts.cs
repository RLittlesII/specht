using Specht.Tests.Shared;

namespace Specht.Tests.Evaluation;

/// <summary>
/// The manifest members, the root-relative path and text of each file, and each <c>SPEC2001</c>, <c>SPEC2002</c>,
/// <c>SPEC4001</c>, <c>SPEC4002</c> and <c>SPEC4003</c> violation the check should report, as <c>id path(line)
/// identifier</c> (<c>0001-F16</c> B-008, B-020, B-021; § 7, item <c>0392</c>): one id on a scenario in each of two files
/// of one set, and on the Feature and a scenario of one file; a Rule whose two scenarios only inherit its tag, beside a
/// file declaring one id twice; one id in two sets of two identities, beside a file of the second declaring another id
/// twice; a record with every canonical section but the claims one, and with a claims section holding a wrong table out
/// of order; a record without the matrix section; a record whose matrix, or whose sign-off table, has the wrong
/// headers; a set with no record and a set whose record names no file; and a record with three claim files beside it.
/// A record's canonical sections are <see cref="SpechtTree.Sections"/>, below a frontmatter of <c>id</c> <c>F1</c> and
/// a title.
/// </summary>
public sealed class NativeFeatureRuleVerdicts : TheoryData<string, string[], string[], string[]>
{
    /// <summary>Initializes one row per case of § 7's testability table.</summary>
    public NativeFeatureRuleVerdicts()
    {
        var withoutClaims = SpechtTree.Sections.Where(static section => !section.StartsWith(ClaimsHeading, StringComparison.Ordinal)).ToArray();
        var recordWithoutClaims = Front + string.Join('\n', withoutClaims);

        Add("{}", [AddFile, SubtractFile], [Adds, Adds], [$"SPEC4003 {AddFile}(4) B-001", $"SPEC4003 {SubtractFile}(4) B-001"]);
        Add(
            "{}",
            [AddFile],
            ["@B-001\nFeature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n"],
            [$"SPEC4003 {AddFile}(2) B-001", $"SPEC4003 {AddFile}(5) B-001"]);
        Add(
            "{}",
            [AddFile, SubtractFile],
            [
                "Feature: Arithmetic\n\n  @B-001\n  Rule: Sums\n\n    Scenario: Adds\n      Given two numbers\n\n"
                + "    Scenario: Adds three\n      Given three numbers\n",
                TwiceB002,
            ],
            [$"SPEC4003 {SubtractFile}(4) B-002", $"SPEC4003 {SubtractFile}(8) B-002"]);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" },
              { "glob": ".spec/features/sums/*.feature", "feature": "F2" } ] } }
            """,
            [AddFile, TotalFile],
            [Adds, "Feature: Sums\n\n  @B-001\n  Scenario: Totals\n    Given numbers\n\n  @B-002\n  Scenario: Totals two\n    Given numbers\n\n"
                + "  @B-002\n  Scenario: Totals three\n    Given numbers\n"],
            [$"SPEC4003 {TotalFile}(8) B-002", $"SPEC4003 {TotalFile}(12) B-002"]);
        Add(WithRecord, [AddFile, RecordFile], [Adds, recordWithoutClaims], []);
        Add(
            WithRecord,
            [AddFile, RecordFile],
            [Adds, recordWithoutClaims + "\n" + ClaimsHeading + "\n\n| Wrong | Header |\n| ----- | ------ |\n| a | b |\n"],
            []);
        Add(
            WithRecord,
            [AddFile, RecordFile],
            [Adds, Front + string.Join('\n', withoutClaims.Where(static section => !section.StartsWith(MatrixHeading, StringComparison.Ordinal)))],
            [$"SPEC2001 {RecordFile}(0) 9. Traceability Matrix"]);
        Add(
            WithRecord,
            [AddFile, RecordFile],
            [Adds, recordWithoutClaims.Replace("| Claim ID | Scenario | Test | Status |", "| Claim | Scenario | Test | Status |", StringComparison.Ordinal)],
            [$"SPEC2002 {RecordFile}(41) 9. Traceability Matrix"]);
        Add(
            WithRecord,
            [AddFile, RecordFile],
            [Adds, recordWithoutClaims.Replace("| Section | Status | Reviewer | Note |", "| Section | Status | Reviewer | Notes |", StringComparison.Ordinal)],
            [$"SPEC2002 {RecordFile}(55) 12. Sign-off"]);
        Add("{}", [AddFile], [Adds], []);
        Add(
            """
            { "claim": { "files": [
              { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/missing.md" } ] } }
            """,
            [AddFile],
            [Adds],
            []);
        Add(
            WithRecord,
            [AddFile, SubtractFile, ".spec/features/arithmetic/total.feature", RecordFile],
            [
                Adds,
                "Feature: Arithmetic\n\n  @B-002\n  Scenario: Subtracts\n    Given two numbers\n",
                "Feature: Arithmetic\n\n  @B-003\n  Scenario: Totals\n    Given numbers\n",
                recordWithoutClaims,
            ],
            []);
    }

    private const string AddFile = ".spec/features/arithmetic/add.feature";

    private const string SubtractFile = ".spec/features/arithmetic/subtract.feature";

    private const string TotalFile = ".spec/features/sums/total.feature";

    private const string RecordFile = ".spec/features/arithmetic/README.md";

    private const string Adds = "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n";

    private const string TwiceB002 = "Feature: Arithmetic\n\n  @B-002\n  Scenario: Subtracts\n    Given two numbers\n\n"
        + "  @B-002\n  Scenario: Subtracts three\n    Given three numbers\n";

    private const string Front = "---\nid: F1\n---\n\n# Arithmetic\n\n";

    private const string ClaimsHeading = "## 3. Acceptance Criteria\n";

    private const string MatrixHeading = "## 9. Traceability Matrix\n";

    private const string WithRecord = """
        { "claim": { "files": [
          { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] } }
        """;
}
