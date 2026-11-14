using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Specht.Identifiers;
using Specht.Versioning;

namespace Specht.Manifest;

/// <summary>
/// Moves a manifest's top-level <c>mint</c> and its <c>identifiers</c> members for the <c>0001-F19</c> C-13 types into
/// <c>ids</c> records, once, under a pin of schema 0.4.0 or later (0001-F7 B-155 to B-161, C-19): a grammar equal to a shipped
/// default below 0.4.0 is dropped, any other is split into its record's <c>plain</c> and <c>id</c>, a <c>mint</c> entry
/// becomes its record's <c>mint</c> less its <c>prefix</c>, a <c>mint</c> entry for any other name is refused, and every byte
/// outside the moved members is kept.
/// </summary>
/// <param name="manifest">The pin editor whose encoding detection this edit shares.</param>
/// <param name="versions">The shipped versions whose manifests hold the defaults and the records.</param>
/// <implements epic="0001" feature="F7" claim="B-155" />
/// <implements epic="0001" feature="F7" claim="B-156" />
/// <implements epic="0001" feature="F7" claim="B-157" />
/// <implements epic="0001" feature="F7" claim="B-158" />
/// <implements epic="0001" feature="F7" claim="B-159" />
/// <implements epic="0001" feature="F7" claim="B-160" />
/// <implements epic="0001" feature="F7" claim="B-161" />
public sealed class ManifestIdMover(ManifestPinEditor manifest, SchemaVersions versions)
{
    /// <summary>Moves the old id settings of a manifest's bytes, keeping its BOM and its encoding.</summary>
    /// <param name="bytes">The manifest, as read.</param>
    /// <returns><paramref name="bytes"/> itself when nothing moves; otherwise a new array.</returns>
    /// <exception cref="SpechtManifestException">A setting cannot be moved exactly (B-159).</exception>
    /// <implements epic="0001" feature="F7" claim="B-158" />
    /// <implements epic="0001" feature="F7" claim="B-160" />
    public byte[] Move(byte[] bytes)
    {
        var (encoding, preamble) = manifest.Format(bytes);
        var text = encoding.GetString(bytes.AsSpan(preamble));
        var moved = Move(text);
        return ReferenceEquals(moved, text) ? bytes : [.. bytes.AsSpan(0, preamble), .. encoding.GetBytes(moved)];
    }

    /// <summary>Moves the old id settings of a manifest's text.</summary>
    /// <param name="text">The manifest's text.</param>
    /// <returns><paramref name="text"/> itself when nothing moves; otherwise the moved text.</returns>
    /// <exception cref="SpechtManifestException">A setting cannot be moved exactly (B-159).</exception>
    /// <exception cref="JsonException">The text is not JSON.</exception>
    /// <implements epic="0001" feature="F7" claim="B-155" />
    /// <implements epic="0001" feature="F7" claim="B-156" />
    /// <implements epic="0001" feature="F7" claim="B-157" />
    /// <implements epic="0001" feature="F7" claim="B-159" />
    /// <implements epic="0001" feature="F7" claim="B-160" />
    /// <implements epic="0001" feature="F7" claim="B-161" />
    public string Move(string text)
    {
        if (JsonNode.Parse(text) is not JsonObject root || Shipped(root) is not { } defaults || defaults["ids"] is not JsonObject shipped)
        {
            return text;
        }

        var identifiers = root["identifiers"] as JsonObject ?? [];
        var mint = root["mint"] as JsonObject ?? [];
        var declared = root["ids"] as JsonObject ?? [];
        var types = IdTypes.RepositoryWide.Concat(IdTypes.PerFeature).ToList();
        if (!root.ContainsKey("mint") && !identifiers.Any(member => types.Contains(member.Key)))
        {
            return text;
        }

        var faults = new List<string>();
        var records = new List<(string Type, string? Plain, string? Id, bool Mint)>();
        var heads = new Dictionary<string, (string Head, string? Member, bool Moved)>(StringComparer.Ordinal);
        var refused = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in types)
        {
            var grammar = identifiers.ContainsKey(type);
            var entry = mint.ContainsKey(type);
            if (!grammar && !entry)
            {
                var held = Head((declared[type] as JsonObject)?["plain"]) ?? Head(shipped[type]?["plain"]) ?? string.Empty;
                heads[type] = (held, declared.ContainsKey(type) ? $"ids.{type}" : null, false);
                continue;
            }

            if (declared.ContainsKey(type))
            {
                var members = new[] { grammar ? $"identifiers.{type}" : null, entry ? $"mint.{type}" : null }.OfType<string>();
                faults.Add($"ids.{type} already holds a record, so {string.Join(" and ", members)} cannot move into it; move it by hand");
                refused.Add(type);
                continue;
            }

            string? plain = null;
            string? id = null;
            var value = grammar ? Text(identifiers[type]) : null;
            if (grammar && (value is null || !Defaults(type).Contains(value)))
            {
                if (value is null || Split(value) is not { } split)
                {
                    faults.Add(
                        $"identifiers.{type} {Quoted(identifiers[type])} is not ^, a literal head, a pattern with no alternation outside a group, "
                        + $"and $; write ids.{type} by hand");
                    refused.Add(type);
                    continue;
                }

                (plain, id) = (split.Head + SpechtIdRecord.IdPlaceholder, split.Pattern);
            }

            var head = Head(plain) ?? Head(shipped[type]?["plain"]);
            heads[type] = (head ?? string.Empty, grammar ? $"identifiers.{type}" : null, plain is not null);
            if (entry && head is not null && mint[type] is JsonObject settings && settings.ContainsKey("prefix")
                && !string.Equals(Text(settings["prefix"]), head, StringComparison.Ordinal))
            {
                faults.Add($"mint.{type}.prefix {Quoted(settings["prefix"])} is not the head '{head}' of ids.{type}.plain after the move");
            }

            if (plain is not null || entry)
            {
                records.Add((type, plain, id, entry));
            }
        }

        faults.AddRange(mint.Select(static entry => entry.Key)
            .Where(name => !name.StartsWith('$') && !types.Contains(name))
            .Select(name => $"mint.{name} names none of the id types {string.Join(", ", types)}, so it has no record to move into; remove it by hand"));
        if (!refused.Contains("claim") && !refused.Contains("scenario") && heads.TryGetValue("claim", out var claim)
            && heads.TryGetValue("scenario", out var scenario) && (claim.Moved || scenario.Moved)
            && string.Equals(claim.Head, scenario.Head, Comparison(root, defaults)))
        {
            var members = new[] { claim.Member, scenario.Member }.OfType<string>();
            faults.Add($"{string.Join(" and ", members)} would give the claim and scenario records the one head '{claim.Head}'");
        }

        if (faults.Count > 0)
        {
            throw new SpechtManifestException($"{SpechtManifestStore.RelativePath}: {string.Join("; ", faults)}.");
        }

        return Edit(text, types, records);
    }

    /// <implements epic="0001" feature="F7" claim="B-159" />
    private static StringComparison Comparison(JsonObject root, JsonObject defaults) =>
        ((root["claim"] as JsonObject)?["tagIgnoreCase"] ?? defaults["claim"]?["tagIgnoreCase"]) is JsonValue ignore
        && ignore.TryGetValue<bool>(out var value) && value
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    /// <implements epic="0001" feature="F7" claim="B-155" />
    private static (string Head, string Pattern)? Split(string grammar)
    {
        if (grammar.Length < 2 || grammar[0] != '^' || grammar[^1] != '$')
        {
            return null;
        }

        var escapes = 0;
        for (var at = grammar.Length - 2; at > 0 && grammar[at] == '\\'; at--)
        {
            escapes++;
        }

        if (escapes % 2 != 0)
        {
            return null;
        }

        var body = grammar[1..^1];
        var run = 0;
        while (run < body.Length && !"\\.+*?()[]{}|^$".Contains(body[run], StringComparison.Ordinal))
        {
            run++;
        }

        if (run > 0 && Quantifies(body, run))
        {
            run--;
        }

        var pattern = body[run..];
        return pattern.Length == 0 || Alternates(pattern) ? null : (body[..run], pattern);
    }

    /// <implements epic="0001" feature="F7" claim="B-155" />
    private static bool Quantifies(string body, int at)
    {
        if (at >= body.Length)
        {
            return false;
        }

        if (body[at] is '*' or '+' or '?')
        {
            return true;
        }

        if (body[at] != '{')
        {
            return false;
        }

        var next = Digits(body, at + 1);
        if (next == at + 1)
        {
            return false;
        }

        if (next < body.Length && body[next] == ',')
        {
            next = Digits(body, next + 1);
        }

        return next < body.Length && body[next] == '}';
    }

    /// <implements epic="0001" feature="F7" claim="B-155" />
    private static int Digits(string body, int at)
    {
        while (at < body.Length && char.IsAsciiDigit(body[at]))
        {
            at++;
        }

        return at;
    }

    /// <implements epic="0001" feature="F7" claim="B-155" />
    private static bool Alternates(string pattern)
    {
        var depth = 0;
        for (var at = 0; at < pattern.Length; at++)
        {
            switch (pattern[at])
            {
                case '\\':
                    at++;
                    break;
                case '[':
                    at++;
                    if (at < pattern.Length && pattern[at] == '^')
                    {
                        at++;
                    }

                    if (at < pattern.Length && pattern[at] == ']')
                    {
                        at++;
                    }

                    while (at < pattern.Length && pattern[at] != ']')
                    {
                        at += pattern[at] == '\\' ? 2 : 1;
                    }

                    break;
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    break;
                case '|' when depth == 0:
                    return true;
            }
        }

        return false;
    }

    /// <implements epic="0001" feature="F7" claim="B-155" />
    /// <implements epic="0001" feature="F7" claim="B-156" />
    /// <implements epic="0001" feature="F7" claim="B-157" />
    /// <implements epic="0001" feature="F7" claim="B-158" />
    private static string Edit(string text, List<string> types, List<(string Type, string? Plain, string? Id, bool Mint)> records)
    {
        var utf8 = Encoding.UTF8.GetBytes(text);
        var top = Members(utf8, 0);
        var edits = Removals(top.Members, top.Open, top.Close, static member => member.Name == "mint");
        var identifiers = top.Members.FindIndex(static member => member.Name == "identifiers");
        if (identifiers >= 0 && utf8[top.Members[identifiers].Value] == (byte) '{')
        {
            var inner = Members(utf8, top.Members[identifiers].Value);
            edits.AddRange(Removals(inner.Members, inner.Open, inner.Close, member => types.Contains(member.Name)));
        }

        if (records.Count > 0)
        {
            var at = top.Members.FindIndex(static member => member.Name == "mint");
            var mint = at >= 0 && utf8[top.Members[at].Value] == (byte) '{' ? Members(utf8, top.Members[at].Value).Members : [];
            var written = records.Select(record => Record(utf8, record, mint)).ToList();
            var ids = top.Members.FindIndex(static member => member.Name == "ids");
            if (ids >= 0)
            {
                var held = Members(utf8, top.Members[ids].Value);
                edits.Add(held.Members.Count == 0
                    ? (held.Open, held.Open, "\n    " + string.Join(",\n    ", written) + "\n  ")
                    : (held.Members[^1].End, held.Members[^1].End, ",\n    " + string.Join(",\n    ", written)));
            }
            else
            {
                var (_, _, _, end) = top.Members.FindLast(static member => member.Name != "mint");
                edits.Add((end, end, ",\n  \"ids\": {\n    " + string.Join(",\n    ", written) + "\n  }"));
            }
        }

        var result = new List<byte>(utf8.Length);
        var cursor = 0;
        foreach (var (start, end, replacement) in edits.OrderBy(static edit => edit.Start).ThenBy(static edit => edit.End))
        {
            result.AddRange(utf8.AsSpan(cursor, start - cursor));
            result.AddRange(Encoding.UTF8.GetBytes(replacement));
            cursor = end;
        }

        result.AddRange(utf8.AsSpan(cursor));
        return Encoding.UTF8.GetString([.. result]);
    }

    /// <implements epic="0001" feature="F7" claim="B-157" />
    /// <implements epic="0001" feature="F7" claim="B-158" />
    private static List<(int Start, int End, string Replacement)> Removals(
        List<(string Name, int Start, int Value, int End)> members, int open, int close, Func<(string Name, int Start, int Value, int End), bool> removed)
    {
        var edits = new List<(int Start, int End, string Replacement)>();
        if (members.Count > 0 && members.TrueForAll(member => removed(member)))
        {
            edits.Add((open, close, string.Empty));
            return edits;
        }

        for (var first = 0; first < members.Count; first++)
        {
            if (!removed(members[first]))
            {
                continue;
            }

            var last = first;
            while (last + 1 < members.Count && removed(members[last + 1]))
            {
                last++;
            }

            edits.Add(first > 0
                ? (members[first - 1].End, members[last].End, string.Empty)
                : (members[first].Start, members[last + 1].Start, string.Empty));
            first = last;
        }

        return edits;
    }

    /// <implements epic="0001" feature="F7" claim="B-155" />
    /// <implements epic="0001" feature="F7" claim="B-156" />
    private static string Record(
        byte[] utf8, (string Type, string? Plain, string? Id, bool Mint) record, List<(string Name, int Start, int Value, int End)> mint)
    {
        var members = new List<string>();
        if (record.Plain is not null)
        {
            members.Add($"\"plain\": {Encoded(record.Plain)}");
            members.Add($"\"id\": {Encoded(record.Id!)}");
        }

        if (record.Mint)
        {
            var (_, _, start, end) = mint.Find(member => member.Name == record.Type);
            var value = utf8[start] == (byte) '{' ? Settings(utf8, start) : Encoding.UTF8.GetString(utf8.AsSpan(start, end - start));
            members.Add($"\"mint\": {value}");
        }

        return $"{Encoded(record.Type)}: {{\n      {string.Join(",\n      ", members)}\n    }}";
    }

    /// <implements epic="0001" feature="F7" claim="B-156" />
    private static string Settings(byte[] utf8, int at)
    {
        var kept = Members(utf8, at).Members
            .Where(static member => member.Name != "prefix")
            .Select(member => Encoding.UTF8.GetString(utf8.AsSpan(member.Start, member.End - member.Start)))
            .ToList();
        return kept.Count == 0 ? "{}" : "{\n        " + string.Join(",\n        ", kept) + "\n      }";
    }

    /// <implements epic="0001" feature="F7" claim="B-158" />
    private static (List<(string Name, int Start, int Value, int End)> Members, int Open, int Close) Members(byte[] utf8, int at)
    {
        var reader = new Utf8JsonReader(utf8.AsSpan(at));
        reader.Read();
        var open = at + checked((int) reader.BytesConsumed);
        var members = new List<(string Name, int Start, int Value, int End)>();
        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            var name = reader.GetString()!;
            var start = at + checked((int) reader.TokenStartIndex);
            reader.Read();
            var value = at + checked((int) reader.TokenStartIndex);
            reader.Skip();
            members.Add((name, start, value, at + checked((int) reader.BytesConsumed)));
        }

        return (members, open, at + checked((int) reader.TokenStartIndex));
    }

    /// <implements epic="0001" feature="F7" claim="B-155" />
    private static string Encoded(string value) => $"\"{JsonEncodedText.Encode(value, JavaScriptEncoder.UnsafeRelaxedJsonEscaping)}\"";

    /// <implements epic="0001" feature="F7" claim="B-155" />
    private static string? Head(JsonNode? plain) => Head(Text(plain));

    /// <implements epic="0001" feature="F7" claim="B-155" />
    private static string? Head(string? plain) =>
        plain is not null && plain.EndsWith(SpechtIdRecord.IdPlaceholder, StringComparison.Ordinal) ? plain[..^SpechtIdRecord.IdPlaceholder.Length] : null;

    /// <implements epic="0001" feature="F7" claim="B-159" />
    private static string? Text(JsonNode? node) => node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    /// <implements epic="0001" feature="F7" claim="B-159" />
    private static string Quoted(JsonNode? node) => Text(node) is { } text ? $"'{text}'" : node?.ToJsonString() ?? "null";

    /// <implements epic="0001" feature="F7" claim="B-160" />
    private JsonObject? Shipped(JsonObject root)
    {
        if (root["schemaVersion"] is not JsonValue pin || !pin.TryGetValue<string>(out var value)
            || !SemanticVersion.TryParse(value, out var number) || number.CompareTo(SpechtManifestParser.IdsVersion) < 0
            || versions.Find(number) is not { } version || (root.ContainsKey("ids") && root["ids"] is not JsonObject))
        {
            return null;
        }

        return JsonNode.Parse(version.Manifest) as JsonObject;
    }

    /// <implements epic="0001" feature="F7" claim="B-161" />
    private HashSet<string> Defaults(string type) =>
        versions.Versions
            .Where(static version => version.Number.CompareTo(SpechtManifestParser.IdsVersion) < 0)
            .Select(version => Text(JsonNode.Parse(version.Manifest)?["identifiers"]?[type]))
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
}
