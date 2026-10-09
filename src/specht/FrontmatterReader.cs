using System.Text.Json.Nodes;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace specht;

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
public static class FrontmatterReader
{
    /// <summary>Reads the frontmatter of the document at <paramref name="path"/>.</summary>
    public static Frontmatter Read(string path) => Parse(File.ReadAllText(path));

    /// <summary>Reads the frontmatter of <paramref name="text"/>, a whole document.</summary>
    public static Frontmatter Parse(string text)
    {
        var lines = Lines(text);

        if (lines.Length == 0 || lines[0].TrimEnd() != Delimiter)
        {
            return None;
        }

        var end = Array.FindIndex(lines, 1, static line => line.TrimEnd() == Delimiter);

        if (end < 0)
        {
            return None;
        }

        var stream = new YamlStream();

        try
        {
            stream.Load(new StringReader(string.Join('\n', lines[1..end])));
        }
        catch (YamlException)
        {
            return None;
        }

        if (stream.Documents.Count == 0 || stream.Documents[0].RootNode is not YamlMappingNode mapping)
        {
            return None;
        }

        var keyLines = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var entry in mapping.Children)
        {
            if (entry.Key is YamlScalarNode { Value: { } key })
            {
                // +1 for the opening delimiter the parsed text excludes.
                keyLines[key] = (int)entry.Key.Start.Line + 1;
            }
        }

        return new Frontmatter((JsonObject)Convert(mapping)!, 1, keyLines);
    }

    private static Frontmatter None => new(null, 1, new Dictionary<string, int>(StringComparer.Ordinal));

    private static string[] Lines(string text)
    {
        var lines = new List<string>();
        using var reader = new StringReader(text);

        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        return [.. lines];
    }

    private static JsonNode? Convert(YamlNode node) => node switch
    {
        YamlMappingNode mapping => ConvertMapping(mapping),
        YamlSequenceNode sequence => ConvertSequence(sequence),
        YamlScalarNode scalar => ConvertScalar(scalar),
        _ => null,
    };

    private static JsonNode ConvertMapping(YamlMappingNode mapping)
    {
        var result = new JsonObject();

        foreach (var entry in mapping.Children)
        {
            if (entry.Key is YamlScalarNode { Value: { } key })
            {
                result[key] = Convert(entry.Value);
            }
        }

        return result;
    }

    private static JsonNode ConvertSequence(YamlSequenceNode sequence)
    {
        var result = new JsonArray();

        foreach (var item in sequence.Children)
        {
            result.Add(Convert(item));
        }

        return result;
    }

    private static JsonNode? ConvertScalar(YamlScalarNode scalar)
    {
        var value = scalar.Value;

        if (value is null)
        {
            return null;
        }

        // A quoted scalar is a string by authorial intent, whatever it looks like.
        if (scalar.Style is ScalarStyle.SingleQuoted or ScalarStyle.DoubleQuoted)
        {
            return JsonValue.Create(value);
        }

        if (value is "null" or "~" or "")
        {
            return null;
        }

        if (value is "true" or "false")
        {
            return JsonValue.Create(value is "true");
        }

        // A leading zero is an identifier's zero-padding (0001), never an
        // integer's: reporting it as "integer but should be string" would send
        // the author to the wrong fix.
        if (value.Length > 1 && value[0] == '0')
        {
            return JsonValue.Create(value);
        }

        return long.TryParse(value, out var integer) ? JsonValue.Create(integer) : JsonValue.Create(value);
    }

    private const string Delimiter = "---";
}
