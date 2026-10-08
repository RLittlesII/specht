namespace specht;

/// <summary>
/// Finds the repository's specifications, in both layouts at once.
/// </summary>
/// <remarks>
/// Both layouts coexist for as long as the migration runs, and neither is
/// treated as second class: the same rules apply to each, and a specification
/// never fails for being un-migrated. The migration itself is reported as a
/// count, not as a violation.
/// </remarks>
public static class SpecDiscovery
{
    /// <summary>Discovers every specification under <paramref name="root"/>.</summary>
    public static IReadOnlyList<SpecLocation> FindSpecifications(string root)
    {
        var found = new List<SpecLocation>();
        var epics = Path.Combine(root, "epics");

        if (Directory.Exists(epics))
        {
            foreach (var path in Directory.EnumerateFiles(epics, "spec.md", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
            {
                found.Add(Location(root, path, SpecLayout.Legacy));
            }
        }

        foreach (var directory in SpecDirectories(root))
        {
            var readme = Path.Combine(directory, "README.md");

            if (File.Exists(readme))
            {
                found.Add(Location(root, readme, SpecLayout.CoLocated));
            }
        }

        return found;
    }

    /// <summary>
    /// Discovers the task, test and spike files beside each specification.
    /// </summary>
    /// <remarks>
    /// Scoped to the directories that hold a specification on purpose. A bare
    /// <c>&lt;dddd&gt;-&lt;dd&gt;-*.md</c> glob also matches the date-named
    /// grooming records under <c>epics/audits/</c>, which carry no frontmatter
    /// and are not items.
    /// </remarks>
    public static IReadOnlyList<string> FindChildItems(IEnumerable<SpecLocation> specifications)
    {
        var found = new List<string>();

        foreach (var specification in specifications)
        {
            foreach (var path in Directory.EnumerateFiles(specification.Directory, "*.md").Order(StringComparer.Ordinal))
            {
                if (IsItemFileName(Path.GetFileName(path)))
                {
                    found.Add(path);
                }
            }
        }

        return found;
    }

    /// <summary>Whether <paramref name="fileName"/> is an <c>&lt;epic&gt;-&lt;nn&gt;-&lt;slug&gt;.md</c> item file.</summary>
    public static bool IsItemFileName(string fileName)
    {
        if (fileName.Length < 11 || !fileName.EndsWith(".md", StringComparison.Ordinal))
        {
            return false;
        }

        var span = fileName.AsSpan();

        return char.IsAsciiDigit(span[0])
            && char.IsAsciiDigit(span[1])
            && char.IsAsciiDigit(span[2])
            && char.IsAsciiDigit(span[3])
            && span[4] == '-'
            && char.IsAsciiDigit(span[5])
            && char.IsAsciiDigit(span[6])
            && span[7] == '-';
    }

    private static IEnumerable<string> SpecDirectories(string root)
    {
        var rootSpec = Path.Combine(root, ".spec");

        foreach (var directory in Directory.EnumerateDirectories(root, ".spec", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            // The repository-wide .spec/ holds adr/, lessons/, templates/ and
            // schema/ - records about every Feature, not one Feature's spec.
            if (string.Equals(directory, rootSpec, StringComparison.Ordinal))
            {
                continue;
            }

            if (!IsExcluded(root, directory))
            {
                yield return directory;
            }
        }
    }

    private static bool IsExcluded(string root, string path) =>
        Path.GetRelativePath(root, path)
            .Split(Path.DirectorySeparatorChar)
            .Any(static segment => ExcludedDirectories.Contains(segment, StringComparer.Ordinal));

    private static SpecLocation Location(string root, string path, SpecLayout layout) =>
        new(path, Relative(root, path), layout, Path.GetDirectoryName(path)!);

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');

    private static readonly string[] ExcludedDirectories =
        [".git", ".artifacts", ".skillfile", ".claude", "graphify-out", "bin", "obj", "node_modules"];
}
