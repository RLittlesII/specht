using System.IO.Abstractions;
using System.Text.RegularExpressions;

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
        string taskGrammar)
    {
        var task = $"(?:{taskGrammar.TrimStart('^').TrimEnd('$')})";
        var shape = new Regex(Pattern(inputs.TaskFiles).Replace(@"\{task}", task, StringComparison.Ordinal));

        return specifications.SelectMany(specification => Beside(fileSystem, specification, shape)).ToList();
    }

    /// <summary>
    /// Discovers every epic file under <paramref name="root"/>: each file the manifest's epic glob matches and its
    /// exclusions do not skip (<c>0001-F6</c> B-002, B-003).
    /// </summary>
    /// <param name="fileSystem">The file system the tree is read through.</param>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>The path of each epic file.</returns>
    public static IReadOnlyList<string> FindEpics(IFileSystem fileSystem, string root, SpecDiscoveryInputs inputs) =>
        Matching(fileSystem, root, inputs.EpicFiles, inputs.Exclusions).Select(static file => file.Path).ToList();

    /// <summary>
    /// Discovers the companion files beside <paramref name="specification"/>: every file whose name matches the manifest's
    /// companion glob (<c>0001-F6</c> B-003).
    /// </summary>
    /// <param name="fileSystem">The file system the tree is read through.</param>
    /// <param name="specification">The specification.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>The path of each companion file.</returns>
    public static IReadOnlyList<string> FindCompanions(IFileSystem fileSystem, SpecLocation specification, SpecDiscoveryInputs inputs) =>
        Beside(fileSystem, specification, new Regex(Pattern(inputs.CompanionFiles))).ToList();

    /// <summary>Gets <paramref name="path"/> relative to <paramref name="root"/>, with <c>/</c> separators (<c>0001-F3</c> B-021).</summary>
    /// <param name="root">The root the tool was given.</param>
    /// <param name="path">A path under it.</param>
    /// <returns>The root-relative path.</returns>
    public static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');

    private static IEnumerable<(string Path, string Relative)> Matching(
        IFileSystem fileSystem,
        string root,
        string glob,
        IReadOnlyList<string> exclusions)
    {
        var expression = new Regex(Pattern(glob));
        var origin = fileSystem.Path.GetFullPath(root);

        var separator = fileSystem.Path.DirectorySeparatorChar;

        return fileSystem.Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => (Path: path, Relative: fileSystem.Path.GetRelativePath(origin, fileSystem.Path.GetFullPath(path)).Replace(separator, '/')))
            .Where(file => expression.IsMatch(file.Relative) && !IsExcluded(file.Relative, exclusions))
            .OrderBy(static file => file.Relative, StringComparer.Ordinal);
    }

    private static IEnumerable<string> Beside(IFileSystem fileSystem, SpecLocation specification, Regex name) =>
        fileSystem.Directory.EnumerateFiles(specification.Directory)
            .Where(path => name.IsMatch(fileSystem.Path.GetFileName(path)))
            .Order(StringComparer.Ordinal);

    private static string Pattern(string glob)
    {
        var segments = glob.Split('/').Select(static segment =>
            segment == "**" ? "(?:[^/]+/)*" : Regex.Escape(segment).Replace(@"\*", "[^/]*", StringComparison.Ordinal) + "/");

        return @"\A" + string.Concat(segments).TrimEnd('/') + @"\z";
    }

    private static bool IsExcluded(string relative, IReadOnlyList<string> exclusions)
    {
        var directories = relative.Split('/')[..^1];
        var anchored = $"/{string.Join('/', directories)}/";

        return exclusions.Any(entry => entry.StartsWith('/')
            ? anchored.StartsWith(entry + "/", StringComparison.Ordinal)
            : directories.Contains(entry, StringComparer.Ordinal));
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

    private static readonly string[] ExcludedDirectories =
        [".git", ".artifacts", ".skillfile", ".claude", "graphify-out", "bin", "obj", "node_modules"];
}
