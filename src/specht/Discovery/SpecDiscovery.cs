using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace Specht.Discovery;

/// <summary>
/// Finds the repository's specifications, items, epics and companion files where the manifest says they are
/// (<c>0001-F6</c> B-001, B-002, B-003, B-012).
/// </summary>
/// <param name="fileSystem">The file system the tree is read through.</param>
public sealed class SpecDiscovery(IFileSystem fileSystem)
{
    /// <summary>
    /// Discovers every specification under <paramref name="root"/> in the layouts <paramref name="inputs"/> declares,
    /// skipping what it excludes (<c>0001-F6</c> B-001, B-002, C-6, C-7).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>Each specification, carrying the layout it was found in.</returns>
    public IReadOnlyList<SpecLocation> FindSpecifications(string root, SpecDiscoveryInputs inputs)
    {
        var files = Files(root);

        return inputs.Layouts
            .SelectMany(layout => Matching(files, [layout.Glob], inputs.Exclusions)
                .Select(file => new SpecLocation(file.Path, file.Relative, layout, fileSystem.Path.GetDirectoryName(file.Path)!)))
            .ToList();
    }

    /// <summary>
    /// Discovers the item files beside each specification: every file whose name matches one of the manifest's task file
    /// shapes, <c>{task}</c> standing for <paramref name="taskGrammar"/> (<c>0001-F6</c> B-003, B-012).
    /// </summary>
    /// <param name="specifications">The discovered specifications.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <param name="taskGrammar">The manifest's <c>identifiers.task</c> grammar.</param>
    /// <returns>The path of each item file.</returns>
    public IReadOnlyList<string> FindChildItems(
        IEnumerable<SpecLocation> specifications,
        SpecDiscoveryInputs inputs,
        string taskGrammar)
    {
        var task = $"(?:{taskGrammar.TrimStart('^').TrimEnd('$')})";
        var shapes = inputs.TaskFiles
            .Select(shape => new Regex(Pattern(shape).Replace(@"\{task}", task, StringComparison.Ordinal)))
            .ToList();

        return specifications.SelectMany(specification => Beside(specification, shapes)).ToList();
    }

    /// <summary>
    /// Discovers every epic file under <paramref name="root"/>: each file one of the manifest's epic globs matches and
    /// its exclusions do not skip (<c>0001-F6</c> B-002, B-003, B-012).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>The path of each epic file.</returns>
    public IReadOnlyList<string> FindEpics(string root, SpecDiscoveryInputs inputs) =>
        Matching(Files(root), inputs.EpicFiles, inputs.Exclusions).Select(static file => file.Path).ToList();

    /// <summary>
    /// Discovers the companion files beside <paramref name="specification"/>: every file whose name matches one of the
    /// manifest's companion globs (<c>0001-F6</c> B-003, B-012).
    /// </summary>
    /// <param name="specification">The specification.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>The path of each companion file.</returns>
    public IReadOnlyList<string> FindCompanions(SpecLocation specification, SpecDiscoveryInputs inputs) =>
        Beside(specification, inputs.CompanionFiles.Select(static glob => new Regex(Pattern(glob))).ToList()).ToList();

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

    private List<(string Path, string Relative)> Files(string root)
    {
        var origin = fileSystem.Path.GetFullPath(root);
        var separator = fileSystem.Path.DirectorySeparatorChar;

        return fileSystem.Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => (path, fileSystem.Path.GetRelativePath(origin, fileSystem.Path.GetFullPath(path)).Replace(separator, '/')))
            .ToList();
    }

    private IEnumerable<string> Beside(SpecLocation specification, List<Regex> names) =>
        fileSystem.Directory.EnumerateFiles(specification.Directory)
            .Where(path => names.Any(name => name.IsMatch(fileSystem.Path.GetFileName(path))))
            .Order(StringComparer.Ordinal);
}
