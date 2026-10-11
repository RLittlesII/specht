using System.IO.Abstractions;
using System.Text.Json.Nodes;
using LanguageExt;
using Specht.Discovery;
using Specht.Versioning;
using static LanguageExt.Prelude;

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
    /// manifest, and the rule settings as the manifest wrote them (<c>0001-F5</c> B-044); or the failure:
    /// <see cref="SpechtFailureKind.RootNotFound"/> when <paramref name="root"/> is not a directory,
    /// <see cref="SpechtFailureKind.ManifestNotFound"/> when there is no file at the manifest path,
    /// <see cref="SpechtFailureKind.ManifestUnreadable"/> when the manifest is not well-formed JSON or not the manifest's
    /// shape, and <see cref="SpechtFailureKind.ManifestRejected"/> when the manifest carries a key the engine does not
    /// know, a <c>schemaVersion</c> that is not a <c>major.minor.patch</c> string, a role naming a title <c>sections</c>
    /// does not list, a <c>tables</c> key that is not a role, a marker whose text is empty, an empty <c>taskFiles</c>,
    /// <c>epicFiles</c> or <c>companionFiles</c> list, an <c>exclusions</c> entry with a <c>/</c> inside it and no leading
    /// <c>/</c>, a <c>layouts</c> entry whose <c>identity</c> holds a member that is not a non-negative integer, a member
    /// other than <c>epic</c> and <c>feature</c>, or neither of the two, or a <c>rules</c> entry whose value is not
    /// <c>error</c>, <c>warning</c> or <c>off</c>.
    /// </returns>
    public static Either<SpechtFailure, SpecStructure> Load(IFileSystem fileSystem, string root)
    {
        if (!fileSystem.Directory.Exists(root))
        {
            return new SpechtFailure(SpechtFailureKind.RootNotFound, "The root is not a directory.");
        }

        var path = fileSystem.Path.Combine(root, ".spec", "schema", "spec-structure.schema.json");

        if (!fileSystem.File.Exists(path))
        {
            return new SpechtFailure(SpechtFailureKind.ManifestNotFound, $"{RelativePath}: there is no manifest at the manifest path.");
        }

        return Read(fileSystem.File.ReadAllText(path));
    }

    private static Either<SpechtFailure, SpecStructure> Read(string text) =>
        from manifest in Parse(text).Bind(Known)
        from sections in Strings(manifest["sections"] ?? Defaults["sections"])
        from tables in Tables(manifest["tables"] ?? Defaults["tables"])
        from identifiers in Filled(manifest, "identifiers")
        from version in SchemaVersion(manifest)
        from discovery in Discovery(manifest)
        from frontmatterSchemas in Filled(manifest, "frontmatterSchemas")
        from roles in Filled(manifest, "roles")
        from markers in Filled(manifest, "markers")
        from rules in Levels(manifest["rules"] ?? new JsonObject())
        from structure in Check(
            new SpecStructure(sections, tables, identifiers, version)
            {
                Discovery = discovery,
                FrontmatterSchemas = frontmatterSchemas,
                Roles = roles,
                Markers = markers,
                Rules = rules,
            },
            IdentityFaults(manifest))
        select structure;

    private static Either<SpechtFailure, JsonObject> Parse(string text) =>
        Try.lift(() => JsonNode.Parse(text)).Run() is Fin<JsonNode?>.Succ { Value: JsonObject manifest } ? manifest : Unreadable;

    private static Either<SpechtFailure, JsonObject> Known(JsonObject manifest)
    {
        var unknown = manifest
            .Select(static entry => entry.Key)
            .Where(static key => !key.StartsWith('$') && !KnownKeys.Contains(key))
            .Select(static key => $"'{key}'")
            .ToList();

        return unknown.Count > 0
            ? new SpechtFailure(
                SpechtFailureKind.ManifestRejected,
                $"{RelativePath}: the engine does not know the key {string.Join(", ", unknown)}.")
            : manifest;
    }

    private static Either<SpechtFailure, SpecStructure> Check(SpecStructure structure, IEnumerable<string> identityFaults)
    {
        var roles = Defaults["roles"]!.AsObject();

        return (Faults(
                    roles
                        .Select(static entry => entry.Key)
                        .Where(role => !structure.Sections.Contains(structure.Roles[role], StringComparer.Ordinal))
                        .Select(role => $"the role '{role}' names '{structure.Roles[role]}', which sections does not list"))
                & Faults(structure.Tables.Keys.Where(key => !roles.ContainsKey(key)).Select(static key => $"the tables key '{key}' is not a role"))
                & Faults(
                    Defaults["markers"]!.AsObject()
                        .Select(static entry => entry.Key)
                        .Where(marker => structure.Markers[marker].Length == 0)
                        .Select(static marker => $"the marker '{marker}' in markers is empty"))
                & Faults(
                    new (string Key, IReadOnlyList<string> Entries)[]
                        {
                            ("taskFiles", structure.Discovery.TaskFiles),
                            ("epicFiles", structure.Discovery.EpicFiles),
                            ("companionFiles", structure.Discovery.CompanionFiles),
                        }
                        .Where(static list => list.Entries.Count == 0)
                        .Select(static list => $"the list '{list.Key}' is empty"))
                & Faults(
                    structure.Discovery.Exclusions
                        .Where(static entry => entry.Contains('/') && !entry.StartsWith('/'))
                        .Select(static entry => $"the exclusion '{entry}' has a '/' inside it and no leading '/'"))
                & Faults(identityFaults)
                & Faults(
                    structure.Rules
                        .Where(static rule => rule.Value is not ("error" or "warning" or "off"))
                        .Select(static rule => $"the rule '{rule.Key}' in rules is set to '{rule.Value}', which is not 'error', 'warning' or 'off'")))
            .ToEither()
            .MapLeft(static faults => new SpechtFailure(SpechtFailureKind.ManifestRejected, $"{RelativePath}: {string.Join("; ", faults)}."))
            .Map(_ => structure);
    }

    private static Validation<Seq<string>, Unit> Faults(IEnumerable<string> faults) =>
        toSeq(faults) is { IsEmpty: false } found
            ? Validation.Fail<Seq<string>, Unit>(found)
            : Validation.Success<Seq<string>, Unit>(unit);

    private static Either<SpechtFailure, SpecDiscoveryInputs> Discovery(JsonObject manifest) =>
        from layouts in Items(manifest["layouts"] ?? Defaults["layouts"], Layout)
        from exclusions in Strings(manifest["exclusions"] ?? Defaults["exclusions"])
        from taskFiles in Strings(manifest["taskFiles"] ?? Defaults["taskFiles"])
        from epicFiles in Strings(manifest["epicFiles"] ?? Defaults["epicFiles"])
        from companionFiles in Strings(manifest["companionFiles"] ?? Defaults["companionFiles"])
        select new SpecDiscoveryInputs(layouts, exclusions, taskFiles, epicFiles, companionFiles);

    private static Either<SpechtFailure, SpecLayout> Layout(JsonNode? node) =>
        node is JsonObject layout && layout["identity"] is null or JsonObject
            ? from name in Text(layout["name"])
              from glob in Text(layout["glob"])
              select new SpecLayout(
                  name,
                  glob,
                  layout["identity"] is { } identity ? new SpecPathIdentity(Segment(identity["epic"]), Segment(identity["feature"])) : null)
            : Unreadable;

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

    private static Either<SpechtFailure, Dictionary<string, string>> Filled(JsonObject manifest, string key) =>
        from values in Named(Defaults[key])
        from declared in Named(manifest[key] ?? new JsonObject())
        select Overlaid(values, declared);

    private static Dictionary<string, string> Overlaid(Dictionary<string, string> values, Dictionary<string, string> declared)
    {
        foreach (var (name, value) in declared)
        {
            values[name] = value;
        }

        return values;
    }

    private static Either<SpechtFailure, SemanticVersion> SchemaVersion(JsonObject manifest)
    {
        if (!manifest.TryGetPropertyValue("schemaVersion", out var node))
        {
            return new SemanticVersion(0, 1, 0);
        }

        if (node is JsonValue value && value.TryGetValue<string>(out var text) && SemanticVersion.TryParse(text, out var version))
        {
            return version;
        }

        return new SpechtFailure(
            SpechtFailureKind.ManifestRejected,
            $"{RelativePath}: schemaVersion {node?.ToJsonString() ?? "null"} is not a major.minor.patch version such as \"0.1.0\".");
    }

    private static Either<SpechtFailure, List<string>> Strings(JsonNode? node) => Items(node, Text);

    private static Either<SpechtFailure, Dictionary<string, IReadOnlyList<string>>> Tables(JsonNode? node) =>
        Entries(node, static value => Strings(value).Map(static cells => (IReadOnlyList<string>)cells));

    private static Either<SpechtFailure, Dictionary<string, string>> Named(JsonNode? node) => Entries(node, Text);

    private static Either<SpechtFailure, Dictionary<string, string>> Levels(JsonNode node) =>
        Entries(
            node,
            static value => Right<SpechtFailure, string>(
                value is JsonValue level && level.TryGetValue<string>(out var text) ? text : value?.ToJsonString() ?? "null"));

    private static Either<SpechtFailure, List<T>> Items<T>(JsonNode? node, Func<JsonNode?, Either<SpechtFailure, T>> read) =>
        node is JsonArray array
            ? toSeq(array).Traverse(read).As().Map(static items => items.ToList())
            : Unreadable;

    private static Either<SpechtFailure, Dictionary<string, T>> Entries<T>(JsonNode? node, Func<JsonNode?, Either<SpechtFailure, T>> read) =>
        node is JsonObject named
            ? toSeq(named)
                .Traverse(entry => read(entry.Value).Map(value => KeyValuePair.Create(entry.Key, value)))
                .As()
                .Map(static entries => entries.ToDictionary(StringComparer.Ordinal))
            : Unreadable;

    private static Either<SpechtFailure, string> Text(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var text) ? text : Unreadable;

    private static JsonObject ReadDefaults()
    {
        using var stream = typeof(SpecManifest).Assembly.GetManifestResourceStream(DefaultResource)!;

        return JsonNode.Parse(stream)!.AsObject();
    }

    private const string DefaultResource = "specht.default-manifest.json";

    private static readonly System.Collections.Generic.HashSet<string> KnownKeys = new(StringComparer.Ordinal)
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

    private static readonly SpechtFailure Unreadable = new(
        SpechtFailureKind.ManifestUnreadable,
        $"{RelativePath}: the manifest does not parse into the manifest's shape.");
}
