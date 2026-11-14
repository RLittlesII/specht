namespace Specht.Facts;

/// <summary>
/// One row of a Feature's sign-off table (<c>0132-F2</c> B-010), its cells under the headers the manifest's
/// <c>tables.signOff</c> lists, by their place in the list, or the engine's where it lists none (B-016, B-017).
/// </summary>
/// <param name="Section">The cell under the first header, or <c>null</c> when the table has no such cell.</param>
/// <param name="Status">The cell under the second header, or <c>null</c> when the table has no such cell.</param>
/// <param name="Reviewer">The cell under the third header, or <c>null</c> when the table has no such cell.</param>
/// <param name="Note">The cell under the fourth header, or <c>null</c> when the table has no such cell.</param>
/// <param name="Line">The row's one-based line.</param>
/// <implements epic="0132" feature="F2" claim="B-010" />
/// <implements epic="0132" feature="F2" claim="B-016" />
public sealed record SignOffFacts(string? Section, string? Status, string? Reviewer, string? Note, int Line);
