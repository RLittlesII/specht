namespace Specht.Facts;

/// <summary>
/// One decision, ADR or lesson record beside a Feature specification (<c>0132-F2</c> B-011), its status read on OQ-3's
/// default.
/// </summary>
/// <param name="Kind">The kind its folder gives: <c>decision</c>, <c>adr</c> or <c>lesson</c>.</param>
/// <param name="Number">The file name's first word as written, or <c>null</c> when the name begins with no letter or digit.</param>
/// <param name="Title">The frontmatter's <c>title</c>, or <c>null</c> when the frontmatter is missing, unreadable or gives none.</param>
/// <param name="Status">
/// For an ADR the first word of its first line under <c>## Status</c>, or <c>null</c>; always <c>null</c> for a decision or
/// a lesson.
/// </param>
/// <param name="Path">The record's path, relative to the root with <c>/</c> separators.</param>
/// <implements epic="0132" feature="F2" claim="B-011" />
public sealed record RecordFacts(string Kind, string? Number, string? Title, string? Status, string Path);
