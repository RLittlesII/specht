using System.Text;
using System.Text.Json;

namespace Specht.Manifest;

/// <summary>
/// Adds the top-level keys a shipped manifest defines and a consumer's lacks, copying each shipped member's text and keeping
/// every byte already there (0001-F7 B-016, B-052, C-13); <c>identifiers</c> is never added, so an upgrade never opts a
/// manifest into the epic grammar (0001-F7 B-138, OQ-68; 0001-F5 decision 0004).
/// </summary>
/// <param name="manifest">The pin editor whose encoding detection and offset arithmetic this edit shares.</param>
/// <implements epic="0001" feature="F7" claim="B-016" />
/// <implements epic="0001" feature="F7" claim="B-138" />
public sealed class ManifestKeyEditor(ManifestPinEditor manifest)
{
    /// <summary>
    /// Appends each defined key the consumer's manifest lacks, in shipped order, after its last member; a defined key is a
    /// top-level member other than <c>schemaVersion</c> and <c>identifiers</c> whose name does not begin with <c>$</c>.
    /// </summary>
    /// <param name="bytes">The consumer's manifest, as read.</param>
    /// <param name="shipped">The shipped manifest, UTF-8 without a BOM.</param>
    /// <returns><paramref name="bytes"/> itself when no defined key is missing; otherwise a new array.</returns>
    /// <exception cref="SpechtManifestException">Either root is not a JSON object.</exception>
    /// <implements epic="0001" feature="F7" claim="B-016" />
    /// <implements epic="0001" feature="F7" claim="B-052" />
    /// <implements epic="0001" feature="F7" claim="B-138" />
    public byte[] AddMissing(byte[] bytes, byte[] shipped)
    {
        var (encoding, preamble) = manifest.Format(bytes);
        var text = encoding.GetString(bytes.AsSpan(preamble));
        var utf8 = Encoding.UTF8.GetBytes(text);
        var (present, insert) = Members(utf8);
        var names = present.Select(static member => member.Name).ToHashSet(StringComparer.Ordinal);
        var missing = Members(shipped).Members
            .Where(member => member.Name is not ("schemaVersion" or "identifiers") && !member.Name.StartsWith('$') && !names.Contains(member.Name))
            .ToList();
        if (missing.Count == 0)
        {
            return bytes;
        }

        var addition = new StringBuilder();
        foreach (var member in missing)
        {
            addition.Append(addition.Length == 0 && present.Count == 0 ? "\n  " : ",\n  ")
                .Append(Encoding.UTF8.GetString(shipped.AsSpan(checked((int)member.Start), checked((int)(member.End - member.Start)))));
        }

        var at = manifest.OriginalOffset(insert, utf8, text, encoding, preamble);
        return [.. bytes.AsSpan(0, at), .. encoding.GetBytes(addition.ToString()), .. bytes.AsSpan(at)];
    }

    /// <implements epic="0001" feature="F7" claim="B-016" />
    private static (List<(string Name, long Start, long End)> Members, long Insert) Members(byte[] utf8)
    {
        var reader = new Utf8JsonReader(utf8);
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            throw new SpechtManifestException($"{SpecManifestStore.RelativePath}: expected a manifest object.");
        }

        var members = new List<(string Name, long Start, long End)>();
        var insert = reader.BytesConsumed;
        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            var name = reader.GetString()!;
            var start = reader.TokenStartIndex;
            reader.Read();
            reader.Skip();
            insert = reader.BytesConsumed;
            members.Add((name, start, insert));
        }

        return (members, insert);
    }
}
