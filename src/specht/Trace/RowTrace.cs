namespace Specht.Trace;

/// <summary>A claim's traceability row, its four cells read by position and trimmed (<c>0001-F13</c> B-003).</summary>
/// <param name="Line">The row's one-based line in the specification.</param>
/// <param name="Claim">The first cell, the claim.</param>
/// <param name="Scenario">The second cell, the scenario.</param>
/// <param name="Test">The third cell's visible text.</param>
/// <param name="Status">The fourth cell, the coverage status.</param>
/// <implements epic="0001" feature="F13" claim="B-003" />
public sealed record RowTrace(int Line, string Claim, string Scenario, string Test, string Status);
