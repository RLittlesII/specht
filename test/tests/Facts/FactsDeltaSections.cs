namespace Specht.Tests.Facts;

/// <summary>
/// A lessons-and-deltas section's list items and their lines, its table's headers, rows and row lines, and the entries the
/// facts give for it, as texts, cells and lines in line order (<c>0132-F2</c> B-009, B-015): list items alone, a table
/// alone under the schema <c>0.4.0</c> template's headers, both with the table between two items under the <c>0.1.0</c>
/// template's headers, and neither.
/// </summary>
public sealed class FactsDeltaSections : TheoryData<string[], int[], string[], string[][], int[], string?[], string[][], int[]>
{
    public FactsDeltaSections()
    {
        Add(
            ["2026-10-30, after review round 1. B-001 narrowed to one input.", "2026-10-31, after review round 2."],
            [132, 133],
            [],
            [],
            [],
            ["2026-10-30, after review round 1. B-001 narrowed to one input.", "2026-10-31, after review round 2."],
            [[], []],
            [132, 133]);
        Add(
            [],
            [],
            ["Trigger", "Change", "Affected IDs"],
            [["Review round 1", "B-001 narrowed to one input", "B-001"], ["Review round 2", "B-002 gains an example", "B-002"]],
            [134, 135],
            [null, null],
            [["Review round 1", "B-001 narrowed to one input", "B-001"], ["Review round 2", "B-002 gains an example", "B-002"]],
            [134, 135]);
        Add(
            ["Before the table.", "After the table."],
            [130, 140],
            ["Lesson", "Delta", "Claim"],
            [["Review round 1", "B-001 narrowed", "B-001"], ["Review round 2", "B-002 gains an example", "B-002"]],
            [134, 135],
            ["Before the table.", null, null, "After the table."],
            [[], ["Review round 1", "B-001 narrowed", "B-001"], ["Review round 2", "B-002 gains an example", "B-002"], []],
            [130, 134, 135, 140]);
        Add([], [], [], [], [], [], [], []);
    }
}
