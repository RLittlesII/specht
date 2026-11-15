using System.Text.Json.Serialization;

namespace Specht.Trace;

/// <summary>One claim's trace: its row, the scenarios carrying its tag and the tests its row names (<c>0001-F13</c> B-003).</summary>
/// <param name="Id">The claim id as the claims table's first cell writes it, trimmed.</param>
/// <param name="Line">The claim's one-based line in the specification.</param>
/// <param name="Status">The claims table's Status cell, trimmed; empty when the row has none.</param>
/// <param name="Row">The first traceability row naming the claim, in document order; absent when none does.</param>
/// <param name="Scenarios">The companion scenarios carrying the claim's tag; empty when none does.</param>
/// <param name="Tests">The tests the row names, in cell order; empty without a row.</param>
/// <implements epic="0001" feature="F13" claim="B-003" />
public sealed record ClaimTrace(
    string Id,
    int Line,
    string Status,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] RowTrace? Row,
    IReadOnlyList<ScenarioTrace> Scenarios,
    IReadOnlyList<TestTrace> Tests);
