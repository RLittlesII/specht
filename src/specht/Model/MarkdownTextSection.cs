using System.Collections.Generic;

namespace Specht.Model;

/// <summary>A shared Markdown section retaining visible table cells, code spans and source lines.</summary>
internal sealed record MarkdownTextSection(
    string Title,
    int Line,
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    IReadOnlyList<int> RowLines)
{
    public IReadOnlyList<IReadOnlyList<IReadOnlyList<string>>> RowCodeSpans { get; init; } = [];
}
