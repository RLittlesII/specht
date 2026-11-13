namespace Specht.Versioning;

/// <summary>The schema versions a check can select from, ascending by version (<c>0001-F7</c> B-001, B-003, B-004).</summary>
/// <implements epic="0001" feature="F7" claim="B-001" />
public sealed class SchemaVersions
{
    /// <summary>A set holding <paramref name="versions"/>.</summary>
    /// <param name="versions">The versions, in any order.</param>
    /// <implements epic="0001" feature="F7" claim="B-001" />
    public SchemaVersions(IEnumerable<SchemaVersion> versions) =>
        Versions = versions.OrderBy(static version => version.Number).ToList();

    /// <summary>Assembles shipped versions from an already-read raw resource snapshot.</summary>
    /// <param name="resources">The raw schema and rule-page texts by logical name.</param>
    /// <implements epic="0001" feature="F7" claim="B-001" />
    public SchemaVersions(IReadOnlyDictionary<string, string> resources) : this(ReadVersions(resources)) { }

    /// <summary>The versions, ascending by major, minor and patch.</summary>
    public IReadOnlyList<SchemaVersion> Versions { get; }

    /// <summary>Finds the exact version, or returns null when it is absent.</summary>
    /// <param name="number">The exact version number.</param>
    /// <returns>The held version, or null.</returns>
    /// <implements epic="0001" feature="F7" claim="B-001" />
    public SchemaVersion? Find(SemanticVersion number) => Versions.FirstOrDefault(version => version.Number == number);

    /// <summary>Selects the held version the check uses under <paramref name="policy"/> for a held pin (B-053, B-054, B-147).</summary>
    /// <param name="pin">The pinned version, which the set holds.</param>
    /// <param name="policy">The manifest's pin policy.</param>
    /// <returns>The version in use.</returns>
    /// <implements epic="0001" feature="F7" claim="B-053" />
    /// <implements epic="0001" feature="F7" claim="B-054" />
    /// <implements epic="0001" feature="F7" claim="B-147" />
    public SchemaVersion InUse(SemanticVersion pin, PinPolicy policy) =>
        Versions.Last(version => version.Number.CompareTo(pin) >= 0 && policy switch
        {
            PinPolicy.LatestPatch => version.Number.Major == pin.Major && version.Number.Minor == pin.Minor,
            PinPolicy.LatestMinor => version.Number.Major == pin.Major,
            _ => version.Number == pin,
        });

    /// <implements epic="0001" feature="F7" claim="B-001" />
    private static IEnumerable<SchemaVersion> ReadVersions(IReadOnlyDictionary<string, string> resources)
    {
        var folders = resources.Keys
            .Where(static name => name.StartsWith(Prefix, StringComparison.Ordinal))
            .Select(static name => name[Prefix.Length..name.IndexOf('/', Prefix.Length)])
            .Distinct();

        return folders.Select(folder =>
        {
            var number = SemanticVersion.TryParse(folder, out var version)
                ? version
                : throw new InvalidOperationException($"schema/v{folder}/ is embedded with an invalid schema version.");
            var names = Shipped(number);
            return new SchemaVersion(
                number,
                resources[$"{Prefix}{folder}/frontmatter/feature.schema.json"],
                resources[$"{Prefix}{folder}/frontmatter/task.schema.json"],
                resources[$"{Prefix}{folder}/frontmatter/epic.schema.json"],
                names.Values.ToHashSet(StringComparer.Ordinal),
                resources[$"manifest/v{folder}/manifest.json"])
            {
                RuleNames = names,
                RulePages = resources.Keys
                    .Where(name => name.StartsWith($"{PagePrefix}{folder}/", StringComparison.Ordinal))
                    .ToDictionary(static name => Path.GetFileNameWithoutExtension(name), name => resources[name], StringComparer.Ordinal),
                Templates = resources.Keys
                    .Where(name => name.StartsWith($"{TemplatePrefix}{folder}/", StringComparison.Ordinal))
                    .Select(static name => name[(name.LastIndexOf('/') + 1)..])
                    .ToHashSet(StringComparer.Ordinal),
            };
        });
    }

    /// <implements epic="0001" feature="F7" claim="B-001" />
    /// <implements epic="0001" feature="F7" claim="B-151" />
    /// <implements epic="0001" feature="F7" claim="B-152" />
    private static Dictionary<string, string> Shipped(SemanticVersion number)
    {
        string[] original =
        [
            "SPEC001", "SPEC002", "SPEC003", "SPEC004",
            "SPEC010", "SPEC011", "SPEC012", "SPEC013",
            "SPEC020", "SPEC021",
            "SPEC030", "SPEC031",
            "SPEC040", "SPEC041", "SPEC043", "SPEC044",
            "SPEC050", "SPEC051", "SPEC052",
            "SPEC060", "SPEC061",
        ];
        return number switch
        {
            { Major: 0, Minor: 1, Patch: 0 } => original.ToDictionary(static id => id, StringComparer.Ordinal),
            { Major: 0, Minor: 2, Patch: 0 or 1 } =>
                original.Append("SPEC034").Append("SPEC035").ToDictionary(static id => id, StringComparer.Ordinal),
            { Major: 0, Minor: 3, Patch: 0 or 1 } or { Major: 0, Minor: 4, Patch: 0 } => new(StringComparer.Ordinal)
            {
                ["SPEC001"] = "SPEC1001",
                ["SPEC002"] = "SPEC1002",
                ["SPEC003"] = "SPEC1003",
                ["SPEC004"] = "SPEC1004",
                ["SPEC010"] = "SPEC2001",
                ["SPEC013"] = "SPEC2002",
                ["SPEC011"] = "SPEC3001",
                ["SPEC012"] = "SPEC3002",
                ["SPEC020"] = "SPEC4001",
                ["SPEC021"] = "SPEC4002",
                ["SPEC022"] = "SPEC4007",
                ["SPEC030"] = "SPEC4003",
                ["SPEC031"] = "SPEC4004",
                ["SPEC032"] = "SPEC4009",
                ["SPEC034"] = "SPEC4005",
                ["SPEC035"] = "SPEC4006",
                ["SPEC040"] = "SPEC5001",
                ["SPEC041"] = "SPEC5002",
                ["SPEC043"] = "SPEC5003",
                ["SPEC044"] = "SPEC5004",
                ["SPEC050"] = "SPEC6001",
                ["SPEC051"] = "SPEC6002",
                ["SPEC052"] = "SPEC6003",
                ["SPEC060"] = "SPEC7001",
                ["SPEC061"] = "SPEC7002",
            },
            _ => throw new InvalidOperationException($"schema/v{number}/ is embedded with no rule vocabulary."),
        };
    }

    private const string Prefix = "schema/v";
    private const string PagePrefix = "rules/v";
    private const string TemplatePrefix = "templates/v";
}
