using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace Specht.Discovery;

/// <summary>
/// Finds the repository's specifications, items, epics and companion files where the manifest says they are
/// (<c>0001-F6</c> B-001, B-002, B-003, B-012).
/// </summary>
public static class SpecDiscovery
{
    /// <summary>
    /// Discovers every specification under <paramref name="root"/> in the layouts <paramref name="inputs"/> declares,
    /// skipping what it excludes (<c>0001-F6</c> B-001, B-002, C-6, C-7).
    /// </summary>
    /// <param name="fileSystem">The file system the tree is read through.</param>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>Each specification, carrying the layout it was found in.</returns>
    public static IReadOnlyList<SpecLocation> FindSpecifications(IFileSystem fileSystem, string root, SpecDiscoveryInputs inputs)
    {
        var files = Files(fileSystem, root);

        return inputs.Layouts
            .SelectMany(layout => Matching(files, [layout.Glob], inputs.Exclusions)
                .Select(file => new SpecLocation(file.Path, file.Relative, layout, fileSystem.Path.GetDirectoryName(file.Path)!)))
            .ToList();
    }

    /// <summary>
    /// Discovers the item files beside each specification: every file whose name matches one of the manifest's task file
    /// shapes, <c>{task}</c> standing for <paramref name="taskGrammar"/> (<c>0001-F6</c> B-003, B-012).
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
        var shapes = inputs.TaskFiles
            .Select(shape => new Regex(Pattern(shape).Replace(@"\{task}", task, StringComparison.Ordinal)))
            .ToList();

        return specifications.SelectMany(specification => Beside(fileSystem, specification, shapes)).ToList();
    }

    /// <summary>
    /// Discovers every epic file under <paramref name="root"/>: each file one of the manifest's epic globs matches and
    /// its exclusions do not skip (<c>0001-F6</c> B-002, B-003, B-012).
    /// </summary>
    /// <param name="fileSystem">The file system the tree is read through.</param>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>The path of each epic file.</returns>
    public static IReadOnlyList<string> FindEpics(IFileSystem fileSystem, string root, SpecDiscoveryInputs inputs) =>
        Matching(Files(fileSystem, root), inputs.EpicFiles, inputs.Exclusions).Select(static file => file.Path).ToList();

    /// <summary>
    /// Discovers the companion files beside <paramref name="specification"/>: every file whose name matches one of the
    /// manifest's companion globs (<c>0001-F6</c> B-003, B-012).
    /// </summary>
    /// <param name="fileSystem">The file system the tree is read through.</param>
    /// <param name="specification">The specification.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>The path of each companion file.</returns>
    public static IReadOnlyList<string> FindCompanions(IFileSystem fileSystem, SpecLocation specification, SpecDiscoveryInputs inputs) =>
        Beside(fileSystem, specification, inputs.CompanionFiles.Select(static glob => new Regex(Pattern(glob))).ToList()).ToList();

    /// <summary>Gets <paramref name="path"/> relative to <paramref name="root"/>, with <c>/</c> separators (<c>0001-F3</c> B-021).</summary>
    /// <param name="root">The root the tool was given.</param>
    /// <param name="path">A path under it.</param>
    /// <returns>The root-relative path.</returns>
    public static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');

    private static List<(string Path, string Relative)> Files(IFileSystem fileSystem, string root)
    {
        var origin = fileSystem.Path.GetFullPath(root);
        var separator = fileSystem.Path.DirectorySeparatorChar;

        return fileSystem.Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => (path, fileSystem.Path.GetRelativePath(origin, fileSystem.Path.GetFullPath(path)).Replace(separator, '/')))
            .ToList();
    }

    private static IEnumerable<(string Path, string Relative)> Matching(
        List<(string Path, string Relative)> files,
        IReadOnlyList<string> globs,
        IReadOnlyList<string> exclusions)
    {
        var expressions = globs.Select(static glob => new Regex(Pattern(glob))).ToList();

        return files
            .Where(file => expressions.Any(expression => expression.IsMatch(file.Relative)) && !IsExcluded(file.Relative, exclusions))
            .OrderBy(static file => file.Relative, StringComparer.Ordinal);
    }

    private static IEnumerable<string> Beside(IFileSystem fileSystem, SpecLocation specification, List<Regex> names) =>
        fileSystem.Directory.EnumerateFiles(specification.Directory)
            .Where(path => names.Any(name => name.IsMatch(fileSystem.Path.GetFileName(path))))
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
}
