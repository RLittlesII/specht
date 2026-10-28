using System.Text;
using System.Text.Json;

namespace Specht.Manifest;

/// <summary>Edits one root rule level while preserving the surrounding manifest bytes.</summary>
/// <implements epic="0001" feature="F11" claim="B-001" />
public sealed class ManifestRuleEditor(ManifestPinEditor manifest)
{
    /// <summary>Replaces an existing level or inserts the missing rule entry or rules object.</summary>
    /// <implements epic="0001" feature="F11" claim="B-003" />
    /// <implements epic="0001" feature="F11" claim="B-004" />
    /// <implements epic="0001" feature="F11" claim="B-005" />
    /// <implements epic="0001" feature="F11" claim="B-012" />
    public byte[] Change(byte[] bytes, string id, string level)
    {
        var (encoding, preamble) = manifest.Format(bytes);
        var text = encoding.GetString(bytes.AsSpan(preamble));
        var utf8 = Encoding.UTF8.GetBytes(text);
        var reader = new Utf8JsonReader(utf8);
        var rootProperties = false;
        var inRules = false;
        var ruleProperties = false;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName && reader.CurrentDepth == 1)
            {
                rootProperties = true;
                if (reader.ValueTextEquals("rules"))
                {
                    reader.Read();
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        var value = $"{{{JsonSerializer.Serialize(id)}:{JsonSerializer.Serialize(level)}}}";
                        return Replace(bytes, utf8, text, encoding, preamble, reader.TokenStartIndex, reader.BytesConsumed, value);
                    }
                    inRules = true;
                }
            }
            else if (inRules && reader.TokenType == JsonTokenType.PropertyName && reader.CurrentDepth == 2)
            {
                ruleProperties = true;
                if (reader.ValueTextEquals(id))
                {
                    reader.Read();
                    if (reader.ValueTextEquals(level))
                    {
                        return bytes;
                    }
                    var start = reader.TokenStartIndex;
                    reader.Skip();
                    return Replace(bytes, utf8, text, encoding, preamble, start, reader.BytesConsumed, JsonSerializer.Serialize(level));
                }
            }
            else if (inRules && reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == 1)
            {
                var addition = $"{(ruleProperties ? "," : string.Empty)}{JsonSerializer.Serialize(id)}:{JsonSerializer.Serialize(level)}";
                return Replace(bytes, utf8, text, encoding, preamble, reader.TokenStartIndex, reader.TokenStartIndex, addition);
            }
            else if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == 0)
            {
                var addition = $"{(rootProperties ? "," : string.Empty)}\"rules\":{{{JsonSerializer.Serialize(id)}:{JsonSerializer.Serialize(level)}}}";
                return Replace(bytes, utf8, text, encoding, preamble, reader.TokenStartIndex, reader.TokenStartIndex, addition);
            }
        }
        throw new SpechtManifestException($"{SpecManifestStore.RelativePath}: expected a manifest object.");
    }

    /// <implements epic="0001" feature="F11" claim="B-003" />
    private byte[] Replace(byte[] bytes, byte[] utf8, string text, Encoding encoding, int preamble, long start, long end, string value)
    {
        var first = manifest.OriginalOffset(start, utf8, text, encoding, preamble);
        var last = manifest.OriginalOffset(end, utf8, text, encoding, preamble);
        return [.. bytes.AsSpan(0, first), .. encoding.GetBytes(value), .. bytes.AsSpan(last)];
    }
}
