using System.Text.Json.Nodes;

namespace Specht.Model;

/// <summary>Parses the YAML frontmatter of a markdown document's text into a <see cref="JsonObject"/>; it reads no file.</summary>
/// <remarks>
/// Reads through YamlDotNet's representation model rather than deserializing,
/// which is what keeps dates comparable. Both <c>created: 2026-08-27</c> and
/// <c>created: "2026-09-29"</c> appear in this repository, and a deserializer
/// resolves the first to a timestamp and the second to a string - so only one
/// of them would satisfy <c>format: date</c>. The representation model hands
/// back the raw scalar in both cases, and this reader types it: a quoted
/// scalar is a string by authorial intent, an unquoted one is only converted
/// when it is unambiguously null, boolean or integral. Dates therefore stay
/// strings, whichever way they were written.
/// </remarks>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class FrontmatterReader
{
    /// <summary>Parses the frontmatter of the document whose complete text is <paramref name="text"/>.</summary>
    /// <param name="text">The document's text.</param>
    /// <returns>The frontmatter, with the line of each key.</returns>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public Frontmatter Parse(string text)
    {
        var parsed = new FrontmatterTextReader().Read(text);
        return new Frontmatter(parsed.Node, parsed.StartLine, parsed.KeyLines);
    }
}
