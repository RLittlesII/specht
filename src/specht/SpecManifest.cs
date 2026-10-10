using System.IO.Abstractions;
using System.Text.Json;
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
    /// <returns>The section contract, id grammars and frontmatter schema file names, every omitted value read as the default manifest's.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no file at the manifest path.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest is not well-formed JSON or not the manifest's shape.</exception>
    /// <exception cref="SpechtManifestException">
    /// The manifest carries a key the engine does not know, or a <c>schemaVersion</c> that is not an integer of at least 1.
    /// </exception>
    public static SpecStructure Load(IFileSystem fileSystem, string root)
    {
        if (!fileSystem.Directory.Exists(root))
        {
            throw new SpechtRootNotFoundException();
        }

        var path = fileSystem.Path.Combine(root, ".spec", "schema", "spec-structure.schema.json");

        if (!fileSystem.File.Exists(path))
        {
            throw new SpechtManifestNotFoundException();
        }

        try
        {
            return Read(fileSystem.File.ReadAllText(path));
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException)
        {
            throw new SpechtManifestUnreadableException(exception);
        }
    }

    private static SpecStructure Read(string text)
    {
        var manifest = Present(JsonNode.Parse(text)).AsObject();
        var unknown = manifest
            .Select(static entry => entry.Key)
            .Where(static key => !key.StartsWith('$') && !KnownKeys.Contains(key))
            .Select(static key => $"'{key}'")
            .ToList();

        if (unknown.Count > 0)
        {
            throw new SpechtManifestException(
                $"{RelativePath}: the engine does not know the key {string.Join(", ", unknown)}.");
        }

        return new SpecStructure(
            Strings(manifest["sections"] ?? Defaults["sections"]!),
            Tables(manifest["tables"] ?? Defaults["tables"]!),
            Filled(manifest, "identifiers"),
            SchemaVersion(manifest))
        {
            FrontmatterSchemas = Filled(manifest, "frontmatterSchemas"),
        };
    }

    private static Dictionary<string, string> Filled(JsonObject manifest, string key)
    {
        var values = Named(Defaults[key]!);

        if (manifest[key] is { } declared)
        {
            foreach (var (name, value) in Named(declared))
            {
                values[name] = value;
            }
        }

        return values;
    }

    private static int SchemaVersion(JsonObject manifest)
    {
        if (!manifest.TryGetPropertyValue("schemaVersion", out var node))
        {
            return 1;
        }

        if (node is JsonValue value && value.GetValueKind() == JsonValueKind.Number && value.TryGetValue<int>(out var version) && version >= 1)
        {
            return version;
        }

        throw new SpechtManifestException($"{RelativePath}: schemaVersion must be an integer of at least 1.");
    }

    private static List<string> Strings(JsonNode node) =>
        node.AsArray().Select(static element => Present(element).GetValue<string>()).ToList();

    private static Dictionary<string, IReadOnlyList<string>> Tables(JsonNode node) =>
        node.AsObject().ToDictionary(
            static entry => entry.Key,
            static entry => (IReadOnlyList<string>)Strings(Present(entry.Value)),
            StringComparer.Ordinal);

    private static Dictionary<string, string> Named(JsonNode node) =>
        node.AsObject().ToDictionary(
            static entry => entry.Key,
            static entry => Present(entry.Value).GetValue<string>(),
            StringComparer.Ordinal);

    private static JsonNode Present(JsonNode? node) =>
        node ?? throw new InvalidOperationException("A JSON null stands where the manifest's shape requires a value.");

    private static JsonObject ReadDefaults()
    {
        using var stream = typeof(SpecManifest).Assembly.GetManifestResourceStream(DefaultResource)!;

        return JsonNode.Parse(stream)!.AsObject();
    }

    private const string DefaultResource = "specht.default-manifest.json";

    private static readonly HashSet<string> KnownKeys = new(StringComparer.Ordinal)
    {
        "schemaVersion",
        "sections",
        "tables",
        "identifiers",
        "frontmatterSchemas",
    };

    private static readonly JsonObject Defaults = ReadDefaults();
}
