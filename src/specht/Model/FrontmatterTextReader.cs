using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Specht.Model;

/// <summary>The shared text-only YAML representation reader used by the engine and compiler component.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
internal sealed class FrontmatterTextReader
{
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public FrontmatterText Read(string text)
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
            stream.Load(new StringReader(string.Join("\n", lines.Skip(1).Take(end - 1))));
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
                keyLines[key] = (int)entry.Key.Start.Line + 1;
            }
        }

        return new FrontmatterText((JsonObject)Convert(mapping)!, 1, keyLines);
    }

    private static FrontmatterText None => new(null, 1, new Dictionary<string, int>(StringComparer.Ordinal));

    /// <implements epic="0001" feature="F1" claim="B-004" />
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

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static JsonNode? Convert(YamlNode node) => node switch
    {
        YamlMappingNode mapping => ConvertMapping(mapping),
        YamlSequenceNode sequence => ConvertSequence(sequence),
        YamlScalarNode scalar => ConvertScalar(scalar),
        _ => null,
    };

    /// <implements epic="0001" feature="F1" claim="B-004" />
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

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static JsonNode ConvertSequence(YamlSequenceNode sequence)
    {
        var result = new JsonArray();

        foreach (var item in sequence.Children)
        {
            result.Add(Convert(item));
        }

        return result;
    }

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static JsonNode? ConvertScalar(YamlScalarNode scalar)
    {
        var value = scalar.Value;

        if (value is null)
        {
            return null;
        }

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

        if (value.Length > 1 && value[0] == '0')
        {
            return JsonValue.Create(value);
        }

        return long.TryParse(value, out var integer) ? JsonValue.Create(integer) : JsonValue.Create(value);
    }

    private const string Delimiter = "---";
}
