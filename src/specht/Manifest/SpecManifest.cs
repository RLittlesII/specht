using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Specht.Discovery;
using Specht.Versioning;

namespace Specht.Manifest;

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

    /// <summary>Reads, checks, and fills the manifest under <paramref name="root"/>.</summary>
    /// <param name="fileSystem">The file system the manifest is read through.</param>
    /// <param name="root">The repository root.</param>
    /// <returns>
    /// The section contract, id grammars, discovery inputs and schema file names, every omitted value read as the default
    /// manifest, and the rule settings as the manifest wrote them (<c>0001-F5</c> B-044); or a <see cref="RootNotFound"/>
    /// when <paramref name="root"/> is not a directory, a <see cref="ManifestNotFound"/> when there is no file at the
    /// manifest path, a <see cref="ManifestUnreadable"/> when the manifest is not well-formed JSON or not the manifest's
    /// shape, and a <see cref="ManifestRejected"/> when the manifest carries a key the engine does not know, a
    /// <c>schemaVersion</c> that is not a <c>major.minor.patch</c> string, a role naming a title <c>sections</c> does not
    /// list, a <c>tables</c> key that is not a role, a marker whose text is empty, an empty <c>taskFiles</c>,
    /// <c>epicFiles</c> or <c>companionFiles</c> list, an <c>exclusions</c> entry with a <c>/</c> inside it and no leading
    /// <c>/</c>, a <c>layouts</c> entry whose <c>identity</c> holds a member that is not a non-negative integer, a member
    /// other than <c>epic</c> and <c>feature</c>, or neither of the two, or a <c>rules</c> entry whose value is not
    /// <c>error</c>, <c>warning</c> or <c>off</c>.
    /// </returns>
    public static Outcome<SpecStructure> Load(IFileSystem fileSystem, string root)
    {
        if (!fileSystem.Directory.Exists(root))
        {
            return new RootNotFound();
        }

        var path = fileSystem.Path.Combine(root, ".spec", "schema", "spec-structure.schema.json");

        if (!fileSystem.File.Exists(path))
        {
            return new ManifestNotFound();
        }

        return Parsed(fileSystem.File.ReadAllText(path)) is { } manifest ? Read(manifest) : new ManifestUnreadable();
    }

    private static JsonObject? Parsed(string text)
    {
        try
        {
            return JsonNode.Parse(text) as JsonObject;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static Outcome<SpecStructure> Read(JsonObject manifest)
    {
        var unknown = manifest
            .Select(static entry => entry.Key)
            .Where(static key => !key.StartsWith('$') && !KnownKeys.Contains(key))
            .Select(static key => $"'{key}'")
            .ToList();

        if (unknown.Count > 0)
        {
            return new ManifestRejected($"{RelativePath}: the engine does not know the key {string.Join(", ", unknown)}.");
        }

        if (Strings(manifest["sections"] ?? Defaults["sections"]!) is not { } sections
            || Tables(manifest["tables"] ?? Defaults["tables"]!) is not { } tables
            || Filled(manifest, "identifiers") is not { } identifiers)
        {
            return new ManifestUnreadable();
        }

        return SchemaVersion(manifest) switch
        {
            SemanticVersion version => Pinned(manifest, new SpecStructure(sections, tables, identifiers, version)),
            InputFailure failure => failure,
        };
    }

    private static Outcome<SpecStructure> Pinned(JsonObject manifest, SpecStructure pinned)
    {
        if (Discovery(manifest) is not { } discovery
            || Filled(manifest, "frontmatterSchemas") is not { } frontmatterSchemas
            || Filled(manifest, "roles") is not { } roles
            || Filled(manifest, "markers") is not { } markers
            || Levels(manifest["rules"]) is not { } rules)
        {
            return new ManifestUnreadable();
        }

        return Check(
            pinned with { Discovery = discovery, FrontmatterSchemas = frontmatterSchemas, Roles = roles, Markers = markers, Rules = rules },
            IdentityFaults(manifest));
    }

    private static Outcome<SpecStructure> Check(SpecStructure structure, IEnumerable<string> identityFaults)
    {
        var roles = Defaults["roles"]!.AsObject();
        var faults = roles
            .Select(static entry => entry.Key)
            .Where(role => !structure.Sections.Contains(structure.Roles[role], StringComparer.Ordinal))
            .Select(role => $"the role '{role}' names '{structure.Roles[role]}', which sections does not list")
            .Concat(structure.Tables.Keys.Where(key => !roles.ContainsKey(key)).Select(static key => $"the tables key '{key}' is not a role"))
            .Concat(
                Defaults["markers"]!.AsObject()
                    .Select(static entry => entry.Key)
                    .Where(marker => structure.Markers[marker].Length == 0)
                    .Select(static marker => $"the marker '{marker}' in markers is empty"))
            .Concat(
                new (string Key, IReadOnlyList<string> Entries)[]
                    {
                        ("taskFiles", structure.Discovery.TaskFiles),
                        ("epicFiles", structure.Discovery.EpicFiles),
                        ("companionFiles", structure.Discovery.CompanionFiles),
                    }
                    .Where(static list => list.Entries.Count == 0)
                    .Select(static list => $"the list '{list.Key}' is empty"))
            .Concat(
                structure.Discovery.Exclusions
                    .Where(static entry => entry.Contains('/') && !entry.StartsWith('/'))
                    .Select(static entry => $"the exclusion '{entry}' has a '/' inside it and no leading '/'"))
            .Concat(identityFaults)
            .Concat(
                structure.Rules
                    .Where(static rule => rule.Value is not ("error" or "warning" or "off"))
                    .Select(static rule => $"the rule '{rule.Key}' in rules is set to '{rule.Value}', which is not 'error', 'warning' or 'off'"))
            .ToList();

        return faults.Count > 0 ? new ManifestRejected($"{RelativePath}: {string.Join("; ", faults)}.") : structure;
    }

    private static SpecDiscoveryInputs? Discovery(JsonObject manifest)
    {
        if ((manifest["layouts"] ?? Defaults["layouts"]!) is not JsonArray declared)
        {
            return null;
        }

        var layouts = new List<SpecLayout>();

        foreach (var node in declared)
        {
            if (Layout(node) is not { } layout)
            {
                return null;
            }

            layouts.Add(layout);
        }

        return Strings(manifest["exclusions"] ?? Defaults["exclusions"]!) is { } exclusions
            && Strings(manifest["taskFiles"] ?? Defaults["taskFiles"]!) is { } taskFiles
            && Strings(manifest["epicFiles"] ?? Defaults["epicFiles"]!) is { } epicFiles
            && Strings(manifest["companionFiles"] ?? Defaults["companionFiles"]!) is { } companionFiles
            ? new SpecDiscoveryInputs(layouts, exclusions, taskFiles, epicFiles, companionFiles)
            : null;
    }

    private static SpecLayout? Layout(JsonNode? node)
    {
        if (node is not JsonObject layout || Text(layout["name"]) is not { } name || Text(layout["glob"]) is not { } glob)
        {
            return null;
        }

        return layout["identity"] switch
        {
            null => new SpecLayout(name, glob),
            JsonObject identity => new SpecLayout(name, glob, new SpecPathIdentity(Segment(identity["epic"]), Segment(identity["feature"]))),
            _ => null,
        };
    }

    private static int? Segment(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<int>(out var index) && index >= 0 ? index : null;

    private static IEnumerable<string> IdentityFaults(JsonObject manifest)
    {
        foreach (var layout in manifest["layouts"]?.AsArray() ?? [])
        {
            if (layout!["identity"] is not { } node)
            {
                continue;
            }

            var name = layout["name"]!.GetValue<string>();
            var identity = node.AsObject();

            foreach (var (member, value) in identity)
            {
                if (member is not ("epic" or "feature"))
                {
                    yield return $"the layout '{name}' declares the identity member '{member}', which is neither 'epic' nor 'feature'";
                }
                else if (Segment(value) is null)
                {
                    yield return $"the identity member '{member}' of the layout '{name}' is {value?.ToJsonString() ?? "null"}, "
                        + "which is not a non-negative integer";
                }
            }

            if (!identity.ContainsKey("epic") && !identity.ContainsKey("feature"))
            {
                yield return $"the identity of the layout '{name}' declares neither 'epic' nor 'feature'";
            }
        }
    }

    private static Dictionary<string, string>? Filled(JsonObject manifest, string key)
    {
        var values = Named(Defaults[key]!)!;

        if (manifest[key] is not { } declared)
        {
            return values;
        }

        if (Named(declared) is not { } named)
        {
            return null;
        }

        foreach (var (name, value) in named)
        {
            values[name] = value;
        }

        return values;
    }

    private static Outcome<SemanticVersion> SchemaVersion(JsonObject manifest)
    {
        if (!manifest.TryGetPropertyValue("schemaVersion", out var node))
        {
            return new SemanticVersion(0, 1, 0);
        }

        if (node is JsonValue value && value.TryGetValue<string>(out var text) && SemanticVersion.TryParse(text, out var version))
        {
            return version;
        }

        return new ManifestRejected(
            $"{RelativePath}: schemaVersion {node?.ToJsonString() ?? "null"} is not a major.minor.patch version such as \"0.1.0\".");
    }

    private static List<string>? Strings(JsonNode node)
    {
        if (node is not JsonArray array)
        {
            return null;
        }

        var strings = new List<string>();

        foreach (var element in array)
        {
            if (Text(element) is not { } text)
            {
                return null;
            }

            strings.Add(text);
        }

        return strings;
    }

    private static Dictionary<string, IReadOnlyList<string>>? Tables(JsonNode node)
    {
        if (node is not JsonObject declared)
        {
            return null;
        }

        var tables = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

        foreach (var (key, value) in declared)
        {
            if (value is null || Strings(value) is not { } headers)
            {
                return null;
            }

            tables[key] = headers;
        }

        return tables;
    }

    private static Dictionary<string, string>? Named(JsonNode node)
    {
        if (node is not JsonObject declared)
        {
            return null;
        }

        var named = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (key, value) in declared)
        {
            if (Text(value) is not { } text)
            {
                return null;
            }

            named[key] = text;
        }

        return named;
    }

    private static Dictionary<string, string>? Levels(JsonNode? node) =>
        node switch
        {
            null => new Dictionary<string, string>(StringComparer.Ordinal),
            JsonObject levels => levels.ToDictionary(
                static entry => entry.Key,
                static entry => Text(entry.Value) ?? entry.Value?.ToJsonString() ?? "null",
                StringComparer.Ordinal),
            _ => null,
        };

    private static string? Text(JsonNode? node) => node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private static JsonObject ReadDefaults()
    {
        using var stream = typeof(SpecManifest).Assembly.GetManifestResourceStream(DefaultResource)!;

        return JsonNode.Parse(stream)!.AsObject();
    }

    private const string DefaultResource = "specht.default-manifest.json";

    private static readonly HashSet<string> KnownKeys = new(StringComparer.Ordinal)
    {
        "schemaVersion",
        "layouts",
        "exclusions",
        "taskFiles",
        "epicFiles",
        "companionFiles",
        "sections",
        "tables",
        "identifiers",
        "frontmatterSchemas",
        "roles",
        "markers",
        "rules",
    };

    private static readonly JsonObject Defaults = ReadDefaults();
}
