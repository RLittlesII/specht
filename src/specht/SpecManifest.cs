using System.IO.Abstractions;
using System.Text.Json.Nodes;

namespace specht;

/// <summary>
/// Reads the manifest, <c>.spec/schema/spec-structure.schema.json</c>, whole: every key is checked before a structure is
/// returned, so no rule ever sees a manifest that was not accepted (<c>0001-F5</c> C-5).
/// </summary>
/// <remarks>
/// The manifest is always read from the root's file (C-6). The default manifest (A-4) is this repository's own manifest,
/// embedded into the engine from that same file rather than copied, and supplies only the values a manifest leaves out
/// (B-019); it never stands in for a missing file.
/// </remarks>
public static class SpecManifest
{
    /// <summary>The manifest's path under a root, as a message names it.</summary>
    public const string RelativePath = ".spec/schema/spec-structure.schema.json";

    /// <summary>Reads, checks and fills the manifest under <paramref name="root"/>.</summary>
    /// <param name="fileSystem">The file system the manifest is read through.</param>
    /// <param name="root">The repository root.</param>
    /// <returns>The section contract and id grammars, every omitted value read as the default manifest's.</returns>
    /// <exception cref="SpecManifestException">The manifest carries a key the engine does not know.</exception>
    public static SpecStructure Load(IFileSystem fileSystem, string root)
    {
        var path = fileSystem.Path.Combine(root, ".spec", "schema", "spec-structure.schema.json");
        var manifest = JsonNode.Parse(fileSystem.File.ReadAllText(path))!.AsObject();
        var unknown = manifest
            .Select(static entry => entry.Key)
            .Where(static key => !key.StartsWith('$') && !KnownKeys.Contains(key))
            .Select(static key => $"'{key}'")
            .ToList();

        if (unknown.Count > 0)
        {
            throw new SpecManifestException(
                $"{RelativePath}: the engine does not know the key {string.Join(", ", unknown)}.");
        }

        var identifiers = Grammars(Defaults["identifiers"]!);

        if (manifest["identifiers"] is { } declared)
        {
            foreach (var (name, grammar) in Grammars(declared))
            {
                identifiers[name] = grammar;
            }
        }

        return new SpecStructure(
            Strings(manifest["sections"] ?? Defaults["sections"]!),
            Tables(manifest["tables"] ?? Defaults["tables"]!),
            identifiers);
    }

    private static List<string> Strings(JsonNode node) =>
        node.AsArray().Select(static element => element!.GetValue<string>()).ToList();

    private static Dictionary<string, IReadOnlyList<string>> Tables(JsonNode node) =>
        node.AsObject().ToDictionary(
            static entry => entry.Key,
            static entry => (IReadOnlyList<string>)Strings(entry.Value!),
            StringComparer.Ordinal);

    private static Dictionary<string, string> Grammars(JsonNode node) =>
        node.AsObject().ToDictionary(
            static entry => entry.Key,
            static entry => entry.Value!.GetValue<string>(),
            StringComparer.Ordinal);

    private static JsonObject ReadDefaults()
    {
        using var stream = typeof(SpecManifest).Assembly.GetManifestResourceStream(DefaultResource)!;

        return JsonNode.Parse(stream)!.AsObject();
    }

    private const string DefaultResource = "specht.default-manifest.json";

    private static readonly HashSet<string> KnownKeys = new(StringComparer.Ordinal) { "sections", "tables", "identifiers" };

    private static readonly JsonObject Defaults = ReadDefaults();
}
