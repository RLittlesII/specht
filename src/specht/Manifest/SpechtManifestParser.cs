using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.Discovery;
using Specht.Identifiers;
using Specht.Versioning;

namespace Specht.Manifest;

/// <summary>Parses and validates a manifest in memory using the embedded defaults (ADR-0010).</summary>
/// <param name="versions">The instance catalog carrying each pin's shipped defaults and rule vocabulary.</param>
/// <implements epic="0001" feature="F5" claim="B-048" />
public sealed class SpechtManifestParser(SchemaVersions versions)
{
    /// <summary>Returns the validated structure with omitted values supplied by the default manifest.</summary>
    /// <param name="text">The manifest JSON to validate.</param>
    /// <returns>The structure and its upstream schema source, if declared.</returns>
    /// <implements epic="0001" feature="F5" claim="B-048" />
    public SpechtStructure Parse(string text) => Parse(text, false, out _);

    /// <summary>Returns the validated structure and its owned JSON document with the same omission defaults.</summary>
    /// <param name="text">The manifest JSON to validate.</param>
    /// <returns>The parsed structure and reusable effective document before rule-level expansion.</returns>
    /// <implements epic="0001" feature="F9" claim="B-003" />
    /// <implements epic="0001" feature="F9" claim="B-004" />
    /// <implements epic="0001" feature="F9" claim="B-006" />
    public SpechtManifestDocument ParseDocument(string text)
    {
        var structure = Parse(text, true, out var document);
        return new SpechtManifestDocument(structure, document!);
    }

    /// <summary>Resolves a parsed manifest's pin and validates its rule ids before schema reads (0001-F5 C-5).</summary>
    /// <param name="structure">The parsed and shape-validated manifest.</param>
    /// <returns>The selected version.</returns>
    /// <exception cref="SpechtManifestException">The pin is not shipped or a rule id is outside its vocabulary.</exception>
    /// <implements epic="0001" feature="F5" claim="B-048" />
    public SchemaVersion PinnedVersion(SpechtStructure structure)
    {
        var version = Version(structure.SchemaVersion);
        var outside = structure.Rules.Keys.Where(id => !version.RuleIds.Contains(id)).Select(static id => $"'{id}'").ToList();
        if (outside.Count > 0)
        {
            throw new SpechtManifestException(
                $"{SpechtManifestStore.RelativePath}: rules names the rule id {string.Join(", ", outside)}, "
                + $"which schemaVersion {version.Number} does not hold.");
        }

        return version;
    }

    /// <summary>
    /// Resolves the pin as <see cref="PinnedVersion"/> does, then the version the check uses under the manifest's pin policy
    /// (<c>0001-F7</c> B-053, B-054, B-147; OQ-71 (a)).
    /// </summary>
    /// <param name="structure">The parsed and shape-validated manifest.</param>
    /// <returns>The version in use, the pin itself under <see cref="PinPolicy.Exact"/>.</returns>
    /// <exception cref="SpechtManifestException">The pin is not shipped or a rule id is outside its vocabulary.</exception>
    /// <implements epic="0001" feature="F7" claim="B-053" />
    /// <implements epic="0001" feature="F7" claim="B-054" />
    /// <implements epic="0001" feature="F7" claim="B-147" />
    public SchemaVersion VersionInUse(SpechtStructure structure) =>
        versions.InUse(PinnedVersion(structure).Number, structure.PinPolicy);

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static IEnumerable<string> ClaimTagFaults(SpechtStructure structure)
    {
        if (structure.ClaimTagForm is not { } form)
        {
            yield break;
        }

        if (form.Split("{claim}", StringSplitOptions.None).Length != 2)
        {
            yield return $"claim.tag '{form}' must hold '{{claim}}' exactly once";
        }

        if (structure.Ids is not null)
        {
            yield break;
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

    /// <implements epic="0001" feature="F5" claim="B-081" />
    /// <implements epic="0001" feature="F5" claim="B-083" />
    /// <implements epic="0001" feature="F5" claim="B-084" />
    /// <implements epic="0001" feature="F5" claim="B-085" />
    /// <implements epic="0001" feature="F5" claim="B-094" />
    /// <implements epic="0001" feature="F5" claim="B-097" />
    private static Dictionary<string, SpechtIdRecord> Ids(
        JsonObject manifest, JsonObject defaults, StringComparison comparison, List<string> faults, JsonObject? document)
    {
        var shipped = Present(defaults["ids"]).AsObject();
        var records = IdTypes.RepositoryWide.Concat(IdTypes.PerFeature).ToDictionary(
            static type => type,
            type => new SpechtIdRecord(Text(shipped[type]?["plain"])!, Text(shipped[type]?["form"]), Text(shipped[type]?["id"])!),
            StringComparer.Ordinal);

        if (!manifest.TryGetPropertyValue("ids", out var node))
        {
            document?["ids"] = shipped.DeepClone();
        }
        else if (node is not JsonObject declared)
        {
            faults.Add($"ids {node?.ToJsonString() ?? "null"} is not an object of id records");
        }
        else
        {
            foreach (var (type, value) in declared)
            {
                if (type.StartsWith('$'))
                {
                    continue;
                }

                if (!records.TryGetValue(type, out var filled))
                {
                    faults.Add($"ids.{type} is not an id type; ids names {string.Join(", ", records.Keys.Select(static name => $"'{name}'"))}");
                    continue;
                }

                if (value is not JsonObject record)
                {
                    faults.Add($"ids.{type} {value?.ToJsonString() ?? "null"} is not a record");
                    continue;
                }

                faults.AddRange(record
                    .Select(static member => member.Key)
                    .Where(static member => !member.StartsWith('$') && member is not ("plain" or "form" or "id" or "mint"))
                    .Select(member => $"ids.{type}.{member} is not 'plain', 'form', 'id' or 'mint'"));

                records[type] = new SpechtIdRecord(
                    record.TryGetPropertyValue("plain", out var plain) ? Plain(type, plain, faults) ?? filled.Plain : filled.Plain,
                    record.TryGetPropertyValue("form", out var form) ? Form(type, form, faults) : filled.Form,
                    record.TryGetPropertyValue("id", out var id) ? Id(type, id, faults) ?? filled.Id : filled.Id);

                if (document?["ids"]?[type] is JsonObject projected)
                {
                    var defaultRecord = shipped[type]!.AsObject();
                    foreach (var member in new[] { "plain", "form", "id" }.Where(member => !projected.ContainsKey(member) && defaultRecord[member] is not null))
                    {
                        projected[member] = defaultRecord[member]!.DeepClone();
                    }
                }
            }

            if (document?["ids"] is JsonObject projectedIds)
            {
                foreach (var (type, record) in shipped.Where(entry => !entry.Key.StartsWith('$') && !declared.ContainsKey(entry.Key)))
                {
                    projectedIds[type] = record!.DeepClone();
                }
            }
        }

        var claim = records["claim"];
        var scenario = records["scenario"];
        var shared = string.Equals(claim.Head(null), scenario.Head(null), comparison) ? claim.Plain
            : claim.Form is not null && scenario.Form is not null
                && string.Equals(claim.Form[..^SpechtIdRecord.IdPlaceholder.Length], scenario.Form[..^SpechtIdRecord.IdPlaceholder.Length], comparison)
                ? claim.Form
                : null;
        if (shared is not null)
        {
            faults.Add($"ids.claim and ids.scenario give claims and scenarios the one head '{shared[..^SpechtIdRecord.IdPlaceholder.Length]}'");
        }

        return records;
    }

    /// <implements epic="0001" feature="F5" claim="B-084" />
    private static string? Plain(string type, JsonNode? node, List<string> faults)
    {
        if (Text(node) is { } plain && plain.EndsWith(SpechtIdRecord.IdPlaceholder, StringComparison.Ordinal)
            && plain[..^SpechtIdRecord.IdPlaceholder.Length].IndexOfAny(Braces) < 0)
        {
            return plain;
        }

        faults.Add($"ids.{type}.plain {Quoted(node)} must end with its only '{SpechtIdRecord.IdPlaceholder}' and hold no other brace");
        return null;
    }

    /// <implements epic="0001" feature="F5" claim="B-084" />
    private static string? Form(string type, JsonNode? node, List<string> faults)
    {
        if (type is "adr" or "lesson")
        {
            faults.Add($"ids.{type}.form {Quoted(node)} is not read: an {type} id has no form in a Feature");
            return null;
        }

        if (Text(node) is { } form && Count(form, SpechtIdRecord.PrefixPlaceholder) == 1
            && form.EndsWith(SpechtIdRecord.IdPlaceholder, StringComparison.Ordinal)
            && form[..^SpechtIdRecord.IdPlaceholder.Length].Replace(SpechtIdRecord.PrefixPlaceholder, string.Empty, StringComparison.Ordinal)
                .IndexOfAny(Braces) < 0)
        {
            return form;
        }

        faults.Add(
            $"ids.{type}.form {Quoted(node)} must hold '{SpechtIdRecord.PrefixPlaceholder}' once, end with its only "
            + $"'{SpechtIdRecord.IdPlaceholder}' and hold no other brace");
        return null;
    }

    /// <implements epic="0001" feature="F5" claim="B-085" />
    private static string? Id(string type, JsonNode? node, List<string> faults)
    {
        switch (Text(node))
        {
            case null:
                faults.Add($"ids.{type}.id {Quoted(node)} is not text");
                return null;
            case "":
                faults.Add($"ids.{type}.id is empty");
                return null;
            case { } id when Compiled(id) is null:
                faults.Add($"ids.{type}.id '{id}' does not compile");
                return null;
            case { } id:
                return id;
        }
    }

    /// <implements epic="0001" feature="F5" claim="B-084" />
    private static string Quoted(JsonNode? node) => Text(node) is { } text ? $"'{text}'" : node?.ToJsonString() ?? "null";

    /// <implements epic="0001" feature="F5" claim="B-092" />
    /// <implements epic="0001" feature="F5" claim="B-093" />
    private static IEnumerable<string> OldIdFaults(JsonObject manifest)
    {
        if (manifest.TryGetPropertyValue("mint", out var mint))
        {
            var types = (mint as JsonObject ?? []).Select(static member => member.Key).Where(static member => !member.StartsWith('$')).ToList();
            yield return types.Count == 0
                ? "mint is not read from schema 0.4.0; a type's mint settings go in its ids record"
                : $"mint is not read from schema 0.4.0; {string.Join(", ", types.Select(static type => $"mint.{type} goes in ids.{type}.mint"))}";
        }

        foreach (var type in (manifest["identifiers"] as JsonObject ?? []).Select(static member => member.Key)
                     .Where(static member => IdTypes.RepositoryWide.Contains(member) || IdTypes.PerFeature.Contains(member)))
        {
            yield return $"identifiers.{type} is not read from schema 0.4.0; its grammar goes in ids.{type}";
        }
    }

    /// <implements epic="0001" feature="F5" claim="B-014" />
    /// <implements epic="0001" feature="F5" claim="B-050" />
    /// <implements epic="0001" feature="F5" claim="B-051" />
    private static IEnumerable<string> GrammarFaults(SpechtStructure structure)
    {
        foreach (var (name, grammar) in structure.Identifiers)
        {
            if (name == "claim")
            {
                continue;
            }

            if (Compiled(grammar) is not { } expression)
            {
                yield return $"the {name} grammar '{grammar}' does not compile";
                continue;
            }

            if (name != "task")
            {
                continue;
            }

            var groups = expression.GetGroupNames();
            if (!groups.Contains("sequence", StringComparer.Ordinal))
            {
                yield return $"the task grammar '{grammar}' has no named 'sequence' group";
            }

            if (structure.DeclaresEpicGrammar && !groups.Contains("epic", StringComparer.Ordinal))
            {
                yield return $"the task grammar '{grammar}' has no named 'epic' group";
            }
        }
    }

    /// <implements epic="0001" feature="F5" claim="B-014" />
    private static Regex? Compiled(string grammar)
    {
        try
        {
            return new Regex(grammar, RegexOptions.CultureInvariant);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <implements epic="0001" feature="F5" claim="B-052" />
    private static IEnumerable<string> FormFaults(Dictionary<string, string> forms)
    {
        foreach (var name in new[] { "identity", "qualifiedEdge" })
        {
            var form = forms[name];
            if (Count(form, SpechtForms.EpicPlaceholder) != 1 || Count(form, SpechtForms.IdPlaceholder) != 1)
            {
                yield return $"the {name} form '{form}' must hold '{SpechtForms.EpicPlaceholder}' and '{SpechtForms.IdPlaceholder}' exactly once each";
            }
        }

        var local = forms["localEdge"];
        if (Count(local, SpechtForms.IdPlaceholder) != 1)
        {
            yield return $"the localEdge form '{local}' must hold '{SpechtForms.IdPlaceholder}' exactly once";
        }
    }

    /// <implements epic="0001" feature="F5" claim="B-052" />
    private static int Count(string form, string placeholder) => form.Split(placeholder, StringSplitOptions.None).Length - 1;

    /// <implements epic="0001" feature="F5" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-019" />
    private static Dictionary<string, string> KeysOf(JsonObject manifest, JsonObject defaults, string kind, JsonObject? document)
    {
        JsonObject? projected = null;
        if (document is not null && (!manifest.ContainsKey("keys") || manifest["keys"] is not null))
        {
            if (document["keys"] is not JsonObject keys)
            {
                keys = [];
                document["keys"] = keys;
            }

            projected = keys;
        }

        return Filled(manifest["keys"]?.AsObject() ?? [], Present(defaults["keys"]).AsObject(), kind, projected);
    }

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static SpechtTestSettings? TestSettings(JsonNode? node) =>
        node is null ? null : new SpechtTestSettings(Strings(Present(node["files"])), node["declarations"] is { } forms ? Strings(forms) : []);

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static IEnumerable<string> MatrixKeyFaults(SpechtStructure structure) =>
        (structure.Tests?.Declarations ?? [])
            .Where(static form => form.Split("{test}", StringSplitOptions.None).Length != 2)
            .Select(static form => $"tests.declarations entry '{form}' must hold '{{test}}' exactly once")
            .Concat(
                structure.Owners
                    .Where(owner => !structure.Sections.Contains(owner.Key, StringComparer.Ordinal))
                    .Select(static owner => $"the owners title '{owner.Key}' is not listed in sections"))
            .Concat(
                structure.Owners
                    .Where(static owner => owner.Value.Count == 0)
                    .Select(static owner => $"the owners title '{owner.Key}' lists no role"));

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static SchemaSourceSelection? SourceSelection(JsonObject manifest, List<string> faults)
    {
        if (!manifest.TryGetPropertyValue("schemaSource", out var node))
        {
            return null;
        }

        switch (Text(node))
        {
            case "embedded":
                return SchemaSourceSelection.Embedded;
            case "disk":
                return SchemaSourceSelection.Disk;
            default:
                faults.Add("schemaSource must be 'embedded' or 'disk'");
                return null;
        }
    }

    /// <implements epic="0001" feature="F7" claim="B-056" />
    /// <implements epic="0001" feature="F7" claim="B-150" />
    private static PinPolicy Policy(JsonObject manifest, SemanticVersion pin, List<string> faults)
    {
        if (!manifest.TryGetPropertyValue("pinPolicy", out var node))
        {
            return PinPolicy.Exact;
        }

        switch (Text(node))
        {
            case "exact":
                return PinPolicy.Exact;
            case "latest-patch":
                return PinPolicy.LatestPatch;
            case "latest-minor" when pin.Major == 0:
                faults.Add($"pinPolicy 'latest-minor' needs a pin of 1.0.0 or later; schemaVersion {pin} floats with 'latest-patch'");
                return PinPolicy.Exact;
            case "latest-minor":
                return PinPolicy.LatestMinor;
            case { } text:
                faults.Add($"pinPolicy '{text}' must be 'exact', 'latest-patch' or 'latest-minor'");
                return PinPolicy.Exact;
            default:
                faults.Add($"pinPolicy {node?.ToJsonString() ?? "null"} must be 'exact', 'latest-patch' or 'latest-minor'");
                return PinPolicy.Exact;
        }
    }

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static UpstreamSchemaSource? SpechtSchema(
        JsonObject manifest, Dictionary<string, string> fileNames, SchemaVersion? pinned, List<string> faults)
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

        var templates = Templates(source, location, pinned, faults);

        return faults.Count == 0 ? new UpstreamSchemaSource(location!, version!, files) { Templates = templates } : null;
    }

    /// <implements epic="0001" feature="F7" claim="B-125" />
    /// <implements epic="0001" feature="F7" claim="B-126" />
    /// <implements epic="0001" feature="F7" claim="B-127" />
    /// <implements epic="0001" feature="F7" claim="B-131" />
    private static List<UpstreamTemplateFile> Templates(JsonObject source, string? location, SchemaVersion? pinned, List<string> faults)
    {
        var templates = new List<UpstreamTemplateFile>();
        if (!source.TryGetPropertyValue("templates", out var declared))
        {
            return templates;
        }

        foreach (var (fileName, entry) in Present(declared).AsObject())
        {
            if (Text(entry) is not { } hash || !IsSha256(hash))
            {
                faults.Add(
                    $"the 'templates' entry of spechtSchema for the template '{fileName}' is not 'sha256:' followed by 64 lowercase hexadecimal digits");
            }
            else if (pinned is not null && !pinned.Templates.Contains(fileName))
            {
                faults.Add($"the 'templates' of spechtSchema names '{fileName}', which schemaVersion {pinned.Number} does not ship as a template");
            }
            else
            {
                templates.Add(new UpstreamTemplateFile(fileName, location + fileName, hash));
            }
        }

        return templates;
    }

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static string? Text(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static bool IsSha256(string hash) =>
        hash.Length == 71
            && hash.StartsWith("sha256:", StringComparison.Ordinal)
            && hash.AsSpan(7).IndexOfAnyExcept("0123456789abcdef") < 0;



    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static SpechtLayout Layout(JsonNode? node)
    {
        var layout = Present(node);

        return new SpechtLayout(
            Present(layout["name"]).GetValue<string>(),
            Present(layout["glob"]).GetValue<string>(),
            layout["identity"] is { } identity ? new SpechtPathIdentity(Segment(identity["epic"]), Segment(identity["feature"])) : null);
    }

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static int? Segment(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<int>(out var index) && index >= 0 ? index : null;

    /// <implements epic="0001" feature="F5" claim="B-048" />
    /// <implements epic="0001" feature="F5" claim="B-074" />
    private static IEnumerable<string> IdentityFaults(JsonObject manifest, bool annotations)
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
                if (annotations && member.StartsWith('$'))
                {
                    continue;
                }

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


    /// <implements epic="0001" feature="F5" claim="B-048" />
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
            $"{SpechtManifestStore.RelativePath}: schemaVersion {node?.ToJsonString() ?? "null"} is not a major.minor.patch version such as \"0.1.0\".");
    }

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static SemanticVersion? ReadVersion(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var text) && SemanticVersion.TryParse(text, out var version) ? version : null;

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static List<string> Strings(JsonNode node) =>
        node.AsArray().Select(static element => Present(element).GetValue<string>()).ToList();

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static Dictionary<string, IReadOnlyList<string>> Tables(JsonNode node) =>
        node.AsObject().ToDictionary(
            static entry => entry.Key,
            static entry => (IReadOnlyList<string>) Strings(Present(entry.Value)),
            StringComparer.Ordinal);

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static Dictionary<string, string> Named(JsonNode node) =>
        node.AsObject().ToDictionary(
            static entry => entry.Key,
            static entry => Present(entry.Value).GetValue<string>(),
            StringComparer.Ordinal);

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static Dictionary<string, string> Levels(JsonNode? node) =>
        node is null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : node.AsObject().ToDictionary(
                static entry => entry.Key,
                static entry => entry.Value is JsonValue value && value.TryGetValue<string>(out var level)
                    ? level
                    : entry.Value?.ToJsonString() ?? "null",
                StringComparer.Ordinal);

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static JsonNode Present(JsonNode? node) =>
        node ?? throw new InvalidOperationException("A JSON null stands where the manifest's shape requires a value.");

    /// <implements epic="0001" feature="F5" claim="B-048" />
    /// <implements epic="0001" feature="F5" claim="B-073" />
    /// <implements epic="0001" feature="F5" claim="B-075" />
    private static void Check(
        SpechtStructure structure,
        JsonObject defaults,
        IEnumerable<string> identityFaults,
        IReadOnlyDictionary<string, string> levels,
        bool blanks)
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
                    .Where(marker => blanks ? string.IsNullOrWhiteSpace(structure.Markers[marker]) : structure.Markers[marker].Length == 0)
                    .Select(marker => structure.Markers[marker].Length == 0
                        ? $"the marker '{marker}' in markers is empty"
                        : $"the marker '{marker}' in markers is white space alone"))
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
                    .Where(schema => schema.Value.Contains("..", StringComparison.Ordinal)
                        || schema.Value.Contains('/') || schema.Value.Contains('\\')
                        || (blanks && schema.Value is "" or "."))
                    .Select(static schema =>
                        $"the frontmatterSchemas kind '{schema.Key}' names '{schema.Value}', which is not a plain file name"))
            .Concat(
                levels
                    .Where(static rule => rule.Value is not ("error" or "warning" or "off"))
                    .Select(static rule => $"the rule '{rule.Key}' in rules is set to '{rule.Value}', which is not 'error', 'warning' or 'off'"))
            .ToList();

        if (faults.Count > 0)
        {
            throw new SpechtManifestException($"{SpechtManifestStore.RelativePath}: {string.Join("; ", faults)}.");
        }
    }

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static SpechtDiscoveryInputs Discovery(JsonObject manifest, JsonObject defaults, JsonObject? document) =>
        new(
            Selected(manifest, defaults, "layouts", document).AsArray().Select(Layout).ToList(),
            Strings(Selected(manifest, defaults, "exclusions", document)),
            Strings(Selected(manifest, defaults, "taskFiles", document)),
            Strings(Selected(manifest, defaults, "epicFiles", document)),
            Strings(Selected(manifest, defaults, "companionFiles", document)));

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private static Dictionary<string, string> Filled(JsonObject manifest, JsonObject defaults, string key, JsonObject? document)
    {
        var values = Named(defaults[key]!);

        if (manifest[key] is { } declared)
        {
            foreach (var (name, value) in Named(declared))
            {
                values[name] = value;
            }
        }

        if (document is not null && (!manifest.ContainsKey(key) || manifest[key] is not null))
        {
            if (!document.ContainsKey(key))
            {
                document[key] = new JsonObject();
            }

            var projected = document[key]!.AsObject();
            foreach (var (name, value) in values)
            {
                if (!projected.ContainsKey(name) && !(key == "identifiers" && name == "epic"))
                {
                    projected[name] = value;
                }
            }
        }

        return values;
    }

    /// <implements epic="0001" feature="F9" claim="B-004" />
    private static JsonNode Selected(JsonObject? manifest, JsonObject defaults, string key, JsonObject? document)
    {
        var selected = manifest?[key] ?? Present(defaults[key]);
        if (document is not null && manifest?.ContainsKey(key) != true)
        {
            document[key] = selected.DeepClone();
        }

        return selected;
    }

    /// <implements epic="0001" feature="F5" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-025" />
    /// <implements epic="0001" feature="F5" claim="B-048" />
    /// <implements epic="0001" feature="F5" claim="B-049" />
    /// <implements epic="0001" feature="F7" claim="B-145" />
    /// <implements epic="0001" feature="F7" claim="B-153" />
    /// <implements epic="0001" feature="F19" claim="B-014" />
    /// <implements epic="0001" feature="F19" claim="B-044" />
    /// <implements epic="0001" feature="F5" claim="B-082" />
    /// <implements epic="0001" feature="F5" claim="B-092" />
    private SpechtStructure Parse(string text, bool includeDocument, out JsonObject? document)
    {
        var manifest = Present(JsonNode.Parse(text)).AsObject();
        document = includeDocument ? manifest.DeepClone().AsObject() : null;
        var number = ReadVersion(manifest["schemaVersion"]) ?? OriginalVersion;
        if ((manifest.ContainsKey("schemaSource") || manifest.ContainsKey("pinPolicy")) && versions.Find(number) is null)
        {
            _ = Version(number);
        }

        var tags = number.CompareTo(ClaimTagVersion) >= 0 && versions.Find(number) is not null;
        var selection = number.CompareTo(SchemaSourceVersion) >= 0 && versions.Find(number) is not null;
        var matrix = number.CompareTo(MatrixKeyVersion) >= 0 && versions.Find(number) is not null;
        var policy = number.CompareTo(PinPolicyVersion) >= 0 && versions.Find(number) is not null;
        var shipped = versions.Find(number);
        var selected = shipped ?? versions.Find(OriginalVersion) ?? Version(number);
        var defaults = JsonNode.Parse(selected.Manifest)!.AsObject();
        var keysAndForms = number.CompareTo(KeysAndFormsVersion) >= 0 && shipped is not null
            && defaults.ContainsKey("keys") && defaults.ContainsKey("forms");
        var blanks = number.CompareTo(BlankValuesVersion) >= 0 && shipped is not null;
        var mint = number.CompareTo(MintVersion) >= 0 && shipped is not null;
        var claimPrefix = number.CompareTo(ClaimPrefixVersion) >= 0 && shipped is not null;
        var ids = number.CompareTo(IdsVersion) >= 0 && shipped is not null && defaults.ContainsKey("ids");
        var unknown = manifest
            .Select(static entry => entry.Key)
            .Where(key => !key.StartsWith('$') && !KnownKeys.Contains(key)
                && !(tags && key == "claim") && !(selection && key == "schemaSource") && !(matrix && MatrixKeys.Contains(key))
                && !(policy && key == "pinPolicy")
                && !(keysAndForms && key is "keys" or "forms")
                && !(mint && key == "mint")
                && !(ids && key == "ids"))
            .Select(static key => $"'{key}'")
            .ToList();

        if (unknown.Count > 0)
        {
            throw new SpechtManifestException(
                $"{SpechtManifestStore.RelativePath}: the engine does not know the key {string.Join(", ", unknown)}.");
        }

        var tables = Tables(Selected(manifest, defaults, "tables", document));
        if (matrix)
        {
            foreach (var (role, headers) in Tables(defaults["tables"]!))
            {
                if (tables.TryAdd(role, headers) && document?["tables"] is JsonObject projectedTables)
                {
                    projectedTables[role] = defaults["tables"]![role]!.DeepClone();
                }
            }
        }

        var frontmatterSchemas = Filled(manifest, defaults, "frontmatterSchemas", document);
        var claim = tags ? manifest["claim"]?.AsObject() : null;
        var claimDefaults = tags ? Present(defaults["claim"]).AsObject() : null;
        JsonObject? claimDocument = null;
        if (tags && document is not null && (!manifest.ContainsKey("claim") || manifest["claim"] is not null))
        {
            if (!document.ContainsKey("claim"))
            {
                document["claim"] = new JsonObject();
            }

            claimDocument = document["claim"]!.AsObject();
        }

        var sourceFaults = new List<string>();
        Dictionary<string, string> levels = [];
        var structure = new SpechtStructure(
            Strings(Selected(manifest, defaults, "sections", document)),
            tables,
            Filled(manifest, defaults, "identifiers", document),
            SchemaVersion(manifest))
        {
            Discovery = Discovery(manifest, defaults, document),
            FrontmatterSchemas = frontmatterSchemas,
            Roles = Filled(manifest, defaults, "roles", document),
            Markers = Filled(manifest, defaults, "markers", document),
            Rules = new SpechtRuleSettings((levels = Levels(manifest["rules"])).Where(static entry => entry.Value is "error" or "warning" or "off")
                .Select(static entry => new KeyValuePair<string, SpechtRuleLevel>(entry.Key, entry.Value switch
                {
                    "warning" => SpechtRuleLevel.Warning,
                    "off" => SpechtRuleLevel.Off,
                    _ => SpechtRuleLevel.Error,
                }))),
            ClaimTagForm = tags ? Selected(claim, claimDefaults!, "tag", claimDocument).GetValue<string>() : null,
            ClaimTagIgnoreCase = tags && Selected(claim, claimDefaults!, "tagIgnoreCase", claimDocument).GetValue<bool>(),
            SchemaSource = SourceSelection(manifest, sourceFaults),
            PinPolicy = Policy(manifest, number, sourceFaults),
            DeclaresEpicGrammar = manifest["identifiers"]?["epic"] is not null,
            SpechtSchema = SpechtSchema(manifest, frontmatterSchemas, versions.Find(number), sourceFaults),
            Tests = matrix ? TestSettings(manifest["tests"]) : null,
            MatrixScenarioTitles = matrix && manifest["matrix"]?["scenarioTitles"] is { } titles && titles.GetValue<bool>(),
            Owners = matrix && manifest["owners"] is { } owners ? Tables(owners) : new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal),
            RequiredSpecifications = matrix && manifest["requiredSpecifications"] is { } required ? Strings(required) : [],
            ConsumerTemplates = manifest["consumerTemplates"] is { } templates ? Strings(templates) : [],
        };

        if (structure.SchemaSource == SchemaSourceSelection.Embedded && structure.SpechtSchema is not null)
        {
            sourceFaults.Add("schemaSource 'embedded' contradicts the recorded spechtSchema source");
        }

        Dictionary<string, string> forms = [];
        if (keysAndForms)
        {
            var feature = KeysOf(manifest, defaults, "feature", document);
            var task = KeysOf(manifest, defaults, "task", document);
            forms = Filled(manifest, defaults, "forms", document);
            structure = structure with
            {
                FeatureKeys = new SpechtFeatureKeys(
                    feature["id"],
                    feature["epic"],
                    feature["specStatus"],
                    feature["status"],
                    feature["children"],
                    feature["spikes"],
                    feature["dependsOn"],
                    feature["blocks"],
                    claimPrefix ? feature.GetValueOrDefault("claimPrefix") : null),
                TaskKeys = new SpechtTaskKeys(task["id"], task["parent"], task["type"]),
                Forms = new SpechtForms(
                    structure.DeclaresEpicGrammar ? forms["identity"] : SpechtForms.IdPlaceholder,
                    forms["localEdge"],
                    forms["qualifiedEdge"]),
            };
        }

        List<string> idFaults = [];
        if (ids)
        {
            structure = structure with { Ids = Ids(manifest, defaults, structure.TagComparison, idFaults, document) };
            idFaults.AddRange(OldIdFaults(manifest));
        }

        Check(
            structure,
            defaults,
            IdentityFaults(manifest, blanks)
                .Concat(idFaults)
                .Concat(sourceFaults)
                .Concat(ClaimTagFaults(structure))
                .Concat(MatrixKeyFaults(structure))
                .Concat(keysAndForms ? GrammarFaults(structure).Concat(FormFaults(forms)) : []),
            levels,
            blanks);

        if (document is not null && !manifest.ContainsKey("schemaVersion"))
        {
            document["schemaVersion"] = structure.SchemaVersion.ToString();
        }

        return structure;
    }

    /// <implements epic="0001" feature="F5" claim="B-048" />
    private SchemaVersion Version(SemanticVersion number) =>
        versions.Find(number) ?? throw new SpechtManifestException(
            $"{SpechtManifestStore.RelativePath}: schemaVersion {number} is not shipped; this tool ships "
            + $"{string.Join(", ", versions.Versions.Select(static version => version.Number))}.");

    private static readonly HashSet<string> KnownKeys = new(StringComparer.Ordinal)
    {
        "schemaVersion",
        "codeAnalysis",
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
        "consumerTemplates",
    };

    private static readonly SemanticVersion ClaimTagVersion = new(0, 2, 0);

    private static readonly SemanticVersion SchemaSourceVersion = new(0, 2, 0);

    private static readonly SemanticVersion MatrixKeyVersion = new(0, 2, 0);

    private static readonly SemanticVersion PinPolicyVersion = new(0, 3, 0);

    private static readonly HashSet<string> MatrixKeys = new(StringComparer.Ordinal) { "tests", "matrix", "owners", "requiredSpecifications" };

    private static readonly SemanticVersion OriginalVersion = new(0, 1, 0);

    private static readonly SemanticVersion KeysAndFormsVersion = new(0, 3, 0);

    private static readonly SemanticVersion BlankValuesVersion = new(0, 3, 0);

    private static readonly SemanticVersion MintVersion = new(0, 3, 1);

    private static readonly SemanticVersion ClaimPrefixVersion = new(0, 3, 1);

    private static readonly SemanticVersion IdsVersion = new(0, 4, 0);

    private static readonly char[] Braces = ['{', '}'];
}
