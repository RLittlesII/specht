using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Manifest;

/// <summary>Validates non-version settings and edits only the root pin's bytes (0001-F7 C-13).</summary>
/// <param name="parser">The existing manifest parser.</param>
/// <param name="versions">The shipped catalog used for validation and repair.</param>
/// <implements epic="0001" feature="F7" claim="B-043" />
public sealed class ManifestPinEditor(SpecManifestParser parser, SchemaVersions versions)
{
    /// <summary>Decodes the BOM encodings accepted by the existing manifest reader, or UTF-8 without a BOM.</summary>
    /// <implements epic="0001" feature="F7" claim="B-043" />
    public string Decode(byte[] bytes)
    {
        var (encoding, offset) = Format(bytes);
        return encoding.GetString(bytes.AsSpan(offset));
    }

    /// <summary>Validates settings under the current shipped version, or newest when that pin is unusable.</summary>
    /// <implements epic="0001" feature="F7" claim="B-043" />
    public void Validate(byte[] bytes)
    {
        try
        {
            var manifest = JsonNode.Parse(Decode(bytes))?.AsObject()
                ?? throw new InvalidOperationException("The manifest must be a JSON object.");
            var current = new SemanticVersion(0, 1, 0);
            if (manifest.ContainsKey("schemaVersion"))
            {
                var value = manifest["schemaVersion"] as JsonValue;
                if (value is null || !value.TryGetValue<string>(out var text)
                    || !SemanticVersion.TryParse(text, out current) || versions.Find(current) is null)
                {
                    current = versions.Versions[^1].Number;
                }
            }
            manifest["schemaVersion"] = current.ToString();
            _ = parser.PinnedVersion(parser.Parse(manifest.ToJsonString()));
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException or ArgumentException)
        {
            throw new SpechtManifestUnreadableException(exception);
        }
    }

    /// <summary>Replaces only the root pin value, or adds the missing key without changing other values.</summary>
    /// <implements epic="0001" feature="F7" claim="B-043" />
    public byte[] Change(byte[] bytes, SemanticVersion target)
    {
        var (encoding, offset) = Format(bytes);
        var text = encoding.GetString(bytes.AsSpan(offset));
        var utf8 = Encoding.UTF8.GetBytes(text);
        var reader = new Utf8JsonReader(utf8);
        var properties = false;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName && reader.CurrentDepth == 1)
            {
                properties = true;
                if (reader.ValueTextEquals("schemaVersion"))
                {
                    reader.Read();
                    var start = OriginalOffset(reader.TokenStartIndex, utf8, text, encoding, offset);
                    reader.Skip();
                    var end = OriginalOffset(reader.BytesConsumed, utf8, text, encoding, offset);
                    return [.. bytes.AsSpan(0, start), .. encoding.GetBytes(JsonSerializer.Serialize(target.ToString())), .. bytes.AsSpan(end)];
                }
            }
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == 0)
            {
                var end = OriginalOffset(reader.TokenStartIndex, utf8, text, encoding, offset);
                var addition = encoding.GetBytes($"{(properties ? "," : string.Empty)}\"schemaVersion\":\"{target}\"");
                return [.. bytes.AsSpan(0, end), .. addition, .. bytes.AsSpan(end)];
            }
        }
        throw new SpechtManifestException($"{SpecManifestStore.RelativePath}: expected a manifest object.");
    }

    /// <implements epic="0001" feature="F7" claim="B-043" />
    internal int OriginalOffset(long utf8Offset, byte[] utf8, string text, Encoding encoding, int preamble) =>
        preamble + encoding.GetByteCount(text.AsSpan(0, Encoding.UTF8.GetCharCount(utf8.AsSpan(0, checked((int)utf8Offset)))));

    /// <implements epic="0001" feature="F7" claim="B-043" />
    internal (Encoding Encoding, int Offset) Format(byte[] bytes)
    {
        ReadOnlySpan<byte> value = bytes;
        if (value.StartsWith(new byte[] { 0xFF, 0xFE, 0x00, 0x00 }))
        {
            return (Encoding.UTF32, 4);
        }
        if (value.StartsWith(new byte[] { 0x00, 0x00, 0xFE, 0xFF }))
        {
            return (new UTF32Encoding(bigEndian: true, byteOrderMark: true), 4);
        }
        if (value.StartsWith(new byte[] { 0xFF, 0xFE }))
        {
            return (Encoding.Unicode, 2);
        }
        if (value.StartsWith(new byte[] { 0xFE, 0xFF }))
        {
            return (Encoding.BigEndianUnicode, 2);
        }
        return (Encoding.UTF8, value.StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }) ? 3 : 0);
    }
}
