using System.Text.Json.Nodes;

namespace specht;

/// <summary>A document's YAML frontmatter, as a JSON object a schema can evaluate.</summary>
/// <param name="Node">The frontmatter mapping, or <c>null</c> when the document has none.</param>
/// <param name="StartLine">One-based line of the opening <c>---</c>.</param>
/// <param name="KeyLines">One-based line of each top-level key, so a schema error can point at the row.</param>
public sealed record Frontmatter(JsonObject? Node, int StartLine, IReadOnlyDictionary<string, int> KeyLines)
{
    /// <summary>Line of <paramref name="key"/>, falling back to the frontmatter's own start.</summary>
    public int LineOf(string? key) =>
        key is not null && KeyLines.TryGetValue(key, out var line) ? line : StartLine;

    /// <summary>A top-level string value, or <c>null</c> when absent or not a string.</summary>
    public string? Text(string key) => Node?[key] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
}
