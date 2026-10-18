using Markdig;

namespace Specht.Model;

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
    /// <summary>A document from its parts, as <see cref="Parse"/> gives them.</summary>
    public SpecDocument(string relativePath, Frontmatter frontmatter, IReadOnlyList<SpecSection> sections)
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

    /// <summary>Parses <paramref name="text"/>, the document at <paramref name="relativePath"/>.</summary>
    public static SpecDocument Parse(string text, string relativePath)
    {
        var sections = new MarkdownTextReader().Read(text, Pipeline).Select(static section =>
            new SpecSection(section.Title, section.Line, section.Headers, section.Rows, section.RowLines)
            {
                RowCodeSpans = section.RowCodeSpans,
            }).ToArray();
        return new SpecDocument(relativePath, FrontmatterReader.Parse(text), sections);
    }

    /// <summary>The section titled <paramref name="title"/>, or <c>null</c>.</summary>
    public SpecSection? Section(string title) =>
        Sections.FirstOrDefault(section => string.Equals(section.Title, title, StringComparison.Ordinal));

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
}
