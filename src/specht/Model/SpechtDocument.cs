namespace Specht.Model;

/// <summary>
/// A parsed specification document: its frontmatter and its top-level sections, as <see cref="SpechtDocumentParser"/> gives them.
/// </summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpechtDocument
{
    /// <summary>A document from its parts, as <see cref="SpechtDocumentParser.Parse"/> gives them.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpechtDocument(string relativePath, Frontmatter frontmatter, IReadOnlyList<SpechtSection> sections)
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
    public IReadOnlyList<SpechtSection> Sections { get; }

    /// <summary>The section titled <paramref name="title"/>, or <c>null</c>.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpechtSection? Section(string title) =>
        Sections.FirstOrDefault(section => string.Equals(section.Title, title, StringComparison.Ordinal));
}
