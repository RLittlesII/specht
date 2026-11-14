namespace Specht.Model;

/// <summary>A decision, ADR or lesson record beside a specification, read for the facts alone (<c>0132-F2</c> B-011).</summary>
/// <param name="RelativePath">Path relative to the repository root, its segments separated by <c>/</c>.</param>
/// <param name="Kind">The kind its folder gives: <c>adr</c>, <c>decision</c> or <c>lesson</c>.</param>
/// <param name="Frontmatter">Its frontmatter, with no node when it has none or it cannot be read.</param>
/// <param name="StatusLine">The first line under its <c>## Status</c> heading, or <c>null</c>.</param>
/// <implements epic="0132" feature="F2" claim="B-011" />
public sealed record RecordFile(string RelativePath, string Kind, Frontmatter Frontmatter, string? StatusLine);
