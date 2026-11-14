namespace Specht.Facts;

/// <summary>One row of a Feature's out-of-scope table (<c>0132-F2</c> B-007), its cells under the engine's headers (C-7).</summary>
/// <param name="Number">The row's first cell as the parse gives it, never parsed into a number; <c>""</c> when the row has none.</param>
/// <param name="Item">The cell under the <c>Item</c> header, or <c>null</c> when the table has no such cell.</param>
/// <param name="Reason">The cell under the <c>Exclusion Reason</c> header, or <c>null</c> when the table has no such cell.</param>
/// <param name="Line">The row's one-based line.</param>
/// <implements epic="0132" feature="F2" claim="B-007" />
public sealed record OutOfScopeFacts(string Number, string? Item, string? Reason, int Line);
