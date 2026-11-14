namespace Specht.Facts;

/// <summary>
/// One entry of a Feature's lessons-and-deltas section (<c>0132-F2</c> B-009): a list item, given by its text, or a row of
/// the section's table, given by its cells.
/// </summary>
/// <param name="Text">A list item's text as the parse gives it, or <c>null</c> for a table row.</param>
/// <param name="Cells">A table row's cells in column order, as the parse gives them; empty for a list item.</param>
/// <param name="Line">The entry's one-based first line.</param>
/// <implements epic="0132" feature="F2" claim="B-009" />
public sealed record DeltaFacts(string? Text, IReadOnlyList<string> Cells, int Line);
