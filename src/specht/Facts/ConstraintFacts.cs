namespace Specht.Facts;

/// <summary>One row of a Feature's constraints table (<c>0132-F2</c> B-006), its cells under the engine's headers (C-7).</summary>
/// <param name="Id">The row's first cell, <c>""</c> when the row has none.</param>
/// <param name="Text">The cell under the <c>Constraint</c> header, or <c>null</c> when the table has no such cell.</param>
/// <param name="RulesOut">The cell under the <c>Rules Out</c> header, or <c>null</c> when the table has no such cell.</param>
/// <param name="Line">The row's one-based line.</param>
/// <implements epic="0132" feature="F2" claim="B-006" />
public sealed record ConstraintFacts(string Id, string? Text, string? RulesOut, int Line);
