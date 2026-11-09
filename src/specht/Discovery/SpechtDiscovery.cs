using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace Specht.Discovery;

/// <summary>
/// Lists the repository's specifications, items, epics, companion files and declared test files where the manifest says
/// they are (<c>0001-F6</c> B-001, B-002, B-003, B-012), each by its root-relative path; it reads no file.
/// </summary>
/// <param name="fileSystem">The file system the tree is listed through.</param>
/// <param name="childProcess">The child process git is run through (<c>0001-F6</c> B-005, decision 0002).</param>
/// <implements epic="0001" feature="F6" claim="B-001" />
public sealed class SpechtDiscovery(IFileSystem fileSystem, IChildProcess childProcess) : ISpecDiscovery
{
    /// <summary>
    /// Discovers every specification under <paramref name="root"/> in the layouts <paramref name="inputs"/> declares,
    /// skipping what it excludes (<c>0001-F6</c> B-001, B-002, C-6, C-7).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>Each specification, carrying its root-relative path and directory and the layout it was found in.</returns>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    /// <implements epic="0001" feature="F6" claim="B-006" />
    public IReadOnlyList<SpechtLocation> FindSpecifications(string root, SpechtDiscoveryInputs inputs)
    {
        var files = Files(root, inputs.Exclusions);

        return inputs.Layouts
            .SelectMany(layout => Matching(files, [layout.Glob])
                .Select(file => new SpechtLocation(file.Relative, layout, DirectoryOf(file.Relative))))
            .ToList();
    }

    /// <summary>
    /// Discovers the item files beside each specification: every file whose name matches one of the manifest's task file
    /// shapes, <c>{task}</c> standing for <paramref name="taskGrammar"/> (<c>0001-F6</c> B-003, B-012).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="specifications">The discovered specifications.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <param name="taskGrammar">The manifest's <c>identifiers.task</c> grammar.</param>
    /// <returns>The root-relative path of each item file, in specification order, each one's in ordinal order of file name.</returns>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    public IReadOnlyList<string> FindChildItems(
        string root,
        IEnumerable<SpechtLocation> specifications,
        SpechtDiscoveryInputs inputs,
        string taskGrammar)
    {
        var shapes = new SpechtTaskShapes(inputs.TaskFiles, taskGrammar);

        return specifications.SelectMany(specification => Beside(root, specification, shapes.IsMatch)).ToList();
    }

    /// <summary>
    /// Discovers every epic file under <paramref name="root"/>: each file one of the manifest's epic globs matches and
    /// its exclusions do not skip (<c>0001-F6</c> B-002, B-003, B-012).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>The root-relative path of each epic file, in ordinal order.</returns>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    /// <implements epic="0001" feature="F6" claim="B-006" />
    public IReadOnlyList<string> FindEpics(string root, SpechtDiscoveryInputs inputs) =>
        Matching(Files(root, inputs.Exclusions), inputs.EpicFiles).Select(static file => file.Relative).ToList();

    /// <summary>
    /// Discovers the companion files beside <paramref name="specification"/>: every file whose name matches one of the
    /// manifest's companion globs (<c>0001-F6</c> B-003, B-012).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="specification">The specification.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <returns>The root-relative path of each companion file, in ordinal order of file name.</returns>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    public IReadOnlyList<string> FindCompanions(string root, SpechtLocation specification, SpechtDiscoveryInputs inputs) =>
        Beside(root, specification, CompanionShapes(inputs.CompanionFiles)).ToList();

    /// <summary>
    /// Lists each declared test file once, in root-relative ordinal order, outside the manifest's exclusions; with no glob
    /// declared it walks nothing.
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="globs">The test file globs.</param>
    /// <param name="exclusions">The selected manifest's directory exclusions.</param>
    /// <returns>The root-relative path of each selected file.</returns>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    /// <implements epic="0001" feature="F6" claim="B-006" />
    public IReadOnlyList<string> FindTestFiles(
        string root, IReadOnlyList<string> globs, IReadOnlyList<string> exclusions) =>
        globs.Count == 0
            ? []
            : Matching(Files(root, exclusions), globs).Select(static file => file.Relative).ToArray();

    /// <summary>The anchored expression a glob is read as, <c>**</c> spanning directories and <c>*</c> matching within one.</summary>
    /// <param name="glob">The glob.</param>
    /// <returns>The expression's text.</returns>
    /// <implements epic="0001" feature="F6" claim="B-001" />
    internal static string Pattern(string glob)
    {
        var segments = glob.Split('/').Select(static segment =>
            segment == "**" ? "(?:[^/]+/)*" : Regex.Escape(segment).Replace(@"\*", "[^/]*", StringComparison.Ordinal) + "/");

        return @"\A" + string.Concat(segments).TrimEnd('/') + @"\z";
    }

    /// <implements epic="0001" feature="F6" claim="B-001" />
    private static IEnumerable<(string Path, string Relative)> Matching(List<(string Path, string Relative)> files, IReadOnlyList<string> globs)
    {
        var expressions = globs.Select(static glob => new Regex(Pattern(glob))).ToList();

        return files
            .Where(file => expressions.Any(expression => expression.IsMatch(file.Relative)))
            .OrderBy(static file => file.Relative, StringComparer.Ordinal);
    }

    /// <implements epic="0001" feature="F6" claim="B-002" />
    /// <implements epic="0001" feature="F6" claim="B-006" />
    private static bool Skips(string relative, IReadOnlyList<string> exclusions) =>
        exclusions.Any(entry => entry.StartsWith('/')
            ? entry.AsSpan(1).SequenceEqual(relative)
            : relative.Length > 0 && entry == relative[(relative.LastIndexOf('/') + 1)..]);

    /// <implements epic="0001" feature="F6" claim="B-001" />
    private static string DirectoryOf(string relative) =>
        relative.LastIndexOf('/') is var slash and >= 0 ? relative[..slash] : string.Empty;

    /// <implements epic="0001" feature="F6" claim="B-001" />
    private static Func<string, bool> CompanionShapes(IReadOnlyList<string> globs)
    {
        var shapes = globs.Select(static glob => new Regex(Pattern(glob))).ToList();

        return name => shapes.Any(shape => shape.IsMatch(name));
    }

    /// <implements epic="0001" feature="F6" claim="B-001" />
    /// <implements epic="0001" feature="F6" claim="B-006" />
    private List<(string Path, string Relative)> Files(string root, IReadOnlyList<string> exclusions)
    {
        var origin = fileSystem.Path.GetFullPath(root);
        var separator = fileSystem.Path.DirectorySeparatorChar;
        var files = new List<(string Path, string Relative)>();
        var pending = new Stack<(string Directory, bool Skipped)>([(root, Skips(string.Empty, exclusions))]);

        while (pending.TryPop(out var next))
        {
            if (!next.Skipped)
            {
                files.AddRange(fileSystem.Directory.EnumerateFiles(next.Directory).Select(path => (path, Relative(path))));
            }

            foreach (var child in fileSystem.Directory.EnumerateDirectories(next.Directory).Where(child => !Skips(Relative(child), exclusions)))
            {
                pending.Push((child, false));
            }
        }

        return files;

        string Relative(string path) => fileSystem.Path.GetRelativePath(origin, fileSystem.Path.GetFullPath(path)).Replace(separator, '/');
    }

    /// <implements epic="0001" feature="F6" claim="B-001" />
    private IEnumerable<string> Beside(string root, SpechtLocation specification, Func<string, bool> matches) =>
        fileSystem.Directory.EnumerateFiles(
                fileSystem.Path.Combine(root, specification.Directory.Replace('/', fileSystem.Path.DirectorySeparatorChar)))
            .Select(path => fileSystem.Path.GetFileName(path))
            .Where(matches)
            .Order(StringComparer.Ordinal)
            .Select(name => specification.Directory.Length == 0 ? name : specification.Directory + "/" + name);

    private readonly IChildProcess _childProcess = childProcess;
}
