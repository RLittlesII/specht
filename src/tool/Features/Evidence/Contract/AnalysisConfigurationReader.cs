using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Specht.Tool.Features.Evidence.Contract;

/// <summary>Projects explicitly mapped consumer fields without interpreting a schema version.</summary>
public sealed class AnalysisConfigurationReader
{
    public JsonObject Read(JsonNode configuration, IReadOnlyDictionary<string, string> mappings)
    {
        var result = new JsonObject();
        foreach (var mapping in mappings.OrderBy(static entry => entry.Key, StringComparer.Ordinal))
        {
            var target = Segments(mapping.Key);
            var source = Segments(mapping.Value);
            if (target.Count(static part => part == "*") != source.Count(static part => part == "*"))
            {
                throw new ArgumentException("Explicit mappings must preserve array dimensions.");
            }

            var wildcard = System.Array.IndexOf(target, "*");
            if (wildcard == 0)
            {
                throw new ArgumentException("A normalized analysis mapping must select an object field.");
            }

            if (wildcard > 0)
            {
                var prefix = target.Take(wildcard).ToArray();
                var prefixPointer = JsonPointer.Parse(string.Concat(prefix.Select(static part => "/" + part.Replace("~", "~0").Replace("/", "~1"))));
                if (!prefixPointer.TryEvaluate(result, out _))
                {
                    Set(result, prefix, 0, Array.Empty<int>(), 0, new JsonArray());
                }
            }

            foreach (var value in Values(configuration, source, 0, Array.Empty<int>()))
            {
                Set(result, target, 0, value.Key, 0, value.Value);
            }
        }

        return result;
    }

    private static string[] Segments(string pointer)
    {
        if (!pointer.StartsWith("/", StringComparison.Ordinal) || pointer.Length < 2 || !JsonPointer.TryParse(pointer, out _))
        {
            throw new ArgumentException("An explicit mapping must be a nonempty JSON Pointer.");
        }

        return pointer.Substring(1).Split('/').Select(static part => part.Replace("~1", "/").Replace("~0", "~")).ToArray();
    }

    private static IEnumerable<KeyValuePair<int[], JsonNode?>> Values(JsonNode? node, string[] parts, int position, int[] indices)
    {
        if (position == parts.Length)
        {
            yield return new KeyValuePair<int[], JsonNode?>(indices, node);
            yield break;
        }

        var part = parts[position];
        if (part == "*" && node is JsonArray array)
        {
            for (var index = 0; index < array.Count; index++)
            {
                foreach (var value in Values(array[index], parts, position + 1, indices.Concat(new[] { index }).ToArray()))
                {
                    yield return value;
                }
            }
        }
        else if (node is not null && JsonPointer.Create(part).TryEvaluate(node, out var child))
        {
            foreach (var value in Values(child, parts, position + 1, indices))
            {
                yield return value;
            }
        }
        else
        {
            throw new ArgumentException("A configured source field is missing or has the wrong shape.");
        }
    }

    private static void Set(JsonNode container, string[] parts, int position, int[] indices, int dimension, JsonNode? value)
    {
        var part = parts[position];
        var last = position == parts.Length - 1;
        if (part == "*" && container is JsonArray array)
        {
            var index = indices[dimension];
            while (array.Count <= index)
            {
                array.Add(null);
            }

            if (last)
            {
                array[index] = value?.DeepClone();
            }
            else
            {
                array[index] ??= parts[position + 1] == "*" ? new JsonArray() : new JsonObject();
                Set(array[index]!, parts, position + 1, indices, dimension + 1, value);
            }
        }
        else if (container is JsonObject obj)
        {
            if (last)
            {
                obj[part] = value?.DeepClone();
            }
            else
            {
                obj[part] ??= parts[position + 1] == "*" ? new JsonArray() : new JsonObject();
                Set(obj[part]!, parts, position + 1, indices, dimension, value);
            }
        }
        else
        {
            throw new ArgumentException("Explicit mappings have conflicting target shapes.");
        }
    }
}
