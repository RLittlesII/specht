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
    public SpechtDocument Parse(string text, string relativePath)
    {
        var sections = new MarkdownTextReader().Read(text, Pipeline).Select(static section =>
            new SpechtSection(section.Title, section.Line, section.Headers, section.Rows, section.RowLines)
            {
                RowCodeSpans = section.RowCodeSpans,
            }).ToArray();
        return new SpechtDocument(relativePath, frontmatter.Parse(text), sections);
    }

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
}
