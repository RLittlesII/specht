using Specht.Tests.Shared;

namespace Specht.Tests.Evaluation;

/// <summary>
/// The manifest members, the root-relative path and text of each file, and each <c>SPEC2003</c> and <c>SPEC4004</c> to
/// <c>SPEC4010</c> violation the check should report, as <c>id path(line) identifier</c> (<c>0001-F16</c> B-022 to
/// B-024; § 7, item <c>0396</c>): a claim file in a layout glob's directory part with no specification beside it, and a
/// companion beside a record that no set's glob matches; a matrix with one row per declared claim, one that lacks a
/// claim, one citing an id no claim file declares and one holding a claim twice; under title matching a cell naming a
/// scenario that carries the tag, one naming a scenario that does not and one naming no scenario; a covered row of a claim
/// the Feature declares with no scenario, and a covered row whose test no test file holds; and a set with no record and a
/// record without the matrix. A record is a Feature frontmatter and the canonical sections with the matrix replaced.
/// </summary>
public sealed class NativeMatrixVerdicts : TheoryData<string, string[], string[], string[]>
{
    /// <summary>Initializes one row per case of § 7's testability table.</summary>
    public NativeMatrixVerdicts()
    {
        var oneRow = Record(Heading + Header + "| B-001 | Adds | Missing | Missing |\n");
        var both = Record(Matrix("| B-001 | Adds | Missing | Missing |\n| B-002 | Subtracts | Missing | Missing |\n"));
        Add(WithRecord, [AddFile, SubtractFile, RecordFile], [Adds, Subtracts, both], []);
        Add(WithBesideSpecification, [BesideFile], [Adds], []);
        Add(WithNarrowGlob, [BesideFile, OtherFile, BesideRecord], [Adds, Adds, oneRow], [$"SPEC2003 {OtherFile}(0)"]);

        var lacking = Record(Matrix("| B-001 | Adds | Missing | Missing |\n"));
        Add(WithRecord, [AddFile, SubtractFile, RecordFile], [Adds, Subtracts, lacking], [$"SPEC4004 {RecordFile}({Line(lacking, Heading.TrimEnd())}) B-002"]);

        var unknown = Record(Matrix("| B-001 | Adds | Missing | Missing |\n| B-009 | Strays | Missing | Missing |\n"));
        Add(WithRecord, [AddFile, RecordFile], [Adds, unknown], [$"SPEC4004 {RecordFile}({Line(unknown, "| B-009")}) B-009"]);

        var twice = Record(Matrix("| B-001 | Adds | Missing | Missing |\n| B-001 | Adds again | Missing | Missing |\n"));
        Add(WithRecord, [AddFile, RecordFile], [Adds, twice], [$"SPEC4004 {RecordFile}({Line(twice, "| B-001 | Adds again")}) B-001"]);

        var titled = Record(Matrix("| B-001 | Adds | Missing | Missing |\n"));
        Add(WithTitles, [AddFile, RecordFile], [Adds, titled], []);

        var untagged = Record(Matrix("| B-001 | Subtracts | Missing | Missing |\n| B-002 | Subtracts | Missing | Missing |\n"));
        var untaggedLine = Line(untagged, "| B-001 | Subtracts");
        Add(WithTitles, [AddFile, SubtractFile, RecordFile], [Adds, Subtracts, untagged], [$"SPEC4007 {RecordFile}({untaggedLine}) B-001"]);

        var nameless = Record(Matrix("| B-001 | Nowhere | Missing | Missing |\n"));
        Add(WithTitles, [AddFile, RecordFile], [Adds, nameless], [$"SPEC4007 {RecordFile}({Line(nameless, "| B-001")}) B-001"]);

        var covered = Record(Matrix("| B-001 | Adds | `AddsTwoNumbers` | Covered |\n"));
        Add(WithTests, [AddFile, RecordFile, TestFile], ["@B-001\nFeature: Arithmetic\n", covered, Tests], []);

        var unresolved = Record(Matrix("| B-001 | Adds | `NoSuchTest` | Covered |\n"));
        Add(
            WithTests,
            [AddFile, RecordFile, TestFile],
            [Adds, unresolved, Tests],
            [
                $"SPEC4009 {RecordFile}({Line(unresolved, "| B-001")}) B-001",
                $"SPEC4010 {RecordFile}({Line(unresolved, "| B-001")}) B-001",
            ]);

        Add(WithoutRecord, [AddFile], [Adds], []);
        var withoutMatrix = Front + string.Join('\n', SpechtTree.Sections.Where(static section => !section.StartsWith("## 9.", StringComparison.Ordinal)));
        Add(WithRecord, [AddFile, RecordFile], [Adds, withoutMatrix], []);
    }

    private static string Matrix(string rows) => Heading + Header + rows;

    private static string Record(string matrix) =>
        Front + string.Join('\n', SpechtTree.SectionsWith("9. Traceability Matrix", matrix));

    private static int Line(string text, string fragment) => text[..text.LastIndexOf(fragment, StringComparison.Ordinal)].Count(static c => c == '\n') + 1;

    private const string AddFile = ".spec/features/arithmetic/add.feature";

    private const string BesideFile = "src/arithmetic/.spec/add.feature";

    private const string BesideRecord = "src/arithmetic/.spec/README.md";

    private const string OtherFile = "src/arithmetic/.spec/other.feature";

    private const string SubtractFile = ".spec/features/arithmetic/subtract.feature";

    private const string RecordFile = ".spec/features/arithmetic/README.md";

    private const string TestFile = "test/AddTests.cs";

    private const string Adds = "Feature: Arithmetic\n\n  @B-001\n  Scenario: Adds\n    Given two numbers\n";

    private const string Subtracts = "Feature: Arithmetic\n\n  @B-002\n  Scenario: Subtracts\n    Given two numbers\n";

    private const string Tests = "public sealed class AddTests\n{\n    public void AddsTwoNumbers() { }\n}\n";

    private const string Front = "---\nid: F1\n---\n\n# Arithmetic\n\n";

    private const string Heading = "## 9. Traceability Matrix\n\n";

    private const string Header = "| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n";

    private const string WithBesideSpecification = """
        { "claim": { "files": [ { "glob": "src/arithmetic/.spec/*.feature", "feature": "F1" } ] } }
        """;

    private const string WithNarrowGlob = """
        { "claim": { "files": [
          { "glob": "src/arithmetic/.spec/add.feature", "feature": "F1", "record": "src/arithmetic/.spec/README.md" } ] } }
        """;

    private const string WithRecord = """
        { "claim": { "files": [
          { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] } }
        """;

    private const string WithoutRecord = """
        { "claim": { "files": [ { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1" } ] } }
        """;

    private const string WithTitles = """
        { "matrix": { "scenarioTitles": true }, "claim": { "files": [
          { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] } }
        """;

    private const string WithTests = """
        { "tests": { "files": [ "test/**/*.cs" ] }, "claim": { "files": [
          { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] } }
        """;
}
