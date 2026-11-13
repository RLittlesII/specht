namespace Specht.Tests.Facts;

/// <summary>
/// A claims table's headers and two rows under them: the shipped templates' <c>ID | Claim | Source | Status</c>, and a
/// table with no <c>Source</c> column, whose status is still the cell under <c>Status</c> (<c>0132-F2</c> B-005).
/// </summary>
public sealed class FactsClaimTables : TheoryData<string[], string[], string[]>
{
    public FactsClaimTables()
    {
        Add(
            ["ID", "Claim", "Source", "Status"],
            ["B-001", "Given a tree, this Feature gives its facts.", "the owner", "Active"],
            ["B-002", "Given a flag, this Feature writes one document.", "the owner", "Proposed"]);
        Add(
            ["ID", "Claim", "Status"],
            ["B-001", "Given a tree, this Feature gives its facts.", "Active"],
            ["B-002", "Given a flag, this Feature writes one document.", "Proposed"]);
    }
}
