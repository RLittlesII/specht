using System.Globalization;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace specht.tests;

/// <summary>
/// A rule page as a test reads it (<c>0001-F3</c> B-032, B-033, C-8), shared by the unit tests and the acceptance steps.
/// The frontmatter and the metadata table come from the engine's own <see cref="SpecDocument"/>; the headings of every
/// level, a heading nested in a list or a quote included, come from Markdig with its frontmatter extension on, because
/// <see cref="SpecDocument"/> keeps only the <c>## </c> sections and reads a frontmatter block's closing delimiter as a
/// heading underline.
/// </summary>
internal sealed class RulePage
{
    private RulePage(int version, string id, string text, SpecDocument document, IReadOnlyList<(string Heading, int Line)> headings)
    {
        Version = version;
        Id = id;
        Text = text;
        Frontmatter = document.Frontmatter;
        Metadata = document.Section("Metadata");
        _headings = headings;
    }

    /// <summary>Gets the headings after the first, as the one page shape orders them (decision 0003).</summary>
    public static IReadOnlyList<string> Shape { get; } =
    [
        "## Metadata",
        "## Cause",
        "## Rule description",
        "### Example violation",
        "### Corrected",
        "## How to fix violations",
    ];

    /// <summary>Gets the schema version the page ships under.</summary>
    public int Version { get; }

    /// <summary>Gets the rule id the page is kept under.</summary>
    public string Id { get; }

    /// <summary>Gets the page's text.</summary>
    public string Text { get; }

    /// <summary>Gets the page's frontmatter.</summary>
    public Frontmatter Frontmatter { get; }

    /// <summary>Gets the <c>## Metadata</c> section and its table, or <c>null</c>.</summary>
    public SpecSection? Metadata { get; }

    /// <summary>Gets every heading in document order, each with its <c>#</c> marker, such as <c>## Cause</c>.</summary>
    public IReadOnlyList<string> Outline => _headings.Select(static heading => heading.Heading).ToList();

    /// <summary>Reads the page <paramref name="version"/> ships for <paramref name="id"/>.</summary>
    /// <param name="version">The schema version the page ships under.</param>
    /// <param name="id">The rule id the page is kept under.</param>
    /// <param name="text">The page's Markdown.</param>
    /// <returns>The page.</returns>
    public static RulePage Read(int version, string id, string text)
    {
        var headings = Markdown.Parse(text, Pipeline)
            .Descendants<HeadingBlock>()
            .Select(static heading => ($"{new string('#', heading.Level)} {Flatten(heading)}", heading.Line))
            .ToList();

        return new RulePage(version, id, text, SpecDocument.Parse(text, $"v{version}/{id}.md"), headings);
    }

    /// <summary>The value the metadata table gives for <paramref name="property"/>, or <c>null</c>.</summary>
    /// <param name="property">The row's first cell.</param>
    /// <returns>The row's second cell.</returns>
    public string? Value(string property) =>
        Metadata?.Rows.FirstOrDefault(row => row.Count > 1 && string.Equals(row[0], property, StringComparison.Ordinal))?[1];

    /// <summary>The non-blank lines between <paramref name="heading"/> and the heading after it.</summary>
    /// <param name="heading">The heading, with its marker.</param>
    /// <returns>The lines, trimmed; empty when the page has no such heading.</returns>
    public IReadOnlyList<string> Body(string heading)
    {
        var index = _headings.ToList().FindIndex(entry => string.Equals(entry.Heading, heading, StringComparison.Ordinal));

        if (index < 0)
        {
            return [];
        }

        var lines = Text.Split('\n');
        var end = index + 1 < _headings.Count ? _headings[index + 1].Line : lines.Length;

        return lines[(_headings[index].Line + 1)..end].Select(static line => line.Trim()).Where(static line => line.Length > 0).ToList();
    }

    /// <summary>The lines under <c>### Example violation</c> shaped as the line the tool prints for this page's rule.</summary>
    /// <returns>The file each such line names.</returns>
    public IReadOnlyList<string> ToolLineFiles()
    {
        var line = new Regex(
            $@"^(?<file>[^\s(:][^\s(]*)(\(\d+\))?: (error|warning) {Regex.Escape(Id)}: \S",
            RegexOptions.CultureInvariant);

        return Body("### Example violation")
            .Select(text => line.Match(text))
            .Where(static match => match.Success)
            .Select(static match => match.Groups["file"].Value)
            .ToList();
    }

    /// <summary>The schema version the page ships under, as its metadata table writes it.</summary>
    /// <returns>The number, in the invariant culture.</returns>
    public string VersionText() => Version.ToString(CultureInfo.InvariantCulture);

    private static string Flatten(HeadingBlock heading) =>
        heading.Inline is null
            ? string.Empty
            : string.Concat(heading.Inline.Descendants().Select(static inline => inline switch
            {
                LiteralInline literal => literal.ToString(),
                CodeInline code => code.Content,
                _ => string.Empty,
            })).Trim();

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseYamlFrontMatter().UsePipeTables().Build();

    private readonly IReadOnlyList<(string Heading, int Line)> _headings;
}
