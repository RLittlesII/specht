using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.Discovery;
using Specht.Versioning;

namespace Specht.Manifest;

/// <summary>Parses and validates a manifest in memory using the embedded defaults (ADR-0010).</summary>
/// <param name="versions">The instance catalog carrying each pin's shipped defaults and rule vocabulary.</param>
public sealed class SpecManifestParser(SchemaVersions versions)
{
    /// <summary>Returns the validated structure with omitted values supplied by the default manifest.</summary>
    /// <param name="text">The manifest JSON to validate.</param>
    /// <returns>The structure and its upstream schema source, if declared.</returns>
    public SpecStructure Parse(string text)
    {
        var manifest = Present(JsonNode.Parse(text)).AsObject();
        var number = ReadVersion(manifest["schemaVersion"]) ?? OriginalVersion;
        var tags = number.CompareTo(ClaimTagVersion) >= 0 && versions.Find(number) is not null;
        var unknown = manifest
            .Select(static entry => entry.Key)
            .Where(key => !key.StartsWith('$') && !KnownKeys.Contains(key) && !(tags && key == "claim"))
            .Select(static key => $"'{key}'")
            .ToList();

        if (unknown.Count > 0)
        {
            throw new SpechtManifestException(
                $"{SpecManifestStore.RelativePath}: the engine does not know the key {string.Join(", ", unknown)}.");
        }

        var selected = versions.Find(number) ?? versions.Find(OriginalVersion) ?? Version(number);
        var defaults = JsonNode.Parse(selected.Manifest)!.AsObject();
        var frontmatterSchemas = Filled(manifest, defaults, "frontmatterSchemas");
        var claim = tags ? manifest["claim"]?.AsObject() : null;
        var claimDefaults = tags ? Present(defaults["claim"]).AsObject() : null;
        var sourceFaults = new List<string>();
        var structure = new SpecStructure(
            Strings(manifest["sections"] ?? defaults["sections"]!),
            Tables(manifest["tables"] ?? defaults["tables"]!),
            Filled(manifest, defaults, "identifiers"),
            SchemaVersion(manifest))
        {
            Discovery = Discovery(manifest, defaults),
            FrontmatterSchemas = frontmatterSchemas,
            Roles = Filled(manifest, defaults, "roles"),
            Markers = Filled(manifest, defaults, "markers"),
            Rules = Levels(manifest["rules"]),
            ClaimTagForm = tags ? Present(claim?["tag"] ?? claimDefaults?["tag"]).GetValue<string>() : null,
            ClaimTagIgnoreCase = tags && Present(claim?["tagIgnoreCase"] ?? claimDefaults?["tagIgnoreCase"]).GetValue<bool>(),
            SpechtSchema = SpechtSchema(manifest, frontmatterSchemas, sourceFaults),
        };

        Check(structure, defaults, IdentityFaults(manifest).Concat(sourceFaults).Concat(ClaimTagFaults(structure)));

        return structure;
    }

    /// <summary>Resolves a parsed manifest's pin and validates its rule ids before schema reads (0001-F5 C-5).</summary>
    /// <param name="structure">The parsed and shape-validated manifest.</param>
    /// <returns>The selected version.</returns>
    /// <exception cref="SpechtManifestException">The pin is not shipped or a rule id is outside its vocabulary.</exception>
    public SchemaVersion PinnedVersion(SpecStructure structure)
    {
        var version = Version(structure.SchemaVersion);
        var outside = structure.Rules.Keys.Where(id => !version.RuleIds.Contains(id)).Select(static id => $"'{id}'").ToList();
        if (outside.Count > 0)
        {
            throw new SpechtManifestException(
                $"{SpecManifestStore.RelativePath}: rules names the rule id {string.Join(", ", outside)}, "
                + $"which schemaVersion {version.Number} does not hold.");
        }

        return version;
    }

    private static IEnumerable<string> ClaimTagFaults(SpecStructure structure)
    {
        if (structure.ClaimTagForm is not { } form)
        {
            yield break;
        }

        if (form.Split("{claim}", StringSplitOptions.None).Length != 2)
        {
            yield return $"claim.tag '{form}' must hold '{{claim}}' exactly once";
        }

        var grammar = structure.Identifiers["claim"];
        string? fault = null;
        try
        {
            _ = new Regex(grammar, RegexOptions.CultureInvariant);
        }
        catch (ArgumentException)
        {
            fault = $"the claim grammar '{grammar}' does not compile";
        }

        if (fault is not null)
        {
            yield return fault;
        }
    }

    private static UpstreamSchemaSource? SpechtSchema(JsonObject manifest, Dictionary<string, string> fileNames, List<string> faults)
    {
        if (!manifest.TryGetPropertyValue("spechtSchema", out var node))
        {
            return null;
        }

        var source = Present(node).AsObject();

        faults.AddRange(
            new[] { "location", "version", "hashes" }
                .Where(member => !source.ContainsKey(member))
                .Select(static member => $"spechtSchema has no '{member}'"));

        var location = Text(source["location"]);
        var version = Text(source["version"]);

        if (source.ContainsKey("location")
            && !(Uri.TryCreate(location, UriKind.Absolute, out var address) && address.Scheme == Uri.UriSchemeHttps))
        {
            faults.Add("the 'location' of spechtSchema is not an https address");
        }

        if (source.ContainsKey("version") && string.IsNullOrEmpty(version))
        {
            faults.Add("the 'version' of spechtSchema is not a non-empty string");
        }

        var files = new List<UpstreamSchemaFile>();

        if (source.TryGetPropertyValue("hashes", out var declared))
        {
            var hashes = Present(declared).AsObject();
            var kinds = manifest["identifiers"]?["epic"] is null ? new[] { "feature", "task" } : ["feature", "task", "epic"];

            foreach (var kind in kinds)
            {
                if (!hashes.TryGetPropertyValue(kind, out var entry))
                {
                    faults.Add($"the 'hashes' of spechtSchema holds no entry for the kind '{kind}'");
                }
                else if (Text(entry) is not { } hash || !IsSha256(hash))
                {
                    faults.Add(
                        $"the 'hashes' entry of spechtSchema for the kind '{kind}' is not 'sha256:' followed by 64 lowercase hexadecimal digits");
                }
                else
                {
                    files.Add(new UpstreamSchemaFile(kind, fileNames[kind], location + fileNames[kind], hash));
                }
            }
        }

        return faults.Count == 0 ? new UpstreamSchemaSource(location!, version!, files) : null;
    }

    private static string? Text(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private static bool IsSha256(string hash) =>
        hash.Length == 71
            && hash.StartsWith("sha256:", StringComparison.Ordinal)
            && hash.AsSpan(7).IndexOfAnyExcept("0123456789abcdef") < 0;



    private static SpecLayout Layout(JsonNode? node)
    {
        var layout = Present(node);

        return new SpecLayout(
            Present(layout["name"]).GetValue<string>(),
            Present(layout["glob"]).GetValue<string>(),
            layout["identity"] is { } identity ? new SpecPathIdentity(Segment(identity["epic"]), Segment(identity["feature"])) : null);
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


    private static SemanticVersion SchemaVersion(JsonObject manifest)
    {
        if (!manifest.TryGetPropertyValue("schemaVersion", out var node))
        {
            return OriginalVersion;
        }

        if (ReadVersion(node) is { } version)
        {
            return version;
        }

        throw new SpechtManifestException(
            $"{SpecManifestStore.RelativePath}: schemaVersion {node?.ToJsonString() ?? "null"} is not a major.minor.patch version such as \"0.1.0\".");
    }

    private static SemanticVersion? ReadVersion(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var text) && SemanticVersion.TryParse(text, out var version) ? version : null;

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

    private static Dictionary<string, string> Levels(JsonNode? node) =>
        node is null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : node.AsObject().ToDictionary(
                static entry => entry.Key,
                static entry => entry.Value is JsonValue value && value.TryGetValue<string>(out var level)
                    ? level
                    : entry.Value?.ToJsonString() ?? "null",
                StringComparer.Ordinal);

    private static JsonNode Present(JsonNode? node) =>
        node ?? throw new InvalidOperationException("A JSON null stands where the manifest's shape requires a value.");

    private static void Check(SpecStructure structure, JsonObject defaults, IEnumerable<string> identityFaults)
    {
        var roles = defaults["roles"]!.AsObject();
        var faults = roles
            .Select(static entry => entry.Key)
            .Where(role => !structure.Sections.Contains(structure.Roles[role], StringComparer.Ordinal))
            .Select(role => $"the role '{role}' names '{structure.Roles[role]}', which sections does not list")
            .Concat(structure.Tables.Keys.Where(key => !roles.ContainsKey(key)).Select(static key => $"the tables key '{key}' is not a role"))
            .Concat(
                defaults["markers"]!.AsObject()
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
                structure.FrontmatterSchemas
                    .Where(static schema => schema.Value.Contains("..", StringComparison.Ordinal)
                        || schema.Value.Contains('/') || schema.Value.Contains('\\'))
                    .Select(static schema =>
                        $"the frontmatterSchemas kind '{schema.Key}' names '{schema.Value}', which is not a plain file name"))
            .Concat(
                structure.Rules
                    .Where(static rule => rule.Value is not ("error" or "warning" or "off"))
                    .Select(static rule => $"the rule '{rule.Key}' in rules is set to '{rule.Value}', which is not 'error', 'warning' or 'off'"))
            .ToList();

        if (faults.Count > 0)
        {
            throw new SpechtManifestException($"{SpecManifestStore.RelativePath}: {string.Join("; ", faults)}.");
        }
    }

    private static SpecDiscoveryInputs Discovery(JsonObject manifest, JsonObject defaults) =>
        new(
            (manifest["layouts"] ?? defaults["layouts"]!).AsArray().Select(Layout).ToList(),
            Strings(manifest["exclusions"] ?? defaults["exclusions"]!),
            Strings(manifest["taskFiles"] ?? defaults["taskFiles"]!),
            Strings(manifest["epicFiles"] ?? defaults["epicFiles"]!),
            Strings(manifest["companionFiles"] ?? defaults["companionFiles"]!));

    private static Dictionary<string, string> Filled(JsonObject manifest, JsonObject defaults, string key)
    {
        var values = Named(defaults[key]!);

        if (manifest[key] is { } declared)
        {
            foreach (var (name, value) in Named(declared))
            {
                values[name] = value;
            }
        }

        return values;
    }

    private SchemaVersion Version(SemanticVersion number) =>
        versions.Find(number) ?? throw new SpechtManifestException(
            $"{SpecManifestStore.RelativePath}: schemaVersion {number} is not shipped; this tool ships "
            + $"{string.Join(", ", versions.Versions.Select(static version => version.Number))}.");

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
        "spechtSchema",
    };

    private static readonly SemanticVersion ClaimTagVersion = new(0, 2, 0);

    private static readonly SemanticVersion OriginalVersion = new(0, 1, 0);
}
