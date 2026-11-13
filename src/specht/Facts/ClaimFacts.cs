namespace Specht.Facts;

/// <summary>
/// One row of a Feature's claims table (<c>0132-F2</c> B-005), given whatever its status or its id (B-012, B-013), with no
/// traceability row, scenario or test (B-025).
/// </summary>
/// <param name="Id">The row's id as the check reads it, its trimmed first cell, <c>""</c> when the cell is empty.</param>
/// <param name="Text">The cell under the <c>Claim</c> header, or <c>null</c> when the table has no such cell.</param>
/// <param name="Status">The cell under the <c>Status</c> header as written, or <c>null</c> when the table has no such cell.</param>
/// <param name="Line">The row's one-based line.</param>
/// <implements epic="0132" feature="F2" claim="B-005" />
/// <implements epic="0132" feature="F2" claim="B-012" />
/// <implements epic="0132" feature="F2" claim="B-025" />
public sealed record ClaimFacts(string Id, string? Text, string? Status, int Line);
