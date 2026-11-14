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

    /// <implements epic="0001" feature="F19" claim="B-041" />
    public IReadOnlyList<string> FirstCells(string text, MarkdownPipeline pipeline) =>
        Markdown.Parse(text, pipeline).Descendants<Table>()
            .SelectMany(static table => table.OfType<TableRow>())
            .Select(static row => row.OfType<TableCell>().FirstOrDefault())
            .OfType<TableCell>()
            .Select(CellText)
            .ToList();

    /// <implements epic="0132" feature="F2" claim="B-011" />
    public string? FirstLine(string text, string title, MarkdownPipeline pipeline)
    {
        var document = Markdown.Parse(text, pipeline);

        for (var index = 0; index < document.Count; index++)
        {
            if (document[index] is HeadingBlock { Level: 2 } heading && string.Equals(HeadingText(heading), title, StringComparison.Ordinal))
            {
                return document.Skip(index + 1)
                    .TakeWhile(static block => block is not HeadingBlock { Level: 2 })
                    .SelectMany(Paragraphs)
                    .Select(FirstLineText)
                    .FirstOrDefault();
            }
        }

        return null;
    }

    /// <implements epic="0132" feature="F2" claim="B-011" />
    private static IEnumerable<ParagraphBlock> Paragraphs(Block block) => block switch
    {
        ParagraphBlock paragraph => [paragraph],
        ContainerBlock container => container.Descendants<ParagraphBlock>(),
        _ => [],
    };

    /// <implements epic="0132" feature="F2" claim="B-011" />
    private static string FirstLineText(ParagraphBlock paragraph) =>
        paragraph.Inline is null
            ? string.Empty
            : string.Concat(paragraph.Inline.Descendants().TakeWhile(static inline => inline is not LineBreakInline).Select(Visible)).Trim();

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
            .Select(static row => (IReadOnlyList<IReadOnlyList<string>>) row.OfType<TableCell>()
                .Select(static cell => (IReadOnlyList<string>) cell.Descendants().OfType<CodeInline>()
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
    private static string Flatten(MarkdownObject node) => string.Concat(node.Descendants().Select(Visible)).Trim();

    /// <implements epic="0001" feature="F1" claim="B-004" />
    /// <implements epic="0132" feature="F2" claim="B-011" />
    private static string Visible(MarkdownObject descendant) => descendant switch
    {
        LiteralInline literal => literal.ToString(),
        CodeInline code => code.Content,
        _ => string.Empty,
    };
}
