using System.IO.Abstractions;

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
    /// Discovers every specification under <paramref name="root"/> in the layouts <paramref name="inputs"/> declares,
    /// skipping what it excludes (<c>0001-F6</c> B-001, B-002, C-6, C-7).
    /// </summary>
    /// <param name="fileSystem">The file system the tree is read through.</param>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>Each specification, carrying the layout it was found in.</returns>
    public static IReadOnlyList<SpecLocation> FindSpecifications(IFileSystem fileSystem, string root, SpecDiscoveryInputs inputs) =>
        throw new NotImplementedException("0005: discovery does not read its layouts and exclusions from the manifest yet.");

    /// <summary>
    /// Discovers the item files beside each specification: every file whose name matches the manifest's task file shape,
    /// <c>{task}</c> standing for <paramref name="taskGrammar"/> (<c>0001-F6</c> B-003).
    /// </summary>
    /// <param name="fileSystem">The file system the tree is read through.</param>
    /// <param name="specifications">The discovered specifications.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <param name="taskGrammar">The manifest's <c>identifiers.task</c> grammar.</param>
    /// <returns>The path of each item file.</returns>
    public static IReadOnlyList<string> FindChildItems(
        IFileSystem fileSystem,
        IEnumerable<SpecLocation> specifications,
        SpecDiscoveryInputs inputs,
        string taskGrammar) =>
        throw new NotImplementedException("0005: discovery does not read the task file shape from the manifest yet.");

    /// <summary>
    /// Discovers every epic file under <paramref name="root"/>: each file the manifest's epic glob matches and its
    /// exclusions do not skip (<c>0001-F6</c> B-002, B-003).
    /// </summary>
    /// <param name="fileSystem">The file system the tree is read through.</param>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>The path of each epic file.</returns>
    public static IReadOnlyList<string> FindEpics(IFileSystem fileSystem, string root, SpecDiscoveryInputs inputs) =>
        throw new NotImplementedException("0005: discovery does not read the epic file glob from the manifest yet.");

    /// <summary>
    /// Discovers the companion files beside <paramref name="specification"/>: every file whose name matches the manifest's
    /// companion glob (<c>0001-F6</c> B-003).
    /// </summary>
    /// <param name="fileSystem">The file system the tree is read through.</param>
    /// <param name="specification">The specification.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>The path of each companion file.</returns>
    public static IReadOnlyList<string> FindCompanions(IFileSystem fileSystem, SpecLocation specification, SpecDiscoveryInputs inputs) =>
        throw new NotImplementedException("0005: discovery does not read the companion glob from the manifest yet.");

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

    /// <summary>Gets <paramref name="path"/> relative to <paramref name="root"/>, with <c>/</c> separators (<c>0001-F3</c> B-021).</summary>
    /// <param name="root">The root the tool was given.</param>
    /// <param name="path">A path under it.</param>
    /// <returns>The root-relative path.</returns>
    public static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');

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

    private static readonly string[] ExcludedDirectories =
        [".git", ".artifacts", ".skillfile", ".claude", "graphify-out", "bin", "obj", "node_modules"];
}
