namespace Specht.Tests.Evaluation;

/// <summary>
/// The manifest members of one claim file set, its record's text or none, a frontmatter rule, and the files that rule
/// should report (<c>0001-F16</c> B-003): a record is held by <c>SPEC1001</c> and <c>SPEC1002</c>, and the item beside it
/// by <c>SPEC1003</c>, as any specification and its items are; a set with no record has no frontmatter to hold and lists
/// no item. The tree also holds <c>.spec/features/arithmetic/0001-01-add.md</c>, an item whose frontmatter its schema
/// rejects.
/// </summary>
public sealed class RecordFrontmatterFindings : TheoryData<string, string?, string, string[]>
{
    /// <summary>Initializes one row per rule with a record, and one per rule without.</summary>
    public RecordFrontmatterFindings()
    {
        Add(WithRecord, "# Arithmetic\n", "SPEC1001", [Record]);
        Add(WithRecord, "---\nid: F1\n---\n\n# Arithmetic\n", "SPEC1002", [Record]);
        Add(WithRecord, "---\nid: F1\n---\n\n# Arithmetic\n", "SPEC1003", [".spec/features/arithmetic/0001-01-add.md"]);
        Add("{}", null, "SPEC1001", []);
        Add("{}", null, "SPEC1002", []);
        Add("{}", null, "SPEC1003", []);
    }

    private const string Record = ".spec/features/arithmetic/README.md";

    private const string WithRecord = """
        { "claim": { "files": [
          { "glob": ".spec/features/arithmetic/*.feature", "feature": "F1", "record": ".spec/features/arithmetic/README.md" } ] } }
        """;
}
