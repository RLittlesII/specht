using System.IO.Abstractions;
using System.Text.Json.Nodes;

namespace Specht.Model;

/// <summary>
/// Reads the YAML frontmatter of a markdown document into a <see cref="JsonObject"/>.
/// </summary>
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
/// <param name="fileSystem">The file system the documents are read from.</param>
public sealed class FrontmatterReader(IFileSystem fileSystem)
{
    /// <summary>Reads the frontmatter of the document at <paramref name="path"/>.</summary>
    public Frontmatter Read(string path) => Parse(fileSystem.File.ReadAllText(path));

    internal static Frontmatter Parse(string text)
    {
        var parsed = new FrontmatterTextReader().Read(text);
        return new Frontmatter(parsed.Node, parsed.StartLine, parsed.KeyLines);
    }
}
