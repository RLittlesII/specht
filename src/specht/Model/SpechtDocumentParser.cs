using Markdig;

namespace Specht.Model;

/// <summary>Parses a specification document's text into a <see cref="SpechtDocument"/>; it reads no file.</summary>
/// <remarks>
/// Tables are read from Markdig's AST rather than split on pipe characters.
/// Acceptance-criteria cells in this repository contain <c>|</c> inside inline
/// code and bold spans, and the largest specification is a third of a
/// megabyte - hand-rolled splitting is wrong on correctness and on cost.
/// </remarks>
/// <param name="frontmatter">The frontmatter parser.</param>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpechtDocumentParser(FrontmatterReader frontmatter)
{
    /// <summary>Parses <paramref name="text"/>, the document at <paramref name="relativePath"/>.</summary>
    /// <param name="text">The document's complete text.</param>
    /// <param name="relativePath">The document's path relative to the repository root.</param>
    /// <returns>The document's frontmatter and top-level sections.</returns>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    /// <implements epic="0132" feature="F2" claim="B-009" />
    public SpechtDocument Parse(string text, string relativePath)
    {
        var sections = new MarkdownTextReader().Read(text, Pipeline).Select(static section =>
            new SpechtSection(section.Title, section.Line, section.Headers, section.Rows, section.RowLines)
            {
                RowCodeSpans = section.RowCodeSpans,
                Items = section.Items,
                ItemLines = section.ItemLines,
            }).ToArray();
        return new SpechtDocument(relativePath, frontmatter.Parse(text), sections);
    }

    /// <summary>The first cell of every row of every pipe table in <paramref name="text"/>, as the check reads a cell.</summary>
    /// <param name="text">The document's complete text.</param>
    /// <returns>Each first cell, in document order.</returns>
    /// <implements epic="0001" feature="F19" claim="B-041" />
    public IReadOnlyList<string> FirstCells(string text) => new MarkdownTextReader().FirstCells(text, Pipeline);

    /// <summary>
    /// The first line of the first paragraph under the first level-2 heading titled <paramref name="title"/>, before the
    /// next level-2 heading, flattened as the check reads a cell.
    /// </summary>
    /// <param name="text">The document's complete text.</param>
    /// <param name="title">The heading's text, compared ordinally.</param>
    /// <returns>The line, or <c>null</c> with no such heading or no paragraph under it.</returns>
    /// <implements epic="0132" feature="F2" claim="B-011" />
    public string? FirstLine(string text, string title) => new MarkdownTextReader().FirstLine(text, title, Pipeline);

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
}
