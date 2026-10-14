namespace Specht.Versioning;

/// <summary>The schema versions a check can select from, ascending by version (<c>0001-F7</c> B-001, B-003, B-004).</summary>
public sealed class SchemaVersions
{
    /// <summary>A set holding <paramref name="versions"/>.</summary>
    /// <param name="versions">The versions, in any order.</param>
    public SchemaVersions(IEnumerable<SchemaVersion> versions) =>
        Versions = versions.OrderBy(static version => version.Number).ToList();

    /// <summary>Assembles shipped versions from an already-read raw resource snapshot.</summary>
    /// <param name="resources">The raw schema and rule-page texts by logical name.</param>
    public SchemaVersions(IReadOnlyDictionary<string, string> resources) : this(ReadVersions(resources)) { }

    /// <summary>The versions, ascending by major, minor and patch.</summary>
    public IReadOnlyList<SchemaVersion> Versions { get; }

    /// <summary>Finds the exact version, or returns null when it is absent.</summary>
    /// <param name="number">The exact version number.</param>
    /// <returns>The held version, or null.</returns>
    public SchemaVersion? Find(SemanticVersion number) => Versions.FirstOrDefault(version => version.Number == number);

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
            return new SchemaVersion(
                number,
                resources[$"{Prefix}{folder}/frontmatter/feature.schema.json"],
                resources[$"{Prefix}{folder}/frontmatter/task.schema.json"],
                resources[$"{Prefix}{folder}/frontmatter/epic.schema.json"],
                Shipped(number),
                resources[$"manifest/v{folder}/manifest.json"])
            {
                RulePages = resources.Keys
                    .Where(name => name.StartsWith($"{PagePrefix}{folder}/", StringComparison.Ordinal))
                    .ToDictionary(static name => Path.GetFileNameWithoutExtension(name), name => resources[name], StringComparer.Ordinal),
            };
        });
    }

    private static HashSet<string> Shipped(SemanticVersion number) =>
        number switch
        {
            { Major: 0, Minor: 1, Patch: 0 or 1 } or { Major: 0, Minor: 2, Patch: 0 } =>
                [
                    "SPEC001", "SPEC002", "SPEC003", "SPEC004",
                    "SPEC010", "SPEC011", "SPEC012", "SPEC013",
                    "SPEC020", "SPEC021",
                    "SPEC030", "SPEC031",
                    "SPEC040", "SPEC041", "SPEC043", "SPEC044",
                    "SPEC050", "SPEC051", "SPEC052",
                    "SPEC060", "SPEC061",
                ],
            _ => throw new InvalidOperationException($"schema/v{number}/ is embedded with no rule vocabulary."),
        };

    private const string Prefix = "schema/v";
    private const string PagePrefix = "rules/v";
}
