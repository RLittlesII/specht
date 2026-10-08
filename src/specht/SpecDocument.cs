using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace specht;

/// <summary>
/// A parsed specification document: its frontmatter and its top-level sections.
/// </summary>
/// <remarks>
/// Tables are read from Markdig's AST rather than split on pipe characters.
/// Acceptance-criteria cells in this repository contain <c>|</c> inside inline
/// code and bold spans, and the largest specification is a third of a
/// megabyte - hand-rolled splitting is wrong on correctness and on cost.
/// </remarks>
public sealed class SpecDocument
{
    private SpecDocument(string relativePath, Frontmatter frontmatter, IReadOnlyList<SpecSection> sections)
    {
        RelativePath = relativePath;
        Frontmatter = frontmatter;
        Sections = sections;
    }

    /// <summary>Path relative to the repository root.</summary>
    public string RelativePath { get; }

    /// <summary>The document's frontmatter.</summary>
    public Frontmatter Frontmatter { get; }

    /// <summary>Top-level (<c>## </c>) sections, in document order.</summary>
    public IReadOnlyList<SpecSection> Sections { get; }

    /// <summary>Parses the document at <paramref name="absolutePath"/>.</summary>
    public static SpecDocument Parse(string absolutePath, string relativePath)
    {
        var document = Markdown.Parse(File.ReadAllText(absolutePath), Pipeline);
        var sections = new List<SpecSection>();

        for (var index = 0; index < document.Count; index++)
        {
            if (document[index] is not HeadingBlock { Level: 2 } heading)
            {
                continue;
            }

            var table = NextTable(document, index);

            sections.Add(new SpecSection(
                HeadingText(heading),
                heading.Line + 1,
                table is null ? [] : HeaderCells(table),
                table is null ? [] : BodyRows(table),
                table is null ? [] : BodyRowLines(table)));
        }

        return new SpecDocument(relativePath, FrontmatterReader.Read(absolutePath), sections);
    }

    /// <summary>The section titled <paramref name="title"/>, or <c>null</c>.</summary>
    public SpecSection? Section(string title) =>
        Sections.FirstOrDefault(section => string.Equals(section.Title, title, StringComparison.Ordinal));

    private static string HeadingText(HeadingBlock heading) =>
        heading.Inline is null ? string.Empty : Flatten(heading.Inline);

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

    private static IReadOnlyList<string> HeaderCells(Table table) =>
        table.OfType<TableRow>().Where(static row => row.IsHeader).Select(Cells).FirstOrDefault() ?? [];

    private static IReadOnlyList<IReadOnlyList<string>> BodyRows(Table table) =>
        table.OfType<TableRow>().Where(static row => !row.IsHeader).Select(Cells).ToList();

    private static IReadOnlyList<int> BodyRowLines(Table table) =>
        table.OfType<TableRow>().Where(static row => !row.IsHeader).Select(static row => row.Line + 1).ToList();

    private static IReadOnlyList<string> Cells(TableRow row) =>
        row.OfType<TableCell>().Select(CellText).ToList();

    private static string CellText(TableCell cell) => Flatten(cell);

    /// <summary>
    /// The visible text of a node: its literals and the content of its code
    /// spans. A code span is a leaf, not a literal, so a cell written
    /// <c>`B-001`</c> would otherwise read as empty and the claim would be
    /// skipped silently rather than checked.
    /// </summary>
    private static string Flatten(MarkdownObject node) =>
        string.Concat(node.Descendants().Select(static descendant => descendant switch
        {
            LiteralInline literal => literal.ToString(),
            CodeInline code => code.Content,
            _ => string.Empty,
        })).Trim();

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
}
