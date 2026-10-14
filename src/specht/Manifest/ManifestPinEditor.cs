using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Manifest;

/// <summary>Validates non-version settings and edits only the root pin's bytes (0001-F7 C-13).</summary>
/// <param name="parser">The existing manifest parser.</param>
/// <param name="versions">The shipped catalog used for validation and repair.</param>
public sealed class ManifestPinEditor(SpecManifestParser parser, SchemaVersions versions)
{
    /// <summary>Validates settings under the current shipped version, or newest when that pin is unusable.</summary>
    public void Validate(byte[] bytes)
    {
        try
        {
            var manifest = JsonNode.Parse(Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF'))?.AsObject()
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
    public byte[] Change(byte[] bytes, SemanticVersion target)
    {
        var offset = bytes.AsSpan().StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }) ? 3 : 0;
        var reader = new Utf8JsonReader(bytes.AsSpan(offset));
        var properties = false;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName && reader.CurrentDepth == 1)
            {
                properties = true;
                if (reader.ValueTextEquals("schemaVersion"))
                {
                    reader.Read();
                    var start = checked((int)reader.TokenStartIndex) + offset;
                    reader.Skip();
                    var end = checked((int)reader.BytesConsumed) + offset;
                    return [.. bytes.AsSpan(0, start), .. JsonSerializer.SerializeToUtf8Bytes(target.ToString()), .. bytes.AsSpan(end)];
                }
            }
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == 0)
            {
                var end = checked((int)reader.TokenStartIndex) + offset;
                var addition = Encoding.UTF8.GetBytes($"{(properties ? "," : string.Empty)}\"schemaVersion\":\"{target}\"");
                return [.. bytes.AsSpan(0, end), .. addition, .. bytes.AsSpan(end)];
            }
        }
        throw new SpechtManifestException($"{SpecManifestStore.RelativePath}: expected a manifest object.");
    }
}
