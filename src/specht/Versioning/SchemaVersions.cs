using Specht.Manifest;

namespace Specht.Versioning;

/// <summary>The schema versions a check can select from, ascending by version (<c>0001-F7</c> B-001, B-003, B-004).</summary>
public sealed class SchemaVersions
{
    /// <summary>A set holding <paramref name="versions"/>.</summary>
    /// <param name="versions">The versions, in any order.</param>
    public SchemaVersions(IEnumerable<SchemaVersion> versions) =>
        Versions = versions.OrderBy(static version => version.Number).ToList();

    /// <summary>Every version embedded in the engine, read from the versioned schema and rule-page resource folders.</summary>
    public static SchemaVersions Embedded { get; } = ReadEmbedded();

    /// <summary>The versions, ascending by major, minor and patch.</summary>
    public IReadOnlyList<SchemaVersion> Versions { get; }

    /// <summary>The version a manifest pins.</summary>
    /// <param name="number">The pinned version.</param>
    /// <returns>That version.</returns>
    /// <exception cref="SpechtManifestException">The set holds no version <paramref name="number"/>.</exception>
    public SchemaVersion Select(SemanticVersion number) =>
        Versions.FirstOrDefault(version => version.Number == number)
        ?? throw new SpechtManifestException(
            $"{SpecManifest.RelativePath}: schemaVersion {number} is not shipped; this tool ships "
                + $"{string.Join(", ", Versions.Select(static version => version.Number))}.");

    private static SchemaVersions ReadEmbedded()
    {
        var assembly = typeof(SchemaVersions).Assembly;
        var folders = assembly.GetManifestResourceNames()
            .Where(static name => name.StartsWith(Prefix, StringComparison.Ordinal))
            .Select(static name => name[Prefix.Length..name.IndexOf('/', Prefix.Length)])
            .Distinct();

        return new SchemaVersions(folders.Select(folder =>
        {
            var number = SemanticVersion.TryParse(folder, out var version)
                ? version
                : throw new InvalidOperationException($"schema/v{folder}/ is embedded with an invalid schema version.");
            var ruleIds = Shipped(number);

            return new SchemaVersion(
                number,
                Read($"{Prefix}{folder}/frontmatter/feature.schema.json"),
                Read($"{Prefix}{folder}/frontmatter/task.schema.json"),
                Read($"{Prefix}{folder}/frontmatter/epic.schema.json"),
                ruleIds)
            {
                RulePages = assembly.GetManifestResourceNames()
                    .Where(name => name.StartsWith($"{PagePrefix}{folder}/", StringComparison.Ordinal))
                    .ToDictionary(static name => Path.GetFileNameWithoutExtension(name), Read, StringComparer.Ordinal),
            };
        }));

        string Read(string name)
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }

    private static HashSet<string> Shipped(SemanticVersion number) =>
        number switch
        {
            { Major: 0, Minor: 1, Patch: 0 } =>
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
