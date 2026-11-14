namespace Specht.Facts;

/// <summary>
/// One row of a Feature's open-questions table (<c>0132-F2</c> B-008), its cells under the engine's headers (C-7) and
/// whether OQ-2's default reads it as resolved.
/// </summary>
/// <param name="Id">The row's first cell, <c>""</c> when the row has none.</param>
/// <param name="Question">The cell under the <c>Question</c> header, or <c>null</c> when the table has no such cell.</param>
/// <param name="Blocks">The cell under the <c>Blocks</c> header as written, not split into ids, or <c>null</c>.</param>
/// <param name="Resolution">The cell under the <c>Resolution</c> header as written, or <c>null</c>.</param>
/// <param name="Resolved">
/// <c>false</c> when the resolution is <c>null</c>, empty, or its first word is <c>Open</c> in exact case; <c>true</c>
/// otherwise.
/// </param>
/// <param name="Line">The row's one-based line.</param>
/// <implements epic="0132" feature="F2" claim="B-008" />
public sealed record OpenQuestionFacts(string Id, string? Question, string? Blocks, string? Resolution, bool Resolved, int Line);
