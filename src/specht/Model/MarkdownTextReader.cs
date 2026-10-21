using System;
using System.Collections.Generic;
using System.Linq;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Specht.Model;

/// <summary>The shared top-level Markdown section and first-pipe-table reader.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
internal sealed class MarkdownTextReader
{
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public IReadOnlyList<MarkdownTextSection> Read(string text, MarkdownPipeline pipeline)
    {
        var document = Markdown.Parse(text, pipeline);
        var sections = new List<MarkdownTextSection>();

        for (var index = 0; index < document.Count; index++)
        {
            if (document[index] is not HeadingBlock { Level: 2 } heading)
            {
                continue;
            }

            var table = NextTable(document, index);

            sections.Add(new MarkdownTextSection(
                HeadingText(heading),
                heading.Line + 1,
                table is null ? [] : HeaderCells(table),
                table is null ? [] : BodyRows(table),
                table is null ? [] : BodyRowLines(table))
            {
                RowCodeSpans = table is null ? [] : BodyCodeSpans(table),
            });
        }

        return sections;
    }

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static string HeadingText(HeadingBlock heading) =>
        heading.Inline is null ? string.Empty : Flatten(heading.Inline);

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static Table? NextTable(MarkdownDocument document, int headingIndex)
    {
        for (var index = headingIndex + 1; index < document.Count; index++)
        {
            switch (document[index])
            {
                case HeadingBlock { Level: 2 }:
                    return null;
                case Table table:
                    return table;
            }
        }

        return null;
    }

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static IReadOnlyList<string> HeaderCells(Table table) =>
        table.OfType<TableRow>().Where(static row => row.IsHeader).Select(Cells).FirstOrDefault() ?? [];

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static IReadOnlyList<IReadOnlyList<string>> BodyRows(Table table) =>
        table.OfType<TableRow>().Where(static row => !row.IsHeader).Select(Cells).ToList();

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static IReadOnlyList<IReadOnlyList<IReadOnlyList<string>>> BodyCodeSpans(Table table) =>
        table.OfType<TableRow>().Where(static row => !row.IsHeader)
            .Select(static row => (IReadOnlyList<IReadOnlyList<string>>)row.OfType<TableCell>()
                .Select(static cell => (IReadOnlyList<string>)cell.Descendants().OfType<CodeInline>()
                    .Select(static code => code.Content).ToArray())
                .ToArray())
            .ToArray();

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static IReadOnlyList<int> BodyRowLines(Table table) =>
        table.OfType<TableRow>().Where(static row => !row.IsHeader).Select(static row => row.Line + 1).ToList();

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static IReadOnlyList<string> Cells(TableRow row) =>
        row.OfType<TableCell>().Select(CellText).ToList();

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static string CellText(TableCell cell) => Flatten(cell);

    /// <summary>The visible literals and leaf code-span content of a Markdown node.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static string Flatten(MarkdownObject node) =>
        string.Concat(node.Descendants().Select(static descendant => descendant switch
        {
            LiteralInline literal => literal.ToString(),
            CodeInline code => code.Content,
            _ => string.Empty,
        })).Trim();
}
