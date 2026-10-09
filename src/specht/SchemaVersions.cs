using System.Globalization;

namespace specht;

/// <summary>The schema versions a check can select from, ascending by number (<c>0001-F7</c> B-001, B-003, B-004).</summary>
public sealed class SchemaVersions
{
    /// <summary>A set holding <paramref name="versions"/>.</summary>
    /// <param name="versions">The versions, in any order.</param>
    public SchemaVersions(IEnumerable<SchemaVersion> versions) =>
        Versions = versions.OrderBy(static version => version.Number).ToList();

    /// <summary>Every version embedded in the engine, read from its <c>schema/v&lt;n&gt;/</c> resources.</summary>
    public static SchemaVersions Embedded { get; } = ReadEmbedded();

    /// <summary>The versions, ascending by number.</summary>
    public IReadOnlyList<SchemaVersion> Versions { get; }

    /// <summary>The version a manifest pins.</summary>
    /// <param name="number">The pinned version.</param>
    /// <returns>That version.</returns>
    /// <exception cref="SpechtManifestException">The set holds no version <paramref name="number"/>.</exception>
    public SchemaVersion Select(int number) =>
        Versions.FirstOrDefault(version => version.Number == number)
        ?? throw new SpechtManifestException(
            $"{SpecManifest.RelativePath}: schemaVersion {number} is not shipped; this tool ships "
                + $"{string.Join(", ", Versions.Select(static version => version.Number))}.");

    private static SchemaVersions ReadEmbedded()
    {
        var assembly = typeof(SchemaVersions).Assembly;
        var numbers = assembly.GetManifestResourceNames()
            .Where(static name => name.StartsWith(Prefix, StringComparison.Ordinal))
            .Select(static name => int.Parse(name[Prefix.Length..name.IndexOf('/', Prefix.Length)], CultureInfo.InvariantCulture))
            .Distinct();

        return new SchemaVersions(numbers.Select(number => new SchemaVersion(
            number,
            Read(number, "feature-spec.frontmatter.schema.json"),
            Read(number, "task.frontmatter.schema.json"),
            Read(number, "epic.frontmatter.schema.json"),
            Vocabulary(number))));

        string Read(int number, string file)
        {
            using var stream = assembly.GetManifestResourceStream($"schema/v{number}/{file}")!;
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }

    private static HashSet<string> Vocabulary(int number) =>
        number switch
        {
            1 =>
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
}
